using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;


namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService
{

    public class TimeSheet_EngineService : ServiceBase, ITimeSheet_EngineService
    {
        //private readonly IPar_OrarioRepository _par_OrarioRepository;
        //private readonly IPar_OrarioIntervalloHHRepository _par_OrarioIntervalloHHRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public TimeSheet_EngineService(IMapper mapper,
                                      UserManager<ApplicationUser> userManager,
                                      IAspNetUsersRepository aspNetUsersRepository,
                                      IOptions<JwtParameter> jwtParameter,
                                      IHttpContextAccessor httpContextAccessor,
                                      IConfiguration configuration,

                                      IGestionePresenzeUserUtility gestionePresenzeUserUtility
                                      //IPar_OrarioRepository par_OrarioRepository,
                                      //IPar_OrarioIntervalloHHRepository par_OrarioIntervalloHHRepository
                                      ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            //_par_OrarioIntervalloHHRepository = par_OrarioIntervalloHHRepository;
            //_par_OrarioRepository = par_OrarioRepository;
        }

        
        public virtual async Task<GenericResult<TimeSheet_CalculateOutModel>> Calculate(GenericRequest<TimeSheet_CalculateInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                TimeSheet_CalculateOutModel retVal = new TimeSheet_CalculateOutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface ITimeSheet_EngineService : IServiceBase
    {
        
        Task<GenericResult<TimeSheet_CalculateOutModel>> Calculate(GenericRequest<TimeSheet_CalculateInModel> model, bool isSubProcess);

    }
}
