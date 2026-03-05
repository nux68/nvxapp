using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService.Models
{
    public class Par_ProfiloOrarioModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string Codice { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public int NumGiorniCiclo { get; set; }
        public int TipoProfilo { get; set; }
        public int idPar_Orario_Festivo { get; set; }
        
    }

    public class Par_ProfiloOrario_GetAllInModel { }
    public class Par_ProfiloOrario_GetAllOutModel : ModelResult
    {
        public List<Par_ProfiloOrarioModel> Par_ProfiloOrario { get; set; } = new List<Par_ProfiloOrarioModel>();
    }

    public class Par_ProfiloOrario_GetInModel
    {
        public int Id { get; set; }
    }
    public class Par_ProfiloOrario_GetOutModel : ModelResult
    {
        public Par_ProfiloOrarioModel? Par_ProfiloOrario { get; set; }
        public List<Par_ProfiloOrarioGGModel> Par_ProfiloOrarioGG { get; set; } = new List<Par_ProfiloOrarioGGModel>();
        public List<Par_OrarioIntervalloHHModel> Par_OrarioIntervalloHH { get; set; } = new List<Par_OrarioIntervalloHHModel>();

    }

    public class Par_ProfiloOrario_PutInModel
    {
        public Par_ProfiloOrarioModel Par_ProfiloOrario { get; set; } = new Par_ProfiloOrarioModel();
        public List<Par_ProfiloOrarioGGModel> Par_ProfiloOrarioGG { get; set; } = new List<Par_ProfiloOrarioGGModel>();
    }
    public class Par_ProfiloOrario_PutOutModel : ModelResult
    {
        public Par_ProfiloOrarioModel? Par_ProfiloOrario { get; set; }
        public List<Par_ProfiloOrarioGGModel> Par_ProfiloOrarioGG { get; set; } = new List<Par_ProfiloOrarioGGModel>();
    }

    public class Par_ProfiloOrario_DeleteInModel
    {
        public int Id { get; set; }
    }
    public class Par_ProfiloOrario_DeleteOutModel : ModelResult { }
}
