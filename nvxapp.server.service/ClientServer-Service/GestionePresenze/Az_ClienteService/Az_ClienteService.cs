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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_ClienteService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_ClienteService
{
    public class Az_ClienteService : ServiceBase, IAz_ClienteService
    {
        private readonly IAz_ClienteRepository _az_ClienteRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Az_ClienteService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_ClienteRepository az_ClienteRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_ClienteRepository = az_ClienteRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Az_Cliente_GetAll_OutModel>> GetAll(GenericRequest<Az_Cliente_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_Cliente_GetAll_OutModel retVal = new Az_Cliente_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_Cliente = _az_ClienteRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA.az_Anagrafica.Id).ToList();
                    retVal.Az_Cliente = _mapper.Map<List<Az_ClienteModel>>(az_Cliente);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IAz_ClienteService : IServiceBase
    {
        Task<GenericResult<Az_Cliente_GetAll_OutModel>> GetAll(GenericRequest<Az_Cliente_GetAll_InModel> model, bool isSubProcess);
    }
}
