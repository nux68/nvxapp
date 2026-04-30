using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.VacationPlanService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.VacationPlanService
{
    public class VacationPlanService : ServiceBase, IVacationPlanService
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IDip_GG_GiustificativiService _dip_GG_GiustificativiService;
        private readonly IDip_GG_RichiestaService _dip_GG_RichiestaService;

        public VacationPlanService(IMapper mapper,
                                   UserManager<ApplicationUser> userManager,
                                   IAspNetUsersRepository aspNetUsersRepository,
                                   IOptions<JwtParameter> jwtParameter,
                                   IHttpContextAccessor httpContextAccessor,
                                   IConfiguration configuration,

                                   IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                   IDip_GG_GiustificativiService dip_GG_GiustificativiService,
                                   IDip_GG_RichiestaService dip_GG_RichiestaService

                                   ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_GG_GiustificativiService = dip_GG_GiustificativiService;
            _dip_GG_RichiestaService = dip_GG_RichiestaService;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<VacationPlan_GetOutModel>> VacationPlanGet(GenericRequest<VacationPlan_GetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                VacationPlan_GetOutModel retVal = new VacationPlan_GetOutModel();




                DateTime dal = new DateTime(model.Data.VacationPlan.Year, model.Data.VacationPlan.Month, 1);
                DateTime al = new DateTime(model.Data.VacationPlan.Year, model.Data.VacationPlan.Month, DateTime.DaysInMonth(model.Data.VacationPlan.Year, model.Data.VacationPlan.Month));

                var req_Giustificativi = new GenericRequest<Dip_GG_Giustificativi_Get_4Calculation_InModel>();
                req_Giustificativi.Data.UsersId = model.Data.VacationPlan.SelectedUserId;
                req_Giustificativi.Data.Dal = dal;
                req_Giustificativi.Data.Al = al;
                var res_Giustificativi = await _dip_GG_GiustificativiService.Dip_GG_Giustificativi_Get_4Calculation(req_Giustificativi, true);

                var req_Richiesta = new GenericRequest<Dip_GG_Richiesta_Get_4Calculation_InModel>();
                req_Richiesta.Data.UsersId = model.Data.VacationPlan.SelectedUserId;
                req_Richiesta.Data.Dal = dal;
                req_Richiesta.Data.Al = al;
                var res_Richiesta = await _dip_GG_RichiestaService.Dip_GG_Richiesta_Get_4Calculation(req_Richiesta, true);



                if (res_Giustificativi.Success && res_Giustificativi.Data != null && res_Richiesta.Success && res_Richiesta.Data != null)
                {
                    foreach (var currUserId in model.Data.VacationPlan.SelectedUserId)
                    {
                        var userData = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(currUserId, false);
                        if (userData?.dip_RapportoLavoro != null)
                        {
                            VacationPlan_DaySlot daySlot = new VacationPlan_DaySlot();
                            
                             daySlot.Dip_GG_Giustificativi = res_Giustificativi.Data.Dip_GG_Giustificativi
                                                                               .Where(t => t.IdDip_RapportoLavoro == userData.dip_RapportoLavoro.Id)
                                                                               .OrderBy(t => t.Data)
                                                                               .ToList();

                            daySlot.Dip_GG_Richieste = res_Richiesta.Data.Dip_GG_Richiesta
                                                                            .Where(t => t.IdDip_RapportoLavoro == userData.dip_RapportoLavoro.Id)
                                                                            .OrderBy(t => t.Data)
                                                                            .ToList();

                            
                            daySlot.IdAspNetUsers = currUserId;
                            retVal.DaySlots.Add(daySlot);
                        }
                    }
                }

                // TODO: implementare la logica di recupero del piano ferie

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
    }

    public interface IVacationPlanService : IServiceBase
    {
        Task<GenericResult<VacationPlan_GetOutModel>> VacationPlanGet(GenericRequest<VacationPlan_GetInModel> model, bool isSubProcess);
    }
}
