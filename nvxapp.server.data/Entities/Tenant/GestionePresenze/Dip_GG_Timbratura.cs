using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Dip_GG_Timbratura : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        [ForeignKey("IdDip_RapportoLavoro")]
        public virtual Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        
        public DateTime Timbratura { get; set; }
        public DateTime TimbraturaOriginale { get; set; }
        public DateTime? TimbraturaArrotondata { get; set; }
        public DateTime GiornoCompetenza { get; set; } // girno al quale viene agganciata la timbratura (servirà per cavallo notte montanti /smontanti)
        public TipoTimbratura TimbraturaTipo { get; set; }

        /* 
          per gli inserimenti diretti, 
                StatoRichiasta = Diretta e
                IdDip_GG_Richiesta = null
         */
        public StatoRichiesta RichiestaStato { get; set; }
        //TODO UNIFORMARE
        public int? IdDip_GG_Richiesta { get; set; }
        [ForeignKey("IdDip_GG_Richiesta")]
        public virtual Dip_GG_Richiesta? Dip_RichiestaNavigation { get; set; }


        
        public int IdAz_SubCommessaAttivita { get; set; }
        [ForeignKey("IdAz_SubCommessaAttivita")]
        public virtual Az_SubCommessaAttivita? Az_SubCommessaAttivitaNavigation { get; set; }

    }


    public enum TipoTimbratura
    {
        Entrata ,
        Uscita ,
        SenzaVerso 
        //Attivita
    }


}
