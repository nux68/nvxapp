using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using nvxapp.server.data.Entities;
using nvxapp.server.data.Interfaces;
using nvxapp.server.data.Repositories;
using nvxapp.server.service.ClientServer.Models;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using Serilog;

namespace nvxapp.server.service.Infrastructure
{

    public class ServiceBase : IServiceBase , ICurrentUser
    {
        protected readonly IMapper _mapper;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly IAspNetUsersRepository _aspNetUsersRepository;

        private string? _currentUser;
        public string? CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; }
        }

        public ServiceBase(
                          IMapper mapper,
                          UserManager<ApplicationUser> userManager,
                          IAspNetUsersRepository aspNetUsersRepository
                          )
        {
            _mapper = mapper;
            _userManager = userManager;
            _aspNetUsersRepository = aspNetUsersRepository;
        }

        protected async Task<GenericResult<T>> ExecuteAction<T, P1>(P1? p1, Func<Task<T>> function)
        {
            var callGUID = Guid.NewGuid();


            try
            {

                await Initialize();

                Log.Information("Call: {a1}, GUID: {a2}, param: {a3}", function.Method.Name, callGUID, JsonConvert.SerializeObject(p1));


                // execute method
                var data = await function();


                Log.Verbose("END Call: {a1}, GUID: {a2}, result: {a3}", function.Method.Name, callGUID,
                JsonConvert.SerializeObject(data, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));


                return OkResponse(data);

            }
            catch (Exception e)
            {
                // rollback transaction
                this.TransactionRollback();

                var _class = this.GetType().Name;
                //var _err = _class + " - " + e.ToErrorString() + " - " + e.StackTrace;
                // per avere un errore piu' leggibile dall'utente e usare throwexception piu facilmente
                var _err = _class + " - " + e.ToErrorString();

                Log.Error(e, "END Call(ExecuteAction ERR): {a1}, GUID: {a2}, result: {a3}", function.Method.Name, callGUID, _err);

                return ErrorResponse<T>(_err);
            }
        }


        protected async Task Initialize()
        {
            //var authUser = Request.ToUserAuth();

            //if (authUser == null || authUser.IsNotAuthorized())
            //    throw new Exception(HttpErrors.NotAuthorized.ToErrorString());

            //// get the data user and the loggedInUser role
            //LoggedInUser = await UserManager.GetLoggeInUser(authUser);

            //eliminare
            // Nessun 'await' qui
            await Task.Delay(1);

        }

        protected GenericResult<T> OkResponse<T>(T data)
        {
            // return the model
            return new GenericResult<T>
            {
                Success = true,
                //Token = RefreshToken(),
                //UserName = LoggedInUser.LoginUser.UserName,
                Data = data
            };
        }

        protected GenericResult<T> ErrorResponse<T>(string error)
        {

            Console.WriteLine(error);
            Log.Error(error);

            // return the model
            return new GenericResult<T>
            {
                Success = false,
                Error = error
            };
        }

        protected void TransactionRollback()
        {
           // _databaseTransaction?.Rollback();
        }
    }


}
