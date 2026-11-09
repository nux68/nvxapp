using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Repositories.Public;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.License
{
    /// <summary>Stato della licenza di una funzione (ANALISI_LICENZE.md §8.1).</summary>
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
        /// <summary>limite utenti della copertura effettiva; null = illimitati</summary>
        public int? MaxUtenti { get; set; }
        public FonteCopertura Fonte { get; set; }
        public DateTime? DataScadenza { get; set; }
        public int? IdAbbonamento { get; set; }
        public string? DescrizionePiano { get; set; }
        public string? Studio { get; set; }
        /// <summary>true = sistema licenze non attivo, risposta neutra</summary>
        public bool Neutra { get; set; }
    }

    /*
     Punto di verita' delle licenze (ANALISI_LICENZE.md §8.1, §8.1.1, §8.1.2).
     Copertura di una funzione per un'azienda (§4.3):
        1. abbonamento proprio dell'azienda valido (anche Cumulativo)
        2. abbonamento dello studio valido, con DisponibileSuCompany (anche Cumulativo)
        3. se valgono piu' coperture vince il Livello piu' alto (a parita', il limite utenti piu' alto)
     Un abbonamento e' valido se Attivo e oggi <= DataScadenza + giorni di grace (dopo la scadenza: stato Grace).
     Con il sistema licenze non attivo (AppSetting Licenze_Attive) tutte le risposte sono neutre: come Pro, utenti illimitati.
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

        Task<int?> MaxUsersAsync(int idCompany, ApplicationType ambito);
        Task<int> ActiveUsersAsync(int idCompany);
        Task<bool> IsWithinUserLimitAsync(int idCompany, ApplicationType ambito);
        /// <summary>limite piu' alto tra le funzioni coperte dell'azienda (null = illimitati)</summary>
        Task<int?> CompanyUserLimitAsync(int idCompany);
        Task<bool> CanAddUserAsync(int idCompany);

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

        private readonly ILic_AbbonamentoRepository _abbonamentoRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserCompanyRepository _userCompanyRepository;
        private readonly IUserFinancialAdvisorRepository _userFinancialAdvisorRepository;
        private readonly ILicenseSettingsProvider _settingsProvider;

        public LicenseGuard(ILic_AbbonamentoRepository abbonamentoRepository,
                            ICompanyRepository companyRepository,
                            IUserCompanyRepository userCompanyRepository,
                            IUserFinancialAdvisorRepository userFinancialAdvisorRepository,
                            ILicenseSettingsProvider settingsProvider)
        {
            _abbonamentoRepository = abbonamentoRepository;
            _companyRepository = companyRepository;
            _userCompanyRepository = userCompanyRepository;
            _userFinancialAdvisorRepository = userFinancialAdvisorRepository;
            _settingsProvider = settingsProvider;
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

        public async Task<int?> MaxUsersAsync(int idCompany, ApplicationType ambito)
        {
            if (Funzione(ambito) is not AmbitoLicenza a)
                return null;
            var c = await CoverageAsync(idCompany, a);
            return c.Livello == null ? (await _settingsProvider.GetAsync()).MaxUtentiFree : c.MaxUtenti;
        }

        public Task<int> ActiveUsersAsync(int idCompany)
            => _userCompanyRepository.FindAll(x => x.IdCompany == idCompany && !x.Disabled).CountAsync();

        public async Task<bool> IsWithinUserLimitAsync(int idCompany, ApplicationType ambito)
        {
            var max = await MaxUsersAsync(idCompany, ambito);
            return max == null || await ActiveUsersAsync(idCompany) <= max;
        }

        public async Task<int?> CompanyUserLimitAsync(int idCompany)
        {
            var settings = await _settingsProvider.GetAsync();
            if (!settings.Attive)
                return null;
            var coperture = new List<LicenseCoverage>();
            foreach (var a in Funzioni)
                coperture.Add(await CoverageAsync(idCompany, a));
            return LimitePiuAlto(coperture, settings);
        }

        public async Task<bool> CanAddUserAsync(int idCompany)
        {
            var limite = await CompanyUserLimitAsync(idCompany);
            return limite == null || await ActiveUsersAsync(idCompany) + 1 <= limite;
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
        {
            if (Funzione(ambito) is not AmbitoLicenza a)
                return null;
            var c = await StudioCoverageAsync(idFinancialAdvisor, a);
            return c.Livello == null ? (await _settingsProvider.GetAsync()).MaxUtentiFree : c.MaxUtenti;
        }

        public Task<int> StudioActiveUsersAsync(int idFinancialAdvisor)
            => _userFinancialAdvisorRepository.FindAll(x => x.IdFinancialAdvisor == idFinancialAdvisor && !x.Disabled).CountAsync();

        public async Task<int?> StudioUserLimitAsync(int idFinancialAdvisor)
        {
            var settings = await _settingsProvider.GetAsync();
            if (!settings.Attive)
                return null;
            var coperture = new List<LicenseCoverage>();
            foreach (var a in Funzioni)
                coperture.Add(await StudioCoverageAsync(idFinancialAdvisor, a));
            return LimitePiuAlto(coperture, settings);
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

        private static LicenseCoverage Neutra(AmbitoLicenza ambito) => new()
        {
            Ambito = ambito,
            Stato = LicenseState.Licensed,
            Livello = LivelloLicenza.Pro,
            MaxUtenti = null,
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

            // vince il livello piu' alto; a parita' il limite utenti piu' alto (null = illimitati), poi la scadenza piu' lontana
            var migliore = valide.OrderByDescending(x => x.Livello)
                                 .ThenByDescending(x => x.MaxUtentiBloccato ?? int.MaxValue)
                                 .ThenByDescending(x => x.DataScadenza)
                                 .First();
            return new LicenseCoverage
            {
                Ambito = ambito,
                Stato = oggi > migliore.DataScadenza.Date ? LicenseState.Grace : LicenseState.Licensed,
                Livello = migliore.Livello,
                MaxUtenti = migliore.MaxUtentiBloccato,
                Fonte = migliore.IdCompany != null ? FonteCopertura.Propria : FonteCopertura.Studio,
                DataScadenza = migliore.DataScadenza,
                IdAbbonamento = migliore.Id,
                DescrizionePiano = migliore.DescrizionePiano,
                Studio = migliore.IdCompany == null ? migliore.FinancialAdvisorNavigation?.Descrizione : null
            };
        }

        // limite piu' alto tra le funzioni coperte; nessuna funzione coperta = limite Free predefinito
        public static int? LimitePiuAlto(List<LicenseCoverage> coperture, LicenseSettings settings)
        {
            var coperte = coperture.Where(x => x.Livello != null).ToList();
            if (coperte.Count == 0)
                return settings.MaxUtentiFree;
            if (coperte.Any(x => x.MaxUtenti == null))
                return null;
            return coperte.Max(x => x.MaxUtenti);
        }
    }
}
