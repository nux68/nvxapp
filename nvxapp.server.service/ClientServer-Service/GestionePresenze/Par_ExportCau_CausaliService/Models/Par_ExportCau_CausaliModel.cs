using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_CausaliService.Models
{
    public class Par_ExportCau_CausaliModel
    {
        public int Id { get; set; }
        public int IdPar_ExportCau { get; set; }
        public int IdCausale { get; set; }
        public string? Codice { get; set; }
        public Par_Export_TipoElaborazione TipoElaborazione { get; set; }
        public Par_Export_TipoUnita TipoUnita { get; set; }     
    }

    public class Par_ExportCau_Causali_GetAll_InModel
    {
        public int IdPar_ExportCau { get; set; }
    }
    public class Par_ExportCau_Causali_GetAll_OutModel : ModelResult
    {
        public List<Par_ExportCau_CausaliModel> Par_ExportCau_Causali { get; set; } = new();
    }

    public class Par_ExportCau_Causali_Get_InModel
    {
        public int Id { get; set; }
    }
    public class Par_ExportCau_Causali_Get_OutModel : ModelResult
    {
        public List<Par_ExportCau_CausaliModel> Par_ExportCau_Causali { get; set; } = new();
    }

    public class Par_ExportCau_Causali_Put_InModel
    {
        public int IdPar_ExportCau { get; set; }
        public List<Par_ExportCau_CausaliModel> Par_ExportCau_Causali { get; set; } = new();
    }
    public class Par_ExportCau_Causali_Put_OutModel : ModelResult
    {
        public List<Par_ExportCau_CausaliModel> Par_ExportCau_Causali { get; set; } = new();
    }

    public class Par_ExportCau_Causali_Delete_InModel
    {
        public int Id { get; set; }
    }
    public class Par_ExportCau_Causali_Delete_OutModel : ModelResult { }
}
