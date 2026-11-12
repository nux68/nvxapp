using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Assets.Models
{
    // ------------------------------------------------------------------------------------------------ prenotazioni (AST-PREN)
    public class Ast_RisorsaModel
    {
        public int Id { get; set; }
        public int Tipo { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public int? IdAz_Sedi { get; set; }
        public string? Sede { get; set; }
        public int? Capienza { get; set; }
        public int? IdAst_Veicolo { get; set; }
        public string? Veicolo { get; set; }
        public bool Attiva { get; set; } = true;
        public string? Note { get; set; }
        /// <summary>le prenotazioni degli utenti vanno approvate da un amministratore</summary>
        public bool RichiedeApprovazione { get; set; }
    }

    public class Ast_PrenotazioneModel
    {
        public int Id { get; set; }
        public int IdAst_Risorsa { get; set; }
        public string? Risorsa { get; set; }
        public int TipoRisorsa { get; set; }
        public DateTime Inizio { get; set; }
        public DateTime Fine { get; set; }
        public string? Titolo { get; set; }
        public string? Note { get; set; }
        /// <summary>AstStatoPrenotazione: 0 confermata, 1 annullata, 2 in attesa di approvazione, 3 rifiutata</summary>
        public int Stato { get; set; }
        /// <summary>motivo del rifiuto (solo per le proprie e per gli amministratori)</summary>
        public string? MotivoRifiuto { get; set; }
        // confermata con la richiesta di annullamento in attesa di approvazione
        public bool AnnullamentoRichiesto { get; set; }
        // l'amministratore collegato puo' approvarla o rifiutarla adesso (richiesta o annullamento in attesa): id della richiesta comune
        public bool DaDecidere { get; set; }
        public int? IdRichiesta { get; set; }
        // richiedente: ritira la richiesta ancora in attesa ("Annulla")
        public bool Ritirabile { get; set; }
        // richiedente: chiede l'annullamento della prenotazione approvata ("Cancella", va approvato)
        public bool AnnullamentoChiedibile { get; set; }
        /// <summary>prenotazione dell'utente collegato</summary>
        public bool Mia { get; set; }
        /// <summary>chi ha prenotato: solo per le proprie e per gli amministratori</summary>
        public string? Utente { get; set; }
        /// <summary>l'utente la puo' modificare o annullare (sua o amministratore, non passata, confermata o in attesa)</summary>
        public bool Modificabile { get; set; }
    }

    public class Ast_RisorsaListInModel { public bool Tutte { get; set; } }
    public class Ast_RisorsaListOutModel : ModelResult
    {
        public Ast_PermessiModel Permessi { get; set; } = new();
        public List<Ast_RisorsaModel> Risorsa { get; set; } = new();
        public List<Ast_LookupModel> Sede { get; set; } = new();
        /// <summary>veicoli in pool della flotta, per le auto prenotabili</summary>
        public List<Ast_LookupModel> VeicoloPool { get; set; } = new();
    }

    public class Ast_RisorsaPutInModel { public Ast_RisorsaModel Risorsa { get; set; } = new(); }
    public class Ast_RisorsaPutOutModel : ModelResult { public Ast_RisorsaModel Risorsa { get; set; } = new(); }

    public class Ast_PrenotazioneListInModel
    {
        // facoltative: senza, da oggi per 7 giorni (es. pagina di modifica aperta da "Nuova richiesta", che chiede solo risorse e limiti)
        public DateTime? Dal { get; set; }
        public DateTime? Al { get; set; }
        public int? IdAst_Risorsa { get; set; }
        public bool SoloMie { get; set; }
    }

    public class Ast_PrenotazioneListOutModel : ModelResult
    {
        public Ast_PermessiModel Permessi { get; set; } = new();
        public List<Ast_PrenotazioneModel> Prenotazione { get; set; } = new();
        public List<Ast_RisorsaModel> Risorsa { get; set; } = new();
        public int DurataMaxOre { get; set; }
        public int AnticipoMaxGiorni { get; set; }
    }

    public class Ast_PrenotazionePutInModel { public Ast_PrenotazioneModel Prenotazione { get; set; } = new(); }
    public class Ast_PrenotazionePutOutModel : ModelResult { public Ast_PrenotazioneModel Prenotazione { get; set; } = new(); }
}
