using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Entities.Public
{
    public class UserFinancialAdvisor : BaseEntity
    {

        [Required]
        public int IdFinancialAdvisor { get; set; }
        public FinancialAdvisor? FinancialAdvisorNavigation { get; set; }
        [Required]
        public required string IdAspNetUsers { get; set; }
        public ApplicationUser? AspNetUsersNavigation { get; set; }
        public Boolean MainUser { get; set; }
    }
}
