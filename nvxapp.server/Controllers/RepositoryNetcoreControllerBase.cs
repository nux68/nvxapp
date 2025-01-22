using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RepositoryNetcoreControllerBase : ControllerBase //, IActionFilter
    {
        public RepositoryNetcoreControllerBase(/*IHttpContextAccessor httpContextAccessor*/)
        {
            //_httpContextAccessor = httpContextAccessor;
        }

    }
}
