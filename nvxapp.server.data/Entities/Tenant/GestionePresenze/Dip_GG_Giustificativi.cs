using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Dip_GG_Giustificativi : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        public DateTime Data { get; set; }
        

        public int IdJustificationType { get; set; }

        public JustificationInputType InputType { get; set; }

        public TimeSpan? Hours { get; set; }  // se InputType=manual

        public TimeSpan? From { get; set; }   // se InputType=manual  (dalle)


        public required int IdPar_Giustificativi { get; set; }
        [ForeignKey("IdPar_Giustificativi")]
        public virtual Par_Giustificativi? Par_GiustificativiNavigation { get; set; }



        public StatoRichiesta RichiestaStato { get; set; }
        public int? IdDip_Richiesta { get; set; }
        [ForeignKey("IdDip_Richiesta")]
        public virtual Dip_GG_Richiesta? Dip_RichiestaNavigation { get; set; }

    }

    public enum JustificationInputType
    {
        Manual,
        AllDay,
        IntegrateDay
    }

}
