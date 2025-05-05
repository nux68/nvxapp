using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{

    
    public class Az_RepartoUser : BaseEntity
    {
        [Required]
        public required int IdAz_Reparto { get; set; }
        [ForeignKey("IdAz_Reparto")]
        public virtual Az_Reparto? Az_RepartoNavigation { get; set; }

        [Required]
        public required string IdAspNetUsers { get; set; }
        public ApplicationUser? AspNetUsersNavigation { get; set; }
        public Boolean EnabledToApproval { get; set; }



    }



}
