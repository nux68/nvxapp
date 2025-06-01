using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Par_AttivitaCompetenza : BaseEntity
    {
        [Required]
        public required int IdPar_Attivita { get; set; }
        [ForeignKey("IdPar_Attivita")]
        public virtual Par_Attivita? Par_AttivitaNavigation { get; set; }

        [Required]
        [MaxLength(50)]
        public string Descrizione { get; set; } = string.Empty;
    }
}
