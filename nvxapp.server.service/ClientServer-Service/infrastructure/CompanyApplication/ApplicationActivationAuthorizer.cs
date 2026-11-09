using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyApplication
{
    /*
     Chi puo' attivare e disattivare gli applicativi (ANALISI_DOCUMENTALE.md [branch DocumentManagement] §4.1, regola S2, estesa a tutti gli applicativi):
        - il SuperUser (utente del login, anche mentre impersona)
        - il DealerPowerAdmin del centro (Dealer) a cui appartengono lo studio o l'azienda
     Nessun altro ruolo: ne' lo studio ne' l'azienda attivano da soli i propri applicativi.
     I ruoli e il centro sono quelli del token (utente corrente), mai dal client.
    */
    public interface IApplicationActivationAuthorizer : IServiceBase
    {
        Task<bool> CanManageCompanyAsync(int idCompany);
        Task<bool> CanManageFinancialAdvisorAsync(int idFinancialAdvisor);

        /// <summary>L'utente del login e' SuperUser (anche mentre impersona).</summary>
        Task<bool> IsSuperUserAsync();

        /// <summary>L'utente corrente (token) ha il ruolo.</summary>
        Task<bool> CurrentUserHasRoleAsync(string role);
    }

    public class ApplicationActivationAuthorizer : ServiceBase, IApplicationActivationAuthorizer
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IFinancialAdvisorRepository _financialAdvisorRepository;
        private readonly IAspNetRolesRepository _aspNetRolesRepository;
        private readonly IAspNetUserRolesRepository _aspNetUserRolesRepository;

        public ApplicationActivationAuthorizer(IMapper mapper,
                                               UserManager<ApplicationUser> userManager,
                                               IAspNetUsersRepository aspNetUsersRepository,
                                               IOptions<JwtParameter> jwtParameter,
                                               IHttpContextAccessor httpContextAccessor,
                                               IConfiguration configuration,
                                               ICompanyRepository companyRepository,
                                               IFinancialAdvisorRepository financialAdvisorRepository,
                                               IAspNetRolesRepository aspNetRolesRepository,
                                               IAspNetUserRolesRepository aspNetUserRolesRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _companyRepository = companyRepository;
            _financialAdvisorRepository = financialAdvisorRepository;
            _aspNetRolesRepository = aspNetRolesRepository;
            _aspNetUserRolesRepository = aspNetUserRolesRepository;
        }

        public async Task<bool> CanManageCompanyAsync(int idCompany)
        {
            var idDealer = await _companyRepository.FindAll(x => x.Id == idCompany)
                                            .Select(x => (int?)x.FinancialAdvisorNavigation!.IdDealer)
                                            .FirstOrDefaultAsync();
            return await CanManageDealerAsync(idDealer);
        }

        public async Task<bool> CanManageFinancialAdvisorAsync(int idFinancialAdvisor)
        {
            var idDealer = await _financialAdvisorRepository.FindAll(x => x.Id == idFinancialAdvisor)
                                                     .Select(x => (int?)x.IdDealer)
                                                     .FirstOrDefaultAsync();
            return await CanManageDealerAsync(idDealer);
        }

        public Task<bool> IsSuperUserAsync() => HasRoleAsync(UserIdFirstConnection, nameof(RoleCode.SuperUser));

        public Task<bool> CurrentUserHasRoleAsync(string role) => HasRoleAsync(CurrentUserId, role);

        private async Task<bool> CanManageDealerAsync(int? idDealer)
        {
            if (await HasRoleAsync(UserIdFirstConnection, nameof(RoleCode.SuperUser)))
                return true;

            return idDealer.HasValue
                   && int.TryParse(CurrentDealer, out int dealerCorrente) && dealerCorrente == idDealer.Value
                   && await HasRoleAsync(CurrentUserId, nameof(RoleCode.DealerPowerAdmin));
        }

        private async Task<bool> HasRoleAsync(string? idAspNetUsers, string role)
        {
            if (string.IsNullOrEmpty(idAspNetUsers))
                return false;
            var idRole = await _aspNetRolesRepository.FindAll(r => r.Name == role).Select(r => r.Id).FirstOrDefaultAsync();
            return idRole != null && await _aspNetUserRolesRepository.FindAll(x => x.RoleId == idRole && x.UserId == idAspNetUsers).AnyAsync();
        }
    }
}
