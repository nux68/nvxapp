using nvxapp.server.data.Extensions;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models
{
    public class Par_OrarioIntervalloHHModel
    {

        public int Id { get; set; }
        public int IdPar_Orario { get; set; }
        //public int IdAz_SubCommessaAttivita { get; set; }
        public TimeOnly? Dalle { get; set; }
        public TimeOnly? Dalle_Limite_SX { get; set; }
        public TimeOnly? Dalle_Limite_DX { get; set; }
        public TimeRoundInterval Dalle_Arrotondamento { get; set; }
        public RoundDirection Dalle_Arrotondamento_Verso { get; set; }
        public Boolean Dalle_Use_4_Match { get; set; }

        public TimeOnly? Alle { get; set; }
        public TimeOnly? Alle_Limite_SX { get; set; }
        public TimeOnly? Alle_Limite_DX { get; set; }
        public TimeRoundInterval Alle_Arrotondamento { get; set; }
        public RoundDirection Alle_Arrotondamento_Verso { get; set; }
        public Boolean Alle_Use_4_Match { get; set; }

        public int NumCoppia { get; set; } = 0;
        public int IdCausale_HH_Lav { get; set; } = 0;

    }

    public class Par_OrarioIntervalloHHInModel
    {

    }

    public class Par_OrarioIntervalloHHOutModel : ModelResult
    {
        public List<Par_OrarioIntervalloHHModel> Par_OrarioIntervalloHH { get; set; } = new List<Par_OrarioIntervalloHHModel>();

        public Par_OrarioIntervalloHHOutModel()
        {

        }
    }



    public class Par_OrarioIntervalloHH_Get_4Edit_InModel
    {
        public int Id { get; set; }  // id del orario
    }
    public class Par_OrarioIntervalloHH_Get_4Edit_OutModel
    {
        public List<Par_OrarioIntervalloHHModel> Par_OrarioIntervalloHH { get; set; } = new List<Par_OrarioIntervalloHHModel>();
    }

    public class Par_OrarioIntervalloHH_Put_4Edit_InModel
    {
        public int Id { get; set; }  // id del orario
        public List<Par_OrarioIntervalloHHModel> Par_OrarioIntervalloHH { get; set; } = new List<Par_OrarioIntervalloHHModel>();
    }
    public class Par_OrarioIntervalloHH_Put_4Edit_OutModel
    {
        public int Id { get; set; }  // id del orario
        public List<Par_OrarioIntervalloHHModel> Par_OrarioIntervalloHH { get; set; } = new List<Par_OrarioIntervalloHHModel>();
    }


    public class Par_OrarioIntervalloHH_Arrange_Coppie_InModel
    {
        public int Id { get; set; }  // id del orario
        public int NumCoppie { get; set; }  
        public List<Par_OrarioIntervalloHHModel> Par_OrarioIntervalloHH { get; set; } = new List<Par_OrarioIntervalloHHModel>();
    }
    public class Par_OrarioIntervalloHH_Arrange_Coppie_OutModel
    {
        public int Id { get; set; }  // id del orario
        public List<Par_OrarioIntervalloHHModel> Par_OrarioIntervalloHH { get; set; } = new List<Par_OrarioIntervalloHHModel>();
    }

}
