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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService
{
    public class Az_SubCommessaAttivitaService : ServiceBase, IAz_SubCommessaAttivitaService
    {
        private readonly IAz_SubCommessaAttivitaRepository _az_SubCommessaAttivitaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Az_SubCommessaAttivitaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_SubCommessaAttivitaRepository az_SubCommessaAttivitaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SubCommessaAttivitaRepository = az_SubCommessaAttivitaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Az_SubCommessaAttivita_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessaAttivita_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessaAttivita_GetAll_OutModel retVal = new Az_SubCommessaAttivita_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_SubCommessaAttivita = _az_SubCommessaAttivitaRepository.FindAll().ToList();
                    retVal.Az_SubCommessaAttivita = _mapper.Map<List<Az_SubCommessaAttivitaModel>>(az_SubCommessaAttivita);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IAz_SubCommessaAttivitaService : IServiceBase
    {
        Task<GenericResult<Az_SubCommessaAttivita_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessaAttivita_GetAll_InModel> model, bool isSubProcess);
    }
}
