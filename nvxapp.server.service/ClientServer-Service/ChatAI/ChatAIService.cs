using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant;
using nvxapp.server.service.ClientServer_Service.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Infrastructure;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using nvxapp.server.service.Service.MyTableService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
