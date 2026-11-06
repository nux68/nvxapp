using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // impostazioni dell'applicazione (chiave testuale: si usano i metodi per chiave, non Upsert per Id)
    public class AppSettingRepository : Repository<PublicDbContext, AppSetting>, IAppSettingRepository
    {
        public AppSettingRepository(PublicDbContext dbContext, IServiceProvider provider, IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }

        public Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default)
            => FindAll(x => x.Key == key).Select(x => x.Value).FirstOrDefaultAsync(cancellationToken);

        public async Task SetValueAsync(string key, string? value, CancellationToken cancellationToken = default)
        {
            var row = await DbContext.AppSetting.AsTracking().FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
            if (row == null)
                DbContext.AppSetting.Add(new AppSetting { Key = key, Value = value, ModifiedDate = DateTime.Now });
            else
            {
                row.Value = value;
                row.ModifiedDate = DateTime.Now;
            }
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public interface IAppSettingRepository : IRepository<AppSetting>
    {
        Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default);
        Task SetValueAsync(string key, string? value, CancellationToken cancellationToken = default);
    }
}
