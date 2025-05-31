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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AttivitaCompetenzaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AttivitaCompetenzaService
{
    public class Az_AttivitaCompetenzaService : ServiceBase, IAz_AttivitaCompetenzaService
    {
        private readonly IAz_AttivitaCompetenzaRepository _az_AttivitaCompetenzaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Az_AttivitaCompetenzaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_AttivitaCompetenzaRepository az_AttivitaCompetenzaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_AttivitaCompetenzaRepository = az_AttivitaCompetenzaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Az_AttivitaCompetenza_GetAll_OutModel>> GetAll(GenericRequest<Az_AttivitaCompetenza_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_AttivitaCompetenza_GetAll_OutModel retVal = new Az_AttivitaCompetenza_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    // Qui puoi filtrare per IdAz_Attivita se necessario
                    var az_AttivitaCompetenza = _az_AttivitaCompetenzaRepository.FindAll().ToList();
                    retVal.Az_AttivitaCompetenza = _mapper.Map<List<Az_AttivitaCompetenzaModel>>(az_AttivitaCompetenza);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IAz_AttivitaCompetenzaService : IServiceBase
    {
        Task<GenericResult<Az_AttivitaCompetenza_GetAll_OutModel>> GetAll(GenericRequest<Az_AttivitaCompetenza_GetAll_InModel> model, bool isSubProcess);
    }
}
