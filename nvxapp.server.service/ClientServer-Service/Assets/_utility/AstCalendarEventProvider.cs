using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Assets;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Calendar;

namespace nvxapp.server.service.ClientServer_Service.Assets._utility
{
    /*
     Prenotazioni sul calendario personale (ANALISI_ASSETS_APPLICATIVO.md, AST-PREN.3; INF-CAL.2): le prenotazioni confermate
     dell'utente (postazioni, sale, auto in pool) diventano eventi del suo feed .ics.
     Il feed arriva SENZA utente collegato: niente contesto della richiesta, lo schema dell'azienda si apre con la factory
     (AstSchema, come i deleter) e i dati si leggono con i repository. Solo con Risorse aziendali attivo per l'azienda.
    */
    public class AstCalendarEventProvider : ICalendarEventProvider
    {
        private readonly TenancySettings _tenancy;
        private readonly IApplicationDbContextFactory _factory;
        private readonly ITenantProvisioningService _provisioning;
        private readonly IServiceProvider _sp;
        private readonly IHttpContextAccessor _http;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;

        public AstCalendarEventProvider(TenancySettings tenancy, IApplicationDbContextFactory factory, ITenantProvisioningService provisioning,
                                        IServiceProvider sp, IHttpContextAccessor http, ICompanyApplicationRepository companyApplicationRepository)
        {
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
            _sp = sp;
            _http = http;
            _companyApplicationRepository = companyApplicationRepository;
        }

        public async Task<IEnumerable<CalendarEvent>> EventsAsync(int idCompany, string idAspNetUsers, DateTime dal, DateTime al)
        {
            if (!await _companyApplicationRepository.FindAll(x => x.IdCompany == idCompany && x.ApplicationType == ApplicationType.Assets && x.Active).AnyAsync())
                return Enumerable.Empty<CalendarEvent>();
            await using var db = await AstSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, CancellationToken.None);
            if (db == null)
                return Enumerable.Empty<CalendarEvent>();
            var rep = new AstRepositorySet(db, _sp, _http);
            var idAz = await rep.Az_Anagrafica.FindAll(x => x.IdCompany == idCompany).Select(x => x.Id).FirstOrDefaultAsync();
            if (idAz == 0)
                return Enumerable.Empty<CalendarEvent>();

            var righe = await rep.Ast_Prenotazione.FindAll(x => x.IdAspNetUsers == idAspNetUsers && x.Stato == AstStatoPrenotazione.Confermata
                                                              && x.Ast_RisorsaNavigation!.IdAz_Anagrafica == idAz
                                                              && x.Inizio >= dal && x.Inizio <= al)
                                                  .Select(x => new
                                                  {
                                                      x.Id, x.Inizio, x.Fine, x.Titolo, x.Note, x.ModifiedDate, x.CreationDate,
                                                      Risorsa = x.Ast_RisorsaNavigation!.Descrizione, x.Ast_RisorsaNavigation.Tipo,
                                                      Sede = x.Ast_RisorsaNavigation.Az_SediNavigation != null ? x.Ast_RisorsaNavigation.Az_SediNavigation.Descrizione : null,
                                                  })
                                                  .OrderBy(x => x.Inizio)
                                                  .Take(2000)
                                                  .ToListAsync();

            return righe.Select(x => new CalendarEvent
            {
                Uid = "ast.prenotazione-" + x.Id,
                Titolo = AssetsMessages.Get("CalendarBookingTitle", x.Risorsa) + (string.IsNullOrEmpty(x.Titolo) ? string.Empty : " - " + x.Titolo),
                Descrizione = string.Join("\n", new[] { x.Sede == null ? null : AssetsMessages.Get("CalendarBookingSite", x.Sede), x.Note }.Where(s => !string.IsNullOrEmpty(s))),
                Inizio = x.Inizio,
                Fine = x.Fine,
                Url = Ast_PrenotazioniService.UrlPrenotazioni,
                UltimaModifica = x.ModifiedDate ?? x.CreationDate,
            }).ToList();
        }
    }
}
