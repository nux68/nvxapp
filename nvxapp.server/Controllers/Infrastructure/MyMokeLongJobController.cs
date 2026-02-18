using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.infrastructure.MyMokeLongJob;
using nvxapp.server.service.ClientServer_Service.infrastructure.MyMokeLongJob.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.Infrastructure
{
    
    
    [ApiController]
    [Route("api/[controller]")]
    public class MyMokeLongJobController : NvxControllerBase 
    {
        private readonly IMyMokeLongJobService _myMokeLongJobService;
        

        public MyMokeLongJobController(IHttpContextAccessor httpContextAccessor,
                                       IMyMokeLongJobService myMokeLongJobService
            ): base(httpContextAccessor)
        {
            _myMokeLongJobService = myMokeLongJobService;
        }

        
        [HttpPost]
        [Route("StartJob")]
        public async Task<GenericResult<MyMokeLongJobOutModel>> StartJob(GenericRequest<MyMokeLongJobInModel> request)
        {

            var result = await _myMokeLongJobService.StartJob(request, isSubProcess: false);

            return result;
        }
    }
}