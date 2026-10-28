using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.PasswordRecovery;
using nvxapp.server.service.ClientServer_Service.Infrastructure.PasswordRecovery.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Recupero dell'accesso con password dimenticata, senza login: link per email, poi nuova password.
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordRecoveryController : NvxControllerBase
    {
        private readonly IPasswordRecoveryService _passwordRecoveryService;

        public PasswordRecoveryController(
                                    IHttpContextAccessor httpContextAccessor,
                                    IPasswordRecoveryService passwordRecoveryService
          ) : base(httpContextAccessor)
        {
            _passwordRecoveryService = passwordRecoveryService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("PasswordRecoveryRequest")]
        public async Task<GenericResult<PasswordRecoveryRequestOutModel>> PasswordRecoveryRequest(GenericRequest<PasswordRecoveryRequestInModel> inModel)
        {
            return await _passwordRecoveryService.PasswordRecoveryRequest(inModel, false);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("PasswordRecoveryReset")]
        public async Task<GenericResult<PasswordRecoveryResetOutModel>> PasswordRecoveryReset(GenericRequest<PasswordRecoveryResetInModel> inModel)
        {
            return await _passwordRecoveryService.PasswordRecoveryReset(inModel, false);
        }

        // cambio della password conoscendo quella attuale (dal login)
        [AllowAnonymous]
        [HttpPost]
        [Route("PasswordChange")]
        public async Task<GenericResult<PasswordChangeOutModel>> PasswordChange(GenericRequest<PasswordChangeInModel> inModel)
        {
            return await _passwordRecoveryService.PasswordChange(inModel, false);
        }
    }
}
