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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CompetenzaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CompetenzaService
{
    public class Par_CompetenzaService : ServiceBase, IPar_CompetenzaService
    {
        private readonly IPar_CompetenzaRepository _par_CompetenzaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Par_CompetenzaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IPar_CompetenzaRepository az_CompetenzaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_CompetenzaRepository = az_CompetenzaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Par_Competenza_GetAll_OutModel>> GetAll(GenericRequest<Par_Competenza_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_Competenza_GetAll_OutModel retVal = new Par_Competenza_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_Competenza = _par_CompetenzaRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA.az_Anagrafica.Id).ToList();
                    retVal.Par_Competenza = _mapper.Map<List<Par_CompetenzaModel>>(az_Competenza);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_CompetenzaGetOutModel>> Par_CompetenzaGet(GenericRequest<Par_CompetenzaGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_CompetenzaGetOutModel retVal = new Par_CompetenzaGetOutModel();
                var az_Competenza = await _par_CompetenzaRepository.FindByIdAsync(model.Data.Id);
                if (az_Competenza != null)
                {
                    retVal.Par_Competenza = _mapper.Map<Par_CompetenzaModel>(az_Competenza);
                }
                else
                {
                    retVal.Par_Competenza = new Par_CompetenzaModel();
                }
                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_CompetenzaPutOutModel>> Par_CompetenzaPut(GenericRequest<Par_CompetenzaPutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_CompetenzaPutOutModel retVal = new Par_CompetenzaPutOutModel();
                retVal.Par_Competenza = model.Data.Par_Competenza;

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_Competenza = await _par_CompetenzaRepository.FindByIdAsync(model.Data.Par_Competenza.Id);
                    if (az_Competenza == null)
                    {
                        az_Competenza = _mapper.Map<Par_Competenza>(model.Data.Par_Competenza);
                        az_Competenza.IdAz_Anagrafica = company_DATA.az_Anagrafica.Id;
                    }
                    else
                    {
                        az_Competenza = _mapper.Map<Par_Competenza>(model.Data.Par_Competenza);
                    }
                    az_Competenza = await _par_CompetenzaRepository.UpsertAsync(az_Competenza);
                    retVal.Par_Competenza = _mapper.Map<Par_CompetenzaModel>(az_Competenza);
                }
                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IPar_CompetenzaService : IServiceBase
    {
        Task<GenericResult<Par_Competenza_GetAll_OutModel>> GetAll(GenericRequest<Par_Competenza_GetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<Par_CompetenzaGetOutModel>> Par_CompetenzaGet(GenericRequest<Par_CompetenzaGetInModel> model, bool isSubProcess);
        Task<GenericResult<Par_CompetenzaPutOutModel>> Par_CompetenzaPut(GenericRequest<Par_CompetenzaPutInModel> model, bool isSubProcess);
    }
}
