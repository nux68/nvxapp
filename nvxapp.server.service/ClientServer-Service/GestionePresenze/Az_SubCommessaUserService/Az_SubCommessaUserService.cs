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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaUserService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaUserService
{
    public class Az_SubCommessaUserService : ServiceBase, IAz_SubCommessaUserService
    {
        private readonly IAz_SubCommessaUserRepository _az_SubCommessaUserRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Az_SubCommessaUserService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_SubCommessaUserRepository az_SubCommessaUserRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SubCommessaUserRepository = az_SubCommessaUserRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Az_SubCommessaUser_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessaUser_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessaUser_GetAll_OutModel retVal = new Az_SubCommessaUser_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var entities = await _az_SubCommessaUserRepository.FindAll();
                    retVal.Az_SubCommessaUser = _mapper.Map<List<Az_SubCommessaUserModel>>(entities);
                }

                return retVal;
            }, isSubProcess);
        }
    }

    public interface IAz_SubCommessaUserService : IServiceBase
    {
        Task<GenericResult<Az_SubCommessaUser_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessaUser_GetAll_InModel> model, bool isSubProcess);
    }
}