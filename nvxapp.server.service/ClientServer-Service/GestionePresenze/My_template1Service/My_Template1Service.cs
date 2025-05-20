using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.Base;
using nvxapp.server.service.Interfaces;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Microsoft.Extensions.Options;
using nvxapp.server.service.ServerModels;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.My_template1Service.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.My_template1Service
{
    public class My_Template1Service : ServiceBase, IMy_template1Service
    {
        private readonly IMy_template1Repository _my_template1Repository;

        public My_Template1Service(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IMy_template1Repository my_template1Repository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _my_template1Repository = my_template1Repository;
        }

        public virtual async Task<GenericResult<My_template1OutModel>> GetAll(GenericRequest<My_template1InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                My_template1OutModel retVal = new My_template1OutModel();
                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IMy_template1Service : IServiceBase
    {
        public Task<GenericResult<My_template1OutModel>> GetAll(GenericRequest<My_template1InModel> model, Boolean isSubProcess);
    }
}
