using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket
{
    /*
     Ticket di assistenza sui livelli SuperUser, DEV, Dealer, FinancialAdvisor, Company, User
     (specifica: nvxapp.server.service/ANALISI_TICKET.md).

     - Il ticket sale al primo livello abilitato dal tipo sopra quello corrente; il SuperUser apre verso il basso
       assegnando direttamente il ticket a un PowerAdmin o Admin (DEV).
     - Al livello corrente il ticket e' gestito dai PowerAdmin del nodo (anche se assegnato) e dall'assegnatario.
     - Un utente puo' avere piu' ruoli (es. User + CompanyAdmin): i permessi sono l'unione dei ruoli e
       in apertura si sceglie il ruolo, che decide il destinatario.
     - Tutti i controlli sono qui: il client non decide mai livello, nodo o permessi.
     - Ogni passaggio salva insieme cronologia, allegati e ticket (un solo SaveChanges) e incrementa Version:
       un'azione fatta su una versione superata e' rifiutata.
    */
    public class TicketService : ServiceBase, ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ITicketCronologyRepository _ticketCronologyRepository;
        private readonly ITicketAttachRepository _ticketAttachRepository;
        private readonly ITicketTypeRepository _ticketTypeRepository;
        private readonly ITicketTypeLevelRepository _ticketTypeLevelRepository;
        private readonly IAspNetRolesRepository _aspNetRolesRepository;
        private readonly IAspNetUserRolesRepository _aspNetUserRolesRepository;
        private readonly IUserDealerRepository _userDealerRepository;
        private readonly IUserFinancialAdvisorRepository _userFinancialAdvisorRepository;
        private readonly IUserCompanyRepository _userCompanyRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IFinancialAdvisorRepository _financialAdvisorRepository;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly ITicketAiResolverService _ticketAiResolverService;
        private readonly ITicketNotifier _ticketNotifier;

        // ruoli dell'applicazione (Id <-> codice), letti una volta per richiesta
        private Dictionary<string, RoleCode>? _roleCodeById;
        private Dictionary<RoleCode, string>? _roleIdByCode;

        public TicketService(IMapper mapper,
                             UserManager<ApplicationUser> userManager,
                             IAspNetUsersRepository aspNetUsersRepository,
                             IOptions<JwtParameter> jwtParameter,
                             IHttpContextAccessor httpContextAccessor,
                             IConfiguration configuration,

                             ITicketRepository ticketRepository,
                             ITicketCronologyRepository ticketCronologyRepository,
                             ITicketAttachRepository ticketAttachRepository,
                             ITicketTypeRepository ticketTypeRepository,
                             ITicketTypeLevelRepository ticketTypeLevelRepository,
                             IAspNetRolesRepository aspNetRolesRepository,
                             IAspNetUserRolesRepository aspNetUserRolesRepository,
                             IUserDealerRepository userDealerRepository,
                             IUserFinancialAdvisorRepository userFinancialAdvisorRepository,
                             IUserCompanyRepository userCompanyRepository,
                             ICompanyRepository companyRepository,
                             IFinancialAdvisorRepository financialAdvisorRepository,
                             ICompanyApplicationRepository companyApplicationRepository,
                             ITicketAiResolverService ticketAiResolverService,
                             ITicketNotifier ticketNotifier) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _ticketRepository = ticketRepository;
            _ticketCronologyRepository = ticketCronologyRepository;
            _ticketAttachRepository = ticketAttachRepository;
            _ticketTypeRepository = ticketTypeRepository;
            _ticketTypeLevelRepository = ticketTypeLevelRepository;
            _aspNetRolesRepository = aspNetRolesRepository;
            _aspNetUserRolesRepository = aspNetUserRolesRepository;
            _userDealerRepository = userDealerRepository;
            _userFinancialAdvisorRepository = userFinancialAdvisorRepository;
            _userCompanyRepository = userCompanyRepository;
            _companyRepository = companyRepository;
            _financialAdvisorRepository = financialAdvisorRepository;
            _companyApplicationRepository = companyApplicationRepository;
            _ticketAiResolverService = ticketAiResolverService;
            _ticketNotifier = ticketNotifier;
        }


        #region Endpoint

        public virtual async Task<GenericResult<TicketListOutModel>> TicketList(GenericRequest<TicketListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TicketListOutModel();
                var user = await CurrentTicketUser();

                IQueryable<Ticket> query = model.Data.View switch
                {
                    0 => QueryInCarico(user),
                    1 => _ticketRepository.FindAll(t => t.IdAspNetUsersOpener == user.UserId),
                    2 => QueryAltri(user),
                    _ => throw new UserMessageException(InfrastructureMessages.Get("TicketViewNotValid")),
                };

                if (model.Data.State != null)
                {
                    var state = model.Data.State.Value;
                    query = query.Where(t => t.State == state);
                }

                var tickets = query.OrderByDescending(t => t.ModifiedDate).ToList();
                retVal.Ticket = await ToListModel(tickets);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<TicketCountOutModel>> TicketCount(GenericRequest<TicketCountInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TicketCountOutModel();
                var user = await CurrentTicketUser();

                retVal.InCarico = QueryInCarico(user).Count();

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<TicketGetOutModel>> TicketGet(GenericRequest<TicketGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TicketGetOutModel();
                var user = await CurrentTicketUser();

                var ticket = await VisibleTicket(user, model.Data.Id);
                retVal.Ticket = await ToDetailModel(user, ticket);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<TicketOpenInfoOutModel>> TicketOpenInfo(GenericRequest<TicketOpenInfoInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TicketOpenInfoOutModel();
                var user = await CurrentTicketUser();

                retVal.OpenerRole = await OpenerRoles(user);

                if (user.Has(RoleCode.SuperUser))
                    retVal.Assegnatario = await ToUserModel(await DevUsers(false));

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<TicketOpenOutModel>> TicketOpen(GenericRequest<TicketOpenInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TicketOpenOutModel();
                var user = await CurrentTicketUser();
                var data = model.Data.Ticket;

                // ruolo e tipo devono essere tra quelli proposti all'utente
                var openerRole = (await OpenerRoles(user)).FirstOrDefault(r => r.Level == data.OpenerLevel)
                                 ?? throw new UserMessageException(InfrastructureMessages.Get("TicketOpenerRoleNotAvailable"));
                if (!openerRole.TicketType.Any(t => t.Id == data.IdTicketType))
                    throw new UserMessageException(InfrastructureMessages.Get("TicketTypeNotAvailable"));

                string oggetto = (data.Oggetto ?? string.Empty).Trim();
                string text = (data.Text ?? string.Empty).Trim();
                if (oggetto.Length == 0 || text.Length == 0)
                    throw new UserMessageException(InfrastructureMessages.Get("TicketTextRequired"));
                if (oggetto.Length > TicketRules.OggettoMaxLength || text.Length > TicketRules.TextMaxLength)
                    throw new UserMessageException(InfrastructureMessages.Get("TicketTextTooLong", TicketRules.TextMaxLength));
                CheckAttach(data.TicketAttach);

                var type = await TypeWithLevels(data.IdTicketType);
                bool superUser = data.OpenerLevel == TicketLevel.SuperUser;

                // il SuperUser assegna direttamente a un PowerAdmin o Admin
                string? assignee = null;
                if (superUser)
                {
                    assignee = data.IdAspNetUsersAssignee;
                    if (string.IsNullOrEmpty(assignee) || !(await DevUsers(false)).Contains(assignee))
                        throw new UserMessageException(InfrastructureMessages.Get("TicketAssigneeNotValid"));
                }

                var ticket = new Ticket
                {
                    IdTicketType = type.Type.Id,
                    Oggetto = oggetto,
                    Text = text,
                    IdAspNetUsersOpener = user.UserId,
                    OpenerLevel = data.OpenerLevel,
                    OpenerRoleId = RoleId(user.RoleOfLevel(data.OpenerLevel)),
                    Version = 1,
                };
                await SetOriginPath(user, ticket);

                // origine: chi apre, con testo e allegati
                var origin = new TicketCronology
                {
                    TicketNavigation = ticket,
                    RoleId = ticket.OpenerRoleId,
                    Level = ticket.OpenerLevel,
                    UserId = user.UserId,
                    IdAspNetUsersAuthor = user.UserId,
                    Action = TicketAction.Apertura,
                    State = TicketState.Aperto,
                    Text = text,
                };

                var rows = new List<(TicketCronology row, List<TicketAttachModel> attach)> { (origin, data.TicketAttach) };

                if (superUser)
                {
                    rows.Add((new TicketCronology
                    {
                        TicketNavigation = ticket,
                        RoleId = RoleId(RoleCode.PowerAdmin),
                        Level = TicketLevel.Dev,
                        UserId = assignee,
                        IdAspNetUsersAuthor = user.UserId,
                        Action = TicketAction.Assegnazione,
                        State = TicketState.InCarico,
                    }, new List<TicketAttachModel>()));
                }
                else
                {
                    // destinatario: primo livello abilitato sopra chi apre (garantito dai tipi proposti)
                    var next = TicketRules.NextLevelUp(type.Levels, ticket.OpenerLevel)
                               ?? throw new UserMessageException(InfrastructureMessages.Get("TicketNoUpperLevel"));
                    rows.AddRange(await SalitaRows(user, ticket, type, next, null, new List<TicketAttachModel>(), rows.Select(r => r.row).ToList()));
                }

                var last = rows.Last().row;
                ticket.CurrentLevel = last.Level;
                ticket.CurrentRoleId = last.RoleId;
                ticket.CurrentIdAspNetUsers = last.UserId;
                ticket.State = last.State;

                await _ticketRepository.CreateAsync(ticket, false);
                await AddRows(rows);
                await _ticketRepository.SaveChange();

                retVal.Id = ticket.Id;

                var extra = superUser ? await DevUsers(true) : new List<string>();
                await Notify(user, ticket, last.Action, extra);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<TicketActionOutModel>> TicketExecute(GenericRequest<TicketActionInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TicketActionOutModel();
                var user = await CurrentTicketUser();
                var data = model.Data;

                var ticket = await VisibleTicket(user, data.Id);
                if (ticket.Version != data.Version)
                    throw new UserMessageException(InfrastructureMessages.Get("TicketChanged"));

                CheckAttach(data.TicketAttach);
                string? text = string.IsNullOrWhiteSpace(data.Text) ? null : data.Text.Trim();
                if (text != null && text.Length > TicketRules.TextMaxLength)
                    throw new UserMessageException(InfrastructureMessages.Get("TicketTextTooLong", TicketRules.TextMaxLength));

                var type = await TypeWithLevels(ticket.IdTicketType);
                var cronology = Cronology(ticket.Id);
                var actions = await Actions(user, ticket, type, cronology);
                var attach = data.TicketAttach ?? new List<TicketAttachModel>();

                List<(TicketCronology row, List<TicketAttachModel> attach)> rows;
                List<string> extraNotify = new List<string>();

                switch (data.Action)
                {
                    case TicketAction.Assegnazione:
                        {
                            if (!actions.Assegna || !actions.Assegnatario.Any(u => u.IdAspNetUsers == data.IdAspNetUsers))
                                throw NotAllowed();
                            rows = new() { (Row(user, ticket, TicketAction.Assegnazione, ticket.CurrentLevel, ticket.CurrentRoleId, data.IdAspNetUsers,
                                                ticket.State == TicketState.InAttesaInfo ? TicketState.InAttesaInfo : TicketState.InCarico, text), attach) };
                            break;
                        }

                    case TicketAction.Inoltro:
                        {
                            if (!actions.Inoltra || actions.InoltraLevel == null)
                                throw NotAllowed();
                            rows = await SalitaRows(user, ticket, type, actions.InoltraLevel.Value, text, attach, cronology);
                            break;
                        }

                    case TicketAction.RichiestaInfo:
                        {
                            if (!actions.RichiediInfo)
                                throw NotAllowed();
                            RequireText(text);

                            TicketInfoTargetModel? target = data.InfoToOpener
                                ? actions.InfoTarget.FirstOrDefault(x => x.IsOpener)
                                : actions.InfoTarget.FirstOrDefault(x => !x.IsOpener && x.Level == data.InfoLevel);
                            if (target == null)
                                throw new UserMessageException(InfrastructureMessages.Get("TicketInfoTargetNotValid"));

                            string roleId = target.IsOpener ? ticket.OpenerRoleId : RoleId(TicketRules.PowerRole(target.Level));
                            string? userId = target.IsOpener ? ticket.IdAspNetUsersOpener : (data.InfoToLevel ? null : target.IdAspNetUsers);

                            var row = Row(user, ticket, TicketAction.RichiestaInfo, target.Level, roleId, userId, TicketState.InAttesaInfo, text);
                            // la risposta torna a chi chiede
                            row.ReturnLevel = ticket.CurrentLevel;
                            row.ReturnRoleId = ticket.CurrentRoleId;
                            row.ReturnUserId = user.UserId;
                            rows = new() { (row, attach) };
                            break;
                        }

                    case TicketAction.RispostaInfo:
                        {
                            if (!actions.Rispondi)
                                throw NotAllowed();
                            RequireText(text);

                            var request = cronology.LastOrDefault(c => c.Action == TicketAction.RichiestaInfo)
                                          ?? throw NotAllowed();
                            var level = request.ReturnLevel ?? throw NotAllowed();
                            string? returnUser = await StillEligible(ticket, level, request.ReturnUserId);

                            rows = new() { (Row(user, ticket, TicketAction.RispostaInfo, level, request.ReturnRoleId ?? RoleId(TicketRules.PowerRole(level)), returnUser,
                                                returnUser != null ? TicketState.InCarico : TicketState.Aperto, text), attach) };
                            break;
                        }

                    case TicketAction.Nota:
                        {
                            if (!actions.Nota)
                                throw NotAllowed();
                            if (text == null && attach.Count == 0)
                                throw new UserMessageException(InfrastructureMessages.Get("TicketTextRequired"));
                            rows = new() { (Row(user, ticket, TicketAction.Nota, ticket.CurrentLevel, ticket.CurrentRoleId, ticket.CurrentIdAspNetUsers, ticket.State, text), attach) };
                            break;
                        }

                    case TicketAction.Risoluzione:
                        {
                            if (!actions.Risolvi)
                                throw NotAllowed();
                            RequireText(text);

                            var row = Row(user, ticket, TicketAction.Risoluzione, ticket.OpenerLevel, ticket.OpenerRoleId, ticket.IdAspNetUsersOpener, TicketState.Risolto, text);
                            // riaprendo si torna a chi ha risolto
                            row.ReturnLevel = ticket.CurrentLevel;
                            row.ReturnRoleId = ticket.CurrentRoleId;
                            row.ReturnUserId = user.UserId;
                            rows = new() { (row, attach) };
                            break;
                        }

                    case TicketAction.Riapertura:
                        {
                            if (!actions.Riapri)
                                throw NotAllowed();
                            RequireText(text);

                            var resolution = cronology.LastOrDefault(c => c.Action == TicketAction.Risoluzione || c.Action == TicketAction.RisoluzioneAI)
                                             ?? throw NotAllowed();
                            var level = resolution.ReturnLevel ?? throw NotAllowed();
                            // dopo una risoluzione AI si va al livello sostituito, senza assegnatario e senza ritentare l'AI
                            string? returnUser = await StillEligible(ticket, level, resolution.ReturnUserId);

                            rows = new() { (Row(user, ticket, TicketAction.Riapertura, level, resolution.ReturnRoleId ?? RoleId(TicketRules.PowerRole(level)), returnUser,
                                                returnUser != null ? TicketState.InCarico : TicketState.Aperto, text), attach) };
                            break;
                        }

                    case TicketAction.Chiusura:
                        {
                            if (!actions.Chiudi)
                                throw NotAllowed();
                            rows = new() { (Row(user, ticket, TicketAction.Chiusura, ticket.CurrentLevel, ticket.CurrentRoleId, ticket.CurrentIdAspNetUsers, TicketState.Chiuso, text), attach) };
                            break;
                        }

                    case TicketAction.Riclassificazione:
                        {
                            if (!actions.Riclassifica || data.IdTicketType == null || !actions.TicketType.Any(t => t.Id == data.IdTicketType))
                                throw new UserMessageException(InfrastructureMessages.Get("TicketReclassifyNotAllowed"));

                            var row = Row(user, ticket, TicketAction.Riclassificazione, ticket.CurrentLevel, ticket.CurrentRoleId, ticket.CurrentIdAspNetUsers, ticket.State, text);
                            row.IdTicketType = data.IdTicketType;
                            ticket.IdTicketType = data.IdTicketType.Value;
                            rows = new() { (row, attach) };
                            break;
                        }

                    default:
                        throw NotAllowed();
                }

                var last = rows.Last().row;
                ticket.CurrentLevel = last.Level;
                ticket.CurrentRoleId = last.RoleId;
                ticket.CurrentIdAspNetUsers = last.UserId;
                ticket.State = last.State;
                ticket.Version++;

                await AddRows(rows);
                // salva anche cronologia e allegati aggiunti sopra
                await _ticketRepository.UpdateAsync(ticket);

                if (last.Action != TicketAction.Nota && last.Action != TicketAction.Riclassificazione && last.Action != TicketAction.Chiusura)
                    await Notify(user, ticket, last.Action, extraNotify);

                retVal.Ticket = await ToDetailModel(user, ticket);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<TicketAttachGetOutModel>> TicketAttachGet(GenericRequest<TicketAttachGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TicketAttachGetOutModel();
                var user = await CurrentTicketUser();

                var attach = await _ticketAttachRepository.FindByIdAsync(model.Data.Id)
                             ?? throw new UserMessageException(InfrastructureMessages.Get("FileNotFound"));
                var row = await _ticketCronologyRepository.FindByIdAsync(attach.IdTicketCronology)
                          ?? throw new UserMessageException(InfrastructureMessages.Get("FileNotFound"));

                // l'allegato si scarica solo se il ticket e' visibile
                await VisibleTicket(user, row.IdTicket);

                retVal.TicketAttach = _mapper.Map<TicketAttachModel>(attach);
                retVal.TicketAttach.File = attach.File;

                return retVal;
            }, isSubProcess);
        }

        /*
         Azienda spostata sotto un altro studio (sottoprocesso di CompanyMove, gia' riservato al SuperUser):
         sui ticket non chiusi dell'azienda si aggiornano studio e centro; quelli che si trovano al livello
         del vecchio studio (o del vecchio centro, se cambia) passano al nuovo nodo senza assegnatario.
         Chi doveva ricevere una risposta o una riapertura e non e' piu' nel nodo viene saltato al momento
         del ritorno (StillEligible).
        */
        public virtual async Task<GenericResult<TicketCompanyMoveOutModel>> TicketCompanyMove(GenericRequest<TicketCompanyMoveInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TicketCompanyMoveOutModel();
                var user = await CurrentTicketUser();

                var company = await _companyRepository.FindByIdAsync(model.Data.IdCompany)
                              ?? throw new UserMessageException(InfrastructureMessages.Get("CompanyNotFound"));
                var financialAdvisor = await _financialAdvisorRepository.FindByIdAsync(company.IdFinancialAdvisor)
                                       ?? throw new UserMessageException(InfrastructureMessages.Get("CompanyMoveTargetNotFound"));

                var tickets = _ticketRepository.FindAll(t => t.IdCompany == company.Id && t.State != TicketState.Chiuso).ToList();
                foreach (var ticket in tickets)
                {
                    bool dealerChanged = ticket.IdDealer != financialAdvisor.IdDealer;
                    bool nodeChanged = ticket.CurrentLevel == TicketLevel.FinancialAdvisor
                                       || (ticket.CurrentLevel == TicketLevel.Dealer && dealerChanged);

                    ticket.IdFinancialAdvisor = financialAdvisor.Id;
                    ticket.IdDealer = financialAdvisor.IdDealer;

                    if (nodeChanged)
                    {
                        var state = ticket.State == TicketState.InCarico ? TicketState.Aperto : ticket.State;
                        var row = Row(user, ticket, TicketAction.Spostamento, ticket.CurrentLevel, ticket.CurrentRoleId, null, state, null);
                        await _ticketCronologyRepository.CreateAsync(row, false);

                        ticket.CurrentIdAspNetUsers = null;
                        ticket.State = state;
                        ticket.Version++;
                    }

                    await _ticketRepository.UpdateAsync(ticket);

                    if (nodeChanged)
                        await Notify(user, ticket, TicketAction.Spostamento, new List<string>());
                }

                retVal.Riallineati = tickets.Count;

                return retVal;
            }, isSubProcess);
        }

        #endregion


        #region Utente corrente e permessi

        // utente corrente: ruoli (tutti, non solo il primo) e nodi del token
        private class TicketUser
        {
            public string UserId { get; set; } = string.Empty;
            public HashSet<RoleCode> Roles { get; set; } = new HashSet<RoleCode>();
            public int? IdDealer { get; set; }
            public int? IdFinancialAdvisor { get; set; }
            public int? IdCompany { get; set; }

            public bool Has(RoleCode role) => Roles.Contains(role);

            // ha un ruolo di amministrazione del livello (Power o Admin); per User e SuperUser il ruolo stesso
            public bool HasLevel(TicketLevel level) => Roles.Any(r => TicketRules.LevelOfRole(r) == level);

            public bool IsPower(TicketLevel level) => level != TicketLevel.User && Has(TicketRules.PowerRole(level));

            // ruolo con cui l'utente opera a quel livello (Power prima di Admin)
            public RoleCode RoleOfLevel(TicketLevel level)
                => IsPower(level) || level == TicketLevel.User ? TicketRules.PowerRole(level) : TicketRules.AdminRole(level) ?? TicketRules.PowerRole(level);
        }

        private async Task<TicketUser> CurrentTicketUser()
        {
            if (string.IsNullOrEmpty(this.CurrentUserId))
                throw new UserMessageException(InfrastructureMessages.Get("UserNotFound"));

            await LoadRoles();
            var user = new TicketUser
            {
                UserId = this.CurrentUserId,
                Roles = (await UserRoles(new List<string> { this.CurrentUserId })).GetValueOrDefault(this.CurrentUserId) ?? new HashSet<RoleCode>(),
                IdDealer = int.TryParse(this.CurrentDealer, out int d) ? d : null,
                IdFinancialAdvisor = int.TryParse(this.CurrentFinancialAdvisor, out int f) ? f : null,
                IdCompany = int.TryParse(this.CurrentCompany, out int c) ? c : null,
            };
            return user;
        }

        // il ticket appartiene al nodo dell'utente per quel livello
        private static bool NodeMatch(TicketUser user, Ticket ticket, TicketLevel level) => level switch
        {
            TicketLevel.SuperUser or TicketLevel.Dev => true,
            TicketLevel.Dealer => ticket.IdDealer != null && ticket.IdDealer == user.IdDealer,
            TicketLevel.FinancialAdvisor => ticket.IdFinancialAdvisor != null && ticket.IdFinancialAdvisor == user.IdFinancialAdvisor,
            TicketLevel.Company => ticket.IdCompany != null && ticket.IdCompany == user.IdCompany,
            _ => false,
        };

        // ticket aperto come dipendente da chi e' anche PowerAdmin dell'azienda: lo gestiscono gli altri PowerAdmin
        private static bool OwnTicketAsUser(TicketUser user, Ticket ticket)
            => ticket.IdAspNetUsersOpener == user.UserId && ticket.OpenerLevel == TicketLevel.User && ticket.CurrentLevel == TicketLevel.Company;

        // PowerAdmin del nodo del livello corrente
        private static bool IsPowerAt(TicketUser user, Ticket ticket)
            => user.IsPower(ticket.CurrentLevel) && NodeMatch(user, ticket, ticket.CurrentLevel) && !OwnTicketAsUser(user, ticket);

        private static bool IsAssignee(TicketUser user, Ticket ticket)
            => ticket.CurrentIdAspNetUsers == user.UserId;

        // la richiesta di informazioni corrente e' rivolta a chi ha aperto
        private static bool InfoToOpener(Ticket ticket)
            => ticket.State == TicketState.InAttesaInfo && ticket.CurrentLevel == ticket.OpenerLevel && ticket.CurrentIdAspNetUsers == ticket.IdAspNetUsersOpener;

        // ticket in carico: assegnati all'utente o al livello di un nodo di cui e' PowerAdmin
        private IQueryable<Ticket> QueryInCarico(TicketUser user)
        {
            string me = user.UserId;
            bool pSuperUser = user.IsPower(TicketLevel.SuperUser);
            bool pDev = user.IsPower(TicketLevel.Dev);
            bool pDealer = user.IsPower(TicketLevel.Dealer) && user.IdDealer != null;
            bool pFinancialAdvisor = user.IsPower(TicketLevel.FinancialAdvisor) && user.IdFinancialAdvisor != null;
            bool pCompany = user.IsPower(TicketLevel.Company) && user.IdCompany != null;
            int? idDealer = user.IdDealer, idFinancialAdvisor = user.IdFinancialAdvisor, idCompany = user.IdCompany;

            return _ticketRepository.FindAll(t => t.State != TicketState.Chiuso &&
                                                  (t.CurrentIdAspNetUsers == me
                                                   || (pSuperUser && t.CurrentLevel == TicketLevel.SuperUser)
                                                   || (pDev && t.CurrentLevel == TicketLevel.Dev)
                                                   || (pDealer && t.CurrentLevel == TicketLevel.Dealer && t.IdDealer == idDealer)
                                                   || (pFinancialAdvisor && t.CurrentLevel == TicketLevel.FinancialAdvisor && t.IdFinancialAdvisor == idFinancialAdvisor)
                                                   || (pCompany && t.CurrentLevel == TicketLevel.Company && t.IdCompany == idCompany
                                                       && !(t.IdAspNetUsersOpener == me && t.OpenerLevel == TicketLevel.User))));
        }

        // livelli figli (PowerAdmin, solo i tipi abilitati al suo livello) e ticket assegnati in passato; il SuperUser vede tutto
        private IQueryable<Ticket> QueryAltri(TicketUser user)
        {
            if (user.Has(RoleCode.SuperUser))
                return _ticketRepository.FindAll(t => true);

            string me = user.UserId;
            bool pDev = user.IsPower(TicketLevel.Dev);
            bool pDealer = user.IsPower(TicketLevel.Dealer) && user.IdDealer != null;
            bool pFinancialAdvisor = user.IsPower(TicketLevel.FinancialAdvisor) && user.IdFinancialAdvisor != null;
            bool pCompany = user.IsPower(TicketLevel.Company) && user.IdCompany != null;
            int? idDealer = user.IdDealer, idFinancialAdvisor = user.IdFinancialAdvisor, idCompany = user.IdCompany;

            var assegnatiInPassato = _ticketCronologyRepository.FindAll(c => c.UserId == me).Select(c => c.IdTicket);
            var livelli = _ticketTypeLevelRepository.FindAll(l => true);

            return _ticketRepository.FindAll(t => assegnatiInPassato.Contains(t.Id)
                                                  || (pDev && livelli.Any(l => l.IdTicketType == t.IdTicketType && l.Level == TicketLevel.Dev))
                                                  || (pDealer && t.IdDealer == idDealer && livelli.Any(l => l.IdTicketType == t.IdTicketType && l.Level == TicketLevel.Dealer))
                                                  || (pFinancialAdvisor && t.IdFinancialAdvisor == idFinancialAdvisor && livelli.Any(l => l.IdTicketType == t.IdTicketType && l.Level == TicketLevel.FinancialAdvisor))
                                                  || (pCompany && t.IdCompany == idCompany && livelli.Any(l => l.IdTicketType == t.IdTicketType && l.Level == TicketLevel.Company)));
        }

        // ticket visibile all'utente, altrimenti errore
        private async Task<Ticket> VisibleTicket(TicketUser user, int id)
        {
            var ticket = await _ticketRepository.FindByIdAsync(id)
                         ?? throw new UserMessageException(InfrastructureMessages.Get("TicketNotFound"));

            bool visible = ticket.IdAspNetUsersOpener == user.UserId
                           || IsAssignee(user, ticket)
                           || QueryInCarico(user).Any(t => t.Id == id)
                           || QueryAltri(user).Any(t => t.Id == id);

            if (!visible)
                throw new UserMessageException(InfrastructureMessages.Get("TicketNotFound"));

            return ticket;
        }

        // azioni consentite all'utente sul ticket, con assegnatari, destinatari delle informazioni e tipi
        private async Task<TicketActionsModel> Actions(TicketUser user, Ticket ticket, TypeLevels type, List<TicketCronology> cronology)
        {
            var actions = new TicketActionsModel();

            bool power = IsPowerAt(user, ticket);
            bool assignee = IsAssignee(user, ticket);
            bool handler = power || assignee;
            bool superUserTicket = ticket.OpenerLevel == TicketLevel.SuperUser;

            switch (ticket.State)
            {
                case TicketState.Aperto:
                case TicketState.InCarico:
                    actions.Assegna = power;
                    actions.Nota = handler;
                    actions.Risolvi = handler;
                    if (handler && !superUserTicket)
                    {
                        actions.InoltraLevel = TicketRules.NextLevelUp(type.Levels, ticket.CurrentLevel);
                        actions.Inoltra = actions.InoltraLevel != null;
                    }
                    if (handler)
                    {
                        actions.InfoTarget = await InfoTargets(ticket, cronology);
                        actions.RichiediInfo = actions.InfoTarget.Count > 0;
                        actions.TicketType = await ReclassifyTypes(ticket);
                        actions.Riclassifica = actions.TicketType.Count > 0;
                    }
                    break;

                case TicketState.InAttesaInfo:
                    // risponde il destinatario; i PowerAdmin di un livello intermedio possono rispondere o assegnare
                    actions.Rispondi = assignee || (power && !InfoToOpener(ticket));
                    actions.Assegna = power && !InfoToOpener(ticket);
                    break;

                case TicketState.Risolto:
                    actions.Chiudi = assignee;
                    actions.Riapri = assignee;
                    break;
            }

            if (actions.Assegna)
            {
                var users = await NodeUsers(ticket, ticket.CurrentLevel, false);
                // chi ha aperto come dipendente non puo' ricevere il proprio ticket
                if (ticket.OpenerLevel == TicketLevel.User)
                    users.Remove(ticket.IdAspNetUsersOpener);
                actions.Assegnatario = await ToUserModel(users);
                actions.Assegna = actions.Assegnatario.Count > 0;
            }

            return actions;
        }

        // a chi chiedere informazioni: chi ha aperto e i livelli intermedi da cui e' passato il ticket
        private async Task<List<TicketInfoTargetModel>> InfoTargets(Ticket ticket, List<TicketCronology> cronology)
        {
            var targets = new List<TicketInfoTargetModel>
            {
                new TicketInfoTargetModel { Level = ticket.OpenerLevel, IsOpener = true, IdAspNetUsers = ticket.IdAspNetUsersOpener }
            };

            var livelli = cronology.Where(c => (c.Action == TicketAction.Inoltro || c.Action == TicketAction.Assegnazione)
                                               && c.Level > ticket.CurrentLevel && c.Level != ticket.OpenerLevel)
                                   .Select(c => c.Level)
                                   .Distinct()
                                   .OrderBy(l => l);

            foreach (var level in livelli)
            {
                // chi aveva in carico il ticket a quel livello, se puo' ancora riceverlo
                string? lastUser = cronology.LastOrDefault(c => c.Level == level && c.UserId != null)?.UserId;
                targets.Add(new TicketInfoTargetModel { Level = level, IsOpener = false, IdAspNetUsers = await StillEligible(ticket, level, lastUser) });
            }

            var names = UserNames(targets.Where(t => t.IdAspNetUsers != null).Select(t => t.IdAspNetUsers!));
            foreach (var t in targets.Where(t => t.IdAspNetUsers != null))
                t.UserName = names.GetValueOrDefault(t.IdAspNetUsers!);

            return targets;
        }

        #endregion


        #region Apertura e salita

        // ruoli con cui l'utente puo' aprire e tipi disponibili per ciascuno
        private async Task<List<TicketOpenerRoleModel>> OpenerRoles(TicketUser user)
        {
            var levels = new List<TicketLevel>();
            if (user.Has(RoleCode.SuperUser))
                levels.Add(TicketLevel.SuperUser);
            if (user.HasLevel(TicketLevel.Dealer) && user.IdDealer != null)
                levels.Add(TicketLevel.Dealer);
            if (user.HasLevel(TicketLevel.FinancialAdvisor) && user.IdFinancialAdvisor != null)
                levels.Add(TicketLevel.FinancialAdvisor);
            if (user.HasLevel(TicketLevel.Company) && user.IdCompany != null)
                levels.Add(TicketLevel.Company);
            if (user.Has(RoleCode.User) && user.IdCompany != null)
            {
                // l'unico PowerAdmin dell'azienda non apre come dipendente: nessuno potrebbe gestire il ticket
                bool unicoPowerAdmin = user.Has(RoleCode.CompanyPowerAdmin)
                                       && (await CompanyUsers(user.IdCompany.Value, RoleCode.CompanyPowerAdmin)).Count <= 1;
                if (!unicoPowerAdmin)
                    levels.Add(TicketLevel.User);
            }

            var types = await ActiveTypes();
            var applications = user.IdCompany != null ? _companyApplicationRepository.ActiveApplications(user.IdCompany.Value) : new();

            var retVal = new List<TicketOpenerRoleModel>();
            foreach (var level in levels)
            {
                var available = types.Where(t => level == TicketLevel.SuperUser
                                                     ? t.Levels.Contains(TicketLevel.SuperUser) && t.Levels.Contains(TicketLevel.Dev)
                                                     : t.Levels.Contains(level) && TicketRules.NextLevelUp(t.Levels, level) != null)
                                     .Where(t => t.Type.ApplicationType == null
                                                 || (level != TicketLevel.User && level != TicketLevel.Company)
                                                 || applications.Contains(t.Type.ApplicationType.Value))
                                     .Select(t => t.Option)
                                     .ToList();

                if (available.Count > 0)
                    retVal.Add(new TicketOpenerRoleModel { Level = level, TicketType = available });
            }
            return retVal;
        }

        // percorso di origine dal nodo di chi apre (letto dal database: segue gli spostamenti delle aziende)
        private async Task SetOriginPath(TicketUser user, Ticket ticket)
        {
            switch (ticket.OpenerLevel)
            {
                case TicketLevel.User:
                case TicketLevel.Company:
                    {
                        var company = await _companyRepository.FindByIdAsync(user.IdCompany!.Value)
                                      ?? throw new UserMessageException(InfrastructureMessages.Get("NoCompanySelected"));
                        var financialAdvisor = await _financialAdvisorRepository.FindByIdAsync(company.IdFinancialAdvisor);
                        ticket.IdCompany = company.Id;
                        ticket.IdFinancialAdvisor = company.IdFinancialAdvisor;
                        ticket.IdDealer = financialAdvisor?.IdDealer;
                        break;
                    }
                case TicketLevel.FinancialAdvisor:
                    {
                        var financialAdvisor = await _financialAdvisorRepository.FindByIdAsync(user.IdFinancialAdvisor!.Value)
                                               ?? throw new UserMessageException(InfrastructureMessages.Get("TicketOpenerRoleNotAvailable"));
                        ticket.IdFinancialAdvisor = financialAdvisor.Id;
                        ticket.IdDealer = financialAdvisor.IdDealer;
                        break;
                    }
                case TicketLevel.Dealer:
                    ticket.IdDealer = user.IdDealer;
                    break;
            }
        }

        /*
         Righe di un passaggio verso l'alto (apertura o inoltro) fino al livello "next".
         Se il tipo lo prevede (e non si sale dal DEV al SuperUser) si tenta prima la risoluzione AI:
         se risolve, il ticket torna a chi ha aperto come Risolto invece di restare al livello superiore.
        */
        private async Task<List<(TicketCronology row, List<TicketAttachModel> attach)>> SalitaRows(TicketUser user, Ticket ticket, TypeLevels type, TicketLevel next,
                                                                                                    string? text, List<TicketAttachModel> attach, List<TicketCronology> cronology)
        {
            var inoltro = Row(user, ticket, TicketAction.Inoltro, next, RoleId(TicketRules.PowerRole(next)), null, TicketState.Aperto, text);
            var rows = new List<(TicketCronology row, List<TicketAttachModel> attach)> { (inoltro, attach) };

            if (type.Type.AiResolution && next != TicketLevel.SuperUser)
            {
                var ai = await AiResolve(ticket, next, cronology);
                if (ai != null && ai.Resolved)
                {
                    var resolution = new TicketCronology
                    {
                        IdTicket = ticket.Id,
                        TicketNavigation = ticket.Id == 0 ? ticket : null,
                        RoleId = ticket.OpenerRoleId,
                        Level = ticket.OpenerLevel,
                        UserId = ticket.IdAspNetUsersOpener,
                        IdAspNetUsersAuthor = null,
                        Action = TicketAction.RisoluzioneAI,
                        State = TicketState.Risolto,
                        Text = ai.Text,
                        // riaprendo si va al livello sostituito dall'AI, senza assegnatario
                        ReturnLevel = next,
                        ReturnRoleId = inoltro.RoleId,
                        ReturnUserId = null,
                    };
                    rows.Add((resolution, ai.TicketAttach ?? new List<TicketAttachModel>()));
                }
            }

            return rows;
        }

        // risoluzione automatica AI: un errore non blocca mai il passaggio
        private async Task<TicketAiResolveOutModel?> AiResolve(Ticket ticket, TicketLevel next, List<TicketCronology> cronology)
        {
            try
            {
                var request = new GenericRequest<TicketAiResolveInModel>(new TicketAiResolveInModel
                {
                    IdTicket = ticket.Id,
                    IdTicketType = ticket.IdTicketType,
                    DestinationLevel = next,
                    Oggetto = ticket.Oggetto,
                    Text = ticket.Text,
                    TicketCronology = _mapper.Map<List<TicketCronologyModel>>(cronology),
                });
                var res = await _ticketAiResolverService.TicketAiResolve(request, true);
                return res.Data;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Ticket {IdTicket}: risoluzione AI non riuscita, il ticket passa al livello {Level}", ticket.Id, next);
                return null;
            }
        }

        #endregion


        #region Passaggi

        private TicketCronology Row(TicketUser user, Ticket ticket, TicketAction action, TicketLevel level, string roleId, string? userId, TicketState state, string? text)
            => new TicketCronology
            {
                IdTicket = ticket.Id,
                TicketNavigation = ticket.Id == 0 ? ticket : null,
                RoleId = roleId,
                Level = level,
                UserId = userId,
                IdAspNetUsersAuthor = user.UserId,
                Action = action,
                State = state,
                Text = text,
            };

        // aggiunge righe e allegati senza salvare: il salvataggio unico avviene con il ticket
        private async Task AddRows(List<(TicketCronology row, List<TicketAttachModel> attach)> rows)
        {
            foreach (var (row, attach) in rows)
            {
                await _ticketCronologyRepository.CreateAsync(row, false);
                foreach (var a in attach ?? new List<TicketAttachModel>())
                {
                    await _ticketAttachRepository.CreateAsync(new TicketAttach
                    {
                        TicketCronologyNavigation = row,
                        FileName = Path.GetFileName(a.FileName ?? string.Empty),
                        ContentType = string.IsNullOrWhiteSpace(a.ContentType) ? "application/octet-stream" : a.ContentType,
                        Size = a.File?.LongLength ?? 0,
                        File = a.File ?? Array.Empty<byte>(),
                    }, false);
                }
            }
        }

        private static void CheckAttach(List<TicketAttachModel>? attach)
        {
            string? error = TicketRules.AttachError(attach);
            if (error != null)
                throw new UserMessageException(error);
        }

        private static void RequireText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new UserMessageException(InfrastructureMessages.Get("TicketTextRequired"));
        }

        private static UserMessageException NotAllowed()
            => new UserMessageException(InfrastructureMessages.Get("TicketActionNotAllowed"));

        // notifica a chi deve agire ora (assegnatario o PowerAdmin del livello), escluso chi ha eseguito il passaggio
        private async Task Notify(TicketUser user, Ticket ticket, TicketAction action, List<string> extra)
        {
            try
            {
                var recipients = ticket.CurrentIdAspNetUsers != null
                    ? new List<string> { ticket.CurrentIdAspNetUsers }
                    : await NodeUsers(ticket, ticket.CurrentLevel, true);
                recipients.AddRange(extra);
                recipients.RemoveAll(x => x == user.UserId);

                await _ticketNotifier.TicketNotifierAsync(recipients, new TicketNotifierData
                {
                    IdTicket = ticket.Id,
                    Oggetto = ticket.Oggetto,
                    Action = action,
                    State = ticket.State,
                });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Ticket {IdTicket}: notifica non inviata", ticket.Id);
            }
        }

        #endregion


        #region Utenti dei nodi e ruoli

        private async Task LoadRoles()
        {
            if (_roleCodeById != null)
                return;

            var roles = await _aspNetRolesRepository.FindAll();
            _roleCodeById = roles.ToDictionary(r => r.Id, r => r.Code);
            _roleIdByCode = roles.GroupBy(r => r.Code).ToDictionary(g => g.Key, g => g.First().Id);
        }

        private string RoleId(RoleCode code)
            => _roleIdByCode != null && _roleIdByCode.TryGetValue(code, out var id) ? id : code.ToString();

        // ruoli di un gruppo di utenti
        private async Task<Dictionary<string, HashSet<RoleCode>>> UserRoles(List<string> userIds)
        {
            await LoadRoles();
            return _aspNetUserRolesRepository.FindAll(x => userIds.Contains(x.UserId))
                                             .ToList()
                                             .Where(x => _roleCodeById!.ContainsKey(x.RoleId))
                                             .GroupBy(x => x.UserId)
                                             .ToDictionary(g => g.Key, g => g.Select(x => _roleCodeById![x.RoleId]).ToHashSet());
        }

        // utenti con uno dei ruoli indicati
        private async Task<List<string>> UsersWithRole(params RoleCode[] codes)
        {
            await LoadRoles();
            var roleIds = codes.Select(RoleId).ToList();
            return _aspNetUserRolesRepository.FindAll(x => roleIds.Contains(x.RoleId)).Select(x => x.UserId).Distinct().ToList();
        }

        private async Task<List<string>> DevUsers(bool onlyPower)
            => onlyPower ? await UsersWithRole(RoleCode.PowerAdmin) : await UsersWithRole(RoleCode.PowerAdmin, RoleCode.Admin);

        private async Task<List<string>> CompanyUsers(int idCompany, params RoleCode[] codes)
        {
            var linked = _userCompanyRepository.FindAll(x => x.IdCompany == idCompany && !x.Disabled).Select(x => x.IdAspNetUsers).ToList();
            var withRole = await UsersWithRole(codes);
            return linked.Intersect(withRole).ToList();
        }

        // utenti attivi del nodo del ticket a quel livello con il ruolo Power (o anche Admin)
        private async Task<List<string>> NodeUsers(Ticket ticket, TicketLevel level, bool onlyPower)
        {
            var codes = new List<RoleCode> { TicketRules.PowerRole(level) };
            var admin = TicketRules.AdminRole(level);
            if (!onlyPower && admin != null)
                codes.Add(admin.Value);

            switch (level)
            {
                case TicketLevel.SuperUser:
                case TicketLevel.Dev:
                    return await UsersWithRole(codes.ToArray());

                case TicketLevel.Dealer:
                    {
                        if (ticket.IdDealer == null) return new List<string>();
                        var linked = _userDealerRepository.FindAll(x => x.IdDealer == ticket.IdDealer && !x.Disabled).Select(x => x.IdAspNetUsers).ToList();
                        return linked.Intersect(await UsersWithRole(codes.ToArray())).ToList();
                    }

                case TicketLevel.FinancialAdvisor:
                    {
                        if (ticket.IdFinancialAdvisor == null) return new List<string>();
                        var linked = _userFinancialAdvisorRepository.FindAll(x => x.IdFinancialAdvisor == ticket.IdFinancialAdvisor && !x.Disabled).Select(x => x.IdAspNetUsers).ToList();
                        return linked.Intersect(await UsersWithRole(codes.ToArray())).ToList();
                    }

                case TicketLevel.Company:
                    return ticket.IdCompany == null ? new List<string>() : await CompanyUsers(ticket.IdCompany.Value, codes.ToArray());

                default:
                    return new List<string> { ticket.IdAspNetUsersOpener };
            }
        }

        // l'utente puo' ancora ricevere il ticket a quel livello (attivo nel nodo, ruolo giusto); altrimenti null
        private async Task<string?> StillEligible(Ticket ticket, TicketLevel level, string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;
            return (await NodeUsers(ticket, level, false)).Contains(userId) ? userId : null;
        }

        private Dictionary<string, string> UserNames(IEnumerable<string> userIds)
        {
            var ids = userIds.Distinct().ToList();
            return _aspNetUsersRepository.FindAll(x => ids.Contains(x.Id))
                                         .Select(x => new { x.Id, x.UserName })
                                         .ToList()
                                         .ToDictionary(x => x.Id, x => x.UserName ?? string.Empty);
        }

        private async Task<List<TicketUserModel>> ToUserModel(List<string> userIds)
        {
            await Task.Delay(DelayAsyncMethod);
            var names = UserNames(userIds);
            return userIds.Distinct()
                          .Select(id => new TicketUserModel { IdAspNetUsers = id, UserName = names.GetValueOrDefault(id) ?? string.Empty })
                          .OrderBy(u => u.UserName)
                          .ToList();
        }

        #endregion


        #region Tipi di richiesta

        // tipo con i livelli abilitati
        private class TypeLevels
        {
            public TicketType Type { get; set; } = new TicketType();
            public List<TicketLevel> Levels { get; set; } = new List<TicketLevel>();
            public TicketTypeOptionModel Option { get; set; } = new TicketTypeOptionModel();
        }

        private async Task<TypeLevels> TypeWithLevels(int idTicketType)
        {
            var type = await _ticketTypeRepository.FindByIdAsync(idTicketType)
                       ?? throw new UserMessageException(InfrastructureMessages.Get("TicketTypeNotAvailable"));
            return (await ToTypeLevels(new List<TicketType> { type })).First();
        }

        private async Task<List<TypeLevels>> ActiveTypes()
            => await ToTypeLevels(_ticketTypeRepository.FindAll(x => x.Active).OrderBy(x => x.Ordine).ThenBy(x => x.Codice).ToList());

        private async Task<List<TypeLevels>> ToTypeLevels(List<TicketType> types)
        {
            await Task.Delay(DelayAsyncMethod);
            var ids = types.Select(t => t.Id).ToList();
            var levels = _ticketTypeLevelRepository.FindAll(x => ids.Contains(x.IdTicketType)).ToList();

            return types.Select(t => new TypeLevels
            {
                Type = t,
                Levels = levels.Where(l => l.IdTicketType == t.Id).Select(l => l.Level).ToList(),
                Option = new TicketTypeOptionModel
                {
                    Id = t.Id,
                    ApplicationType = t.ApplicationType,
                    Descrizione = t.Descrizione,
                },
            }).ToList();
        }

        // tipi per la riclassificazione: abilitati sia per chi ha aperto sia per il livello corrente
        private async Task<List<TicketTypeOptionModel>> ReclassifyTypes(Ticket ticket)
        {
            var applications = ticket.IdCompany != null ? _companyApplicationRepository.ActiveApplications(ticket.IdCompany.Value) : new();
            bool companyOpener = ticket.OpenerLevel == TicketLevel.User || ticket.OpenerLevel == TicketLevel.Company;

            return (await ActiveTypes())
                .Where(t => t.Type.Id != ticket.IdTicketType)
                .Where(t => ticket.OpenerLevel == TicketLevel.SuperUser
                            ? t.Levels.Contains(TicketLevel.SuperUser) && t.Levels.Contains(TicketLevel.Dev)
                            : t.Levels.Contains(ticket.OpenerLevel) && t.Levels.Contains(ticket.CurrentLevel))
                .Where(t => t.Type.ApplicationType == null || !companyOpener || applications.Contains(t.Type.ApplicationType.Value))
                .Select(t => t.Option)
                .ToList();
        }

        private Dictionary<int, string> TypeDescriptions(IEnumerable<int> idTypes)
        {
            var ids = idTypes.Distinct().ToList();
            return _ticketTypeRepository.FindAll(x => ids.Contains(x.Id))
                                        .Select(x => new { x.Id, x.Descrizione })
                                        .ToList()
                                        .ToDictionary(t => t.Id, t => t.Descrizione);
        }

        #endregion


        #region Modelli

        private List<TicketCronology> Cronology(int idTicket)
            => _ticketCronologyRepository.FindAll(c => c.IdTicket == idTicket).OrderBy(c => c.Id).ToList();

        private async Task<List<TicketListModel>> ToListModel(List<Ticket> tickets)
        {
            await Task.Delay(DelayAsyncMethod);
            var retVal = _mapper.Map<List<TicketListModel>>(tickets);

            var names = UserNames(tickets.Select(t => t.IdAspNetUsersOpener).Concat(tickets.Where(t => t.CurrentIdAspNetUsers != null).Select(t => t.CurrentIdAspNetUsers!)));
            var types = TypeDescriptions(tickets.Select(t => t.IdTicketType));
            foreach (var t in retVal)
            {
                t.OpenerUserName = names.GetValueOrDefault(t.IdAspNetUsersOpener) ?? string.Empty;
                t.CurrentUserName = t.CurrentIdAspNetUsers != null ? names.GetValueOrDefault(t.CurrentIdAspNetUsers) : null;
                t.TipoDescrizione = types.GetValueOrDefault(t.IdTicketType) ?? string.Empty;
            }
            return retVal;
        }

        private async Task<TicketDetailModel> ToDetailModel(TicketUser user, Ticket ticket)
        {
            var retVal = _mapper.Map<TicketDetailModel>(ticket);
            var cronology = Cronology(ticket.Id);

            var cronologyIds = cronology.Select(c => c.Id).ToList();
            // allegati senza il contenuto
            var attach = _ticketAttachRepository.FindAll(a => cronologyIds.Contains(a.IdTicketCronology))
                                                .OrderBy(a => a.Id)
                                                .Select(a => new { a.IdTicketCronology, Model = new TicketAttachModel { Id = a.Id, FileName = a.FileName, ContentType = a.ContentType, Size = a.Size } })
                                                .ToList();

            var userIds = cronology.SelectMany(c => new[] { c.UserId, c.IdAspNetUsersAuthor, c.ReturnUserId })
                                   .Append(ticket.IdAspNetUsersOpener)
                                   .Append(ticket.CurrentIdAspNetUsers)
                                   .Where(x => !string.IsNullOrEmpty(x))
                                   .Select(x => x!);
            var names = UserNames(userIds);
            var types = TypeDescriptions(cronology.Where(c => c.IdTicketType != null).Select(c => c.IdTicketType!.Value).Append(ticket.IdTicketType));

            retVal.OpenerUserName = names.GetValueOrDefault(ticket.IdAspNetUsersOpener) ?? string.Empty;
            retVal.CurrentUserName = ticket.CurrentIdAspNetUsers != null ? names.GetValueOrDefault(ticket.CurrentIdAspNetUsers) : null;
            retVal.TipoDescrizione = types.GetValueOrDefault(ticket.IdTicketType) ?? string.Empty;

            retVal.TicketCronology = _mapper.Map<List<TicketCronologyModel>>(cronology);
            foreach (var c in retVal.TicketCronology)
            {
                c.UserName = c.UserId != null ? names.GetValueOrDefault(c.UserId) : null;
                c.AuthorName = c.IdAspNetUsersAuthor != null ? names.GetValueOrDefault(c.IdAspNetUsersAuthor) : null;
                var row = cronology.First(x => x.Id == c.Id);
                c.ReturnUserName = row.ReturnUserId != null ? names.GetValueOrDefault(row.ReturnUserId) : null;
                c.TipoDescrizione = c.IdTicketType != null ? types.GetValueOrDefault(c.IdTicketType.Value) : null;
                c.TicketAttach = attach.Where(a => a.IdTicketCronology == c.Id).Select(a => a.Model).ToList();
            }

            retVal.Actions = await Actions(user, ticket, await TypeWithLevels(ticket.IdTicketType), cronology);

            return retVal;
        }

        #endregion
    }

    public interface ITicketService : IServiceBase
    {
        Task<GenericResult<TicketListOutModel>> TicketList(GenericRequest<TicketListInModel> model, Boolean isSubProcess);
        Task<GenericResult<TicketCountOutModel>> TicketCount(GenericRequest<TicketCountInModel> model, Boolean isSubProcess);
        Task<GenericResult<TicketGetOutModel>> TicketGet(GenericRequest<TicketGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<TicketOpenInfoOutModel>> TicketOpenInfo(GenericRequest<TicketOpenInfoInModel> model, Boolean isSubProcess);
        Task<GenericResult<TicketOpenOutModel>> TicketOpen(GenericRequest<TicketOpenInModel> model, Boolean isSubProcess);
        Task<GenericResult<TicketActionOutModel>> TicketExecute(GenericRequest<TicketActionInModel> model, Boolean isSubProcess);
        Task<GenericResult<TicketAttachGetOutModel>> TicketAttachGet(GenericRequest<TicketAttachGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<TicketCompanyMoveOutModel>> TicketCompanyMove(GenericRequest<TicketCompanyMoveInModel> model, Boolean isSubProcess);
    }
}
