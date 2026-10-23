using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Ticket di assistenza (la richiesta). La progressione e' in TicketCronology.
        - IdAspNetUsersOpener / OpenerRoleId / OpenerLevel: chi ha aperto e con quale ruolo (scelto in apertura)
        - IdDealer, IdFinancialAdvisor, IdCompany: percorso di origine, valorizzato fino al livello di chi apre
          (vuoto per i ticket del SuperUser); individua il nodo dei livelli e filtra i ticket dei figli
        - Current*: copia dell'ultima riga della cronologia (livello e assegnatario correnti)
        - Version : cresce a ogni passaggio, per rifiutare le azioni fatte su una versione superata
     Nessuna relazione verso Company, Dealer, FinancialAdvisor o AspNetUsers: i contesti degli applicativi
     mappano Company e scoprirebbero la tabella; la cronologia resta anche se l'azienda o l'utente spariscono.
    */
    public class Ticket : BaseEntity
    {
        [Required]
        public int IdTicketType { get; set; }
        public TicketType? TicketTypeNavigation { get; set; }

        [Required]
        [MaxLength(200)]
        public string Oggetto { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Text { get; set; } = string.Empty;

        public TicketState State { get; set; }

        [Required]
        [MaxLength(450)]
        public string IdAspNetUsersOpener { get; set; } = string.Empty;

        [Required]
        [MaxLength(450)]
        public string OpenerRoleId { get; set; } = string.Empty;

        public TicketLevel OpenerLevel { get; set; }

        public int? IdDealer { get; set; }
        public int? IdFinancialAdvisor { get; set; }
        public int? IdCompany { get; set; }

        [Required]
        [MaxLength(450)]
        public string CurrentRoleId { get; set; } = string.Empty;

        public TicketLevel CurrentLevel { get; set; }

        [MaxLength(450)]
        public string? CurrentIdAspNetUsers { get; set; }

        public int Version { get; set; }

        public ICollection<TicketCronology>? TicketCronology { get; set; }
    }
}
