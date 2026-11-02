using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models
{
    /*
     Livello di cui l'utente gestisce le impostazioni AI:
       IsDefault = true  -> SuperUser: valori di default del livello (Dev, Dealer, FinancialAdvisor, Company)
       IsDefault = false -> PowerAdmin/Admin: personalizzazione del proprio nodo
    */
    public class TicketTypeAiSettingScopeModel
    {
        public TicketLevel Level { get; set; }
        public bool IsDefault { get; set; }
        public bool CanEdit { get; set; }
    }

    // un tipo di richiesta abilitato al livello: valori effettivi e di default
    public class TicketTypeAiSettingModel
    {
        public int IdTicketType { get; set; }
        public string Codice { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public ApplicationType? ApplicationType { get; set; }
        // il nodo ha una riga propria (sempre falso per i valori di default)
        public bool Custom { get; set; }
        public bool AiEnabled { get; set; }
        public int AiMaxAttempts { get; set; }
        public bool DefaultAiEnabled { get; set; }
        public int DefaultAiMaxAttempts { get; set; }
        // la risposta AI usa anche la documentazione dei livelli sopra (false: solo quella del nodo)
        public bool UseParentDocuments { get; set; } = true;
        public bool DefaultUseParentDocuments { get; set; } = true;
    }


    public class TicketTypeAiSettingScopeInModel
    {
    }

    public class TicketTypeAiSettingScopeOutModel : ModelResult
    {
        public List<TicketTypeAiSettingScopeModel> Scope { get; set; } = new List<TicketTypeAiSettingScopeModel>();
        public int MaxAttempts { get; set; }
    }


    public class TicketTypeAiSettingListInModel
    {
        public TicketLevel Level { get; set; }
    }

    public class TicketTypeAiSettingListOutModel : ModelResult
    {
        public List<TicketTypeAiSettingModel> TicketTypeAiSetting { get; set; } = new List<TicketTypeAiSettingModel>();
    }


    // Custom = false sul nodo: torna ai valori di default (la riga del nodo si elimina)
    public class TicketTypeAiSettingPutInModel
    {
        public TicketLevel Level { get; set; }
        public List<TicketTypeAiSettingModel> TicketTypeAiSetting { get; set; } = new List<TicketTypeAiSettingModel>();
    }

    public class TicketTypeAiSettingPutOutModel : ModelResult
    {
        public List<TicketTypeAiSettingModel> TicketTypeAiSetting { get; set; } = new List<TicketTypeAiSettingModel>();
    }
}
