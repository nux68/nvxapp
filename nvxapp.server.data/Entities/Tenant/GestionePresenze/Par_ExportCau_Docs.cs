using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{
    // Documento che descrive il formato del file da produrre per le paghe (tracciato, manuale, esempio):
    // si manda all'AI insieme alle note per ottenere la ricetta del formato. Il file e' salvato nel database.
    public class Par_ExportCau_Docs : BaseEntity
    {
        [Required]
        public required int IdPar_ExportCau { get; set; }
        [ForeignKey("IdPar_ExportCau")]
        public virtual Par_ExportCau? Par_ExportCauNavigation { get; set; }

        [Required]
        [MaxLength(255)]
        public string NomeFile { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>dimensione del file in byte</summary>
        public int Dimensione { get; set; }

        [Required]
        public byte[] Contenuto { get; set; } = Array.Empty<byte>();

        // testo estratto al caricamento: e' quello che va all'AI e che si mostra per DOCX/ODT/XLSX
        public string? Testo { get; set; }

        [MaxLength(200)]
        public string? Descrizione { get; set; }

        // ordine di invio all'AI
        public int Ordine { get; set; }
    }
}
