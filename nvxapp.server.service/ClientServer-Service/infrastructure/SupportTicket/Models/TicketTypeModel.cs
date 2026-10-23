using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models
{
    // riga dell'elenco dei tipi di richiesta
    public class TicketTypeListModel
    {
        public int Id { get; set; }
        public ApplicationType? ApplicationType { get; set; }
        public string Codice { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public bool AiResolution { get; set; }
        public int Ordine { get; set; }
        public bool Active { get; set; }
        public List<TicketLevel> Level { get; set; } = new List<TicketLevel>();
    }

    // tipo di richiesta in modifica
    public class TicketTypeEditModel
    {
        public int Id { get; set; }
        public ApplicationType? ApplicationType { get; set; }
        public string Codice { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public bool AiResolution { get; set; }
        public int Ordine { get; set; }
        public bool Active { get; set; } = true;
        public List<TicketLevel> Level { get; set; } = new List<TicketLevel>();
    }


    public class TicketTypeListInModel
    {
    }

    public class TicketTypeListOutModel : ModelResult
    {
        public List<TicketTypeListModel> TicketType { get; set; } = new List<TicketTypeListModel>();
    }


    public class TicketTypeGetInModel
    {
        public int Id { get; set; }
    }

    public class TicketTypeGetOutModel : ModelResult
    {
        public TicketTypeEditModel TicketType { get; set; } = new TicketTypeEditModel();
    }


    public class TicketTypePutInModel
    {
        public TicketTypeEditModel TicketType { get; set; } = new TicketTypeEditModel();
    }

    public class TicketTypePutOutModel : ModelResult
    {
        public TicketTypeEditModel TicketType { get; set; } = new TicketTypeEditModel();
    }
}
