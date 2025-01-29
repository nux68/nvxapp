﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using nvxapp.server.data.Entities;
using nvxapp.server.data.Repositories;
using nvxapp.server.service.ClientServer_Service.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Infrastructure;
using nvxapp.server.service.Interfaces;


namespace nvxapp.server.service.ClientServer_Service.Account
{
    public class AccountService : ServiceBase, IAccountService
    {
        

        public AccountService(IMapper mapper,
                                      UserManager<ApplicationUser> userManager,
                                      IAspNetUsersRepository aspNetUsersRepository
                                      ) : base(mapper, userManager, aspNetUsersRepository)
        {
        
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
                await Task.Delay(1);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<LoginOutModel>> Login(GenericRequest<LoginInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                LoginOutModel retVal = new LoginOutModel();


                //if (this.CurrentUser != null)
                //{
                //    var applicationUser = await _userManager.FindByNameAsync(this.CurrentUser);


                //    if (applicationUser != null)
                //    {
                //        var roles = await _userManager.GetRolesAsync(applicationUser);
                //        retVal.Roles = roles.ToList();
                //    }
                //}


                //eliminare
                // Nessun 'await' qui
                await Task.Delay(1);

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
