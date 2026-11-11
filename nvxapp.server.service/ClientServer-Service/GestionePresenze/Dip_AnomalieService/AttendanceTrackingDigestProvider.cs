using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Common;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Digest;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnomalieService
{
    /*
     Sezione delle Presenze nel riepilogo AI settimanale (INF-AI, ANALISI_ATTENDANCETRACKING_ANOMALIE_AI.md §6):
     giornate da sistemare prima dell'export paghe (critica), giornate da verificare, ritardi ricorrenti e assenze
     pianificate nei 7 giorni successivi. Chiamato senza utente collegato: i dati si leggono con un contesto creato da
     IApplicationDbContextFactory sullo schema dell'azienda e i repository costruiti su di esso.
     Visibilita': studio (idAspNetUsers null) e CompanyPowerAdmin tutta l'azienda, CompanyAdmin i reparti che amministra,
     altri utenti nessuna sezione.
    */
    public class AttendanceTrackingDigestProvider : IDigestProvider
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TenancySettings _tenancySettings;
        private readonly IApplicationDbContextFactory _applicationDbContextFactory;
        private readonly ITenantProvisioningService _tenantProvisioningService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAspNetRolesRepository _aspNetRolesRepository;
        private readonly IAspNetUserRolesRepository _aspNetUserRolesRepository;

        public AttendanceTrackingDigestProvider(UserManager<ApplicationUser> userManager,
                                                TenancySettings tenancySettings,
                                                IApplicationDbContextFactory applicationDbContextFactory,
                                                ITenantProvisioningService tenantProvisioningService,
                                                IServiceProvider serviceProvider,
                                                IHttpContextAccessor httpContextAccessor,
                                                IAspNetRolesRepository aspNetRolesRepository,
                                                IAspNetUserRolesRepository aspNetUserRolesRepository)
        {
            _userManager = userManager;
            _tenancySettings = tenancySettings;
            _applicationDbContextFactory = applicationDbContextFactory;
            _tenantProvisioningService = tenantProvisioningService;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
            _aspNetRolesRepository = aspNetRolesRepository;
            _aspNetUserRolesRepository = aspNetUserRolesRepository;
        }

        public ApplicationType Application => ApplicationType.AttendanceTracking;

        public async Task<DigestSection?> SectionAsync(int idCompany, string? idAspNetUsers, DateTime dal, DateTime al)
        {
            // vista: azienda intera o reparti amministrati
            bool tutta = true;
            if (idAspNetUsers != null)
            {
                var user = await _userManager.FindByIdAsync(idAspNetUsers);
                if (user == null)
                    return null;
                var ruoli = await _userManager.GetRolesAsync(user);
                if (!ruoli.Contains(nameof(RoleCode.CompanyPowerAdmin)))
                {
                    if (!ruoli.Contains(nameof(RoleCode.CompanyAdmin)))
                        return null;
                    tutta = false;
                }
            }

            string schemaPresenze = TenantSchemaName.Public, schemaComune = TenantSchemaName.Public;
            if (_tenancySettings.MultiTenant)
            {
                schemaPresenze = TenantSchemaName.For(idCompany, ApplicationType.AttendanceTracking);
                schemaComune = TenantSchemaName.For(idCompany, ApplicationType.Common);
                var schemi = await _tenantProvisioningService.CompanySchemasAsync(idCompany);
                if (!schemi.Contains(schemaPresenze) || !schemi.Contains(schemaComune))
                    return null;
            }

            int? idAz_Anagrafica;
            await using (var comune = (CommonDbContext)_applicationDbContextFactory.Create(ApplicationType.Common, schemaComune))
            {
                idAz_Anagrafica = await new Az_AnagraficaRepository(comune, _serviceProvider, _httpContextAccessor)
                                      .FindAll(x => x.IdCompany == idCompany).Select(x => (int?)x.Id).FirstOrDefaultAsync();
            }
            if (idAz_Anagrafica == null)
                return null;

            await using var db = (AttendanceTrackingDbContext)_applicationDbContextFactory.Create(ApplicationType.AttendanceTracking, schemaPresenze);

            var oggi = al.Date;
            var anagrafica = new Dip_AnagraficaRepository(db, _serviceProvider, _httpContextAccessor);
            var query = anagrafica.FindAll(a => a.IdAz_Anagrafica == idAz_Anagrafica.Value && a.IdAspNetUsers != null);
            if (!tutta)
            {
                var repartoUser = new Az_SediRepartoUserRepository(db, _serviceProvider, _httpContextAccessor);
                var reparti = await repartoUser.FindAll(x => x.IdAspNetUsers == idAspNetUsers && x.EnabledToAdmin).Select(x => x.IdAz_SediReparto).ToListAsync();
                var utenti = await repartoUser.FindAll(x => reparti.Contains(x.IdAz_SediReparto) && x.UserInDepartment
                                                            && (x.DataDal == null || x.DataDal <= oggi)
                                                            && (x.DataAl == null || x.DataAl >= oggi))
                                              .Select(x => x.IdAspNetUsers).ToListAsync();
                query = query.Where(a => utenti.Contains(a.IdAspNetUsers!));
            }
            // solo i dipendenti (ruolo User), come le pagine e i widget delle presenze
            var candidati = await query.Select(a => new { a.Id, a.IdAspNetUsers }).ToListAsync();
            var idUtenti = candidati.Select(a => a.IdAspNetUsers!).ToList();
            var ruoloUser = await _aspNetRolesRepository.FindAll(x => x.Code == RoleCode.User).Select(x => x.Id).ToListAsync();
            var conRuolo = (await _aspNetUserRolesRepository.FindAll(x => ruoloUser.Contains(x.RoleId) && idUtenti.Contains(x.UserId))
                                                            .Select(x => x.UserId).ToListAsync()).ToHashSet();
            var dipendenti = candidati.Where(a => conRuolo.Contains(a.IdAspNetUsers!)).Select(a => a.Id).ToList();
            if (dipendenti.Count == 0)
                return null;

            var motore = GestionePresenzeAnomalie.Crea(db, _serviceProvider, _httpContextAccessor);
            var r = motore.Analizza(dipendenti, dal.Date, oggi, oggi);
            var previsione = motore.Prevedi(dipendenti, oggi.AddDays(1), 7);

            var section = new DigestSection
            {
                Codice = "presenze.anomalie",
                Application = ApplicationType.AttendanceTracking,
                Titolo = AttendanceTrackingMessages.Get("DigestSectionAttendance"),
                Ordine = 100,
            };

            if (r.GiornateDaSistemare > 0)
            {
                section.Critica = true;
                section.Righe.Add(new DigestFact
                {
                    Testo = AttendanceTrackingMessages.Get("DigestDaysToFix", r.GiornateDaSistemare, r.DipendentiCoinvolti),
                    Numero = r.GiornateDaSistemare,
                    Critico = true,
                    Dettagli = r.Giornate.Where(g => g.DaSistemare).Take(10)
                                .Select(g => $"{g.Data:dd/MM/yyyy} - {g.NomeDipendente}: {string.Join("; ", g.Anomalie.Where(a => a.Gravita == AnomaliaGravita.Errore).Select(a => a.Descrizione))}")
                                .ToList(),
                });
            }
            if (r.GiornateDaVerificare > 0)
                section.Righe.Add(new DigestFact
                {
                    Testo = AttendanceTrackingMessages.Get("DigestDaysToCheck", r.GiornateDaVerificare),
                    Numero = r.GiornateDaVerificare,
                    Dettagli = r.Conteggi.Where(c => c.Gravita == AnomaliaGravita.Avviso)
                                .Select(c => AttendanceTrackingMessages.Get("Anomalie_ConteggioAvviso", c.Descrizione, c.Numero)).ToList(),
                });
            if (r.RitardiRicorrenti.Count > 0)
                section.Righe.Add(new DigestFact
                {
                    Testo = AttendanceTrackingMessages.Get("DigestLateness", r.RitardiRicorrenti.Count),
                    Numero = r.RitardiRicorrenti.Count,
                    Dettagli = r.RitardiRicorrenti.Take(10)
                                .Select(x => AttendanceTrackingMessages.Get("Anomalie_RitardiRicorrenti", x.NomeDipendente, x.Ritardi, x.RitardoMedioMin)).ToList(),
                });

            int pianificate = previsione.Giorni.Sum(g => g.Pianificate);
            var picchi = previsione.Giorni.Where(g => g.Picco).ToList();
            if (pianificate > 0 || picchi.Count > 0)
                section.Righe.Add(new DigestFact
                {
                    Testo = AttendanceTrackingMessages.Get("DigestPlannedAbsences", pianificate, previsione.Dal.ToString("dd/MM/yyyy"), previsione.Al.ToString("dd/MM/yyyy")),
                    Numero = pianificate,
                    Dettagli = picchi.Take(10)
                                     .Select(g => AttendanceTrackingMessages.Get("DigestAbsencePeak", g.Data.ToString("dd/MM/yyyy"), g.Previste, g.InServizio))
                                     .ToList(),
                });

            return section.Righe.Count == 0 ? null : section;
        }
    }
}
