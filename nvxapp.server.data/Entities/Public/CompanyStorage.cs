using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Spazio dei file di un'azienda (una riga per azienda), solo per i file del documentale.
        - IdStoragePlan   : pacchetto assegnato (spazio = QuotaMB del pacchetto)
        - MaxFileMB, WarningPercent: null = del pacchetto / predefiniti dell'installazione
        - UsedBytes       : spazio occupato (aggiornato a ogni caricamento ed eliminazione, riallineato di notte)
        - ReservedBytes   : spazio prenotato dai caricamenti in corso (prenotazione atomica, poi conferma o annullo)
        - LastWarningPercent: ultima soglia avvisata (80 o 100), per non ripetere l'avviso finche' lo spazio non scende
    */
    public class CompanyStorage : BaseEntity
    {
        [Required]
        public int IdCompany { get; set; }
        public Company? CompanyNavigation { get; set; }

        [Required]
        public int IdStoragePlan { get; set; }
        public StoragePlan? StoragePlanNavigation { get; set; }

        public int? MaxFileMB { get; set; }

        public int? WarningPercent { get; set; }

        public long UsedBytes { get; set; }

        public long ReservedBytes { get; set; }

        public int FileCount { get; set; }

        public DateTime? LastRecalc { get; set; }

        public int LastWarningPercent { get; set; }

        public DateTime? LastWarningDate { get; set; }
    }
}
