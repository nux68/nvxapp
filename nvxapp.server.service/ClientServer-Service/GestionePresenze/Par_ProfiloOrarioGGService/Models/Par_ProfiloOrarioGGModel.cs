using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService.Models
{
    public class Par_ProfiloOrarioGGModel
    {
        public int Id { get; set; }
        public int IdPar_ProfiloOrario { get; set; }
        public int NumGiorno { get; set; }

        
    }



    public class Par_ProfiloOrarioGGInModel
    {

    }
    public class Par_ProfiloOrarioGGOutModel : ModelResult 
    {
        public Par_ProfiloOrarioGGOutModel() 
        {
        
        }
    }

   

    public class Par_ProfiloOrarioGG_Get_4Edit_InModel
    {
        public int Id { get; set; }  // id del profilo orario
    }
    public class Par_ProfiloOrarioGG_Get_4Edit_OutModel : ModelResult
    {
        public List<Par_ProfiloOrarioGGModel> Par_ProfiloOrarioGG { get; set; } = new List<Par_ProfiloOrarioGGModel>();
    }


    public class Par_ProfiloOrarioGG_Put_4Edit_InModel : ModelResult
    {
        public int Id { get; set; }  // id del profilo orario
        public List<Par_ProfiloOrarioGGModel> Par_ProfiloOrarioGG { get; set; } = new List<Par_ProfiloOrarioGGModel>();
    }
    public class Par_ProfiloOrarioGG_Put_4Edit_OutModel : ModelResult
    {
        public int Id { get; set; }  // id del profilo orario
        public List<Par_ProfiloOrarioGGModel> Par_ProfiloOrarioGG { get; set; } = new List<Par_ProfiloOrarioGGModel>();
    }

}
