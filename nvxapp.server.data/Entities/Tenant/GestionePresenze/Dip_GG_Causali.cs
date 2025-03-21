using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Dip_GG_Causali : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        public Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        public DateTime Data { get; set; }


        public int IdPar_Causali { get; set; }
        public Par_Causali? Par_CausaliNavigation { get; set; }
    }



}
