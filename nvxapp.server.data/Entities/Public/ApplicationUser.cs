using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Entities.Public
{
    public class ApplicationUser : IdentityUser<string>, IDataChangeStatEntity
    {
        public DateTime? ModifiedDate { get; set; }
        public DateTime? CreationDate { get; set; }
        [MaxLength(256)]
        public string? ChangeUser { get; set; }

        public ICollection<UserDealer>? UserDealer { get; set; }
        public ICollection<UserCompany>? UserCompany { get; set; }
        public ICollection<UserFinancialAdvisor>? UserFinancialAdvisor { get; set; }

        
    }





}
