using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Entities.Public
{
    public class Company : BaseEntity
    {
        [Required]
        public int IdFinancialAdvisor { get; set; }
        public FinancialAdvisor? FinancialAdvisorNavigation { get; set; }


        [Required]
        [MaxLength(50)]
        public string? Descrizione { get; set; }


        public ICollection<UserCompany>? UserCompany { get; set; }

        // applicativi attivati per l'azienda
        public ICollection<CompanyApplication>? CompanyApplication { get; set; }
    }
}
