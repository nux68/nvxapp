using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models
{
    public class Dip_GG_CausaliModel:HashModel
    {
        [HashField]
        public int Id { get; set; }
        [HashField]
        public int IdDip_RapportoLavoro { get; set; }
        [HashField]
        public DateTime Data { get; set; }
        [HashField]
        public int IdPar_Causali { get; set; }
        [HashField]
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



    public class Dip_GG_CausaliGetInModel 
    {
        public int Id { get; set; }

        /* per inizializzare il record nuovo */
        public required int IdDip_RapportoLavoro { get; set; }


        public DateTime? Data { get; set; }
    }
    public class Dip_GG_CausaliGetOutModel : ModelResult
    {
        public Dip_GG_CausaliModel Dip_GG_Causali { get; set; } = new Dip_GG_CausaliModel();
    }

    public class Dip_GG_CausaliPutInModel 
    {
        public Boolean ExcludeRicalc { get; set; }
        public int IdDip_RapportoLavoro { get; set; }
        public Dip_GG_CausaliModel Dip_GG_Causali { get; set; } = new Dip_GG_CausaliModel();
    }
    public class Dip_GG_CausaliPutOutModel : ModelResult
    {
        public Dip_GG_CausaliModel Dip_GG_Causali { get; set; } = new Dip_GG_CausaliModel();
    }

    public class Dip_GG_Causali_DeleteInModel
    {
        public Boolean ExcludeRicalc { get; set; }
        public int Id { get; set; }
    }
    public class Dip_GG_Causali_DeleteOutModel : ModelResult { }



}
