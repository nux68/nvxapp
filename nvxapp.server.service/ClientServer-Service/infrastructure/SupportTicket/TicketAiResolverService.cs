using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket
{
    /*
     Risoluzione automatica AI dei ticket (PREDISPOSIZIONE).
     TicketService la chiama prima di ogni passaggio verso l'alto se il tipo ha AiResolution attivo
     (escluso il passaggio DEV -> SuperUser). Se risolve, il ticket torna a chi l'ha aperto come Risolto
     invece di salire. Per ora risponde sempre "non risolto": il ticket passa sempre al padre.
     L'implementazione reale potra' appoggiarsi al servizio ChatAI; se richiede tempo conviene
     eseguirla in background (Hangfire) con uno stato temporaneo.
    */
    public class TicketAiResolverService : ServiceBase, ITicketAiResolverService
    {
        public TicketAiResolverService(IMapper mapper,
                                       UserManager<ApplicationUser> userManager,
                                       IAspNetUsersRepository aspNetUsersRepository,
                                       IOptions<JwtParameter> jwtParameter,
                                       IHttpContextAccessor httpContextAccessor,
                                       IConfiguration configuration) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
        }

        public virtual async Task<GenericResult<TicketAiResolveOutModel>> TicketAiResolve(GenericRequest<TicketAiResolveInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TicketAiResolveOutModel { Resolved = false };

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
    }

    public interface ITicketAiResolverService : IServiceBase
    {
        Task<GenericResult<TicketAiResolveOutModel>> TicketAiResolve(GenericRequest<TicketAiResolveInModel> model, Boolean isSubProcess);
    }
}
