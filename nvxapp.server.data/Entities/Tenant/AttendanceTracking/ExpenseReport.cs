using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{
    public class ExpenseReport : BaseEntity
    {
        [Required]
        public required string IdAspNetUsers { get; set; }
        public ApplicationUser? AspNetUsersNavigation { get; set; }

        public DateTime Date { get; set; }
    }



}
