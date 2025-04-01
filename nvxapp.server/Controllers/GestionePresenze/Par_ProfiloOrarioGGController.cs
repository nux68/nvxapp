using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGG;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService.Models;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Par_ProfiloOrarioGGController : NvxControllerBase
    {
        private readonly IPar_ProfiloOrarioGGService _par_ProfiloOrarioGGService;

        public Par_ProfiloOrarioGGController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IPar_ProfiloOrarioGGService par_ProfiloOrarioGGService
        ) : base(httpContextAccessor)
        {
            _par_ProfiloOrarioGGService = par_ProfiloOrarioGGService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Par_ProfiloOrarioGGOutModel>> GetAll(GenericRequest<Par_ProfiloOrarioGGInModel> inModel)
        {
            var res = await _par_ProfiloOrarioGGService.GetAll(inModel, false);

            return res;
        }

        

    }



}
