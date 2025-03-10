using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Infrastructure;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;


namespace nvxapp.server.service.ClientServer_Service.ChatAI
{
    public class ChatAIService: ServiceBase, IChatAIService
    {

        public ChatAIService(IMapper mapper,
                           UserManager<ApplicationUser> userManager,
                           IAspNetUsersRepository aspNetUsersRepository,
                           IOptions<JwtParameter> jwtParameter,
                           IHttpContextAccessor httpContextAccessor,
                           IConfiguration configuration

                           ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            
        }

        public virtual async Task<GenericResult<ChatAIOutModel>> SendMessage(GenericRequest<ChatAIInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                ChatAIOutModel retVal = new ChatAIOutModel();

                retVal.Responce = "AI responce :" +  model.Data.Request;


                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }


    }

    public interface IChatAIService : IServiceBase
    {
        public Task<GenericResult<ChatAIOutModel>> SendMessage(GenericRequest<ChatAIInModel> model, Boolean isSubProcess);
    }

}
