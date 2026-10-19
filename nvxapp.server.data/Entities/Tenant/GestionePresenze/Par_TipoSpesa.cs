using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    // Tipo di spesa delle note spese (es. vitto, alloggio, trasporti), parametro dell'azienda
    public class Par_TipoSpesa : BaseEntity
    {
        [Required]
        public required int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(10)]
        public string? Codice { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }

        public ICollection<Dip_GG_NotaSpesa>? Dip_GG_NotaSpesa { get; set; }
    }
}
