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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCauService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_CausaliService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCauService
{
    public class Par_ExportCauService : ServiceBase, IPar_ExportCauService
    {
        private readonly IPar_ExportCauRepository          _par_ExportCauRepository;
        private readonly IGestionePresenzeUserUtility      _gestionePresenzeUserUtility;
        private readonly IPar_ExportCau_CausaliService     _par_ExportCau_CausaliService;

        public Par_ExportCauService(IMapper mapper,
                                    UserManager<ApplicationUser> userManager,
                                    IAspNetUsersRepository aspNetUsersRepository,
                                    IOptions<JwtParameter> jwtParameter,
                                    IHttpContextAccessor httpContextAccessor,
                                    IConfiguration configuration,
                                    IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                    IPar_ExportCau_CausaliService par_ExportCau_CausaliService,
                                    IPar_ExportCauRepository par_ExportCauRepository)
            : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_ExportCauRepository      = par_ExportCauRepository;
            _gestionePresenzeUserUtility  = gestionePresenzeUserUtility;
            _par_ExportCau_CausaliService = par_ExportCau_CausaliService;
        }

        public virtual async Task<GenericResult<Par_ExportCau_GetAll_OutModel>> GetAll(GenericRequest<Par_ExportCau_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_ExportCau_GetAll_OutModel();
                var entities = await _par_ExportCauRepository.FindAll();
                retVal.Par_ExportCau = _mapper.Map<List<Par_ExportCauModel>>(entities);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ExportCau_Get_OutModel>> Par_ExportCauGet(GenericRequest<Par_ExportCau_Get_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_ExportCau_Get_OutModel();

                int.TryParse(this.CurrentCompany, out int idCompany);
                var companyData = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);
                if (companyData?.az_Anagrafica != null)
                {
                    var entity = await _par_ExportCauRepository.FindByIdAsync(model.Data.Id);
                    if (entity == null)
                    {
                        entity = new Par_ExportCau()
                        {
                            IdAz_Anagrafica = companyData.az_Anagrafica.Id,
                            Codice          = "0000",
                            Descrizione     = "Nuovo Export",
                            TipoFile        = Par_Export_TipoFile.CSV
                        };
                    }
                    retVal.Par_ExportCau = _mapper.Map<Par_ExportCauModel>(entity);

                    var req_1 = new GenericRequest<Par_ExportCau_Causali_Get_InModel>();
                    req_1.Data = new Par_ExportCau_Causali_Get_InModel() { Id = model.Data.Id };
                    var res_1 = await _par_ExportCau_CausaliService.Par_ExportCau_Causali_Get(req_1, true);
                    if (res_1.Success && res_1.Data != null)
                        retVal.Par_ExportCau_Causali = res_1.Data.Par_ExportCau_Causali;
                }

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ExportCau_Put_OutModel>> Par_ExportCauPut(GenericRequest<Par_ExportCau_Put_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_ExportCau_Put_OutModel();
                var entity = _mapper.Map<Par_ExportCau>(model.Data.Par_ExportCau);

                int.TryParse(this.CurrentCompany, out int idCompany);
                var companyData = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);
                if (companyData?.az_Anagrafica != null)
                    entity.IdAz_Anagrafica = companyData.az_Anagrafica.Id;

                var updatedEntity = await _par_ExportCauRepository.UpsertAsync(entity);
                retVal.Par_ExportCau = _mapper.Map<Par_ExportCauModel>(updatedEntity);

                var req_1 = new GenericRequest<Par_ExportCau_Causali_Put_InModel>();
                req_1.Data = new Par_ExportCau_Causali_Put_InModel()
                {
                    IdPar_ExportCau     = retVal.Par_ExportCau.Id,
                    Par_ExportCau_Causali = model.Data.Par_ExportCau_Causali
                };
                var res_1 = await _par_ExportCau_CausaliService.Par_ExportCau_Causali_Put(req_1, true);
                if (res_1.Success && res_1.Data != null)
                    retVal.Par_ExportCau_Causali = res_1.Data.Par_ExportCau_Causali;

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ExportCau_Delete_OutModel>> Par_ExportCauDelete(GenericRequest<Par_ExportCau_Delete_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var entity = await _par_ExportCauRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                    await _par_ExportCauRepository.DeleteAsync(entity);

                return new Par_ExportCau_Delete_OutModel();
            }, isSubProcess);
        }
    }

    public interface IPar_ExportCauService : IServiceBase
    {
        Task<GenericResult<Par_ExportCau_GetAll_OutModel>>  GetAll(GenericRequest<Par_ExportCau_GetAll_InModel>   model, bool isSubProcess);
        Task<GenericResult<Par_ExportCau_Get_OutModel>>     Par_ExportCauGet(GenericRequest<Par_ExportCau_Get_InModel>    model, bool isSubProcess);
        Task<GenericResult<Par_ExportCau_Put_OutModel>>     Par_ExportCauPut(GenericRequest<Par_ExportCau_Put_InModel>    model, bool isSubProcess);
        Task<GenericResult<Par_ExportCau_Delete_OutModel>>  Par_ExportCauDelete(GenericRequest<Par_ExportCau_Delete_InModel> model, bool isSubProcess);
    }
}
