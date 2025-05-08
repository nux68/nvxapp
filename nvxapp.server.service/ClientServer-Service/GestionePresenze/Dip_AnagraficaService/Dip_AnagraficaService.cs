using System.Data;
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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService
{

    public class Dip_AnagraficaService : ServiceBase, IDip_AnagraficaService
    {
        private readonly IAccountService _accountService;
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IAspNetRolesRepository _aspNetRolesRepository;

        public Dip_AnagraficaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAspNetRolesRepository aspNetRolesRepository,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAccountService accountService,
                                  IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                  IDip_AnagraficaRepository dip_AnagraficaRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _accountService = accountService;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _aspNetRolesRepository = aspNetRolesRepository;
        }

        public virtual async Task<GenericResult<Dip_Anagrafica_GetAll_OutModel>> GetAll(GenericRequest<Dip_Anagrafica_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_Anagrafica_GetAll_OutModel retVal = new Dip_Anagrafica_GetAll_OutModel();

                GenericRequest<UserCompanyListInModel> req = new GenericRequest<UserCompanyListInModel>();
                var res = await _accountService.UserCompanyList(req, true);
                if (res.Success && res.Data != null)
                {
                    var UserCompanyList = res.Data.UserCompanyList;
                    List<string?> idAspNetUsers = UserCompanyList.Select(x => x.IdAspNetUsers).ToList();

                    // TODO eliminare a regime
                    foreach (var item in idAspNetUsers)
                    {
                        if (item != null)
                            await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(item, true);
                    }

                    var aspNetRoles = _aspNetRolesRepository.GetAll().ToList();


                    List<Dip_Anagrafica> dip_Anagrafica = _dip_AnagraficaRepository.FindAll(x => idAspNetUsers.Contains(x.IdAspNetUsers)).ToList();
                    List<Dip_RapportoLavoro> dip_RapportoLavoro = _dip_RapportoLavoroRepository.FindAll(x => dip_Anagrafica.Select(x => x.Id).ToList().Contains(x.IdDip_Anagrafica)).ToList();
                    foreach (var item in dip_Anagrafica)
                    {


                        var applicationUser = await _userManager.FindByIdAsync(item.IdAspNetUsers);
                        if (applicationUser != null)
                        {
                            var usrRoles = new List<string>(await _userManager.GetRolesAsync(applicationUser));
                            retVal.Dip_Anagrafica.Add(new Dip_AnagraficaModel()
                            {
                                UserName = applicationUser.UserName != null ? applicationUser.UserName : string.Empty,
                                IdAspNetUsers = item.IdAspNetUsers,
                                Cognome = item.Cognome,
                                Nome = item.Nome,
                                Id = item.Id,
                                Dip_RapportoLavoro = _mapper.Map<List<Dip_RapportoLavoroModel>>(dip_RapportoLavoro.Where(x => x.IdDip_Anagrafica == item.Id).ToList()),
                                Roles = aspNetRoles.Where(x=> x.Name!= null && usrRoles.Contains(x.Name)).Select(x=> x.Id).ToList()
                                
                            });
                        }

                    }
                }


                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IDip_AnagraficaService : IServiceBase
    {
        public Task<GenericResult<Dip_Anagrafica_GetAll_OutModel>> GetAll(GenericRequest<Dip_Anagrafica_GetAll_InModel> model, Boolean isSubProcess);
    }
}
