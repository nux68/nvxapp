using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Calendar;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline
{
    /*
     Scadenze dello scadenzario sul calendario personale (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-CAL.2): le scadenze
     aperte che l'utente vede nello scadenzario (stesse regole di DeadlineService: amministratori dell'azienda tutte, gli altri
     solo quelle di cui sono responsabili, solo applicativi attivi), come eventi di tutto il giorno.
    */
    public class DeadlineCalendarEventProvider : ICalendarEventProvider
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IScadenzaRepository _scadenzaRepository;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;

        public DeadlineCalendarEventProvider(UserManager<ApplicationUser> userManager,
                                             IScadenzaRepository scadenzaRepository,
                                             ICompanyApplicationRepository companyApplicationRepository)
        {
            _userManager = userManager;
            _scadenzaRepository = scadenzaRepository;
            _companyApplicationRepository = companyApplicationRepository;
        }

        public async Task<IEnumerable<CalendarEvent>> EventsAsync(int idCompany, string idAspNetUsers, DateTime dal, DateTime al)
        {
            var user = await _userManager.FindByIdAsync(idAspNetUsers);
            if (user == null)
                return Enumerable.Empty<CalendarEvent>();
            bool admin = await DeadlineService.AmministratoreAsync(_userManager, user);

            var dalGiorno = dal.Date;
            var alGiorno = al.Date;
            var righe = await (await DeadlineService.VisibiliPerUtente(_scadenzaRepository, _companyApplicationRepository, idCompany, idAspNetUsers, admin, false))
                              .Where(x => x.Stato == StatoScadenza.Aperta && x.DataScadenza >= dalGiorno && x.DataScadenza <= alGiorno)
                              .OrderBy(x => x.DataScadenza)
                              .Take(2000)
                              .ToListAsync();

            return righe.Select(x => new CalendarEvent
            {
                Uid = "scadenza-" + x.Id,
                Titolo = InfrastructureMessages.Get("CalendarDeadlineTitle", x.Descrizione),
                Descrizione = x.Note,
                Inizio = x.DataScadenza.Date,
                TuttoIlGiorno = true,
                Url = "/deadlines",
                UltimaModifica = x.ModifiedDate ?? x.CreationDate,
            }).ToList();
        }
    }
}
