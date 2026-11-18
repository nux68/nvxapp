using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Mcp.Models
{
    // Accesso di un'AI esterna tramite MCP (nvxapp.server/docs/ANALISI_INFRASTRUCTURE_MCP.md): modelli delle pagine
    // del client (token personali, impostazione dell'azienda, registro delle chiamate).

    /// <summary>token personale nell'elenco: il valore non c'e' mai (si vede una volta sola, alla creazione)</summary>
    public class McpTokenModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        /// <summary>primi caratteri del token, per riconoscerlo</summary>
        public string TokenPrefix { get; set; } = string.Empty;
        /// <summary>utente con cui lavora il client AI</summary>
        public string UserName { get; set; } = string.Empty;
        /// <summary>azienda su cui lavora; vuoto = nessuna (centro, studio, superuser)</summary>
        public string Company { get; set; } = string.Empty;
        /// <summary>vero se il token lavora come un utente diverso da chi lo ha creato (impersonificazione)</summary>
        public bool Impersonated { get; set; }
        public string Language { get; set; } = "it";
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        /// <summary>"active", "expired" o "revoked"</summary>
        public string State { get; set; } = string.Empty;
    }

    /// <summary>stato dell'accesso AI per l'utente corrente: serve a tutte le pagine per spiegare cosa manca</summary>
    public class McpStatusModel
    {
        /// <summary>endpoint MCP presente in questa installazione (Mcp:Enabled)</summary>
        public bool ServerEnabled { get; set; }
        /// <summary>indirizzo da indicare al client AI</summary>
        public string EndpointUrl { get; set; } = string.Empty;
        /// <summary>l'utente lavora in un'azienda</summary>
        public bool HasCompany { get; set; }
        /// <summary>accesso AI attivo per l'azienda (sempre falso senza azienda)</summary>
        public bool CompanyEnabled { get; set; }
        /// <summary>livello massimo degli strumenti per l'azienda (McpAccessLevel)</summary>
        public int CompanyMaxLevel { get; set; }
        /// <summary>livello massimo ammesso dall'installazione (Mcp:MaxLevel)</summary>
        public int ServerMaxLevel { get; set; }
        /// <summary>l'utente puo' cambiare l'impostazione dell'azienda</summary>
        public bool CanManageCompany { get; set; }
        /// <summary>durata massima di un token, in giorni (Mcp:TokenMaxDays)</summary>
        public int TokenMaxDays { get; set; }
    }

    // ---- elenco dei token -------------------------------------------------------------------------

    public class McpTokenListInModel
    {
        /// <summary>anche i token revocati o scaduti</summary>
        public bool ShowAll { get; set; }
    }

    public class McpTokenListOutModel : ModelResult
    {
        public List<McpTokenModel> McpToken { get; set; } = new();
        public McpStatusModel Status { get; set; } = new();
    }

    // ---- creazione --------------------------------------------------------------------------------

    public class McpTokenCreateInModel
    {
        public string Name { get; set; } = string.Empty;
        /// <summary>durata in giorni (da 1 a Mcp:TokenMaxDays)</summary>
        public int Days { get; set; } = 90;
        /// <summary>"it" o "en"; vuoto = lingua della richiesta</summary>
        public string? Language { get; set; }
    }

    public class McpTokenCreateOutModel : ModelResult
    {
        public McpTokenModel? McpToken { get; set; }
        /// <summary>valore del token: restituito solo qui, non e' piu' leggibile in seguito</summary>
        public string Token { get; set; } = string.Empty;
        public string EndpointUrl { get; set; } = string.Empty;
    }

    // ---- revoca -----------------------------------------------------------------------------------

    public class McpTokenRevokeInModel
    {
        public int Id { get; set; }
    }

    public class McpTokenRevokeOutModel : ModelResult
    {
    }

    // ---- impostazione dell'azienda ----------------------------------------------------------------

    public class McpCompanySettingModel
    {
        public bool Enabled { get; set; }
        /// <summary>McpAccessLevel: 1 lettura, 2 modifiche personali, 3 modifiche amministrative</summary>
        public int MaxLevel { get; set; } = 1;
    }

    public class McpCompanySettingGetInModel
    {
    }

    public class McpCompanySettingGetOutModel : ModelResult
    {
        public McpCompanySettingModel McpCompanySetting { get; set; } = new();
        public McpStatusModel Status { get; set; } = new();
    }

    public class McpCompanySettingPutInModel
    {
        public McpCompanySettingModel McpCompanySetting { get; set; } = new();
    }

    public class McpCompanySettingPutOutModel : ModelResult
    {
        public McpCompanySettingModel McpCompanySetting { get; set; } = new();
    }

    // ---- registro delle chiamate ------------------------------------------------------------------

    public class McpCallLogModel
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string TokenName { get; set; } = string.Empty;
        /// <summary>utente che ha fatto il login (chi ha creato il token)</summary>
        public string UserNameLogin { get; set; } = string.Empty;
        /// <summary>utente con cui ha lavorato il client AI</summary>
        public string UserName { get; set; } = string.Empty;
        public string Tool { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int DurationMs { get; set; }
    }

    public class McpCallLogListInModel
    {
        /// <summary>giorni all'indietro da oggi (da 1 a 365)</summary>
        public int Days { get; set; } = 30;
    }

    public class McpCallLogListOutModel : ModelResult
    {
        public List<McpCallLogModel> McpCallLog { get; set; } = new();
        /// <summary>vero se l'elenco e' quello di tutta l'azienda (amministratori), falso se sono solo le proprie chiamate</summary>
        public bool WholeCompany { get; set; }
        /// <summary>vero se le righe sono piu' del massimo restituito</summary>
        public bool Truncated { get; set; }
    }
}
