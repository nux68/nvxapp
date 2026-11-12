using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyApplication.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.CompanyInit;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.User;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.service.ClientServer_Service.Infrastructure.License;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using nvxapp.server.data.Localization;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyApplication
{
    /*
     Attivazione degli applicativi per azienda.
        - attivazione: registra l'applicativo come attivo, crea/migra il suo schema (multi-tenant)
          ed esegue gli inizializzatori di quell'applicativo per l'azienda
        - disattivazione: blocca l'accesso; schema e dati restano (riattivando si ritrova tutto)
     Chi cambia lo stato di un applicativo (ANALISI_INFRASTRUCTURE_LICENZE.md §15.6):
        - SuperUser: sempre
        - sistema licenze spento: il DealerPowerAdmin del centro (regola di sempre)
        - sistema licenze acceso: solo se l'azienda e' coperta da una licenza per l'applicativo (propria, dello studio o
          cumulativa), il DealerPowerAdmin del centro e il FinancialAdvisorPowerAdmin dello studio dell'azienda (companyedit)
     Un'azienda che sottoscrive da se' una licenza attiva l'applicativo con ActivateForLicenseAsync (schema e dati iniziali).
    */
    public class CompanyApplicationService : ServiceBase, ICompanyApplicationService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly ITenantProvisioningService _tenantProvisioningService;
        private readonly ICompanyInitializerRegistry _companyInitializerRegistry;
        private readonly ApplicationDbContextRegistry _applicationDbContextRegistry;
        private readonly IApplicationActivationAuthorizer _applicationActivationAuthorizer;
        private readonly ILicenseGuard _licenseGuard;
        private readonly ILicenseSubscriptionManager _licenseSubscriptionManager;
        private readonly IScadenzaRepository _scadenzaRepository;
        private readonly IPresaVisioneRepository _presaVisioneRepository;

        public CompanyApplicationService(IMapper mapper,
                                         UserManager<ApplicationUser> userManager,
                                         IAspNetUsersRepository aspNetUsersRepository,
                                         IOptions<JwtParameter> jwtParameter,
                                         IHttpContextAccessor httpContextAccessor,
                                         IConfiguration configuration,

                                         ICompanyRepository companyRepository,
                                         ICompanyApplicationRepository companyApplicationRepository,
                                         ITenantProvisioningService tenantProvisioningService,
                                         ICompanyInitializerRegistry companyInitializerRegistry,
                                         ApplicationDbContextRegistry applicationDbContextRegistry,
                                         IApplicationActivationAuthorizer applicationActivationAuthorizer,
                                         ILicenseGuard licenseGuard,
                                         ILicenseSubscriptionManager licenseSubscriptionManager,
                                         IScadenzaRepository scadenzaRepository,
                                         IPresaVisioneRepository presaVisioneRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _licenseGuard = licenseGuard;
            _licenseSubscriptionManager = licenseSubscriptionManager;
            _scadenzaRepository = scadenzaRepository;
            _presaVisioneRepository = presaVisioneRepository;
            _companyRepository = companyRepository;
            _companyApplicationRepository = companyApplicationRepository;
            _tenantProvisioningService = tenantProvisioningService;
            _companyInitializerRegistry = companyInitializerRegistry;
            _applicationDbContextRegistry = applicationDbContextRegistry;
            _applicationActivationAuthorizer = applicationActivationAuthorizer;
        }

        public virtual async Task<GenericResult<CompanyApplicationListOutModel>> CompanyApplicationList(GenericRequest<CompanyApplicationListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                CompanyApplicationListOutModel retVal = new CompanyApplicationListOutModel();

                var company = await _companyRepository.FindByIdAsync(model.Data.IdCompany)
                              ?? throw new UserMessageException(InfrastructureMessages.Get("CompanyNotFoundWithId", model.Data.IdCompany));

                retVal.CompanyApplication = Applications(company.Id);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<CompanyApplicationPutOutModel>> CompanyApplicationPut(GenericRequest<CompanyApplicationPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                CompanyApplicationPutOutModel retVal = new CompanyApplicationPutOutModel();

                var application = model.Data.ApplicationType;
                // Common (dati comuni) non si attiva: e' sempre presente; un applicativo senza contesto in questo ramo
                // (es. CRM o documentale prima del loro sviluppo) non si puo' attivare
                if (!Enum.IsDefined(application) || !application.IsActivatable() || !_applicationDbContextRegistry.IsRegistered(application))
                    throw new UserMessageException(InfrastructureMessages.Get("ApplicationInvalid", (int)application));

                var company = await _companyRepository.FindByIdAsync(model.Data.IdCompany)
                              ?? throw new UserMessageException(InfrastructureMessages.Get("CompanyNotFoundWithId", model.Data.IdCompany));

                var row = _companyApplicationRepository.FindAll(x => x.IdCompany == company.Id && x.ApplicationType == application).FirstOrDefault();

                // cambiare lo stato: vedi CanChangeAsync (stesso stato: nessun controllo, perche' il salvataggio
                // dell'azienda rimanda sempre tutti gli applicativi)
                if (model.Data.Active != (row?.Active ?? false) && !await CanChangeAsync(company.Id, application))
                {
                    if (model.Data.Active && !await _licenseGuard.IsLicensedAsync(company.Id, application))
                        throw new UserMessageException(InfrastructureMessages.Get("ApplicationLicenseRequired", application.ToString()));
                    throw new UserMessageException(InfrastructureMessages.Get("ApplicationActivationNotAllowed"));
                }

                row = await SetActiveAsync(company, application, model.Data.Active, row);

                retVal.CompanyApplication = ToModel(company.Id, application, row);
                return retVal;
            }, isSubProcess);
        }

        /*
         Applicativi che il ramo attiva sulle aziende appena create (es. presenze con ActivateOnNewCompany):
         nessun controllo del ruolo, perche' non e' una scelta dell'utente. Da usare SOLO subito dopo la creazione
         dell'azienda.
        */
        public async Task ActivateForNewCompanyAsync(int idCompany, ApplicationType application)
        {
            if (!application.IsActivatable() || !_applicationDbContextRegistry.IsRegistered(application))
                return;
            var company = await _companyRepository.FindByIdAsync(idCompany)
                          ?? throw new UserMessageException(InfrastructureMessages.Get("CompanyNotFoundWithId", idCompany));
            var row = _companyApplicationRepository.FindAll(x => x.IdCompany == company.Id && x.ApplicationType == application).FirstOrDefault();
            await SetActiveAsync(company, application, true, row);
        }

        public Task<bool> CanManageAsync(int idCompany) => _applicationActivationAuthorizer.CanManageCompanyAsync(idCompany);

        /*
         L'utente corrente puo' cambiare lo stato dell'applicativo per l'azienda (regole in testa alla classe).
         idCompany = 0: azienda nuova dello studio idFinancialAdvisor (licenza = copertura dello studio).
        */
        public async Task<bool> CanChangeAsync(int idCompany, ApplicationType application, int idFinancialAdvisor = 0)
        {
            if (await _applicationActivationAuthorizer.IsSuperUserAsync())
                return true;
            // sistema licenze spento, o applicativo non soggetto a licenza (es. Moke): la regola di sempre (centro)
            if (!await _licenseGuard.EnabledAsync() || LicenseGuard.Funzione(application) == null)
                return idCompany > 0 && await _applicationActivationAuthorizer.CanManageCompanyAsync(idCompany);
            if (!await IsLicensedAsync(idCompany, application, idFinancialAdvisor))
                return false;
            return idCompany > 0
                   ? await _applicationActivationAuthorizer.CanManageCompanyAsync(idCompany) || await _applicationActivationAuthorizer.IsStudioPowerAdminOfCompanyAsync(idCompany)
                   : await _applicationActivationAuthorizer.CanManageFinancialAdvisorAsync(idFinancialAdvisor) || await _applicationActivationAuthorizer.IsStudioPowerAdminAsync(idFinancialAdvisor);
        }

        // coperta da licenza (sistema spento: sempre); azienda nuova: copertura dello studio verso le sue aziende
        private async Task<bool> IsLicensedAsync(int idCompany, ApplicationType application, int idFinancialAdvisor)
        {
            if (!await _licenseGuard.EnabledAsync())
                return true;
            if (idCompany > 0)
                return await _licenseGuard.IsLicensedAsync(idCompany, application);
            return LicenseGuard.Funzione(application) is not nvxapp.server.data.Entities.Public.AmbitoLicenza ambito
                   || (idFinancialAdvisor > 0 && await _licenseSubscriptionManager.StudioCoverageLevelAsync(idFinancialAdvisor, ambito) != null);
        }

        /// <summary>Applicativi con lo stato per l'azienda e, per ognuno, licenza e possibilita' di cambiarlo da parte dell'utente corrente.</summary>
        public async Task<List<CompanyApplicationModel>> ApplicationsWithRightsAsync(int idCompany, int idFinancialAdvisor = 0)
        {
            var list = Applications(idCompany);
            foreach (var a in list)
            {
                a.Licensed = await IsLicensedAsync(idCompany, a.ApplicationType, idFinancialAdvisor);
                a.CanChange = await CanChangeAsync(idCompany, a.ApplicationType, idFinancialAdvisor);
            }
            return list;
        }

        /*
         Attivazione per una licenza sottoscritta dall'azienda (ANALISI_INFRASTRUCTURE_LICENZE.md §15.6): schema e dati iniziali come in
         companyedit, senza controllo del ruolo (la sottoscrizione e' gia' autorizzata). Cumulativo = tutte le funzioni.
         Restituisce gli applicativi attivati adesso (quelli gia' attivi o non presenti in questo ramo sono saltati).
        */
        public async Task<List<ApplicationType>> ActivateForLicenseAsync(int idCompany, nvxapp.server.data.Entities.Public.AmbitoLicenza ambito)
        {
            var applicativi = ambito == nvxapp.server.data.Entities.Public.AmbitoLicenza.Cumulativo
                              ? LicenseGuard.Funzioni.Select(x => (ApplicationType)(int)x).ToList()
                              : new List<ApplicationType> { (ApplicationType)(int)ambito };
            var company = await _companyRepository.FindByIdAsync(idCompany)
                          ?? throw new UserMessageException(InfrastructureMessages.Get("CompanyNotFoundWithId", idCompany));
            var attivati = new List<ApplicationType>();
            foreach (var application in applicativi.Where(a => a.IsActivatable() && _applicationDbContextRegistry.IsRegistered(a)))
            {
                var row = _companyApplicationRepository.FindAll(x => x.IdCompany == company.Id && x.ApplicationType == application).FirstOrDefault();
                if (row?.Active == true)
                    continue;
                await SetActiveAsync(company, application, true, row);
                attivati.Add(application);
            }
            return attivati;
        }

        /*
         Disdetta della licenza dell'azienda (ANALISI_INFRASTRUCTURE_LICENZE.md §15.9), da chiamare dopo aver annullato
         l'abbonamento e dentro la transazione della disdetta. Per ogni applicativo della licenza che resta senza copertura
         (nessun'altra licenza propria, dello studio o cumulativa): applicativo disattivato, schema eliminato con tutti i dati,
         scadenze e prese visione del modulo eliminate, cache degli inizializzatori azzerata (una nuova sottoscrizione riparte
         da uno schema nuovo con i dati iniziali). Con il sistema licenze spento non fa nulla: gli applicativi si usano senza licenza.
         I file su disco (chiavi nelle tabelle dello schema) non vengono toccati.
         Restituisce gli applicativi i cui dati sono stati eliminati.
        */
        public async Task<List<ApplicationType>> RemoveForLicenseCancelAsync(int idCompany, nvxapp.server.data.Entities.Public.AmbitoLicenza ambito)
        {
            var rimossi = new List<ApplicationType>();
            if (!await _licenseGuard.EnabledAsync())
                return rimossi;

            var applicativi = ambito == nvxapp.server.data.Entities.Public.AmbitoLicenza.Cumulativo
                              ? LicenseGuard.Funzioni.Select(x => (ApplicationType)(int)x).ToList()
                              : new List<ApplicationType> { (ApplicationType)(int)ambito };
            foreach (var application in applicativi.Where(a => a.IsActivatable() && _applicationDbContextRegistry.IsRegistered(a)))
            {
                var copertura = await _licenseGuard.CoverageAsync(idCompany, (nvxapp.server.data.Entities.Public.AmbitoLicenza)(int)application, reale: true);
                if (copertura.Livello != null)
                    continue;

                var row = _companyApplicationRepository.FindAll(x => x.IdCompany == idCompany && x.ApplicationType == application).FirstOrDefault();
                if (row?.Active == true)
                {
                    row.Active = false;
                    row.DeactivationDate = DateTime.Now;
                    await _companyApplicationRepository.UpdateAsync(row);
                }

                var schema = await _tenantProvisioningService.DropApplicationSchemaAsync(idCompany, application);
                int app = (int)application;
                await _scadenzaRepository.FindAll(x => x.IdCompany == idCompany && x.Application == app).ExecuteDeleteAsync();
                await _presaVisioneRepository.FindAll(x => x.IdCompany == idCompany && x.Application == app).ExecuteDeleteAsync();
                CompanyInitializerRegistry.Forget(idCompany, application);
                UserInitializerRegistry.Forget(idCompany, application);

                if (row != null || schema != null)
                    rimossi.Add(application);
            }
            return rimossi;
        }

        // attiva o disattiva (gia' autorizzato): attivazione con schema e dati iniziali, disattivazione con soli dati conservati
        private async Task<nvxapp.server.data.Entities.Public.CompanyApplication?> SetActiveAsync(Company company, ApplicationType application, bool active,
                                                                                                  nvxapp.server.data.Entities.Public.CompanyApplication? row)
        {
                if (active)
                {
                    if (row == null)
                    {
                        row = await _companyApplicationRepository.CreateAsync(new nvxapp.server.data.Entities.Public.CompanyApplication
                        {
                            IdCompany = company.Id,
                            ApplicationType = application,
                            Active = true,
                            ActivationDate = DateTime.Now,
                        });
                    }
                    else if (!row.Active)
                    {
                        row.Active = true;
                        row.ActivationDate = DateTime.Now;
                        row.DeactivationDate = null;
                        row = await _companyApplicationRepository.UpdateAsync(row);
                    }

                    // schema dell'applicativo (multi-tenant) e dati iniziali dell'azienda per l'applicativo
                    await _tenantProvisioningService.EnsureApplicationAsync(company.Id, application);
                    await _companyInitializerRegistry.InitializeApplicationAsync(company, application);
                }
                else if (row != null && row.Active)
                {
                    // solo blocco dell'accesso: schema e dati restano
                    row.Active = false;
                    row.DeactivationDate = DateTime.Now;
                    row = await _companyApplicationRepository.UpdateAsync(row);
                }
                return row;
        }

        // Tutti gli applicativi dell'enum ApplicationType (letto dinamicamente: un nuovo valore
        // compare da solo) con lo stato per l'azienda, tranne Common (sempre presente) e gli applicativi senza
        // contesto registrato in questo ramo (es. CRM e documentale finche' non sono sviluppati).
        // idCompany = 0 (azienda nuova): tutti non attivi.
        public List<CompanyApplicationModel> Applications(int idCompany)
        {
            var rows = idCompany > 0
                ? _companyApplicationRepository.FindAll(x => x.IdCompany == idCompany).ToList()
                : new List<nvxapp.server.data.Entities.Public.CompanyApplication>();

            return Enum.GetValues<ApplicationType>()
                       .Where(application => application.IsActivatable() && _applicationDbContextRegistry.IsRegistered(application))
                       .Select(application => ToModel(idCompany, application, rows.FirstOrDefault(x => x.ApplicationType == application)))
                       .ToList();
        }

        private static CompanyApplicationModel ToModel(int idCompany, ApplicationType application, nvxapp.server.data.Entities.Public.CompanyApplication? row)
            => new CompanyApplicationModel
            {
                IdCompany = idCompany,
                ApplicationType = application,
                Descrizione = application.ToString(),
                Active = row?.Active ?? false,
                ActivationDate = row?.ActivationDate,
                DeactivationDate = row?.DeactivationDate,
            };
    }

    public interface ICompanyApplicationService : IServiceBase
    {
        public Task<GenericResult<CompanyApplicationListOutModel>> CompanyApplicationList(GenericRequest<CompanyApplicationListInModel> model, Boolean isSubProcess);
        public Task<GenericResult<CompanyApplicationPutOutModel>> CompanyApplicationPut(GenericRequest<CompanyApplicationPutInModel> model, Boolean isSubProcess);

        // tutti gli applicativi con lo stato per l'azienda (senza chiamata API)
        public List<CompanyApplicationModel> Applications(int idCompany);

        // l'utente corrente puo' cambiare gli applicativi dell'azienda (SuperUser o DealerPowerAdmin del centro)
        public Task<bool> CanManageAsync(int idCompany);

        // applicativi che il ramo attiva sulle aziende appena create (senza controllo del ruolo)
        public Task ActivateForNewCompanyAsync(int idCompany, ApplicationType application);

        // licenze (ANALISI_INFRASTRUCTURE_LICENZE.md §15.6): chi puo' cambiare lo stato, elenco con licenza e diritti, attivazione da sottoscrizione
        public Task<bool> CanChangeAsync(int idCompany, ApplicationType application, int idFinancialAdvisor = 0);

        // disdetta (§15.9): elimina schema e dati degli applicativi della licenza rimasti senza copertura
        public Task<List<ApplicationType>> RemoveForLicenseCancelAsync(int idCompany, nvxapp.server.data.Entities.Public.AmbitoLicenza ambito);
        public Task<List<CompanyApplicationModel>> ApplicationsWithRightsAsync(int idCompany, int idFinancialAdvisor = 0);
        public Task<List<ApplicationType>> ActivateForLicenseAsync(int idCompany, nvxapp.server.data.Entities.Public.AmbitoLicenza ambito);
    }
}
