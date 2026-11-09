using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Pacchetto di spazio per i file delle aziende (ANALISI_DOCUMENTALE.md [branch DocumentManagement] §5.3): lo spazio di un'azienda e'
     sempre quello del suo pacchetto (niente quote scritte a mano). Uno solo e' il predefinito, assegnato
     all'azienda quando non ne ha uno. Un pacchetto usato da aziende non si elimina: si disattiva.
     Li gestisce solo il SuperUser.
    */
    public class StoragePlan : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Descrizione { get; set; } = string.Empty;

        // spazio del pacchetto in MB
        public int QuotaMB { get; set; }

        // dimensione massima di un file in MB (null = valore predefinito dell'installazione)
        public int? MaxFileMB { get; set; }

        public int Ordine { get; set; }

        public bool Active { get; set; } = true;

        public bool Predefinito { get; set; }
    }
}
