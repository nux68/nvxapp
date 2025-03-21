using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Dip_GG_Giustificativi : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        public Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        public DateTime Data { get; set; }
        

        public int IdJustificationType { get; set; }

        public JustificationInputType InputType { get; set; }

        public TimeSpan? Hours { get; set; }  // se InputType=manual

        public TimeSpan? From { get; set; }   // se InputType=manual  (dalle)


        public required int IdPar_Giustificativi { get; set; }
        public Par_Giustificativi? Par_GiustificativiNavigation { get; set; }



        public StatoRichiesta Stato { get; set; }
        public int? IdDip_Richiesta { get; set; }
        public Dip_GG_Richiesta? Dip_RichiestaNavigation { get; set; }

    }

    public enum JustificationInputType
    {
        Manual,
        AllDay,
        IntegrateDay
    }

}
