using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Passaggio di un ticket (cronologia).
        - RoleId / Level      : livello di visibilita' dopo il passaggio
        - UserId              : assegnatario (null = in attesa al livello, visibile ai PowerAdmin del nodo)
        - IdAspNetUsersAuthor : chi ha eseguito il passaggio e scritto il testo (null = AI)
        - Action / State      : azione eseguita e stato del ticket dopo il passaggio
        - Return*             : dove tornare; valorizzati su RichiestaInfo (chi chiede) e su
                                Risoluzione / RisoluzioneAI (chi ha risolto, o il livello sostituito dall'AI)
        - IdTicketType        : nuovo tipo, sulle righe di Riclassificazione
    */
    public class TicketCronology : BaseEntity
    {
        [Required]
        public int IdTicket { get; set; }
        public Ticket? TicketNavigation { get; set; }

        [Required]
        [MaxLength(450)]
        public string RoleId { get; set; } = string.Empty;

        public TicketLevel Level { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }

        [MaxLength(450)]
        public string? IdAspNetUsersAuthor { get; set; }

        public TicketAction Action { get; set; }

        public TicketState State { get; set; }

        [MaxLength(4000)]
        public string? Text { get; set; }

        [MaxLength(450)]
        public string? ReturnRoleId { get; set; }

        public TicketLevel? ReturnLevel { get; set; }

        [MaxLength(450)]
        public string? ReturnUserId { get; set; }

        public int? IdTicketType { get; set; }

        public ICollection<TicketAttach>? TicketAttach { get; set; }
    }
}
