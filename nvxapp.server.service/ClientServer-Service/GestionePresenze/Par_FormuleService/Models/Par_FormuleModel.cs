using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.FormuleEngine;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_FormuleService.Models
{
    public class Par_FormuleModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string? Codice { get; set; }
        public string? Titolo { get; set; }
        public string? Descrizione { get; set; }
        public string? Testo { get; set; }
    }

    public class Par_FormuleInModel { }
    public class Par_FormuleOutModel : ModelResult
    {
        public List<Par_FormuleModel> Par_Formule { get; set; } = new();
    }

    public class Par_FormuleGetInModel
    {
        public int Id { get; set; }
    }
    public class Par_FormuleGetOutModel : ModelResult
    {
        public Par_FormuleModel Par_Formule { get; set; } = new();
    }

    public class Par_FormulePutInModel
    {
        public Par_FormuleModel Par_Formule { get; set; } = new();
    }
    public class Par_FormulePutOutModel : ModelResult
    {
        public Par_FormuleModel Par_Formule { get; set; } = new();
        public List<FormulaErrore> Errori { get; set; } = new();
    }

    public class Par_Formule_DeleteInModel
    {
        public int Id { get; set; }
    }
    public class Par_Formule_DeleteOutModel : ModelResult { }

    public class Par_Formule_DuplicaInModel
    {
        public int Id { get; set; }
        public string? Codice { get; set; }
        public string? Titolo { get; set; }
    }
    public class Par_Formule_DuplicaOutModel : ModelResult
    {
        public Par_FormuleModel Par_Formule { get; set; } = new();
    }

    public class Par_Formule_CtrlSintassiInModel
    {
        public string? Testo { get; set; }
    }
    public class Par_Formule_CtrlSintassiOutModel : ModelResult
    {
        public List<FormulaErrore> Errori { get; set; } = new();
    }

    public class Par_Formule_MetodiInModel { }
    public class Par_Formule_MetodiOutModel : ModelResult
    {
        public List<FormulaMetodo> Metodi { get; set; } = new();
        public string Template { get; set; } = string.Empty;
    }

    /// <summary>Rapporto di lavoro proponibile nella "Prova formula".</summary>
    public class Par_Formule_RapportoProvaModel
    {
        public int IdDip_RapportoLavoro { get; set; }
        public string Descrizione { get; set; } = string.Empty;
    }
    public class Par_Formule_RapportiProvaInModel { }
    public class Par_Formule_RapportiProvaOutModel : ModelResult
    {
        public List<Par_Formule_RapportoProvaModel> Rapporti { get; set; } = new();
    }

    public class Par_Formule_ProvaInModel
    {
        public string? Testo { get; set; }
        public int IdDip_RapportoLavoro { get; set; }
        public DateTime Giorno { get; set; }
        public FormulaFase Fase { get; set; } = FormulaFase.Finale;
    }
    public class Par_Formule_ProvaOutModel : ModelResult
    {
        public List<FormulaErrore> Errori { get; set; } = new();
        public FormulaProvaRisultato Risultato { get; set; } = new();
    }

    public class Par_Formule_LogInModel
    {
        public int? IdPar_Formule { get; set; }
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
    }
    public class Par_Formule_LogRigaModel
    {
        public DateTime Giorno { get; set; }
        public string Dipendente { get; set; } = string.Empty;
        public string Formula { get; set; } = string.Empty;
        public FormulaLogTipo Tipo { get; set; }
        public string? Messaggio { get; set; }
        public DateTime Data { get; set; }
    }
    public class Par_Formule_LogOutModel : ModelResult
    {
        public List<Par_Formule_LogRigaModel> Log { get; set; } = new();
    }

    public class Par_Formule_GeneraAIInModel
    {
        public string? Richiesta { get; set; }
        public string? TestoAttuale { get; set; }
    }
    public class Par_Formule_GeneraAIOutModel : ModelResult
    {
        public string Testo { get; set; } = string.Empty;
        public string Spiegazione { get; set; } = string.Empty;
        public List<FormulaErrore> Errori { get; set; } = new();
    }
}
