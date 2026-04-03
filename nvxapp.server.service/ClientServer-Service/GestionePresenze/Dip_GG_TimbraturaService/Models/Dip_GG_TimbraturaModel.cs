using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models
{
    public class Dip_GG_TimbraturaModel : HashModel
    {
        [HashField]
        public int Id { get; set; }

        [HashField]
        public int IdDip_RapportoLavoro { get; set; }

        [HashField]
        public DateTime Timbratura { get; set; }
        [HashField]
        public DateTime TimbraturaOriginale { get; set; }
        [HashField]
        public DateTime? TimbraturaArrotondata { get; set; }
        [HashField]
        public DateTime GiornoCompetenza { get; set; } // girno al quale viene agganciata la timbratura (servirà per cavallo notte montanti /smontanti)
        [HashField]
        public TipoTimbratura TimbraturaTipo { get; set; }

        /* 
          per gli inserimenti diretti, 
                StatoRichiasta = Diretta e
                IdDip_GG_Richiesta = null
         */
        [HashField]
        public StatoRichiesta RichiestaStato { get; set; }
        [HashField]
        public int? idDip_GG_Richiesta { get; set; }


    }



    public class Dip_GG_Timbratura_GetAll_InModel
    {
        public string? IdAspNetUsers { get; set; }
        public int Year { get; set; } = 0;
        public int Month { get; set; } = 0;
    }
    public class Dip_GG_Timbratura_GetAll_OutModel : ModelResult
    {
        public List<Dip_GG_TimbraturaModel> Dip_GG_Timbratura { get; set; } = new List<Dip_GG_TimbraturaModel>();

    }



    public class Dip_GG_Timbratura_Stamp_InModel
    {
        public string DateStamp { get; set; } = string.Empty;
    }
    public class Dip_GG_Timbratura_Stamp_OutModel : ModelResult
    {

    }

    public class Dip_GG_Timbratura_Get_4Calculation_InModel
    {
        public List<string> UsersId { get; set; } = new List<string>();
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
    }

    public class Dip_GG_Timbratura_Get_4Calculation_OutModel : ModelResult
    {
        public List<Dip_GG_TimbraturaModel> Dip_GG_Timbratura { get; set; } = new List<Dip_GG_TimbraturaModel>();
    }

    public class Dip_GG_TimbraturaGetInModel
    {
        public int Id { get; set; }

        /* per inizializzare il record nuovo */
        public required int IdDip_RapportoLavoro { get; set; }
        public DateTime? Data { get; set; }
    }
    public class Dip_GG_TimbraturaGetOutModel : ModelResult
    {
        public Dip_GG_TimbraturaModel Dip_GG_Timbratura { get; set; } = new Dip_GG_TimbraturaModel();
    }
    
    public class Dip_GG_TimbraturaPutInModel
    {
        public int IdDip_RapportoLavoro { get; set; }
        public Dip_GG_TimbraturaModel Dip_GG_Timbratura { get; set; } = new Dip_GG_TimbraturaModel() { Id = 0, IdDip_RapportoLavoro = 0 };
    }
    public class Dip_GG_TimbraturaPutOutModel : ModelResult
    {
        public Dip_GG_TimbraturaModel Dip_GG_Timbratura { get; set; } = new Dip_GG_TimbraturaModel() { Id = 0, IdDip_RapportoLavoro = 0 };
    }

    public class Dip_GG_Timbratura_DeleteInModel
    {
        public int Id { get; set; }
    }
    public class Dip_GG_Timbratura_DeleteOutModel : ModelResult { }


}
