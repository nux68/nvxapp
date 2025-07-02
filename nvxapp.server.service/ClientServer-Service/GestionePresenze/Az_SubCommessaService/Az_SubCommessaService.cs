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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaUserService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaUserService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService
{
    public class Az_SubCommessaService : ServiceBase, IAz_SubCommessaService
    {
        private readonly IAz_SubCommessaRepository _az_SubCommessaRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        private readonly IAz_SubCommessaUserRepository _az_SubCommessaUserRepository;
        private readonly IAz_SubCommessaAttivitaRepository _az_SubCommessaAttivitaRepository;
        private readonly IAz_SubCommessaSediRepartoRepository _az_SubCommessaSediRepartoRepository;

        private readonly IAz_SubCommessaUserService _az_SubCommessaUserService;
        private readonly IAz_SubCommessaAttivitaService _az_SubCommessaAttivitaService;

        public Az_SubCommessaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAz_SubCommessaUserService az_SubCommessaUserService,

                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_SubCommessaUserRepository az_SubCommessaUserRepository,
                                  IAz_SubCommessaAttivitaRepository az_SubCommessaAttivitaRepository,
                                  IAz_SubCommessaAttivitaService az_SubCommessaAttivitaService,
                                  IAz_SubCommessaSediRepartoRepository az_SubCommessaSediRepartoRepository,
                                  IAz_SubCommessaRepository az_SubCommessaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SubCommessaRepository = az_SubCommessaRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _az_SubCommessaUserRepository = az_SubCommessaUserRepository;
            _az_SubCommessaAttivitaRepository = az_SubCommessaAttivitaRepository;
            _az_SubCommessaSediRepartoRepository = az_SubCommessaSediRepartoRepository;

            _az_SubCommessaUserService = az_SubCommessaUserService;
            _az_SubCommessaAttivitaService = az_SubCommessaAttivitaService;
        }

        public virtual async Task<GenericResult<Az_SubCommessa_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessa_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessa_GetAll_OutModel retVal = new Az_SubCommessa_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_SubCommessa = (await _az_SubCommessaRepository.FindAll()).ToList();
                    retVal.Az_SubCommessa = _mapper.Map<List<Az_SubCommessaModel>>(az_SubCommessa);
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_SubCommessa_GetAll_4Edit_OutModel>> GetAll_4Edit(GenericRequest<Az_SubCommessa_GetAll_4Edit_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessa_GetAll_4Edit_OutModel retVal = new Az_SubCommessa_GetAll_4Edit_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var az_SubCommessa = _az_SubCommessaRepository.FindAll(x => x.IdAz_Commessa == model.Data.Id).ToList();
                    retVal.Az_SubCommessa = _mapper.Map<List<Az_SubCommessa_4EditModel>>(az_SubCommessa);
                    if (retVal.Az_SubCommessa != null)
                    {
                        var Az_SubCommessa_Id = retVal.Az_SubCommessa.Select(x => x.Id).ToList();

                        var az_SubCommessaUser = _az_SubCommessaUserRepository.FindAll(x => Az_SubCommessa_Id.Contains(x.IdAz_SubCommessa)).ToList();
                        var az_SubCommessaAttivita = _az_SubCommessaAttivitaRepository.FindAll(x => Az_SubCommessa_Id.Contains(x.IdAz_SubCommessa)).ToList();
                        var az_SubCommessaSediReparto = _az_SubCommessaSediRepartoRepository.FindAll(x => Az_SubCommessa_Id.Contains(x.IdAz_SubCommessa)).ToList();

                        foreach (var itemSubCommessa in retVal.Az_SubCommessa)
                        {
                            // Az_SubCommessaUser assignment
                            var req_SubCommessaUser = new GenericRequest<Az_SubCommessaUser_Get4SubCommessa_InModel>();
                            req_SubCommessaUser.Data.IdAz_Commessa = model.Data.Id;
                            var res_SubCommessaUser = await _az_SubCommessaUserService.Get4Commessa(req_SubCommessaUser, true);
                            if (res_SubCommessaUser.Success && res_SubCommessaUser.Data != null)
                                itemSubCommessa.Az_SubCommessaUser = res_SubCommessaUser.Data.Az_SubCommessaUser;

                            // Az_SubCommessaAttivita assignment
                            var req_SubCommessaAttivita = new GenericRequest<Az_SubCommessaAttivita_Get4SubCommessa_InModel>();
                            req_SubCommessaAttivita.Data.IdAz_SubCommessa = itemSubCommessa.Id;
                            var res_SubCommessaAttivita = await _az_SubCommessaAttivitaService.Get4SubCommessa(req_SubCommessaAttivita, true);
                            if (res_SubCommessaAttivita.Success && res_SubCommessaAttivita.Data != null)
                                itemSubCommessa.Az_SubCommessaAttivita = res_SubCommessaAttivita.Data.Az_SubCommessaAttivita;

                            // Az_SubCommessaSediReparto assignment
                            az_SubCommessaSediReparto.Where(x => x.IdAz_SubCommessa == itemSubCommessa.Id).ToList().ForEach(x =>
                            {
                                itemSubCommessa.Az_SubCommessaSediReparto.Add(new CheckObjOn_Id_Number()
                                {
                                    Id = x.IdAz_SediReparto,
                                    Checked = true
                                });
                            });
                        }
                    }
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_SubCommessa_PutAll_4Edit_OutModel>> PutAll_4Edit(GenericRequest<Az_SubCommessa_PutAll_4Edit_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessa_PutAll_4Edit_OutModel retVal = new Az_SubCommessa_PutAll_4Edit_OutModel();
                retVal.Id = model.Data.Id;

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                //rileggo i dati originali
                var reqAz_Sub = new GenericRequest<Az_SubCommessa_GetAll_4Edit_InModel>();
                reqAz_Sub.Data.Id = model.Data.Id;

                var resAz_Sub = await GetAll_4Edit(reqAz_Sub, true);
                if (resAz_Sub.Success && resAz_Sub.Data != null)
                {
                    //cancellazione sub commesse eliminate
                    foreach (var item in resAz_Sub.Data.Az_SubCommessa)
                    {   //ciclo le commesse originali

                        //ottengo il record orig del db
                        Az_SubCommessa? az_SubCommessa = _az_SubCommessaRepository.FindAll(x => x.IdAz_Commessa == item.Id).FirstOrDefault();

                        if (az_SubCommessa != null)
                        {
                            //cerco la commessa nei dati tornati dal client
                            var orig_TMP = model.Data.Az_SubCommessa.Where(x => x.Id == item.Id).FirstOrDefault();

                            //se non trovo la corrispondenza nei dati del client, vul dire che è stata eliminata
                            if (orig_TMP == null)
                            {
                                //procedo alla cancelazione
                                await _az_SubCommessaRepository.DeleteAsync(az_SubCommessa);
                            }
                        }
                    }
                    //upsert commesse
                    foreach (var item in model.Data.Az_SubCommessa)
                    {
                        //ottengo il record orig del db
                        Az_SubCommessa? az_SubCommessa = _az_SubCommessaRepository.FindAll(x => x.IdAz_Commessa == item.Id).FirstOrDefault();
                        if (az_SubCommessa == null)
                        {
                            az_SubCommessa = _mapper.Map<Az_SubCommessa>(item);
                            az_SubCommessa.IdAz_Commessa = model.Data.Id;
                        }
                        else
                        {
                            az_SubCommessa = _mapper.Map<Az_SubCommessa>(item);
                        }
                        az_SubCommessa = await _az_SubCommessaRepository.UpsertAsync(az_SubCommessa);

                        var req_SubCommessaUser = new GenericRequest<Az_SubCommessaUser_Put4SubCommessa_InModel>();
                        req_SubCommessaUser.Data.IdAz_Commessa = az_SubCommessa.IdAz_Commessa;
                        req_SubCommessaUser.Data.Az_SubCommessaUser = item.Az_SubCommessaUser;
                        var res_SubCommessaUser = await _az_SubCommessaUserService.Put4Commessa(req_SubCommessaUser, true);

                        var req_SubCommessaAttivita = new GenericRequest<Az_SubCommessaAttivita_Put4SubCommessa_InModel>();
                        req_SubCommessaAttivita.Data.IdAz_SubCommessa = item.Id;
                        req_SubCommessaAttivita.Data.Az_SubCommessaAttivita = item.Az_SubCommessaAttivita;
                        var res_SubCommessaAttivita = await _az_SubCommessaAttivitaService.Put4SubCommessa(req_SubCommessaAttivita, true);
                    }

                    //rileggo i dati dopo le varizioni per ritornare il valore corrente
                    resAz_Sub = await GetAll_4Edit(reqAz_Sub, true);
                    if (resAz_Sub.Success && resAz_Sub.Data != null)
                    {
                        retVal.Az_SubCommessa = resAz_Sub.Data.Az_SubCommessa;
                    }
                }

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IAz_SubCommessaService : IServiceBase
    {
        Task<GenericResult<Az_SubCommessa_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessa_GetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_SubCommessa_GetAll_4Edit_OutModel>> GetAll_4Edit(GenericRequest<Az_SubCommessa_GetAll_4Edit_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_SubCommessa_PutAll_4Edit_OutModel>> PutAll_4Edit(GenericRequest<Az_SubCommessa_PutAll_4Edit_InModel> model, bool isSubProcess);
    }
}
