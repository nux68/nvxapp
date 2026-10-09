using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant
{

    /*
     Repository di una tabella dell'applicativo Moke: usa MokeDbContext, che lavora sullo schema
     dell'azienda della richiesta per Moke (search_path impostato all'apertura della connessione).
     */

    public class MyTableRepository : Repository<MokeDbContext, MyTable>, IMyTableRepository
    {
        public MyTableRepository(MokeDbContext dbContext,
                                 IServiceProvider provider,
                                 IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IMyTableRepository : IRepository<MyTable>
    {
    }
}
