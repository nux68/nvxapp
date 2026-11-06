using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.UserManual;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.Infrastructure
{
    // Manuali utente in PDF: il file si genera con un job e si scarica dalla barra dei job (Account/Download)
    [ApiController]
    [Route("api/[controller]")]
    public class UserManualController : NvxControllerBase
    {
        private readonly IUserManualService _service;

        public UserManualController(IHttpContextAccessor httpContextAccessor, IUserManualService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize, HttpPost, Route("UserManualGenerate")]
        public async Task<GenericResult<UserManualGenerateOutModel>> UserManualGenerate(GenericRequest<UserManualGenerateInModel> inModel) => await _service.UserManualGenerate(inModel, false);
    }
}
