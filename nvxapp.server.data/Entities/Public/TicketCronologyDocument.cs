using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Documento usato dall'AI per una risposta (righe RisoluzioneAI e RisoluzioneAINonTrovata della cronologia).
     Il titolo e' copiato: resta leggibile anche se il documento viene eliminato (IdDocument diventa null).
    */
    public class TicketCronologyDocument : BaseEntity
    {
        [Required]
        public int IdTicketCronology { get; set; }
        public TicketCronology? TicketCronologyNavigation { get; set; }

        public int? IdDocument { get; set; }
        public Document? DocumentNavigation { get; set; }

        [Required]
        [MaxLength(200)]
        public string Titolo { get; set; } = string.Empty;
    }
}
