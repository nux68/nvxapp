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
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CompetenzaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CompetenzaService
{
    public class Az_CompetenzaService : ServiceBase, IAz_CompetenzaService
    {
        private readonly IAz_CompetenzaRepository _az_CompetenzaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Az_CompetenzaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_CompetenzaRepository az_CompetenzaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_CompetenzaRepository = az_CompetenzaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Az_Competenza_GetAll_OutModel>> GetAll(GenericRequest<Az_Competenza_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_Competenza_GetAll_OutModel retVal = new Az_Competenza_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_Competenza = _az_CompetenzaRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA.az_Anagrafica.Id).ToList();
                    retVal.Az_Competenza = _mapper.Map<List<Az_CompetenzaModel>>(az_Competenza);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_CompetenzaGetOutModel>> Az_CompetenzaGet(GenericRequest<Az_CompetenzaGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_CompetenzaGetOutModel retVal = new Az_CompetenzaGetOutModel();
                var az_Competenza = await _az_CompetenzaRepository.FindByIdAsync(model.Data.Id);
                if (az_Competenza != null)
                {
                    retVal.Az_Competenza = _mapper.Map<Az_CompetenzaModel>(az_Competenza);
                }
                else
                {
                    retVal.Az_Competenza = new Az_CompetenzaModel();
                }
                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_CompetenzaPutOutModel>> Az_CompetenzaPut(GenericRequest<Az_CompetenzaPutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_CompetenzaPutOutModel retVal = new Az_CompetenzaPutOutModel();
                retVal.Az_Competenza = model.Data.Az_Competenza;

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_Competenza = await _az_CompetenzaRepository.FindByIdAsync(model.Data.Az_Competenza.Id);
                    if (az_Competenza == null)
                    {
                        az_Competenza = _mapper.Map<Az_Competenza>(model.Data.Az_Competenza);
                        az_Competenza.IdAz_Anagrafica = company_DATA.az_Anagrafica.Id;
                    }
                    else
                    {
                        az_Competenza = _mapper.Map<Az_Competenza>(model.Data.Az_Competenza);
                    }
                    az_Competenza = await _az_CompetenzaRepository.UpsertAsync(az_Competenza);
                    retVal.Az_Competenza = _mapper.Map<Az_CompetenzaModel>(az_Competenza);
                }
                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IAz_CompetenzaService : IServiceBase
    {
        Task<GenericResult<Az_Competenza_GetAll_OutModel>> GetAll(GenericRequest<Az_Competenza_GetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_CompetenzaGetOutModel>> Az_CompetenzaGet(GenericRequest<Az_CompetenzaGetInModel> model, bool isSubProcess);
        Task<GenericResult<Az_CompetenzaPutOutModel>> Az_CompetenzaPut(GenericRequest<Az_CompetenzaPutInModel> model, bool isSubProcess);
    }
}
