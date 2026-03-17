using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Dip_GG_Result : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }
        
        public DateTime Data { get; set; } 
        public TimeOnly HH_Teo { get; set; }
        public TimeOnly HH_Lav { get; set; }
        public GG_ResultStato Stato { get; set; }
    }


    public enum GG_ResultStato
    {
        Init,
        Err,
        OK,
        Locked

    }


}
