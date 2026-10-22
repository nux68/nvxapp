using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyMove.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using nvxapp.server.data.Localization;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyMove
{
    /*
     Spostamento di un'azienda sotto un altro studio (FinancialAdvisor), riservato al SuperUser.
     Basta aggiornare Company.IdFinancialAdvisor: il centro (Dealer) segue lo studio, gli utenti,
     gli applicativi, le restrizioni di menu e gli schemi tenant dipendono solo dall'azienda.
     Gli utenti del vecchio studio perdono l'accesso da soli (l'elenco aziende dello studio filtra su IdFinancialAdvisor).
     Il controllo usa l'utente che ha fatto il login (UserIdFirstConnection), non quello impersonato.
    */
    public class CompanyMoveService : ServiceBase, ICompanyMoveService
    {
        private const string RuoloSuperUser = "SuperUser";

        private readonly IDealerRepository _dealerRepository;
        private readonly IFinancialAdvisorRepository _financialAdvisorRepository;
        private readonly ICompanyRepository _companyRepository;

        public CompanyMoveService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IDealerRepository dealerRepository,
                                  IFinancialAdvisorRepository financialAdvisorRepository,
                                  ICompanyRepository companyRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dealerRepository = dealerRepository;
            _financialAdvisorRepository = financialAdvisorRepository;
            _companyRepository = companyRepository;
        }

        public virtual async Task<GenericResult<CompanyMoveTreeOutModel>> CompanyMoveTree(GenericRequest<CompanyMoveTreeInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await VerificaSuperUser();

                var retVal = new CompanyMoveTreeOutModel();

                retVal.Dealers = _dealerRepository.FindAll(x => true)
                    .OrderBy(x => x.Descrizione)
                    .Select(x => new CompanyMoveDealerModel { IdDealer = x.Id, Descrizione = x.Descrizione })
                    .ToList();

                retVal.FinancialAdvisors = _financialAdvisorRepository.FindAll(x => true)
                    .OrderBy(x => x.Descrizione)
                    .Select(x => new CompanyMoveFinancialAdvisorModel { IdFinancialAdvisor = x.Id, IdDealer = x.IdDealer, Descrizione = x.Descrizione })
                    .ToList();

                retVal.Companies = _companyRepository.FindAll(x => true)
                    .OrderBy(x => x.Descrizione)
                    .Select(x => new CompanyMoveCompanyModel { IdCompany = x.Id, IdFinancialAdvisor = x.IdFinancialAdvisor, Descrizione = x.Descrizione })
                    .ToList();

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<CompanyMovePutOutModel>> CompanyMovePut(GenericRequest<CompanyMovePutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await VerificaSuperUser();

                var retVal = new CompanyMovePutOutModel();

                var company = await _companyRepository.FindByIdAsync(model.Data.IdCompany)
                              ?? throw new UserMessageException(InfrastructureMessages.Get("CompanyNotFound"));
                var financialAdvisor = await _financialAdvisorRepository.FindByIdAsync(model.Data.IdFinancialAdvisor)
                                       ?? throw new UserMessageException(InfrastructureMessages.Get("CompanyMoveTargetNotFound"));

                if (company.IdFinancialAdvisor == financialAdvisor.Id)
                    throw new UserMessageException(InfrastructureMessages.Get("CompanyMoveAlreadyInTarget"));

                company.IdFinancialAdvisor = financialAdvisor.Id;
                await _companyRepository.UpdateAsync(company);

                retVal.Company = new CompanyMoveCompanyModel { IdCompany = company.Id, IdFinancialAdvisor = company.IdFinancialAdvisor, Descrizione = company.Descrizione };

                return retVal;
            }, isSubProcess);
        }

        // l'utente del login deve essere SuperUser (anche mentre sta impersonando qualcun altro)
        private async Task VerificaSuperUser()
        {
            var user = await _userManager.FindByIdAsync(this.UserIdFirstConnection)
                       ?? throw new UserMessageException(InfrastructureMessages.Get("UserNotFound"));
            if (!await _userManager.IsInRoleAsync(user, RuoloSuperUser))
                throw new UserMessageException(InfrastructureMessages.Get("OperationReservedToSuperUser"));
        }
    }

    public interface ICompanyMoveService : IServiceBase
    {
        Task<GenericResult<CompanyMoveTreeOutModel>> CompanyMoveTree(GenericRequest<CompanyMoveTreeInModel> model, Boolean isSubProcess);
        Task<GenericResult<CompanyMovePutOutModel>> CompanyMovePut(GenericRequest<CompanyMovePutInModel> model, Boolean isSubProcess);
    }
}
