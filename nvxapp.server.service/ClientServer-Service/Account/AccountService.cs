using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Infrastructure;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using static nvxapp.server.data.Entities.AspNetUsersDataUtil;


namespace nvxapp.server.service.ClientServer_Service.Account
{
    public class AccountService : ServiceBase, IAccountService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAspNetUserRolesRepository _aspNetUserRolesRepository;
        private readonly IAspNetRolesRepository _aspNetRolesRepository;
        private readonly IDealerRepository _dealerRepository;

        private readonly IUserDealerRepository _userDealerRepository;



        public AccountService(IMapper mapper,
                              UserManager<ApplicationUser> userManager,
                              IAspNetUsersRepository aspNetUsersRepository,
                              IOptions<JwtParameter> jwtParameter,
                              IHttpContextAccessor httpContextAccessor,

                              IAspNetUserRolesRepository aspNetUserRolesRepository,
                              IDealerRepository dealerRepository,
                              IAspNetRolesRepository aspNetRolesRepository,
                              IUserDealerRepository userDealerRepository,
                              SignInManager<ApplicationUser> signInManager
                              ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, httpContextAccessor)
        {
            _signInManager = signInManager;
            _aspNetUserRolesRepository = aspNetUserRolesRepository;
            _aspNetRolesRepository = aspNetRolesRepository;
            _dealerRepository = dealerRepository;
            _userDealerRepository = userDealerRepository;
        }


        public virtual async Task<GenericResult<LoginOutModel>> Login(GenericRequest<LoginInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                LoginOutModel retVal = new LoginOutModel();


                if (model.Data.UserName != null)
                {
                    var applicationUser = await _userManager.FindByNameAsync(model.Data.UserName);


                    if (applicationUser != null)
                    {
                        var result = await _signInManager.CheckPasswordSignInAsync(applicationUser, model.Data.Password, false);
                        if (!result.Succeeded)
                        {
                            retVal.AddMessage("Password errata", MessageType.Error);

                            //throw new Exception("Password errata");
                        }
                        else
                        {
                            retVal.Id = applicationUser.Id;
                            retVal.Token = UtilToken.GenerateJwtToken(applicationUser,
                                                                      _jwtParameter.Key,
                                                                      _jwtParameter.Issuer,
                                                                      _jwtParameter.Audience,
                                                                      _jwtParameter.ExpireMinutes,
                                                                      "schema_a");
                        }
                    }
                    else
                    {
                        //throw new Exception("Nome utente non trovato");
                        retVal.AddMessage("Nome utente non trovato", MessageType.Error);
                    }
                }
                else
                {
                    //throw new Exception("Nome utente non valito");
                    retVal.AddMessage("Nome utente non valito", MessageType.Error);
                }


                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<UserRolesOutModel>> UserRoles(GenericRequest<UserRolesInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserRolesOutModel retVal = new UserRolesOutModel();


                if (!string.IsNullOrEmpty(this.CurrentUserId))
                {
                    var applicationUser = await _userManager.FindByIdAsync(this.CurrentUserId);


                    if (applicationUser != null)
                    {
                        var roles = await _userManager.GetRolesAsync(applicationUser);
                        if (roles != null && roles.Any())
                        {
                            var aspNetRoles = _aspNetRolesRepository.GetAll().Where(x => x.Name != null && roles.Contains(x.Name)).ToList();

                            retVal.Roles = _mapper.Map<List<AspNetRolesModel>>(aspNetRoles);
                        }

                    }
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<UserLoadOutModel>> UserLoad(GenericRequest<UserLoadInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserLoadOutModel retVal = new UserLoadOutModel();

                if (!string.IsNullOrEmpty(model.Data.Id))
                {
                    var applicationUser = await _userManager.FindByIdAsync(model.Data.Id);

                    if (applicationUser != null)
                    {
                        retVal.UserData.Id = model.Data.Id;
                        retVal.UserData.UserName = applicationUser.UserName;

                        var roles = await _userManager.GetRolesAsync(applicationUser);
                        if (roles != null && roles.Any())
                        {
                            var aspNetRoles = _aspNetRolesRepository.GetAll().Where(x => x.Name != null && roles.Contains(x.Name)).ToList();

                            retVal.UserData.Roles = _mapper.Map<List<AspNetRolesModel>>(aspNetRoles);
                        }
                    }
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<DealerListOutModel>> DealerList(GenericRequest<DealerListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                DealerListOutModel retVal = new DealerListOutModel();

                var dealer = _dealerRepository.GetAll().ToList();


                ApplicationRole? applicationRole = _aspNetRolesRepository.GetAll().Where(x => x.Code == RoleCode.DealerPowerAdmin).FirstOrDefault();
                if (applicationRole != null)
                {
                    if (applicationRole.Name != null)
                    {
                        var usrRole = await _userManager.GetUsersInRoleAsync(applicationRole.Name);
                        if (usrRole != null)
                        {
                            var usrId = usrRole.Select(x => x.Id).ToList();
                            var userDealer = _userDealerRepository.GetAll().Where(x => usrId.Contains(x.IdAspNetUsers)).ToList();

                            foreach (var item in userDealer)
                            {
                                var _dealer = _dealerRepository.FindById(item.IdDealer);

                                retVal.DealerList.Add(new DealerListModel()
                                {
                                    IdAspNetUsers = item.IdAspNetUsers,
                                    IdDealer = item.IdDealer,
                                    Descrizione = _dealer?.Descrizione
                                });
                            }
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

    public interface IAccountService : IServiceBase
    {
        public Task<GenericResult<LoginOutModel>> Login(GenericRequest<LoginInModel> model, Boolean isSubProcess);
        public Task<GenericResult<UserRolesOutModel>> UserRoles(GenericRequest<UserRolesInModel> model, Boolean isSubProcess);
        public Task<GenericResult<UserLoadOutModel>> UserLoad(GenericRequest<UserLoadInModel> model, Boolean isSubProcess);
        public Task<GenericResult<DealerListOutModel>> DealerList(GenericRequest<DealerListInModel> inModel, Boolean isSubProcess);
    }
}
