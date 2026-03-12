using nvxapp.server.data.Extensions;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models
{
    public class TimeSheet_CalculateModel
    {

        public int Year { get; set; }
        public int Month { get; set; } = 0;
        public List<string> SelectedUserId { get; set; } = new List<string>();
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        
        public Boolean Approva_Richieste_Timbrature { get; set; }
        public Boolean Approva_Richieste_Giustificativo { get; set; }
        public Boolean Genera_Timbrature_Mancanti { get; set; }

        

    }

    public class TimeSheet_CalculateInModel
    {
        public TimeSheet_CalculateModel TimeSheet_Calculate { get; set; } = new TimeSheet_CalculateModel();
    }

    public class TimeSheet_CalculateOutModel : ModelResult
    {
        public TimeSheet_CalculateModel TimeSheet_Calculate { get; set; } = new TimeSheet_CalculateModel();

        public TimeSheet_CalculateOutModel()
        {

        }
    }




    
    public class TimeSheet_Calculate_GetAllData_InModel
    {
        public List<string> UsersId { get; set; } = new List<string>();
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
    }

    // Una riga orario del giorno, corrispondente a una riga Par_ProfiloOrarioGG.
    // ZOrder 1 = orario base (sempre presente), ZOrder > 1 = override condizionale.
    public class Dip_ProfiloOrario_DaySlot_GG
    {
        public int ZOrder      { get; set; } // ordinamento: 1 = default, 2+ = override
        public int IdPar_Orario { get; set; }
    }

    // Un record per ogni combinazione dipendente × giorno con tutte le righe orario ordinate per ZOrder
    public class Dip_ProfiloOrario_DaySlot
    {
        public string   IdAspNetUsers        { get; set; } = string.Empty;
        public int      IdDip_RapportoLavoro { get; set; }
        public DateTime Data                 { get; set; }
        public int      IdPar_ProfiloOrario  { get; set; }
        // righe orario del giorno ordinate per ZOrder (1=base, 2+=override)
        public List<Dip_ProfiloOrario_DaySlot_GG> Orari { get; set; } = new List<Dip_ProfiloOrario_DaySlot_GG>();
    }

    public class TimeSheet_All_Data_Container : ModelResult
    {
        public List<Dip_AnagraficaModel>           Dip_Anagrafica         { get; set; } = new List<Dip_AnagraficaModel>();
        public List<Dip_RapportoLavoroModel>       Dip_RapportoLavoro     { get; set; } = new List<Dip_RapportoLavoroModel>();
        public List<Dip_ProfiloOrario_DaySlot>     DaySlots               { get; set; } = new List<Dip_ProfiloOrario_DaySlot>();
        public List<Par_ProfiloOrarioModel>        Par_ProfiloOrario      { get; set; } = new List<Par_ProfiloOrarioModel>();
        public List<Par_ProfiloOrarioGGModel>      Par_ProfiloOrarioGG    { get; set; } = new List<Par_ProfiloOrarioGGModel>();
        public List<Par_OrarioModel>               ParOrario              { get; set; }= new List<Par_OrarioModel>();
        public List<Par_OrarioIntervalloHHModel>   Par_OrarioIntervalloHH { get; set; }= new List<Par_OrarioIntervalloHHModel>();
    }

    public class AllData
    {
        public TimeSheet_All_Data_Container Dip_ProfiloOrario_Calculate { get; set; } = new TimeSheet_All_Data_Container();
        public List<Dip_GG_TimbraturaModel> Dip_GG_Timbratura { get; set; } = new List<Dip_GG_TimbraturaModel>();
        public List<Dip_GG_CausaliModel> Dip_GG_Causali { get; set; } = new List<Dip_GG_CausaliModel>();
        public List<Dip_GG_GiustificativiModel> Dip_GG_Giustificativi { get; set; } = new List<Dip_GG_GiustificativiModel>();
        public List<Dip_GG_RichiestaModel> Dip_GG_Richiesta { get; set; } = new List<Dip_GG_RichiestaModel>();
    }

}
