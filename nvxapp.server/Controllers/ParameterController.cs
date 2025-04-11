using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Account;
using nvxapp.server.service.ClientServer_Service.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.Parameter.Models;


namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParameterController : NvxControllerBase
    {
        private readonly IParameterService _parameterService;

        public ParameterController(
                                    IHttpContextAccessor httpContextAccessor,
                                    IParameterService parameterService
          ) : base(httpContextAccessor)
        {
            _parameterService = parameterService;
        }

        

        //[Authorize]
        [HttpPost]
        [Route("Roles")]
        public async Task<GenericResult<RolesListOutModel>> Roles(GenericRequest<RolesListInModel> inModel)
        {
            var res = await _parameterService.Roles(inModel, false);

            return res;
        }

        


    }



}
