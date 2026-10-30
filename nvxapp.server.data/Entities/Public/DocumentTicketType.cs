using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Argomento di un documento della sezione TicketAi: tipo di richiesta a cui il documento risponde.
     Un documento senza righe vale per tutti i tipi. Una sola riga per documento/tipo.
    */
    public class DocumentTicketType : BaseEntity
    {
        [Required]
        public int IdDocument { get; set; }
        public Document? DocumentNavigation { get; set; }

        [Required]
        public int IdTicketType { get; set; }
        public TicketType? TicketTypeNavigation { get; set; }
    }
}
