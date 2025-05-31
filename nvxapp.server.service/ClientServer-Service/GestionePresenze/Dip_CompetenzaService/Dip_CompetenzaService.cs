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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_CompetenzaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_CompetenzaService
{
    public class Dip_CompetenzaService : ServiceBase, IDip_CompetenzaService
    {
        private readonly IDip_CompetenzaRepository _dip_CompetenzaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Dip_CompetenzaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IDip_CompetenzaRepository dip_CompetenzaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_CompetenzaRepository = dip_CompetenzaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Dip_Competenza_GetAll_OutModel>> GetAll(GenericRequest<Dip_Competenza_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_Competenza_GetAll_OutModel retVal = new Dip_Competenza_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var dip_Competenza = _dip_CompetenzaRepository.FindAll().ToList();
                    retVal.Dip_Competenza = _mapper.Map<List<Dip_CompetenzaModel>>(dip_Competenza);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IDip_CompetenzaService : IServiceBase
    {
        Task<GenericResult<Dip_Competenza_GetAll_OutModel>> GetAll(GenericRequest<Dip_Competenza_GetAll_InModel> model, bool isSubProcess);
    }
}
