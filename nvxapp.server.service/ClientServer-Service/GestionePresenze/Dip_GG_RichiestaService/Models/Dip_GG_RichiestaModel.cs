using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models
{




    public class Dip_GG_RichiestaModel : HashModel
    {
        public required int Id { get; set; }
        public required int IdDip_RapportoLavoro { get; set; }

        [HashField]
        public string Data { get; set; } = string.Empty;
        [HashField]
        public string DataA { get; set; } = string.Empty;

        [HashField]
        public TipoRichiesta RichiestaTipo { get; set; }
        // Campo per oggetto JSON
        [HashField]
        public required string Dati { get; set; } = string.Empty;

        [HashField]
        public StatoRichiesta RichiestaStato { get; set; }
        [HashField]
        public List<Dip_GG_Richiesta_Stato_Cronology>? RichiestaApprovazioneData { get; set; } = new List<Dip_GG_Richiesta_Stato_Cronology>();
        [HashField]
        public StatoRichiesta? RevocaStato { get; set; }
        [HashField]
        public List<Dip_GG_Richiesta_Stato_Cronology>? RevocaApprovazioneData { get; set; } = new List<Dip_GG_Richiesta_Stato_Cronology>();

        // Campo per oggetto JSON
        [HashField]
        public string? CronologiaApprovazione { get; set; }

    }

    public class Dip_GG_Richiesta_Body_Timbratura
    {
        public required string hhmm { get; set; } = string.Empty;
    }
    public class Dip_GG_Richiesta_Body_Giustificativo
    {
        public required string hhmm { get; set; } = string.Empty;
        public required bool AllDay { get; set; } = false;
        public required int IdPar_Giustificativi { get; set; } = 0;
    }
    public class Dip_GG_Richiesta_Body_NotaSpesa
    {

    }


    public class Dip_GG_Richiesta_GetAll4User_InModel
    {
        public string? IdAspNetUsers { get; set; }
        public int Year { get; set; } = 0;
        public int Month { get; set; } = 0;
    }
    public class Dip_GG_Richiesta_GetAll4User_OutModel : ModelResult
    {
        public List<Dip_GG_RichiestaModel> Dip_GG_Richiesta { get; set; } = new List<Dip_GG_RichiestaModel>();
    }


    public class Dip_GG_Richiesta_GetAll4Admin_InModel
    {
        public int Year { get; set; } = 0;
        public int Month { get; set; } = 0;
    }
    public class Dip_GG_Richiesta_GetAll4Admin_OutModel : ModelResult
    {
        public List<Dip_GG_RichiestaModel> Dip_GG_Richiesta { get; set; } = new List<Dip_GG_RichiestaModel>();
    }




    public class Dip_GG_Richiesta_Send_InModel
    {
        public string? IdAspNetUsers { get; set; }
        public Dip_GG_RichiestaModel? Dip_GG_Richiesta { get; set; }
        public Boolean FromHR { get; set; }
    }
    public class Dip_GG_Richiesta_Send_OutModel : ModelResult
    {
        public Dip_GG_Richiesta_Send_OutModel()
        {
            Dip_GG_Richiesta = new List<Dip_GG_RichiestaModel>();
            Dip_GG_Timbratura = new List<Dip_GG_TimbraturaModel>();
            Dip_GG_Giustificativi = new List<Dip_GG_GiustificativiModel>();
        }

        public List<Dip_GG_RichiestaModel> Dip_GG_Richiesta { get; set; } = new List<Dip_GG_RichiestaModel>();
        public List<Dip_GG_TimbraturaModel> Dip_GG_Timbratura { get; set; } = new List<Dip_GG_TimbraturaModel>();
        public List<Dip_GG_GiustificativiModel> Dip_GG_Giustificativi { get; set; } = new List<Dip_GG_GiustificativiModel>();
    }


    public class Dip_GG_Richiesta_SetState_InModel
    {
        public List<int> IdDip_GG_Richiesta { get; set; } = new List<int>();

        public StatoRichiesta RichiestaStato { get; set; }
        public Boolean FromHR { get; set; }

    }
    public class Dip_GG_Richiesta_SetState_OutModel : ModelResult
    {
        // dati modificati
        public List<Dip_GG_RichiestaModel> Dip_GG_Richiesta { get; set; } = new List<Dip_GG_RichiestaModel>();
        public List<Dip_GG_TimbraturaModel> Dip_GG_Timbratura { get; set; } = new List<Dip_GG_TimbraturaModel>();
        public List<Dip_GG_GiustificativiModel> Dip_GG_Giustificativi { get; set; } = new List<Dip_GG_GiustificativiModel>();

    }

    public class Dip_GG_Richiesta_Get_4Calculation_InModel
    {
        public List<string> UsersId { get; set; } = new List<string>();
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
    }

    public class Dip_GG_Richiesta_Get_4Calculation_OutModel : ModelResult
    {
        public List<Dip_GG_RichiestaModel> Dip_GG_Richiesta { get; set; } = new List<Dip_GG_RichiestaModel>();
    }

}
