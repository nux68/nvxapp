using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CfgService.Models
{
    public class Az_CfgModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public TipoApprovazione ApprovazioneTipo { get; set; }

    }

    public class Az_Cfg_GetAll_InModel
    {

    }
    public class Az_Cfg_GetAll_OutModel : ModelResult 
    {
        public Az_Cfg_GetAll_OutModel() 
        {
        
        }
    }


    public class Az_Cfg_Get_InModel
    {
        public  Az_CfgModel? Az_Cfg {  get; set; }
    }
    public class Az_Cfg_Get_OutModel : ModelResult
    {
        public Az_CfgModel? Az_Cfg { get; set; }
    }
    public class Az_Cfg_Put_InModel
    {
        public Az_CfgModel? Az_Cfg { get; set; }
    }
    public class Az_Cfg_Put_OutModel : ModelResult
    {
        public Az_CfgModel? Az_Cfg { get; set; }
    }

}
