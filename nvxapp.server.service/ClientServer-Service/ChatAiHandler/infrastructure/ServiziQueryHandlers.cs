using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Request;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Request.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using static nvxapp.server.service.ClientServer_Service.ChatAiHandler.infrastructure.ChatServiziHelper;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.infrastructure
{
    /*
     Interrogazioni dei servizi comuni dalla chat e dall'accesso AI (MCP): sola lettura, senza conferma
     (ANALISI_INFRASTRUCTURE_MCP.md, MCP-30). I dati arrivano solo dai servizi delle pagine, che applicano azienda,
     utente e permessi; nessuna lettura cambia lo stato (le notifiche restano da leggere).
     Ogni comando restituisce un testo breve (chat e voce, elenchi troncati) e i dati strutturati per l'AI
     (date ISO yyyy-MM-dd).
    */

    // ---------------------------------------------------------------------------------------------
    // Ticket di assistenza
    // ---------------------------------------------------------------------------------------------

    // ticket non chiusi aperti dall'utente, oppure in carico a lui o al suo livello (chi gestisce l'assistenza)
    public class SupportTicketsHandler : BaseCommandHandler
    {
        public const string Mine = "mine";
        public const string Handled = "handled";

        // ticket al massimo nei dati (e di cui si legge l'ultima risposta)
        private const int MaxTicket = TicketService.TicketLastReplyMax;

        private readonly ITicketService _ticketService;
        private readonly IntentDefinition _intentDefinition;

        public SupportTicketsHandler(ITicketService ticketService)
        {
            _ticketService = ticketService;
            _intentDefinition = new IntentDefinition
            {
                Name = "SupportTickets",
                Category = ChatAIMessages.Get("ChatCategoryServices"),
                CategoryOrder = IntentCategoryOrder.Services,
                DisplayName = ChatAIMessages.Get("ChatTicketsDisplayName"),
                Description = ChatAIMessages.Get("ChatTicketsDescription"),
                Usable4Role = new List<string> { "*" },
                Keywords = new() { "ticket", "assistenza", "segnalazion", "support", "help desk", "helpdesk" },
                Examples = new() { "quali ticket ho aperto?", "a che punto è il mio ticket di assistenza?", "quali ticket ho in carico?", "show my open support tickets" },
                RequiresConfirmation = false,
                Pages = new() { "/ticketlist" },
                Slots = BuildSlots(
                    SlotScelta("scope", Mine,
                        "quali ticket: 'mine' = aperti dall'utente (predefinito), 'handled' = in carico all'utente o al suo livello, per chi gestisce l'assistenza",
                        ChatAIMessages.Get("ChatTicketsScopeQuestion"), ChatAIMessages.Get("ChatTicketsScopeLabel"),
                        (Handled, new[] { "in carico", "gestire", "gestisco", "da gestire", "assegnati", "da lavorare", "handled", "assigned", "to handle", "queue" }),
                        (Mine, new[] { "miei", "mio", "mie", "aperti da me", "ho aperto", "mine", "my", "opened by me" })))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var scope = slots.GetValueOrDefault("scope") == Handled ? Handled : Mine;

            // View 1 = aperti da me, 0 = in carico (gia' senza i chiusi)
            var res = await _ticketService.TicketList(new GenericRequest<TicketListInModel>(
                new TicketListInModel { View = scope == Mine ? 1 : 0 }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var tutti = res.Data.Ticket.Where(t => t.State != TicketState.Chiuso).OrderByDescending(t => t.ModifiedDate ?? t.CreationDate).ToList();
            if (tutti.Count == 0)
                return CommandResult.Ok(ChatAIMessages.Get(scope == Mine ? "ChatTicketsNoneMine" : "ChatTicketsNoneHandled"));

            var tickets = tutti.Take(MaxTicket).ToList();
            var risposte = new Dictionary<int, TicketLastReplyModel>();
            var resRisposte = await _ticketService.TicketLastReply(new GenericRequest<TicketLastReplyInModel>(
                new TicketLastReplyInModel { Id = tickets.Select(t => t.Id).ToList() }), false);
            if (resRisposte.Success && resRisposte.Data != null)
                risposte = resRisposte.Data.Reply.ToDictionary(r => r.IdTicket);

            var dati = new ChatTicketsData
            {
                Scope = scope,
                Total = tutti.Count,
                Tickets = tickets.Select(t =>
                {
                    risposte.TryGetValue(t.Id, out var r);
                    return new ChatTicketItem
                    {
                        Id = t.Id,
                        Subject = t.Oggetto,
                        Type = t.TipoDescrizione,
                        State = ChatAIMessages.Get("ChatTicketState_" + (int)t.State),
                        StateCode = t.State.ToString(),
                        OpenedOn = Iso(t.CreationDate),
                        UpdatedOn = Iso(t.ModifiedDate),
                        OpenedBy = t.OpenerUserName,
                        AssignedTo = t.CurrentUserName,
                        LastReply = Tronca(r?.Text, MaxTestoDati),
                        LastReplyBy = r?.AuthorName,
                        LastReplyOn = Iso(r?.CreationDate)
                    };
                }).ToList()
            };

            var titolo = ChatAIMessages.Get(scope == Mine ? "ChatTicketsTitleMine" : "ChatTicketsTitleHandled", tutti.Count);
            var righe = tickets.Select(t =>
            {
                var riga = $"#{t.Id} {UnaRiga(t.Oggetto, 80)} — {ChatAIMessages.Get("ChatTicketState_" + (int)t.State)}";
                if (t.CreationDate != null)
                    riga += ", " + ChatAIMessages.Get("ChatTicketOpenedOn", Data(t.CreationDate.Value));
                if (risposte.TryGetValue(t.Id, out var r) && r.CreationDate != null)
                    riga += "; " + ChatAIMessages.Get("ChatTicketLastReplyOn", Data(r.CreationDate.Value));
                return riga;
            });
            return CommandResult.Ok(Elenco(titolo, righe), dati);
        }
    }

    public class ChatTicketsData
    {
        // "mine" = aperti dall'utente, "handled" = in carico
        public string Scope { get; set; } = string.Empty;
        // ticket non chiusi trovati (Tickets ne contiene al massimo 50, dal piu' recente)
        public int Total { get; set; }
        public List<ChatTicketItem> Tickets { get; set; } = new();
    }

    public class ChatTicketItem
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string? OpenedOn { get; set; }
        public string? UpdatedOn { get; set; }
        public string OpenedBy { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
        public string? LastReply { get; set; }
        public string? LastReplyBy { get; set; }
        public string? LastReplyOn { get; set; }
    }

    // ---------------------------------------------------------------------------------------------
    // Notifiche
    // ---------------------------------------------------------------------------------------------

    // ultime notifiche dell'utente (da leggere per default); non le segna come lette
    public class MyNotificationsHandler : BaseCommandHandler
    {
        public const string Unread = "unread";
        public const string All = "all";

        private const int Predefinite = 10;
        // la pagina del servizio ne restituisce 50
        private const int Massimo = 50;

        private readonly INotificationService _notificationService;
        private readonly INotificationCategories _categories;
        private readonly IntentDefinition _intentDefinition;

        public MyNotificationsHandler(INotificationService notificationService, INotificationCategories categories)
        {
            _notificationService = notificationService;
            _categories = categories;
            _intentDefinition = new IntentDefinition
            {
                Name = "MyNotifications",
                Category = ChatAIMessages.Get("ChatCategoryServices"),
                CategoryOrder = IntentCategoryOrder.Services,
                DisplayName = ChatAIMessages.Get("ChatNotificationsDisplayName"),
                Description = ChatAIMessages.Get("ChatNotificationsDescription"),
                Usable4Role = new List<string> { "*" },
                Keywords = new() { "notifiche", "notifica", "avvisi", "notification", "alerts" },
                Examples = new() { "ho notifiche da leggere?", "quali sono le mie ultime notifiche?", "mostrami le ultime 20 notifiche, anche quelle lette", "do I have unread notifications?" },
                RequiresConfirmation = false,
                Pages = new() { "/notifications" },
                Slots = BuildSlots(
                    SlotScelta("filter", Unread,
                        "quali notifiche: 'unread' = solo da leggere (predefinito), 'all' = anche quelle gia' lette",
                        ChatAIMessages.Get("ChatNotificationsFilterQuestion"), ChatAIMessages.Get("ChatNotificationsFilterLabel"),
                        (All, new[] { "tutte", "anche lette", "anche quelle lette", "lette", "all", "including read", "read ones" }),
                        (Unread, new[] { "da leggere", "non lette", "nuove", "unread", "new" })),
                    SlotNumero("count", Predefinite, 1, Massimo,
                        $"quante notifiche al massimo, dalla piu' recente (numero da 1 a {Massimo}); ometti se l'utente non lo dice",
                        ChatAIMessages.Get("ChatNotificationsCountQuestion"), ChatAIMessages.Get("ChatNotificationsCountLabel")))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            bool soloDaLeggere = slots.GetValueOrDefault("filter") != All;
            int quante = Numero(slots, "count", Predefinite, 1, Massimo);

            // solo l'elenco: NotificationRead non si chiama, le notifiche restano da leggere
            var res = await _notificationService.NotificationList(new GenericRequest<NotificationListInModel>(
                new NotificationListInModel { SoloNonLette = soloDaLeggere, Pagina = 0 }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var righe = res.Data.Notification.Take(quante).ToList();
            if (righe.Count == 0)
                return CommandResult.Ok(ChatAIMessages.Get(soloDaLeggere ? "ChatNotificationsNoneUnread" : "ChatNotificationsNone"));

            var dati = new ChatNotificationsData
            {
                Filter = soloDaLeggere ? Unread : All,
                Unread = res.Data.NonLette,
                Notifications = righe.Select(n =>
                {
                    var categoria = _categories.Find(n.Categoria);
                    return new ChatNotificationItem
                    {
                        Id = n.Id,
                        Date = Iso(n.Data),
                        Time = IsoOra(n.Data),
                        Title = n.Titolo,
                        Message = Tronca(n.Messaggio, MaxTestoDati),
                        Category = categoria?.Titolo ?? n.Categoria,
                        Application = NomeApplicativo(categoria?.Application ?? ApplicationType.Common),
                        Read = n.Letta
                    };
                }).ToList()
            };

            var titolo = ChatAIMessages.Get(soloDaLeggere ? "ChatNotificationsTitleUnread" : "ChatNotificationsTitle", righe.Count, res.Data.NonLette);
            var testo = righe.Select(n =>
            {
                var quando = n.Data != null ? DataOra(n.Data.Value) + " " : string.Empty;
                var riga = quando + UnaRiga(n.Titolo, 80);
                if (!string.IsNullOrWhiteSpace(n.Messaggio))
                    riga += " — " + UnaRiga(n.Messaggio, 100);
                return riga;
            });
            return CommandResult.Ok(Elenco(titolo, testo), dati);
        }
    }

    public class ChatNotificationsData
    {
        // "unread" = solo da leggere, "all" = anche le lette
        public string Filter { get; set; } = string.Empty;
        // notifiche da leggere in tutto
        public int Unread { get; set; }
        public List<ChatNotificationItem> Notifications { get; set; } = new();
    }

    public class ChatNotificationItem
    {
        public int Id { get; set; }
        public string? Date { get; set; }
        public string? Time { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Application { get; set; } = string.Empty;
        public bool Read { get; set; }
    }

    // ---------------------------------------------------------------------------------------------
    // Scadenzario
    // ---------------------------------------------------------------------------------------------

    // scadenze aperte dello scadenzario comune entro n giorni (comprese quelle gia' scadute)
    public class DeadlinesHandler : BaseCommandHandler
    {
        public const string All = "all";
        public const string Mine = "mine";

        private const int GiorniPredefiniti = 30;
        private const int GiorniMassimi = 365;

        private readonly IDeadlineService _deadlineService;
        private readonly IntentDefinition _intentDefinition;

        public DeadlinesHandler(IDeadlineService deadlineService)
        {
            _deadlineService = deadlineService;
            _intentDefinition = new IntentDefinition
            {
                Name = "Deadlines",
                Category = ChatAIMessages.Get("ChatCategoryServices"),
                CategoryOrder = IntentCategoryOrder.Services,
                DisplayName = ChatAIMessages.Get("ChatDeadlinesDisplayName"),
                Description = ChatAIMessages.Get("ChatDeadlinesDescription"),
                Usable4Role = RuoliAzienda,
                Keywords = new() { "scadenzario", "scadenze", "in scadenza", "scadute", "deadline", "due dates", "expiring" },
                Examples = new() { "cosa scade nei prossimi 30 giorni?", "ci sono scadenze scadute nello scadenzario?", "le mie scadenze dei prossimi 3 mesi", "what deadlines are coming up?" },
                RequiresConfirmation = false,
                Pages = new() { "/deadlines" },
                Slots = BuildSlots(
                    SlotNumero("days", GiorniPredefiniti, 1, GiorniMassimi,
                        $"entro quanti giorni da oggi, numero da 1 a {GiorniMassimi} (es. 'tre mesi' -> 90, 'un anno' -> 365); ometti se l'utente non lo dice",
                        ChatAIMessages.Get("ChatDeadlinesDaysQuestion"), ChatAIMessages.Get("ChatDeadlinesDaysLabel")),
                    SlotScelta("scope", All,
                        "quali scadenze: 'all' = tutte quelle che l'utente puo' vedere (predefinito; per chi non amministra l'azienda sono comunque solo le sue), 'mine' = solo quelle di cui e' responsabile",
                        ChatAIMessages.Get("ChatDeadlinesScopeQuestion"), ChatAIMessages.Get("ChatDeadlinesScopeLabel"),
                        (Mine, new[] { "mie", "miei", "di cui sono responsabile", "mine", "my" }),
                        (All, new[] { "tutte", "azienda", "dell azienda", "all", "company" })))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            int giorni = Numero(slots, "days", GiorniPredefiniti, 1, GiorniMassimi);
            bool soloMie = slots.GetValueOrDefault("scope") == Mine;

            var res = await _deadlineService.DeadlineList(new GenericRequest<DeadlineListInModel>(
                new DeadlineListInModel { Stato = (int)StatoScadenza.Aperta, EntroGiorni = giorni, SoloMie = soloMie }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var scadenze = res.Data.Deadline.OrderBy(d => d.DataScadenza).ToList();
            if (scadenze.Count == 0)
                return CommandResult.Ok(ChatAIMessages.Get("ChatDeadlinesNone", giorni));

            var oggi = DateTime.Today;
            var dati = new ChatDeadlinesData
            {
                Days = giorni,
                Total = scadenze.Count,
                Overdue = scadenze.Count(d => d.Scaduta),
                Deadlines = scadenze.Select(d => new ChatDeadlineItem
                {
                    Id = d.Id,
                    DueDate = Iso(d.DataScadenza)!,
                    DaysLeft = (d.DataScadenza.Date - oggi).Days,
                    Overdue = d.Scaduta,
                    Description = d.Descrizione,
                    Notes = Tronca(d.Note, MaxTestoDati),
                    Application = NomeApplicativo(d.Application),
                    Source = d.Fonte,
                    Responsible = d.Responsabile
                }).ToList()
            };

            var titolo = ChatAIMessages.Get("ChatDeadlinesTitle", scadenze.Count, giorni, dati.Overdue);
            var righe = scadenze.Select(d =>
                $"{Data(d.DataScadenza)} {UnaRiga(d.Descrizione, 80)} ({NomeApplicativo(d.Application)})"
                + (d.Scaduta ? " — " + ChatAIMessages.Get("ChatDeadlineOverdue") : string.Empty));
            return CommandResult.Ok(Elenco(titolo, righe), dati);
        }
    }

    public class ChatDeadlinesData
    {
        // scadenze aperte entro questi giorni da oggi, comprese quelle gia' scadute
        public int Days { get; set; }
        public int Total { get; set; }
        public int Overdue { get; set; }
        public List<ChatDeadlineItem> Deadlines { get; set; } = new();
    }

    public class ChatDeadlineItem
    {
        public int Id { get; set; }
        public string DueDate { get; set; } = string.Empty;
        // giorni da oggi alla scadenza (negativo = gia' scaduta)
        public int DaysLeft { get; set; }
        public bool Overdue { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Notes { get; set; }
        // applicativo (modulo) e fonte che hanno registrato la scadenza ("manuale" = inserita a mano)
        public string Application { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string? Responsible { get; set; }
    }

    // ---------------------------------------------------------------------------------------------
    // Richieste da approvare / le mie richieste
    // ---------------------------------------------------------------------------------------------

    // stessa fonte della pagina comune delle richieste (RequestService: elenco unico dei moduli attivi)
    public class RequestsHandler : BaseCommandHandler
    {
        public const string ToApprove = "toApprove";
        public const string Mine = "mine";

        private readonly IRequestService _requestService;
        private readonly IChatUserContext _chatUserContext;
        private readonly IntentDefinition _intentDefinition;

        public RequestsHandler(IRequestService requestService, IChatUserContext chatUserContext)
        {
            _requestService = requestService;
            _chatUserContext = chatUserContext;
            _intentDefinition = new IntentDefinition
            {
                Name = "Requests",
                Category = ChatAIMessages.Get("ChatCategoryServices"),
                CategoryOrder = IntentCategoryOrder.Services,
                DisplayName = ChatAIMessages.Get("ChatRequestsDisplayName"),
                Description = ChatAIMessages.Get("ChatRequestsDescription"),
                Usable4Role = RuoliAzienda,
                Keywords = new() { "richieste", "richiesta", "da approvare", "approvazione", "requests", "to approve", "approval" },
                Examples = new() { "quali richieste ci sono da approvare?", "le mie richieste in attesa", "ho richieste ancora da approvare?", "show the requests waiting for my approval" },
                RequiresConfirmation = false,
                Pages = new() { "/requestsadmin", "/requests" },
                Slots = BuildSlots(
                    SlotScelta("scope", string.Empty,
                        "quali richieste: 'toApprove' = quelle che l'amministratore dell'azienda puo' approvare adesso, 'mine' = quelle dell'utente ancora in attesa; se omesso: 'toApprove' per gli amministratori, 'mine' per gli altri",
                        ChatAIMessages.Get("ChatRequestsScopeQuestion"), ChatAIMessages.Get("ChatRequestsScopeLabel"),
                        (Mine, new[] { "mie", "miei", "le mie", "ho fatto", "ho chiesto", "mine", "my", "i made" }),
                        (ToApprove, new[] { "da approvare", "approvare", "approvazione", "to approve", "approve", "approval" })))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            bool amministratore = _chatUserContext.IsInRole(RuoliAmministratoriAzienda);
            var scope = slots.GetValueOrDefault("scope");
            if (scope != ToApprove && scope != Mine)
                scope = amministratore ? ToApprove : Mine;
            if (scope == ToApprove && !amministratore)
                return CommandResult.Fail(ChatAIMessages.Get("ChatRequestsNotAdmin"));

            // DaApprovare: per l'amministratore quelle che puo' decidere adesso, per il richiedente le sue in attesa
            var res = await _requestService.RequestList(new GenericRequest<RequestListInModel>(
                new RequestListInModel { Admin = scope == ToApprove, Filtro = RequestFilter.DaApprovare }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var richieste = res.Data.Richieste.OrderBy(r => r.Dal).ToList();
            if (richieste.Count == 0)
                return CommandResult.Ok(ChatAIMessages.Get(scope == ToApprove ? "ChatRequestsNoneToApprove" : "ChatRequestsNoneMine"));

            var dati = new ChatRequestsData
            {
                Scope = scope,
                Total = richieste.Count,
                Requests = richieste.Select(r => new ChatRequestItem
                {
                    Id = r.Id,
                    Application = NomeApplicativo(r.Application),
                    Type = r.TipoTesto,
                    Requester = r.Richiedente,
                    From = Iso(r.Dal)!,
                    FromTime = r.SoloData ? null : IsoOra(r.Dal),
                    To = Iso(r.Al),
                    ToTime = r.SoloData || r.Al == null ? null : IsoOra(r.Al),
                    Detail = Tronca(r.Dettaglio, MaxTestoDati),
                    State = r.StatoTesto,
                    StateCode = r.Stato.ToString(),
                    RequestedOn = Iso(r.DataRichiesta)
                }).ToList()
            };

            var titolo = ChatAIMessages.Get(scope == ToApprove ? "ChatRequestsTitleToApprove" : "ChatRequestsTitleMine", richieste.Count);
            var righe = richieste.Select(r =>
            {
                var periodo = Periodo(r);
                var riga = scope == ToApprove && !string.IsNullOrWhiteSpace(r.Richiedente)
                    ? $"{r.Richiedente} — {r.TipoTesto}, {periodo}"
                    : $"{r.TipoTesto}, {periodo}";
                if (!string.IsNullOrWhiteSpace(r.Dettaglio))
                    riga += " (" + UnaRiga(r.Dettaglio, 80) + ")";
                if (!string.IsNullOrWhiteSpace(r.StatoTesto))
                    riga += " — " + r.StatoTesto;
                return riga;
            });
            return CommandResult.Ok(Elenco(titolo, righe), dati);
        }

        // "20/11/2026", "20/11/2026 - 22/11/2026" oppure "20/11/2026 09:00-11:00"
        private static string Periodo(RequestItem r)
        {
            if (r.SoloData)
                return r.Al == null || r.Al.Value.Date == r.Dal.Date ? Data(r.Dal) : $"{Data(r.Dal)} - {Data(r.Al.Value)}";
            if (r.Al == null)
                return DataOra(r.Dal);
            return r.Al.Value.Date == r.Dal.Date
                ? $"{DataOra(r.Dal)}-{r.Al.Value:HH:mm}"
                : $"{DataOra(r.Dal)} - {DataOra(r.Al.Value)}";
        }
    }

    public class ChatRequestsData
    {
        // "toApprove" = da approvare (amministratore), "mine" = dell'utente in attesa
        public string Scope { get; set; } = string.Empty;
        public int Total { get; set; }
        public List<ChatRequestItem> Requests { get; set; } = new();
    }

    public class ChatRequestItem
    {
        public string Id { get; set; } = string.Empty;
        public string Application { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Requester { get; set; }
        public string From { get; set; } = string.Empty;
        // ora di inizio e di fine solo per le richieste con orario
        public string? FromTime { get; set; }
        public string? To { get; set; }
        public string? ToTime { get; set; }
        public string? Detail { get; set; }
        public string State { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string? RequestedOn { get; set; }
    }
}
