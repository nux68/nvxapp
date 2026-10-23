using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models
{
    // riga degli elenchi
    public class TicketListModel
    {
        public int Id { get; set; }
        public int IdTicketType { get; set; }
        public string TipoDescrizione { get; set; } = string.Empty;
        public string Oggetto { get; set; } = string.Empty;
        public TicketState State { get; set; }
        public TicketLevel OpenerLevel { get; set; }
        public string IdAspNetUsersOpener { get; set; } = string.Empty;
        public string OpenerUserName { get; set; } = string.Empty;
        public TicketLevel CurrentLevel { get; set; }
        public string? CurrentIdAspNetUsers { get; set; }
        public string? CurrentUserName { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int Version { get; set; }
    }

    // allegato: File valorizzato solo in invio e nel download
    public class TicketAttachModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public byte[]? File { get; set; }
    }

    // passaggio della cronologia
    public class TicketCronologyModel
    {
        public int Id { get; set; }
        public TicketLevel Level { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? IdAspNetUsersAuthor { get; set; }
        public string? AuthorName { get; set; }
        public TicketAction Action { get; set; }
        public TicketState State { get; set; }
        public string? Text { get; set; }
        public TicketLevel? ReturnLevel { get; set; }
        public string? ReturnUserName { get; set; }
        public int? IdTicketType { get; set; }
        public string? TipoDescrizione { get; set; }
        public DateTime? CreationDate { get; set; }
        public List<TicketAttachModel> TicketAttach { get; set; } = new List<TicketAttachModel>();
    }

    // utente proposto (assegnatari)
    public class TicketUserModel
    {
        public string IdAspNetUsers { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }

    // destinatario di una richiesta di informazioni: chi ha aperto o un livello intermedio
    public class TicketInfoTargetModel
    {
        public TicketLevel Level { get; set; }
        public bool IsOpener { get; set; }
        // chi aveva in carico il ticket a quel livello (o chi ha aperto); null = solo al livello
        public string? IdAspNetUsers { get; set; }
        public string? UserName { get; set; }
    }

    // tipo di richiesta proponibile
    public class TicketTypeOptionModel
    {
        public int Id { get; set; }
        public ApplicationType? ApplicationType { get; set; }
        public string Descrizione { get; set; } = string.Empty;
    }

    // ruolo con cui l'utente puo' aprire un ticket e tipi disponibili per quel ruolo
    public class TicketOpenerRoleModel
    {
        public TicketLevel Level { get; set; }
        public List<TicketTypeOptionModel> TicketType { get; set; } = new List<TicketTypeOptionModel>();
    }

    // azioni che l'utente corrente puo' eseguire sul ticket
    public class TicketActionsModel
    {
        public bool Assegna { get; set; }
        public bool Inoltra { get; set; }
        public bool RichiediInfo { get; set; }
        public bool Rispondi { get; set; }
        public bool Nota { get; set; }
        public bool Risolvi { get; set; }
        public bool Riapri { get; set; }
        public bool Chiudi { get; set; }
        public bool Riclassifica { get; set; }
        public TicketLevel? InoltraLevel { get; set; }
        public List<TicketUserModel> Assegnatario { get; set; } = new List<TicketUserModel>();
        public List<TicketInfoTargetModel> InfoTarget { get; set; } = new List<TicketInfoTargetModel>();
        public List<TicketTypeOptionModel> TicketType { get; set; } = new List<TicketTypeOptionModel>();
    }

    // dettaglio
    public class TicketDetailModel : TicketListModel
    {
        public string Text { get; set; } = string.Empty;
        public List<TicketCronologyModel> TicketCronology { get; set; } = new List<TicketCronologyModel>();
        public TicketActionsModel Actions { get; set; } = new TicketActionsModel();
    }


    // elenco: View 0 = in carico a me, 1 = aperti da me, 2 = livelli figli e assegnati in passato
    public class TicketListInModel
    {
        public int View { get; set; }
        public TicketState? State { get; set; }
    }

    public class TicketListOutModel : ModelResult
    {
        public List<TicketListModel> Ticket { get; set; } = new List<TicketListModel>();
    }


    // numero di ticket in carico all'utente (contatore del menu)
    public class TicketCountInModel
    {
    }

    public class TicketCountOutModel : ModelResult
    {
        public int InCarico { get; set; }
    }


    public class TicketGetInModel
    {
        public int Id { get; set; }
    }

    public class TicketGetOutModel : ModelResult
    {
        public TicketDetailModel Ticket { get; set; } = new TicketDetailModel();
    }


    // dati per la pagina di apertura
    public class TicketOpenInfoInModel
    {
    }

    public class TicketOpenInfoOutModel : ModelResult
    {
        public List<TicketOpenerRoleModel> OpenerRole { get; set; } = new List<TicketOpenerRoleModel>();
        // solo per il SuperUser: utenti PowerAdmin e Admin a cui assegnare il ticket
        public List<TicketUserModel> Assegnatario { get; set; } = new List<TicketUserModel>();
    }


    public class TicketOpenModel
    {
        public TicketLevel OpenerLevel { get; set; }
        public int IdTicketType { get; set; }
        public string Oggetto { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        // solo per il SuperUser
        public string? IdAspNetUsersAssignee { get; set; }
        public List<TicketAttachModel> TicketAttach { get; set; } = new List<TicketAttachModel>();
    }

    public class TicketOpenInModel
    {
        public TicketOpenModel Ticket { get; set; } = new TicketOpenModel();
    }

    public class TicketOpenOutModel : ModelResult
    {
        public int Id { get; set; }
    }


    /*
     Passaggio sul ticket (Assegnazione, Inoltro, RichiestaInfo, RispostaInfo, Nota, Risoluzione,
     Riapertura, Chiusura, Riclassificazione).
        - Version       : versione del ticket vista dall'utente
        - IdAspNetUsers : assegnatario (Assegnazione)
        - InfoLevel     : livello a cui chiedere informazioni (RichiestaInfo); InfoToOpener = a chi ha aperto,
                          InfoToLevel = al livello senza assegnatario invece che a chi l'aveva in carico
        - IdTicketType  : nuovo tipo (Riclassificazione)
    */
    public class TicketActionInModel
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public TicketAction Action { get; set; }
        public string? Text { get; set; }
        public string? IdAspNetUsers { get; set; }
        public TicketLevel? InfoLevel { get; set; }
        public bool InfoToOpener { get; set; }
        public bool InfoToLevel { get; set; }
        public int? IdTicketType { get; set; }
        public List<TicketAttachModel> TicketAttach { get; set; } = new List<TicketAttachModel>();
    }

    public class TicketActionOutModel : ModelResult
    {
        public TicketDetailModel Ticket { get; set; } = new TicketDetailModel();
    }


    // download di un allegato
    public class TicketAttachGetInModel
    {
        public int Id { get; set; }
    }

    public class TicketAttachGetOutModel : ModelResult
    {
        public TicketAttachModel TicketAttach { get; set; } = new TicketAttachModel();
    }


    // azienda spostata sotto un altro studio (chiamato da CompanyMove): si riallineano i ticket non chiusi
    public class TicketCompanyMoveInModel
    {
        public int IdCompany { get; set; }
    }

    public class TicketCompanyMoveOutModel : ModelResult
    {
        // ticket riallineati
        public int Riallineati { get; set; }
    }


    /*
     Risoluzione automatica AI (predisposizione): chiamata prima di passare il ticket al livello superiore.
     Se Resolved il ticket torna a chi l'ha aperto come Risolto con Text e gli allegati.
    */
    public class TicketAiResolveInModel
    {
        public int IdTicket { get; set; }
        public int IdTicketType { get; set; }
        public TicketLevel DestinationLevel { get; set; }
        public string Oggetto { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public List<TicketCronologyModel> TicketCronology { get; set; } = new List<TicketCronologyModel>();
    }

    public class TicketAiResolveOutModel : ModelResult
    {
        public bool Resolved { get; set; }
        public string? Text { get; set; }
        public List<TicketAttachModel> TicketAttach { get; set; } = new List<TicketAttachModel>();
    }
}
