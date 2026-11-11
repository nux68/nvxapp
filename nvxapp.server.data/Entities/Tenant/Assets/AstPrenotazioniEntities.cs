using System.ComponentModel.DataAnnotations;
using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.data.Entities.Tenant.Assets
{
    /*
     Prenotazioni (ANALISI_ASSETS_APPLICATIVO.md, AST-PREN): risorse prenotabili (postazioni, sale, auto in pool) e prenotazioni.
     Conflitti: due prenotazioni confermate della stessa risorsa non si sovrappongono; il controllo sta nel repository
     (Ast_PrenotazioneRepository.SalvaSeLiberaAsync: transazione con lock della risorsa), cosi' vale anche con richieste concorrenti.
     Senza approvazione (scelta provvisoria AST-D4), con i limiti di durata e anticipo in Ast_Cfg.
    */

    public enum AstTipoRisorsa { Postazione = 0, Sala = 1, AutoPool = 2 }

    public enum AstStatoPrenotazione { Confermata = 0, Annullata = 1 }

    public class Ast_Risorsa : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        public AstTipoRisorsa Tipo { get; set; }

        [Required]
        [MaxLength(100)]
        public string Descrizione { get; set; } = string.Empty;

        public int? IdAz_Sedi { get; set; }
        public Az_Sedi? Az_SediNavigation { get; set; }

        // sale: numero di posti
        public int? Capienza { get; set; }

        // auto in pool: il veicolo della flotta (uso "pool")
        public int? IdAst_Veicolo { get; set; }
        public Ast_Veicolo? Ast_VeicoloNavigation { get; set; }

        public bool Attiva { get; set; } = true;

        [MaxLength(500)]
        public string? Note { get; set; }
    }

    public class Ast_Prenotazione : BaseEntity
    {
        [Required]
        public int IdAst_Risorsa { get; set; }
        public Ast_Risorsa? Ast_RisorsaNavigation { get; set; }

        // chi prenota (utente dell'azienda)
        [Required]
        [MaxLength(450)]
        public string IdAspNetUsers { get; set; } = string.Empty;
        public ApplicationUser? AspNetUsersNavigation { get; set; }

        public DateTime Inizio { get; set; }

        // esclusa
        public DateTime Fine { get; set; }

        [MaxLength(200)]
        public string? Titolo { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public AstStatoPrenotazione Stato { get; set; }

        public DateTime? DataAnnullamento { get; set; }
    }
}
