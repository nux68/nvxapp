using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Justification : BaseEntity
    {
        [Required]
        public required string IdAspNetUsers { get; set; }
        public ApplicationUser? AspNetUsersNavigation { get; set; }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public int IdJustificationType { get; set; }
        public JustificationType? JustificationTypeNavigation { get; set; }

    }



}
