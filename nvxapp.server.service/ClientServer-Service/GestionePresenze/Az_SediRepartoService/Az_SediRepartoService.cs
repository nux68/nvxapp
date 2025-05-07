using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.Base;
using nvxapp.server.service.Interfaces;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Microsoft.Extensions.Options;
using nvxapp.server.service.ServerModels;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService
{

    public class Az_SediRepartoService : ServiceBase, IAz_SediRepartoService
    {
        private readonly IAz_SediRepartoRepository _az_SediRepartoRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Az_SediRepartoService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_SediRepartoRepository az_RepartoRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SediRepartoRepository = az_RepartoRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Az_SediRepartoOutModel>> GetAll(GenericRequest<Az_SediRepartoInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediRepartoOutModel retVal = new Az_SediRepartoOutModel();


                var az_Rep = _az_SediRepartoRepository.FindAll(x => x.Id > 0).ToList();

                retVal.Az_SediReparto = _mapper.Map<List<Az_SediRepartoModel>>(az_Rep);

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_SediRepartoGetOutModel>> Az_SediRepartoGet(GenericRequest<Az_SediRepartoGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {

                Az_SediRepartoGetOutModel retVal = new Az_SediRepartoGetOutModel();

                var az_SediReparto = await _az_SediRepartoRepository.FindByIdAsync(model.Data.Id);
                if (az_SediReparto != null)
                {
                    retVal.Az_SediReparto = _mapper.Map<Az_SediRepartoModel>(az_SediReparto);
                }
                else
                {
                    retVal.Az_SediReparto = new Az_SediRepartoModel() { };
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_SediRepartoPutOutModel>> Az_SediRepartoPut(GenericRequest<Az_SediRepartoPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediRepartoPutOutModel retVal = new Az_SediRepartoPutOutModel();
                retVal.Az_SediReparto = model.Data.Az_SediReparto;


                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);

                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi != null)
                {
                    Az_SediReparto? az_SediReparto = await _az_SediRepartoRepository.FindByIdAsync(model.Data.Az_SediReparto.Id);
                    if (az_SediReparto == null)
                    {
                        az_SediReparto = _mapper.Map<Az_SediReparto>(model.Data.Az_SediReparto);
                        az_SediReparto.IdAz_Sedi = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi.Id;
                    }
                    else
                    {
                        //update 
                        az_SediReparto = _mapper.Map<Az_SediReparto>(model.Data.Az_SediReparto);
                    }

                    //aggiurna il valore ritornato al client
                    az_SediReparto = await _az_SediRepartoRepository.UpsertAsync(az_SediReparto);
                    retVal.Az_SediReparto = _mapper.Map<Az_SediRepartoModel>(az_SediReparto);
                }




                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IAz_SediRepartoService : IServiceBase
    {
        public Task<GenericResult<Az_SediRepartoOutModel>> GetAll( GenericRequest<Az_SediRepartoInModel> model, Boolean isSubProcess);
        public Task<GenericResult<Az_SediRepartoGetOutModel>> Az_SediRepartoGet(GenericRequest<Az_SediRepartoGetInModel> model, Boolean isSubProcess);
        public Task<GenericResult<Az_SediRepartoPutOutModel>> Az_SediRepartoPut(GenericRequest<Az_SediRepartoPutInModel> model, Boolean isSubProcess);
    }

}
