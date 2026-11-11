using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Rendiconto mensile con cui il SuperUser liquida al centro la sua quota degli incassi (ANALISI_INFRASTRUCTURE_PAGAMENTI_CENTRI.md §3).
     Le aziende e gli studi pagano la licenza alla piattaforma (Lic_Pagamento); a fine mese per ogni centro si raccolgono
     i pagamenti incassati e non ancora liquidati (Lic_Pagamento.IdRendiconto), si toglie la trattenuta della piattaforma
     e il resto e' la quota del centro.
     IdDealer senza chiave esterna e DealerDescrizione fotografata: il rendiconto resta anche se il centro viene cancellato.
     Importi e percentuale sono fotografati alla generazione; dopo la conferma non cambiano.
    */
    public class Pag_Rendiconto : BaseEntity
    {
        public int IdDealer { get; set; }

        [MaxLength(50)]
        public string? DealerDescrizione { get; set; }

        public int Anno { get; set; }
        public int Mese { get; set; }

        public int NumeroPagamenti { get; set; }

        /// <summary>somma dei pagamenti incassati del rendiconto</summary>
        public decimal TotaleIncassato { get; set; }

        /// <summary>somma (positiva) dei pagamenti gia' liquidati e poi rimborsati, da togliere</summary>
        public decimal TotaleRettifiche { get; set; }

        /// <summary>trattenuta della piattaforma in percentuale (0-100)</summary>
        public decimal PercentualePiattaforma { get; set; }

        public decimal QuotaPiattaforma { get; set; }

        /// <summary>importo da bonificare al centro</summary>
        public decimal QuotaCentro { get; set; }

        [Required]
        [MaxLength(3)]
        public string Valuta { get; set; } = "EUR";

        public StatoRendiconto Stato { get; set; }

        public DateTime? DataConferma { get; set; }
        public DateTime? DataLiquidazione { get; set; }

        [MaxLength(100)]
        public string? RiferimentoBonifico { get; set; }

        /// <summary>fattura emessa dal centro alla piattaforma per la sua quota</summary>
        [MaxLength(50)]
        public string? NumeroFatturaCentro { get; set; }

        public DateTime? DataFatturaCentro { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
