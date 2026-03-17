using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService.Models
{
    public class Dip_GG_ResultModel
    {
        public required int Id { get; set; }
        public required int IdDip_RapportoLavoro { get; set; }
        public string Data { get; set; } = string.Empty;
        public TimeOnly HH_Teo { get; set; }
        public TimeOnly HH_Lav { get; set; }
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
}

      

