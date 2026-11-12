using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Assets
{
    // tabella di Risorse aziendali (schema tenant_<IdAzienda>_7)
    public class Ast_PrenotazioneRepository : Repository<AssetsDbContext, Ast_Prenotazione>, IAst_PrenotazioneRepository
    {
        // primo argomento del lock consultivo: distingue i lock delle prenotazioni da quelli di altri moduli
        private const int ChiaveLock = 7001;

        public Ast_PrenotazioneRepository(AssetsDbContext dbContext,
                         IServiceProvider provider,
                         Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }

        /*
         Salva la prenotazione (nuova o modificata) solo se la risorsa e' libera nell'intervallo [Inizio, Fine):
         occupano la risorsa le prenotazioni confermate e quelle in attesa di approvazione.
         In una transazione, con un lock consultivo sulla risorsa (pg_advisory_xact_lock, rilasciato al commit/rollback):
         due richieste concorrenti sulla stessa risorsa si mettono in fila, la seconda vede la prima e viene rifiutata.
         Il lock vale per id della risorsa (gli id di schemi diversi possono coincidere: al piu' si serializzano due aziende).
        */
        public async Task<bool> SalvaSeLiberaAsync(Ast_Prenotazione prenotazione)
        {
            await using var tx = await DbContext.Database.BeginTransactionAsync();
            await DbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock({0}, {1})", ChiaveLock, prenotazione.IdAst_Risorsa);

            bool occupata = await DbContext.Set<Ast_Prenotazione>().AsNoTracking()
                                           .AnyAsync(x => x.IdAst_Risorsa == prenotazione.IdAst_Risorsa && x.Id != prenotazione.Id
                                                          && (x.Stato == AstStatoPrenotazione.Confermata || x.Stato == AstStatoPrenotazione.InAttesa)
                                                          && x.Inizio < prenotazione.Fine && x.Fine > prenotazione.Inizio);
            if (occupata)
            {
                await tx.RollbackAsync();
                return false;
            }

            var ora = DateTime.Now;
            prenotazione.ModifiedDate = ora;
            prenotazione.ChangeUser = CurrentUserId;
            if (prenotazione.Id == 0)
            {
                prenotazione.CreationDate = ora;
                DbContext.Set<Ast_Prenotazione>().Add(prenotazione);
            }
            else if (DbContext.Entry(prenotazione).State == EntityState.Detached)
                DbContext.Set<Ast_Prenotazione>().Update(prenotazione);
            await DbContext.SaveChangesAsync();
            await tx.CommitAsync();
            return true;
        }
    }

    public interface IAst_PrenotazioneRepository : IRepository<Ast_Prenotazione>
    {
        /// <summary>Salva la prenotazione se la risorsa e' libera nell'intervallo (controllo atomico); false = conflitto.</summary>
        Task<bool> SalvaSeLiberaAsync(Ast_Prenotazione prenotazione);
    }
}
