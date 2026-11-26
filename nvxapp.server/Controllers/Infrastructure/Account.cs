using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Security.Claims;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.data.Localization;


namespace nvxapp.server.Controllers.Infrastructure
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : NvxControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(
                                    IHttpContextAccessor httpContextAccessor,
                                    IAccountService accountService
          ) : base(httpContextAccessor)
        {
            _accountService = accountService;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<GenericResult<LoginOutModel>> Login(GenericRequest<LoginInModel> inModel)
        {
            var res = await _accountService.Login(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("UserRoles")]
        public async Task<GenericResult<UserRolesOutModel>> UserRoles(GenericRequest<UserRolesInModel> inModel)
        {
            var res = await _accountService.UserRoles(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("UserCheck")]
        public async Task<GenericResult<UserCheckOutModel>> UserCheck(GenericRequest<UserCheckInModel> inModel)
        {
            var res = await _accountService.UserCheck(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("UserLoad")]
        public async Task<GenericResult<UserLoadOutModel>> UserLoad(GenericRequest<UserLoadInModel> inModel)
        {
            var res = await _accountService.UserLoad(inModel, false);

            return res;
        }



        [Authorize]
        [HttpPost]
        [Route("DealerList")]
        public async Task<GenericResult<DealerListOutModel>> DealerList(GenericRequest<DealerListInModel> inModel)
        {
            var res = await _accountService.DealerList(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("DealerGet")]
        public async Task<GenericResult<DealerGetOutModel>> DealerGet(GenericRequest<DealerGetInModel> inModel)
        {
            var res = await _accountService.DealerGet(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("DealerPut")]
        public async Task<GenericResult<DealerPutOutModel>> DealerPut(GenericRequest<DealerPutInModel> inModel)
        {
            var res = await _accountService.DealerPut(inModel, false);

            return res;
        }


      



        [Authorize]
        [HttpPost]
        [Route("FinancialAdvisorList")]
        public async Task<GenericResult<FinancialAdvisorListOutModel>> FinancialAdvisorList(GenericRequest<FinancialAdvisorListInModel> inModel)
        {
            var res = await _accountService.FinancialAdvisorList(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("FinancialAdvisorGet")]
        public async Task<GenericResult<FinancialAdvisorGetOutModel>> FinancialAdvisorGet(GenericRequest<FinancialAdvisorGetInModel> inModel)
        {
            var res = await _accountService.FinancialAdvisorGet(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("FinancialAdvisorPut")]
        public async Task<GenericResult<FinancialAdvisorPutOutModel>> FinancialAdvisorPut(GenericRequest<FinancialAdvisorPutInModel> inModel)
        {
            var res = await _accountService.FinancialAdvisorPut(inModel, false);

            return res;
        }





        [Authorize]
        [HttpPost]
        [Route("CompanyList")]
        public async Task<GenericResult<CompanyListOutModel>> CompanyList(GenericRequest<CompanyListInModel> inModel)
        {
            var res = await _accountService.CompanyList(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("CompanyGet")]
        public async Task<GenericResult<CompanyGetOutModel>> CompanyGet(GenericRequest<CompanyGetInModel> inModel)
        {
            var res = await _accountService.CompanyGet(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("CompanyPut")]
        public async Task<GenericResult<CompanyPutOutModel>> CompanyPut(GenericRequest<CompanyPutInModel> inModel)
        {
            var res = await _accountService.CompanyPut(inModel, false);

            return res;
        }




        [Authorize]
        [HttpPost]
        [Route("UserCompanyList")]
        public async Task<GenericResult<UserCompanyListOutModel>> UserCompanyList(GenericRequest<UserCompanyListInModel> inModel)
        {
            var res = await _accountService.UserCompanyList(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("UserCompanyGet")]
        public async Task<GenericResult<UserCompanyGetOutModel>> UserCompanyGet(GenericRequest<UserCompanyGetInModel> inModel)
        {
            var res = await _accountService.UserCompanyGet(inModel, false);

            return res;
        }


        [Authorize]
        [HttpPost]
        [Route("UserCompanyPut")]
        public async Task<GenericResult<UserCompanyPutOutModel>> UserCompanyPut(GenericRequest<UserCompanyPutInModel> inModel)
        {
            var res = await _accountService.UserCompanyPut(inModel, false);

            return res;
        }




        [Authorize]
        [HttpPost]
        [Route("UserList")]
        public async Task<GenericResult<UserListOutModel>> UserList(GenericRequest<UserListInModel> inModel)
        {
            var res = await _accountService.UserList(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("UserGet")]
        public async Task<GenericResult<UserGetOutModel>> UserGet(GenericRequest<UserGetInModel> inModel)
        {
            var res = await _accountService.UserGet(inModel, false);

            return res;
        }
        [Authorize]
        [HttpPost]
        [Route("UserPut")]
        public async Task<GenericResult<UserPutOutModel>> UserPut(GenericRequest<UserPutInModel> inModel)
        {
            var res = await _accountService.UserPut(inModel, false);

            return res;
        }


        [Authorize]
        [HttpPost]
        [Route("UserDealerList")]
        public async Task<GenericResult<UserDealerListOutModel>> UserDealerList(GenericRequest<UserDealerListInModel> inModel)
        {
            var res = await _accountService.UserDealerList(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("UserDealerGet")]
        public async Task<GenericResult<UserDealerGetOutModel>> UserDealerGet(GenericRequest<UserDealerGetInModel> inModel)
        {
            var res = await _accountService.UserDealerGet(inModel, false);

            return res;
        }


        [Authorize]
        [HttpPost]
        [Route("UserDealerPut")]
        public async Task<GenericResult<UserDealerPutOutModel>> UserDealerPut(GenericRequest<UserDealerPutInModel> inModel)
        {
            var res = await _accountService.UserDealerPut(inModel, false);

            return res;
        }


        [Authorize]
        [HttpPost]
        [Route("UserFinancialAdvisorList")]
        public async Task<GenericResult<UserFinancialAdvisorListOutModel>> UserFinancialAdvisorList(GenericRequest<UserFinancialAdvisorListInModel> inModel)
        {
            var res = await _accountService.UserFinancialAdvisorList(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("UserFinancialAdvisorGet")]
        public async Task<GenericResult<UserFinancialAdvisorGetOutModel>> UserFinancialAdvisorGet(GenericRequest<UserFinancialAdvisorGetInModel> inModel)
        {
            var res = await _accountService.UserFinancialAdvisorGet(inModel, false);

            return res;
        }


        [Authorize]
        [HttpPost]
        [Route("UserFinancialAdvisorPut")]
        public async Task<GenericResult<UserFinancialAdvisorPutOutModel>> UserFinancialAdvisorPut(GenericRequest<UserFinancialAdvisorPutInModel> inModel)
        {
            var res = await _accountService.UserFinancialAdvisorPut(inModel, false);

            return res;
        }

        [Authorize]
        [HttpGet]
        [Route("Download/{fileName}")]
        public IActionResult Download(string fileName)
        {
            if (!FileJobScaricabile(fileName, out var filePath, out var info))
                return NotFound(new { message = InfrastructureMessages.Get("FileNotFound") });

            if (!_contentTypeProvider.TryGetContentType(fileName, out var contentType))
                contentType = "application/octet-stream";

            return PhysicalFile(filePath, contentType, info?.FileName ?? fileName);
        }

        // codice per il collegamento temporaneo al file (Account/DownloadFile): stessi controlli del download
        [Authorize]
        [HttpGet]
        [Route("DownloadCode/{fileName}")]
        public IActionResult DownloadCode(string fileName)
        {
            if (!FileJobScaricabile(fileName, out _, out _))
                return NotFound(new { message = InfrastructureMessages.Get("FileNotFound") });

            var jwt = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<nvxapp.server.service.ServerModels.JwtParameter>>().Value;
            return Ok(new { code = JobFileLink.Code(jwt.Key, fileName, DateTime.UtcNow.Add(JobFileLink.Durata)) });
        }

        /*
         Download del file di un job con il collegamento temporaneo (JobFileLink): niente sessione, vale il codice
         firmato nell'indirizzo, legato a quel file e valido pochi minuti. Risposta come allegato: il browser (anche
         l'app installata sul telefono) lo salva invece di aprirlo. Qualunque errore risponde "non trovato".
        */
        [AllowAnonymous]
        [HttpGet]
        [Route("DownloadFile/{fileName}")]
        public IActionResult DownloadFile(string fileName, [FromQuery] string? code)
        {
            var jwt = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<nvxapp.server.service.ServerModels.JwtParameter>>().Value;
            if (!NVXSystem.NomeFileJobValido(fileName) || !JobFileLink.Check(jwt.Key, fileName, code, DateTime.UtcNow))
                return NotFound(new { message = InfrastructureMessages.Get("FileNotFound") });

            var filePath = Path.Combine(NVXSystem.ExportFolder, fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = InfrastructureMessages.Get("FileNotFound") });

            if (!_contentTypeProvider.TryGetContentType(fileName, out var contentType))
                contentType = "application/octet-stream";
            return PhysicalFile(filePath, contentType, NVXSystem.InfoFileJob(fileName)?.FileName ?? fileName);
        }

        // file del job che l'utente collegato puo' scaricare: nome semplice con estensione ammessa (niente cartelle,
        // niente .meta), file presente e, se registrato (NVXSystem.SalvaFileJob/RegistraFileJob), lanciato da lui,
        // cioe' dall'utente del login anche se sta impersonando qualcuno (come le notifiche del job)
        private bool FileJobScaricabile(string fileName, out string filePath, out JobFileInfo? info)
        {
            filePath = string.Empty;
            info = null;
            if (!NVXSystem.NomeFileJobValido(fileName))
                return false;

            filePath = Path.Combine(NVXSystem.ExportFolder, fileName);
            if (!System.IO.File.Exists(filePath))
                return false;

            info = NVXSystem.InfoFileJob(fileName);
            if (info != null)
            {
                var userId = User.FindFirst("useridfirstconnection")?.Value;
                if (string.IsNullOrEmpty(userId))
                    userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || userId != info.Owner)
                    return false;
            }
            return true;
        }

        /*
         Download di un file prodotto da uno strumento MCP (report, export) con il collegamento temporaneo dato
         all'assistente AI (McpDownloadLink, ANALISI_INFRASTRUCTURE_MCP.md MCP-48). Niente sessione: vale il codice
         firmato nell'indirizzo, legato a quel file e valido pochi minuti. Qualunque errore risponde "non trovato".
        */
        [AllowAnonymous]
        [HttpGet]
        [Route("DownloadLink/{fileName}")]
        public IActionResult DownloadLink(string fileName, [FromQuery] string? code)
        {
            var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var jwt = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<nvxapp.server.service.ServerModels.JwtParameter>>().Value;
            if (!nvxapp.server.service.ClientServer_Service.Infrastructure.Mcp.McpOptions.Enabled(configuration)
                || !NVXSystem.NomeFileJobValido(fileName)
                || !nvxapp.server.service.ClientServer_Service.Infrastructure.Mcp.McpDownloadLink.Check(jwt.Key, fileName, code, DateTime.UtcNow))
                return NotFound(new { message = InfrastructureMessages.Get("FileNotFound") });

            var filePath = Path.Combine(NVXSystem.ExportFolder, fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = InfrastructureMessages.Get("FileNotFound") });

            if (!_contentTypeProvider.TryGetContentType(fileName, out var contentType))
                contentType = "application/octet-stream";
            return PhysicalFile(filePath, contentType, NVXSystem.InfoFileJob(fileName)?.FileName ?? fileName);
        }

        private static readonly FileExtensionContentTypeProvider _contentTypeProvider = new();
    }



}
