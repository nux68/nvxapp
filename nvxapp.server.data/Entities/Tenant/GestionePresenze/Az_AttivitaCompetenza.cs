using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Az_AttivitaCompetenza : BaseEntity
    {
        [Required]
        public required int IdAz_Attivita { get; set; }
        [ForeignKey("IdAz_Attivita")]
        public virtual Az_Attivita? Az_AttivitaNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string Descrizione { get; set; } = string.Empty;
    }
}
