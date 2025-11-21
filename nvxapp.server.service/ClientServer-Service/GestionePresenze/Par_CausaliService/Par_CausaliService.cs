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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService
{

    public class Par_CausaliService : ServiceBase, IPar_CausaliService
    {
        private readonly IPar_CausaliRepository _par_CausaliRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Par_CausaliService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IPar_CausaliRepository par_CausaliRepository,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_CausaliRepository = par_CausaliRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Par_CausaliOutModel>> GetAll(GenericRequest<Par_CausaliInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_CausaliOutModel retVal = new Par_CausaliOutModel();

                int idCompany;
                int.TryParse(this.CurrentCompany, out idCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);

                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica != null)
                {
                    var par_Causali_all = _par_CausaliRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id).ToList();
                    retVal.Par_Causali = _mapper.Map<List<Par_CausaliModel>>(par_Causali_all);
                }

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_CausaliGetOutModel>> Par_CausaliGet(GenericRequest<Par_CausaliGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_CausaliGetOutModel retVal = new Par_CausaliGetOutModel();

                var par_Causale = await _par_CausaliRepository.FindByIdAsync(model.Data.Id);
                if (par_Causale != null)
                {
                    retVal.Par_Causale = _mapper.Map<Par_CausaliModel>(par_Causale);
                }
                else
                {
                    retVal.Par_Causale = new Par_CausaliModel { };
                }

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_CausaliPutOutModel>> Par_CausaliPut(GenericRequest<Par_CausaliPutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_CausaliPutOutModel retVal = new Par_CausaliPutOutModel();
                retVal.Par_Causale = model.Data.Par_Causale;

                int idCompany;
                int.TryParse(this.CurrentCompany, out idCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);

                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica != null)
                {
                    Par_Causali? par_Causale = await _par_CausaliRepository.FindByIdAsync(model.Data.Par_Causale.Id);
                    if (par_Causale == null)
                    {
                        par_Causale = _mapper.Map<Par_Causali>(model.Data.Par_Causale);
                        par_Causale.IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id;
                    }
                    else
                    {
                        //update 
                        par_Causale = _mapper.Map<Par_Causali>(model.Data.Par_Causale);
                    }

                    //aggiurna il valore ritornato al client
                    par_Causale = await _par_CausaliRepository.UpsertAsync(par_Causale);
                    retVal.Par_Causale = _mapper.Map<Par_CausaliModel>(par_Causale);
                }

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
    }

    public interface IPar_CausaliService : IServiceBase
    {
        public Task<GenericResult<Par_CausaliOutModel>> GetAll(GenericRequest<Par_CausaliInModel> model, bool isSubProcess);
        public Task<GenericResult<Par_CausaliGetOutModel>> Par_CausaliGet(GenericRequest<Par_CausaliGetInModel> model, bool isSubProcess);
        public Task<GenericResult<Par_CausaliPutOutModel>> Par_CausaliPut(GenericRequest<Par_CausaliPutInModel> model, bool isSubProcess);
    }
}
