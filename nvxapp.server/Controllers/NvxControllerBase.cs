using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.data.Interfaces;
using nvxapp.server.service.Helpers;
using System.Security.Claims;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NvxControllerBase : ControllerBase , IActionFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NvxControllerBase(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected string? CurrentUserId
        {
            get
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var tenant = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant")?.Value;


                    var UserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    return UserId;
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
                ((ICurrentUser)item).CurrentUserId = this.CurrentUserId;

                //scandice i repo
                List<object> repo_base = nvxReflection.GetObjectsOfType<ICurrentUser>(item);
                foreach (var item2 in repo_base)
                {
                    ((ICurrentUser)item2).CurrentUserId = this.CurrentUserId;
                }

            }

        }



    }
}
