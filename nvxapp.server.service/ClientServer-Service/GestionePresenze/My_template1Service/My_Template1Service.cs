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
                                  IMy_template1Repository my_template1Repository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _my_template1Repository = my_template1Repository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<My_template1_GetAllOutModel>> GetAll(GenericRequest<My_template1_GetAllInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                My_template1_GetAllOutModel retVal = new My_template1_GetAllOutModel();
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




                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);

                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica != null)
                {
                    My_Template1? my_template1 = await _my_template1Repository.FindByIdAsync(model.Data.my_Template1.Id);
                    if (my_template1 == null)
                    {
                        my_template1 = _mapper.Map<My_Template1>(model.Data.my_Template1);
                        my_template1.IdAz_Anagrafica = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica.Id;
                    }
                    else
                    {
                        my_template1 = _mapper.Map<My_Template1>(model.Data.my_Template1);
                    }
                    my_template1 = await _my_template1Repository.UpsertAsync(my_template1);
                    retVal.my_Template1 = _mapper.Map<My_Template1Model>(my_template1);
                }



                return retVal;
            }, isSubProcess);
        }

        // DELETE
        public virtual async Task<GenericResult<My_template1_DeleteOutModel>> MyTemplate1Delete(GenericRequest<My_template1_DeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                My_template1_DeleteOutModel retVal = new My_template1_DeleteOutModel();
                var entity = await _my_template1Repository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                {
                    await _my_template1Repository.DeleteAsync(entity);
                    retVal.my_Template1 = _mapper.Map<My_Template1Model>(entity);
                }
                else
                {
                    retVal.AddMessage($"Elemento con Id {model.Data.Id} non trovato.", MessageType.Warning);
                }
                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IMy_template1Service : IServiceBase
    {
        public Task<GenericResult<My_template1_GetAllOutModel>> GetAll(GenericRequest<My_template1_GetAllInModel> model, Boolean isSubProcess);
        public Task<GenericResult<My_template1_GetOutModel>> MyTemplate1Get(GenericRequest<My_template1_GetInModel> model, Boolean isSubProcess);
        public Task<GenericResult<My_template1_PutOutModel>> MyTemplate1Put(GenericRequest<My_template1_PutInModel> model, Boolean isSubProcess);
        public Task<GenericResult<My_template1_DeleteOutModel>> MyTemplate1Delete(GenericRequest<My_template1_DeleteInModel> model, Boolean isSubProcess);
    }
}
