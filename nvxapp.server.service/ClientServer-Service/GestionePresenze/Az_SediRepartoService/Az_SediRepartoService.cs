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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoUserService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoUserService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService
{

    public class Az_SediRepartoService : ServiceBase, IAz_SediRepartoService
    {

        private readonly IAz_SediRepartoUserService _az_SediRepartoUserService;
        private readonly IAccountService _accountService;
        private readonly IAz_SediRepartoRepository _az_SediRepartoRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Az_SediRepartoService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAccountService accountService,
                                  IAz_SediRepartoUserService az_SediRepartoUserService,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_SediRepartoRepository az_RepartoRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SediRepartoRepository = az_RepartoRepository;
            _accountService = accountService;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _az_SediRepartoUserService = az_SediRepartoUserService;
        }

        public virtual async Task<GenericResult<Az_SediReparto_GetAll_OutModel>> GetAll(GenericRequest<Az_SediReparto_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediReparto_GetAll_OutModel retVal = new Az_SediReparto_GetAll_OutModel();


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

                    GenericRequest<Az_SediRepartoUser_GetAll_InModel> req1 = new GenericRequest<Az_SediRepartoUser_GetAll_InModel>();
                    req1.Data.IdAz_SediReparto = retVal.Az_SediReparto.Id;

                    var res1 = await _az_SediRepartoUserService.GetAll(req1, true);
                    if (res1.Success && res1.Data != null)
                    {

                    }

                    GenericRequest<UserCompanyListInModel> req2 = new GenericRequest<UserCompanyListInModel>();
                    var res2 = await _accountService.UserCompanyList(req2, true);
                    if (res2.Success && res2.Data != null)
                    {

                    }

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
        public Task<GenericResult<Az_SediReparto_GetAll_OutModel>> GetAll(GenericRequest<Az_SediReparto_GetAll_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Az_SediRepartoGetOutModel>> Az_SediRepartoGet(GenericRequest<Az_SediRepartoGetInModel> model, Boolean isSubProcess);
        public Task<GenericResult<Az_SediRepartoPutOutModel>> Az_SediRepartoPut(GenericRequest<Az_SediRepartoPutInModel> model, Boolean isSubProcess);
    }

}
