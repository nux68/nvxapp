using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models
{
    

   

    public class Dip_GG_RichiestaModel
    {
        public required int Id { get; set; }
        public required int IdDip_RapportoLavoro { get; set; }
        
        public string Data { get; set; } = string.Empty;
        public string DataA { get; set; } = string.Empty;

        public TipoRichiesta RichiestaTipo { get; set; }
        // Campo per oggetto JSON
        public required string Dati { get; set; } = string.Empty;

        public StatoRichiesta RichiestaStato { get; set; }
        public List<Dip_GG_Richiesta_Stato_Cronology>? RichiestaApprovazioneData { get; set; } = new List<Dip_GG_Richiesta_Stato_Cronology>();

        public StatoRichiesta? RevocaStato { get; set; }
        public List<Dip_GG_Richiesta_Stato_Cronology>? RevocaApprovazioneData { get; set; } = new List<Dip_GG_Richiesta_Stato_Cronology>();

        // Campo per oggetto JSON
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
        public Dip_GG_RichiestaModel? Dip_GG_Richiesta { get; set; }
    }


    public class Dip_GG_Richiesta_SetState_InModel
    {
        public List<int> IdDip_GG_Richiesta { get; set; } = new List<int>();

        public StatoRichiesta RichiestaStato { get; set; }
        public Boolean FromHR { get; set; }

    }
    public class Dip_GG_Richiesta_SetState_OutModel : ModelResult
    {
        
    }

}
