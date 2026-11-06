using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Crm;
using nvxapp.server.service.ClientServer_Service.Crm._utility;
using nvxapp.server.service.ClientServer_Service.Crm.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Crm
{
    /*
     Report del CRM (ANALISI_CRM.md §6.2), sui dati visibili all'utente: pipeline aperta per fase e per responsabile
     (valore e valore pesato con la probabilita'), opportunita' vinte e perse nel periodo, conversione dei lead,
     attivita' fatte per utente. Tabelle semplici (le pagine le mostrano con barre).
    */
    public class Crm_ReportService : ServiceBase, ICrm_ReportService
    {
        private readonly ICrmUtility _utility;

        public Crm_ReportService(IMapper mapper,
                                 UserManager<ApplicationUser> userManager,
                                 IAspNetUsersRepository aspNetUsersRepository,
                                 IOptions<JwtParameter> jwtParameter,
                                 IHttpContextAccessor httpContextAccessor,
                                 IConfiguration configuration,
                                 ICrmUtility utility) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _utility = utility;
        }

        public virtual async Task<GenericResult<Crm_ReportOutModel>> Crm_Report(GenericRequest<Crm_ReportInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var dal = model.Data.Dal.Date;
                var al = model.Data.Al.Date.AddDays(1);
                if (al <= dal)
                    throw new UserMessageException(CrmMessages.Get("PeriodInvalid"));
                var senza = CrmMessages.Get("ReportNoResponsible");
                var retVal = new Crm_ReportOutModel();

                var opp = _utility.OpportunitaVisibili(ctx).AsNoTracking();
                var aperte = await opp.Where(o => o.Crm_Par_FaseNavigation!.Tipo == Crm_FaseTipo.Aperta)
                                      .Select(o => new { o.IdCrm_Par_Fase, Fase = o.Crm_Par_FaseNavigation!.Descrizione, Ordine = o.Crm_Par_FaseNavigation!.Ordine, o.IdAspNetUsersResponsabile, o.Valore, o.Probabilita })
                                      .ToListAsync();
                var chiuse = await opp.Where(o => o.DataChiusura >= dal && o.DataChiusura < al && o.Crm_Par_FaseNavigation!.Tipo != Crm_FaseTipo.Aperta)
                                      .Select(o => new { Tipo = o.Crm_Par_FaseNavigation!.Tipo, o.IdAspNetUsersResponsabile, o.Valore, o.IdCrm_Par_MotivoPerdita, Motivo = o.Crm_Par_MotivoPerditaNavigation != null ? o.Crm_Par_MotivoPerditaNavigation.Descrizione : null })
                                      .ToListAsync();
                var attivita = await _utility.AttivitaVisibili(ctx).AsNoTracking()
                                     .Where(a => a.Stato == Crm_StatoAttivita.Fatta && a.Inizio >= dal && a.Inizio < al)
                                     .Select(a => new { a.IdAspNetUsersAssegnatario, a.Crm_Par_TipoAttivitaNavigation!.Descrizione })
                                     .ToListAsync();
                var nomi = await _utility.NomiAsync(aperte.Select(x => x.IdAspNetUsersResponsabile).Concat(chiuse.Select(x => x.IdAspNetUsersResponsabile)).Concat(attivita.Select(x => x.IdAspNetUsersAssegnatario)));
                string Nome(string? id) => id != null && nomi.TryGetValue(id, out var n) ? n : senza;

                retVal.PipelinePerFase = aperte.GroupBy(x => new { x.IdCrm_Par_Fase, x.Fase, x.Ordine }).OrderBy(g => g.Key.Ordine)
                    .Select(g => Riga(g.Key.IdCrm_Par_Fase.ToString(), g.Key.Fase, g.Count(), g.Sum(x => x.Valore), g.Sum(x => x.Valore * x.Probabilita / 100m))).ToList();
                retVal.PipelinePerResponsabile = aperte.GroupBy(x => x.IdAspNetUsersResponsabile)
                    .Select(g => Riga(g.Key ?? string.Empty, Nome(g.Key), g.Count(), g.Sum(x => x.Valore), g.Sum(x => x.Valore * x.Probabilita / 100m))).OrderByDescending(r => r.Valore).ToList();

                var vinte = chiuse.Where(x => x.Tipo == Crm_FaseTipo.Vinta).ToList();
                var perse = chiuse.Where(x => x.Tipo == Crm_FaseTipo.Persa).ToList();
                retVal.Vinte = vinte.Count;
                retVal.Perse = perse.Count;
                retVal.ValoreVinto = vinte.Sum(x => x.Valore);
                retVal.VintePerResponsabile = vinte.GroupBy(x => x.IdAspNetUsersResponsabile)
                    .Select(g => Riga(g.Key ?? string.Empty, Nome(g.Key), g.Count(), g.Sum(x => x.Valore), g.Sum(x => x.Valore))).OrderByDescending(r => r.Valore).ToList();
                retVal.PersePerMotivo = perse.GroupBy(x => new { x.IdCrm_Par_MotivoPerdita, x.Motivo })
                    .Select(g => Riga(g.Key.IdCrm_Par_MotivoPerdita?.ToString() ?? string.Empty, g.Key.Motivo ?? CrmMessages.Get("ReportNoReason"), g.Count(), g.Sum(x => x.Valore), 0))
                    .OrderByDescending(r => r.Numero).ToList();

                var lead = _utility.LeadVisibili(ctx).AsNoTracking().Where(l => l.CreationDate >= dal && l.CreationDate < al);
                retVal.LeadCreati = await lead.CountAsync();
                retVal.LeadConvertiti = await lead.CountAsync(l => l.Stato == Crm_StatoLead.Convertito);

                retVal.AttivitaPerUtente = attivita.GroupBy(x => x.IdAspNetUsersAssegnatario)
                    .Select(g => Riga(g.Key, Nome(g.Key), g.Count(), 0, 0)).OrderByDescending(r => r.Numero).ToList();
                return retVal;
            }, isSubProcess);
        }

        private static Crm_ReportRigaModel Riga(string chiave, string etichetta, int numero, decimal valore, decimal pesato)
            => new() { Chiave = chiave, Etichetta = etichetta, Numero = numero, Valore = valore, ValorePesato = Math.Round(pesato, 2) };
    }

    public interface ICrm_ReportService : IServiceBase
    {
        Task<GenericResult<Crm_ReportOutModel>> Crm_Report(GenericRequest<Crm_ReportInModel> model, Boolean isSubProcess);
    }
}
