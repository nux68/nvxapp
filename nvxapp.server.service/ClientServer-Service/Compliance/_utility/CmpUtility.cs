using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Compliance;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Compliance;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License;
using System.Security.Claims;

namespace nvxapp.server.service.ClientServer_Service.Compliance._utility
{
    /*
     Chi sta lavorando in Compliance (azienda e ruoli), ANALISI_COMPLIANCE_APPLICATIVO.md §3:
        - CompanyPowerAdmin: configurazione del modulo e tutto il resto
        - CompanyAdmin     : sicurezza sul lavoro (CMP-81) e registro GDPR (CMP-GDPR)
        - User             : i propri corsi, visite (solo esito), DPI ricevuti e nomine
        - utenti abilitati ai dati sanitari (Cmp_Cfg.UtentiDatiSanitari, medico competente / RSPP designato):
          visite mediche con i dati sanitari, anche se non amministratori
        - studio (FinancialAdvisorPowerAdmin/Admin): registro GDPR e scadenze del modulo delle aziende dello studio, se lo
          studio ha Compliance abilitato e l'azienda lo ha attivo. Senza claim "company" nel token l'azienda arriva dalla
          richiesta (IdCompany) ed e' verificata qui; i contesti si aprono sui suoi schemi (TenantScope)
     Le regole valgono lato server per ogni servizio (le pagine le rispecchiano soltanto).
    */
    public sealed class CmpContesto
    {
        public int IdCompany { get; init; }
        public int IdAz_Anagrafica { get; init; }
        public string IdUtente { get; init; } = string.Empty;
        public bool PowerAdmin { get; init; }
        public bool Admin { get; init; }
        public bool User { get; init; }
        /// <summary>utente dello studio che lavora per conto dell'azienda</summary>
        public bool Studio { get; init; }
        /// <summary>abilitato ai dati sanitari delle visite</summary>
        public bool Sanitario { get; init; }
        /// <summary>dipendente collegato all'utente (null se l'utente non e' un dipendente dell'azienda)</summary>
        public int? IdDip_Anagrafica { get; init; }
        public int GiorniPreavviso { get; init; } = 30;
        public bool NotificheAttive { get; init; } = true;

        public bool Amministratore => PowerAdmin || Admin;
        /// <summary>gestisce il registro GDPR: amministratori dell'azienda o studio</summary>
        public bool GestoreGdpr => Amministratore || Studio;
        /// <summary>vede le visite mediche (esito e scadenza): amministratori e utenti abilitati ai dati sanitari</summary>
        public bool VedeVisite => Amministratore || Sanitario;

        /// <summary>per lo studio fissa l'azienda dei contesti degli applicativi (nessun effetto per gli utenti dell'azienda)</summary>
        public IDisposable Scope() => Studio ? TenantScope.Use(IdCompany) : NessunoScope.Instance;

        private sealed class NessunoScope : IDisposable
        {
            public static readonly NessunoScope Instance = new();
            public void Dispose() { }
        }
    }

    public interface ICmpUtility
    {
        /// <summary>
        /// Contesto della richiesta; UserMessageException se non c'e' un'azienda o l'utente non ha un ruolo dell'azienda.
        /// idCompanyStudio: azienda scelta dall'utente dello studio (ignorata per gli utenti dell'azienda).
        /// </summary>
        Task<CmpContesto> ContestoAsync(int? idCompanyStudio = null);

        /// <summary>Configurazione predefinita dell'azienda (se manca).</summary>
        Task EnsureDefaultsAsync(int idAz_Anagrafica);

        Task<Cmp_Cfg> CfgAsync(CmpContesto ctx);

        /// <summary>Aziende dello studio dell'utente con Compliance attivo (vuoto se l'utente non e' dello studio o lo studio non lo ha).</summary>
        Task<List<Company>> AziendeStudioAsync();

        /// <summary>Cifratura dei dati sanitari (DataProtection, chiavi su DB).</summary>
        string? Cifra(string? testo);
        string? Decifra(string? testo);
    }

    public class CmpUtility : ICmpUtility
    {
        private const string ScopoDatiSanitari = "nvxapp.Compliance.DatiSanitari.v1";

        private readonly ICmp_CfgRepository _Cmp_Cfg;
        private readonly ICmp_Dip_AnagraficaReadRepository _Dip_Anagrafica;
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserFinancialAdvisorRepository _userFinancialAdvisorRepository;
        private readonly IFinancialAdvisorApplicationRepository _financialAdvisorApplicationRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly ILicenseGuard _licenseGuard;
        private readonly IDataProtector _protector;

        private CmpContesto? _contesto;

        public CmpUtility(ICmp_CfgRepository cmp_CfgRepository, ICmp_Dip_AnagraficaReadRepository dip_AnagraficaRepository,
                          ICommonCompanyUtility commonCompanyUtility, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager,
                          IUserFinancialAdvisorRepository userFinancialAdvisorRepository, IFinancialAdvisorApplicationRepository financialAdvisorApplicationRepository,
                          ICompanyRepository companyRepository, ICompanyApplicationRepository companyApplicationRepository, ILicenseGuard licenseGuard,
                          IDataProtectionProvider dataProtectionProvider)
        {
            _Cmp_Cfg = cmp_CfgRepository;
            _Dip_Anagrafica = dip_AnagraficaRepository;
            _commonCompanyUtility = commonCompanyUtility;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _userFinancialAdvisorRepository = userFinancialAdvisorRepository;
            _financialAdvisorApplicationRepository = financialAdvisorApplicationRepository;
            _companyRepository = companyRepository;
            _companyApplicationRepository = companyApplicationRepository;
            _licenseGuard = licenseGuard;
            _protector = dataProtectionProvider.CreateProtector(ScopoDatiSanitari);
        }

        public async Task<CmpContesto> ContestoAsync(int? idCompanyStudio = null)
        {
            if (_contesto != null)
            {
                // nella stessa richiesta lo studio non cambia azienda (i contesti sono gia' aperti sui suoi schemi)
                if (_contesto.Studio && idCompanyStudio != _contesto.IdCompany)
                    throw new UserMessageException(ComplianceMessages.Get("NotAllowed"));
                return _contesto;
            }

            var user = _httpContextAccessor.HttpContext?.User;
            var idUtente = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var appUser = string.IsNullOrEmpty(idUtente) ? null : await _userManager.FindByIdAsync(idUtente);
            var ruoli = appUser == null ? new List<string>() : (await _userManager.GetRolesAsync(appUser)).ToList();
            bool powerAdmin = ruoli.Contains(nameof(RoleCode.CompanyPowerAdmin));
            bool admin = ruoli.Contains(nameof(RoleCode.CompanyAdmin));
            bool utente = ruoli.Contains(nameof(RoleCode.User));
            bool studio = ruoli.Contains(nameof(RoleCode.FinancialAdvisorPowerAdmin)) || ruoli.Contains(nameof(RoleCode.FinancialAdvisorAdmin));

            if (!powerAdmin && !admin && !utente)
            {
                if (studio)
                    return _contesto = await ContestoStudioAsync(idUtente, idCompanyStudio);
                throw new UserMessageException(ComplianceMessages.Get("NotAllowed"));
            }

            if (!int.TryParse(user?.FindFirst(TenantSchemaAccessor.CompanyClaim)?.Value, out var idCompany) || idCompany <= 0)
                throw new UserMessageException(InfrastructureMessages.Get("NoCompanySelected"));

            var az = await _commonCompanyUtility.GetAz_AnagraficaAsync(idCompany, true);
            await EnsureDefaultsAsync(az!.Id);
            var cfg = await _Cmp_Cfg.FindAll(x => x.IdAz_Anagrafica == az.Id).FirstAsync();
            var idDip = await _Dip_Anagrafica.FindAll(x => x.IdAz_Anagrafica == az.Id && x.IdAspNetUsers == idUtente).Select(x => (int?)x.Id).FirstOrDefaultAsync();

            _contesto = new CmpContesto
            {
                IdCompany = idCompany,
                IdAz_Anagrafica = az.Id,
                IdUtente = idUtente,
                PowerAdmin = powerAdmin,
                Admin = admin,
                User = utente,
                Sanitario = UtentiSanitari(cfg).Contains(idUtente),
                IdDip_Anagrafica = idDip,
                GiorniPreavviso = cfg.GiorniPreavviso,
                NotificheAttive = cfg.NotificheAttive,
            };
            return _contesto;
        }

        // utente dello studio: l'azienda deve essere dello studio, lo studio deve avere Compliance (abilitato e coperto da licenza)
        // e l'azienda lo deve avere attivo
        private async Task<CmpContesto> ContestoStudioAsync(string idUtente, int? idCompanyStudio)
        {
            if (idCompanyStudio == null || idCompanyStudio <= 0)
                throw new UserMessageException(ComplianceMessages.Get("StudioCompanyRequired"));
            var idStudio = await StudioAsync(idUtente)
                           ?? throw new UserMessageException(ComplianceMessages.Get("NotAllowed"));
            if (!await StudioAbilitatoAsync(idStudio))
                throw new UserMessageException(ComplianceMessages.Get("StudioNotEnabled"));
            int idCompany = idCompanyStudio.Value;
            if (!await _companyRepository.FindAll(x => x.Id == idCompany && x.IdFinancialAdvisor == idStudio).AnyAsync())
                throw new UserMessageException(ComplianceMessages.Get("NotAllowed"));
            if (!_companyApplicationRepository.ActiveApplications(idCompany).Contains(ApplicationType.Compliance))
                throw new UserMessageException(ComplianceMessages.Get("CompanyNotActive"));

            // i contesti (Common e Compliance) si aprono qui sugli schemi dell'azienda e restano su quelli per tutta la richiesta
            using var scope = TenantScope.Use(idCompany);
            var az = await _commonCompanyUtility.GetAz_AnagraficaAsync(idCompany, true);
            await EnsureDefaultsAsync(az!.Id);
            var cfg = await _Cmp_Cfg.FindAll(x => x.IdAz_Anagrafica == az.Id).FirstAsync();
            return new CmpContesto
            {
                IdCompany = idCompany,
                IdAz_Anagrafica = az.Id,
                IdUtente = idUtente,
                Studio = true,
                GiorniPreavviso = cfg.GiorniPreavviso,
                NotificheAttive = cfg.NotificheAttive,
            };
        }

        private async Task<int?> StudioAsync(string idUtente)
            => string.IsNullOrEmpty(idUtente) ? null
               : await _userFinancialAdvisorRepository.FindAll(x => x.IdAspNetUsers == idUtente && !x.Disabled).Select(x => (int?)x.IdFinancialAdvisor).FirstOrDefaultAsync();

        private async Task<bool> StudioAbilitatoAsync(int idStudio)
            => _financialAdvisorApplicationRepository.ActiveApplications(idStudio).Contains(ApplicationType.Compliance)
               && await _licenseGuard.StudioIsLicensedAsync(idStudio, ApplicationType.Compliance);

        public async Task<List<Company>> AziendeStudioAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var idUtente = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var appUser = string.IsNullOrEmpty(idUtente) ? null : await _userManager.FindByIdAsync(idUtente);
            var ruoli = appUser == null ? new List<string>() : (await _userManager.GetRolesAsync(appUser)).ToList();
            if (!ruoli.Contains(nameof(RoleCode.FinancialAdvisorPowerAdmin)) && !ruoli.Contains(nameof(RoleCode.FinancialAdvisorAdmin)))
                throw new UserMessageException(ComplianceMessages.Get("NotAllowed"));
            var idStudio = await StudioAsync(idUtente) ?? throw new UserMessageException(ComplianceMessages.Get("NotAllowed"));
            if (!await StudioAbilitatoAsync(idStudio))
                throw new UserMessageException(ComplianceMessages.Get("StudioNotEnabled"));

            var aziende = await _companyRepository.FindAll(x => x.IdFinancialAdvisor == idStudio).OrderBy(x => x.Descrizione).ToListAsync();
            return aziende.Where(c => _companyApplicationRepository.ActiveApplications(c.Id).Contains(ApplicationType.Compliance)).ToList();
        }

        public async Task EnsureDefaultsAsync(int idAz_Anagrafica)
        {
            if (await _Cmp_Cfg.FindAll(_ => true).AnyAsync(x => x.IdAz_Anagrafica == idAz_Anagrafica))
                return;
            await _Cmp_Cfg.CreateAsync(new Cmp_Cfg { IdAz_Anagrafica = idAz_Anagrafica, CreationDate = DateTime.Now });
        }

        public Task<Cmp_Cfg> CfgAsync(CmpContesto ctx) => _Cmp_Cfg.FindAll(_ => true).FirstAsync(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica);

        public static List<string> UtentiSanitari(Cmp_Cfg cfg)
            => (cfg.UtentiDatiSanitari ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        public string? Cifra(string? testo) => string.IsNullOrWhiteSpace(testo) ? null : _protector.Protect(testo.Trim());

        public string? Decifra(string? testo)
        {
            if (string.IsNullOrEmpty(testo))
                return null;
            try { return _protector.Unprotect(testo); }
            catch (System.Security.Cryptography.CryptographicException) { return null; }
        }
    }
}
