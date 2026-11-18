using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Accesso di un'AI esterna tramite MCP (nvxapp.server/docs/ANALISI_INFRASTRUCTURE_MCP.md).
        - McpAccessToken   : token personale con cui un client AI (Claude Code, Claude Desktop...) si presenta all'endpoint MCP
        - McpCompanySetting: interruttore dell'azienda (accesso AI attivo e livello massimo degli strumenti)
        - McpCallLog       : registro delle chiamate agli strumenti
    */

    /// <summary>
    /// Livello di uno strumento MCP e livello massimo ammesso da un'azienda (i valori finiscono nel database: non vanno cambiati).
    /// </summary>
    public enum McpAccessLevel
    {
        /// <summary>strumento non pubblicato (comandi della sola interfaccia: navigazione, tema, lingua)</summary>
        Excluded = 0,
        /// <summary>sola lettura</summary>
        Read = 1,
        /// <summary>modifiche che l'utente fa per se' (timbrare, chiedere ferie, prenotare)</summary>
        PersonalWrite = 2,
        /// <summary>modifiche amministrative (dati di altri dipendenti, approvazioni, ricalcoli)</summary>
        AdminWrite = 3,
    }

    /// <summary>
    /// Token personale MCP. Conserva le due identita' del token dell'applicazione (sezione 7.1 dell'analisi):
    /// l'utente che ha fatto il login (proprietario del token) e l'utente con cui si lavora (lo stesso, oppure quello
    /// impersonato quando il token e' stato creato), con centro, studio e azienda di quest'ultimo.
    /// Del token si salva solo l'impronta (SHA-256): il valore si mostra una volta sola, alla creazione.
    /// </summary>
    public class McpAccessToken : BaseEntity
    {
        /// <summary>utente che ha fatto il login e ha creato il token (claim "useridfirstconnection")</summary>
        [Required]
        [MaxLength(450)]
        public string IdAspNetUsersLogin { get; set; } = string.Empty;

        /// <summary>utente con cui lavora il client AI: dati visibili, ruoli e strumenti sono i suoi</summary>
        [Required]
        [MaxLength(450)]
        public string IdAspNetUsers { get; set; } = string.Empty;

        public int? IdDealer { get; set; }
        public int? IdFinancialAdvisor { get; set; }
        public int? IdCompany { get; set; }

        [MaxLength(64)]
        public string? Tenant { get; set; }

        /// <summary>nome dato dall'utente (es. "Claude Desktop ufficio")</summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>impronta SHA-256 del token, in esadecimale</summary>
        [Required]
        [MaxLength(64)]
        public string TokenHash { get; set; } = string.Empty;

        /// <summary>primi caratteri del token, per riconoscerlo nell'elenco</summary>
        [Required]
        [MaxLength(16)]
        public string TokenPrefix { get; set; } = string.Empty;

        /// <summary>lingua dei testi restituiti al client AI ("it" / "en")</summary>
        [Required]
        [MaxLength(5)]
        public string Language { get; set; } = "it";

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }

    /// <summary>
    /// Accesso AI dell'azienda: spento finche' l'amministratore non lo attiva (decisione MCP-D4). Una riga per azienda.
    /// </summary>
    public class McpCompanySetting : BaseEntity
    {
        [Required]
        public int IdCompany { get; set; }

        public bool Enabled { get; set; }

        /// <summary>livello massimo degli strumenti pubblicati per l'azienda</summary>
        public McpAccessLevel MaxLevel { get; set; } = McpAccessLevel.Read;
    }

    /// <summary>
    /// Registro delle chiamate agli strumenti MCP: chi (utente del login e utente con cui si lavora), quale strumento,
    /// con quali parametri e con quale esito. I nomi sono copiati: restano leggibili anche dopo la revoca del token.
    /// </summary>
    public class McpCallLog : BaseEntity
    {
        public DateTime Data { get; set; }

        public int? IdMcpAccessToken { get; set; }

        [MaxLength(100)]
        public string? TokenName { get; set; }

        [Required]
        [MaxLength(450)]
        public string IdAspNetUsersLogin { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? UserNameLogin { get; set; }

        [Required]
        [MaxLength(450)]
        public string IdAspNetUsers { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? UserName { get; set; }

        public int? IdCompany { get; set; }

        [Required]
        [MaxLength(100)]
        public string Tool { get; set; } = string.Empty;

        /// <summary>parametri della chiamata (JSON, troncato)</summary>
        [MaxLength(4000)]
        public string? Arguments { get; set; }

        public bool Success { get; set; }

        /// <summary>esito in breve (messaggio dello strumento, troncato)</summary>
        [MaxLength(1000)]
        public string? Message { get; set; }

        public int DurationMs { get; set; }
    }
}
