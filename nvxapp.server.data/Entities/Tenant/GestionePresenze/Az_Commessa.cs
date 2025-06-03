using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Az_Commessa : BaseEntity
    {
        [Required]
        public required int IdAz_Anagrafica { get; set; }
        [ForeignKey("IdAz_Anagrafica")]
        public virtual Az_Anagrafica? Az_AnagraficaNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string Descrizione { get; set; } = string.Empty;

        [Required]
        public required int IdAz_Cliente { get; set; }
        [ForeignKey("IdAz_Cliente")]
        public virtual Az_Cliente? Az_ClienteNavigation { get; set; }

        public Boolean Default { get; set; }
        public DateTime Data { get; set; }
        public DateTime DataA { get; set; }

        public ICollection<Az_SubCommessa>? Az_SubCommessa { get; set; }
    }
}
