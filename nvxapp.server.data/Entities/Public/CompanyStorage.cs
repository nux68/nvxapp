using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Spazio dei file di un'azienda (una riga per azienda), solo per i file del documentale.
     La quota non sta qui: e' quella della licenza Documentale (ILicenseGuard.StorageQuotaAsync, ANALISI_INFRASTRUCTURE_LICENZE.md §5.1);
     senza licenza (o con il sistema licenze spento) lo spazio e' illimitato. I pacchetti di spazio (StoragePlan) sono stati eliminati.
        - WarningPercent  : soglia di avviso propria dell'azienda (null = predefinita dell'installazione)
        - UsedBytes       : spazio occupato (aggiornato a ogni caricamento ed eliminazione, riallineato di notte)
        - ReservedBytes   : spazio prenotato dai caricamenti in corso (prenotazione atomica, poi conferma o annullo)
        - LastWarningPercent: ultima soglia avvisata (80 o 100), per non ripetere l'avviso finche' lo spazio non scende
    */
    public class CompanyStorage : BaseEntity
    {
        [Required]
        public int IdCompany { get; set; }
        public Company? CompanyNavigation { get; set; }

        public int? WarningPercent { get; set; }

        public long UsedBytes { get; set; }

        public long ReservedBytes { get; set; }

        public int FileCount { get; set; }

        public DateTime? LastRecalc { get; set; }

        public int LastWarningPercent { get; set; }

        public DateTime? LastWarningDate { get; set; }
    }
}
