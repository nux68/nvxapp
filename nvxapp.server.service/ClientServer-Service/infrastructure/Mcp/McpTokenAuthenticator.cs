using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using nvxapp.server.data.Repositories.Public;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Mcp
{
    /// <summary>
    /// Identita' ricavata da un token personale MCP: gli stessi dati del token dell'applicazione (ServiceBase li legge
    /// dai claim della richiesta), piu' il token usato e la lingua dei testi.
    /// </summary>
    public class McpIdentity
    {
        public int IdToken { get; set; }
        public string TokenName { get; set; } = string.Empty;
        /// <summary>utente con cui si lavora (claim NameIdentifier)</summary>
        public string UserId { get; set; } = string.Empty;
        /// <summary>utente che ha fatto il login e ha creato il token (claim "useridfirstconnection")</summary>
        public string UserIdFirstConnection { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string Dealer { get; set; } = string.Empty;
        public string FinancialAdvisor { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Language { get; set; } = "it";
    }

    /*
     Verifica di un token personale MCP (ANALISI_INFRASTRUCTURE_MCP.md, sezione 7). Il token vale se:
        - l'endpoint e' attivo nell'installazione (Mcp:Enabled)
        - esiste (ricerca per impronta), non e' revocato e non e' scaduto
        - esistono ancora sia l'utente che lo ha creato sia quello con cui lavora
        - se lavora su un'azienda, l'utente e' ancora collegato all'azienda e non e' disabilitato
     L'accesso AI dell'azienda (McpCompanySetting) NON si controlla qui: con l'accesso spento il client si collega
     comunque e lo strumento "chi sono" gli spiega perche' non vede altri strumenti (vedi McpToolService).
    */
    public class McpTokenAuthenticator : IMcpTokenAuthenticator
    {
        // l'ultimo uso si aggiorna al massimo una volta al minuto: un client AI fa molte chiamate ravvicinate
        private static readonly TimeSpan _intervalloUltimoUso = TimeSpan.FromMinutes(1);

        private readonly IConfiguration _configuration;
        private readonly IMcpAccessTokenRepository _mcpAccessTokenRepository;
        private readonly IAspNetUsersRepository _aspNetUsersRepository;
        private readonly IUserCompanyRepository _userCompanyRepository;

        public McpTokenAuthenticator(IConfiguration configuration,
                                     IMcpAccessTokenRepository mcpAccessTokenRepository,
                                     IAspNetUsersRepository aspNetUsersRepository,
                                     IUserCompanyRepository userCompanyRepository)
        {
            _configuration = configuration;
            _mcpAccessTokenRepository = mcpAccessTokenRepository;
            _aspNetUsersRepository = aspNetUsersRepository;
            _userCompanyRepository = userCompanyRepository;
        }

        public async Task<McpIdentity?> AuthenticateAsync(string? token)
        {
            if (!McpOptions.Enabled(_configuration) || !McpTokenValue.LooksValid(token))
                return null;

            string hash = McpTokenValue.Hash(token!);
            var now = DateTime.Now;
            var entity = await _mcpAccessTokenRepository.FindAll(x => x.TokenHash == hash).FirstOrDefaultAsync();
            if (entity == null || entity.RevokedAt != null || entity.ExpiresAt <= now)
                return null;

            var utenti = new[] { entity.IdAspNetUsers, entity.IdAspNetUsersLogin }.Distinct().ToList();
            int presenti = await _aspNetUsersRepository.FindAll(x => utenti.Contains(x.Id)).CountAsync();
            if (presenti != utenti.Count)
                return null;

            if (entity.IdCompany != null)
            {
                bool collegato = await _userCompanyRepository
                    .FindAll(x => x.IdCompany == entity.IdCompany && x.IdAspNetUsers == entity.IdAspNetUsers && !x.Disabled)
                    .AnyAsync();
                if (!collegato)
                    return null;
            }

            if (entity.LastUsedAt == null || now - entity.LastUsedAt.Value > _intervalloUltimoUso)
                await _mcpAccessTokenRepository.SegnaUsoAsync(entity.Id, now);

            return new McpIdentity
            {
                IdToken = entity.Id,
                TokenName = entity.Name,
                UserId = entity.IdAspNetUsers,
                UserIdFirstConnection = entity.IdAspNetUsersLogin,
                Tenant = entity.Tenant ?? string.Empty,
                Dealer = entity.IdDealer?.ToString() ?? string.Empty,
                FinancialAdvisor = entity.IdFinancialAdvisor?.ToString() ?? string.Empty,
                Company = entity.IdCompany?.ToString() ?? string.Empty,
                Language = entity.Language
            };
        }
    }

    public interface IMcpTokenAuthenticator
    {
        /// <summary>identita' del token; null = token assente, sconosciuto, revocato, scaduto o non piu' valido</summary>
        Task<McpIdentity?> AuthenticateAsync(string? token);
    }
}
