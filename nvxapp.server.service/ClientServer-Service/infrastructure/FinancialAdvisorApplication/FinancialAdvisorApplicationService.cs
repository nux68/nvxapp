using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyApplication;
using nvxapp.server.service.ClientServer_Service.Infrastructure.FinancialAdvisorApplication.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.FinancialAdvisorApplication
{
    /*
     Abilitazione degli applicativi per studio (ANALISI_DOCUMENTMANAGEMENT_APPLICATIVO.md [branch DocumentManagement] §4.1): abilita le funzioni lato studio.
     Non crea schemi e non limita le aziende dello studio. Cambiano lo stato solo il SuperUser e il
     DealerPowerAdmin del centro dello studio. Si elencano gli applicativi attivabili presenti nel ramo.
    */
    public class FinancialAdvisorApplicationService : ServiceBase, IFinancialAdvisorApplicationService
    {
        private readonly IFinancialAdvisorRepository _financialAdvisorRepository;
        private readonly IFinancialAdvisorApplicationRepository _financialAdvisorApplicationRepository;
        private readonly ApplicationDbContextRegistry _applicationDbContextRegistry;
        private readonly IApplicationActivationAuthorizer _applicationActivationAuthorizer;

        public FinancialAdvisorApplicationService(IMapper mapper,
                                                  UserManager<ApplicationUser> userManager,
                                                  IAspNetUsersRepository aspNetUsersRepository,
                                                  IOptions<JwtParameter> jwtParameter,
                                                  IHttpContextAccessor httpContextAccessor,
                                                  IConfiguration configuration,
                                                  IFinancialAdvisorRepository financialAdvisorRepository,
                                                  IFinancialAdvisorApplicationRepository financialAdvisorApplicationRepository,
                                                  ApplicationDbContextRegistry applicationDbContextRegistry,
                                                  IApplicationActivationAuthorizer applicationActivationAuthorizer) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _financialAdvisorRepository = financialAdvisorRepository;
            _financialAdvisorApplicationRepository = financialAdvisorApplicationRepository;
            _applicationDbContextRegistry = applicationDbContextRegistry;
            _applicationActivationAuthorizer = applicationActivationAuthorizer;
        }

        public virtual async Task<GenericResult<FinancialAdvisorApplicationListOutModel>> FinancialAdvisorApplicationList(GenericRequest<FinancialAdvisorApplicationListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var studio = await _financialAdvisorRepository.FindByIdAsync(model.Data.IdFinancialAdvisor)
                             ?? throw new UserMessageException(InfrastructureMessages.Get("FinancialAdvisorApplicationStudioNotFound"));
                return new FinancialAdvisorApplicationListOutModel
                {
                    FinancialAdvisorApplication = Applications(studio.Id),
                    CanManage = await _applicationActivationAuthorizer.CanManageFinancialAdvisorAsync(studio.Id)
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<FinancialAdvisorApplicationPutOutModel>> FinancialAdvisorApplicationPut(GenericRequest<FinancialAdvisorApplicationPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var application = model.Data.ApplicationType;
                if (!Enum.IsDefined(application) || !application.IsActivatable() || !_applicationDbContextRegistry.IsRegistered(application))
                    throw new UserMessageException(InfrastructureMessages.Get("ApplicationInvalid", (int)application));

                var studio = await _financialAdvisorRepository.FindByIdAsync(model.Data.IdFinancialAdvisor)
                             ?? throw new UserMessageException(InfrastructureMessages.Get("FinancialAdvisorApplicationStudioNotFound"));

                var row = _financialAdvisorApplicationRepository.FindAll(x => x.IdFinancialAdvisor == studio.Id && x.ApplicationType == application).FirstOrDefault();

                // stesso stato: nessun controllo (il salvataggio dello studio rimanda sempre tutti gli applicativi)
                if (model.Data.Active != (row?.Active ?? false))
                {
                    if (!await _applicationActivationAuthorizer.CanManageFinancialAdvisorAsync(studio.Id))
                        throw new UserMessageException(InfrastructureMessages.Get("ApplicationActivationNotAllowed"));

                    if (row == null)
                    {
                        row = await _financialAdvisorApplicationRepository.CreateAsync(new nvxapp.server.data.Entities.Public.FinancialAdvisorApplication
                        {
                            IdFinancialAdvisor = studio.Id,
                            ApplicationType = application,
                            Active = true,
                            ActivationDate = DateTime.Now,
                        });
                    }
                    else
                    {
                        row.Active = model.Data.Active;
                        if (row.Active)
                        {
                            row.ActivationDate = DateTime.Now;
                            row.DeactivationDate = null;
                        }
                        else
                        {
                            row.DeactivationDate = DateTime.Now;
                        }
                        row = await _financialAdvisorApplicationRepository.UpdateAsync(row);
                    }
                }

                return new FinancialAdvisorApplicationPutOutModel { FinancialAdvisorApplication = ToModel(studio.Id, application, row) };
            }, isSubProcess);
        }

        public List<FinancialAdvisorApplicationModel> Applications(int idFinancialAdvisor)
        {
            var rows = idFinancialAdvisor > 0
                ? _financialAdvisorApplicationRepository.FindAll(x => x.IdFinancialAdvisor == idFinancialAdvisor).ToList()
                : new List<nvxapp.server.data.Entities.Public.FinancialAdvisorApplication>();

            return Enum.GetValues<ApplicationType>()
                       .Where(application => application.IsActivatable() && _applicationDbContextRegistry.IsRegistered(application))
                       .Select(application => ToModel(idFinancialAdvisor, application, rows.FirstOrDefault(x => x.ApplicationType == application)))
                       .ToList();
        }

        public Task<bool> CanManageAsync(int idFinancialAdvisor) => _applicationActivationAuthorizer.CanManageFinancialAdvisorAsync(idFinancialAdvisor);

        private static FinancialAdvisorApplicationModel ToModel(int idFinancialAdvisor, ApplicationType application, nvxapp.server.data.Entities.Public.FinancialAdvisorApplication? row)
            => new FinancialAdvisorApplicationModel
            {
                IdFinancialAdvisor = idFinancialAdvisor,
                ApplicationType = application,
                Descrizione = application.ToString(),
                Active = row?.Active ?? false,
                ActivationDate = row?.ActivationDate,
                DeactivationDate = row?.DeactivationDate,
            };
    }

    public interface IFinancialAdvisorApplicationService : IServiceBase
    {
        Task<GenericResult<FinancialAdvisorApplicationListOutModel>> FinancialAdvisorApplicationList(GenericRequest<FinancialAdvisorApplicationListInModel> model, Boolean isSubProcess);
        Task<GenericResult<FinancialAdvisorApplicationPutOutModel>> FinancialAdvisorApplicationPut(GenericRequest<FinancialAdvisorApplicationPutInModel> model, Boolean isSubProcess);

        // applicativi attivabili con lo stato per lo studio (senza chiamata API)
        List<FinancialAdvisorApplicationModel> Applications(int idFinancialAdvisor);

        // l'utente corrente puo' cambiarli (SuperUser o DealerPowerAdmin del centro dello studio)
        Task<bool> CanManageAsync(int idFinancialAdvisor);
    }
}
