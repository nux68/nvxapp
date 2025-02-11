using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Interfaces;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.Infrastructure
{

    public class ServiceBase : IServiceBase, ICurrentUser
    {
        protected readonly IMapper _mapper;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly IAspNetUsersRepository _aspNetUsersRepository;
        protected readonly JwtParameter _jwtParameter;
        private readonly IHttpContextAccessor _httpContextAccessor;

#if DEBUG
        public const int DelayAsyncMethod = 1000;
#else
        public const int DelayAsyncMethod = 1;
#endif


        private string? _currentUser;
        public string? CurrentUserId
        {
            get { return _currentUser; }
            set { _currentUser = value; }
        }

        public ServiceBase(
                          IMapper mapper,
                          UserManager<ApplicationUser> userManager,
                          IAspNetUsersRepository aspNetUsersRepository,
                          IOptions<JwtParameter> jwtParameter,
                          IHttpContextAccessor httpContextAccessor
                          )
        {
            _mapper = mapper;
            _userManager = userManager;
            _aspNetUsersRepository = aspNetUsersRepository;
            _jwtParameter = jwtParameter.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        protected async Task<GenericResult<T>> ExecuteAction<T, P1>(P1? p1, Func<Task<T>> function, Boolean isSubProcess = false)
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


                if (isSubProcess == false)
                {
                    if (data != null && data is iGenericResult4Server)
                    {
                        getAllMessages((iGenericResult4Server)data);
                    }
                }

                return await OkResponse(data);

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

                /* 
                   se è un sotto processo
                   ritorno l'Exception cosi il chiamante la potrà gestire a piacimento
                */
                if (isSubProcess)
                    throw;

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

        protected async Task<GenericResult<T>> OkResponse<T>(T data)
        {
            string? token = null;

            

            if ( !string.IsNullOrEmpty( CurrentUserId))
            {
                var applicationUser = await _userManager.FindByIdAsync(this.CurrentUserId);
                if(applicationUser!=null)
                {
                    var currten = this.CurrentTenat;
                    token = UtilToken.GenerateJwtToken(applicationUser, _jwtParameter.Key, _jwtParameter.Issuer, _jwtParameter.Audience, _jwtParameter.ExpireMinutes, currten);
                }
            }

            // return the model
            return new GenericResult<T>(data, token);
        }

        protected GenericResult<T> ErrorResponse<T>(string error)
        {

            Console.WriteLine(error);
            Log.Error(error);

            return new GenericResult<T>(default(T), error, MessageType.Exception,null);
        }

        protected void TransactionRollback()
        {
            // _databaseTransaction?.Rollback();
        }

        private void getAllMessages(iGenericResult4Server data)
        {

            if (data != null)
            {
                List<Message> AllMessages = new List<Message>();

                var properties = data?.GetType().GetProperties();
                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        var value = property.GetValue(data);
                        if (value is iGenericResult4Server)
                        {
                            getAllMessagesRecursive((iGenericResult4Server)value, AllMessages);
                        }
                    }
                }
                data!.AddMessages(AllMessages);
                
            }

        }

        private void getAllMessagesRecursive(iGenericResult4Server data, List<Message> AllMessages)
        {
            if (data != null)
            {
                var properties = data?.GetType().GetProperties();
                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        var value = property.GetValue(data);
                        if (value is iGenericResult4Server)
                        {
                            getAllMessagesRecursive((iGenericResult4Server)value, AllMessages);
                        }
                    }
                }

                AllMessages.AddRange(data!.GetMessages());

            }
        }

        protected string CurrentTenat
        {
            get
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var tenant = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant")?.Value;
                    return tenant ?? "";
                }

                return "";
            }
        }

    }


}
