using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.PresentStaffService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.PresentStaffService
{
    public class PresentStaffService : ServiceBase, IPresentStaffService
    {
        public PresentStaffService(IMapper mapper,
                                   UserManager<ApplicationUser> userManager,
                                   IAspNetUsersRepository aspNetUsersRepository,
                                   IOptions<JwtParameter> jwtParameter,
                                   IHttpContextAccessor httpContextAccessor,
                                   IConfiguration configuration
                                   ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
        }

        public virtual async Task<GenericResult<PresentStaff_GetOutModel>> PresentStaffGet(GenericRequest<PresentStaff_GetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                PresentStaff_GetOutModel retVal = new PresentStaff_GetOutModel();

                // TODO: implementare la logica di recupero del personale presente

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
    }

    public interface IPresentStaffService : IServiceBase
    {
        Task<GenericResult<PresentStaff_GetOutModel>> PresentStaffGet(GenericRequest<PresentStaff_GetInModel> model, bool isSubProcess);
    }
}
