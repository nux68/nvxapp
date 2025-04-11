using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Account;
using nvxapp.server.service.ClientServer_Service.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.HubAI;
using nvxapp.server.service.Infrastructure;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static nvxapp.server.data.Entities.AspNetUsersDataUtil;

namespace nvxapp.server.service.ClientServer_Service.Parameter.Models
{
    public class ParameterService : ServiceBase, IParameterService
    {
        private readonly IAspNetRolesRepository _aspNetRolesRepository;

        public ParameterService(IMapper mapper,
                              UserManager<ApplicationUser> userManager,
                              IAspNetUsersRepository aspNetUsersRepository,
                              IOptions<JwtParameter> jwtParameter,
                              IHttpContextAccessor httpContextAccessor,
                              IConfiguration configuration,

                              
                              IAspNetRolesRepository aspNetRolesRepository
                              ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            
            _aspNetRolesRepository = aspNetRolesRepository;
        }
        
        public virtual async Task<GenericResult<RolesListOutModel>> Roles(GenericRequest<RolesListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                RolesListOutModel retVal = new RolesListOutModel();

                retVal.Roles = _aspNetRolesRepository.GetAll().ToList();


                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
   

    }

    public interface IParameterService : IServiceBase
    {
    public Task<GenericResult<RolesListOutModel>> Roles(GenericRequest<RolesListInModel> model, Boolean isSubProcess);
}
}
