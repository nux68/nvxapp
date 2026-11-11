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

        // il centro puo' definire i propri listini delle licenze (ANALISI_INFRASTRUCTURE_LICENZE.md §3.3); lo abilita il SuperUser
        public bool PuoGestireListini { get; set; }

        public ICollection<FinancialAdvisor>? FinancialAdvisor { get; set; }
        public ICollection<UserDealer>? UserDealer { get; set; }
    }
}
