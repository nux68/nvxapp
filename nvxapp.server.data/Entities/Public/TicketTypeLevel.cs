using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Livello in cui un tipo di richiesta e' abilitato: il ticket esiste e si muove solo nei livelli abilitati.
     Una sola riga per tipo/livello.
    */
    public class TicketTypeLevel : BaseEntity
    {
        [Required]
        public int IdTicketType { get; set; }
        public TicketType? TicketTypeNavigation { get; set; }

        [Required]
        public TicketLevel Level { get; set; }
    }
}
