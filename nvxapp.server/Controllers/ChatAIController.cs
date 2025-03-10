using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.data.Repositories.Tenant;
using nvxapp.server.service.ClientServer_Service.Account.Models;
using nvxapp.server.service.ClientServer_Service.ChatAI;
using nvxapp.server.service.ClientServer_Service.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatAIController : NvxControllerBase
    {
        private readonly IChatAIService _chatAIService;

        public ChatAIController(
                                IHttpContextAccessor httpContextAccessor,

                                IChatAIService chatAIService
                              ) : base(httpContextAccessor)
        {
            _chatAIService = chatAIService;
        }

        //[Authorize]
        [HttpPost]
        [Route("SendMessage")]
        public async Task<GenericResult<ChatAIOutModel>> SendMessage(GenericRequest<ChatAIInModel> inModel)
        {
            var res = await _chatAIService.SendMessage(inModel, false);

            return res;
        }

    }
}
