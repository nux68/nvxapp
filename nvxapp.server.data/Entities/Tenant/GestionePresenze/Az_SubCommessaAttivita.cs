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
        public required int IdAz_SediAttivita { get; set; }
        [ForeignKey("IdAz_SediAttivita")]
        public virtual Az_SediAttivita? Az_SediAttivitaNavigation { get; set; }
        public Boolean Default { get; set; }
    }
}
