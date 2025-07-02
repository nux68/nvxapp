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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaUserService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaUserService
{
    public class Az_SubCommessaUserService : ServiceBase, IAz_SubCommessaUserService
    {
        private readonly IAz_SubCommessaUserRepository _az_SubCommessaUserRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Az_SubCommessaUserService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_SubCommessaUserRepository az_SubCommessaUserRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SubCommessaUserRepository = az_SubCommessaUserRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Az_SubCommessaUser_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessaUser_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessaUser_GetAll_OutModel retVal = new Az_SubCommessaUser_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var entities = await _az_SubCommessaUserRepository.FindAll();
                    retVal.Az_SubCommessaUser = _mapper.Map<List<Az_SubCommessaUserModel>>(entities);
                }

                return retVal;
            }, isSubProcess);
        }


        public virtual async Task<GenericResult<Az_SubCommessaUser_Get4Commessa_OutModel>> Get4Commessa(GenericRequest<Az_SubCommessaUser_Get4Commessa_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessaUser_Get4Commessa_OutModel retVal = new Az_SubCommessaUser_Get4Commessa_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var entities = _az_SubCommessaUserRepository.FindAll(x => x.IdAz_SubCommessa == model.Data.IdAz_Commessa).ToList();
                    retVal.Az_SubCommessaUser = _mapper.Map<List<Az_SubCommessaUser4EditModel>>(entities);
                }

                foreach (var item in retVal.Az_SubCommessaUser)
                    item.Checked = true;


                return retVal;
            }, isSubProcess);
        }


        public virtual async Task<GenericResult<Az_SubCommessaUser_Put4Commessa_OutModel>> Put4Commessa(GenericRequest<Az_SubCommessaUser_Put4Commessa_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessaUser_Put4Commessa_OutModel retVal = new Az_SubCommessaUser_Put4Commessa_OutModel();

                //rileggo i dati originali
                var reqSubCommessaUser = new GenericRequest<Az_SubCommessaUser_Get4Commessa_InModel>();
                reqSubCommessaUser.Data.IdAz_Commessa = model.Data.IdAz_Commessa;

                var resSubCommessaUser = await Get4Commessa(reqSubCommessaUser, true);
                if (resSubCommessaUser.Success && resSubCommessaUser.Data != null)
                {
                    //cancellazione user eliminati
                    foreach (var item in resSubCommessaUser.Data.Az_SubCommessaUser)
                    {
                        //ottengo il record orig del db
                        Az_SubCommessaUser? az_SubCommessaUser = _az_SubCommessaUserRepository.FindAll(x => x.Id == item.Id).FirstOrDefault();
                        if (az_SubCommessaUser != null)
                        {
                            //cerco la commessa nei dati tornati dal client
                            var orig_TMP = model.Data.Az_SubCommessaUser.Where(x => x.Id == item.Id && x.Checked == true).FirstOrDefault();
                            //se non trovo la corrispondenza nei dati del client, vul dire che è stata eliminata
                            if (orig_TMP == null)
                            {
                                //procedo alla cancelazione
                                await _az_SubCommessaUserRepository.DeleteAsync(az_SubCommessaUser);
                            }
                        }
                    }
                    //upsert 
                    foreach (var item in model.Data.Az_SubCommessaUser.Where(x=>x.Checked==true).ToList())
                    {
                        //ottengo il record orig del db
                        Az_SubCommessaUser? az_SubCommessaUser = _az_SubCommessaUserRepository.FindAll(x => x.IdAz_SubCommessa == item.IdAz_SubCommessa &&
                                                                                                            x.IdAspNetUsers == item.IdAspNetUsers).FirstOrDefault();
                        if (az_SubCommessaUser == null)
                        {
                            az_SubCommessaUser = _mapper.Map<Az_SubCommessaUser>(item);
                            az_SubCommessaUser.IdAz_SubCommessa = model.Data.IdAz_Commessa;
                            az_SubCommessaUser.IdAspNetUsers = item.IdAspNetUsers;
                        }
                        else
                        {
                            az_SubCommessaUser = _mapper.Map<Az_SubCommessaUser>(item);
                        }
                        az_SubCommessaUser = await _az_SubCommessaUserRepository.UpsertAsync(az_SubCommessaUser);
                    }
                }



                return retVal;
            }, isSubProcess);
        }

    }

    public interface IAz_SubCommessaUserService : IServiceBase
    {
        Task<GenericResult<Az_SubCommessaUser_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessaUser_GetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_SubCommessaUser_Get4Commessa_OutModel>> Get4Commessa(GenericRequest<Az_SubCommessaUser_Get4Commessa_InModel> model, bool isSubProcess);
        Task<GenericResult<Az_SubCommessaUser_Put4Commessa_OutModel>> Put4Commessa(GenericRequest<Az_SubCommessaUser_Put4Commessa_InModel> model, bool isSubProcess);



    }
}