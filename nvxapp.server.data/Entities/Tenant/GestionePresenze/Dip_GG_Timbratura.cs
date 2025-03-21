using nvxapp.server.data.Entities.Public;
using System.ComponentModel.DataAnnotations;

namespace nvxapp.server.data.Entities.Tenant
{
    public class Dip_GG_Timbratura : BaseEntity
    {
        [Required]
        public required int IdDip_RapportoLavoro { get; set; }
        public Dip_RapportoLavoro? Dip_RapportoLavoroNavigation { get; set; }

        
        public DateTime Timbratura { get; set; }
        public DateTime TimbraturaOrigianle { get; set; }
        public DateTime? TimbraturaArrotondata { get; set; }
        public DateTime GiornoCompetenza { get; set; } // girno al quale viene agganciata la timbratura (servirà per cavallo notte montanti /smontanti)
        public TipoTimbratura TimbraturaTipo { get; set; }

        /* 
          per gli inserimenti diretti, 
                StatoRichiasta = Diretta e
                IdDip_Richiesta = null
         */
        public StatoRichiesta RichiestaStato { get; set; }
        public int? IdDip_Richiesta { get; set; }
        public Dip_GG_Richiesta? Dip_RichiestaNavigation { get; set; }
    }


    public enum TipoTimbratura
    {
        Entrata ,
        Uscita ,
        SenzaVerso ,
        Attivita
    }


}
