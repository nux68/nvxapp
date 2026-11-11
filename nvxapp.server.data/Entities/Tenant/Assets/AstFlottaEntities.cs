using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.Assets
{
    /*
     Flotta aziendale (ANALISI_ASSETS_APPLICATIVO.md, AST-FLO): veicoli, assegnazioni ai dipendenti (con il fringe benefit),
     scadenze (registrate nello scadenzario comune), costi e multe.

     Assegnazione e Dip_RapportoAutovetture (Common, D5): Common non puo' riferire tabelle di un applicativo, quindi il legame
     sta qui (scelta provvisoria AST-D3, tabella ponte): Ast_VeicoloAssegnazione.IdDip_RapportoAutovetture e' l'autovettura
     del dipendente che Assets crea/aggiorna in Common all'assegnazione, usata dalle note spese delle Presenze per i rimborsi km.
    */

    public enum AstAlimentazione { Benzina = 0, Diesel = 1, Gpl = 2, Metano = 3, Ibrida = 4, Elettrica = 5, Altro = 6 }

    public enum AstPossesso { Proprieta = 0, Noleggio = 1, Leasing = 2 }

    public enum AstUsoVeicolo { Assegnato = 0, Pool = 1 }

    public enum AstStatoVeicolo { Attivo = 0, Dismesso = 1 }

    public enum AstTipoScadenzaVeicolo { Bollo = 0, Assicurazione = 1, Revisione = 2, Tagliando = 3, Altro = 4 }

    public enum AstTipoCostoVeicolo { Carburante = 0, Manutenzione = 1, Pedaggi = 2, Parcheggio = 3, Assicurazione = 4, Bollo = 5, Altro = 6 }

    public enum AstStatoMulta { DaNotificare = 0, Notificata = 1, Pagata = 2, Contestata = 3 }

    // veicolo dell'azienda
    public class Ast_Veicolo : BaseEntity
    {
        [Required]
        public int IdAz_Anagrafica { get; set; }
        public Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(15)]
        public string Targa { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Marca { get; set; }

        [MaxLength(50)]
        public string? Modello { get; set; }

        public AstAlimentazione Alimentazione { get; set; }

        public DateTime? DataImmatricolazione { get; set; }

        // proprieta' / noleggio / leasing, con i dati del contratto (la scadenza va nello scadenzario)
        public AstPossesso Possesso { get; set; }

        [MaxLength(100)]
        public string? Fornitore { get; set; }

        [MaxLength(50)]
        public string? NumeroContratto { get; set; }

        public DateTime? ScadenzaContratto { get; set; }

        public int? KmInclusi { get; set; }

        [Column(TypeName = "numeric(12,2)")]
        public decimal? CanoneMensile { get; set; }

        // sede (Common, facoltativa)
        public int? IdAz_Sedi { get; set; }
        public Az_Sedi? Az_SediNavigation { get; set; }

        public AstUsoVeicolo Uso { get; set; }

        // fringe benefit: costo chilometrico ACI (euro/km) ed emissioni di CO2 (g/km) per la percentuale
        [Column(TypeName = "numeric(10,4)")]
        public decimal CostoKmAci { get; set; }

        public int? EmissioniCO2 { get; set; }

        public int? KmAttuali { get; set; }

        public AstStatoVeicolo Stato { get; set; }

        [MaxLength(2000)]
        public string? Note { get; set; }

        public ICollection<Ast_VeicoloAssegnazione>? Ast_VeicoloAssegnazione { get; set; }
        public ICollection<Ast_VeicoloScadenza>? Ast_VeicoloScadenza { get; set; }
        public ICollection<Ast_VeicoloCosto>? Ast_VeicoloCosto { get; set; }
        public ICollection<Ast_Multa>? Ast_Multa { get; set; }
    }

    // periodo di assegnazione del veicolo a un dipendente (DataFine null = in corso)
    public class Ast_VeicoloAssegnazione : BaseEntity
    {
        [Required]
        public int IdAst_Veicolo { get; set; }
        public Ast_Veicolo? Ast_VeicoloNavigation { get; set; }

        [Required]
        public int IdDip_Anagrafica { get; set; }
        public Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        public DateTime DataInizio { get; set; }
        public DateTime? DataFine { get; set; }

        // uso promiscuo (lavoro e privato): genera il fringe benefit
        public bool UsoPromiscuo { get; set; } = true;

        // percentuale del fringe benefit sul costo convenzionale di 15.000 km (25/30/50/60 secondo le emissioni)
        [Column(TypeName = "numeric(5,2)")]
        public decimal PercentualeFringe { get; set; }

        // autovettura del dipendente in Common (Dip_RapportoAutovetture), usata dalle note spese delle Presenze
        public int? IdDip_RapportoAutovetture { get; set; }
        public Dip_RapportoAutovetture? Dip_RapportoAutovettureNavigation { get; set; }

        // avviso agli amministratori del veicolo da restituire alla cessazione (AST-BENI.3)
        public DateTime? DataAvvisoCessazione { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }

    // scadenza del veicolo (registrata nello scadenzario comune, fonte "ast.veicolo.scadenza")
    public class Ast_VeicoloScadenza : BaseEntity
    {
        [Required]
        public int IdAst_Veicolo { get; set; }
        public Ast_Veicolo? Ast_VeicoloNavigation { get; set; }

        public AstTipoScadenzaVeicolo Tipo { get; set; }

        [MaxLength(100)]
        public string? Descrizione { get; set; }

        public DateTime DataScadenza { get; set; }

        // tagliando: chilometri a cui va fatto
        public int? Km { get; set; }

        [Column(TypeName = "numeric(12,2)")]
        public decimal? Importo { get; set; }

        // eseguita (pagato il bollo, fatta la revisione...): la scadenza si chiude nello scadenzario
        public DateTime? DataEsecuzione { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }

    // costo del veicolo (carburante, manutenzione, pedaggi...)
    public class Ast_VeicoloCosto : BaseEntity
    {
        [Required]
        public int IdAst_Veicolo { get; set; }
        public Ast_Veicolo? Ast_VeicoloNavigation { get; set; }

        public DateTime Data { get; set; }

        public AstTipoCostoVeicolo Tipo { get; set; }

        [Column(TypeName = "numeric(12,2)")]
        public decimal Importo { get; set; }

        public int? Km { get; set; }

        [MaxLength(200)]
        public string? Descrizione { get; set; }

        // numero del documento (fattura, scontrino)
        [MaxLength(50)]
        public string? Documento { get; set; }
    }

    // multa: il conducente si propone dal periodo di assegnazione alla data dell'infrazione
    public class Ast_Multa : BaseEntity
    {
        [Required]
        public int IdAst_Veicolo { get; set; }
        public Ast_Veicolo? Ast_VeicoloNavigation { get; set; }

        public DateTime DataInfrazione { get; set; }

        [MaxLength(50)]
        public string? NumeroVerbale { get; set; }

        [MaxLength(200)]
        public string? Luogo { get; set; }

        [MaxLength(500)]
        public string? Descrizione { get; set; }

        [Column(TypeName = "numeric(12,2)")]
        public decimal Importo { get; set; }

        public int? IdDip_Anagrafica { get; set; }
        public Dip_Anagrafica? Dip_AnagraficaNavigation { get; set; }

        public AstStatoMulta Stato { get; set; }

        public DateTime? DataNotifica { get; set; }
        public DateTime? DataPagamento { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
