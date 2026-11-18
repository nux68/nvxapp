using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Mcp.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Site;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Mcp
{
    /*
     Accesso di un'AI esterna tramite MCP (nvxapp.server/docs/ANALISI_INFRASTRUCTURE_MCP.md): servizio delle pagine del client.
        - token personali (sezione 7): McpTokenList, McpTokenCreate, McpTokenRevoke. Il token appartiene all'utente che
          ha fatto il login e lavora come l'utente corrente (lo stesso, oppure quello impersonato in quel momento, con il
          suo centro / studio / azienda: sezione 7.1). Il valore si vede solo nella risposta di McpTokenCreate.
        - impostazione dell'azienda (sezione 8.6): McpCompanySettingGet / Put, solo CompanyPowerAdmin. Accesso spento
          finche' non viene attivato; il livello non puo' superare quello dell'installazione (Mcp:MaxLevel).
        - registro delle chiamate (sezione 8.4): McpCallLogList. Gli amministratori dell'azienda vedono tutte le chiamate
          sull'azienda, gli altri solo quelle fatte con i propri token.
    */
    public class McpAccessService : ServiceBase, IMcpAccessService
    {
        public const int MaxLogRows = 1000;
        public const int MaxActiveTokens = 20;

        private readonly IMcpAccessTokenRepository _mcpAccessTokenRepository;
        private readonly IMcpCompanySettingRepository _mcpCompanySettingRepository;
        private readonly IMcpCallLogRepository _mcpCallLogRepository;
        private readonly ICompanyRepository _companyRepository;

        public McpAccessService(IMapper mapper,
                                UserManager<ApplicationUser> userManager,
                                IAspNetUsersRepository aspNetUsersRepository,
                                IOptions<JwtParameter> jwtParameter,
                                IHttpContextAccessor httpContextAccessor,
                                IConfiguration configuration,

                                IMcpAccessTokenRepository mcpAccessTokenRepository,
                                IMcpCompanySettingRepository mcpCompanySettingRepository,
                                IMcpCallLogRepository mcpCallLogRepository,
                                ICompanyRepository companyRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _mcpAccessTokenRepository = mcpAccessTokenRepository;
            _mcpCompanySettingRepository = mcpCompanySettingRepository;
            _mcpCallLogRepository = mcpCallLogRepository;
            _companyRepository = companyRepository;
        }

        // ---------------------------------------------------------------------------
        // Token personali
        // ---------------------------------------------------------------------------

        public virtual async Task<GenericResult<McpTokenListOutModel>> McpTokenList(GenericRequest<McpTokenListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new McpTokenListOutModel();
                var login = Login();
                bool showAll = model.Data?.ShowAll ?? false;
                var now = DateTime.Now;

                var tokens = await _mcpAccessTokenRepository.FindAll(x => x.IdAspNetUsersLogin == login)
                                                            .Where(x => showAll || (x.RevokedAt == null && x.ExpiresAt > now))
                                                            .OrderByDescending(x => x.CreatedAt)
                                                            .ToListAsync();

                retVal.McpToken = await Modelli(tokens, now);
                retVal.Status = await Stato();
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<McpTokenCreateOutModel>> McpTokenCreate(GenericRequest<McpTokenCreateInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new McpTokenCreateOutModel();
                var login = Login();
                var io = this.CurrentUserId;
                var now = DateTime.Now;

                if (!McpOptions.Enabled(_configuration))
                    throw new UserMessageException(InfrastructureMessages.Get("McpServerDisabled"));

                string name = (model.Data?.Name ?? string.Empty).Trim();
                if (name.Length == 0)
                    throw new UserMessageException(InfrastructureMessages.Get("McpTokenNameRequired"));
                if (name.Length > 100)
                    name = name[..100];

                int maxDays = McpOptions.TokenMaxDays(_configuration);
                int days = model.Data?.Days ?? 0;
                if (days < 1 || days > maxDays)
                    throw new UserMessageException(InfrastructureMessages.Get("McpTokenDaysInvalid", maxDays));

                int attivi = await _mcpAccessTokenRepository.FindAll(x => x.IdAspNetUsersLogin == login && x.RevokedAt == null && x.ExpiresAt > now).CountAsync();
                if (attivi >= MaxActiveTokens)
                    throw new UserMessageException(InfrastructureMessages.Get("McpTokenTooMany", MaxActiveTokens));

                string language = (model.Data?.Language ?? string.Empty).Trim().ToLowerInvariant();
                if (language != "it" && language != "en")
                    language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en" ? "en" : "it";

                string token = McpTokenValue.New();
                var entity = await _mcpAccessTokenRepository.CreateAsync(new McpAccessToken
                {
                    IdAspNetUsersLogin = login,
                    IdAspNetUsers = io,
                    IdDealer = Id(this.CurrentDealer),
                    IdFinancialAdvisor = Id(this.CurrentFinancialAdvisor),
                    IdCompany = Id(this.CurrentCompany),
                    Tenant = string.IsNullOrEmpty(this.CurrentTenat) ? null : this.CurrentTenat,
                    Name = name,
                    TokenHash = McpTokenValue.Hash(token),
                    TokenPrefix = McpTokenValue.DisplayPrefix(token),
                    Language = language,
                    CreatedAt = now,
                    ExpiresAt = now.AddDays(days)
                });

                retVal.McpToken = (await Modelli(new List<McpAccessToken> { entity }, now)).First();
                retVal.Token = token;
                retVal.EndpointUrl = EndpointUrl();
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<McpTokenRevokeOutModel>> McpTokenRevoke(GenericRequest<McpTokenRevokeInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new McpTokenRevokeOutModel();
                var login = Login();
                int id = model.Data?.Id ?? 0;

                // solo i propri token: un id di un altro utente vale come "non trovato"
                var entity = await _mcpAccessTokenRepository.FindAll(x => x.Id == id && x.IdAspNetUsersLogin == login).FirstOrDefaultAsync();
                if (entity == null)
                    throw new UserMessageException(InfrastructureMessages.Get("McpTokenNotFound"));

                if (entity.RevokedAt == null)
                {
                    entity.RevokedAt = DateTime.Now;
                    await _mcpAccessTokenRepository.UpdateAsync(entity);
                }
                return retVal;
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------
        // Impostazione dell'azienda
        // ---------------------------------------------------------------------------

        public virtual async Task<GenericResult<McpCompanySettingGetOutModel>> McpCompanySettingGet(GenericRequest<McpCompanySettingGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new McpCompanySettingGetOutModel();
                int idCompany = await AziendaDaAmministrare();

                var setting = await _mcpCompanySettingRepository.OfCompanyAsync(idCompany);
                retVal.McpCompanySetting = setting == null ? new McpCompanySettingModel() : _mapper.Map<McpCompanySettingModel>(setting);
                retVal.Status = await Stato();
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<McpCompanySettingPutOutModel>> McpCompanySettingPut(GenericRequest<McpCompanySettingPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new McpCompanySettingPutOutModel();
                int idCompany = await AziendaDaAmministrare();

                var richiesto = model.Data?.McpCompanySetting ?? new McpCompanySettingModel();
                int massimo = (int)McpOptions.MaxLevel(_configuration);
                if (richiesto.MaxLevel < (int)McpAccessLevel.Read || richiesto.MaxLevel > massimo)
                    throw new UserMessageException(InfrastructureMessages.Get("McpLevelNotAllowed"));

                var setting = await _mcpCompanySettingRepository.OfCompanyAsync(idCompany);
                if (setting == null)
                    setting = await _mcpCompanySettingRepository.CreateAsync(new McpCompanySetting
                    {
                        IdCompany = idCompany,
                        Enabled = richiesto.Enabled,
                        MaxLevel = (McpAccessLevel)richiesto.MaxLevel
                    });
                else
                {
                    setting.Enabled = richiesto.Enabled;
                    setting.MaxLevel = (McpAccessLevel)richiesto.MaxLevel;
                    setting = await _mcpCompanySettingRepository.UpdateAsync(setting);
                }

                retVal.McpCompanySetting = _mapper.Map<McpCompanySettingModel>(setting);
                return retVal;
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------
        // Registro delle chiamate
        // ---------------------------------------------------------------------------

        public virtual async Task<GenericResult<McpCallLogListOutModel>> McpCallLogList(GenericRequest<McpCallLogListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new McpCallLogListOutModel();
                var login = Login();
                int days = Math.Clamp(model.Data?.Days ?? 30, 1, 365);
                var dal = DateTime.Today.AddDays(-days);

                int? idCompany = Id(this.CurrentCompany);
                bool tuttaAzienda = idCompany != null && await HaRuolo(nameof(RoleCode.CompanyPowerAdmin), nameof(RoleCode.CompanyAdmin));

                var query = tuttaAzienda
                    ? _mcpCallLogRepository.FindAll(x => x.IdCompany == idCompany && x.Data >= dal)
                    : _mcpCallLogRepository.FindAll(x => x.IdAspNetUsersLogin == login && x.Data >= dal);

                var righe = await query.OrderByDescending(x => x.Data).ThenByDescending(x => x.Id).Take(MaxLogRows + 1).ToListAsync();
                retVal.Truncated = righe.Count > MaxLogRows;
                retVal.McpCallLog = _mapper.Map<List<McpCallLogModel>>(righe.Take(MaxLogRows).ToList());
                retVal.WholeCompany = tuttaAzienda;
                return retVal;
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------

        // utente che ha fatto il login: proprietario dei token (anche mentre impersona qualcuno)
        private string Login()
        {
            var login = this.UserIdFirstConnection;
            if (string.IsNullOrEmpty(login))
                login = this.CurrentUserId;
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(this.CurrentUserId))
                throw new UserMessageException(InfrastructureMessages.Get("McpNotAllowed"));
            return login;
        }

        private static int? Id(string? value) => int.TryParse(value, out var id) && id > 0 ? id : null;

        private async Task<bool> HaRuolo(params string[] ruoli)
        {
            var utente = await _userManager.FindByIdAsync(this.CurrentUserId);
            if (utente == null)
                return false;
            var suoi = await _userManager.GetRolesAsync(utente);
            return suoi.Any(r => ruoli.Contains(r, StringComparer.OrdinalIgnoreCase));
        }

        // azienda corrente, che solo il CompanyPowerAdmin puo' configurare
        private async Task<int> AziendaDaAmministrare()
        {
            int? idCompany = Id(this.CurrentCompany);
            if (idCompany == null)
                throw new UserMessageException(InfrastructureMessages.Get("McpNoCompany"));
            if (!await HaRuolo(nameof(RoleCode.CompanyPowerAdmin)))
                throw new UserMessageException(InfrastructureMessages.Get("McpNotAllowed"));
            return idCompany.Value;
        }

        private string EndpointUrl() => SiteUrl.ApiBase(_configuration, _httpContextAccessor) + McpOptions.EndpointPath;

        private async Task<McpStatusModel> Stato()
        {
            var stato = new McpStatusModel
            {
                ServerEnabled = McpOptions.Enabled(_configuration),
                EndpointUrl = EndpointUrl(),
                ServerMaxLevel = (int)McpOptions.MaxLevel(_configuration),
                TokenMaxDays = McpOptions.TokenMaxDays(_configuration)
            };

            int? idCompany = Id(this.CurrentCompany);
            if (idCompany != null)
            {
                stato.HasCompany = true;
                var setting = await _mcpCompanySettingRepository.OfCompanyAsync(idCompany.Value);
                stato.CompanyEnabled = setting?.Enabled ?? false;
                stato.CompanyMaxLevel = Math.Min((int)(setting?.MaxLevel ?? McpAccessLevel.Read), stato.ServerMaxLevel);
                stato.CanManageCompany = await HaRuolo(nameof(RoleCode.CompanyPowerAdmin));
            }
            return stato;
        }

        // modelli dell'elenco: nome dell'utente con cui lavora il token, azienda e stato
        private async Task<List<McpTokenModel>> Modelli(List<McpAccessToken> tokens, DateTime now)
        {
            var idUtenti = tokens.Select(x => x.IdAspNetUsers).Distinct().ToList();
            var utenti = await _aspNetUsersRepository.FindAll(x => idUtenti.Contains(x.Id))
                                                     .Select(x => new { x.Id, x.UserName })
                                                     .ToDictionaryAsync(x => x.Id, x => x.UserName ?? string.Empty);

            var idAziende = tokens.Where(x => x.IdCompany != null).Select(x => x.IdCompany!.Value).Distinct().ToList();
            var aziende = await _companyRepository.FindAll(x => idAziende.Contains(x.Id))
                                                  .Select(x => new { x.Id, x.Descrizione })
                                                  .ToDictionaryAsync(x => x.Id, x => x.Descrizione ?? string.Empty);

            var modelli = new List<McpTokenModel>();
            foreach (var token in tokens)
            {
                var modello = _mapper.Map<McpTokenModel>(token);
                modello.UserName = utenti.GetValueOrDefault(token.IdAspNetUsers, string.Empty);
                modello.Company = token.IdCompany != null ? aziende.GetValueOrDefault(token.IdCompany.Value, string.Empty) : string.Empty;
                modello.State = token.RevokedAt != null ? "revoked" : token.ExpiresAt <= now ? "expired" : "active";
                modelli.Add(modello);
            }
            return modelli;
        }
    }

    public interface IMcpAccessService : IServiceBase
    {
        Task<GenericResult<McpTokenListOutModel>> McpTokenList(GenericRequest<McpTokenListInModel> model, Boolean isSubProcess);
        Task<GenericResult<McpTokenCreateOutModel>> McpTokenCreate(GenericRequest<McpTokenCreateInModel> model, Boolean isSubProcess);
        Task<GenericResult<McpTokenRevokeOutModel>> McpTokenRevoke(GenericRequest<McpTokenRevokeInModel> model, Boolean isSubProcess);
        Task<GenericResult<McpCompanySettingGetOutModel>> McpCompanySettingGet(GenericRequest<McpCompanySettingGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<McpCompanySettingPutOutModel>> McpCompanySettingPut(GenericRequest<McpCompanySettingPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<McpCallLogListOutModel>> McpCallLogList(GenericRequest<McpCallLogListInModel> model, Boolean isSubProcess);
    }
}
