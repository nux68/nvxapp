using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.HubAI;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Data;
using System.Linq;
using static nvxapp.server.data.Entities.AspNetUsersDataUtil;


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

        private readonly IDip_RapportoLavoroService _dip_RapportoLavoroService;
        private readonly IDip_ProfiloOrarioService _dip_ProfiloOrarioService;
        
        
        
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        

        private readonly IHubContext<SignalRHub> _hubContext;

        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;


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
                              IGestionePresenzeUserUtility gestionePresenzeUserUtility,

                              IFinancialAdvisorRepository financialAdvisorRepository,
                              IUserFinancialAdvisorRepository userFinancialAdvisorRepository,

                              IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                              IDip_RapportoLavoroService dip_RapportoLavoroService,
                              IDip_ProfiloOrarioService dip_ProfiloOrarioService,

                              ICompanyRepository companyRepository,
                              IUserCompanyRepository userCompanyRepository,
                              IHubContext<SignalRHub> hubContext,

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

            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _dip_RapportoLavoroService = dip_RapportoLavoroService;
            _dip_ProfiloOrarioService = dip_ProfiloOrarioService;

            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;

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
                        var result = await _signInManager.CheckPasswordSignInAsync(applicationUser, model.Data.Password, false);
                        if (!result.Succeeded)
                        {
                            retVal.AddMessage("Password errata", MessageType.Error);

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
                    string userId = string.IsNullOrEmpty(model.Data.IdAspNetUsers) ? this.CurrentUserId : model.Data.IdAspNetUsers;

                    var applicationUser = await _userManager.FindByIdAsync(userId);

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
                        string schema = "";
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
                                            schema = comp.Schema ?? "";
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
                                                                            Tenant = schema,
                                                                            UserId = applicationUser.Id,
                                                                            UserIdFirstConnection = userIdFirstConnection
                                                                        }
                                                                    );

                            retVal.UserData.Id = model.Data.Id;
                            retVal.UserData.UserName = applicationUser.UserName;


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


                ApplicationRole? applicationRole = _aspNetRolesRepository.GetAll().Where(x => x.Code == RoleCode.DealerPowerAdmin).FirstOrDefault();
                if (applicationRole != null)
                {
                    if (applicationRole.Name != null)
                    {
                        var usrRole = await _userManager.GetUsersInRoleAsync(applicationRole.Name);
                        if (usrRole != null)
                        {
                            var usrId = usrRole.Select(x => x.Id).ToList();
                            var userDealer = _userDealerRepository.GetAll().Where(x => usrId.Contains(x.IdAspNetUsers) && x.MainUser == true).ToList();

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
                                        MainUser = item.MainUser
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
                        IdDealer = dealer.Id
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


                ApplicationRole? applicationRole = _aspNetRolesRepository.GetAll().Where(x => x.Code == RoleCode.FinancialAdvisorPowerAdmin).FirstOrDefault();
                if (applicationRole != null)
                {
                    if (applicationRole.Name != null)
                    {
                        var usrRole = await _userManager.GetUsersInRoleAsync(applicationRole.Name);
                        if (usrRole != null)
                        {
                            int IdDealer;
                            int.TryParse(this.CurrentDealer, out IdDealer);

                            var financialAdvisorIdList = _financialAdvisorRepository.GetAll().Where(x => x.IdDealer == IdDealer).Select(x => x.Id).ToList();


                            var usrId = usrRole.Select(x => x.Id).ToList();
                            var userFinancialAdvisor = _userFinancialAdvisorRepository.GetAll()
                                                                                      .Where(x => usrId.Contains(x.IdAspNetUsers) && x.MainUser == true && financialAdvisorIdList.Contains(x.IdFinancialAdvisor))
                                                                                      .ToList();

                            foreach (var item in userFinancialAdvisor)
                            {
                                var _financialAdvisor = _financialAdvisorRepository.FindById(item.IdFinancialAdvisor);

                                retVal.FinancialAdvisorList.Add(new FinancialAdvisorListModel()
                                {
                                    IdAspNetUsers = item.IdAspNetUsers,
                                    IdFinancialAdvisor = item.IdFinancialAdvisor,
                                    Descrizione = _financialAdvisor?.Descrizione,
                                    MainUser = item.MainUser
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
                        IdFinancialAdvisor = financialAdvisor.Id
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


                ApplicationRole? applicationRole = _aspNetRolesRepository.GetAll().Where(x => x.Code == RoleCode.CompanyPowerAdmin).FirstOrDefault();
                if (applicationRole != null)
                {
                    if (applicationRole.Name != null)
                    {
                        var usrRole = await _userManager.GetUsersInRoleAsync(applicationRole.Name);
                        if (usrRole != null)
                        {
                            int IdFinancialAdvisor;
                            int.TryParse(this.CurrentFinancialAdvisor, out IdFinancialAdvisor);

                            var companyIdList = _companyRepository.GetAll().Where(x => x.IdFinancialAdvisor == IdFinancialAdvisor).Select(x => x.Id).ToList();


                            var usrId = usrRole.Select(x => x.Id).ToList();
                            var userCompany = _userCompanyRepository.GetAll().Where(x => usrId.Contains(x.IdAspNetUsers) && x.MainUser == true && companyIdList.Contains(x.IdCompany)).ToList();

                            foreach (var item in userCompany)
                            {
                                var _company = _companyRepository.FindById(item.IdCompany);

                                retVal.CompanyList.Add(new CompanyModel()
                                {
                                    IdAspNetUsers = item.IdAspNetUsers,
                                    IdCompany = item.IdCompany,
                                    Descrizione = _company?.Descrizione
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
                    retVal.CompanyEdit = new CompanyEditModel()
                    {
                        Descrizione = company.Descrizione,
                        IdCompany = company.Id
                    };
                }
                else
                {
                    retVal.CompanyEdit = new CompanyEditModel()
                    {
                        Descrizione = "",
                        IdCompany = 0
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
                if (company != null)
                {
                    company.Descrizione = model.Data.CompanyEdit.Descrizione;

                    await _companyRepository.UpdateAsync(company);
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
                        Schema = "schema_" + StringHelper.RemoveSpecialCharacters(model.Data.CompanyEdit.Descrizione),
                    };

                    company = await _companyRepository.UpsertAsync(company);

                    string password = model.Data.CompanyEdit.Pw != null ? model.Data.CompanyEdit.Pw : "1234";


                    //DealerPowerAdmin
                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = company.Descrizione + "_PowerAdmin", //CompanyPowerAdmin
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
                        UserName = company.Descrizione + "_Admin", //CompanyAdmin
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
                    /////////////////////
                }




                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }


        //TODO MIGLIRARE XKE VERRA USATA MOLTO
        public virtual async Task<GenericResult<UserCompanyListOutModel>> UserCompanyList(GenericRequest<UserCompanyListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserCompanyListOutModel retVal = new UserCompanyListOutModel();


                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                List<UserCompany> userCompany_List = _userCompanyRepository.GetAll().Where(x => x.IdCompany == IdCompany).ToList();
                List<string> IdAspNetUsers_List = userCompany_List.Select(x => x.IdAspNetUsers).ToList();

                List<ApplicationUser> ApplicationUser_List = _aspNetUsersRepository.FindAll(x => IdAspNetUsers_List.Contains(x.Id)).ToList();

                List<IdentityUserRole<string>> IdentityUserRole_list = _aspNetUserRolesRepository.FindAll(x => IdAspNetUsers_List.Contains(x.UserId)).ToList();

                List<ApplicationRole> ApplicationRoleList = _aspNetRolesRepository.GetAll().ToList();

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
                            Roles = new List<string>((await _userManager.GetRolesAsync(cur_user!)).OrderBy(r => r))
                        });
                    }

                }

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
                    ApplicationUser? applicationUser = await _userManager.FindByIdAsync(userCompany.IdAspNetUsers);
                    IdentityUserRole<string>? identityUserRole = _aspNetUserRolesRepository.FindAll(x => x.UserId == userCompany.IdAspNetUsers).FirstOrDefault();

                    if (applicationUser != null && identityUserRole != null)
                    {
                        retVal.UserCompanyEdit = new UserCompanyEditModel()
                        {
                            Descrizione = applicationUser.UserName,
                            IdUserCompany = userCompany.Id,
                            Mail = applicationUser.Email,
                            MainUser = false,
                            RoleId = identityUserRole.RoleId,
                            Roles = new List<string>(await _userManager.GetRolesAsync(applicationUser))
                        };

                        var dipAna =  await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(userCompany.IdAspNetUsers, true);
                        if(dipAna.dip_RapportoLavoro!=null)
                        {
                            var req_1 = new GenericRequest<Dip_RapportoLavoro_Get_InModel>();
                            req_1.Data.Id= dipAna.dip_Anagrafica!.Id; 
                            var resAz_Sub = await _dip_RapportoLavoroService.Dip_RapportoLavoroGet(req_1, true);

                            if (resAz_Sub.Success && resAz_Sub.Data != null)
                            {
                                retVal.UserCompanyEdit.Dip_RapportoLavoro = resAz_Sub.Data.Dip_RapportoLavoro;

                                foreach(var item in retVal.UserCompanyEdit.Dip_RapportoLavoro)
                                {
                                    var req_2 = new GenericRequest<Dip_ProfiloOrario_Get_InModel>();
                                    req_2.Data.Id= item.Id; 
                                    var resAz_Sub_2 = await _dip_ProfiloOrarioService.Dip_ProfiloOrarioGet(req_2, true);
                                    if (resAz_Sub_2.Success && resAz_Sub_2.Data != null)
                                    {
                                        retVal.UserCompanyEdit.Dip_ProfiloOrario.AddRange( resAz_Sub_2.Data.Dip_ProfiloOrario);
                                    }
                                }

                            }
                        }
                        
                        
                    }
                }
                else
                {
                    retVal.UserCompanyEdit = new UserCompanyEditModel()
                    {
                        Descrizione = "",
                        //IdAspNetUsers = string.Empty
                        IdUserCompany = 0,
                        Dip_RapportoLavoro = new List<Dip_RapportoLavoroModel>(),
                        Dip_ProfiloOrario = new List<Dip_ProfiloOrarioModel>()
                    };
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
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

                    await _userCompanyRepository.UpdateAsync(userCompany);


                    var applicationUser = await _userManager.FindByIdAsync(userCompany.IdAspNetUsers);
                    if(applicationUser!=null)
                    {
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

                        var dipAna =  await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(userCompany.IdAspNetUsers, true);
                        if(dipAna.dip_RapportoLavoro!=null)
                        {
                            var req_1 = new GenericRequest<Dip_RapportoLavoro_Put_InModel>();
                            req_1.Data.Id= dipAna.dip_Anagrafica!.Id; 
                            var resAz_Sub = await _dip_RapportoLavoroService.Dip_RapportoLavoroPut(req_1, true);
                            retVal.UserCompanyEdit.Dip_RapportoLavoro = resAz_Sub.Data!.Dip_RapportoLavoro;


                            retVal.UserCompanyEdit.Dip_ProfiloOrario = new List<Dip_ProfiloOrarioModel>(); // azzero la lista da tornare
                            foreach (var item in retVal.UserCompanyEdit.Dip_RapportoLavoro)
                            {
                                var req_2 = new GenericRequest<Dip_ProfiloOrario_Put_InModel>();
                                req_2.Data.Id= item.Id; 
                                req_2.Data.Dip_ProfiloOrario = model.Data.UserCompanyEdit.Dip_ProfiloOrario.Where(x=> x.IdDip_RapportoLavoro == item.Id).ToList();

                                var resAz_Sub_2 = await _dip_ProfiloOrarioService.Dip_ProfiloOrarioPut(req_2, true);
                                if (resAz_Sub_2.Success && resAz_Sub_2.Data != null)
                                {
                                    retVal.UserCompanyEdit.Dip_ProfiloOrario.AddRange( resAz_Sub_2.Data.Dip_ProfiloOrario);
                                }
                            }
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

                        await _userCompanyRepository.UpsertAsync(new UserCompany()
                        {
                            IdAspNetUsers = user.Id,
                            IdCompany = IdCompany,
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


        public virtual async Task<GenericResult<UserListOutModel>> UserList(GenericRequest<UserListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserListOutModel retVal = new UserListOutModel();


                List<string> applicationRole = _aspNetRolesRepository.GetAll()
                                                                     .Where(x => x.Code == RoleCode.Admin ||
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
                    var roleName = _aspNetRolesRepository.GetAll()
                                                                .Where(x => x.Id == model.Data.UserEdit.RoleId)
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

                List<UserDealer> userDealer_List = _userDealerRepository.GetAll().Where(x => x.IdDealer == IdDealer).ToList();
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
                        var roleName = _aspNetRolesRepository.GetAll()
                                                       .Where(x => x.Id == model.Data.UserDealerEdit.RoleId)
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

                List<UserFinancialAdvisor> userFinancialAdvisor_List = _userFinancialAdvisorRepository.GetAll().Where(x => x.IdFinancialAdvisor == IdFinancialAdvisor).ToList();
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
                        var roleName = _aspNetRolesRepository.GetAll()
                                                       .Where(x => x.Id == model.Data.UserFinancialAdvisorEdit.RoleId)
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
