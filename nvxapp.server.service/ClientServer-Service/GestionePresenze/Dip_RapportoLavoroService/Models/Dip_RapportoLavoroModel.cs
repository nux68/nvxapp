using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models
{
    public class Dip_RapportoLavoroModel
    {
        public int Id { get; set; }
    }

    public class Dip_RapportoLavoro_GetAll_InModel
    {

    }

    public class Dip_RapportoLavoro_GetAll_OutModel : ModelResult 
    {
        public Dip_RapportoLavoro_GetAll_OutModel() 
        {
        
        }
    }

   


}
