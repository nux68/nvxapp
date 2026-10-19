using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Registration;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Registration.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Registrazione di una nuova azienda dallo showcase: senza login, l'azienda nasce nel ramo DEMO.
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : NvxControllerBase
    {
        private readonly IRegistrationService _registrationService;

        public RegistrationController(
                                    IHttpContextAccessor httpContextAccessor,
                                    IRegistrationService registrationService
          ) : base(httpContextAccessor)
        {
            _registrationService = registrationService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("CompanyRegister")]
        public async Task<GenericResult<CompanyRegisterOutModel>> CompanyRegister(GenericRequest<CompanyRegisterInModel> inModel)
        {
            return await _registrationService.CompanyRegister(inModel, false);
        }
    }
}
