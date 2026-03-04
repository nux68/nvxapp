using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService.Models
{
    public class Dip_ProfiloOrarioModel
    {
         public int Id { get; set; }
         public int IdDip_RapportoLavoro { get; set; }
         public int IdPar_ProfiloOrario { get; set; }
         public int NumGiornoPartenzaCiclo { get; set; }
         public DateTime Dal { get; set; }
         public DateTime Al { get; set; }
            
    }

    
    public class Dip_ProfiloOrario_Get_InModel
    {
        public int Id { get; set; }  // = IdDip_RapportoLavoro
    }
    public class Dip_ProfiloOrario_Get_OutModel : ModelResult
    {
        public List<Dip_ProfiloOrarioModel> Dip_ProfiloOrario { get; set; } = new List<Dip_ProfiloOrarioModel>();
    }

    public class Dip_ProfiloOrario_Put_InModel
    {
        public int Id { get; set; } // = IdDip_RapportoLavoro
        public List<Dip_ProfiloOrarioModel> Dip_ProfiloOrario { get; set; } = new List<Dip_ProfiloOrarioModel>();
    }
    public class Dip_ProfiloOrario_Put_OutModel : ModelResult
    {
        public int Id { get; set; } // = IdDip_RapportoLavoro
        public List<Dip_ProfiloOrarioModel> Dip_ProfiloOrario { get; set; } = new List<Dip_ProfiloOrarioModel>();
    }


   


}
