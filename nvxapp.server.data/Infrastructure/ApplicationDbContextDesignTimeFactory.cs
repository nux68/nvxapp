using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Infrastructure
{
    public class ApplicationDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        private readonly IHttpContextAccessor? _httpContextAccessor = null;


        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("nvxappDbContext");

            Boolean MultiTenant = false;
            string? sMultiTenant = configuration["DbParameter:MultiTenant"];

            bool.TryParse(sMultiTenant, out MultiTenant);
            SharedSchema.MultiTenant = MultiTenant;


            optionsBuilder.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsAssembly("nvxapp.server.data")
            );

            Console.WriteLine("ApplicationDbContextDesignTimeFactory by N.V.N");

            return new ApplicationDbContext(optionsBuilder.Options, _httpContextAccessor, configuration);
        }
    }
}
