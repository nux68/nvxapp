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
     Attivita' commerciali e agenda (ANALISI_CRM_APPLICATIVO.md §6.1): collegate a un cliente (e contatto/opportunita') o a un lead.
     L'assegnatario e' l'utente corrente salvo diversa scelta di un amministratore (o del commerciale verso se stesso).
     I promemoria li manda CrmReminderService (avviso SignalR) prima dell'inizio, secondo Crm_Cfg.MinutiPreavviso.
    */
    public class Crm_AttivitaService : ServiceBase, ICrm_AttivitaService
    {
        private readonly ICrm_AttivitaRepository _Crm_Attivita;
        private readonly ICrm_Az_ClienteContattoReadRepository _Az_ClienteContatto;
        private readonly ICrm_Par_TipoAttivitaRepository _Crm_Par_TipoAttivita;
        private readonly ICrmUtility _utility;

        public Crm_AttivitaService(IMapper mapper,
                                   UserManager<ApplicationUser> userManager,
                                   IAspNetUsersRepository aspNetUsersRepository,
                                   IOptions<JwtParameter> jwtParameter,
                                   IHttpContextAccessor httpContextAccessor,
                                   IConfiguration configuration,
                                   ICrm_AttivitaRepository crm_AttivitaRepository,
                                   ICrm_Az_ClienteContattoReadRepository az_ClienteContattoRepository,
                                   ICrm_Par_TipoAttivitaRepository crm_Par_TipoAttivitaRepository,
                                   ICrmUtility utility) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Crm_Attivita = crm_AttivitaRepository;
            _Az_ClienteContatto = az_ClienteContattoRepository;
            _Crm_Par_TipoAttivita = crm_Par_TipoAttivitaRepository;
            _utility = utility;
        }

        public virtual async Task<GenericResult<Crm_AttivitaListOutModel>> Crm_AttivitaList(GenericRequest<Crm_AttivitaListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var f = model.Data;
                var q = _utility.AttivitaVisibili(ctx);
                if (f.SoloMie)
                    q = q.Where(a => a.IdAspNetUsersAssegnatario == ctx.IdUtente);
                if (f.SoloDaFare)
                    q = q.Where(a => a.Stato == Crm_StatoAttivita.DaFare);
                // con un periodo: quelle del periodo e (se da fare) anche quelle scadute prima
                if (f.Dal.HasValue)
                {
                    var dal = f.Dal.Value.Date;
                    q = q.Where(a => a.Inizio >= dal || (f.SoloDaFare && a.Stato == Crm_StatoAttivita.DaFare));
                }
                if (f.Al.HasValue)
                {
                    var al = f.Al.Value.Date.AddDays(1);
                    q = q.Where(a => a.Inizio < al);
                }
                return new Crm_AttivitaListOutModel { Attivita = await CrmLists.AttivitaAsync(q, _utility) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_AttivitaGetOutModel>> Crm_AttivitaGet(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var a = await Visibile(ctx, model.Data.Id, false);
                var retVal = new Crm_AttivitaGetOutModel { Attivita = Modello(a) };
                if (a.IdAz_Cliente is int c)
                {
                    retVal.Contatti = await CrmLists.ContattiAsync(_Az_ClienteContatto, c);
                    retVal.OpportunitaCliente = await CrmLists.OpportunitaAsync(_utility.OpportunitaVisibili(ctx).Where(o => o.IdAz_Cliente == c), _utility);
                }
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_AttivitaPutOutModel>> Crm_AttivitaPut(GenericRequest<Crm_AttivitaPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var d = model.Data.Attivita;
                if (string.IsNullOrWhiteSpace(d.Oggetto))
                    throw new UserMessageException(CrmMessages.Get("ActivitySubjectRequired"));
                if (d.Fine.HasValue && d.Fine < d.Inizio)
                    throw new UserMessageException(CrmMessages.Get("ActivityEndBeforeStart"));
                if (!await _Crm_Par_TipoAttivita.FindAll(_ => true).AnyAsync(x => x.Id == d.IdCrm_Par_TipoAttivita && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica))
                    throw new UserMessageException(CrmMessages.Get("ActivityTypeInvalid"));

                // collegamenti: devono essere visibili e coerenti con il cliente
                if (d.IdAz_Cliente is int idCliente)
                    await _utility.ClienteAsync(ctx, idCliente);
                if (d.IdCrm_Opportunita is int idOpp)
                {
                    var opp = await _utility.OpportunitaVisibili(ctx).AsNoTracking().FirstOrDefaultAsync(o => o.Id == idOpp) ?? throw new UserMessageException(CrmMessages.Get("OpportunityNotFound"));
                    d.IdAz_Cliente = opp.IdAz_Cliente;
                }
                if (d.IdCrm_Lead is int idLead && !await _utility.LeadVisibili(ctx).AnyAsync(l => l.Id == idLead))
                    throw new UserMessageException(CrmMessages.Get("LeadNotFound"));
                if (d.IdAz_Cliente == null && d.IdCrm_Lead == null)
                    throw new UserMessageException(CrmMessages.Get("ActivityLinkRequired"));

                Crm_Attivita a;
                if (d.Id != 0)
                    a = await Visibile(ctx, d.Id, true);
                else
                {
                    a = new Crm_Attivita { IdAz_Anagrafica = ctx.IdAz_Anagrafica, CreationDate = DateTime.Now, IdAspNetUsersAssegnatario = ctx.IdUtente };
                    await _Crm_Attivita.CreateAsync(a, false);
                }

                var assegnatario = string.IsNullOrEmpty(d.IdAssegnatario) ? ctx.IdUtente : d.IdAssegnatario;
                if (assegnatario != a.IdAspNetUsersAssegnatario)
                {
                    if (!(await _utility.UtentiAsync(ctx)).Any(u => u.Id == assegnatario))
                        throw new UserMessageException(CrmMessages.Get("UserInvalid"));
                    // un commerciale assegna a se stesso o passa ad altri le proprie attivita'
                    if (!ctx.Amministratore && a.IdAspNetUsersAssegnatario != ctx.IdUtente && assegnatario != ctx.IdUtente)
                        throw new UserMessageException(CrmMessages.Get("ResponsibleChangeNotAllowed"));
                    a.IdAspNetUsersAssegnatario = assegnatario;
                    a.PromemoriaInviato = false;
                }

                if (a.Inizio != d.Inizio)
                    a.PromemoriaInviato = false;
                a.IdCrm_Par_TipoAttivita = d.IdCrm_Par_TipoAttivita;
                a.Oggetto = d.Oggetto.Trim();
                a.Descrizione = string.IsNullOrWhiteSpace(d.Descrizione) ? null : d.Descrizione.Trim();
                a.Inizio = d.Inizio;
                a.Fine = d.Fine;
                a.Stato = d.Stato;
                a.Esito = string.IsNullOrWhiteSpace(d.Esito) ? null : d.Esito.Trim();
                a.IdAz_Cliente = d.IdAz_Cliente;
                a.IdAz_ClienteContatto = d.IdAz_ClienteContatto is int c && d.IdAz_Cliente != null && await _Az_ClienteContatto.FindAll(_ => true).AnyAsync(x => x.Id == c && x.IdAz_Cliente == d.IdAz_Cliente) ? c : null;
                a.IdCrm_Opportunita = d.IdCrm_Opportunita;
                a.IdCrm_Lead = d.IdCrm_Lead;
                a.ModifiedDate = DateTime.Now;
                a.ChangeUser = ctx.IdUtente;
                await _Crm_Attivita.SaveChange();
                return new Crm_AttivitaPutOutModel { Attivita = Modello(a) };
            }, isSubProcess);
        }

        // fatta (con l'esito) o annullata, con un clic dall'agenda
        public virtual async Task<GenericResult<Crm_AttivitaPutOutModel>> Crm_AttivitaCompleta(GenericRequest<Crm_AttivitaCompletaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var a = await Visibile(ctx, model.Data.Id, true);
                a.Stato = model.Data.Annullata ? Crm_StatoAttivita.Annullata : Crm_StatoAttivita.Fatta;
                if (!string.IsNullOrWhiteSpace(model.Data.Esito))
                    a.Esito = model.Data.Esito.Trim();
                a.ModifiedDate = DateTime.Now;
                a.ChangeUser = ctx.IdUtente;
                await _Crm_Attivita.SaveChange();
                return new Crm_AttivitaPutOutModel { Attivita = Modello(a) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_IdOutModel>> Crm_AttivitaDelete(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var a = await Visibile(ctx, model.Data.Id, true);
                await _Crm_Attivita.DeleteAsync(a);
                await _Crm_Attivita.SaveChange();
                return new Crm_IdOutModel { Id = model.Data.Id };
            }, isSubProcess);
        }

        private async Task<Crm_Attivita> Visibile(CrmContesto ctx, int id, bool tracking)
        {
            var q = _utility.AttivitaVisibili(ctx);
            q = tracking ? q.AsTracking() : q.AsNoTracking();
            return await q.FirstOrDefaultAsync(a => a.Id == id) ?? throw new UserMessageException(CrmMessages.Get("ActivityNotFound"));
        }

        private static Crm_AttivitaModel Modello(Crm_Attivita a) => new()
        {
            Id = a.Id, IdCrm_Par_TipoAttivita = a.IdCrm_Par_TipoAttivita, Oggetto = a.Oggetto, Descrizione = a.Descrizione, Inizio = a.Inizio, Fine = a.Fine,
            IdAssegnatario = a.IdAspNetUsersAssegnatario, Stato = a.Stato, Esito = a.Esito, IdAz_Cliente = a.IdAz_Cliente,
            IdAz_ClienteContatto = a.IdAz_ClienteContatto, IdCrm_Opportunita = a.IdCrm_Opportunita, IdCrm_Lead = a.IdCrm_Lead
        };
    }

    public interface ICrm_AttivitaService : IServiceBase
    {
        Task<GenericResult<Crm_AttivitaListOutModel>> Crm_AttivitaList(GenericRequest<Crm_AttivitaListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_AttivitaGetOutModel>> Crm_AttivitaGet(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_AttivitaPutOutModel>> Crm_AttivitaPut(GenericRequest<Crm_AttivitaPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_AttivitaPutOutModel>> Crm_AttivitaCompleta(GenericRequest<Crm_AttivitaCompletaInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_IdOutModel>> Crm_AttivitaDelete(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess);
    }
}
