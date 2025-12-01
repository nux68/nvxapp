using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService
{
    public class Par_OrarioService : ServiceBase, IPar_OrarioService
    {
        private readonly IPar_OrarioRepository _par_OrarioRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Par_OrarioService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IPar_OrarioRepository par_OrarioRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_OrarioRepository = par_OrarioRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Par_Orario_GetAllOutModel>> GetAll(GenericRequest<Par_Orario_GetAllInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_Orario_GetAllOutModel();
                var entities = await _par_OrarioRepository.FindAll();
                retVal.Par_Orario = _mapper.Map<List<Par_OrarioModel>>(entities);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Orario_GetOutModel>> Par_OrarioGet(GenericRequest<Par_Orario_GetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_Orario_GetOutModel();
                var entity = await _par_OrarioRepository.FindByIdAsync(model.Data.Id);
                retVal.Par_Orario = _mapper.Map<Par_OrarioModel>(entity);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Orario_PutOutModel>> Par_OrarioPut(GenericRequest<Par_Orario_PutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_Orario_PutOutModel();
                var entity = _mapper.Map<Par_Orario>(model.Data.Par_Orario);

                int.TryParse(this.CurrentCompany, out int idCompany);
                var companyData = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);
                if (companyData?.az_Anagrafica != null)
                {
                    entity.IdAz_Anagrafica = companyData.az_Anagrafica.Id;
                }

                var updatedEntity = await _par_OrarioRepository.UpsertAsync(entity);
                retVal.Par_Orario = _mapper.Map<Par_OrarioModel>(updatedEntity);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_Orario_DeleteOutModel>> Par_OrarioDelete(GenericRequest<Par_Orario_DeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var entity = await _par_OrarioRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                {
                    await _par_OrarioRepository.DeleteAsync(entity);
                }
                return new Par_Orario_DeleteOutModel();
            }, isSubProcess);
        }
    }

    public interface IPar_OrarioService : IServiceBase
    {
        Task<GenericResult<Par_Orario_GetAllOutModel>> GetAll(GenericRequest<Par_Orario_GetAllInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Orario_GetOutModel>> Par_OrarioGet(GenericRequest<Par_Orario_GetInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Orario_PutOutModel>> Par_OrarioPut(GenericRequest<Par_Orario_PutInModel> model, bool isSubProcess);
        Task<GenericResult<Par_Orario_DeleteOutModel>> Par_OrarioDelete(GenericRequest<Par_Orario_DeleteInModel> model, bool isSubProcess);
    }
}
