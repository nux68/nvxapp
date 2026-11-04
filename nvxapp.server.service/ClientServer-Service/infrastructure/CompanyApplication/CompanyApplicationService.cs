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
    */
    public class CompanyApplicationService : ServiceBase, ICompanyApplicationService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly ITenantProvisioningService _tenantProvisioningService;
        private readonly ICompanyInitializerRegistry _companyInitializerRegistry;
        private readonly ApplicationDbContextRegistry _applicationDbContextRegistry;
        private readonly IApplicationActivationAuthorizer _applicationActivationAuthorizer;

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
                                         IApplicationActivationAuthorizer applicationActivationAuthorizer) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
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

                // cambiare lo stato e' riservato al SuperUser e al DealerPowerAdmin del centro (stesso stato: nessun controllo,
                // perche' il salvataggio dell'azienda rimanda sempre tutti gli applicativi)
                if (model.Data.Active != (row?.Active ?? false) && !await _applicationActivationAuthorizer.CanManageCompanyAsync(company.Id))
                    throw new UserMessageException(InfrastructureMessages.Get("ApplicationActivationNotAllowed"));

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
    }
}
