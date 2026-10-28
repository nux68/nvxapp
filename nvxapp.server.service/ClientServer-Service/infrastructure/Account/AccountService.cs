using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyMenuRestriction.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Extension;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyApplication;
using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyApplication.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.CompanyInit;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.User;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.HubAI;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Data;
using System.Linq;
using static nvxapp.server.data.Entities.AspNetUsersDataUtil;
using nvxapp.server.data.Localization;


namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Account
{
    public class AccountService : ServiceBase, IAccountService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAspNetUserRolesRepository _aspNetUserRolesRepository;
        private readonly IAspNetRolesRepository _aspNetRolesRepository;

        private readonly IDealerRepository _dealerRepository;
        private readonly IUserDealerRepository _userDealerRepository;

        private readonly IFinancialAdvisorRepository _financialAdvisorRepository;
        private readonly IUserFinancialAdvisorRepository _userFinancialAdvisorRepository;

        private readonly ICompanyRepository _companyRepository;
        private readonly IUserCompanyRepository _userCompanyRepository;

        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly IUserInitializerRegistry _userInitializerRegistry;
        private readonly ICompanyInitializerRegistry _companyInitializerRegistry;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly ICompanyMenuRestrictionRepository _companyMenuRestrictionRepository;
        private readonly ICompanyApplicationService _companyApplicationService;
        private readonly IEnumerable<IUserCompanyListExtension> _userCompanyListExtensions;


        public AccountService(IMapper mapper,
                              UserManager<ApplicationUser> userManager,
                              IAspNetUsersRepository aspNetUsersRepository,
                              IOptions<JwtParameter> jwtParameter,
                              IHttpContextAccessor httpContextAccessor,
                              IConfiguration configuration,

                              IAspNetUserRolesRepository aspNetUserRolesRepository,
                              IAspNetRolesRepository aspNetRolesRepository,

                              IDealerRepository dealerRepository,
                              IUserDealerRepository userDealerRepository,

                              IFinancialAdvisorRepository financialAdvisorRepository,
                              IUserFinancialAdvisorRepository userFinancialAdvisorRepository,

                              IUserInitializerRegistry userInitializerRegistry,
                              ICompanyInitializerRegistry companyInitializerRegistry,
                              ICompanyRepository companyRepository,
                              IUserCompanyRepository userCompanyRepository,
                              IHubContext<SignalRHub> hubContext,
                              ICompanyApplicationRepository companyApplicationRepository,
                              ICompanyMenuRestrictionRepository companyMenuRestrictionRepository,
                              ICompanyApplicationService companyApplicationService,
                              IEnumerable<IUserCompanyListExtension> userCompanyListExtensions,

                              SignInManager<ApplicationUser> signInManager
                              ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _signInManager = signInManager;
            _aspNetUserRolesRepository = aspNetUserRolesRepository;
            _aspNetRolesRepository = aspNetRolesRepository;

            _dealerRepository = dealerRepository;
            _userDealerRepository = userDealerRepository;
            _financialAdvisorRepository = financialAdvisorRepository;
            _userFinancialAdvisorRepository = userFinancialAdvisorRepository;
            _companyRepository = companyRepository;
            _userCompanyRepository = userCompanyRepository;
            
            _userInitializerRegistry = userInitializerRegistry;
            _companyInitializerRegistry = companyInitializerRegistry;
            _companyApplicationRepository = companyApplicationRepository;
            _companyMenuRestrictionRepository = companyMenuRestrictionRepository;
            _companyApplicationService = companyApplicationService;
            _userCompanyListExtensions = userCompanyListExtensions;
            _hubContext = hubContext;
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
                        var result = await _signInManager.CheckPasswordSignInAsync(applicationUser, model.Data.Password ?? string.Empty, false);
                        if (!result.Succeeded)
                        {
                            retVal.AddMessage(InfrastructureMessages.Get("LoginWrongPassword"), MessageType.Error);

                            //throw new Exception("Password errata");
                        }
                        else
                        {
                            retVal.Id = applicationUser.Id;
                            retVal.Token = UtilToken.GenerateJwtToken(
                                                                          _jwtParameter.Key,
                                                                          _jwtParameter.Issuer,
                                                                          _jwtParameter.Audience,
                                                                          _jwtParameter.ExpireMinutes,
                                                                          new TokenProperty()
                                                                          {
                                                                              UserIdFirstConnection = applicationUser.Id,
                                                                              UserId = applicationUser.Id
                                                                          }
                                                                      );
                        }
                    }
                    else
                    {
                        //throw new Exception("Nome utente non trovato");
                        retVal.AddMessage(InfrastructureMessages.Get("LoginUserNotFound"), MessageType.Error);
                    }
                }
                else
                {
                    //throw new Exception("Nome utente non valito");
                    retVal.AddMessage(InfrastructureMessages.Get("LoginUserNameInvalid"), MessageType.Error);
                }


                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        // username / email gia' usati da un altro utente (controllo prima di un inserimento)
        public virtual async Task<GenericResult<UserCheckOutModel>> UserCheck(GenericRequest<UserCheckInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserCheckOutModel retVal = new UserCheckOutModel();

                string? escludi = string.IsNullOrEmpty(model.Data.IdAspNetUsers) ? null : model.Data.IdAspNetUsers;

                if (!string.IsNullOrWhiteSpace(model.Data.UserName))
                {
                    var userName = _userManager.NormalizeName(model.Data.UserName.Trim());
                    retVal.UserNameExists = _userManager.Users.Any(x => x.NormalizedUserName == userName && x.Id != escludi);
                }

                if (!string.IsNullOrWhiteSpace(model.Data.Mail))
                {
                    var mail = _userManager.NormalizeEmail(model.Data.Mail.Trim());
                    retVal.MailExists = _userManager.Users.Any(x => x.NormalizedEmail == mail && x.Id != escludi);
                }

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
                    string userId = string.IsNullOrEmpty(model.Data.IdAspNetUsers) ? this.CurrentUserId : model.Data.IdAspNetUsers;

                    var applicationUser = await _userManager.FindByIdAsync(userId);

                    if (applicationUser != null)
                    {




                        var roles = await _userManager.GetRolesAsync(applicationUser);
                        if (roles != null && roles.Any())
                        {
                            var aspNetRoles = _aspNetRolesRepository.FindAll(x => x.Name != null && roles.Contains(x.Name)).ToList();

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
                        string dealer = "";
                        string financialAdvisor = "";
                        string company = "";
                        string userIdFirstConnection = this.UserIdFirstConnection;

                        if (UseSignalR)
                            await _hubContext.Clients.All.SendAsync("ReceiveMessage", applicationUser.UserName + " è entrato");

                        var roles = await _userManager.GetRolesAsync(applicationUser);
                        if (roles != null && roles.Any())
                        {

                            switch (roles[0])
                            {
                                case "DealerPowerAdmin":
                                case "DealerAdmin":
                                    var userDealer = _userDealerRepository.FindAll(x => x.IdAspNetUsers == applicationUser.Id).FirstOrDefault();
                                    if (userDealer != null)
                                        dealer = userDealer.IdDealer.ToString();
                                    break;

                                case "FinancialAdvisorPowerAdmin":
                                case "FinancialAdvisorAdmin":
                                    var userFinancial = _userFinancialAdvisorRepository.FindAll(x => x.IdAspNetUsers == applicationUser.Id).FirstOrDefault();
                                    if (userFinancial != null)
                                    {
                                        financialAdvisor = userFinancial.IdFinancialAdvisor.ToString();

                                        var financial = _financialAdvisorRepository.FindAll(x => x.Id == userFinancial.IdFinancialAdvisor).FirstOrDefault();
                                        if (financial != null)
                                            dealer = financial.IdDealer.ToString();
                                    }
                                    break;

                                case "CompanyPowerAdmin":
                                case "CompanyAdmin":
                                case "User":
                                    var userCompany = _userCompanyRepository.FindAll(x => x.IdAspNetUsers == applicationUser.Id).FirstOrDefault();
                                    if (userCompany != null)
                                    {
                                        company = userCompany.IdCompany.ToString();
                                        var comp = _companyRepository.FindAll(x => x.Id == userCompany.IdCompany).FirstOrDefault();
                                        if (comp != null)
                                        {
                                            financialAdvisor = comp.IdFinancialAdvisor.ToString();
                                            var financial = _financialAdvisorRepository.FindAll(x => x.Id == comp.IdFinancialAdvisor).FirstOrDefault();
                                            if (financial != null)
                                                dealer = financial.IdDealer.ToString();
                                        }
                                    }

                                    break;

                            }


                            retVal.Token = UtilToken.GenerateJwtToken(
                                                                        _jwtParameter.Key,
                                                                        _jwtParameter.Issuer,
                                                                        _jwtParameter.Audience,
                                                                        _jwtParameter.ExpireMinutes,
                                                                        new TokenProperty()
                                                                        {
                                                                            Dealer = dealer,
                                                                            FinancialAdvisor = financialAdvisor,
                                                                            Company = company,
                                                                            UserId = applicationUser.Id,
                                                                            UserIdFirstConnection = userIdFirstConnection
                                                                        }
                                                                    );

                            retVal.UserData.Id = model.Data.Id;
                            retVal.UserData.UserName = applicationUser.UserName;

                            // applicativi attivi dell'azienda dell'utente (il client mostra solo quelli)
                            if (int.TryParse(company, out var idCompanyActive))
                            {
                                retVal.UserData.ActiveApplications = _companyApplicationRepository.ActiveApplications(idCompanyActive);

                                // voci di menu nascoste ai ruoli dell'azienda (il client le toglie da menu e navigazione)
                                retVal.UserData.MenuRestrictions = _companyMenuRestrictionRepository.Restrictions(idCompanyActive)
                                    .Select(x => new CompanyMenuRestrictionModel { Url = x.Url, Role = x.Role })
                                    .ToList();
                            }


                            var aspNetRoles = _aspNetRolesRepository.FindAll(x => x.Name != null && roles.Contains(x.Name)).ToList();

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


                ApplicationRole? applicationRole = _aspNetRolesRepository.FindAll(x => x.Code == RoleCode.DealerPowerAdmin).FirstOrDefault();
                if (applicationRole != null)
                {
                    if (applicationRole.Name != null)
                    {
                        var usrRole = await _userManager.GetUsersInRoleAsync(applicationRole.Name);
                        if (usrRole != null)
                        {
                            var usrId = usrRole.Select(x => x.Id).ToList();
                            var userDealer = _userDealerRepository.FindAll(x => usrId.Contains(x.IdAspNetUsers) && x.MainUser == true).ToList();

                            foreach (var item in userDealer)
                            {
                                var _dealer = _dealerRepository.FindById(item.IdDealer);
                                if (_dealer != null)
                                {
                                    retVal.DealerList.Add(new DealerListModel()
                                    {
                                        IdAspNetUsers = item.IdAspNetUsers,
                                        IdDealer = item.IdDealer,
                                        Descrizione = _dealer?.Descrizione ?? "",
                                        MainUser = item.MainUser,
                                        Disabled = item.Disabled
                                    });
                                }

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
        public virtual async Task<GenericResult<DealerGetOutModel>> DealerGet(GenericRequest<DealerGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                DealerGetOutModel retVal = new DealerGetOutModel();



                var dealer = await _dealerRepository.FindByIdAsync(model.Data.Id);
                if (dealer != null)
                {
                    if (dealer.Descrizione == "PSL")
                    {
                        int i = 0;

                        i = 10 / i;
                    }


                    retVal.DealerEdit = new DealerEditModel()
                    {
                        Descrizione = dealer.Descrizione,
                        IdDealer = dealer.Id,
                        Disabled = _userDealerRepository.FindAll(x => x.IdDealer == dealer.Id && x.MainUser).Any(x => x.Disabled)
                    };
                }
                else
                {
                    retVal.DealerEdit = new DealerEditModel()
                    {
                        Descrizione = "",
                        IdDealer = 0
                    };
                }



                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<DealerPutOutModel>> DealerPut(GenericRequest<DealerPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                DealerPutOutModel retVal = new DealerPutOutModel();



                Dealer? dealer = await _dealerRepository.FindByIdAsync(model.Data.DealerEdit.IdDealer);
                if (dealer != null)
                {
                    dealer.Descrizione = model.Data.DealerEdit.Descrizione;

                    await _dealerRepository.UpdateAsync(dealer);


                    // disabilitato: vale per gli utenti principali (quelli mostrati nella lista)
                    if (model.Data.DealerEdit.Disabled != null)
                    {
                        foreach (var row in _userDealerRepository.FindAll(x => x.IdDealer == dealer.Id && x.MainUser).ToList())
                        {
                            row.Disabled = model.Data.DealerEdit.Disabled.Value;
                            await _userDealerRepository.UpdateAsync(row);
                        }
                    }
                }
                else
                {
                    dealer = new Dealer()
                    {
                        Descrizione = StringHelper.RemoveSpecialCharacters(model.Data.DealerEdit.Descrizione)
                    };

                    dealer = await _dealerRepository.UpsertAsync(dealer);

                    string password = model.Data.DealerEdit.Pw != null ? model.Data.DealerEdit.Pw : "1234";


                    //DealerPowerAdmin
                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = dealer.Descrizione + "_PowerAdmin",
                        Email = model.Data.DealerEdit.Mail
                    };

                    var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRoleAsync(user, "DealerPowerAdmin");

                        await _userDealerRepository.UpsertAsync(new UserDealer()
                        {
                            IdAspNetUsers = user.Id,
                            IdDealer = dealer.Id,
                            MainUser = true
                        });
                    }

                    //DealerAdmin
                    user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = dealer.Descrizione + "_Admin",
                        Email = model.Data.DealerEdit.Mail
                    };

                    result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRoleAsync(user, "DealerAdmin");

                        await _userDealerRepository.UpsertAsync(new UserDealer()
                        {
                            IdAspNetUsers = user.Id,
                            IdDealer = dealer.Id,
                            MainUser = true
                        });
                    }


                }



                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }


        public virtual async Task<GenericResult<FinancialAdvisorListOutModel>> FinancialAdvisorList(GenericRequest<FinancialAdvisorListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                FinancialAdvisorListOutModel retVal = new FinancialAdvisorListOutModel();


                ApplicationRole? applicationRole = _aspNetRolesRepository.FindAll(x => x.Code == RoleCode.FinancialAdvisorPowerAdmin).FirstOrDefault();
                if (applicationRole != null)
                {
                    if (applicationRole.Name != null)
                    {
                        var usrRole = await _userManager.GetUsersInRoleAsync(applicationRole.Name);
                        if (usrRole != null)
                        {
                            int IdDealer;
                            int.TryParse(this.CurrentDealer, out IdDealer);

                            var financialAdvisorIdList = _financialAdvisorRepository.FindAll(x => x.IdDealer == IdDealer).Select(x => x.Id).ToList();


                            var usrId = usrRole.Select(x => x.Id).ToList();
                            var userFinancialAdvisor = _userFinancialAdvisorRepository.FindAll(x => usrId.Contains(x.IdAspNetUsers) && x.MainUser == true && financialAdvisorIdList.Contains(x.IdFinancialAdvisor))
                                                                                      .ToList();

                            foreach (var item in userFinancialAdvisor)
                            {
                                var _financialAdvisor = _financialAdvisorRepository.FindById(item.IdFinancialAdvisor);

                                retVal.FinancialAdvisorList.Add(new FinancialAdvisorListModel()
                                {
                                    IdAspNetUsers = item.IdAspNetUsers,
                                    IdFinancialAdvisor = item.IdFinancialAdvisor,
                                    Descrizione = _financialAdvisor?.Descrizione,
                                    MainUser = item.MainUser,
                                    Disabled = item.Disabled
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
        public virtual async Task<GenericResult<FinancialAdvisorGetOutModel>> FinancialAdvisorGet(GenericRequest<FinancialAdvisorGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {

                FinancialAdvisorGetOutModel retVal = new FinancialAdvisorGetOutModel();

                var financialAdvisor = await _financialAdvisorRepository.FindByIdAsync(model.Data.Id);
                if (financialAdvisor != null)
                {
                    retVal.FinancialAdvisorEdit = new FinancialAdvisorEditModel()
                    {
                        Descrizione = financialAdvisor.Descrizione,
                        IdFinancialAdvisor = financialAdvisor.Id,
                        Disabled = _userFinancialAdvisorRepository.FindAll(x => x.IdFinancialAdvisor == financialAdvisor.Id && x.MainUser).Any(x => x.Disabled)
                    };
                }
                else
                {
                    retVal.FinancialAdvisorEdit = new FinancialAdvisorEditModel()
                    {
                        Descrizione = "",
                        IdFinancialAdvisor = 0
                    };
                }


                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<FinancialAdvisorPutOutModel>> FinancialAdvisorPut(GenericRequest<FinancialAdvisorPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                FinancialAdvisorPutOutModel retVal = new FinancialAdvisorPutOutModel();


                FinancialAdvisor? financialAdvisor = await _financialAdvisorRepository.FindByIdAsync(model.Data.FinancialAdvisorEdit.IdFinancialAdvisor);
                if (financialAdvisor != null)
                {
                    financialAdvisor.Descrizione = model.Data.FinancialAdvisorEdit.Descrizione;

                    await _financialAdvisorRepository.UpdateAsync(financialAdvisor);


                    // disabilitato: vale per gli utenti principali (quelli mostrati nella lista)
                    if (model.Data.FinancialAdvisorEdit.Disabled != null)
                    {
                        foreach (var row in _userFinancialAdvisorRepository.FindAll(x => x.IdFinancialAdvisor == financialAdvisor.Id && x.MainUser).ToList())
                        {
                            row.Disabled = model.Data.FinancialAdvisorEdit.Disabled.Value;
                            await _userFinancialAdvisorRepository.UpdateAsync(row);
                        }
                    }
                }
                else
                {
                    ////

                    int IdDealer;
                    int.TryParse(this.CurrentDealer, out IdDealer);

                    financialAdvisor = new FinancialAdvisor()
                    {
                        IdDealer = IdDealer,
                        Descrizione = StringHelper.RemoveSpecialCharacters(model.Data.FinancialAdvisorEdit.Descrizione)
                    };

                    financialAdvisor = await _financialAdvisorRepository.UpsertAsync(financialAdvisor);

                    string password = model.Data.FinancialAdvisorEdit.Pw != null ? model.Data.FinancialAdvisorEdit.Pw : "1234";


                    //DealerPowerAdmin
                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = financialAdvisor.Descrizione + "_PowerAdmin", //FinancialAdvisorPowerAdmin
                        Email = model.Data.FinancialAdvisorEdit.Mail
                    };

                    var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRoleAsync(user, "FinancialAdvisorPowerAdmin");

                        await _userFinancialAdvisorRepository.UpsertAsync(new UserFinancialAdvisor()
                        {
                            IdAspNetUsers = user.Id,
                            IdFinancialAdvisor = financialAdvisor.Id,
                            MainUser = true
                        });
                    }

                    //DealerAdmin
                    user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = financialAdvisor.Descrizione + "_Admin", //FinancialAdvisorAdmin
                        Email = model.Data.FinancialAdvisorEdit.Mail
                    };

                    result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRoleAsync(user, "FinancialAdvisorAdmin");

                        await _userFinancialAdvisorRepository.UpsertAsync(new UserFinancialAdvisor()
                        {
                            IdAspNetUsers = user.Id,
                            IdFinancialAdvisor = financialAdvisor.Id,
                            MainUser = true
                        });
                    }
                    ////
                }




                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }



        public virtual async Task<GenericResult<CompanyListOutModel>> CompanyList(GenericRequest<CompanyListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                CompanyListOutModel retVal = new CompanyListOutModel();


                ApplicationRole? applicationRole = _aspNetRolesRepository.FindAll(x => x.Code == RoleCode.CompanyPowerAdmin).FirstOrDefault();
                if (applicationRole != null)
                {
                    if (applicationRole.Name != null)
                    {
                        var usrRole = await _userManager.GetUsersInRoleAsync(applicationRole.Name);
                        if (usrRole != null)
                        {
                            int IdFinancialAdvisor;
                            int.TryParse(this.CurrentFinancialAdvisor, out IdFinancialAdvisor);

                            var companyIdList = _companyRepository.FindAll(x => x.IdFinancialAdvisor == IdFinancialAdvisor).Select(x => x.Id).ToList();


                            var usrId = usrRole.Select(x => x.Id).ToList();
                            var userCompany = _userCompanyRepository.FindAll(x => usrId.Contains(x.IdAspNetUsers) && x.MainUser == true && companyIdList.Contains(x.IdCompany)).ToList();

                            foreach (var item in userCompany)
                            {
                                var _company = _companyRepository.FindById(item.IdCompany);

                                retVal.CompanyList.Add(new CompanyModel()
                                {
                                    IdAspNetUsers = item.IdAspNetUsers,
                                    IdCompany = item.IdCompany,
                                    Descrizione = _company?.Descrizione,
                                    Disabled = item.Disabled
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
        public virtual async Task<GenericResult<CompanyGetOutModel>> CompanyGet(GenericRequest<CompanyGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {

                CompanyGetOutModel retVal = new CompanyGetOutModel();

                var company = await _companyRepository.FindByIdAsync(model.Data.Id);
                if (company != null)
                {
                    // Inizializzazione lazy delle strutture dati per l'azienda
                    // (Infrastructure + GestionePresenze + eventuali futuri moduli).
                    // Ogni ICompanyInitializer verifica autonomamente se ci sono dati da creare.
                    // La cache in-memory del registry evita riesecuzioni inutili.
                    await _companyInitializerRegistry.InitializeAllAsync(company);

                    retVal.CompanyEdit = new CompanyEditModel()
                    {
                        Descrizione = company.Descrizione,
                        IdCompany = company.Id,
                        Disabled = _userCompanyRepository.FindAll(x => x.IdCompany == company.Id && x.MainUser).Any(x => x.Disabled),
                        Applications = _companyApplicationService.Applications(company.Id)
                    };
                }
                else
                {
                    retVal.CompanyEdit = new CompanyEditModel()
                    {
                        Descrizione = "",
                        IdCompany = 0,
                        Applications = _companyApplicationService.Applications(0)
                    };
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<CompanyPutOutModel>> CompanyPut(GenericRequest<CompanyPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                CompanyPutOutModel retVal = new CompanyPutOutModel();


                Company? company = await _companyRepository.FindByIdAsync(model.Data.CompanyEdit.IdCompany);
                bool nuovaAzienda = company == null;
                if (company != null)
                {
                    company.Descrizione = model.Data.CompanyEdit.Descrizione;

                    await _companyRepository.UpdateAsync(company);


                    // disabilitato: vale per gli utenti principali (quelli mostrati nella lista)
                    if (model.Data.CompanyEdit.Disabled != null)
                    {
                        foreach (var row in _userCompanyRepository.FindAll(x => x.IdCompany == company.Id && x.MainUser).ToList())
                        {
                            row.Disabled = model.Data.CompanyEdit.Disabled.Value;
                            await _userCompanyRepository.UpdateAsync(row);
                        }
                    }
                }
                else
                {
                    /////////////////////

                    int IdFinancialAdvisor;
                    int.TryParse(this.CurrentFinancialAdvisor, out IdFinancialAdvisor);

                    company = new Company()
                    {
                        Descrizione = StringHelper.RemoveSpecialCharacters(model.Data.CompanyEdit.Descrizione),
                        IdFinancialAdvisor = IdFinancialAdvisor,
                    };

                    company = await _companyRepository.UpsertAsync(company);

                    // nessun applicativo attivo: si attivano con CompanyApplicationPut

                    string password = model.Data.CompanyEdit.Pw != null ? model.Data.CompanyEdit.Pw : "1234";


                    //DealerPowerAdmin
                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = string.IsNullOrWhiteSpace(model.Data.CompanyEdit.PowerAdminUserName) ? company.Descrizione + "_PowerAdmin" : model.Data.CompanyEdit.PowerAdminUserName.Trim(), //CompanyPowerAdmin
                        Email = model.Data.CompanyEdit.Mail
                    };

                    var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRoleAsync(user, "CompanyPowerAdmin");

                        await _userCompanyRepository.UpsertAsync(new UserCompany()
                        {
                            IdAspNetUsers = user.Id,
                            IdCompany = company.Id,
                            MainUser = true
                        });
                    }

                    //DealerAdmin
                    user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = string.IsNullOrWhiteSpace(model.Data.CompanyEdit.AdminUserName) ? company.Descrizione + "_Admin" : model.Data.CompanyEdit.AdminUserName.Trim(), //CompanyAdmin
                        Email = model.Data.CompanyEdit.Mail
                    };

                    result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRoleAsync(user, "CompanyAdmin");

                        await _userCompanyRepository.UpsertAsync(new UserCompany()
                        {
                            IdAspNetUsers = user.Id,
                            IdCompany = company.Id,
                            MainUser = true
                        });
                    }

                    //User: un dipendente, come per le aziende create con il DB (PopolateDB), ma uno solo
                    user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = company.Descrizione + "_User", //User
                        Email = model.Data.CompanyEdit.Mail
                    };

                    result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRoleAsync(user, "User");

                        await _userCompanyRepository.UpsertAsync(new UserCompany()
                        {
                            IdAspNetUsers = user.Id,
                            IdCompany = company.Id,
                            MainUser = false
                        });
                    }
                    /////////////////////
                }

                // applicativi dell'azienda:
                //  - spuntato: attivato se serve e schema verificato a ogni salvataggio (creato se manca)
                //  - tolto: solo disattivato, lo schema e i dati restano
                if (company != null && model.Data.CompanyEdit.Applications != null)
                {
                    foreach (var application in model.Data.CompanyEdit.Applications)
                    {
                        var reqApp = new GenericRequest<CompanyApplicationPutInModel>();
                        reqApp.Data = new CompanyApplicationPutInModel()
                        {
                            IdCompany = company.Id,
                            ApplicationType = application.ApplicationType,
                            Active = application.Active
                        };
                        await _companyApplicationService.CompanyApplicationPut(reqApp, true);
                    }
                }

                // nuova azienda: si attivano gli applicativi che il ramo prevede per le nuove aziende
                // (salvo quelli indicati esplicitamente nella richiesta), si inizializza l'azienda
                // con gli ICompanyInitializer e i suoi utenti con gli IUserInitializer
                if (nuovaAzienda && company != null)
                {
                    var richiesti = model.Data.CompanyEdit.Applications?.Select(x => x.ApplicationType).ToList() ?? new List<ApplicationType>();
                    foreach (var application in _companyInitializerRegistry.ApplicationsForNewCompany().Where(a => !richiesti.Contains(a)))
                    {
                        var reqApp = new GenericRequest<CompanyApplicationPutInModel>();
                        reqApp.Data = new CompanyApplicationPutInModel()
                        {
                            IdCompany = company.Id,
                            ApplicationType = application,
                            Active = true
                        };
                        await _companyApplicationService.CompanyApplicationPut(reqApp, true);
                    }

                    await _companyInitializerRegistry.InitializeAllAsync(company);

                    // utenti dell'azienda (PowerAdmin, Admin e dipendente) inizializzati con gli IUserInitializer
                    // degli applicativi ora attivi (es. anagrafica e rapporto di lavoro della gestione presenze)
                    var userCompanies = _userCompanyRepository.FindAll(x => x.IdCompany == company.Id).ToList();
                    foreach (var userCompany in userCompanies)
                        await _userInitializerRegistry.InitializeAllAsync(userCompany);
                }

                if (company != null)
                {
                    var req_2 = new GenericRequest<CompanyGetInModel>();
                    req_2.Data = new CompanyGetInModel() { Id = company.Id };

                    var res_2 = await CompanyGet(req_2, true);
                    if (res_2.Success && res_2.Data != null)
                        retVal.CompanyEdit = res_2.Data.CompanyEdit;
                }


                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }



        public virtual async Task<GenericResult<UserCompanyListOutModel>> UserCompanyList(GenericRequest<UserCompanyListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserCompanyListOutModel retVal = new UserCompanyListOutModel();


                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                List<UserCompany> userCompany_List = _userCompanyRepository.FindAll(x => x.IdCompany == IdCompany).ToList();
                List<string> IdAspNetUsers_List = userCompany_List.Select(x => x.IdAspNetUsers).ToList();

                List<ApplicationUser> ApplicationUser_List = _aspNetUsersRepository.FindAll(x => IdAspNetUsers_List.Contains(x.Id)).ToList();

                List<IdentityUserRole<string>> IdentityUserRole_list = _aspNetUserRolesRepository.FindAll(x => IdAspNetUsers_List.Contains(x.UserId)).ToList();

                List<ApplicationRole> ApplicationRoleList = _aspNetRolesRepository.FindAll(x => true).ToList();

                foreach (var item in userCompany_List)
                {
                    var cur_user = ApplicationUser_List.Where(x => x.Id == item.IdAspNetUsers).FirstOrDefault();
                    var roles_of_user = IdentityUserRole_list.Where(x => x.UserId == item.IdAspNetUsers).ToList();

                    var  cur_role_of_user_id =  roles_of_user.Select(cr=> cr.RoleId).ToList();
                    var  cur_role_of_user_name = ApplicationRoleList.Where( x=> cur_role_of_user_id.Contains(x.Id) ).Select(x=> x.Name).ToList();


                    Boolean FilteredRolesAbil = true;
                    if(model.Data.FilteredRoles.Any())
                    {
                          if (!cur_role_of_user_name.Any(r => r != null && model.Data.FilteredRoles.Contains(r)))
                              FilteredRolesAbil = false;
                    }

                    if (roles_of_user.Any()  && cur_user != null && FilteredRolesAbil)
                    {
                        retVal.UserCompanyList.Add(new UserCompanyModel()
                        {
                            IdAspNetUsers = item.IdAspNetUsers,
                            IdUserCompany = item.Id,
                            Descrizione = cur_user?.UserName,
                            MainUser = item.MainUser,
                            Disabled = item.Disabled,
                            Roles = new List<string>((await _userManager.GetRolesAsync(cur_user!)).OrderBy(r => r))
                        });
                    }

                }

                // dati aggiuntivi degli applicativi attivi per l'azienda (es. Cognome e Nome)
                var activeApplications = _companyApplicationRepository.ActiveApplications(IdCompany);
                foreach (var extension in _userCompanyListExtensions.Where(x => activeApplications.Contains(x.Application)))
                    await extension.ExtendAsync(IdCompany, retVal.UserCompanyList);

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<UserCompanyGetOutModel>> UserCompanyGet(GenericRequest<UserCompanyGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserCompanyGetOutModel retVal = new UserCompanyGetOutModel();



                var userCompany = await _userCompanyRepository.FindByIdAsync(model.Data.Id);
                if (userCompany != null)
                {
                    // Inizializzazione lazy delle strutture dati per l'utente
                    // (Infrastructure + GestionePresenze + eventuali futuri moduli).
                    // Ogni IUserInitializer verifica autonomamente se ci sono dati da creare.
                    // La cache in-memory del registry evita riesecuzioni inutili.
                    await _userInitializerRegistry.InitializeAllAsync(userCompany);

                    ApplicationUser? applicationUser = await _userManager.FindByIdAsync(userCompany.IdAspNetUsers);
                    IdentityUserRole<string>? identityUserRole = _aspNetUserRolesRepository.FindAll(x => x.UserId == userCompany.IdAspNetUsers).FirstOrDefault();

                    if (applicationUser != null && identityUserRole != null)
                    {
                        retVal.UserCompanyEdit = new UserCompanyEditModel()
                        {
                            Descrizione = applicationUser.UserName,
                            IdUserCompany = userCompany.Id,
                            IdAspNetUsers = userCompany.IdAspNetUsers,
                            Mail = applicationUser.Email,
                            MainUser = false,
                            Disabled = userCompany.Disabled,
                            RoleId = identityUserRole.RoleId,
                            Roles = new List<string>(await _userManager.GetRolesAsync(applicationUser))
                        };
                    }
                }
                else
                {
                    retVal.UserCompanyEdit = new UserCompanyEditModel()
                    {
                        Descrizione = "",
                        //IdAspNetUsers = string.Empty
                        IdUserCompany = 0
                    };
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        // cambia l'email dell'utente se ne arriva una diversa (vuota/null = nessuna modifica)
        protected async Task AggiornaMail(ApplicationUser applicationUser, string? mail)
        {
            string nuova = mail?.Trim() ?? string.Empty;
            if (nuova.Length == 0 || string.Equals(nuova, applicationUser.Email, StringComparison.OrdinalIgnoreCase))
                return;

            var result = await _userManager.SetEmailAsync(applicationUser, nuova);
            if (!result.Succeeded)
                throw new UserMessageException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        public virtual async Task<GenericResult<UserCompanyPutOutModel>> UserCompanyPut(GenericRequest<UserCompanyPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserCompanyPutOutModel retVal = new UserCompanyPutOutModel();

                UserCompany? userCompany = await _userCompanyRepository.FindByIdAsync(model.Data.UserCompanyEdit.IdUserCompany);
                if (userCompany != null)
                {
                    //userCompany.Descrizione = model.Data.UserCompanyEdit.Descrizione;

                    if (model.Data.UserCompanyEdit.Disabled != null)
                        userCompany.Disabled = model.Data.UserCompanyEdit.Disabled.Value;

                    await _userCompanyRepository.UpdateAsync(userCompany);


                    var applicationUser = await _userManager.FindByIdAsync(userCompany.IdAspNetUsers);
                    if(applicationUser!=null)
                    {
                        // email: vuota/null = non modificare (es. salvataggi da altri flussi che non conoscono il campo)
                        await AggiornaMail(applicationUser, model.Data.UserCompanyEdit.Mail);

                        var ruoliAttuali = await _userManager.GetRolesAsync(applicationUser);

                        // Trova i ruoli da aggiungere e rimuovere
                        var ruoliDaAggiungere = model.Data.UserCompanyEdit.Roles.Where(ruolo => !ruoliAttuali.Contains(ruolo)).ToList();
                        var ruoliDaRimuovere = ruoliAttuali.Where(ruolo => !model.Data.UserCompanyEdit.Roles.Contains(ruolo)).ToList();

                        // Esegui gli aggiornamenti necessari
                        if (ruoliDaAggiungere != null)
                        {
                            if (ruoliDaAggiungere.Count > 0)
                                await _userManager.AddToRolesAsync(applicationUser, ruoliDaAggiungere);
                        }
                        if (ruoliDaRimuovere != null)
                        {
                            if (ruoliDaRimuovere.Count > 0)
                                await _userManager.RemoveFromRolesAsync(applicationUser, ruoliDaRimuovere);
                        }
                    }
                }
                else
                {
                    ////
                    int IdCompany;
                    int.TryParse(this.CurrentCompany, out IdCompany);



                    //DealerPowerAdmin
                    string password = model.Data.UserCompanyEdit.Pw != null ? model.Data.UserCompanyEdit.Pw : "1234";

                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = model.Data.UserCompanyEdit.Descrizione,
                        Email = model.Data.UserCompanyEdit.Mail
                    };

                    var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRolesAsync(user, model.Data.UserCompanyEdit.Roles);

                         var comUser = await _userCompanyRepository.UpsertAsync(new UserCompany()
                                        {
                                            IdAspNetUsers = user.Id,
                                            IdCompany = IdCompany,
                                            MainUser = false
                                        });
                          if(comUser!=null)
                          {
                               model.Data.UserCompanyEdit.IdUserCompany = comUser.Id;
                          }
                    }

                }

                
              var req_2 = new GenericRequest<UserCompanyGetInModel>();
              req_2.Data =  new UserCompanyGetInModel() { Id= model.Data.UserCompanyEdit.IdUserCompany };

              var res_2 = await  UserCompanyGet(req_2,true);
              if(res_2.Success && res_2.Data != null)
                 retVal.UserCompanyEdit = res_2.Data.UserCompanyEdit;


                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }


        public virtual async Task<GenericResult<UserListOutModel>> UserList(GenericRequest<UserListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserListOutModel retVal = new UserListOutModel();


                List<string> applicationRole = _aspNetRolesRepository.FindAll(x => x.Code == RoleCode.Admin ||
                                                                                   x.Code == RoleCode.PowerAdmin)
                                                                     .Select(x => x.Id).ToList();

                List<IdentityUserRole<string>> IdentityUserRole_list = _aspNetUserRolesRepository.FindAll(x => applicationRole.Contains(x.RoleId)).ToList();

                List<string> IdAspNetUsers_List = IdentityUserRole_list.Select(x => x.UserId).ToList();

                List<ApplicationUser> ApplicationUser_List = _aspNetUsersRepository.FindAll(x => IdAspNetUsers_List.Contains(x.Id)).ToList();

                foreach (var item in IdentityUserRole_list)
                {
                    var cur_user = ApplicationUser_List.Where(x => x.Id == item.UserId).FirstOrDefault();
                    if (cur_user != null)
                    {
                        retVal.UserList.Add(new UserListModel()
                        {
                            IdAspNetUsers = item.UserId,
                            Descrizione = cur_user?.UserName,
                            RoleId = item.RoleId
                        });
                    }
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<UserGetOutModel>> UserGet(GenericRequest<UserGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserGetOutModel retVal = new UserGetOutModel();


                ApplicationUser? applicationUser = await _userManager.FindByIdAsync(model.Data.Id);
                if (applicationUser != null)
                {
                    var identityUserRole = _aspNetUserRolesRepository.FindAll(x => x.UserId == applicationUser.Id).FirstOrDefault();

                    retVal.UserEdit = new UserEditModel()
                    {
                        Descrizione = applicationUser.UserName,
                        IdAspNetUsers = applicationUser.Id,
                        Mail = applicationUser.Email,
                        RoleId = identityUserRole != null ? identityUserRole.RoleId : string.Empty
                    };
                }
                else
                {
                    retVal.UserEdit = new UserEditModel()
                    {
                        Descrizione = "",
                        IdAspNetUsers = string.Empty,
                        RoleId = string.Empty,
                    };
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<UserPutOutModel>> UserPut(GenericRequest<UserPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserPutOutModel retVal = new UserPutOutModel();

                ApplicationUser? applicationUser = await _userManager.FindByIdAsync(model.Data.UserEdit.IdAspNetUsers);
                if (applicationUser != null)
                {
                    //userCompany.Descrizione = model.Data.UserCompanyEdit.Descrizione;

                    await _userManager.UpdateAsync(applicationUser);
                }
                else
                {
                    var roleName = _aspNetRolesRepository.FindAll(x => x.Id == model.Data.UserEdit.RoleId)
                                                         .Select(x => x.Name).FirstOrDefault();
                    if (roleName == null)
                        roleName = "User";


                    //DealerPowerAdmin
                    string password = model.Data.UserEdit.Pw != null ? model.Data.UserEdit.Pw : "1234";

                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = model.Data.UserEdit.Descrizione,
                        Email = model.Data.UserEdit.Mail
                    };

                    var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRoleAsync(user, roleName);

                    }

                }




                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }


        public virtual async Task<GenericResult<UserDealerListOutModel>> UserDealerList(GenericRequest<UserDealerListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserDealerListOutModel retVal = new UserDealerListOutModel();


                int IdDealer;
                int.TryParse(this.CurrentDealer, out IdDealer);

                List<UserDealer> userDealer_List = _userDealerRepository.FindAll(x => x.IdDealer == IdDealer).ToList();
                List<string> IdAspNetUsers_List = userDealer_List.Select(x => x.IdAspNetUsers).ToList();

                List<ApplicationUser> ApplicationUser_List = _aspNetUsersRepository.FindAll(x => IdAspNetUsers_List.Contains(x.Id)).ToList();

                List<IdentityUserRole<string>> IdentityUserRole_list = _aspNetUserRolesRepository.FindAll(x => IdAspNetUsers_List.Contains(x.UserId)).ToList();

                foreach (var item in userDealer_List)
                {
                    var cur_user = ApplicationUser_List.Where(x => x.Id == item.IdAspNetUsers).FirstOrDefault();
                    var cur_role = IdentityUserRole_list.Where(x => x.UserId == item.IdAspNetUsers).FirstOrDefault();

                    if (cur_role != null)
                    {
                        retVal.UserDealerList.Add(new UserDealerModel()
                        {
                            IdAspNetUsers = item.IdAspNetUsers,
                            IdUserDealer = item.Id,
                            Descrizione = cur_user?.UserName,
                            MainUser = item.MainUser,
                            Disabled = item.Disabled,
                            RoleId = cur_role.RoleId
                        });
                    }

                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<UserDealerGetOutModel>> UserDealerGet(GenericRequest<UserDealerGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserDealerGetOutModel retVal = new UserDealerGetOutModel();



                var userDealer = await _userDealerRepository.FindByIdAsync(model.Data.Id);
                if (userDealer != null)
                {
                    ApplicationUser? applicationUser = await _userManager.FindByIdAsync(userDealer.IdAspNetUsers);
                    IdentityUserRole<string>? identityUserRole = _aspNetUserRolesRepository.FindAll(x => x.UserId == userDealer.IdAspNetUsers).FirstOrDefault();

                    if (applicationUser != null && identityUserRole != null)
                    {
                        retVal.UserDealerEdit = new UserDealerEditModel()
                        {
                            Descrizione = applicationUser.UserName,
                            IdUserDealer = userDealer.Id,
                            Mail = applicationUser.Email,
                            MainUser = false,
                            Disabled = userDealer.Disabled,
                            RoleId = identityUserRole.RoleId
                        };
                    }
                }
                else
                {
                    retVal.UserDealerEdit = new UserDealerEditModel()
                    {
                        Descrizione = "",
                        //IdAspNetUsers = string.Empty
                        IdUserDealer = 0
                    };
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<UserDealerPutOutModel>> UserDealerPut(GenericRequest<UserDealerPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserDealerPutOutModel retVal = new UserDealerPutOutModel();

                UserDealer? userDealer = await _userDealerRepository.FindByIdAsync(model.Data.UserDealerEdit.IdUserDealer);
                if (userDealer != null)
                {


                    if (model.Data.UserDealerEdit.Disabled != null)
                        userDealer.Disabled = model.Data.UserDealerEdit.Disabled.Value;

                    await _userDealerRepository.UpdateAsync(userDealer);
                }
                else
                {
                    ////
                    int IdDealer;
                    int.TryParse(this.CurrentDealer, out IdDealer);



                    //DealerPowerAdmin
                    string password = model.Data.UserDealerEdit.Pw != null ? model.Data.UserDealerEdit.Pw : "1234";

                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = model.Data.UserDealerEdit.Descrizione,
                        Email = model.Data.UserDealerEdit.Mail
                    };

                    var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        var roleName = _aspNetRolesRepository.FindAll(x => x.Id == model.Data.UserDealerEdit.RoleId)
                                                       .Select(x => x.Name).FirstOrDefault();

                        if (string.IsNullOrEmpty(roleName))
                            roleName = "DealerAdmin";

                        result = await _userManager.AddToRoleAsync(user, roleName);

                        await _userDealerRepository.UpsertAsync(new UserDealer()
                        {
                            IdAspNetUsers = user.Id,
                            IdDealer = IdDealer,
                            MainUser = false
                        });
                    }

                }




                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }


        public virtual async Task<GenericResult<UserFinancialAdvisorListOutModel>> UserFinancialAdvisorList(GenericRequest<UserFinancialAdvisorListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserFinancialAdvisorListOutModel retVal = new UserFinancialAdvisorListOutModel();


                int IdFinancialAdvisor;
                int.TryParse(this.CurrentFinancialAdvisor, out IdFinancialAdvisor);

                List<UserFinancialAdvisor> userFinancialAdvisor_List = _userFinancialAdvisorRepository.FindAll(x => x.IdFinancialAdvisor == IdFinancialAdvisor).ToList();
                List<string> IdAspNetUsers_List = userFinancialAdvisor_List.Select(x => x.IdAspNetUsers).ToList();

                List<ApplicationUser> ApplicationUser_List = _aspNetUsersRepository.FindAll(x => IdAspNetUsers_List.Contains(x.Id)).ToList();

                List<IdentityUserRole<string>> IdentityUserRole_list = _aspNetUserRolesRepository.FindAll(x => IdAspNetUsers_List.Contains(x.UserId)).ToList();

                foreach (var item in userFinancialAdvisor_List)
                {
                    var cur_user = ApplicationUser_List.Where(x => x.Id == item.IdAspNetUsers).FirstOrDefault();
                    var cur_role = IdentityUserRole_list.Where(x => x.UserId == item.IdAspNetUsers).FirstOrDefault();

                    if (cur_role != null)
                    {
                        retVal.UserFinancialAdvisorList.Add(new UserFinancialAdvisorModel()
                        {
                            IdAspNetUsers = item.IdAspNetUsers,
                            IdUserFinancialAdvisor = item.Id,
                            Descrizione = cur_user?.UserName,
                            MainUser = item.MainUser,
                            Disabled = item.Disabled,
                            RoleId = cur_role.RoleId
                        });
                    }

                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<UserFinancialAdvisorGetOutModel>> UserFinancialAdvisorGet(GenericRequest<UserFinancialAdvisorGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserFinancialAdvisorGetOutModel retVal = new UserFinancialAdvisorGetOutModel();



                var userFinancialAdvisor = await _userFinancialAdvisorRepository.FindByIdAsync(model.Data.Id);
                if (userFinancialAdvisor != null)
                {
                    ApplicationUser? applicationUser = await _userManager.FindByIdAsync(userFinancialAdvisor.IdAspNetUsers);
                    IdentityUserRole<string>? identityUserRole = _aspNetUserRolesRepository.FindAll(x => x.UserId == userFinancialAdvisor.IdAspNetUsers).FirstOrDefault();

                    if (applicationUser != null && identityUserRole != null)
                    {
                        retVal.UserFinancialAdvisorEdit = new UserFinancialAdvisorEditModel()
                        {
                            Descrizione = applicationUser.UserName,
                            IdUserFinancialAdvisor = userFinancialAdvisor.Id,
                            Mail = applicationUser.Email,
                            MainUser = false,
                            Disabled = userFinancialAdvisor.Disabled,
                            RoleId = identityUserRole.RoleId
                        };
                    }
                }
                else
                {
                    retVal.UserFinancialAdvisorEdit = new UserFinancialAdvisorEditModel()
                    {
                        Descrizione = "",
                        //IdAspNetUsers = string.Empty
                        IdUserFinancialAdvisor = 0
                    };
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<UserFinancialAdvisorPutOutModel>> UserFinancialAdvisorPut(GenericRequest<UserFinancialAdvisorPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserFinancialAdvisorPutOutModel retVal = new UserFinancialAdvisorPutOutModel();

                UserFinancialAdvisor? userFinancialAdvisor = await _userFinancialAdvisorRepository.FindByIdAsync(model.Data.UserFinancialAdvisorEdit.IdUserFinancialAdvisor);
                if (userFinancialAdvisor != null)
                {


                    if (model.Data.UserFinancialAdvisorEdit.Disabled != null)
                        userFinancialAdvisor.Disabled = model.Data.UserFinancialAdvisorEdit.Disabled.Value;

                    await _userFinancialAdvisorRepository.UpdateAsync(userFinancialAdvisor);
                }
                else
                {
                    ////
                    int IdFinancialAdvisor;
                    int.TryParse(this.CurrentFinancialAdvisor, out IdFinancialAdvisor);



                    //FinancialAdvisorPowerAdmin
                    string password = model.Data.UserFinancialAdvisorEdit.Pw != null ? model.Data.UserFinancialAdvisorEdit.Pw : "1234";

                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = model.Data.UserFinancialAdvisorEdit.Descrizione,
                        Email = model.Data.UserFinancialAdvisorEdit.Mail
                    };

                    var result = await _userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        var roleName = _aspNetRolesRepository.FindAll(x => x.Id == model.Data.UserFinancialAdvisorEdit.RoleId)
                                                       .Select(x => x.Name).FirstOrDefault();
                        if (string.IsNullOrEmpty(roleName))
                            roleName = "FinancialAdvisorAdmin";

                        result = await _userManager.AddToRoleAsync(user, roleName);

                        await _userFinancialAdvisorRepository.UpsertAsync(new UserFinancialAdvisor()
                        {
                            IdAspNetUsers = user.Id,
                            IdFinancialAdvisor = IdFinancialAdvisor,
                            MainUser = false
                        });
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
        public Task<GenericResult<UserCheckOutModel>> UserCheck(GenericRequest<UserCheckInModel> model, Boolean isSubProcess);
        public Task<GenericResult<UserLoadOutModel>> UserLoad(GenericRequest<UserLoadInModel> model, Boolean isSubProcess);

        public Task<GenericResult<DealerListOutModel>> DealerList(GenericRequest<DealerListInModel> inModel, Boolean isSubProcess);
        public Task<GenericResult<DealerGetOutModel>> DealerGet(GenericRequest<DealerGetInModel> inModel, Boolean isSubProcess);
        public Task<GenericResult<DealerPutOutModel>> DealerPut(GenericRequest<DealerPutInModel> inModel, Boolean isSubProcess);


        public Task<GenericResult<FinancialAdvisorListOutModel>> FinancialAdvisorList(GenericRequest<FinancialAdvisorListInModel> inModel, Boolean isSubProcess);
        public Task<GenericResult<FinancialAdvisorGetOutModel>> FinancialAdvisorGet(GenericRequest<FinancialAdvisorGetInModel> inModel, Boolean isSubProcess);
        public Task<GenericResult<FinancialAdvisorPutOutModel>> FinancialAdvisorPut(GenericRequest<FinancialAdvisorPutInModel> inModel, Boolean isSubProcess);


        public Task<GenericResult<CompanyListOutModel>> CompanyList(GenericRequest<CompanyListInModel> inModel, Boolean isSubProcess);
        public Task<GenericResult<CompanyGetOutModel>> CompanyGet(GenericRequest<CompanyGetInModel> inModel, Boolean isSubProcess);
        public Task<GenericResult<CompanyPutOutModel>> CompanyPut(GenericRequest<CompanyPutInModel> inModel, Boolean isSubProcess);


        public Task<GenericResult<UserCompanyListOutModel>> UserCompanyList(GenericRequest<UserCompanyListInModel> inModel, Boolean isSubProcess);
        public Task<GenericResult<UserCompanyGetOutModel>> UserCompanyGet(GenericRequest<UserCompanyGetInModel> inModel, Boolean isSubProcess);
        public Task<GenericResult<UserCompanyPutOutModel>> UserCompanyPut(GenericRequest<UserCompanyPutInModel> inModel, Boolean isSubProcess);

        public Task<GenericResult<UserListOutModel>> UserList(GenericRequest<UserListInModel> model, Boolean isSubProcess);
        public Task<GenericResult<UserGetOutModel>> UserGet(GenericRequest<UserGetInModel> inModel, Boolean isSubProcess);
        public Task<GenericResult<UserPutOutModel>> UserPut(GenericRequest<UserPutInModel> inModel, Boolean isSubProcess);

        public Task<GenericResult<UserDealerListOutModel>> UserDealerList(GenericRequest<UserDealerListInModel> inModel, Boolean isSubProcess);
        public Task<GenericResult<UserDealerGetOutModel>> UserDealerGet(GenericRequest<UserDealerGetInModel> inModel, Boolean isSubProcess);
        public Task<GenericResult<UserDealerPutOutModel>> UserDealerPut(GenericRequest<UserDealerPutInModel> inModel, Boolean isSubProcess);

        public Task<GenericResult<UserFinancialAdvisorListOutModel>> UserFinancialAdvisorList(GenericRequest<UserFinancialAdvisorListInModel> inModel, Boolean isSubProcess);
        public Task<GenericResult<UserFinancialAdvisorGetOutModel>> UserFinancialAdvisorGet(GenericRequest<UserFinancialAdvisorGetInModel> inModel, Boolean isSubProcess);
        public Task<GenericResult<UserFinancialAdvisorPutOutModel>> UserFinancialAdvisorPut(GenericRequest<UserFinancialAdvisorPutInModel> inModel, Boolean isSubProcess);

    }


}