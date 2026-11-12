using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Compliance;
using nvxapp.server.service.ClientServer_Service.Compliance._utility;
using nvxapp.server.service.ClientServer_Service.Compliance.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Compliance
{
    /*
     Dati dei widget della dashboard di Compliance (ANALISI_COMPLIANCE_WIDGET.md), solo conteggi:
        - Cmp_WidgetScadenzeGet: scadenze aperte di Compliance nello scadenzario comune (tabella Scadenza), per tipo:
          scadute e in scadenza entro il preavviso della scadenza (stesso criterio di Cmp_StudioScadenze e del promemoria)
        - Cmp_WidgetGdprGet    : registro GDPR (trattamenti, responsabili esterni, violazioni da notificare)
     Riservati a CompanyPowerAdmin e CompanyAdmin (come le pagine Sicurezza sul lavoro e Registro GDPR). Nessun dato sanitario:
     delle visite si contano solo le scadenze, che gli amministratori vedono gia' nelle pagine.
     I widget "Formazione obbligatoria" e "I miei adempimenti" usano gli endpoint esistenti Cmp_FormazioneStato e Cmp_Mine.
    */
    public class Cmp_WidgetService : ServiceBase, ICmp_WidgetService
    {
        // tipi di scadenza mostrati, nell'ordine (le prese visione dei DPI non sono scadenze)
        private static readonly string[] Fonti =
        {
            CmpFonti.Formazione, CmpFonti.Visita, CmpFonti.Nomina, CmpFonti.Documento, CmpFonti.Responsabile, CmpFonti.Violazione
        };

        private readonly ICmpUtility _utility;
        private readonly IScadenzaRepository _scadenzaRepository;
        private readonly ICmp_TrattamentoRepository _Cmp_Trattamento;
        private readonly ICmp_ResponsabileRepository _Cmp_Responsabile;
        private readonly ICmp_ViolazioneRepository _Cmp_Violazione;

        public Cmp_WidgetService(IMapper mapper,
                                 UserManager<ApplicationUser> userManager,
                                 IAspNetUsersRepository aspNetUsersRepository,
                                 IOptions<JwtParameter> jwtParameter,
                                 IHttpContextAccessor httpContextAccessor,
                                 IConfiguration configuration,
                                 ICmpUtility utility,
                                 IScadenzaRepository scadenzaRepository,
                                 ICmp_TrattamentoRepository cmp_Trattamento,
                                 ICmp_ResponsabileRepository cmp_Responsabile,
                                 ICmp_ViolazioneRepository cmp_Violazione) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _utility = utility;
            _scadenzaRepository = scadenzaRepository;
            _Cmp_Trattamento = cmp_Trattamento;
            _Cmp_Responsabile = cmp_Responsabile;
            _Cmp_Violazione = cmp_Violazione;
        }

        private async Task<CmpContesto> AmministratoreAsync()
        {
            var ctx = await _utility.ContestoAsync();
            if (!ctx.Amministratore)
                throw new UserMessageException(ComplianceMessages.Get("NotAllowed"));
            return ctx;
        }

        public virtual async Task<GenericResult<Cmp_WidgetScadenzeGetOutModel>> Cmp_WidgetScadenzeGet(GenericRequest<Cmp_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var oggi = DateTime.Today;
                var righe = await _scadenzaRepository.FindAll(x => x.IdCompany == ctx.IdCompany && x.Application == (int)ApplicationType.Compliance
                                                                   && x.Stato == StatoScadenza.Aperta && Fonti.Contains(x.Fonte))
                                                     .Select(x => new { x.Fonte, x.DataScadenza, x.GiorniPreavviso })
                                                     .ToListAsync();

                var retVal = new Cmp_WidgetScadenzeGetOutModel { GiorniPreavviso = ctx.GiorniPreavviso };
                foreach (var fonte in Fonti)
                {
                    var delTipo = righe.Where(x => x.Fonte == fonte).ToList();
                    retVal.Aree.Add(new Cmp_WidgetScadenzaAreaModel
                    {
                        Fonte = fonte.Substring("cmp.".Length),
                        Scadute = delTipo.Count(x => x.DataScadenza < oggi),
                        InScadenza = delTipo.Count(x => x.DataScadenza >= oggi && x.DataScadenza <= oggi.AddDays(x.GiorniPreavviso)),
                    });
                }
                retVal.Scadute = retVal.Aree.Sum(x => x.Scadute);
                retVal.InScadenza = retVal.Aree.Sum(x => x.InScadenza);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_WidgetGdprGetOutModel>> Cmp_WidgetGdprGet(GenericRequest<Cmp_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var oggi = DateTime.Today;
                var adesso = DateTime.Now;
                var limitePreavviso = oggi.AddDays(ctx.GiorniPreavviso);

                var trattamenti = await _Cmp_Trattamento.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Attivo)
                                                        .Select(x => x.DatiParticolari).ToListAsync();
                var responsabili = await _Cmp_Responsabile.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.DataScadenza != null)
                                                          .Select(x => x.DataScadenza!.Value).ToListAsync();
                var daUnAnno = oggi.AddYears(-1);
                var violazioni = await _Cmp_Violazione.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica
                                                                    && ((x.Rischio > 0 && x.DataNotificaGarante == null) || x.DataScoperta >= daUnAnno))
                                                      .Select(x => new { x.Rischio, x.DataNotificaGarante, x.DataScoperta }).ToListAsync();
                // termine di 72 ore dalla scoperta, come Cmp_GdprService (NotificaDovuta / NotificaInRitardo)
                var daNotificare = violazioni.Where(x => x.Rischio > 0 && x.DataNotificaGarante == null).ToList();

                return new Cmp_WidgetGdprGetOutModel
                {
                    TrattamentiAttivi = trattamenti.Count,
                    TrattamentiDatiParticolari = trattamenti.Count(x => x),
                    ResponsabiliScaduti = responsabili.Count(x => x < oggi),
                    ResponsabiliInScadenza = responsabili.Count(x => x >= oggi && x <= limitePreavviso),
                    ViolazioniDaNotificare = daNotificare.Count,
                    ViolazioniOltreTermine = daNotificare.Count(x => adesso > x.DataScoperta.AddHours(72)),
                    Violazioni12Mesi = violazioni.Count(x => x.DataScoperta >= daUnAnno),
                };
            }, isSubProcess);
        }
    }

    public interface ICmp_WidgetService : IServiceBase
    {
        Task<GenericResult<Cmp_WidgetScadenzeGetOutModel>> Cmp_WidgetScadenzeGet(GenericRequest<Cmp_EmptyInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_WidgetGdprGetOutModel>> Cmp_WidgetGdprGet(GenericRequest<Cmp_EmptyInModel> model, Boolean isSubProcess);
    }
}
