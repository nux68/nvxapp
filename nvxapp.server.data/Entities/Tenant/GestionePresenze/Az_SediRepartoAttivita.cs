using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Az_SediRepartoAttivita : BaseEntity
    {
        [Required]
        public required int IdAz_SediReparto { get; set; }
        [ForeignKey("IdAz_SediReparto")]
        public virtual Az_SediReparto? Az_SediRepartoNavigation { get; set; }

        [Required]
        public required int IdPar_Attivita { get; set; }
        [ForeignKey("IdPar_Attivita")]
        public virtual Par_Attivita? Par_AttivitaNavigation { get; set; }
    }
}
