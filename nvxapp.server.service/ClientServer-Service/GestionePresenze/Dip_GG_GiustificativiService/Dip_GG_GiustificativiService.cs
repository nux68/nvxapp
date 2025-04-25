using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService
{

    public class Dip_GG_GiustificativiService : ServiceBase, IDip_GG_GiustificativiService
    {
        private readonly IDip_GG_GiustificativiRepository _Dip_GG_GiustificativiRepository;

        public Dip_GG_GiustificativiService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IDip_GG_GiustificativiRepository Dip_GG_GiustificativiRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Dip_GG_GiustificativiRepository = Dip_GG_GiustificativiRepository;
        }

        public virtual async Task<GenericResult<Dip_GG_Giustificativi_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Giustificativi_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Giustificativi_GetAll_OutModel retVal = new Dip_GG_Giustificativi_GetAll_OutModel();


                for (int i = 1; i < 6;  i++)
                {
                    retVal.Dip_GG_Giustificativi.Add(new Dip_GG_GiustificativiModel()
                    {
                        Id = i,
                        IdDip_RapportoLavoro = 1,
                        Data = DateTime.Now.AddDays(i),
                        IdJustificationType = 0,
                        InputType = JustificationInputType.Manual,
                        Hours = new TimeSpan(4,0,0),
                        From = new TimeSpan(9, 0,0),
                        IdPar_Giustificativi = 1,
                        RichiestaStato = StatoRichiesta.Diretta,
                        IdDip_Richiesta = 0
                    });
                }


                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IDip_GG_GiustificativiService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_Giustificativi_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Giustificativi_GetAll_InModel> model, Boolean isSubProcess);
    }
}
