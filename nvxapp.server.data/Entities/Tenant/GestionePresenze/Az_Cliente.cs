using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Az_Cliente : BaseEntity
    {
        [Required]
        public required int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        // ragione sociale
        [Required]
        [MaxLength(100)]
        public string Descrizione { get; set; } = string.Empty;
        public Boolean Default { get; set; }

        // dati fiscali
        [MaxLength(11)]
        public string? PartitaIva { get; set; }
        [MaxLength(16)]
        public string? CodiceFiscale { get; set; }

        // sede
        [MaxLength(100)]
        public string? Indirizzo { get; set; }
        [MaxLength(10)]
        public string? Cap { get; set; }
        [MaxLength(50)]
        public string? Citta { get; set; }
        [MaxLength(2)]
        public string? Provincia { get; set; }
        [MaxLength(2)]
        public string? Nazione { get; set; }

        // contatti
        [MaxLength(30)]
        public string? Telefono { get; set; }
        [MaxLength(100)]
        public string? Email { get; set; }
        [MaxLength(100)]
        public string? Pec { get; set; }
        [MaxLength(100)]
        public string? Referente { get; set; }

        // fatturazione elettronica
        [MaxLength(7)]
        public string? CodiceDestinatario { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
