using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Dip_GG_NotaSpesa : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        public Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        public DateTime Data { get; set; }
        
        
        public StatoRichiesta RichiestaStato { get; set; }
        public int? IdDip_GG_Richiesta { get; set; }
        public Dip_GG_Richiesta? Dip_GG_RichiestaNavigation { get; set; }

    }
}