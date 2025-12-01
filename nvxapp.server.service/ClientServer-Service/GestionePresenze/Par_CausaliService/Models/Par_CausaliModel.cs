using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Collections.Generic;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService.Models
{
    public class Par_CausaliModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string? Codice { get; set; }
        public string? Descrizione { get; set; }
    }

    public class Par_CausaliInModel
    {

    }

    public class Par_CausaliOutModel : ModelResult
    {
        public List<Par_CausaliModel> Par_Causali { get; set; } = new List<Par_CausaliModel>();
        public Par_CausaliOutModel()
        {

        }
    }

    public class Par_CausaliGetInModel
    {
        public int Id { get; set; } = 0;
    }
    public class Par_CausaliGetOutModel : ModelResult
    {
        public Par_CausaliModel Par_Causale { get; set; } = new Par_CausaliModel();
    }
    public class Par_CausaliPutInModel : ModelResult
    {
        public Par_CausaliModel Par_Causale { get; set; } = new Par_CausaliModel();
    }
    public class Par_CausaliPutOutModel : ModelResult
    {
        public Par_CausaliModel Par_Causale { get; set; } = new Par_CausaliModel();
    }

    public class Par_Causali_DeleteInModel
    {
        public int Id { get; set; }
    }
    public class Par_Causali_DeleteOutModel : ModelResult { }
}
