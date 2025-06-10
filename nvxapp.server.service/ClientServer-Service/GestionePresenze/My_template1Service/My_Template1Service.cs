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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.My_template1Service.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.My_template1Service
{
    public class My_Template1Service : ServiceBase, IMy_template1Service
    {
        private readonly IMy_template1Repository _my_template1Repository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public My_Template1Service(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IMy_template1Repository my_template1Repository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _my_template1Repository = my_template1Repository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<My_template1OutModel>> GetAll(GenericRequest<My_template1InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                My_template1OutModel retVal = new My_template1OutModel();
                // Esempio: carica tutti dal repository
                var entities = await _my_template1Repository.FindAll();
                retVal.my_Template1 = _mapper.Map<List<My_Template1Model>>(entities);
                return retVal;
            }, isSubProcess);
        }

        
        public virtual async Task<GenericResult<My_template1_GetOutModel>> MyTemplate1Get(GenericRequest<My_template1_GetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                My_template1_GetOutModel retVal = new My_template1_GetOutModel();
                
                
                var entity = await _my_template1Repository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                {
                    retVal.my_Template1 = _mapper.Map<My_Template1Model>(entity);
                }
                else
                {
                    retVal.my_Template1 = new My_Template1Model() { };
                }


                return retVal;
            }, isSubProcess);
        }

        
        public virtual async Task<GenericResult<My_template1_PutOutModel>> MyTemplate1Put(GenericRequest<My_template1_PutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                My_template1_PutOutModel retVal = new My_template1_PutOutModel();
                
                retVal.my_Template1 = model.Data.my_Template1;
                
                
                    //string userId = string.IsNullOrEmpty(model.Data.IdAspNetUsers) ? this.CurrentUserId : model.Data.IdAspNetUsers;

                    //User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(userId, true);

                    //if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)


                    //My_Template1? my_template1 = await _my_template1Repository.FindByIdAsync(model.Data.my_Template1.Id);
                    //if (my_template1 == null)
                    //{
                    //    my_template1 = _mapper.Map<My_Template1>(model.Data.my_Template1);
                    //    my_template1.Id = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id;
                    //}
                    //else
                    //{
                    //    //update 
                    //    my_template1 = _mapper.Map<My_Template1>(model.Data.my_Template1);
                    //}

                    ////aggiurna il valore ritornato al client
                    //my_template1 = await _my_template1Repository.UpsertAsync(my_template1);
                    //retVal.my_Template1 = _mapper.Map<My_Template1Model>(my_template1);
                


                return retVal;
            }, isSubProcess);
        }
    }

    public interface IMy_template1Service : IServiceBase
    {
        public Task<GenericResult<My_template1OutModel>> GetAll(GenericRequest<My_template1InModel> model, Boolean isSubProcess);
        public Task<GenericResult<My_template1_GetOutModel>> MyTemplate1Get(GenericRequest<My_template1_GetInModel> model, Boolean isSubProcess);
        public Task<GenericResult<My_template1_PutOutModel>> MyTemplate1Put(GenericRequest<My_template1_PutInModel> model, Boolean isSubProcess);
    }
}
