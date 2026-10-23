using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Public
{
    /*
     Allegato di un passaggio del ticket. Il contenuto (File) si legge solo dal download,
     mai negli elenchi o nel dettaglio.
    */
    public class TicketAttach : BaseEntity
    {
        [Required]
        public int IdTicketCronology { get; set; }
        public TicketCronology? TicketCronologyNavigation { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long Size { get; set; }

        [Required]
        public byte[] File { get; set; } = Array.Empty<byte>();
    }
}
