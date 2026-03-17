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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService
{
    
    public class Dip_GG_ResultService : ServiceBase, IDip_GG_ResultService
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IDip_GG_ResultRepository _dip_GG_ResultRepository;

        public Dip_GG_ResultService(  IMapper mapper,
                                          UserManager<ApplicationUser> userManager,
                                          IAspNetUsersRepository aspNetUsersRepository,
                                          IOptions<JwtParameter> jwtParameter,
                                          IHttpContextAccessor httpContextAccessor,
                                          IConfiguration configuration,

                                          IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                          IDip_GG_ResultRepository dip_GG_ResultRepository
                                  ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _dip_GG_ResultRepository = dip_GG_ResultRepository;

        }

        public virtual async Task<GenericResult<Dip_GG_Result_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Result_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Result_GetAll_OutModel retVal = new Dip_GG_Result_GetAll_OutModel();

                string userId = string.IsNullOrEmpty(model.Data.IdAspNetUsers) ? this.CurrentUserId : model.Data.IdAspNetUsers;

                User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(userId, true);

                if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                {

                    List<Dip_GG_Result> timbratura = _dip_GG_ResultRepository.FindAll(x => x.Data.Year == model.Data.Year &&
                                                                                              x.Data.Month == model.Data.Month &&
                                                                                              x.IdDip_RapportoLavoro == user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id)
                                                                                     //.OrderBy(x => x.TimbraturaOriginale)
                                                                                     .ToList();
                    

                    retVal.Dip_GG_Result = _mapper.Map<List<Dip_GG_ResultModel>>(timbratura);

                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Dip_GG_Result_Get_4Calculation_OutModel>> Dip_GG_Result_Get_4Calculation(GenericRequest<Dip_GG_Result_Get_4Calculation_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Result_Get_4Calculation_OutModel retVal = new Dip_GG_Result_Get_4Calculation_OutModel();

                // Recupera i IdDip_RapportoLavoro per tutti gli utenti richiesti
                List<int> idRapportoLavoroList = new List<int>();

                foreach (string userId in model.Data.UsersId)
                {
                    User_DATA_COMB_DipAna_DipRapp userData = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(userId, false);
                    if (userData?.dip_RapportoLavoro != null)
                    {
                        idRapportoLavoroList.Add(userData.dip_RapportoLavoro.Id);
                    }
                }

                if (idRapportoLavoroList.Count > 0)
                {
                    List<Dip_GG_Result> timbrature = _dip_GG_ResultRepository
                        .FindAll(x => idRapportoLavoroList.Contains(x.IdDip_RapportoLavoro) &&
                                      x.Data >= model.Data.Dal &&
                                      x.Data <= model.Data.Al)
                        .OrderBy(x => x.IdDip_RapportoLavoro)
                        //.ThenBy(x => x.TimbraturaOriginale)
                        .ToList();

                    retVal.Dip_GG_Result = _mapper.Map<List<Dip_GG_ResultModel>>(timbrature);
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }    
        
    }

    public interface IDip_GG_ResultService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_Result_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Result_GetAll_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Result_Get_4Calculation_OutModel>> Dip_GG_Result_Get_4Calculation(GenericRequest<Dip_GG_Result_Get_4Calculation_InModel> model, Boolean isSubProcess);
    }


}
