using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models
{
    public class Par_OrarioIntervalloHHModel
    {

        public int Id { get; set; }
        public int IdPar_Orario { get; set; }
        public TimeOnly? Dalle { get; set; } 
        public TimeOnly? Alle { get; set; }
        public TimeOnly? Dalle_Limite_SX { get; set; }
        public TimeOnly? Dalle_Limite_DX { get; set; }
        public TimeOnly? Alle_Limite_SX { get; set; }
        public TimeOnly? Alle_Limite_DX { get; set; }
        public int NumCoppia { get; set; }  = 0; 

    }

    public class Par_OrarioIntervalloHHInModel
    {

    }

    public class Par_OrarioIntervalloHHOutModel : ModelResult 
    {
        public Par_OrarioIntervalloHHOutModel() 
        {
        
        }
    }

   

    public class Par_OrarioIntervalloHH_GetAll_4Edit_InModel
    {
        public int Id { get; set; }  // id del orario
    }
    public class Par_OrarioIntervalloHH_GetAll_4Edit_OutModel
    {
        public List<Par_OrarioIntervalloHHModel> Par_OrarioIntervalloHH { get; set; } = new List<Par_OrarioIntervalloHHModel>();
    }

    public class Par_OrarioIntervalloHH_PutAll_4Edit_InModel
    {
        public int Id { get; set; }  // id del orario
        public List<Par_OrarioIntervalloHHModel> Par_OrarioIntervalloHH { get; set; } = new List<Par_OrarioIntervalloHHModel>();
    }
    public class Par_OrarioIntervalloHH_PutAll_4Edit_OutModel
    {
        public int Id { get; set; }  // id del orario
        public List<Par_OrarioIntervalloHHModel> Par_OrarioIntervalloHH { get; set; } = new List<Par_OrarioIntervalloHHModel>();
    }

}
