using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // pacchetti di spazio dei file (ANALISI_DOCUMENTALE.md [branch DocumentManagement] §5.3)
    public class StoragePlanRepository : Repository<PublicDbContext, StoragePlan>, IStoragePlanRepository
    {
        public StoragePlanRepository(PublicDbContext dbContext, IServiceProvider provider, IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }

        public async Task<StoragePlan> SaveAsync(StoragePlan plan, bool predefinito)
        {
            // un solo pacchetto predefinito: il cambio e il salvataggio stanno nella stessa transazione
            await using var transaction = await DbContext.Database.BeginTransactionAsync();
            if (predefinito && !plan.Predefinito)
            {
                await DbContext.StoragePlan.Where(x => x.Predefinito && x.Id != plan.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.Predefinito, false));
                plan.Predefinito = true;
            }
            plan = await UpsertAsync(plan);
            await transaction.CommitAsync();
            return plan;
        }
    }

    public interface IStoragePlanRepository : IRepository<StoragePlan>
    {
        /// <summary>Salva il pacchetto; con predefinito = true diventa l'unico predefinito.</summary>
        Task<StoragePlan> SaveAsync(StoragePlan plan, bool predefinito);
    }
}
