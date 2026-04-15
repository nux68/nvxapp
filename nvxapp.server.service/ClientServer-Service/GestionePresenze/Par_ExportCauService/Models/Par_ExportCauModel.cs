using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCauService.Models
{
    public class Par_ExportCauModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string Codice { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public Par_Export_TipoFile TipoFile { get; set; }
    }

    public class Par_ExportCau_GetAll_InModel { }
    public class Par_ExportCau_GetAll_OutModel : ModelResult
    {
        public List<Par_ExportCauModel> Par_ExportCau { get; set; } = new();
    }

    public class Par_ExportCau_Get_InModel
    {
        public int Id { get; set; }
    }
    public class Par_ExportCau_Get_OutModel : ModelResult
    {
        public Par_ExportCauModel? Par_ExportCau { get; set; }
        public List<Par_ExportCau_CausaliModel> Par_ExportCau_Causali { get; set; } = new();
    }

    public class Par_ExportCau_Put_InModel
    {
        public Par_ExportCauModel Par_ExportCau { get; set; } = new();
        public List<Par_ExportCau_CausaliModel> Par_ExportCau_Causali { get; set; } = new();
    }
    public class Par_ExportCau_Put_OutModel : ModelResult
    {
        public Par_ExportCauModel? Par_ExportCau { get; set; }
        public List<Par_ExportCau_CausaliModel> Par_ExportCau_Causali { get; set; } = new();
    }

    public class Par_ExportCau_Delete_InModel
    {
        public int Id { get; set; }
    }
    public class Par_ExportCau_Delete_OutModel : ModelResult { }
}
