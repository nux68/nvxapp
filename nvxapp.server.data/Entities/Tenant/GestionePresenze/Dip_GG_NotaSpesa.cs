using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Dip_GG_NotaSpesa : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        public DateTime Data { get; set; }
        
        
        public StatoRichiesta RichiestaStato { get; set; }
        //TODO UNIFORMARE
        public int? IdDip_GG_Richiesta { get; set; }
        [ForeignKey("IdDip_GG_Richiesta")]
        public virtual Dip_GG_Richiesta? Dip_GG_RichiestaNavigation { get; set; }

    }
}