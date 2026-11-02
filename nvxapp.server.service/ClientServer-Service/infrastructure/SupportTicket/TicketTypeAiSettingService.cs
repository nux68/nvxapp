using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Documentation;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket
{
    /*
     Risposta automatica AI dei tipi di richiesta, per livello e nodo (TicketTypeAiSetting):
       - il SuperUser decide i valori di default dei livelli che ricevono i ticket con l'AI (Dev, Dealer,
         FinancialAdvisor, Company); il livello Dev ha solo i default, perche' non ha nodi
       - il PowerAdmin di un nodo li personalizza per il suo nodo o torna ai default; l'Admin legge
     Si propongono i tipi attivi abilitati al livello (per l'azienda, solo quelli dei suoi applicativi attivi).
    */
    public class TicketTypeAiSettingService : ServiceBase, ITicketTypeAiSettingService
    {
        public const int MaxAttempts = 10;

        // livelli in cui il ticket arriva con la risposta AI (il SuperUser riceve solo dal DEV, senza AI)
        public static readonly TicketLevel[] AiLevels = { TicketLevel.Dev, TicketLevel.Dealer, TicketLevel.FinancialAdvisor, TicketLevel.Company };

        private readonly ITicketTypeAiSettingRepository _ticketTypeAiSettingRepository;
        private readonly ITicketTypeRepository _ticketTypeRepository;
        private readonly ITicketTypeLevelRepository _ticketTypeLevelRepository;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;

        public TicketTypeAiSettingService(IMapper mapper,
                                          UserManager<ApplicationUser> userManager,
                                          IAspNetUsersRepository aspNetUsersRepository,
                                          IOptions<JwtParameter> jwtParameter,
                                          IHttpContextAccessor httpContextAccessor,
                                          IConfiguration configuration,

                                          ITicketTypeAiSettingRepository ticketTypeAiSettingRepository,
                                          ITicketTypeRepository ticketTypeRepository,
                                          ITicketTypeLevelRepository ticketTypeLevelRepository,
                                          ICompanyApplicationRepository companyApplicationRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _ticketTypeAiSettingRepository = ticketTypeAiSettingRepository;
            _ticketTypeRepository = ticketTypeRepository;
            _ticketTypeLevelRepository = ticketTypeLevelRepository;
            _companyApplicationRepository = companyApplicationRepository;
        }

        public virtual async Task<GenericResult<TicketTypeAiSettingScopeOutModel>> TicketTypeAiSettingScope(GenericRequest<TicketTypeAiSettingScopeInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TicketTypeAiSettingScopeOutModel { MaxAttempts = MaxAttempts };

                retVal.Scope = (await Scopes()).Select(s => new TicketTypeAiSettingScopeModel
                {
                    Level = s.Level,
                    IsDefault = s.IsDefault,
                    CanEdit = s.Node.CanEdit,
                }).ToList();

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<TicketTypeAiSettingListOutModel>> TicketTypeAiSettingList(GenericRequest<TicketTypeAiSettingListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TicketTypeAiSettingListOutModel();

                var scope = await Scope(model.Data.Level, false);
                retVal.TicketTypeAiSetting = Settings(scope);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<TicketTypeAiSettingPutOutModel>> TicketTypeAiSettingPut(GenericRequest<TicketTypeAiSettingPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TicketTypeAiSettingPutOutModel();

                var scope = await Scope(model.Data.Level, true);
                var node = scope.Node;
                var available = Types(scope).Select(t => t.Id).ToHashSet();
                var rows = (model.Data.TicketTypeAiSetting ?? new List<TicketTypeAiSettingModel>())
                           .GroupBy(r => r.IdTicketType).Select(g => g.Last()).ToList();

                foreach (var r in rows)
                {
                    if (!available.Contains(r.IdTicketType))
                        throw new UserMessageException(InfrastructureMessages.Get("TicketTypeNotAvailable"));
                    if ((scope.IsDefault || r.Custom) && (r.AiMaxAttempts < 1 || r.AiMaxAttempts > MaxAttempts))
                        throw new UserMessageException(InfrastructureMessages.Get("TicketTypeAiSettingAttemptsNotValid", MaxAttempts));
                }

                var level = node.Level;
                int? idDealer = node.IdDealer, idFinancialAdvisor = node.IdFinancialAdvisor, idCompany = node.IdCompany;
                var existing = _ticketTypeAiSettingRepository.FindAll(s => s.Level == level && s.IdDealer == idDealer
                                                                           && s.IdFinancialAdvisor == idFinancialAdvisor && s.IdCompany == idCompany).ToList();

                foreach (var r in rows)
                {
                    var row = existing.FirstOrDefault(e => e.IdTicketType == r.IdTicketType);

                    // nodo che torna ai default: si elimina la sua riga
                    if (!scope.IsDefault && !r.Custom)
                    {
                        if (row != null)
                            await _ticketTypeAiSettingRepository.DeleteAsync(row);
                        continue;
                    }

                    row ??= new TicketTypeAiSetting
                    {
                        IdTicketType = r.IdTicketType,
                        Level = level,
                        IdDealer = idDealer,
                        IdFinancialAdvisor = idFinancialAdvisor,
                        IdCompany = idCompany,
                    };
                    row.AiEnabled = r.AiEnabled;
                    row.AiMaxAttempts = r.AiMaxAttempts;
                    row.UseParentDocuments = r.UseParentDocuments;
                    await _ticketTypeAiSettingRepository.UpsertAsync(row);
                }

                retVal.TicketTypeAiSetting = Settings(scope);

                return retVal;
            }, isSubProcess);
        }


        // livello gestito: i default del SuperUser o il nodo dell'utente
        private class SettingScope
        {
            public LevelScope Node { get; set; } = new LevelScope();
            public bool IsDefault { get; set; }
            public TicketLevel Level => Node.Level;
        }

        private async Task<List<SettingScope>> Scopes()
        {
            var user = await _userManager.FindByIdAsync(this.CurrentUserId)
                       ?? throw new UserMessageException(InfrastructureMessages.Get("UserNotFound"));
            var roles = await _userManager.GetRolesAsync(user);
            var levels = LevelScope.For(roles, this.CurrentDealer, this.CurrentFinancialAdvisor, this.CurrentCompany);

            var retVal = new List<SettingScope>();
            if (levels.Any(l => l.Level == TicketLevel.SuperUser))
                retVal.AddRange(AiLevels.Select(l => new SettingScope { Node = new LevelScope { Level = l, CanEdit = true }, IsDefault = true }));
            else
                retVal.AddRange(levels.Where(l => AiLevels.Contains(l.Level)).Select(l => new SettingScope { Node = l }));
            return retVal;
        }

        private async Task<SettingScope> Scope(TicketLevel level, bool edit)
        {
            var scope = (await Scopes()).FirstOrDefault(s => s.Level == level);
            if (scope == null || (edit && !scope.Node.CanEdit))
                throw new UserMessageException(InfrastructureMessages.Get("TicketTypeAiSettingNotAllowed"));
            return scope;
        }

        // tipi attivi abilitati al livello (per l'azienda, generali o dei suoi applicativi attivi)
        private List<TicketType> Types(SettingScope scope)
        {
            var level = scope.Level;
            var enabled = _ticketTypeLevelRepository.FindAll(l => l.Level == level).Select(l => l.IdTicketType).ToList();
            var types = _ticketTypeRepository.FindAll(t => t.Active && enabled.Contains(t.Id)).OrderBy(t => t.Ordine).ThenBy(t => t.Codice).ToList();
            if (scope.Node.IdCompany != null)
            {
                var applications = _companyApplicationRepository.ActiveApplications(scope.Node.IdCompany.Value);
                types = types.Where(t => t.ApplicationType == null || applications.Contains(t.ApplicationType.Value)).ToList();
            }
            return types;
        }

        private List<TicketTypeAiSettingModel> Settings(SettingScope scope)
        {
            var node = scope.Node;
            var level = node.Level;
            var rows = _ticketTypeAiSettingRepository.FindAll(s => s.Level == level).ToList();
            var defaults = rows.Where(s => s.IdDealer == null && s.IdFinancialAdvisor == null && s.IdCompany == null).ToList();
            var own = scope.IsDefault
                ? new List<TicketTypeAiSetting>()
                : rows.Where(s => s.IdDealer == node.IdDealer && s.IdFinancialAdvisor == node.IdFinancialAdvisor && s.IdCompany == node.IdCompany).ToList();

            return Types(scope).Select(t =>
            {
                var d = defaults.FirstOrDefault(s => s.IdTicketType == t.Id);
                var o = own.FirstOrDefault(s => s.IdTicketType == t.Id);
                var effective = Effective(d, o);
                return new TicketTypeAiSettingModel
                {
                    IdTicketType = t.Id,
                    Codice = t.Codice,
                    Descrizione = t.Descrizione,
                    ApplicationType = t.ApplicationType,
                    Custom = o != null,
                    AiEnabled = effective.AiEnabled,
                    AiMaxAttempts = effective.AiMaxAttempts,
                    DefaultAiEnabled = d?.AiEnabled ?? false,
                    DefaultAiMaxAttempts = d?.AiMaxAttempts ?? 1,
                    UseParentDocuments = effective.UseParentDocuments,
                    DefaultUseParentDocuments = d?.UseParentDocuments ?? true,
                };
            }).ToList();
        }

        // impostazione effettiva: riga del nodo, altrimenti default del livello, altrimenti AI disattivata
        // (senza righe la documentazione dei livelli sopra si usa)
        public static (bool AiEnabled, int AiMaxAttempts, bool UseParentDocuments) Effective(TicketTypeAiSetting? levelDefault, TicketTypeAiSetting? node)
        {
            var s = node ?? levelDefault;
            return s == null ? (false, 1, true) : (s.AiEnabled, Math.Max(1, s.AiMaxAttempts), s.UseParentDocuments);
        }
    }

    public interface ITicketTypeAiSettingService : IServiceBase
    {
        Task<GenericResult<TicketTypeAiSettingScopeOutModel>> TicketTypeAiSettingScope(GenericRequest<TicketTypeAiSettingScopeInModel> model, Boolean isSubProcess);
        Task<GenericResult<TicketTypeAiSettingListOutModel>> TicketTypeAiSettingList(GenericRequest<TicketTypeAiSettingListInModel> model, Boolean isSubProcess);
        Task<GenericResult<TicketTypeAiSettingPutOutModel>> TicketTypeAiSettingPut(GenericRequest<TicketTypeAiSettingPutInModel> model, Boolean isSubProcess);
    }
}
