using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.PresentStaffService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.PresentStaffService
{
    public class PresentStaffService : ServiceBase, IPresentStaffService
    {

        private readonly IDip_GG_TimbraturaService _dip_GG_TimbraturaService;

        public PresentStaffService(IMapper mapper,
                                   UserManager<ApplicationUser> userManager,
                                   IAspNetUsersRepository aspNetUsersRepository,
                                   IOptions<JwtParameter> jwtParameter,
                                   IHttpContextAccessor httpContextAccessor,
                                   IConfiguration configuration,

                                   IDip_GG_TimbraturaService dip_GG_TimbraturaService

                                   ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {

            _dip_GG_TimbraturaService = dip_GG_TimbraturaService;
        }

        public virtual async Task<GenericResult<PresentStaff_GetOutModel>> PresentStaffGet(GenericRequest<PresentStaff_GetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                PresentStaff_GetOutModel retVal = new PresentStaff_GetOutModel();


                var req_Timbrature = new GenericRequest<Dip_GG_Timbratura_Get_4Calculation_InModel>();
                req_Timbrature.Data.UsersId = model.Data.PresentStaff.SelectedUserId;

                DateTime dal = new DateTime(model.Data.PresentStaff.Year, model.Data.PresentStaff.Month, 1);
                DateTime al = new DateTime(model.Data.PresentStaff.Year, model.Data.PresentStaff.Month, DateTime.DaysInMonth(model.Data.PresentStaff.Year, model.Data.PresentStaff.Month));


                req_Timbrature.Data.Dal = dal;
                req_Timbrature.Data.Al = al;

                var res_Timbrature = await _dip_GG_TimbraturaService.Dip_GG_Timbratura_Get_4Calculation(req_Timbrature, true);
                if (res_Timbrature.Success && res_Timbrature.Data != null)
                {
                    foreach (var currUser in model.Data.PresentStaff.SelectedUserId)
                    {
                        PresentStaff_DaySlot daySlot = new PresentStaff_DaySlot();
                        daySlot.IdAspNetUsers = currUser;
                        retVal.DaySlots.Add(daySlot);
                    }
                }


                // TODO: implementare la logica di recupero del personale presente

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
    }

    public interface IPresentStaffService : IServiceBase
    {
        Task<GenericResult<PresentStaff_GetOutModel>> PresentStaffGet(GenericRequest<PresentStaff_GetInModel> model, bool isSubProcess);
    }
}
