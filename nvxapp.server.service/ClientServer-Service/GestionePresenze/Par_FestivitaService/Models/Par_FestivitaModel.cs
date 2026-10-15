using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_FestivitaService.Models
{
    public class Par_FestivitaModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string? Descrizione { get; set; }
        public TipoFestivita TipoFestivita { get; set; }
        public DateTime Data { get; set; }
    }

    public class Par_Festivita_GetAllInModel { }
    public class Par_Festivita_GetAllOutModel : ModelResult
    {
        public List<Par_FestivitaModel> Par_Festivita { get; set; } = new List<Par_FestivitaModel>();
    }

    public class Par_Festivita_GetInModel
    {
        public int Id { get; set; }
    }
    public class Par_Festivita_GetOutModel : ModelResult
    {
        public Par_FestivitaModel Par_Festivita { get; set; } = new Par_FestivitaModel();
    }

    public class Par_Festivita_PutInModel
    {
        public Par_FestivitaModel Par_Festivita { get; set; } = new Par_FestivitaModel();
    }
    public class Par_Festivita_PutOutModel : ModelResult
    {
        public Par_FestivitaModel Par_Festivita { get; set; } = new Par_FestivitaModel();
    }

    public class Par_Festivita_DeleteInModel
    {
        public int Id { get; set; }
    }
    public class Par_Festivita_DeleteOutModel : ModelResult { }
}
