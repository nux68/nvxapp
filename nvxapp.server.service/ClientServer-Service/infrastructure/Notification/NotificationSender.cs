using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Email;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Site;
using nvxapp.server.service.HubAI;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Notification
{
    /*
     Notifiche agli utenti (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.4), usabili da ogni applicativo:
        - la notifica si conserva sempre (tabella Notifica): chi non era collegato la trova nella pagina delle notifiche
        - avviso a comparsa nell'app (IUserNotifier) e/o email, secondo la preferenza dell'utente per la categoria
          (NotificaPreferenza; senza preferenza valgono i predefiniti della categoria)
        - a ogni nuova notifica il client riceve l'evento SignalR "NotificationNew" per aggiornare il contatore
     Le categorie le dichiara ogni modulo con un INotificationCategoryProvider (registrato dallo scanner dei servizi):
     la pagina delle preferenze mostra quelle degli applicativi attivi.
    */
    public class NotificationSender : INotificationSender
    {
        public const string EventNew = "NotificationNew";

        private readonly INotificaRepository _notificaRepository;
        private readonly INotificaPreferenzaRepository _preferenzaRepository;
        private readonly IAspNetUsersRepository _aspNetUsersRepository;
        private readonly IUserNotifier _userNotifier;
        private readonly IEmailService _emailService;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly INotificationCategories _categories;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationSender(INotificaRepository notificaRepository,
                                  INotificaPreferenzaRepository preferenzaRepository,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IUserNotifier userNotifier,
                                  IEmailService emailService,
                                  IHubContext<SignalRHub> hubContext,
                                  INotificationCategories categories,
                                  IConfiguration configuration,
                                  IHttpContextAccessor httpContextAccessor)
        {
            _notificaRepository = notificaRepository;
            _preferenzaRepository = preferenzaRepository;
            _aspNetUsersRepository = aspNetUsersRepository;
            _userNotifier = userNotifier;
            _emailService = emailService;
            _hubContext = hubContext;
            _categories = categories;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> SendAsync(NotificationMessage message)
        {
            var utenti = message.UserIds.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            if (utenti.Count == 0 || string.IsNullOrWhiteSpace(message.Titolo))
                return 0;

            var categoria = _categories.Find(message.Categoria) ?? NotificationCategory.Generale;
            var preferenze = await _preferenzaRepository.FindAll(x => utenti.Contains(x.IdAspNetUsers) && x.Categoria == categoria.Code)
                                                        .ToDictionaryAsync(x => x.IdAspNetUsers);
            var email = categoria.EmailAllowed && _emailService.Configured
                        ? await _aspNetUsersRepository.FindAll(x => utenti.Contains(x.Id) && x.Email != null && x.Email != "")
                                                      .ToDictionaryAsync(x => x.Id, x => x.Email!)
                        : new Dictionary<string, string>();
            string? urlState = message.UrlState == null ? null : Taglia(JsonSerializer.Serialize(message.UrlState), 500);

            int inviate = 0;
            foreach (var idUtente in utenti)
            {
                try
                {
                    preferenze.TryGetValue(idUtente, out var pref);
                    bool app = pref?.App ?? categoria.DefaultApp;
                    bool perEmail = categoria.EmailAllowed && (pref?.Email ?? categoria.DefaultEmail) && email.ContainsKey(idUtente);

                    var notifica = await _notificaRepository.CreateAsync(new Notifica
                    {
                        IdAspNetUsers = idUtente,
                        IdCompany = message.IdCompany,
                        Categoria = categoria.Code,
                        Titolo = Taglia(message.Titolo, 200)!,
                        Messaggio = Taglia(message.Messaggio, 1000),
                        Url = Taglia(message.Url, 200),
                        UrlState = urlState,
                    });

                    if (perEmail && await InviaEmailAsync(email[idUtente], message))
                    {
                        var ora = DateTime.Now;
                        await _notificaRepository.FindAll(x => x.Id == notifica.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.DataEmail, ora));
                    }

                    await _hubContext.Clients.User(idUtente).SendAsync(EventNew, new { id = notifica.Id, categoria = categoria.Code });
                    if (app)
                        await _userNotifier.NotifyAsync(new[] { idUtente }, new UserNotificationData
                        {
                            Title = notifica.Titolo,
                            Message = notifica.Messaggio ?? string.Empty,
                            Url = message.Url,
                            State = message.UrlState
                        });
                    inviate++;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Notifica {Categoria} non inviata all'utente {UserId} ({Titolo})", categoria.Code, idUtente, message.Titolo);
                }
            }
            return inviate;
        }

        private async Task<bool> InviaEmailAsync(string indirizzo, NotificationMessage message)
        {
            try
            {
                string sito = PublicUrl();
                string testo = message.Messaggio ?? string.Empty;
                string html = $"<p><b>{WebUtility.HtmlEncode(message.Titolo)}</b></p>"
                            + (string.IsNullOrWhiteSpace(testo) ? "" : $"<p>{WebUtility.HtmlEncode(testo)}</p>")
                            + (string.IsNullOrEmpty(sito) ? "" : $"<p><a href=\"{WebUtility.HtmlEncode(sito)}\">{WebUtility.HtmlEncode(InfrastructureMessages.Get("NotificationEmailOpen", SiteName.Value))}</a></p>");
                string plain = message.Titolo + "\n\n" + testo + (string.IsNullOrEmpty(sito) ? "" : "\n\n" + sito);
                await _emailService.SendAsync(indirizzo, message.Titolo, html, plain);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Email della notifica non inviata a {Email}", indirizzo);
                return false;
            }
        }

        // indirizzo del sito: App:PublicUrl, altrimenti quello della richiesta (fuori da una richiesta puo' mancare)
        private string PublicUrl()
        {
            try { return SiteUrl.Base(_configuration, _httpContextAccessor) ?? string.Empty; }
            catch { return string.Empty; }
        }

        private static string? Taglia(string? s, int max) => s == null ? null : (s.Length > max ? s[..max] : s);
    }

    public interface INotificationSender
    {
        /// <summary>Crea la notifica per ogni utente e la invia secondo le sue preferenze; restituisce quante ne ha create.</summary>
        Task<int> SendAsync(NotificationMessage message);
    }

    /// <summary>Notifica da inviare: testi gia' tradotti (lingua di chi la genera, di solito quella del server).</summary>
    public class NotificationMessage
    {
        public IEnumerable<string> UserIds { get; set; } = Array.Empty<string>();
        public int? IdCompany { get; set; }
        /// <summary>codice di una categoria dichiarata da un INotificationCategoryProvider (sconosciuta = "generale")</summary>
        public string Categoria { get; set; } = NotificationCategory.Generale.Code;
        public string Titolo { get; set; } = string.Empty;
        public string? Messaggio { get; set; }
        /// <summary>pagina del client da aprire e stato da passarle (es. { id: 12 })</summary>
        public string? Url { get; set; }
        public object? UrlState { get; set; }
    }

    // -------------------------------------------------------------------------------------------------
    // Categorie

    /// <summary>Categoria di notifiche: titolo dalla resx (NotificationCategory_&lt;code&gt;) e invio predefinito.</summary>
    public class NotificationCategory
    {
        public string Code { get; init; } = string.Empty;
        /// <summary>applicativo che la dichiara (ApplicationType.Common = funzioni base): la preferenza si mostra se e' attivo</summary>
        public ApplicationType Application { get; init; } = ApplicationType.Common;
        public bool DefaultApp { get; init; } = true;
        public bool DefaultEmail { get; init; }
        /// <summary>false = mai per email (es. avvisi solo interni)</summary>
        public bool EmailAllowed { get; init; } = true;
        public int Ordine { get; init; }

        public string Titolo => InfrastructureMessages.Get("NotificationCategory_" + Code);

        public static readonly NotificationCategory Generale = new() { Code = "generale", Ordine = 0 };
        public static readonly NotificationCategory Scadenze = new() { Code = "scadenze", DefaultEmail = true, Ordine = 10 };
        public static readonly NotificationCategory PreseVisione = new() { Code = "presevisione", DefaultEmail = true, Ordine = 20 };
        // riepilogo AI periodico (INF-AI): l'utente lo puo' disattivare dalle preferenze
        public static readonly NotificationCategory Riepilogo = new() { Code = "riepilogo", DefaultApp = true, DefaultEmail = true, Ordine = 30 };
    }

    /// <summary>Categorie di notifiche di un modulo (ogni ramo ne dichiara una implementazione).</summary>
    public interface INotificationCategoryProvider
    {
        IEnumerable<NotificationCategory> Categories { get; }
    }

    // categorie delle funzioni base
    public class InfrastructureNotificationCategoryProvider : INotificationCategoryProvider
    {
        public IEnumerable<NotificationCategory> Categories => new[]
        {
            NotificationCategory.Generale, NotificationCategory.Scadenze, NotificationCategory.PreseVisione, NotificationCategory.Riepilogo
        };
    }

    /// <summary>Elenco unito delle categorie dichiarate dai moduli.</summary>
    public class NotificationCategories : INotificationCategories
    {
        private readonly List<NotificationCategory> _all;

        public NotificationCategories(IEnumerable<INotificationCategoryProvider> providers)
        {
            _all = providers.SelectMany(p => p.Categories)
                            .GroupBy(c => c.Code)
                            .Select(g => g.First())
                            .OrderBy(c => c.Ordine).ThenBy(c => c.Code)
                            .ToList();
        }

        public IReadOnlyList<NotificationCategory> All => _all;

        public NotificationCategory? Find(string? code) => _all.FirstOrDefault(c => c.Code == code);
    }

    public interface INotificationCategories
    {
        IReadOnlyList<NotificationCategory> All { get; }
        NotificationCategory? Find(string? code);
    }

    // -------------------------------------------------------------------------------------------------
    // Destinatari

    /// <summary>Utenti a cui mandare gli avvisi di un'azienda (es. scadenze senza responsabile).</summary>
    public class CompanyRecipients : ICompanyRecipients
    {
        public static readonly string[] RuoliAmministratori = { nameof(RoleCode.CompanyPowerAdmin), nameof(RoleCode.CompanyAdmin) };

        private readonly IUserCompanyRepository _userCompanyRepository;
        private readonly IAspNetRolesRepository _aspNetRolesRepository;
        private readonly IAspNetUserRolesRepository _aspNetUserRolesRepository;

        public CompanyRecipients(IUserCompanyRepository userCompanyRepository,
                                 IAspNetRolesRepository aspNetRolesRepository,
                                 IAspNetUserRolesRepository aspNetUserRolesRepository)
        {
            _userCompanyRepository = userCompanyRepository;
            _aspNetRolesRepository = aspNetRolesRepository;
            _aspNetUserRolesRepository = aspNetUserRolesRepository;
        }

        public async Task<List<string>> AdminsAsync(int idCompany)
        {
            var idRuoli = await _aspNetRolesRepository.FindAll(r => RuoliAmministratori.Contains(r.Name!)).Select(r => r.Id).ToListAsync();
            var utenti = await _userCompanyRepository.FindAll(x => x.IdCompany == idCompany && !x.Disabled).Select(x => x.IdAspNetUsers).ToListAsync();
            return await _aspNetUserRolesRepository.FindAll(x => idRuoli.Contains(x.RoleId) && utenti.Contains(x.UserId))
                                                   .Select(x => x.UserId).Distinct().ToListAsync();
        }

        public async Task<bool> IsActiveUserAsync(int idCompany, string idAspNetUsers)
            => await _userCompanyRepository.FindAll(x => x.IdCompany == idCompany && x.IdAspNetUsers == idAspNetUsers && !x.Disabled).AnyAsync();
    }

    public interface ICompanyRecipients
    {
        /// <summary>CompanyPowerAdmin e CompanyAdmin attivi dell'azienda.</summary>
        Task<List<string>> AdminsAsync(int idCompany);
        /// <summary>L'utente lavora (non disabilitato) nell'azienda.</summary>
        Task<bool> IsActiveUserAsync(int idCompany, string idAspNetUsers);
    }
}
