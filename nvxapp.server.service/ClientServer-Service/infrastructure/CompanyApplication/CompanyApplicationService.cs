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

        public CompanyApplicationService(IMapper mapper,
                                         UserManager<ApplicationUser> userManager,
                                         IAspNetUsersRepository aspNetUsersRepository,
                                         IOptions<JwtParameter> jwtParameter,
                                         IHttpContextAccessor httpContextAccessor,
                                         IConfiguration configuration,

                                         ICompanyRepository companyRepository,
                                         ICompanyApplicationRepository companyApplicationRepository,
                                         ITenantProvisioningService tenantProvisioningService,
                                         ICompanyInitializerRegistry companyInitializerRegistry) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _companyRepository = companyRepository;
            _companyApplicationRepository = companyApplicationRepository;
            _tenantProvisioningService = tenantProvisioningService;
            _companyInitializerRegistry = companyInitializerRegistry;
        }

        public virtual async Task<GenericResult<CompanyApplicationListOutModel>> CompanyApplicationList(GenericRequest<CompanyApplicationListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                CompanyApplicationListOutModel retVal = new CompanyApplicationListOutModel();

                var company = await _companyRepository.FindByIdAsync(model.Data.IdCompany)
                              ?? throw new Exception($"Azienda {model.Data.IdCompany} non trovata.");

                var rows = _companyApplicationRepository.FindAll(x => x.IdCompany == company.Id).ToList();

                foreach (var application in Enum.GetValues<ApplicationType>())
                {
                    var row = rows.FirstOrDefault(x => x.ApplicationType == application);
                    retVal.CompanyApplication.Add(ToModel(company.Id, application, row));
                }

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<CompanyApplicationPutOutModel>> CompanyApplicationPut(GenericRequest<CompanyApplicationPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                CompanyApplicationPutOutModel retVal = new CompanyApplicationPutOutModel();

                var application = model.Data.ApplicationType;
                if (!Enum.IsDefined(application))
                    throw new Exception($"Applicativo {(int)application} non valido.");

                var company = await _companyRepository.FindByIdAsync(model.Data.IdCompany)
                              ?? throw new Exception($"Azienda {model.Data.IdCompany} non trovata.");

                var row = _companyApplicationRepository.FindAll(x => x.IdCompany == company.Id && x.ApplicationType == application).FirstOrDefault();

                if (model.Data.Active)
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

                retVal.CompanyApplication = ToModel(company.Id, application, row);
                return retVal;
            }, isSubProcess);
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
    }
}
