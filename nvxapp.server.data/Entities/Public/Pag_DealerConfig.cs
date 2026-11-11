using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Dati del centro per la liquidazione delle sue quote (ANALISI_PAGAMENTI_CENTRI.md §3).
     Una riga per centro, sparisce con il centro.
    */
    public class Pag_DealerConfig : BaseEntity
    {
        public int IdDealer { get; set; }
        public Dealer? DealerNavigation { get; set; }

        /// <summary>trattenuta della piattaforma per questo centro (0-100); null = quella generale (AppSetting Pagamenti_PercentualePiattaforma)</summary>
        public decimal? PercentualePiattaforma { get; set; }

        [MaxLength(34)]
        public string? Iban { get; set; }

        [MaxLength(100)]
        public string? Intestatario { get; set; }

        /// <summary>destinatario dei riepiloghi; vuota = i DealerPowerAdmin del centro</summary>
        [MaxLength(256)]
        public string? EmailAmministrazione { get; set; }
    }
}
