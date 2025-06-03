using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Az_SediAttivita : BaseEntity
    {
        [Required]
        public required int IdAz_Sedi { get; set; }
        [ForeignKey("IdAz_Sedi")]
        public virtual Az_Sedi? Az_SediNavigation { get; set; }

        [Required]
        public required int IdPar_Attivita { get; set; }
        [ForeignKey("IdPar_Attivita")]
        public virtual Par_Attivita? Par_AttivitaNavigation { get; set; }
    }
}
