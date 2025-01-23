using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.data.Interfaces;
using nvxapp.server.service.Helpers;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RepositoryNetcoreControllerBase : ControllerBase , IActionFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RepositoryNetcoreControllerBase(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected string? CurrentUser
        {
            get
            {

                if (_httpContextAccessor.HttpContext != null)
                {
                    if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("UserName", out var headerValue))
                    {
                        string valore = headerValue.ToString();

                        return valore;
                    }
                    else
                    {
                        // L'header non è presente nella richiesta
                        return null;
                    }
                }

                return null;

            }
        }


        [NonAction]
        public void OnActionExecuting(ActionExecutingContext context)
        {
            this.AssingCurrentUser();
        }

        [NonAction]
        public void OnActionExecuted(ActionExecutedContext context) { }

        protected void AssingCurrentUser()
        {
            Type IRepositoryType = typeof(IRepository<>);

            //Scandice i service
            List<object> ser_base = nvxReflection.GetObjectsOfType<ICurrentUser>(this);
            foreach (var item in ser_base)
            {
                ((ICurrentUser)item).CurrentUser = this.CurrentUser;

                //scandice i repo
                List<object> repo_base = nvxReflection.GetObjectsOfType<ICurrentUser>(item);
                foreach (var item2 in repo_base)
                {
                    ((ICurrentUser)item2).CurrentUser = this.CurrentUser;
                }

            }

        }



    }
}
