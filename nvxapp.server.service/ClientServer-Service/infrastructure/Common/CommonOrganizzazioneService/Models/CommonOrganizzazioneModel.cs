using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Collections.Generic;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Common.CommonOrganizzazioneService.Models
{
    // sede dell'azienda (tabella comune Az_Sedi) con i suoi reparti di primo livello
    public class Az_SediCommonModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string? Descrizione { get; set; }
        public Boolean Default { get; set; }

        // reparti senza padre; i sottoreparti stanno in Az_SediRepartoCommonModel.Az_SediReparto_Sub
        public List<Az_SediRepartoCommonModel> Az_SediReparto { get; set; } = new List<Az_SediRepartoCommonModel>();
    }

    // reparto di una sede (tabella comune Az_SediReparto), ad albero
    public class Az_SediRepartoCommonModel
    {
        public int Id { get; set; }
        public int IdAz_Sedi { get; set; }
        public int? IdAz_SediReparto { get; set; }   // reparto padre
        public string? Descrizione { get; set; }
        public Boolean Default { get; set; }

        public List<Az_SediRepartoCommonModel> Az_SediReparto_Sub { get; set; } = new List<Az_SediRepartoCommonModel>();
    }

    // competenza definita dall'azienda (tabella comune Par_Competenza)
    public class Par_CompetenzaCommonModel
    {
        public int Id { get; set; }
        public int IdAz_Anagrafica { get; set; }
        public string? Descrizione { get; set; }
        public Boolean Default { get; set; }
    }

    public class CommonOrganizzazione_SediGetAll_InModel
    {
    }

    public class CommonOrganizzazione_SediGetAll_OutModel : ModelResult
    {
        public List<Az_SediCommonModel> Az_Sedi { get; set; } = new List<Az_SediCommonModel>();
        public CommonOrganizzazione_SediGetAll_OutModel() { }
    }

    public class CommonOrganizzazione_CompetenzeGetAll_InModel
    {
    }

    public class CommonOrganizzazione_CompetenzeGetAll_OutModel : ModelResult
    {
        public List<Par_CompetenzaCommonModel> Par_Competenza { get; set; } = new List<Par_CompetenzaCommonModel>();
        public CommonOrganizzazione_CompetenzeGetAll_OutModel() { }
    }
}
