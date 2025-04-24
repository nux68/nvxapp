using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models
{
    public class Dip_GG_TimbraturaModel
    {
        public int Id { get; set; }

        public int IdDip_RapportoLavoro { get; set; }


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


    }

    public class Dip_GG_TimbraturaInModel
    {

    }

    public class Dip_GG_TimbraturaOutModel : ModelResult 
    {
        public List<Dip_GG_TimbraturaModel>  Dip_GG_Timbratura = new List<Dip_GG_TimbraturaModel>();

        public Dip_GG_TimbraturaOutModel() 
        {
        
        }
    }

   


}
