using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.HumanResources;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.HumanResources;
using nvxapp.server.service.ClientServer_Service.HumanResources._utility;
using nvxapp.server.service.ClientServer_Service.HumanResources.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.HumanResources
{
    /*
     Dati dei widget della dashboard di Risorse Umane (ANALISI_HUMANRESOURCES_WIDGET.md), per l'ufficio del personale
     (CompanyPowerAdmin, CompanyAdmin: gli stessi permessi delle pagine Checklist e Corsi, IHrUtility.AmministratoreAsync).
        - Onboarding/offboarding: checklist aperte e voci da fare (scadute e dei prossimi giorni). Prima si sincronizzano le
          checklist generate dal modello predefinito, come fa l'elenco delle checklist (HrOnboardingCore.SincronizzaAsync).
        - Formazione: attestati scaduti o in scadenza; per dipendente e corso conta solo l'ultimo conseguito (un rinnovo
          supera i precedenti), solo per dipendenti in forza e corsi attivi.
     Il widget "I miei cedolini" usa Hr_Cedolino/Hr_MieiDocumenti (area del dipendente): nessun endpoint nuovo.
    */
    public class Hr_WidgetService : ServiceBase, IHr_WidgetService
    {
        // voci "prossime" dell'onboarding
        public const int GiorniProssime = 7;
        // finestra degli attestati in scadenza
        public const int GiorniFinestra = 60;
        // righe restituite negli elenchi dei widget
        public const int MaxRighe = 5;

        private readonly IHrUtility _utility;
        private readonly IHrOnboardingCore _onboardingCore;
        private readonly IHr_ChecklistRepository _Hr_Checklist;
        private readonly IHr_ChecklistVoceRepository _Hr_ChecklistVoce;
        private readonly IHr_CorsoPartecipanteRepository _Hr_CorsoPartecipante;

        public Hr_WidgetService(IMapper mapper,
                                UserManager<ApplicationUser> userManager,
                                IAspNetUsersRepository aspNetUsersRepository,
                                IOptions<JwtParameter> jwtParameter,
                                IHttpContextAccessor httpContextAccessor,
                                IConfiguration configuration,
                                IHrUtility utility,
                                IHrOnboardingCore onboardingCore,
                                IHr_ChecklistRepository hr_Checklist,
                                IHr_ChecklistVoceRepository hr_ChecklistVoce,
                                IHr_CorsoPartecipanteRepository hr_CorsoPartecipante) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _utility = utility;
            _onboardingCore = onboardingCore;
            _Hr_Checklist = hr_Checklist;
            _Hr_ChecklistVoce = hr_ChecklistVoce;
            _Hr_CorsoPartecipante = hr_CorsoPartecipante;
        }

        public virtual async Task<GenericResult<Hr_WidgetOnboardingGetOutModel>> Hr_WidgetOnboardingGet(GenericRequest<Hr_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                await _onboardingCore.SincronizzaAsync(ctx);

                var oggi = DateTime.Today;
                var limite = oggi.AddDays(GiorniProssime);
                var retVal = new Hr_WidgetOnboardingGetOutModel { GiorniProssime = GiorniProssime };

                var aperte = await _Hr_Checklist.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Stato == Hr_StatoChecklist.Aperta)
                                                .GroupBy(x => x.Tipo)
                                                .Select(g => new { Tipo = g.Key, Numero = g.Count() })
                                                .ToListAsync();
                retVal.Assunzioni = aperte.Where(x => x.Tipo == Hr_TipoChecklist.Assunzione).Sum(x => x.Numero);
                retVal.Cessazioni = aperte.Where(x => x.Tipo == Hr_TipoChecklist.Cessazione).Sum(x => x.Numero);
                retVal.ChecklistAperte = aperte.Sum(x => x.Numero);

                var daFare = _Hr_ChecklistVoce.FindAll(x => x.Hr_ChecklistNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica
                                                            && x.Hr_ChecklistNavigation.Stato == Hr_StatoChecklist.Aperta
                                                            && !x.Completata && x.DataPrevista < limite);
                retVal.VociScadute = await daFare.CountAsync(x => x.DataPrevista < oggi);
                retVal.VociProssime = await daFare.CountAsync(x => x.DataPrevista >= oggi);
                retVal.Voci = await daFare.OrderBy(x => x.DataPrevista).ThenBy(x => x.Ordine)
                                          .Take(MaxRighe)
                                          .Select(x => new Hr_WidgetVoceModel
                                          {
                                              IdHr_Checklist = x.IdHr_Checklist,
                                              Dipendente = (x.Hr_ChecklistNavigation!.Dip_AnagraficaNavigation!.Cognome + " " + x.Hr_ChecklistNavigation.Dip_AnagraficaNavigation.Nome).Trim(),
                                              Tipo = (int)x.Hr_ChecklistNavigation.Tipo,
                                              Descrizione = x.Descrizione,
                                              DataPrevista = x.DataPrevista
                                          })
                                          .ToListAsync();
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_WidgetFormazioneGetOutModel>> Hr_WidgetFormazioneGet(GenericRequest<Hr_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var oggi = DateTime.Today;

                // attestati con scadenza dei corsi attivi, dipendenti in forza
                var attestati = await _Hr_CorsoPartecipante.FindAll(x => x.Esito == Hr_EsitoCorso.Superato && x.DataScadenza != null
                                                                         && x.Hr_CorsoEdizioneNavigation!.Hr_CorsoNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica
                                                                         && x.Hr_CorsoEdizioneNavigation.Hr_CorsoNavigation.Attivo
                                                                         && x.Dip_AnagraficaNavigation!.Attivo
                                                                         && (x.Dip_AnagraficaNavigation.DataCessazione == null || x.Dip_AnagraficaNavigation.DataCessazione >= oggi))
                    .Select(x => new
                    {
                        x.IdDip_Anagrafica,
                        IdHr_Corso = x.Hr_CorsoEdizioneNavigation!.IdHr_Corso,
                        Corso = x.Hr_CorsoEdizioneNavigation.Hr_CorsoNavigation!.Titolo,
                        Obbligatorio = x.Hr_CorsoEdizioneNavigation.Hr_CorsoNavigation.Obbligatorio,
                        Dipendente = (x.Dip_AnagraficaNavigation!.Cognome + " " + x.Dip_AnagraficaNavigation.Nome).Trim(),
                        DataScadenza = x.DataScadenza!.Value
                    })
                    .ToListAsync();

                // per dipendente e corso solo l'ultimo attestato
                var ultimi = attestati.GroupBy(x => new { x.IdDip_Anagrafica, x.IdHr_Corso })
                                      .Select(g => g.OrderByDescending(x => x.DataScadenza).First())
                                      .Where(x => x.DataScadenza.Date <= oggi.AddDays(GiorniFinestra))
                                      .OrderBy(x => x.DataScadenza)
                                      .ToList();

                return new Hr_WidgetFormazioneGetOutModel
                {
                    GiorniFinestra = GiorniFinestra,
                    Scaduti = ultimi.Count(x => x.DataScadenza.Date < oggi),
                    Entro30 = ultimi.Count(x => x.DataScadenza.Date >= oggi && x.DataScadenza.Date <= oggi.AddDays(30)),
                    Entro60 = ultimi.Count(x => x.DataScadenza.Date > oggi.AddDays(30)),
                    Attestati = ultimi.Take(MaxRighe).Select(x => new Hr_WidgetAttestatoModel
                    {
                        IdHr_Corso = x.IdHr_Corso, Corso = x.Corso, Dipendente = x.Dipendente, DataScadenza = x.DataScadenza, Obbligatorio = x.Obbligatorio
                    }).ToList()
                };
            }, isSubProcess);
        }
    }

    public interface IHr_WidgetService : IServiceBase
    {
        Task<GenericResult<Hr_WidgetOnboardingGetOutModel>> Hr_WidgetOnboardingGet(GenericRequest<Hr_EmptyInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_WidgetFormazioneGetOutModel>> Hr_WidgetFormazioneGet(GenericRequest<Hr_EmptyInModel> model, Boolean isSubProcess);
    }
}
