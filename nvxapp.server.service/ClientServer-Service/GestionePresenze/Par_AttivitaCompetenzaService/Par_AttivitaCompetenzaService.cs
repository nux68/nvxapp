using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaCompetenzaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaCompetenzaService
{
    public class Par_AttivitaCompetenzaService : ServiceBase, IPar_AttivitaCompetenzaService
    {
        private readonly IPar_AttivitaCompetenzaRepository _par_AttivitaCompetenzaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Par_AttivitaCompetenzaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IPar_AttivitaCompetenzaRepository par_AttivitaCompetenzaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_AttivitaCompetenzaRepository = par_AttivitaCompetenzaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Par_AttivitaCompetenza_GetAll_OutModel>> GetAll(GenericRequest<Par_AttivitaCompetenza_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_AttivitaCompetenza_GetAll_OutModel retVal = new Par_AttivitaCompetenza_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var par_AttivitaCompetenza = await _par_AttivitaCompetenzaRepository.FindAll();
                    retVal.Par_AttivitaCompetenza = _mapper.Map<List<Par_AttivitaCompetenzaModel>>(par_AttivitaCompetenza);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IPar_AttivitaCompetenzaService : IServiceBase
    {
        Task<GenericResult<Par_AttivitaCompetenza_GetAll_OutModel>> GetAll(GenericRequest<Par_AttivitaCompetenza_GetAll_InModel> model, bool isSubProcess);
    }
}
