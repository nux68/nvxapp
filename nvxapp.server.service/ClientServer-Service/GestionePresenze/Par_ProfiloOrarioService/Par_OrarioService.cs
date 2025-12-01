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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService
{

    public class Par_ProfiloOrarioService : ServiceBase, IPar_ProfiloOrarioService
    {
        private readonly IPar_ProfiloOrarioRepository _par_ProfiloOrarioRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Par_ProfiloOrarioService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IConfiguration configuration,

                                  IPar_ProfiloOrarioRepository par_ProfiloOrarioRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_ProfiloOrarioRepository = par_ProfiloOrarioRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Par_ProfiloOrario_GetAllOutModel>> GetAll(GenericRequest<Par_ProfiloOrario_GetAllInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_ProfiloOrario_GetAllOutModel retVal = new Par_ProfiloOrario_GetAllOutModel();

                var entities = await _par_ProfiloOrarioRepository.FindAll();
                retVal.Par_ProfiloOrario = _mapper.Map<List<Par_ProfiloOrarioModel>>(entities);
                return retVal;
            }, isSubProcess);
        }


        

         public virtual async Task<GenericResult<Par_ProfiloOrario_GetOutModel>> Par_ProfiloOrarioGet(GenericRequest<Par_ProfiloOrario_GetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_ProfiloOrario_GetOutModel();
                var entity = await _par_ProfiloOrarioRepository.FindByIdAsync(model.Data.Id);
                retVal.Par_ProfiloOrario = _mapper.Map<Par_ProfiloOrarioModel>(entity);
                return retVal;
            }, isSubProcess);
        }

         public virtual async Task<GenericResult<Par_ProfiloOrario_PutOutModel>> Par_ProfiloOrarioPut(GenericRequest<Par_ProfiloOrario_PutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_ProfiloOrario_PutOutModel();
                var entity = _mapper.Map<Par_ProfiloOrario>(model.Data.Par_ProfiloOrario);

                int.TryParse(this.CurrentCompany, out int idCompany);
                var companyData = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);
                if (companyData?.az_Anagrafica != null)
                {
                    entity.IdAz_Anagrafica = companyData.az_Anagrafica.Id;
                }

                var updatedEntity = await _par_ProfiloOrarioRepository.UpsertAsync(entity);
                retVal.Par_ProfiloOrario = _mapper.Map<Par_ProfiloOrarioModel>(updatedEntity);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ProfiloOrario_DeleteOutModel>> Par_ProfiloOrarioDelete(GenericRequest<Par_ProfiloOrario_DeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var entity = await _par_ProfiloOrarioRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                {
                    await _par_ProfiloOrarioRepository.DeleteAsync(entity);
                }
                return new Par_ProfiloOrario_DeleteOutModel();
            }, isSubProcess);
        }

    }

    public interface IPar_ProfiloOrarioService : IServiceBase
    {
        public Task<GenericResult<Par_ProfiloOrario_GetAllOutModel>> GetAll( GenericRequest<Par_ProfiloOrario_GetAllInModel> model, Boolean isSubProcess);
        Task<GenericResult<Par_ProfiloOrario_GetOutModel>> Par_ProfiloOrarioGet(GenericRequest<Par_ProfiloOrario_GetInModel> model, bool isSubProcess);
        Task<GenericResult<Par_ProfiloOrario_PutOutModel>> Par_ProfiloOrarioPut(GenericRequest<Par_ProfiloOrario_PutInModel> model, bool isSubProcess);
        Task<GenericResult<Par_ProfiloOrario_DeleteOutModel>> Par_ProfiloOrarioDelete(GenericRequest<Par_ProfiloOrario_DeleteInModel> model, bool isSubProcess);
    
    }
}
