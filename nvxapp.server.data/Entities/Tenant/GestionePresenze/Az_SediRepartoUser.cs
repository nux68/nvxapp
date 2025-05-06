using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{

    
    public class Az_SediRepartoUser : BaseEntity
    {
        [Required]
        public required int IdAz_SediReparto { get; set; }
        [ForeignKey("IdAz_SediReparto")]
        public virtual Az_SediReparto? Az_SediRepartoNavigation { get; set; }

        [Required]
        public required string IdAspNetUsers { get; set; }
        public ApplicationUser? AspNetUsersNavigation { get; set; }
        public Boolean EnabledToApproval { get; set; }

        public int ApprovalZOrder { get; set; }

    }



}
