using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.HumanResources;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.HumanResources;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License;
using System.Security.Claims;

namespace nvxapp.server.service.ClientServer_Service.HumanResources._utility
{
    /*
     Chi sta lavorando in Risorse Umane (azienda e ruoli), ANALISI_HUMANRESOURCES_APPLICATIVO.md §3:
        - CompanyPowerAdmin: configurazione del modulo e tutto il resto
        - CompanyAdmin     : cedolini, corsi, checklist e fascicoli di tutti i dipendenti (ufficio del personale)
        - User             : area del dipendente (solo i propri dati)
        - FinancialAdvisorPowerAdmin/Admin (studio paghe): caricamento e verifica dei cedolini delle aziende dello
                             studio con Risorse Umane attivo (pagine lato studio), mai i fascicoli
     Le regole valgono lato server per ogni servizio (le pagine le rispecchiano soltanto).
    */
    public sealed class HrContesto
    {
        public int IdCompany { get; init; }
        public int IdAz_Anagrafica { get; init; }
        public string IdUtente { get; init; } = string.Empty;
        public bool PowerAdmin { get; init; }
        public bool Admin { get; init; }
        public bool User { get; init; }
        // utente dello studio che lavora sui cedolini di un'azienda dello studio
        public bool Studio { get; init; }
        public int? IdFinancialAdvisor { get; init; }

        public bool Amministratore => PowerAdmin || Admin;
        // carica, verifica e pubblica i cedolini
        public bool GestioneCedolini => Amministratore || Studio;
    }

    public interface IHrUtility
    {
        /// <summary>Contesto della richiesta; UserMessageException se non c'e' un'azienda o l'utente non ha un ruolo dell'azienda.</summary>
        Task<HrContesto> ContestoAsync();

        /// <summary>Come ContestoAsync, ma richiede un amministratore dell'azienda.</summary>
        Task<HrContesto> AmministratoreAsync();

        /// <summary>
        /// Contesto per i cedolini: l'amministratore dell'azienda del token, oppure l'utente dello studio sull'azienda indicata
        /// (dello studio, con Risorse Umane attivo e abilitato per lo studio). Il contesto va usato in HrAzienda.RunAsync.
        /// </summary>
        Task<HrContesto> ContestoCedoliniAsync(int? idCompany);

        /// <summary>Aziende dello studio dell'utente con Risorse Umane attivo (errore se l'utente non e' di uno studio abilitato).</summary>
        Task<List<(int IdCompany, string Descrizione)>> AziendeStudioAsync();

        /// <summary>Configurazione predefinita dell'azienda (se manca).</summary>
        Task EnsureDefaultsAsync(int idAz_Anagrafica);

        Task<Hr_Cfg> CfgAsync(HrContesto ctx);

        /// <summary>Dipendente collegato all'utente corrente (null se l'utente non e' un dipendente dell'azienda).</summary>
        Task<Dip_Anagrafica?> DipendenteCorrenteAsync(HrContesto ctx);

        /// <summary>Dipendente dell'azienda; UserMessageException se non esiste.</summary>
        Task<Dip_Anagrafica> DipendenteAsync(HrContesto ctx, int idDip_Anagrafica);
    }

    public class HrUtility : IHrUtility
    {
        public static readonly string[] RuoliStudio = { nameof(RoleCode.FinancialAdvisorPowerAdmin), nameof(RoleCode.FinancialAdvisorAdmin) };

        private readonly IHr_CfgRepository _Hr_Cfg;
        private readonly IHr_Dip_AnagraficaReadRepository _Dip_Anagrafica;
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly IFinancialAdvisorApplicationRepository _financialAdvisorApplicationRepository;
        private readonly ILicenseGuard _licenseGuard;

        private HrContesto? _contesto;

        public HrUtility(IHr_CfgRepository hr_CfgRepository, IHr_Dip_AnagraficaReadRepository dip_AnagraficaRepository,
                          ICommonCompanyUtility commonCompanyUtility, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager,
                          ICompanyRepository companyRepository, ICompanyApplicationRepository companyApplicationRepository,
                          IFinancialAdvisorApplicationRepository financialAdvisorApplicationRepository, ILicenseGuard licenseGuard)
        {
            _Hr_Cfg = hr_CfgRepository;
            _Dip_Anagrafica = dip_AnagraficaRepository;
            _commonCompanyUtility = commonCompanyUtility;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _companyRepository = companyRepository;
            _companyApplicationRepository = companyApplicationRepository;
            _financialAdvisorApplicationRepository = financialAdvisorApplicationRepository;
            _licenseGuard = licenseGuard;
        }

        private string IdUtente => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        private async Task<List<string>> RuoliAsync()
        {
            var appUser = string.IsNullOrEmpty(IdUtente) ? null : await _userManager.FindByIdAsync(IdUtente);
            return appUser == null ? new List<string>() : (await _userManager.GetRolesAsync(appUser)).ToList();
        }

        public async Task<HrContesto> ContestoAsync()
        {
            if (_contesto != null)
                return _contesto;

            var user = _httpContextAccessor.HttpContext?.User;
            if (!int.TryParse(user?.FindFirst(TenantSchemaAccessor.CompanyClaim)?.Value, out var idCompany) || idCompany <= 0)
                throw new UserMessageException(InfrastructureMessages.Get("NoCompanySelected"));

            var ruoli = await RuoliAsync();
            bool powerAdmin = ruoli.Contains(nameof(RoleCode.CompanyPowerAdmin));
            bool admin = ruoli.Contains(nameof(RoleCode.CompanyAdmin));
            bool utente = ruoli.Contains(nameof(RoleCode.User));
            if (!powerAdmin && !admin && !utente)
                throw new UserMessageException(HumanResourcesMessages.Get("NotAllowed"));

            var az = await _commonCompanyUtility.GetAz_AnagraficaAsync(idCompany, true);
            await EnsureDefaultsAsync(az!.Id);

            _contesto = new HrContesto
            {
                IdCompany = idCompany,
                IdAz_Anagrafica = az.Id,
                IdUtente = IdUtente,
                PowerAdmin = powerAdmin,
                Admin = admin,
                User = utente,
            };
            return _contesto;
        }

        public async Task<HrContesto> AmministratoreAsync()
        {
            var ctx = await ContestoAsync();
            if (!ctx.Amministratore)
                throw new UserMessageException(HumanResourcesMessages.Get("NotAllowed"));
            return ctx;
        }

        public async Task<HrContesto> ContestoCedoliniAsync(int? idCompany)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            // utente dell'azienda: l'azienda e' quella del token
            if (int.TryParse(user?.FindFirst(TenantSchemaAccessor.CompanyClaim)?.Value, out var idCompanyToken) && idCompanyToken > 0)
                return await AmministratoreAsync();

            // utente dello studio: azienda indicata, dello studio, con il modulo attivo e abilitato per lo studio
            var ruoli = await RuoliAsync();
            if (!ruoli.Any(r => RuoliStudio.Contains(r)) || !int.TryParse(user?.FindFirst("financialadvisor")?.Value, out var idStudio) || idStudio <= 0)
                throw new UserMessageException(HumanResourcesMessages.Get("NotAllowed"));
            if (idCompany is not int idc || idc <= 0)
                throw new UserMessageException(InfrastructureMessages.Get("NoCompanySelected"));
            if (!await StudioAbilitatoAsync(idStudio))
                throw new UserMessageException(HumanResourcesMessages.Get("StudioNotEnabled"));
            if (!await _companyRepository.FindAll(x => x.Id == idc && x.IdFinancialAdvisor == idStudio).AnyAsync()
                || !await _companyApplicationRepository.FindAll(x => x.IdCompany == idc && x.ApplicationType == ApplicationType.HumanResources && x.Active).AnyAsync())
                throw new UserMessageException(HumanResourcesMessages.Get("StudioCompanyNotFound"));

            return new HrContesto { IdCompany = idc, IdUtente = IdUtente, Studio = true, IdFinancialAdvisor = idStudio };
        }

        public async Task<List<(int IdCompany, string Descrizione)>> AziendeStudioAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var ruoli = await RuoliAsync();
            if (!ruoli.Any(r => RuoliStudio.Contains(r)) || !int.TryParse(user?.FindFirst("financialadvisor")?.Value, out var idStudio) || idStudio <= 0
                || !await StudioAbilitatoAsync(idStudio))
                throw new UserMessageException(HumanResourcesMessages.Get("StudioNotEnabled"));

            var aziende = await _companyRepository.FindAll(x => x.IdFinancialAdvisor == idStudio).Select(x => new { x.Id, x.Descrizione }).ToListAsync();
            var ids = aziende.Select(x => x.Id).ToList();
            var attive = await _companyApplicationRepository.FindAll(x => ids.Contains(x.IdCompany) && x.ApplicationType == ApplicationType.HumanResources && x.Active)
                                                            .Select(x => x.IdCompany).ToListAsync();
            return aziende.Where(x => attive.Contains(x.Id)).OrderBy(x => x.Descrizione).Select(x => (x.Id, x.Descrizione ?? string.Empty)).ToList();
        }

        private async Task<bool> StudioAbilitatoAsync(int idStudio)
            => _financialAdvisorApplicationRepository.ActiveApplications(idStudio).Contains(ApplicationType.HumanResources)
               && await _licenseGuard.StudioIsLicensedAsync(idStudio, ApplicationType.HumanResources);

        public async Task EnsureDefaultsAsync(int idAz_Anagrafica)
        {
            if (await _Hr_Cfg.FindAll(_ => true).AnyAsync(x => x.IdAz_Anagrafica == idAz_Anagrafica))
                return;
            await _Hr_Cfg.CreateAsync(new Hr_Cfg { IdAz_Anagrafica = idAz_Anagrafica, CreationDate = DateTime.Now });
        }

        public Task<Hr_Cfg> CfgAsync(HrContesto ctx) => _Hr_Cfg.FindAll(_ => true).FirstAsync(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica);

        public async Task<Dip_Anagrafica?> DipendenteCorrenteAsync(HrContesto ctx)
        {
            if (string.IsNullOrEmpty(ctx.IdUtente))
                return null;
            return await _Dip_Anagrafica.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.IdAspNetUsers == ctx.IdUtente).FirstOrDefaultAsync();
        }

        public async Task<Dip_Anagrafica> DipendenteAsync(HrContesto ctx, int idDip_Anagrafica)
            => await _Dip_Anagrafica.FindAll(x => x.Id == idDip_Anagrafica && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
               ?? throw new UserMessageException(HumanResourcesMessages.Get("EmployeeNotFound"));

        public static string NomeDipendente(Dip_Anagrafica d) => $"{d.Cognome} {d.Nome}".Trim();
    }

    /*
     Lavoro su un'azienda precisa in un nuovo scope (TenantScope): i contesti degli applicativi e di Common creati nello scope
     usano gli schemi di quell'azienda. Serve allo studio (il token non ha l'azienda) e ai job in background.
     Il contesto (HrContesto) arriva gia' calcolato; per lo studio si completa qui con l'anagrafica dell'azienda.
    */
    public static class HrAzienda
    {
        public static async Task<T> RunAsync<T>(IServiceScopeFactory scopeFactory, HrContesto ctx, Func<IServiceProvider, HrContesto, Task<T>> work)
        {
            using var tenantScope = TenantScope.Use(ctx.IdCompany);
            using var scope = scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;
            if (ctx.IdAz_Anagrafica == 0)
            {
                var az = await sp.GetRequiredService<ICommonCompanyUtility>().GetAz_AnagraficaAsync(ctx.IdCompany, true);
                await sp.GetRequiredService<IHrUtility>().EnsureDefaultsAsync(az!.Id);
                ctx = new HrContesto
                {
                    IdCompany = ctx.IdCompany, IdAz_Anagrafica = az.Id, IdUtente = ctx.IdUtente, PowerAdmin = ctx.PowerAdmin, Admin = ctx.Admin,
                    User = ctx.User, Studio = ctx.Studio, IdFinancialAdvisor = ctx.IdFinancialAdvisor
                };
            }
            return await work(sp, ctx);
        }
    }
}
