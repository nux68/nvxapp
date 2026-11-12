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
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Request.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Request
{
    /*
     Hub delle richieste da approvare (ANALISI_INFRASTRUCTURE_RICHIESTE.md, RICH-INF.1): unisce le richieste dei moduli
     (IRequestProvider) degli applicativi attivi per l'azienda corrente.
        - le pagine sono di Infrastructure: funzionano con qualunque applicativo che porta richieste (Presenze, Risorse aziendali...)
        - Admin = amministratore dell'azienda (CompanyPowerAdmin, CompanyAdmin); i provider ricontrollano i propri permessi
        - approva / rifiuta / annulla: la chiave "<Prefix>:<Id>" sceglie il provider
        - un provider che fallisce nell'elenco o nei contatori finisce nel log e non blocca gli altri; nelle azioni
          l'errore arriva all'utente
    */
    public class RequestService : ServiceBase, IRequestService
    {
        private readonly IEnumerable<IRequestProvider> _providers;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;

        public RequestService(IMapper mapper,
                              UserManager<ApplicationUser> userManager,
                              IAspNetUsersRepository aspNetUsersRepository,
                              IOptions<JwtParameter> jwtParameter,
                              IHttpContextAccessor httpContextAccessor,
                              IConfiguration configuration,

                              IEnumerable<IRequestProvider> providers,
                              ICompanyApplicationRepository companyApplicationRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _providers = providers;
            _companyApplicationRepository = companyApplicationRepository;
        }

        public virtual async Task<GenericResult<RequestCountOutModel>> RequestCount(GenericRequest<RequestCountInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new RequestCountOutModel();
                var counts = await ContaAsync(await ProvidersAttiviAsync(), await AmministratoreAsync());
                retVal.DaApprovare = counts.DaApprovare;
                retVal.InAttesa = counts.InAttesa;
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<RequestListOutModel>> RequestList(GenericRequest<RequestListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new RequestListOutModel();

                bool admin = await AmministratoreAsync();
                if (model.Data.Admin && !admin)
                    throw new UserMessageException(InfrastructureMessages.Get("RequestNotAllowed"));

                var providers = await ProvidersAttiviAsync();
                var scope = model.Data.Admin ? RequestScope.Admin : RequestScope.Mine;
                foreach (var p in providers)
                {
                    try
                    {
                        var items = await p.ListAsync(scope, model.Data.Filtro);
                        foreach (var item in items)
                        {
                            item.Key = p.Prefix + ":" + item.Id;
                            item.Application = (int)p.Application;
                        }
                        retVal.Richieste.AddRange(items);
                        if (!retVal.Applicativi.Contains((int)p.Application))
                            retVal.Applicativi.Add((int)p.Application);
                    }
                    catch (Exception ex) when (ex is not UserMessageException)
                    {
                        Log.Warning(ex, "Richieste: elenco non disponibile per {Prefix}", p.Prefix);
                    }
                }
                retVal.Richieste = retVal.Richieste.OrderByDescending(x => x.Dal).ToList();

                var counts = await ContaAsync(providers, admin);
                retVal.DaApprovare = counts.DaApprovare;
                retVal.InAttesa = counts.InAttesa;
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<RequestActionOutModel>> RequestApprove(GenericRequest<RequestActionInModel> model, Boolean isSubProcess)
            => await AzioneAsync(model, isSubProcess, (p, id) => p.ApproveAsync(id));

        public virtual async Task<GenericResult<RequestActionOutModel>> RequestReject(GenericRequest<RequestActionInModel> model, Boolean isSubProcess)
            => await AzioneAsync(model, isSubProcess, (p, id) => p.RejectAsync(id, string.IsNullOrWhiteSpace(model.Data.Motivo) ? null : model.Data.Motivo.Trim()));

        public virtual async Task<GenericResult<RequestActionOutModel>> RequestCancel(GenericRequest<RequestActionInModel> model, Boolean isSubProcess)
            => await AzioneAsync(model, isSubProcess, (p, id) => p.CancelAsync(id));

        // ---------------------------------------------------------------------------

        private async Task<GenericResult<RequestActionOutModel>> AzioneAsync(GenericRequest<RequestActionInModel> model, Boolean isSubProcess, Func<IRequestProvider, string, Task> azione)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new RequestActionOutModel();

                var key = model.Data.Key ?? string.Empty;
                int sep = key.IndexOf(':');
                if (sep <= 0 || sep == key.Length - 1)
                    throw new UserMessageException(InfrastructureMessages.Get("RequestNotFound"));

                var providers = await ProvidersAttiviAsync();
                var provider = providers.FirstOrDefault(p => p.Prefix == key.Substring(0, sep))
                               ?? throw new UserMessageException(InfrastructureMessages.Get("RequestNotFound"));
                await azione(provider, key.Substring(sep + 1));

                var counts = await ContaAsync(providers, await AmministratoreAsync());
                retVal.DaApprovare = counts.DaApprovare;
                retVal.InAttesa = counts.InAttesa;
                return retVal;
            }, isSubProcess);
        }

        private static async Task<RequestCounts> ContaAsync(List<IRequestProvider> providers, bool admin)
        {
            var tot = new RequestCounts();
            foreach (var p in providers)
            {
                try
                {
                    var c = await p.CountAsync(admin);
                    tot.DaApprovare += c.DaApprovare;
                    tot.InAttesa += c.InAttesa;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Richieste: contatori non disponibili per {Prefix}", p.Prefix);
                }
            }
            return tot;
        }

        // provider degli applicativi attivi per l'azienda corrente (nessuno senza azienda)
        private async Task<List<IRequestProvider>> ProvidersAttiviAsync()
        {
            if (!int.TryParse(this.CurrentCompany, out int idCompany) || idCompany <= 0)
                return new List<IRequestProvider>();

            var attivi = await _companyApplicationRepository.FindAll(x => x.IdCompany == idCompany && x.Active).Select(x => x.ApplicationType).ToListAsync();
            return _providers.Where(p => p.Application == ApplicationType.Common || attivi.Contains(p.Application))
                             .OrderBy(p => (int)p.Application)
                             .ToList();
        }

        private async Task<bool> AmministratoreAsync()
        {
            var user = await _userManager.FindByIdAsync(this.CurrentUserId);
            if (user == null)
                return false;
            var ruoli = await _userManager.GetRolesAsync(user);
            return ruoli.Any(r => CompanyRecipients.RuoliAmministratori.Contains(r));
        }
    }

    public interface IRequestService : IServiceBase
    {
        Task<GenericResult<RequestCountOutModel>> RequestCount(GenericRequest<RequestCountInModel> model, Boolean isSubProcess);
        Task<GenericResult<RequestListOutModel>> RequestList(GenericRequest<RequestListInModel> model, Boolean isSubProcess);
        Task<GenericResult<RequestActionOutModel>> RequestApprove(GenericRequest<RequestActionInModel> model, Boolean isSubProcess);
        Task<GenericResult<RequestActionOutModel>> RequestReject(GenericRequest<RequestActionInModel> model, Boolean isSubProcess);
        Task<GenericResult<RequestActionOutModel>> RequestCancel(GenericRequest<RequestActionInModel> model, Boolean isSubProcess);
    }
}
