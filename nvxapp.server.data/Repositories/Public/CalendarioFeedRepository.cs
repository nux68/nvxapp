using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // feed iCalendar personale (INF-CAL): vedi Entities/Public/ServiziComuni.cs
    public class CalendarioFeedRepository : Repository<PublicDbContext, CalendarioFeed>, ICalendarioFeedRepository
    {
        public CalendarioFeedRepository(PublicDbContext dbContext,
                                        IServiceProvider provider,
                                        IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }

        // il feed si scarica senza utente collegato: UPDATE diretto, senza toccare data e utente della modifica
        public async Task SegnaAccessoAsync(int id, DateTime quando)
        {
            await DbContext.Set<CalendarioFeed>().Where(x => x.Id == id)
                           .ExecuteUpdateAsync(s => s.SetProperty(x => x.DataUltimoAccesso, quando));
        }
    }

    public interface ICalendarioFeedRepository : IRepository<CalendarioFeed>
    {
        /// <summary>registra l'ultimo scaricamento del feed</summary>
        Task SegnaAccessoAsync(int id, DateTime quando);
    }
}
