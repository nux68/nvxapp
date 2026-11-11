using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    // Profilo formule del rapporto di lavoro, storicizzato per periodo (come Dip_ProfiloOrario)
    public class Dip_ProfiloFormule : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        [Required]
        public required int IdPar_ProfiloFormule { get; set; }
        [ForeignKey("IdPar_ProfiloFormule")]
        public virtual Par_ProfiloFormule? Par_ProfiloFormuleNavigation { get; set; }

        [Column(TypeName = "date")]
        public DateTime Dal { get; set; }
        [Column(TypeName = "date")]
        public DateTime Al { get; set; }
    }
}
