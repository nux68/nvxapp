using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Entities.Public
{
    public class Dealer : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }

        public ICollection<Company>? Company { get; set; }
        public ICollection<UserDealer>? UserDealer { get; set; }
    }
}
