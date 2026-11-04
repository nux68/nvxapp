using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;
using System.Text.Json;

using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage;
namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion
{
    /*
     Cancellazione fisica di centri (Dealer), studi (FinancialAdvisor), aziende (Company) e utenti
     (vedi nvxapp.server/docs/ANALISI_CANCELLAZIONE.md, decisioni al §9).
        - Eliminando un nodo si elimina tutto il ramo: studi, aziende, schemi degli applicativi, ticket nati nel
          ramo, documenti, collegamenti degli utenti e gli utenti rimasti senza altri collegamenti.
        - Un utente tolto da un nodo si elimina solo se non ha altri collegamenti; i suoi dati negli applicativi
          dell'azienda da cui viene tolto si eliminano (IApplicationDataDeleter).
        - Ticket aperti da un utente eliminato: eliminati; ticket in carico a lui: tornano al loro livello senza assegnatario.
     Prima l'anteprima (DeletionPreview, nessuna modifica), poi l'esecuzione (DeletionExecute) con il nome digitato.
     Gli utenti PowerAdmin, DealerPowerAdmin, FinancialAdvisorPowerAdmin e CompanyPowerAdmin non si eliminano
     singolarmente: quelli di un nodo escono solo eliminando il nodo, il PowerAdmin non si elimina mai.
     Chi puo': Dealer e utenti (eliminazione completa) solo il SuperUser; studio anche il DealerPowerAdmin del centro;
     azienda anche il FinancialAdvisorPowerAdmin dello studio; utente tolto da un nodo anche il PowerAdmin del nodo
     o del nodo padre. Il SuperUser e' l'utente del login (UserIdFirstConnection, anche mentre impersona); gli altri
     ruoli e il nodo sono quelli del token (utente corrente), mai dal client.
     Le modifiche su public e l'eliminazione degli schemi stanno in una sola transazione su PublicDbContext (lo stesso
     contesto di repository e UserManager; in PostgreSQL anche DROP SCHEMA si annulla con il rollback). I dati degli
     applicativi tolti con IApplicationDataDeleter usano altri contesti: si eliminano prima, e si possono ripetere.
    */
    public class DeletionService : ServiceBase, IDeletionService
    {
        private const string RuoloSuperUser = nameof(RoleCode.SuperUser);

        // amministratori principali: non si eliminano da soli (togliendoli da un nodo o dalla lista utenti).
        // Quelli di un nodo escono solo con il nodo; il PowerAdmin (globale, senza nodo) non si elimina mai
        private static readonly string[] RuoliPowerAdmin =
            { nameof(RoleCode.PowerAdmin), nameof(RoleCode.DealerPowerAdmin), nameof(RoleCode.FinancialAdvisorPowerAdmin), nameof(RoleCode.CompanyPowerAdmin) };

        private readonly PublicDbContext _db;
        private readonly ITenantProvisioningService _tenantProvisioningService;
        private readonly TenancySettings _tenancySettings;
        private readonly IEnumerable<IApplicationDataDeleter> _applicationDataDeleters;
        private readonly IFileStorage _fileStorage;

        public DeletionService(IMapper mapper,
                               UserManager<ApplicationUser> userManager,
                               IAspNetUsersRepository aspNetUsersRepository,
                               IOptions<JwtParameter> jwtParameter,
                               IHttpContextAccessor httpContextAccessor,
                               IConfiguration configuration,

                               PublicDbContext db,
                               ITenantProvisioningService tenantProvisioningService,
                               TenancySettings tenancySettings,
                               IEnumerable<IApplicationDataDeleter> applicationDataDeleters,
                               IFileStorage fileStorage) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _db = db;
            _tenantProvisioningService = tenantProvisioningService;
            _tenancySettings = tenancySettings;
            _applicationDataDeleters = applicationDataDeleters;
            _fileStorage = fileStorage;
        }

        public virtual async Task<GenericResult<DeletionPreviewOutModel>> DeletionPreview(GenericRequest<DeletionPreviewInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var piano = await Pianifica(model.Data.Target);

                var retVal = new DeletionPreviewOutModel
                {
                    Descrizione = piano.Descrizione,
                    UsersKept = piano.UtentiCheRestano.Values.OrderBy(x => x).ToList(),
                    CanDelete = piano.Blocco == null,
                    BlockReason = piano.Blocco
                };
                retVal.Counts = await Conteggi(piano);

                if (piano.Aziende.Count > 0)
                    retVal.Warnings.Add(InfrastructureMessages.Get("DeletionWarningExport"));
                retVal.Warnings.Add(InfrastructureMessages.Get("DeletionWarningIrreversible"));

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<DeletionExecuteOutModel>> DeletionExecute(GenericRequest<DeletionExecuteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var piano = await Pianifica(model.Data.Target);
                if (piano.Blocco != null)
                    throw new UserMessageException(piano.Blocco);

                // conferma forte: il nome digitato deve coincidere con quello dell'anteprima
                if (!string.Equals((model.Data.ConfirmText ?? string.Empty).Trim(), piano.Descrizione.Trim(), StringComparison.OrdinalIgnoreCase))
                    throw new UserMessageException(InfrastructureMessages.Get("DeletionConfirmMismatch"));

                var retVal = new DeletionExecuteOutModel { Counts = await Conteggi(piano) };

                // 1. dati degli applicativi con i loro contesti (fuori dalla transazione, ripetibili)
                foreach (var (idCompany, idAspNetUsers) in piano.DatiUtenteDaEliminare)
                    foreach (var deleter in await DeleterAzienda(idCompany))
                        await deleter.DeleteUserDataAsync(idCompany, idAspNetUsers);

                if (!_tenancySettings.MultiTenant)
                    foreach (var idCompany in piano.Aziende)
                        foreach (var deleter in await DeleterAzienda(idCompany))
                            await deleter.DeleteCompanyDataAsync(idCompany);

                // 2. public e schemi degli applicativi, tutto o niente
                await using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    foreach (var idCompany in piano.Aziende)
                        await _tenantProvisioningService.DropCompanySchemasAsync(idCompany);

                    // ticket nati nel ramo o aperti da utenti eliminati (cronologia, allegati e documenti citati in cascata)
                    await TicketDaEliminare(piano).ExecuteDeleteAsync();

                    // ticket in carico agli utenti eliminati: restano al loro livello, senza assegnatario
                    var utentiEliminati = piano.UtentiDaEliminare.ToList();
                    await _db.Ticket.Where(x => x.CurrentIdAspNetUsers != null && utentiEliminati.Contains(x.CurrentIdAspNetUsers))
                                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.CurrentIdAspNetUsers, (string?)null));

                    await _db.CompanyRegistrationRequest.Where(x => x.IdCompany != null && piano.Aziende.Contains(x.IdCompany.Value)).ExecuteDeleteAsync();

                    await _db.UserCompany.Where(x => piano.CollegamentiAzienda.Contains(x.Id)).ExecuteDeleteAsync();
                    await _db.UserFinancialAdvisor.Where(x => piano.CollegamentiStudio.Contains(x.Id)).ExecuteDeleteAsync();
                    await _db.UserDealer.Where(x => piano.CollegamentiCentro.Contains(x.Id)).ExecuteDeleteAsync();

                    // utenti rimasti senza collegamenti (ruoli, claims e login di Identity in cascata)
                    foreach (var idAspNetUsers in utentiEliminati)
                    {
                        var user = await _userManager.FindByIdAsync(idAspNetUsers);
                        if (user == null) continue;
                        var result = await _userManager.DeleteAsync(user);
                        if (!result.Succeeded)
                            throw new InvalidOperationException($"Eliminazione utente {idAspNetUsers}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }

                    // nodi dal basso verso l'alto (applicativi attivi, restrizioni di menu, documenti e impostazioni AI in cascata)
                    await _db.Company.Where(x => piano.Aziende.Contains(x.Id)).ExecuteDeleteAsync();
                    await _db.FinancialAdvisor.Where(x => piano.Studi.Contains(x.Id)).ExecuteDeleteAsync();
                    await _db.Dealer.Where(x => piano.Centri.Contains(x.Id)).ExecuteDeleteAsync();

                    await Registra(piano, retVal.Counts);

                    await transaction.CommitAsync();
                }

                // file delle aziende eliminate (storage su disco): dopo la conferma, perche' non si annullano con il rollback
                foreach (var idCompany in piano.Aziende)
                {
                    try
                    {
                        await _fileStorage.DeleteCompanyAsync(idCompany);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[Deletion] File dell'azienda {IdCompany} non eliminati: vanno tolti a mano dalla cartella dei file.", idCompany);
                    }
                }

                Log.Information("[Deletion] {NodeType} {Descrizione} eliminato da {UserId}: {Riepilogo}",
                    piano.Target.NodeType, piano.Descrizione, UserIdFirstConnection,
                    string.Join(", ", retVal.Counts.Select(c => $"{c.Label} {c.Count}")));

                return retVal;
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------
        // Piano: cosa si elimina (uguale per anteprima ed esecuzione)
        // ---------------------------------------------------------------------------

        private sealed class Piano
        {
            public DeletionTargetModel Target { get; init; } = new DeletionTargetModel();
            public string Descrizione { get; set; } = string.Empty;
            public int? IdNodo { get; set; }
            public string? IdUtente { get; set; }

            public List<int> Centri { get; } = new();
            public List<int> Studi { get; } = new();
            public List<int> Aziende { get; } = new();

            // collegamenti utente-nodo eliminati
            public List<int> CollegamentiCentro { get; } = new();
            public List<int> CollegamentiStudio { get; } = new();
            public List<int> CollegamentiAzienda { get; } = new();

            public HashSet<string> UtentiDaEliminare { get; } = new();
            public Dictionary<string, string> UtentiCheRestano { get; } = new();   // id -> nome utente

            // dati negli applicativi di utenti tolti da un'azienda che resta (azienda, utente)
            public List<(int IdCompany, string IdAspNetUsers)> DatiUtenteDaEliminare { get; } = new();

            // motivo per cui l'eliminazione non e' possibile (null: possibile)
            public string? Blocco { get; set; }
        }

        private async Task<Piano> Pianifica(DeletionTargetModel target)
        {
            var piano = new Piano { Target = target ?? new DeletionTargetModel() };
            var ctx = await Contesto();
            int? id = piano.Target.Id;

            switch (piano.Target.NodeType)
            {
                case DeletionNodeType.Dealer:
                {
                    var dealer = await _db.Dealer.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id) ?? throw NonTrovato();
                    if (!ctx.SuperUser) throw NonConsentito();
                    piano.Descrizione = dealer.Descrizione ?? string.Empty;
                    piano.Centri.Add(dealer.Id);
                    piano.Studi.AddRange(await _db.FinancialAdvisor.Where(x => x.IdDealer == dealer.Id).Select(x => x.Id).ToListAsync());
                    break;
                }
                case DeletionNodeType.FinancialAdvisor:
                {
                    var studio = await _db.FinancialAdvisor.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id) ?? throw NonTrovato();
                    if (!ctx.SuperUser && !ctx.PowerAdminDi(TicketLevel.Dealer, studio.IdDealer)) throw NonConsentito();
                    piano.Descrizione = studio.Descrizione ?? string.Empty;
                    piano.Studi.Add(studio.Id);
                    break;
                }
                case DeletionNodeType.Company:
                {
                    var azienda = await _db.Company.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id) ?? throw NonTrovato();
                    if (!ctx.SuperUser && !ctx.PowerAdminDi(TicketLevel.FinancialAdvisor, azienda.IdFinancialAdvisor)) throw NonConsentito();
                    piano.Descrizione = azienda.Descrizione ?? string.Empty;
                    piano.Aziende.Add(azienda.Id);
                    break;
                }
                case DeletionNodeType.UserDealer:
                {
                    var link = await _db.UserDealer.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id) ?? throw NonTrovato();
                    if (!ctx.SuperUser && !ctx.PowerAdminDi(TicketLevel.Dealer, link.IdDealer)) throw NonConsentito();
                    piano.CollegamentiCentro.Add(link.Id);
                    piano.IdUtente = link.IdAspNetUsers;
                    break;
                }
                case DeletionNodeType.UserFinancialAdvisor:
                {
                    var link = await _db.UserFinancialAdvisor.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id) ?? throw NonTrovato();
                    var idDealer = await _db.FinancialAdvisor.Where(x => x.Id == link.IdFinancialAdvisor).Select(x => x.IdDealer).FirstOrDefaultAsync();
                    if (!ctx.SuperUser && !ctx.PowerAdminDi(TicketLevel.FinancialAdvisor, link.IdFinancialAdvisor) && !ctx.PowerAdminDi(TicketLevel.Dealer, idDealer))
                        throw NonConsentito();
                    piano.CollegamentiStudio.Add(link.Id);
                    piano.IdUtente = link.IdAspNetUsers;
                    break;
                }
                case DeletionNodeType.UserCompany:
                {
                    var link = await _db.UserCompany.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id) ?? throw NonTrovato();
                    var idFinancialAdvisor = await _db.Company.Where(x => x.Id == link.IdCompany).Select(x => x.IdFinancialAdvisor).FirstOrDefaultAsync();
                    if (!ctx.SuperUser && !ctx.PowerAdminDi(TicketLevel.Company, link.IdCompany) && !ctx.PowerAdminDi(TicketLevel.FinancialAdvisor, idFinancialAdvisor))
                        throw NonConsentito();
                    piano.CollegamentiAzienda.Add(link.Id);
                    piano.IdUtente = link.IdAspNetUsers;
                    break;
                }
                case DeletionNodeType.User:
                {
                    if (!ctx.SuperUser) throw NonConsentito();
                    var idUtente = piano.Target.IdAspNetUsers ?? string.Empty;
                    if (await _db.Users.AsNoTracking().AnyAsync(x => x.Id == idUtente) == false) throw NonTrovato();
                    piano.IdUtente = idUtente;
                    piano.UtentiDaEliminare.Add(idUtente);
                    piano.CollegamentiCentro.AddRange(await _db.UserDealer.Where(x => x.IdAspNetUsers == idUtente).Select(x => x.Id).ToListAsync());
                    piano.CollegamentiStudio.AddRange(await _db.UserFinancialAdvisor.Where(x => x.IdAspNetUsers == idUtente).Select(x => x.Id).ToListAsync());
                    piano.CollegamentiAzienda.AddRange(await _db.UserCompany.Where(x => x.IdAspNetUsers == idUtente).Select(x => x.Id).ToListAsync());
                    break;
                }
                default:
                    throw NonTrovato();
            }

            piano.IdNodo = piano.Target.NodeType == DeletionNodeType.User ? null : id;

            // il ramo sotto i nodi e i collegamenti dei loro utenti
            piano.Aziende.AddRange(await _db.Company.Where(x => piano.Studi.Contains(x.IdFinancialAdvisor)).Select(x => x.Id).ToListAsync());
            piano.CollegamentiCentro.AddRange(await _db.UserDealer.Where(x => piano.Centri.Contains(x.IdDealer)).Select(x => x.Id).ToListAsync());
            piano.CollegamentiStudio.AddRange(await _db.UserFinancialAdvisor.Where(x => piano.Studi.Contains(x.IdFinancialAdvisor)).Select(x => x.Id).ToListAsync());
            piano.CollegamentiAzienda.AddRange(await _db.UserCompany.Where(x => piano.Aziende.Contains(x.IdCompany)).Select(x => x.Id).ToListAsync());

            await UtentiCoinvolti(piano, ctx);

            // nome da digitare: per un utente il nome utente
            if (piano.IdUtente != null && piano.Target.NodeType is DeletionNodeType.UserDealer or DeletionNodeType.UserFinancialAdvisor
                                                                   or DeletionNodeType.UserCompany or DeletionNodeType.User)
                piano.Descrizione = await _db.Users.Where(x => x.Id == piano.IdUtente).Select(x => x.UserName).FirstOrDefaultAsync() ?? piano.IdUtente;

            // un amministratore principale non si toglie ne' si elimina da solo
            if (piano.Blocco == null && piano.IdUtente != null && piano.Target.NodeType is DeletionNodeType.UserDealer
                    or DeletionNodeType.UserFinancialAdvisor or DeletionNodeType.UserCompany or DeletionNodeType.User)
            {
                foreach (var ruolo in RuoliPowerAdmin)
                    if ((await UtentiConRuolo(ruolo, new List<string> { piano.IdUtente })).Count > 0)
                    {
                        piano.Blocco = InfrastructureMessages.Get("DeletionPowerAdminUser");
                        break;
                    }
            }

            await VerificaUltimoPowerAdmin(piano);

            return piano;
        }

        // utenti dei collegamenti eliminati: eliminati se non restano collegati altrove, altrimenti restano
        private async Task UtentiCoinvolti(Piano piano, ContestoUtente ctx)
        {
            var linkCentro = await _db.UserDealer.Where(x => piano.CollegamentiCentro.Contains(x.Id)).Select(x => x.IdAspNetUsers).ToListAsync();
            var linkStudio = await _db.UserFinancialAdvisor.Where(x => piano.CollegamentiStudio.Contains(x.Id)).Select(x => x.IdAspNetUsers).ToListAsync();
            var linkAzienda = await _db.UserCompany.Where(x => piano.CollegamentiAzienda.Contains(x.Id))
                                                   .Select(x => new { x.IdAspNetUsers, x.IdCompany }).ToListAsync();

            var utenti = linkCentro.Concat(linkStudio).Concat(linkAzienda.Select(x => x.IdAspNetUsers))
                                   .Where(x => !string.IsNullOrEmpty(x)).Select(x => x!).Distinct().ToList();

            foreach (var utente in utenti)
            {
                bool altriCollegamenti =
                    await _db.UserDealer.AnyAsync(x => x.IdAspNetUsers == utente && !piano.CollegamentiCentro.Contains(x.Id)) ||
                    await _db.UserFinancialAdvisor.AnyAsync(x => x.IdAspNetUsers == utente && !piano.CollegamentiStudio.Contains(x.Id)) ||
                    await _db.UserCompany.AnyAsync(x => x.IdAspNetUsers == utente && !piano.CollegamentiAzienda.Contains(x.Id));

                if (!altriCollegamenti || piano.UtentiDaEliminare.Contains(utente))
                    piano.UtentiDaEliminare.Add(utente);
                else
                    piano.UtentiCheRestano[utente] = await NomeUtente(utente);
            }

            // mai sé stessi ne' il SuperUser
            foreach (var utente in piano.UtentiDaEliminare.ToList())
            {
                bool protetto = utente == ctx.IdUtenteCorrente || utente == ctx.IdUtenteLogin || await IsSuperUser(utente) ||
                                (await UtentiConRuolo(nameof(RoleCode.PowerAdmin), new List<string> { utente })).Count > 0;
                if (!protetto) continue;

                if (utente == piano.IdUtente && piano.Target.NodeType == DeletionNodeType.User)
                {
                    piano.Blocco = utente == ctx.IdUtenteCorrente || utente == ctx.IdUtenteLogin
                        ? InfrastructureMessages.Get("DeletionSelf")
                        : InfrastructureMessages.Get("DeletionSuperUser");
                }
                piano.UtentiDaEliminare.Remove(utente);
                piano.UtentiCheRestano[utente] = await NomeUtente(utente);
            }
            if (piano.Target.NodeType != DeletionNodeType.User && piano.IdUtente != null &&
                (piano.IdUtente == ctx.IdUtenteCorrente || piano.IdUtente == ctx.IdUtenteLogin))
                piano.Blocco = InfrastructureMessages.Get("DeletionSelf");

            // dati negli applicativi delle aziende che restano, per gli utenti che ne vengono tolti
            foreach (var link in linkAzienda.Where(x => !piano.Aziende.Contains(x.IdCompany) && !string.IsNullOrEmpty(x.IdAspNetUsers)))
                piano.DatiUtenteDaEliminare.Add((link.IdCompany, link.IdAspNetUsers!));
        }

        // ogni nodo che resta ha sempre un PowerAdmin attivo: non si toglie l'ultimo
        private async Task VerificaUltimoPowerAdmin(Piano piano)
        {
            if (piano.Blocco != null) return;

            async Task<bool> UltimoPowerAdmin<T>(IQueryable<T> collegamentiNodo, IEnumerable<int> eliminati, string ruolo,
                                                Func<T, string?> utente, Func<T, int> idLink, Func<T, bool> attivo)
            {
                var righe = await collegamentiNodo.ToListAsync();
                var powerAdmin = await UtentiConRuolo(ruolo, righe.Select(utente).Where(x => x != null).Select(x => x!).ToList());
                var eliminatiSet = eliminati.ToHashSet();
                bool tolto = righe.Any(r => eliminatiSet.Contains(idLink(r)) && powerAdmin.Contains(utente(r) ?? string.Empty));
                bool restaAltro = righe.Any(r => !eliminatiSet.Contains(idLink(r)) && attivo(r) && powerAdmin.Contains(utente(r) ?? string.Empty));
                return tolto && !restaAltro;
            }

            foreach (var idDealer in await _db.UserDealer.Where(x => piano.CollegamentiCentro.Contains(x.Id)).Select(x => x.IdDealer).Distinct().ToListAsync())
            {
                if (piano.Centri.Contains(idDealer)) continue;
                if (await UltimoPowerAdmin(_db.UserDealer.AsNoTracking().Where(x => x.IdDealer == idDealer), piano.CollegamentiCentro,
                                           nameof(RoleCode.DealerPowerAdmin), x => x.IdAspNetUsers, x => x.Id, x => !x.Disabled))
                { piano.Blocco = InfrastructureMessages.Get("DeletionLastPowerAdmin"); return; }
            }
            foreach (var idStudio in await _db.UserFinancialAdvisor.Where(x => piano.CollegamentiStudio.Contains(x.Id)).Select(x => x.IdFinancialAdvisor).Distinct().ToListAsync())
            {
                if (piano.Studi.Contains(idStudio)) continue;
                if (await UltimoPowerAdmin(_db.UserFinancialAdvisor.AsNoTracking().Where(x => x.IdFinancialAdvisor == idStudio), piano.CollegamentiStudio,
                                           nameof(RoleCode.FinancialAdvisorPowerAdmin), x => x.IdAspNetUsers, x => x.Id, x => !x.Disabled))
                { piano.Blocco = InfrastructureMessages.Get("DeletionLastPowerAdmin"); return; }
            }
            foreach (var idAzienda in await _db.UserCompany.Where(x => piano.CollegamentiAzienda.Contains(x.Id)).Select(x => x.IdCompany).Distinct().ToListAsync())
            {
                if (piano.Aziende.Contains(idAzienda)) continue;
                if (await UltimoPowerAdmin(_db.UserCompany.AsNoTracking().Where(x => x.IdCompany == idAzienda), piano.CollegamentiAzienda,
                                           nameof(RoleCode.CompanyPowerAdmin), x => x.IdAspNetUsers, x => x.Id, x => !x.Disabled))
                { piano.Blocco = InfrastructureMessages.Get("DeletionLastPowerAdmin"); return; }
            }
        }

        // ---------------------------------------------------------------------------
        // Conteggi e registro
        // ---------------------------------------------------------------------------

        private IQueryable<Ticket> TicketDaEliminare(Piano piano)
        {
            var utenti = piano.UtentiDaEliminare.ToList();
            return _db.Ticket.Where(x => (x.IdDealer != null && piano.Centri.Contains(x.IdDealer.Value)) ||
                                         (x.IdFinancialAdvisor != null && piano.Studi.Contains(x.IdFinancialAdvisor.Value)) ||
                                         (x.IdCompany != null && piano.Aziende.Contains(x.IdCompany.Value)) ||
                                         utenti.Contains(x.IdAspNetUsersOpener));
        }

        private async Task<List<DeletionCountModel>> Conteggi(Piano piano)
        {
            var conteggi = new List<DeletionCountModel>();
            void Aggiungi(string chiave, int n) { if (n > 0) conteggi.Add(new DeletionCountModel { Label = InfrastructureMessages.Get(chiave), Count = n }); }

            Aggiungi("DeletionCountDealers", piano.Centri.Count);
            Aggiungi("DeletionCountFinancialAdvisors", piano.Studi.Count);
            Aggiungi("DeletionCountCompanies", piano.Aziende.Count);
            Aggiungi("DeletionCountUsersDeleted", piano.UtentiDaEliminare.Count);
            Aggiungi("DeletionCountUserLinks", piano.CollegamentiCentro.Count + piano.CollegamentiStudio.Count + piano.CollegamentiAzienda.Count);
            Aggiungi("DeletionCountTickets", await TicketDaEliminare(piano).CountAsync());

            var utenti = piano.UtentiDaEliminare.ToList();
            Aggiungi("DeletionCountTicketsReleased", await _db.Ticket.CountAsync(x => x.CurrentIdAspNetUsers != null && utenti.Contains(x.CurrentIdAspNetUsers)));
            Aggiungi("DeletionCountDocuments", await _db.Document.CountAsync(x => (x.IdDealer != null && piano.Centri.Contains(x.IdDealer.Value)) ||
                                                                                  (x.IdFinancialAdvisor != null && piano.Studi.Contains(x.IdFinancialAdvisor.Value)) ||
                                                                                  (x.IdCompany != null && piano.Aziende.Contains(x.IdCompany.Value))));

            int schemi = 0;
            foreach (var idCompany in piano.Aziende)
                schemi += (await _tenantProvisioningService.CompanySchemasAsync(idCompany)).Count;
            Aggiungi("DeletionCountSchemas", schemi);

            // dati degli applicativi (es. dipendenti), sommati per descrizione
            var applicativi = new List<DeletionCountModel>();
            foreach (var idCompany in piano.Aziende)
                foreach (var deleter in await DeleterAzienda(idCompany))
                    applicativi.AddRange(await deleter.CompanyCountsAsync(idCompany));
            conteggi.AddRange(applicativi.GroupBy(x => x.Label)
                                         .Select(g => new DeletionCountModel { Label = g.Key, Count = g.Sum(x => x.Count) })
                                         .Where(x => x.Count > 0));

            return conteggi;
        }

        // cancellatori degli applicativi che l'azienda ha usato (attivi o disattivati), piu' quello dei dati
        // comuni (Common, sempre presente) per ULTIMO: gli applicativi hanno chiavi esterne verso le sue tabelle
        private async Task<List<IApplicationDataDeleter>> DeleterAzienda(int idCompany)
        {
            var applicativi = await _db.CompanyApplication.Where(x => x.IdCompany == idCompany).Select(x => x.ApplicationType).ToListAsync();
            applicativi.Add(ApplicationType.Common);
            return _applicationDataDeleters.Where(d => applicativi.Contains(d.Application))
                                           .OrderBy(d => d.Application == ApplicationType.Common ? 1 : 0)
                                           .ToList();
        }

        private async Task Registra(Piano piano, List<DeletionCountModel> conteggi)
        {
            var nomeLogin = await NomeUtente(UserIdFirstConnection);
            _db.DeletionLog.Add(new DeletionLog
            {
                NodeType = piano.Target.NodeType,
                NodeId = piano.IdNodo,
                IdAspNetUsersDeleted = piano.Target.NodeType == DeletionNodeType.User ? piano.IdUtente : null,
                Descrizione = piano.Descrizione.Length > 256 ? piano.Descrizione[..256] : piano.Descrizione,
                IdAspNetUsers = UserIdFirstConnection ?? string.Empty,
                UserName = nomeLogin,
                Data = DateTime.Now,
                CreationDate = DateTime.Now,
                ChangeUser = UserIdFirstConnection,
                Riepilogo = JsonSerializer.Serialize(conteggi.ToDictionary(x => x.Label, x => x.Count))
            });
            await _db.SaveChangesAsync();
        }

        // ---------------------------------------------------------------------------
        // Utente corrente e ruoli
        // ---------------------------------------------------------------------------

        private sealed class ContestoUtente
        {
            public bool SuperUser { get; init; }
            public string? IdUtenteLogin { get; init; }
            public string? IdUtenteCorrente { get; init; }
            public HashSet<string> Ruoli { get; init; } = new();
            public int? Dealer { get; init; }
            public int? FinancialAdvisor { get; init; }
            public int? Company { get; init; }

            // PowerAdmin del nodo indicato (ruolo e nodo del token)
            public bool PowerAdminDi(TicketLevel livello, int idNodo) => livello switch
            {
                TicketLevel.Dealer => Ruoli.Contains(nameof(RoleCode.DealerPowerAdmin)) && Dealer == idNodo,
                TicketLevel.FinancialAdvisor => Ruoli.Contains(nameof(RoleCode.FinancialAdvisorPowerAdmin)) && FinancialAdvisor == idNodo,
                TicketLevel.Company => Ruoli.Contains(nameof(RoleCode.CompanyPowerAdmin)) && Company == idNodo,
                _ => false
            };
        }

        private async Task<ContestoUtente> Contesto()
        {
            var corrente = string.IsNullOrEmpty(CurrentUserId) ? null : await _userManager.FindByIdAsync(CurrentUserId);
            if (corrente == null) throw new UserMessageException(InfrastructureMessages.Get("UserNotFound"));

            static int? Id(string? valore) => int.TryParse(valore, out var n) ? n : null;
            return new ContestoUtente
            {
                SuperUser = await IsSuperUser(UserIdFirstConnection),
                IdUtenteLogin = UserIdFirstConnection,
                IdUtenteCorrente = CurrentUserId,
                Ruoli = (await _userManager.GetRolesAsync(corrente)).ToHashSet(),
                Dealer = Id(CurrentDealer),
                FinancialAdvisor = Id(CurrentFinancialAdvisor),
                Company = Id(CurrentCompany)
            };
        }

        private async Task<bool> IsSuperUser(string? idAspNetUsers)
        {
            if (string.IsNullOrEmpty(idAspNetUsers)) return false;
            return (await UtentiConRuolo(RuoloSuperUser, new List<string> { idAspNetUsers })).Count > 0;
        }

        private async Task<HashSet<string>> UtentiConRuolo(string ruolo, List<string> utenti)
        {
            var idRuolo = await _db.Roles.Where(r => r.Name == ruolo).Select(r => r.Id).FirstOrDefaultAsync();
            if (idRuolo == null || utenti.Count == 0) return new HashSet<string>();
            return (await _db.UserRoles.Where(x => x.RoleId == idRuolo && utenti.Contains(x.UserId)).Select(x => x.UserId).ToListAsync()).ToHashSet();
        }

        private async Task<string> NomeUtente(string? idAspNetUsers) =>
            string.IsNullOrEmpty(idAspNetUsers)
                ? string.Empty
                : await _db.Users.Where(x => x.Id == idAspNetUsers).Select(x => x.UserName).FirstOrDefaultAsync() ?? idAspNetUsers;

        private static UserMessageException NonTrovato() => new UserMessageException(InfrastructureMessages.Get("DeletionNodeNotFound"));
        private static UserMessageException NonConsentito() => new UserMessageException(InfrastructureMessages.Get("DeletionNotAllowed"));
    }

    public interface IDeletionService : IServiceBase
    {
        Task<GenericResult<DeletionPreviewOutModel>> DeletionPreview(GenericRequest<DeletionPreviewInModel> model, Boolean isSubProcess);
        Task<GenericResult<DeletionExecuteOutModel>> DeletionExecute(GenericRequest<DeletionExecuteInModel> model, Boolean isSubProcess);
    }
}
