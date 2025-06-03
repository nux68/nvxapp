using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Az_SubCommessa : BaseEntity
    {
        [Required]
        public required int IdAz_Commessa { get; set; }
        [ForeignKey("IdAz_Commessa")]
        public virtual Az_Commessa? Az_CommessaNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string Descrizione { get; set; } = string.Empty;
        public Boolean Default { get; set; }

        public ICollection<Az_SubCommessaAttivita>? Az_SubCommessaAttivita { get; set; }
    }
}
