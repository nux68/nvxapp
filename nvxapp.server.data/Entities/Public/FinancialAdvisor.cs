using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Entities.Public
{
    public class FinancialAdvisor : BaseEntity
    {
        [Required]
        public int IdDealer { get; set; }
        public Dealer? DealerNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }


        public ICollection<Company>? Company { get; set; }
        public ICollection<UserFinancialAdvisor>? UserFinancialAdvisor { get; set; }
    }
}
