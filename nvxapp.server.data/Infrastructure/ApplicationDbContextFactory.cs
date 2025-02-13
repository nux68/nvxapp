using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace nvxapp.server.data.Infrastructure
{


    public class ApplicationDbContextFactory : IApplicationDbContextFactory
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public ApplicationDbContextFactory(DbContextOptions<ApplicationDbContext> options,
                                           IHttpContextAccessor httpContextAccessor, 
                                           IConfiguration configuration)
        {
            _options = options;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public ApplicationDbContext CreateDbContext(string? tenant)
        {
            return new ApplicationDbContext(_options, _httpContextAccessor, _configuration, tenant);
        }
    }

    public interface IApplicationDbContextFactory
    {
        ApplicationDbContext CreateDbContext(string? tenant);
    }

}
