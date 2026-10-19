using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_TipoSpesaService.Models
{
    public class Par_TipoSpesaModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string? Codice { get; set; }
        public string? Descrizione { get; set; }
    }

    public class Par_TipoSpesaInModel
    {
    }

    public class Par_TipoSpesaOutModel : ModelResult
    {
        public List<Par_TipoSpesaModel> Par_TipoSpesa { get; set; } = new List<Par_TipoSpesaModel>();
    }

    public class Par_TipoSpesaGetInModel
    {
        public int Id { get; set; } = 0;
    }
    public class Par_TipoSpesaGetOutModel : ModelResult
    {
        public Par_TipoSpesaModel Par_TipoSpesa { get; set; } = new Par_TipoSpesaModel();
    }

    public class Par_TipoSpesaPutInModel
    {
        public Par_TipoSpesaModel Par_TipoSpesa { get; set; } = new Par_TipoSpesaModel();
    }
    public class Par_TipoSpesaPutOutModel : ModelResult
    {
        public Par_TipoSpesaModel Par_TipoSpesa { get; set; } = new Par_TipoSpesaModel();
    }

    public class Par_TipoSpesa_DeleteInModel
    {
        public int Id { get; set; }
    }
    public class Par_TipoSpesa_DeleteOutModel : ModelResult { }
}
