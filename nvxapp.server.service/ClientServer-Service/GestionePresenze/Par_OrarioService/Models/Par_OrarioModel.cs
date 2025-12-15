using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models
{
    public class Par_OrarioModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string Codice { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public int NumGiorniCiclo { get; set; }
        
    }

    public class Par_Orario_GetAllInModel { }
    public class Par_Orario_GetAllOutModel : ModelResult
    {
        public List<Par_OrarioModel> Par_Orario { get; set; } = new List<Par_OrarioModel>();
    }

    public class Par_Orario_GetInModel
    {
        public int Id { get; set; }
    }
    public class Par_Orario_GetOutModel : ModelResult
    {
        public Par_OrarioModel? Par_Orario { get; set; }
    }

    public class Par_Orario_PutInModel
    {
        public Par_OrarioModel Par_Orario { get; set; } = new Par_OrarioModel();
    }
    public class Par_Orario_PutOutModel : ModelResult
    {
        public Par_OrarioModel? Par_Orario { get; set; }
    }

    public class Par_Orario_DeleteInModel
    {
        public int Id { get; set; }
    }
    public class Par_Orario_DeleteOutModel : ModelResult { }
}
