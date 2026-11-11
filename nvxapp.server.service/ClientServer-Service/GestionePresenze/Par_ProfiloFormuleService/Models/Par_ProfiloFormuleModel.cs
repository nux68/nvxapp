using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloFormuleService.Models
{
    public class Par_ProfiloFormuleModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string? Codice { get; set; }
        public string? Descrizione { get; set; }
    }

    public class Par_ProfiloFormuleDettModel
    {
        public int Id { get; set; }               // < 0 = riga nuova (client)
        public int IdPar_ProfiloFormule { get; set; }
        public int IdPar_Formule { get; set; }
        public int Ordine { get; set; } = 1;
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        public bool Sospesa { get; set; }
        public FormulaFase Fase { get; set; } = FormulaFase.Finale;
    }

    public class Par_ProfiloFormuleInModel { }
    public class Par_ProfiloFormuleOutModel : ModelResult
    {
        public List<Par_ProfiloFormuleModel> Par_ProfiloFormule { get; set; } = new();
    }

    public class Par_ProfiloFormuleGetInModel
    {
        public int Id { get; set; }
    }
    public class Par_ProfiloFormuleGetOutModel : ModelResult
    {
        public Par_ProfiloFormuleModel Par_ProfiloFormule { get; set; } = new();
        public List<Par_ProfiloFormuleDettModel> Par_ProfiloFormuleDett { get; set; } = new();
    }

    public class Par_ProfiloFormulePutInModel
    {
        public Par_ProfiloFormuleModel Par_ProfiloFormule { get; set; } = new();
        public List<Par_ProfiloFormuleDettModel> Par_ProfiloFormuleDett { get; set; } = new();
    }
    public class Par_ProfiloFormulePutOutModel : ModelResult
    {
        public Par_ProfiloFormuleModel Par_ProfiloFormule { get; set; } = new();
        public List<Par_ProfiloFormuleDettModel> Par_ProfiloFormuleDett { get; set; } = new();
    }

    public class Par_ProfiloFormule_DeleteInModel
    {
        public int Id { get; set; }
    }
    public class Par_ProfiloFormule_DeleteOutModel : ModelResult { }

    public class Par_ProfiloFormule_DuplicaInModel
    {
        public int Id { get; set; }
        public string? Codice { get; set; }
    }
    public class Par_ProfiloFormule_DuplicaOutModel : ModelResult
    {
        public Par_ProfiloFormuleModel Par_ProfiloFormule { get; set; } = new();
    }
}
