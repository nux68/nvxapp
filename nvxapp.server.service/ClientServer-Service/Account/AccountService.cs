using AutoMapper;
using Microsoft.AspNetCore.Identity;
using nvxapp.server.data.Entities;
using nvxapp.server.data.Repositories;
using nvxapp.server.service.ClientServer_Service.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Infrastructure;
using nvxapp.server.service.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using nvxapp.server.service.ServerModels;


namespace nvxapp.server.service.ClientServer_Service.Account
{
    public class AccountService : ServiceBase, IAccountService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtParameter _jwtParameter;

        public AccountService(IOptions<JwtParameter> jwtParameter,
                              IMapper mapper,
                              UserManager<ApplicationUser> userManager,
                              IAspNetUsersRepository aspNetUsersRepository,
                              SignInManager<ApplicationUser> signInManager
                                      
                              ) : base(mapper, userManager, aspNetUsersRepository)
        {
            _signInManager = signInManager;
            _jwtParameter = jwtParameter.Value;
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
                            retVal.AddMessage("Password errata", MessageType.Error);

                            //throw new Exception("Password errata");
                        }
                        else
                        {
                            retVal.Token = GenerateJwtToken(applicationUser);
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

        private string? GenerateJwtToken(ApplicationUser user)
        {

            if(string.IsNullOrEmpty(  user.Email))
                return null;

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtParameter.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_jwtParameter.Issuer, _jwtParameter.Audience, claims, expires: DateTime.UtcNow.AddHours(2), signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }

    public interface IAccountService : IServiceBase
    {
        public Task<GenericResult<UserRolesOutModel>> UserRoles(GenericRequest<UserRolesInModel> model, Boolean isSubProcess);
        public Task<GenericResult<LoginOutModel>> Login(GenericRequest<LoginInModel> model, Boolean isSubProcess);
    }
}
