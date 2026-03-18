using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models
{
    public class Dip_GG_CausaliModel
    {
        public int Id { get; set; }
        public int IdDip_RapportoLavoro { get; set; }
        public DateTime Data { get; set; }
        public int IdPar_Causali { get; set; }
        public TimeOnly Valore { get; set; }
    }

    public class Dip_GG_Causali_GetAll_InModel
    {

    }

    public class Dip_GG_Causali_GetAll_OutModel : ModelResult 
    {
        public List<Dip_GG_CausaliModel> Dip_GG_Causali { get; set; } = new List<Dip_GG_CausaliModel>();
    }


    public class Dip_GG_Causali_Get_4Calculation_InModel
    {
        public List<string> UsersId { get; set; } = new List<string>();
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
    }

    public class Dip_GG_Causali_Get_4Calculation_OutModel : ModelResult
    {
        public List<Dip_GG_CausaliModel> Dip_GG_Causali { get; set; } = new List<Dip_GG_CausaliModel>();
    }


    public class Dip_GG_Causali_DeleteInModel
    {
        public int Id { get; set; }
    }

    public class Dip_GG_Causali_DeleteOutModel : ModelResult { }


    public class Dip_GG_CausaliPutInModel 
    {
        public Dip_GG_CausaliModel Dip_GG_Causali { get; set; } = new Dip_GG_CausaliModel();
    }
    public class Dip_GG_CausaliPutOutModel : ModelResult
    {
        public Dip_GG_CausaliModel Dip_GG_Causali { get; set; } = new Dip_GG_CausaliModel();
    }
}
