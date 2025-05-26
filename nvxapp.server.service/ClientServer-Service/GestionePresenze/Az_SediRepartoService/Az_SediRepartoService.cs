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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoUserService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoUserService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService
{

    public class Az_SediRepartoService : ServiceBase, IAz_SediRepartoService
    {

        private readonly IAz_SediRepartoUserService _az_SediRepartoUserService;
        private readonly IAccountService _accountService;
        private readonly IAz_SediRepartoRepository _az_SediRepartoRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IAspNetRolesRepository _aspNetRolesRepository;
        private readonly IAz_SediRepartoUserRepository _az_RepartoUserRepository;


        public Az_SediRepartoService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAz_SediRepartoUserRepository az_RepartoUserRepository,
                                  IAspNetRolesRepository aspNetRolesRepository,
                                  IAccountService accountService,
                                  IAz_SediRepartoUserService az_SediRepartoUserService,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IAz_SediRepartoRepository az_RepartoRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SediRepartoRepository = az_RepartoRepository;
            _accountService = accountService;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _az_SediRepartoUserService = az_SediRepartoUserService;
            _aspNetRolesRepository = aspNetRolesRepository;
            _az_RepartoUserRepository = az_RepartoUserRepository;
        }

        public virtual async Task<GenericResult<Az_SediReparto_GetAll_OutModel>> GetAll(GenericRequest<Az_SediReparto_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediReparto_GetAll_OutModel retVal = new Az_SediReparto_GetAll_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);

                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi != null)
                {
                    var az_Rep = _az_SediRepartoRepository.FindAll(x => x.IdAz_Sedi == company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi.Id).ToList();

                    GenericRequest<UserCompanyListInModel> req = new GenericRequest<UserCompanyListInModel>();
                    req.Data.FilteredRoles.AddRange("CompanyAdmin","CompanyPowerAdmin");
                    var resUser = await _accountService.UserCompanyList(req,true);
                    if(resUser.Success && resUser.Data != null)
                    {

                    }

                    //var applicationUser = await _userManager.FindByIdAsync(this.CurrentUserId);
                    //if (applicationUser != null)
                    //{
                    //}
                    //var roles = await _userManager.GetRolesAsync(applicationUser);


                    retVal.Az_SediReparto = _mapper.Map<List<Az_SediRepartoModel>>(az_Rep);
                }


                

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_SediRepartoGetOutModel>> Az_SediRepartoGet(GenericRequest<Az_SediRepartoGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {

                Az_SediRepartoGetOutModel retVal = new Az_SediRepartoGetOutModel();

                var az_SediReparto = await _az_SediRepartoRepository.FindByIdAsync(model.Data.Id);
                if (az_SediReparto != null)
                {
                    retVal.Az_SediReparto = _mapper.Map<Az_SediRepartoModel>(az_SediReparto);

                    GenericRequest<Az_SediRepartoUser_GetAll_InModel> req1 = new GenericRequest<Az_SediRepartoUser_GetAll_InModel>();
                    req1.Data.IdAz_SediReparto = retVal.Az_SediReparto.Id;

                    var res1 = await _az_SediRepartoUserService.GetAll(req1, true);
                    if (res1.Success && res1.Data != null)
                    {
                        ApplicationRole? userRole = _aspNetRolesRepository.GetAll().Where(x => x.Code == RoleCode.User).FirstOrDefault();
                        ApplicationRole? companyAdminRole = _aspNetRolesRepository.GetAll().Where(x => x.Code == RoleCode.CompanyAdmin).FirstOrDefault();
                        ApplicationRole? companyPowerAdminRole = _aspNetRolesRepository.GetAll().Where(x => x.Code == RoleCode.CompanyPowerAdmin).FirstOrDefault();

                        foreach (var item in res1.Data.Az_RepartoUser)
                        {
                            var applicationUser = await _userManager.FindByIdAsync(item.IdAspNetUsers);
                            if (applicationUser != null)
                            {
                                var roles = await _userManager.GetRolesAsync(applicationUser);
                                if (roles != null)
                                {
                                    //user
                                    if (userRole != null && userRole.Name != null && item.UserInDepartment)
                                    {
                                        if (roles.Contains(userRole.Name))
                                        {
                                            retVal.SelectedUser.Add(new CheckObjOn_Id_Text_4ApprovalZorder()
                                            {
                                                Id = item.IdAspNetUsers,
                                                Checked = true
                                            });
                                        }
                                    }
                                    //admin
                                    if (companyAdminRole != null && companyAdminRole.Name != null && companyPowerAdminRole != null && companyPowerAdminRole.Name != null && item.EnabledToAdmin)
                                    {
                                        if (roles.Contains(companyAdminRole.Name) || roles.Contains(companyPowerAdminRole.Name))
                                        {
                                            retVal.SelectedAdmin.Add(new CheckObjOn_Id_Text_4ApprovalZorder()
                                            {
                                                Id = item.IdAspNetUsers,
                                                Checked = true,
                                                ApprovalZOrder = item.ApprovalZOrder,
                                                EnabledToApproval = item.EnabledToApproval
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    retVal.Az_SediReparto = new Az_SediRepartoModel() { };
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_SediRepartoPutOutModel>> Az_SediRepartoPut(GenericRequest<Az_SediRepartoPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediRepartoPutOutModel retVal = new Az_SediRepartoPutOutModel();
                retVal.Az_SediReparto = model.Data.Az_SediReparto;
                retVal.SelectedUser = model.Data.SelectedUser;
                retVal.SelectedAdmin = model.Data.SelectedAdmin;


                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);

                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi != null)
                {
                    Az_SediReparto? az_SediReparto = await _az_SediRepartoRepository.FindByIdAsync(model.Data.Az_SediReparto.Id);
                    if (az_SediReparto == null)
                    {
                        az_SediReparto = _mapper.Map<Az_SediReparto>(model.Data.Az_SediReparto);
                        az_SediReparto.IdAz_Sedi = company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Sedi.Id;
                    }
                    else
                    {
                        //update 
                        az_SediReparto = _mapper.Map<Az_SediReparto>(model.Data.Az_SediReparto);
                    }

                    //aggiurna il valore ritornato al client
                    az_SediReparto = await _az_SediRepartoRepository.UpsertAsync(az_SediReparto);
                    retVal.Az_SediReparto = _mapper.Map<Az_SediRepartoModel>(az_SediReparto);


                    //Az_SediRepartoUser 
                    GenericRequest<Az_SediRepartoUser_GetAll_InModel> req1 = new GenericRequest<Az_SediRepartoUser_GetAll_InModel>();
                    req1.Data.IdAz_SediReparto = retVal.Az_SediReparto.Id;

                    var res1 = await _az_SediRepartoUserService.GetAll(req1, true);
                    if (res1.Success && res1.Data != null)
                    {
                        //cancellazione
                        foreach (var item in res1.Data.Az_RepartoUser)
                        {
                            // ciclo i valori originali, se non presente nei valori ritornati (o Checked=falso) dal client
                            // allora è stato eliminato e procedo alla cancellazione (logica)
                            
                            var recDB = _az_RepartoUserRepository.FindAll(x => x.IdAz_SediReparto == model.Data.Az_SediReparto.Id &&
                                                                               x.IdAspNetUsers == item.IdAspNetUsers).FirstOrDefault();

                            if (recDB != null)
                            {
                                Boolean updateRec = false;

                                var selAdmi = model.Data.SelectedAdmin.Where(x => x.Id == item.IdAspNetUsers && x.Checked).FirstOrDefault();
                                if (selAdmi == null && recDB.EnabledToAdmin)
                                {
                                    recDB.EnabledToAdmin = false;
                                    recDB.EnabledToApproval = false;
                                    recDB.ApprovalZOrder = 0;
                                    updateRec = true;
                                }

                                var selUser = model.Data.SelectedUser.Where(x => x.Id == item.IdAspNetUsers && x.Checked).FirstOrDefault();
                                if (selUser == null && recDB.UserInDepartment)
                                {
                                    recDB.UserInDepartment = false;
                                    updateRec = true;
                                }
                                
                                if(updateRec)
                                    await _az_RepartoUserRepository.UpsertAsync(recDB);
                                
                            }
                            
                        }


                        //aggiornamento
                        var AllAdmin = model.Data.SelectedAdmin.Where(x => x.Checked == true).ToList();
                        foreach (var item in AllAdmin)
                        {
                            var recDB = _az_RepartoUserRepository.FindAll(x => x.IdAz_SediReparto == model.Data.Az_SediReparto.Id &&
                                                                               x.IdAspNetUsers == item.Id).FirstOrDefault();

                            if (recDB == null)
                            {
                                recDB = new Az_SediRepartoUser()
                                {
                                    IdAspNetUsers = item.Id,
                                    IdAz_SediReparto = model.Data.Az_SediReparto.Id
                                };
                            }

                            recDB.EnabledToAdmin = true;
                            recDB.EnabledToApproval = item.EnabledToApproval;
                            recDB.ApprovalZOrder = item.ApprovalZOrder;
                            await _az_RepartoUserRepository.UpsertAsync(recDB);
                        }

                        var AllUser = model.Data.SelectedUser.Where(x => x.Checked == true).ToList();
                        foreach (var item in AllUser)
                        {
                            var recDB = _az_RepartoUserRepository.FindAll(x => x.IdAz_SediReparto == model.Data.Az_SediReparto.Id &&
                                                                               x.IdAspNetUsers == item.Id).FirstOrDefault();

                            if (recDB == null)
                            {
                                recDB = new Az_SediRepartoUser()
                                {
                                    IdAspNetUsers = item.Id,
                                    IdAz_SediReparto = model.Data.Az_SediReparto.Id
                                };
                            }

                            recDB.UserInDepartment = true;
                            await _az_RepartoUserRepository.UpsertAsync(recDB);
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

    public interface IAz_SediRepartoService : IServiceBase
    {
        public Task<GenericResult<Az_SediReparto_GetAll_OutModel>> GetAll(GenericRequest<Az_SediReparto_GetAll_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Az_SediRepartoGetOutModel>> Az_SediRepartoGet(GenericRequest<Az_SediRepartoGetInModel> model, Boolean isSubProcess);
        public Task<GenericResult<Az_SediRepartoPutOutModel>> Az_SediRepartoPut(GenericRequest<Az_SediRepartoPutInModel> model, Boolean isSubProcess);
    }




}
