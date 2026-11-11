using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.Assets
{
    /*
     Beni assegnati (ANALISI_ASSETS_APPLICATIVO.md, AST-BENI): categorie, beni (PC, telefoni, badge, attrezzature) e
     assegnazioni ai dipendenti con consegna e restituzione. I verbali sono prese visione del servizio comune
     (IAcknowledgementService, fonti "ast.beni.consegna" e "ast.beni.restituzione": con il documentale attivo diventano
     il verbale di consegna dei beni da firmare).
    */

    public enum AstStatoBene { Disponibile = 0, Assegnato = 1, InRiparazione = 2, Dismesso = 3 }

    // categoria dei beni (PC, telefono, badge...): le predefinite si creano all'attivazione
    public class Ast_Categoria : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(100)]
        public string Descrizione { get; set; } = string.Empty;

        public bool Attivo { get; set; } = true;

        public int Ordine { get; set; }
    }

    // bene dell'azienda
    public class Ast_Bene : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        // codice di inventario: unico tra i beni dell'azienda
        [Required]
        [MaxLength(30)]
        public string CodiceInventario { get; set; } = string.Empty;

        [Required]
        public int IdAst_Categoria { get; set; }
        public Ast_Categoria? Ast_CategoriaNavigation { get; set; }

        [Required]
        [MaxLength(200)]
        public string Descrizione { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? MarcaModello { get; set; }

        [MaxLength(100)]
        public string? NumeroSerie { get; set; }

        public DateTime? DataAcquisto { get; set; }

        [Column(TypeName = "numeric(12,2)")]
        public decimal? Valore { get; set; }

        // fine della garanzia: nello scadenzario comune (fonte "ast.bene.garanzia")
        public DateTime? ScadenzaGaranzia { get; set; }

        public int? IdAz_Sedi { get; set; }
        public Az_Sedi? Az_SediNavigation { get; set; }

        // lo stato "assegnato" lo tengono consegna e restituzione
        public AstStatoBene Stato { get; set; }

        [MaxLength(2000)]
        public string? Note { get; set; }

        public ICollection<Ast_BeneAssegnazione>? Ast_BeneAssegnazione { get; set; }
    }

    // consegna di un bene a un dipendente (DataRestituzione null = ancora in mano al dipendente)
    public class Ast_BeneAssegnazione : BaseEntity
    {
        [Required]
        public int IdAst_Bene { get; set; }
        public Ast_Bene? Ast_BeneNavigation { get; set; }

        [Required]
        public int IdDip_Anagrafica { get; set; }
        public Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        public DateTime DataConsegna { get; set; }

        [MaxLength(500)]
        public string? CondizioniConsegna { get; set; }

        public DateTime? DataRestituzione { get; set; }

        [MaxLength(500)]
        public string? CondizioniRestituzione { get; set; }

        // avviso agli amministratori del bene da restituire alla cessazione del dipendente (AST-BENI.3)
        public DateTime? DataAvvisoCessazione { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
