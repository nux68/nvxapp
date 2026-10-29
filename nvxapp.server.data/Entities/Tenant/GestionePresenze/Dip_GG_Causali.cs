using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Dip_GG_Causali : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        public DateTime Data { get; set; }
        public TimeOnly Valore { get; set; }

        // causale a importo (Par_Causali.Tipo = Importo): euro; Valore resta 00:00
        [Column(TypeName = "numeric(10,2)")]
        public decimal? Importo { get; set; }

        public int IdPar_Causali { get; set; }
        [ForeignKey("IdPar_Causali")]
        public virtual Par_Causali? Par_CausaliNavigation { get; set; }
    }



}
