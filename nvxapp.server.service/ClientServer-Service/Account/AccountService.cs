using AutoMapper;
using Microsoft.AspNetCore.Identity;
using nvxapp.server.data.Entities;
using nvxapp.server.data.Repositories;
using nvxapp.server.service.ClientServer_Service.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Infrastructure;
using nvxapp.server.service.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace nvxapp.server.service.ClientServer_Service.Account
{
    public class AccountService : ServiceBase, IAccountService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountService(IMapper mapper,
                                      UserManager<ApplicationUser> userManager,
                                      IAspNetUsersRepository aspNetUsersRepository,
                                      SignInManager<ApplicationUser> signInManager
                                      ) : base(mapper, userManager, aspNetUsersRepository)
        {
            _signInManager = signInManager;
        }

        public virtual async Task<GenericResult<UserRolesOutModel>> UserRoles(GenericRequest<UserRolesInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                UserRolesOutModel retVal = new UserRolesOutModel();


                if(this.CurrentUser!=null)
                {
                    var applicationUser = await _userManager.FindByNameAsync(this.CurrentUser);
                    

                    if (applicationUser != null)
                    {
                        var roles = await _userManager.GetRolesAsync(applicationUser);
                        retVal.Roles = roles.ToList();
                    }
                }
                

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
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
                            throw new Exception("Password errata");
                        }
                        else
                        {
                            // ok
                        }
                    }
                    else
                    {
                        throw new Exception("Nome utente non trovato");
                    }
                }
                else
                {
                    throw new Exception("Nome utente non valito");
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
        public Task<GenericResult<UserRolesOutModel>> UserRoles(GenericRequest<UserRolesInModel> model, Boolean isSubProcess);
        public Task<GenericResult<LoginOutModel>> Login(GenericRequest<LoginInModel> model, Boolean isSubProcess);
    }
}
