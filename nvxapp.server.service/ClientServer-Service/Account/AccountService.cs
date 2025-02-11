using AutoMapper;
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
        //private readonly JwtParameter _jwtParameter;
        private readonly IAspNetUserRolesRepository _aspNetUserRolesRepository;
        private readonly IAspNetRolesRepository _aspNetRolesRepository;



        public AccountService(IMapper mapper,
                              UserManager<ApplicationUser> userManager,
                              IAspNetUsersRepository aspNetUsersRepository,
                              IOptions<JwtParameter> jwtParameter,

                              IAspNetUserRolesRepository aspNetUserRolesRepository,
                              IAspNetRolesRepository aspNetRolesRepository,
                              SignInManager<ApplicationUser> signInManager
                              ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter)
        {
            _signInManager = signInManager;
            _aspNetUserRolesRepository = aspNetUserRolesRepository;
            _aspNetRolesRepository = aspNetRolesRepository;
            //_jwtParameter = jwtParameter.Value;
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

                        var aspNetRoles = _aspNetRolesRepository.GetAll().Where(x => roles.Contains(x.Name)).ToList();

                        retVal.Roles = _mapper.Map<List<AspNetRolesModel>>(aspNetRoles);
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
                            //retVal.Token = GenerateJwtToken(applicationUser);
                            retVal.Token = UtilToken.GenerateJwtToken(applicationUser, _jwtParameter.Key, _jwtParameter.Issuer, _jwtParameter.Audience, _jwtParameter.ExpireMinutes);
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

        //private string? GenerateJwtToken(ApplicationUser user)
        //{

        //    if( string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.UserName))
        //        return null;

        //    //valori inseriti nel token, saranno disponibili nelle API
        //    var claims = new[]
        //    {
        //        //new Claim(JwtRegisteredClaimNames.Name, user.UserName),
        //        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        //        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        //    };

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtParameter.Key));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);



        //    //var token = new JwtSecurityToken(_jwtParameter.Issuer, _jwtParameter.Audience, claims, expires: DateTime.UtcNow.AddHours(2), signingCredentials: creds);
        //    var token = new JwtSecurityToken(_jwtParameter.Issuer, _jwtParameter.Audience, claims, expires: DateTime.UtcNow.AddSeconds(_jwtParameter.ExpireMinutes), signingCredentials: creds);
        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}


    }

    public interface IAccountService : IServiceBase
    {
        public Task<GenericResult<UserRolesOutModel>> UserRoles(GenericRequest<UserRolesInModel> model, Boolean isSubProcess);
        public Task<GenericResult<LoginOutModel>> Login(GenericRequest<LoginInModel> model, Boolean isSubProcess);
    }
}
