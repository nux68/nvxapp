using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Risposta automatica AI di un tipo di richiesta a un livello (sostituisce TicketType.AiResolution).
        - Level                                   : livello che riceve il ticket (Dev, Dealer, FinancialAdvisor, Company)
        - IdDealer / IdFinancialAdvisor / IdCompany: nodo che personalizza; tutti null = valore di default del
                                                    livello, deciso dal SuperUser
        - AiEnabled                               : all'arrivo del ticket al livello si tenta la risposta AI
        - AiMaxAttempts                           : tentativi AI al massimo a quel livello, poi l'operatore
     Impostazione effettiva: riga del nodo, altrimenti default del livello, altrimenti AI disattivata.
    */
    public class TicketTypeAiSetting : BaseEntity
    {
        [Required]
        public int IdTicketType { get; set; }
        public TicketType? TicketTypeNavigation { get; set; }

        public TicketLevel Level { get; set; }

        public int? IdDealer { get; set; }
        public int? IdFinancialAdvisor { get; set; }
        public int? IdCompany { get; set; }

        public bool AiEnabled { get; set; }

        public int AiMaxAttempts { get; set; }
    }
}
