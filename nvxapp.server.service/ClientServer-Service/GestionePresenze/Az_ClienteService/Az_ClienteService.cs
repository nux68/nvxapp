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

        public virtual async Task<GenericResult<Az_ClienteGetOutModel>> Az_ClienteGet(GenericRequest<Az_ClienteGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_ClienteGetOutModel retVal = new Az_ClienteGetOutModel();
                var az_Cliente = await _az_ClienteRepository.FindByIdAsync(model.Data.Id);
                if (az_Cliente != null)
                {
                    retVal.Az_Cliente = _mapper.Map<Az_ClienteModel>(az_Cliente);
                }
                else
                {
                    retVal.Az_Cliente = new Az_ClienteModel { Id = 0, IdAz_Anagrafica = 0 };
                }
                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_ClientePutOutModel>> Az_ClientePut(GenericRequest<Az_ClientePutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_ClientePutOutModel retVal = new Az_ClientePutOutModel();
                retVal.Az_Cliente = model.Data.Az_Cliente;

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);

                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_Cliente = await _az_ClienteRepository.FindByIdAsync(model.Data.Az_Cliente.Id);
                    if (az_Cliente == null)
                    {
                        az_Cliente = _mapper.Map<nvxapp.server.data.Entities.Tenant.Az_Cliente>(model.Data.Az_Cliente);
                        az_Cliente.IdAz_Anagrafica = company_DATA.az_Anagrafica.Id;
                    }
                    else
                    {
                        az_Cliente = _mapper.Map<nvxapp.server.data.Entities.Tenant.Az_Cliente>(model.Data.Az_Cliente);
                    }
                    az_Cliente = await _az_ClienteRepository.UpsertAsync(az_Cliente);
                    retVal.Az_Cliente = _mapper.Map<Az_ClienteModel>(az_Cliente);
                }
                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IAz_ClienteService : IServiceBase
    {
        Task<GenericResult<Az_Cliente_GetAll_OutModel>> GetAll(GenericRequest<Az_Cliente_GetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_ClienteGetOutModel>> Az_ClienteGet(GenericRequest<Az_ClienteGetInModel> model, bool isSubProcess);
        Task<GenericResult<Az_ClientePutOutModel>> Az_ClientePut(GenericRequest<Az_ClientePutInModel> model, bool isSubProcess);
    }
}
