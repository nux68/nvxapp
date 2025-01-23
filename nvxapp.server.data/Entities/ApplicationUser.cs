using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Entities
{
    public class ApplicationUser : IdentityUser, IDataChangeStatEntity
    {
        public DateTime? ModifiedDate { get; set; }
        public DateTime? CreationDate { get; set; }
        [MaxLength(256)]
        public string? ChangeUser { get; set; }
    }
}
