using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Az_SubCommessaAttivita : BaseEntity
    {
        [Required]
        public required int IdAz_SubCommessa { get; set; }
        [ForeignKey("IdAz_SubCommessa")]
        public virtual Az_SubCommessa? Az_SubCommessaNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string Descrizione { get; set; } = string.Empty;
    }
}
