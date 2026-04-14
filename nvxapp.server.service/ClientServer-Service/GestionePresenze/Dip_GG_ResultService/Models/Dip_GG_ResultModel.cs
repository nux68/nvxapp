using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService.Models
{
    public class Dip_GG_ResultModel:HashModel
    {
        public required int Id { get; set; }
        public required int IdDip_RapportoLavoro { get; set; }
        public DateTime Data { get; set; } 
        [HashField]
        public TimeOnly HH_Teo { get; set; }
        [HashField]
        public TimeOnly HH_Lav { get; set; }
        [HashField]
        public GG_ResultStato Stato { get; set; }
    }



    public class Dip_GG_Result_GetAll_InModel
    {
        public string? IdAspNetUsers { get; set; }
        public int Year { get; set; } = 0;
        public int Month { get; set; } = 0;
    }
    public class Dip_GG_Result_GetAll_OutModel : ModelResult
    {
        public List<Dip_GG_ResultModel> Dip_GG_Result { get; set; } = new List<Dip_GG_ResultModel>();
    }


    public class Dip_GG_ResultPutInModel 
    {
        public Boolean ExcludeRicalc { get; set; }
        public required int IdDip_RapportoLavoro { get; set; }
        public Dip_GG_ResultModel Dip_GG_Result { get; set; }   = new Dip_GG_ResultModel(){  Id=0, IdDip_RapportoLavoro=0};
    }
    public class Dip_GG_ResultPutOutModel : ModelResult
    {
        public Dip_GG_ResultModel Dip_GG_Result { get; set; } = new Dip_GG_ResultModel(){  Id=0, IdDip_RapportoLavoro=0};
    }

  

    public class Dip_GG_Result_Get_4Calculation_InModel
    {
        public List<string> UsersId { get; set; } = new List<string>();
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
    }
    public class Dip_GG_Result_Get_4Calculation_OutModel : ModelResult
    {
        public List<Dip_GG_ResultModel> Dip_GG_Result { get; set; } = new List<Dip_GG_ResultModel>();
    }


    public class Dip_GG_Result_Init_InModel
    {
        public Boolean ExcludeRicalc { get; set; }
        public required int IdDip_RapportoLavoro { get; set; }
        public List<DateTime> Date { get; set; } = new List<DateTime>();
    }
    public class Dip_GG_Result_Init_OutModel : ModelResult
    {
        public List<Dip_GG_Result> Dip_GG_Result { get; set; } = new List<Dip_GG_Result>();
    }

}

      

