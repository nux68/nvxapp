using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant.GestionePresenze
{
    public class Az_SubCommessaUser : BaseEntity
    {
        [Required]
        public int IdAz_SubCommessa { get; set; }
        [ForeignKey("IdAz_SubCommessa")]
        public virtual Az_SubCommessa? Az_SubCommessaNavigation { get; set; }

        [Required]
        public string IdAspNetUsers { get; set; } = string.Empty;
        [ForeignKey("IdAspNetUsers")]
        public ApplicationUser? AspNetUsersNavigation { get; set; }

        
    }
}