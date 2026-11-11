using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Common;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.License
{
    /// <summary>Stato della licenza di una funzione (ANALISI_INFRASTRUCTURE_LICENZE.md §8.1).</summary>
    public enum LicenseState
    {
        NotSubscribed = 0,
        Licensed = 1,
        /// <summary>scaduta da meno dei giorni di grace: vale ancora, si avvisa</summary>
        Grace = 2,
        Expired = 3,
    }

    public enum FonteCopertura
    {
        Nessuna = 0,
        Propria = 1,
        Studio = 2,
    }

    /// <summary>Copertura risolta di una funzione per un'azienda o uno studio.</summary>
    public class LicenseCoverage
    {
        public AmbitoLicenza Ambito { get; set; }
        public LicenseState Stato { get; set; }
        /// <summary>livello effettivo (il piu' alto tra le coperture valide); null = nessuna licenza valida</summary>
        public LivelloLicenza? Livello { get; set; }
        /// <summary>limiti della copertura effettiva (null = illimitati): utenti (Presenze), clienti (CRM), spazio in MB (Documentale)</summary>
        public int? MaxUtenti { get; set; }
        public int? MaxClienti { get; set; }
        public int? MaxSpazioMB { get; set; }
        public FonteCopertura Fonte { get; set; }
        public DateTime? DataScadenza { get; set; }
        public int? IdAbbonamento { get; set; }
        public string? DescrizionePiano { get; set; }
        public string? Studio { get; set; }
        /// <summary>true = sistema licenze non attivo, risposta neutra</summary>
        public bool Neutra { get; set; }
    }

    /// <summary>Quota dello spazio data dalla licenza del Documentale (ANALISI_INFRASTRUCTURE_LICENZE.md §5.1).</summary>
    public class LicenseStorageQuota
    {
        public long QuotaBytes { get; set; }
        public string Descrizione { get; set; } = string.Empty;
    }

    /*
     Punto di verita' delle licenze (ANALISI_INFRASTRUCTURE_LICENZE.md §8.1, §8.1.1, §8.1.2, §5.1).
     Copertura di una funzione per un'azienda (§4.3):
        1. abbonamento proprio dell'azienda valido (anche Cumulativo)
        2. abbonamento dello studio valido, con DisponibileSuCompany (anche Cumulativo)
        3. se valgono piu' coperture vince il Livello piu' alto (a parita', il limite piu' alto della funzione)
     Un abbonamento e' valido se Attivo e oggi <= DataScadenza + giorni di grace (dopo la scadenza: stato Grace).
     Limiti per funzione (§5.1), ognuno vale solo se l'azienda e' coperta per quella funzione:
        - Presenze    -> utenti attivi (creazione e riabilitazione degli utenti)
        - CRM         -> clienti attivi (Az_Cliente attivi di tipo Cliente o Entrambi)
        - Documentale -> spazio dei file (la quota della licenza sostituisce il pacchetto di spazio)
     Con il sistema licenze non attivo (AppSetting Licenze_Attive) tutte le risposte sono neutre: come Pro, senza limiti.
     Le funzioni non soggette a licenza (Common, Moke) risultano sempre licenziate al livello Pro.
     Le funzioni usano HasAtLeastAsync ("serve almeno Standard"); IsFree/IsStandard/IsPro per il livello esatto.
     Per gli studi (funzioni lato studio) ci sono gli stessi metodi con prefisso Studio.
    */
    public interface ILicenseGuard
    {
        Task<bool> EnabledAsync();

        /// <summary>reale = true: copertura effettiva anche con il sistema licenze non attivo (pagine di gestione)</summary>
        Task<LicenseCoverage> CoverageAsync(int idCompany, AmbitoLicenza ambito, bool reale = false);
        Task<LicenseState> StateAsync(int idCompany, ApplicationType ambito);
        Task<LivelloLicenza?> LevelAsync(int idCompany, ApplicationType ambito);
        Task<bool> IsLicensedAsync(int idCompany, ApplicationType ambito);
        Task<bool> IsFreeAsync(int idCompany, ApplicationType ambito);
        Task<bool> IsStandardAsync(int idCompany, ApplicationType ambito);
        Task<bool> IsProAsync(int idCompany, ApplicationType ambito);
        Task<bool> HasAtLeastAsync(int idCompany, ApplicationType ambito, LivelloLicenza minimo);

        // utenti (Presenze)
        /// <summary>limite utenti della funzione: solo le Presenze hanno un limite utenti (null = illimitati o non applicabile)</summary>
        Task<int?> MaxUsersAsync(int idCompany, ApplicationType ambito);
        Task<int> ActiveUsersAsync(int idCompany);
        Task<bool> IsWithinUserLimitAsync(int idCompany, ApplicationType ambito);
        /// <summary>limite utenti dell'azienda = quello della licenza Presenze (null = illimitati, o Presenze non coperte)</summary>
        Task<int?> CompanyUserLimitAsync(int idCompany);
        Task<bool> CanAddUserAsync(int idCompany);

        // clienti (CRM)
        /// <summary>limite clienti = quello della licenza CRM (null = illimitati, o CRM non coperto)</summary>
        Task<int?> ClientLimitAsync(int idCompany);
        Task<int> ActiveClientsAsync(int idCompany);
        Task<bool> CanAddClientAsync(int idCompany);
        /// <summary>Errore per l'utente (UserMessageException) se un cliente in piu' supera il limite della licenza CRM.</summary>
        Task EnsureCanAddClientAsync(int idCompany);
        /// <summary>Il cliente conta per il limite del CRM: attivo e di tipo Cliente o Entrambi (i soli fornitori no).</summary>
        bool ContaComeCliente(Az_Cliente? cliente);

        // spazio (Documentale)
        /// <summary>quota dello spazio data dalla licenza Documentale; null = la licenza non si applica (si usa il pacchetto)</summary>
        Task<LicenseStorageQuota?> StorageQuotaAsync(int idCompany);

        Task<LicenseCoverage> StudioCoverageAsync(int idFinancialAdvisor, AmbitoLicenza ambito, bool reale = false);
        Task<LivelloLicenza?> StudioLevelAsync(int idFinancialAdvisor, ApplicationType ambito);
        Task<bool> StudioIsLicensedAsync(int idFinancialAdvisor, ApplicationType ambito);
        Task<bool> StudioHasAtLeastAsync(int idFinancialAdvisor, ApplicationType ambito, LivelloLicenza minimo);
        Task<int?> StudioMaxUsersAsync(int idFinancialAdvisor, ApplicationType ambito);
        Task<int> StudioActiveUsersAsync(int idFinancialAdvisor);
        Task<int?> StudioUserLimitAsync(int idFinancialAdvisor);
        Task<bool> StudioCanAddUserAsync(int idFinancialAdvisor);
    }

    public class LicenseGuard : ILicenseGuard
    {
        public static readonly AmbitoLicenza[] Funzioni = { AmbitoLicenza.AttendanceTracking, AmbitoLicenza.Crm, AmbitoLicenza.DocumentManagement };

        /// <summary>spazio "illimitato": il massimo che le pagine dello spazio sanno mostrare in MB</summary>
        public const long SpazioIllimitatoBytes = (long)int.MaxValue * 1024L * 1024L;
        private const long MB = 1024L * 1024L;

        private readonly ILic_AbbonamentoRepository _abbonamentoRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserCompanyRepository _userCompanyRepository;
        private readonly IUserFinancialAdvisorRepository _userFinancialAdvisorRepository;
        private readonly ILicenseSettingsProvider _settingsProvider;
        private readonly TenancySettings _tenancySettings;
        private readonly IApplicationDbContextFactory _applicationDbContextFactory;
        private readonly ITenantProvisioningService _tenantProvisioningService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LicenseGuard(ILic_AbbonamentoRepository abbonamentoRepository,
                            ICompanyRepository companyRepository,
                            IUserCompanyRepository userCompanyRepository,
                            IUserFinancialAdvisorRepository userFinancialAdvisorRepository,
                            ILicenseSettingsProvider settingsProvider,
                            TenancySettings tenancySettings,
                            IApplicationDbContextFactory applicationDbContextFactory,
                            ITenantProvisioningService tenantProvisioningService,
                            IServiceProvider serviceProvider,
                            IHttpContextAccessor httpContextAccessor)
        {
            _abbonamentoRepository = abbonamentoRepository;
            _companyRepository = companyRepository;
            _userCompanyRepository = userCompanyRepository;
            _userFinancialAdvisorRepository = userFinancialAdvisorRepository;
            _settingsProvider = settingsProvider;
            _tenancySettings = tenancySettings;
            _applicationDbContextFactory = applicationDbContextFactory;
            _tenantProvisioningService = tenantProvisioningService;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> EnabledAsync() => (await _settingsProvider.GetAsync()).Attive;

        // ---------------------------------------------------------------------------
        // Azienda

        public async Task<LicenseCoverage> CoverageAsync(int idCompany, AmbitoLicenza ambito, bool reale = false)
        {
            var settings = await _settingsProvider.GetAsync();
            if (!settings.Attive && !reale)
                return Neutra(ambito);

            var idStudio = await _companyRepository.FindAll(x => x.Id == idCompany).Select(x => (int?)x.IdFinancialAdvisor).FirstOrDefaultAsync();
            var righe = await _abbonamentoRepository
                .FindAll(x => x.Stato == StatoAbbonamento.Attivo
                              && (x.Ambito == ambito || x.Ambito == AmbitoLicenza.Cumulativo)
                              && (x.IdCompany == idCompany
                                  || (idStudio != null && x.IdFinancialAdvisor == idStudio && x.DisponibileSuCompany)))
                .Include(x => x.FinancialAdvisorNavigation)
                .ToListAsync();
            return Risolvi(ambito, righe, settings);
        }

        public async Task<LicenseState> StateAsync(int idCompany, ApplicationType ambito)
            => Funzione(ambito) is AmbitoLicenza a ? (await CoverageAsync(idCompany, a)).Stato : LicenseState.Licensed;

        public async Task<LivelloLicenza?> LevelAsync(int idCompany, ApplicationType ambito)
            => Funzione(ambito) is AmbitoLicenza a ? (await CoverageAsync(idCompany, a)).Livello : LivelloLicenza.Pro;

        public async Task<bool> IsLicensedAsync(int idCompany, ApplicationType ambito) => await LevelAsync(idCompany, ambito) != null;
        public async Task<bool> IsFreeAsync(int idCompany, ApplicationType ambito) => await LevelAsync(idCompany, ambito) == LivelloLicenza.Free;
        public async Task<bool> IsStandardAsync(int idCompany, ApplicationType ambito) => await LevelAsync(idCompany, ambito) == LivelloLicenza.Standard;
        public async Task<bool> IsProAsync(int idCompany, ApplicationType ambito) => await LevelAsync(idCompany, ambito) == LivelloLicenza.Pro;

        public async Task<bool> HasAtLeastAsync(int idCompany, ApplicationType ambito, LivelloLicenza minimo)
            => await LevelAsync(idCompany, ambito) is LivelloLicenza livello && livello >= minimo;

        // --- utenti (Presenze)

        public async Task<int?> MaxUsersAsync(int idCompany, ApplicationType ambito)
            => ambito == ApplicationType.AttendanceTracking ? await CompanyUserLimitAsync(idCompany) : null;

        public Task<int> ActiveUsersAsync(int idCompany)
            => _userCompanyRepository.FindAll(x => x.IdCompany == idCompany && !x.Disabled).CountAsync();

        public async Task<bool> IsWithinUserLimitAsync(int idCompany, ApplicationType ambito)
        {
            var max = await MaxUsersAsync(idCompany, ambito);
            return max == null || await ActiveUsersAsync(idCompany) <= max;
        }

        public async Task<int?> CompanyUserLimitAsync(int idCompany)
        {
            if (!await EnabledAsync())
                return null;
            var c = await CoverageAsync(idCompany, AmbitoLicenza.AttendanceTracking);
            return c.Livello == null ? null : c.MaxUtenti;
        }

        public async Task<bool> CanAddUserAsync(int idCompany)
        {
            var limite = await CompanyUserLimitAsync(idCompany);
            return limite == null || await ActiveUsersAsync(idCompany) + 1 <= limite;
        }

        // --- clienti (CRM)

        public async Task<int?> ClientLimitAsync(int idCompany)
        {
            if (!await EnabledAsync())
                return null;
            var c = await CoverageAsync(idCompany, AmbitoLicenza.Crm);
            return c.Livello == null ? null : c.MaxClienti;
        }

        // clienti attivi dell'azienda (schema Common): tipo Cliente o Entrambi, i soli fornitori non contano
        public async Task<int> ActiveClientsAsync(int idCompany)
        {
            string schema = TenantSchemaName.Public;
            if (_tenancySettings.MultiTenant)
            {
                schema = TenantSchemaName.For(idCompany, ApplicationType.Common);
                if (!(await _tenantProvisioningService.CompanySchemasAsync(idCompany)).Contains(schema))
                    return 0;
            }
            await using var context = (CommonDbContext)_applicationDbContextFactory.Create(ApplicationType.Common, schema);
            return await new Az_ClienteRepository(context, _serviceProvider, _httpContextAccessor)
                .FindAll(x => x.Az_AnagraficaNavigation!.IdCompany == idCompany && x.Attivo && x.Tipo != TipoCliente.Fornitore)
                .CountAsync();
        }

        public async Task<bool> CanAddClientAsync(int idCompany)
        {
            var limite = await ClientLimitAsync(idCompany);
            return limite == null || await ActiveClientsAsync(idCompany) + 1 <= limite;
        }

        public async Task EnsureCanAddClientAsync(int idCompany)
        {
            if (!await CanAddClientAsync(idCompany))
                throw new UserMessageException(InfrastructureMessages.Get("LicenseClientLimitReached", await ClientLimitAsync(idCompany)));
        }

        public bool ContaComeCliente(Az_Cliente? cliente) => cliente != null && cliente.Attivo && cliente.Tipo != TipoCliente.Fornitore;

        // --- spazio (Documentale)

        public async Task<LicenseStorageQuota?> StorageQuotaAsync(int idCompany)
        {
            if (!await EnabledAsync())
                return null;
            var c = await CoverageAsync(idCompany, AmbitoLicenza.DocumentManagement);
            if (c.Livello == null)
                return null;
            return new LicenseStorageQuota
            {
                QuotaBytes = c.MaxSpazioMB is int mb ? mb * MB : SpazioIllimitatoBytes,
                Descrizione = c.DescrizionePiano ?? c.Livello.ToString()!
            };
        }

        // ---------------------------------------------------------------------------
        // Studio (funzioni lato studio)

        public async Task<LicenseCoverage> StudioCoverageAsync(int idFinancialAdvisor, AmbitoLicenza ambito, bool reale = false)
        {
            var settings = await _settingsProvider.GetAsync();
            if (!settings.Attive && !reale)
                return Neutra(ambito);

            var righe = await _abbonamentoRepository
                .FindAll(x => x.Stato == StatoAbbonamento.Attivo
                              && x.IdFinancialAdvisor == idFinancialAdvisor
                              && (x.Ambito == ambito || x.Ambito == AmbitoLicenza.Cumulativo))
                .ToListAsync();
            var c = Risolvi(ambito, righe, settings);
            if (c.Fonte == FonteCopertura.Studio)
                c.Fonte = FonteCopertura.Propria; // per lo studio e' la sua licenza
            return c;
        }

        public async Task<LivelloLicenza?> StudioLevelAsync(int idFinancialAdvisor, ApplicationType ambito)
            => Funzione(ambito) is AmbitoLicenza a ? (await StudioCoverageAsync(idFinancialAdvisor, a)).Livello : LivelloLicenza.Pro;

        public async Task<bool> StudioIsLicensedAsync(int idFinancialAdvisor, ApplicationType ambito) => await StudioLevelAsync(idFinancialAdvisor, ambito) != null;

        public async Task<bool> StudioHasAtLeastAsync(int idFinancialAdvisor, ApplicationType ambito, LivelloLicenza minimo)
            => await StudioLevelAsync(idFinancialAdvisor, ambito) is LivelloLicenza livello && livello >= minimo;

        public async Task<int?> StudioMaxUsersAsync(int idFinancialAdvisor, ApplicationType ambito)
            => ambito == ApplicationType.AttendanceTracking ? await StudioUserLimitAsync(idFinancialAdvisor) : null;

        public Task<int> StudioActiveUsersAsync(int idFinancialAdvisor)
            => _userFinancialAdvisorRepository.FindAll(x => x.IdFinancialAdvisor == idFinancialAdvisor && !x.Disabled).CountAsync();

        public async Task<int?> StudioUserLimitAsync(int idFinancialAdvisor)
        {
            if (!await EnabledAsync())
                return null;
            var c = await StudioCoverageAsync(idFinancialAdvisor, AmbitoLicenza.AttendanceTracking);
            return c.Livello == null ? null : c.MaxUtenti;
        }

        public async Task<bool> StudioCanAddUserAsync(int idFinancialAdvisor)
        {
            var limite = await StudioUserLimitAsync(idFinancialAdvisor);
            return limite == null || await StudioActiveUsersAsync(idFinancialAdvisor) + 1 <= limite;
        }

        // ---------------------------------------------------------------------------

        /// <summary>Funzione soggetta a licenza corrispondente all'applicativo; null = non soggetto (Common, Moke).</summary>
        public static AmbitoLicenza? Funzione(ApplicationType application)
            => Funzioni.Contains((AmbitoLicenza)(int)application) ? (AmbitoLicenza)(int)application : null;

        /// <summary>Ultimo giorno in cui la licenza vale ancora (scadenza + grace).</summary>
        public static DateTime FineValidita(DateTime dataScadenza, int graceGiorni) => dataScadenza.Date.AddDays(graceGiorni);

        /// <summary>Limite di un abbonamento per la funzione (null = illimitati): utenti, clienti o spazio.</summary>
        public static int? LimiteFunzione(Lic_Abbonamento x, AmbitoLicenza ambito) => ambito switch
        {
            AmbitoLicenza.Crm => x.MaxClientiBloccato,
            AmbitoLicenza.DocumentManagement => x.MaxSpazioMBBloccato,
            _ => x.MaxUtentiBloccato
        };

        private static LicenseCoverage Neutra(AmbitoLicenza ambito) => new()
        {
            Ambito = ambito,
            Stato = LicenseState.Licensed,
            Livello = LivelloLicenza.Pro,
            Neutra = true
        };

        private static LicenseCoverage Risolvi(AmbitoLicenza ambito, List<Lic_Abbonamento> righe, LicenseSettings settings)
        {
            var oggi = DateTime.Today;
            var valide = righe.Where(x => oggi <= FineValidita(x.DataScadenza, settings.GraceGiorni)).ToList();
            if (valide.Count == 0)
            {
                // scaduta: c'era una licenza attiva, ma oltre la grace
                return new LicenseCoverage { Ambito = ambito, Stato = righe.Count > 0 ? LicenseState.Expired : LicenseState.NotSubscribed };
            }

            // vince il livello piu' alto; a parita' il limite piu' alto della funzione (null = illimitati), poi la scadenza piu' lontana
            var migliore = valide.OrderByDescending(x => x.Livello)
                                 .ThenByDescending(x => LimiteFunzione(x, ambito) ?? int.MaxValue)
                                 .ThenByDescending(x => x.DataScadenza)
                                 .First();
            return new LicenseCoverage
            {
                Ambito = ambito,
                Stato = oggi > migliore.DataScadenza.Date ? LicenseState.Grace : LicenseState.Licensed,
                Livello = migliore.Livello,
                MaxUtenti = migliore.MaxUtentiBloccato,
                MaxClienti = migliore.MaxClientiBloccato,
                MaxSpazioMB = migliore.MaxSpazioMBBloccato,
                Fonte = migliore.IdCompany != null ? FonteCopertura.Propria : FonteCopertura.Studio,
                DataScadenza = migliore.DataScadenza,
                IdAbbonamento = migliore.Id,
                DescrizionePiano = migliore.DescrizionePiano,
                Studio = migliore.IdCompany == null ? migliore.FinancialAdvisorNavigation?.Descrizione : null
            };
        }
    }
}
