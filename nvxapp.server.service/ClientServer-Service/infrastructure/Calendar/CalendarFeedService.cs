using System.Security.Cryptography;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Calendar.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Site;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Calendar
{
    /*
     Feed iCalendar (.ics) personale in sola lettura (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-CAL.1-3, decisione D9).
     La sincronizzazione con Google e Microsoft via OAuth richiede app registrate e credenziali (INF-CAL.4, futuro): per ora
     l'utente sottoscrive dal proprio calendario (Google Calendar, Outlook, Apple Calendar) un indirizzo che contiene un token.
        - un feed per utente e azienda (quella corrente): CalendarFeedGet lo crea se manca, CalendarFeedRegenerate cambia il
          token (il vecchio indirizzo smette di funzionare), CalendarFeedDelete lo elimina
        - IcsAsync: scaricamento ANONIMO del feed con il token (i calendari non mandano credenziali). Token sconosciuto = null
          (404). Se l'utente non lavora piu' nell'azienda (collegamento disabilitato o tolto) il calendario e' vuoto.
        - eventi da oggi - 90 giorni a oggi + 365 giorni, chiesti a tutti gli ICalendarEventProvider (uno per modulo)
    */
    public class CalendarFeedService : ServiceBase, ICalendarFeedService
    {
        public const int GiorniPrima = 90;
        public const int GiorniDopo = 365;

        private readonly ICalendarioFeedRepository _calendarioFeedRepository;
        private readonly IUserCompanyRepository _userCompanyRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IEnumerable<ICalendarEventProvider> _providers;

        public CalendarFeedService(IMapper mapper,
                                   UserManager<ApplicationUser> userManager,
                                   IAspNetUsersRepository aspNetUsersRepository,
                                   IOptions<JwtParameter> jwtParameter,
                                   IHttpContextAccessor httpContextAccessor,
                                   IConfiguration configuration,

                                   ICalendarioFeedRepository calendarioFeedRepository,
                                   IUserCompanyRepository userCompanyRepository,
                                   ICompanyRepository companyRepository,
                                   IEnumerable<ICalendarEventProvider> providers) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _calendarioFeedRepository = calendarioFeedRepository;
            _userCompanyRepository = userCompanyRepository;
            _companyRepository = companyRepository;
            _providers = providers;
        }

        public virtual async Task<GenericResult<CalendarFeedGetOutModel>> CalendarFeedGet(GenericRequest<CalendarFeedGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new CalendarFeedGetOutModel();
                var idCompany = await Contesto();
                var io = this.CurrentUserId;

                var feed = await _calendarioFeedRepository.FindAll(x => x.IdAspNetUsers == io && x.IdCompany == idCompany).FirstOrDefaultAsync();
                if (feed == null && (model.Data?.Crea ?? true))
                    feed = await _calendarioFeedRepository.CreateAsync(new CalendarioFeed
                    {
                        IdAspNetUsers = io,
                        IdCompany = idCompany,
                        Token = NuovoToken(),
                        DataCreazione = DateTime.Now
                    });
                retVal.Feed = feed == null ? null : Modello(feed);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<CalendarFeedRegenerateOutModel>> CalendarFeedRegenerate(GenericRequest<CalendarFeedRegenerateInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new CalendarFeedRegenerateOutModel();
                var idCompany = await Contesto();
                var io = this.CurrentUserId;

                var feed = await _calendarioFeedRepository.FindAll(x => x.IdAspNetUsers == io && x.IdCompany == idCompany).FirstOrDefaultAsync();
                if (feed == null)
                    feed = await _calendarioFeedRepository.CreateAsync(new CalendarioFeed
                    {
                        IdAspNetUsers = io,
                        IdCompany = idCompany,
                        Token = NuovoToken(),
                        DataCreazione = DateTime.Now
                    });
                else
                {
                    feed.Token = NuovoToken();
                    feed.DataCreazione = DateTime.Now;
                    feed.DataUltimoAccesso = null;
                    feed = await _calendarioFeedRepository.UpdateAsync(feed);
                }
                retVal.Feed = Modello(feed);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<CalendarFeedDeleteOutModel>> CalendarFeedDelete(GenericRequest<CalendarFeedDeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new CalendarFeedDeleteOutModel();
                var idCompany = await Contesto();
                var io = this.CurrentUserId;

                var feed = await _calendarioFeedRepository.FindAll(x => x.IdAspNetUsers == io && x.IdCompany == idCompany).FirstOrDefaultAsync();
                if (feed != null)
                    await _calendarioFeedRepository.DeleteAsync(feed);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<string?> IcsAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token) || token.Length > 64)
                return null;
            var feed = await _calendarioFeedRepository.FindAll(x => x.Token == token).FirstOrDefaultAsync();
            if (feed == null)
                return null;

            await _calendarioFeedRepository.SegnaAccessoAsync(feed.Id, DateTime.Now);

            var azienda = await _companyRepository.FindAll(x => x.Id == feed.IdCompany).Select(x => x.Descrizione).FirstOrDefaultAsync();
            var nome = InfrastructureMessages.Get("CalendarFeedName", azienda ?? string.Empty);

            var eventi = new List<CalendarEvent>();
            bool attivo = await _userCompanyRepository.FindAll(x => x.IdCompany == feed.IdCompany && x.IdAspNetUsers == feed.IdAspNetUsers && !x.Disabled).AnyAsync();
            if (attivo)
            {
                var dal = DateTime.Today.AddDays(-GiorniPrima);
                var al = DateTime.Today.AddDays(GiorniDopo);
                foreach (var provider in _providers)
                {
                    try
                    {
                        eventi.AddRange(await provider.EventsAsync(feed.IdCompany, feed.IdAspNetUsers, dal, al));
                    }
                    catch (Exception ex)
                    {
                        // un modulo in errore non deve togliere gli eventi degli altri
                        Log.Error(ex, "Feed del calendario: eventi di {Provider} non letti (azienda {IdCompany})", provider.GetType().Name, feed.IdCompany);
                    }
                }
            }

            // link degli eventi verso le pagine del client: solo con l'indirizzo pubblico configurato (la richiesta arriva dal calendario, non dal sito)
            string? sito = _configuration["App:PublicUrl"];
            return IcsWriter.Scrivi(nome, eventi.OrderBy(x => x.Inizio).ThenBy(x => x.Uid), string.IsNullOrWhiteSpace(sito) ? null : sito, DateTime.UtcNow);
        }

        // ---------------------------------------------------------------------------

        // azienda corrente, in cui l'utente deve lavorare
        private async Task<int> Contesto()
        {
            if (!int.TryParse(this.CurrentCompany, out int idCompany) || idCompany <= 0)
                throw new UserMessageException(InfrastructureMessages.Get("CalendarFeedNoCompany"));
            var io = this.CurrentUserId;
            if (string.IsNullOrEmpty(io) || !await _userCompanyRepository.FindAll(x => x.IdCompany == idCompany && x.IdAspNetUsers == io && !x.Disabled).AnyAsync())
                throw new UserMessageException(InfrastructureMessages.Get("CalendarFeedNotAllowed"));
            return idCompany;
        }

        private CalendarFeedModel Modello(CalendarioFeed feed)
        {
            string url = SiteUrl.ApiBase(_configuration, _httpContextAccessor) + "/api/CalendarFeed/" + feed.Token + ".ics";
            int schema = url.IndexOf("://", StringComparison.Ordinal);
            return new CalendarFeedModel
            {
                Url = url,
                WebcalUrl = schema > 0 ? "webcal" + url.Substring(schema) : url,
                DataCreazione = feed.DataCreazione,
                DataUltimoAccesso = feed.DataUltimoAccesso
            };
        }

        // 32 byte casuali in base64url (43 caratteri, nessun carattere da codificare nell'indirizzo)
        public static string NuovoToken()
            => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public interface ICalendarFeedService : IServiceBase
    {
        Task<GenericResult<CalendarFeedGetOutModel>> CalendarFeedGet(GenericRequest<CalendarFeedGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<CalendarFeedRegenerateOutModel>> CalendarFeedRegenerate(GenericRequest<CalendarFeedRegenerateInModel> model, Boolean isSubProcess);
        Task<GenericResult<CalendarFeedDeleteOutModel>> CalendarFeedDelete(GenericRequest<CalendarFeedDeleteInModel> model, Boolean isSubProcess);
        /// <summary>contenuto del feed .ics (richiesta anonima); null = token sconosciuto</summary>
        Task<string?> IcsAsync(string token);
    }
}
