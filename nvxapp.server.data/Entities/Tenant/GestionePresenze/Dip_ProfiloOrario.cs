using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Dip_ProfiloOrario : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        
        public int? IdPar_ProfiloOrario { get; set; }
        [ForeignKey("IdPar_ProfiloOrario")]
        public virtual Par_ProfiloOrario? Par_ProfiloOrarioNavigation { get; set; }

        public int NumGiornoPartenzaCiclo { get; set; }

    }


  


}
