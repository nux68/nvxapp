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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaService
{
    public class Az_CommessaService : ServiceBase, IAz_CommessaService
    {


        private readonly IAz_SubCommessaService _az_SubCommessaService;

        private readonly IAz_CommessaRepository _az_CommessaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        private readonly IAz_SubCommessaSediRepartoRepository _az_SubCommessaSediRepartoRepository;



        public Az_CommessaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IAz_SubCommessaService az_SubCommessaService,
                                  IAz_SubCommessaSediRepartoRepository az_SubCommessaSediRepartoRepository,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_CommessaRepository az_CommessaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_CommessaRepository = az_CommessaRepository;
            _az_SubCommessaService = az_SubCommessaService;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _az_SubCommessaSediRepartoRepository = az_SubCommessaSediRepartoRepository;
        }

        public virtual async Task<GenericResult<Az_Commessa_GetAll_OutModel>> GetAll(GenericRequest<Az_Commessa_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_Commessa_GetAll_OutModel retVal = new Az_Commessa_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_Commessa = _az_CommessaRepository.FindAll(x => x.IdAz_Anagrafica == company_DATA.az_Anagrafica.Id).ToList();
                    retVal.Az_Commessa = _mapper.Map<List<Az_CommessaModel>>(az_Commessa);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_CommessaGetOutModel>> AZ_CommessaGet(GenericRequest<Az_CommessaGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_CommessaGetOutModel retVal = new Az_CommessaGetOutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var reqAz_Sub = new GenericRequest<Az_SubCommessa_GetAll_4Edit_InModel>();
                    reqAz_Sub.Data.Id= model.Data.Id; // id della commessa
                    var resAz_Sub = await _az_SubCommessaService.GetAll_4Edit(reqAz_Sub, true);

                    if (resAz_Sub.Success && resAz_Sub.Data != null)
                    {
                        var commessa = await _az_CommessaRepository.FindByIdAsync(model.Data.Id);
                        if (commessa != null)
                            retVal.Az_Commessa = _mapper.Map<Az_CommessaModel>(commessa);
                        else
                            retVal.Az_Commessa = new Az_CommessaModel();

                        retVal.Az_SubCommessa = resAz_Sub.Data.Az_SubCommessa;
                    }
                }


                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_CommessaPutOutModel>> AZ_CommessaPut(GenericRequest<Az_CommessaPutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_CommessaPutOutModel retVal = new Az_CommessaPutOutModel();
                retVal.Az_Commessa = model.Data.Az_Commessa;
                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var commessa = await _az_CommessaRepository.FindByIdAsync(model.Data.Az_Commessa.Id);
                    if (commessa == null)
                    {
                        commessa = _mapper.Map<Az_Commessa>(model.Data.Az_Commessa);
                        commessa.IdAz_Anagrafica = company_DATA.az_Anagrafica.Id;
                    }
                    else
                    {
                        commessa = _mapper.Map<Az_Commessa>(model.Data.Az_Commessa);
                    }
                    commessa = await _az_CommessaRepository.UpsertAsync(commessa);
                    retVal.Az_Commessa = _mapper.Map<Az_CommessaModel>(commessa);


                    var reqAz_Sub = new GenericRequest<Az_SubCommessa_PutAll_4Edit_InModel>();
                    reqAz_Sub.Data.Id= retVal.Az_Commessa.Id; 
                    reqAz_Sub.Data.Az_SubCommessa = model.Data.Az_SubCommessa;
                    var resAz_Sub = await _az_SubCommessaService.PutAll_4Edit(reqAz_Sub, true);

                    if (resAz_Sub.Success && resAz_Sub.Data != null)
                    {
                        retVal.Az_SubCommessa = resAz_Sub.Data.Az_SubCommessa;
                    }


                }
                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_CommessaDeleteOutModel>> AZ_CommessaDelete(GenericRequest<Az_CommessaDeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_CommessaDeleteOutModel retVal = new Az_CommessaDeleteOutModel();
                var commessa = await _az_CommessaRepository.FindByIdAsync(model.Data.Id);
                if (commessa != null)
                {
                    retVal.Az_Commessa = _mapper.Map<Az_CommessaModel>(commessa);
                    await _az_CommessaRepository.DeleteAsync(commessa);
                }
                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IAz_CommessaService : IServiceBase
    {
        Task<GenericResult<Az_Commessa_GetAll_OutModel>> GetAll(GenericRequest<Az_Commessa_GetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_CommessaGetOutModel>> AZ_CommessaGet(GenericRequest<Az_CommessaGetInModel> model, bool isSubProcess);
        Task<GenericResult<Az_CommessaPutOutModel>> AZ_CommessaPut(GenericRequest<Az_CommessaPutInModel> model, bool isSubProcess);
        Task<GenericResult<Az_CommessaDeleteOutModel>> AZ_CommessaDelete(GenericRequest<Az_CommessaDeleteInModel> model, bool isSubProcess);
    }
}
