using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    // Documento allegato a una spesa della nota spese (scontrino, fattura, ricevuta...): il file e' salvato nel database
    public class Dip_GG_NotaSpesa_Allegato : BaseEntity
    {
        [Required]
        public required int IdDip_GG_NotaSpesa { get; set; }
        [ForeignKey("IdDip_GG_NotaSpesa")]
        public virtual Dip_GG_NotaSpesa? Dip_GG_NotaSpesaNavigation { get; set; }

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
    }
}
