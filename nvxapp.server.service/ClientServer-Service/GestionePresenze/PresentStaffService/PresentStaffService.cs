using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
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
        private readonly IDip_GG_TimbraturaService         _dip_GG_TimbraturaService;
        private readonly IGestionePresenzeUserUtility      _gestionePresenzeUserUtility;

        public PresentStaffService(IMapper mapper,
                                   UserManager<ApplicationUser> userManager,
                                   IAspNetUsersRepository aspNetUsersRepository,
                                   IOptions<JwtParameter> jwtParameter,
                                   IHttpContextAccessor httpContextAccessor,
                                   IConfiguration configuration,
                                   IDip_GG_TimbraturaService dip_GG_TimbraturaService,
                                   IGestionePresenzeUserUtility gestionePresenzeUserUtility
                                   ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_GG_TimbraturaService    = dip_GG_TimbraturaService;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<PresentStaff_GetOutModel>> PresentStaffGet(GenericRequest<PresentStaff_GetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                PresentStaff_GetOutModel retVal = new PresentStaff_GetOutModel();

                
                DateTime dal = new DateTime( DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day);
                DateTime al  = dal;


                var req_Timbrature = new GenericRequest<Dip_GG_Timbratura_Get_4Calculation_InModel>();
                req_Timbrature.Data.UsersId = model.Data.PresentStaff.SelectedUserId;
                req_Timbrature.Data.Dal     = dal;
                req_Timbrature.Data.Al      = al;

                var res_Timbrature = await _dip_GG_TimbraturaService.Dip_GG_Timbratura_Get_4Calculation(req_Timbrature, true);

                if (res_Timbrature.Success && res_Timbrature.Data != null)
                {
                    foreach (var currUserId in model.Data.PresentStaff.SelectedUserId)
                    {
                        PresentStaff_DaySlot daySlot = new PresentStaff_DaySlot();
                        daySlot.IdAspNetUsers = currUserId;

                        // risolve userId → IdDip_RapportoLavoro
                        var userData = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(currUserId, false);
                        if (userData?.dip_RapportoLavoro != null)
                        {
                            // filtra le timbrature dell'utente e prende quella del giorno più recente
                            daySlot.Dip_GG_Timbratura = res_Timbrature.Data.Dip_GG_Timbratura
                                .Where(t => t.IdDip_RapportoLavoro == userData.dip_RapportoLavoro.Id)
                                .OrderByDescending(t => t.GiornoCompetenza)
                                .ThenByDescending(t => t.TimbraturaOriginale)
                                .FirstOrDefault();

                            if(daySlot.Dip_GG_Timbratura!= null && daySlot.Dip_GG_Timbratura.TimbraturaTipo != data.Entities.Tenant.TipoTimbratura.Uscita)
                                daySlot.IsPresent=true;

                        }

                        retVal.DaySlots.Add(daySlot);
                    }
                }

                return retVal;
            }, isSubProcess);
        }
    }

    public interface IPresentStaffService : IServiceBase
    {
        Task<GenericResult<PresentStaff_GetOutModel>> PresentStaffGet(GenericRequest<PresentStaff_GetInModel> model, bool isSubProcess);
    }
}
