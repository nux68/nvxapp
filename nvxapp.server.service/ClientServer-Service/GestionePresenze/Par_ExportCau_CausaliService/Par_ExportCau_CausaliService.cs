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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_CausaliService
{
    public class Par_ExportCau_CausaliService : ServiceBase, IPar_ExportCau_CausaliService
    {
        private readonly IPar_CausaliRepository            _par_CausaliRepository;
        private readonly IPar_ExportCauRepository          _par_ExportCauRepository;
        private readonly IPar_ExportCau_CausaliRepository  _par_ExportCau_CausaliRepository;
        private readonly IGestionePresenzeUserUtility      _gestionePresenzeUserUtility;

        public Par_ExportCau_CausaliService(IMapper mapper,
                                            UserManager<ApplicationUser> userManager,
                                            IAspNetUsersRepository aspNetUsersRepository,
                                            IOptions<JwtParameter> jwtParameter,
                                            IHttpContextAccessor httpContextAccessor,
                                            IConfiguration configuration,
                                            IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                            IPar_ExportCauRepository par_ExportCauRepository,
                                            IPar_CausaliRepository            par_CausaliRepository,
                                            IPar_ExportCau_CausaliRepository par_ExportCau_CausaliRepository)
            : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility     = gestionePresenzeUserUtility;
            _par_ExportCauRepository         = par_ExportCauRepository;
            _par_ExportCau_CausaliRepository = par_ExportCau_CausaliRepository;
            _par_CausaliRepository           = par_CausaliRepository;
        }


        public virtual async Task<GenericResult<Par_ExportCau_Causali_Get_OutModel>>  Par_ExportCau_Causali_Get(GenericRequest<Par_ExportCau_Causali_Get_InModel>  model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_ExportCau_Causali_Get_OutModel();

                int.TryParse(this.CurrentCompany, out int idCompany);
                var companyData = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);
                if (companyData?.az_Anagrafica != null)
                {
                    var items = _par_ExportCau_CausaliRepository.FindAll(x => x.Id == model.Data.Id).FirstOrDefault();
                    if(items==null)
                    {
                        var cau = _par_CausaliRepository.FindAll(x=> x.Id>0).FirstOrDefault();

                        items = new Par_ExportCau_Causali()
                        {
                            IdPar_ExportCau = 0,
                            Codice ="XX",
                            IdCausale=cau!=null?cau.Id:0,
                            TipoElaborazione= Par_Export_TipoElaborazione.Gionaliera,
                            TipoUnita= Par_Export_TipoUnita.Ore
                        };
                    }

                    retVal.Par_ExportCau_Causali = _mapper.Map<Par_ExportCau_CausaliModel>(items);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Par_ExportCau_Causali_GetAll_4Edit_OutModel>> Par_ExportCau_Causali_GetAll_4Edit(GenericRequest<Par_ExportCau_Causali_GetAll_4Edit_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_ExportCau_Causali_GetAll_4Edit_OutModel();

                int.TryParse(this.CurrentCompany, out int idCompany);
                var companyData = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);
                if (companyData?.az_Anagrafica != null)
                {
                    var items = _par_ExportCau_CausaliRepository
                        .FindAll(x => x.IdPar_ExportCau == model.Data.Id)
                        .ToList();

                    retVal.Par_ExportCau_Causali = _mapper.Map<List<Par_ExportCau_CausaliModel>>(items);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Par_ExportCau_Causali_PutAll_4Edit_OutModel>> Par_ExportCau_Causali_PutAll_4Edit(GenericRequest<Par_ExportCau_Causali_PutAll_4Edit_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_ExportCau_Causali_PutAll_4Edit_OutModel();

                int.TryParse(this.CurrentCompany, out int idCompany);
                var companyData = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);
                if (companyData?.az_Anagrafica != null)
                {
                    var par_ExportCau = _par_ExportCauRepository.FindById(model.Data.IdPar_ExportCau);
                    if (par_ExportCau != null)
                    {
                        // normalizza gli id temporanei negativi (nuovi record)
                        model.Data.Par_ExportCau_Causali
                            .Where(x => x.Id < 0)
                            .ToList()
                            .ForEach(x => x.Id = 0);

                        // elimina i dettagli rimossi lato client
                        var existing = _par_ExportCau_CausaliRepository
                            .FindAll(x => x.IdPar_ExportCau == model.Data.IdPar_ExportCau)
                            .ToList();

                        foreach (var item in existing)
                        {
                            if (!model.Data.Par_ExportCau_Causali.Any(x => x.Id == item.Id))
                                await _par_ExportCau_CausaliRepository.DeleteAsync(item);
                        }

                        // upsert dei dettagli ricevuti dal client
                        foreach (var item in model.Data.Par_ExportCau_Causali)
                        {
                            var entity = _mapper.Map<Par_ExportCau_Causali>(item);
                            entity.IdPar_ExportCau = model.Data.IdPar_ExportCau;
                            await _par_ExportCau_CausaliRepository.UpsertAsync(entity);
                        }

                        // rilegge i dati aggiornati
                        var req_1 = new GenericRequest<Par_ExportCau_Causali_GetAll_4Edit_InModel>();
                        req_1.Data = new Par_ExportCau_Causali_GetAll_4Edit_InModel() { Id = model.Data.IdPar_ExportCau };
                        var res_1 = await Par_ExportCau_Causali_GetAll_4Edit(req_1, true);
                        if (res_1.Success && res_1.Data != null)
                            retVal.Par_ExportCau_Causali = res_1.Data.Par_ExportCau_Causali;
                    }
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IPar_ExportCau_CausaliService : IServiceBase
    {
        Task<GenericResult<Par_ExportCau_Causali_Get_OutModel>>  Par_ExportCau_Causali_Get(GenericRequest<Par_ExportCau_Causali_Get_InModel>  model, bool isSubProcess);
        Task<GenericResult<Par_ExportCau_Causali_GetAll_4Edit_OutModel>>  Par_ExportCau_Causali_GetAll_4Edit(GenericRequest<Par_ExportCau_Causali_GetAll_4Edit_InModel>  model, bool isSubProcess);
        Task<GenericResult<Par_ExportCau_Causali_PutAll_4Edit_OutModel>>  Par_ExportCau_Causali_PutAll_4Edit(GenericRequest<Par_ExportCau_Causali_PutAll_4Edit_InModel>  model, bool isSubProcess);
    }
}
