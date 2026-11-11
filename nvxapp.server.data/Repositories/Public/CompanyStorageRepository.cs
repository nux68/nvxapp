using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    /*
     Spazio dei file di ogni azienda. Le variazioni dello spazio sono istruzioni SQL atomiche (UPDATE condizionali):
     due caricamenti contemporanei non superano mai la quota e i contatori non si perdono aggiornamenti.
     La quota la passa il chiamante (licenza Documentale, StorageQuotaService).
    */
    public class CompanyStorageRepository : Repository<PublicDbContext, CompanyStorage>, ICompanyStorageRepository
    {
        public CompanyStorageRepository(PublicDbContext dbContext, IServiceProvider provider, IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }

        public Task<int> InsertIfMissingAsync(int idCompany, CancellationToken cancellationToken = default)
            => DbContext.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO public.""CompanyStorage"" (""IdCompany"", ""UsedBytes"", ""ReservedBytes"", ""FileCount"", ""LastWarningPercent"", ""CreationDate"", ""ModifiedDate"")
                VALUES ({idCompany}, 0, 0, 0, 0, now(), now())
                ON CONFLICT (""IdCompany"") DO NOTHING", cancellationToken);

        // prenotazione atomica con la quota indicata (ANALISI_INFRASTRUCTURE_LICENZE.md §5.1)
        public async Task<bool> ReserveAsync(int idCompany, long bytes, long quotaBytes, CancellationToken cancellationToken = default)
            => await DbContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE public.""CompanyStorage"" cs
                   SET ""ReservedBytes"" = cs.""ReservedBytes"" + {bytes}
                 WHERE cs.""IdCompany"" = {idCompany}
                   AND cs.""UsedBytes"" + cs.""ReservedBytes"" + {bytes} <= {quotaBytes}", cancellationToken) > 0;

        public Task<int> ConfirmAsync(int idCompany, long bytes, CancellationToken cancellationToken = default)
            => DbContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE public.""CompanyStorage""
                   SET ""ReservedBytes"" = GREATEST(""ReservedBytes"" - {bytes}, 0),
                       ""UsedBytes"" = ""UsedBytes"" + {bytes},
                       ""FileCount"" = ""FileCount"" + 1,
                       ""ModifiedDate"" = now()
                 WHERE ""IdCompany"" = {idCompany}", cancellationToken);

        public Task<int> ReleaseAsync(int idCompany, long bytes, CancellationToken cancellationToken = default)
            => DbContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE public.""CompanyStorage""
                   SET ""ReservedBytes"" = GREATEST(""ReservedBytes"" - {bytes}, 0)
                 WHERE ""IdCompany"" = {idCompany}", cancellationToken);

        public Task<int> FreeAsync(int idCompany, long bytes, int files, CancellationToken cancellationToken = default)
            => DbContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE public.""CompanyStorage""
                   SET ""UsedBytes"" = GREATEST(""UsedBytes"" - {bytes}, 0),
                       ""FileCount"" = GREATEST(""FileCount"" - {files}, 0),
                       ""ModifiedDate"" = now()
                 WHERE ""IdCompany"" = {idCompany}", cancellationToken);

        public Task<int> RecalculateAsync(int idCompany, long usedBytes, int fileCount, CancellationToken cancellationToken = default)
            => DbContext.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE public.""CompanyStorage""
                   SET ""UsedBytes"" = {usedBytes}, ""FileCount"" = {fileCount}, ""ReservedBytes"" = 0, ""LastRecalc"" = now()
                 WHERE ""IdCompany"" = {idCompany}", cancellationToken);
    }

    public interface ICompanyStorageRepository : IRepository<CompanyStorage>
    {
        /// <summary>Riga dello spazio dell'azienda, se manca (idempotente).</summary>
        Task<int> InsertIfMissingAsync(int idCompany, CancellationToken cancellationToken = default);

        /// <summary>Prenota lo spazio solo se c'e' entro la quota indicata (atomico); false = quota superata.</summary>
        Task<bool> ReserveAsync(int idCompany, long bytes, long quotaBytes, CancellationToken cancellationToken = default);

        /// <summary>Spazio prenotato che diventa occupato (+1 file).</summary>
        Task<int> ConfirmAsync(int idCompany, long bytes, CancellationToken cancellationToken = default);

        /// <summary>Annulla una prenotazione.</summary>
        Task<int> ReleaseAsync(int idCompany, long bytes, CancellationToken cancellationToken = default);

        /// <summary>Libera lo spazio di file eliminati.</summary>
        Task<int> FreeAsync(int idCompany, long bytes, int files, CancellationToken cancellationToken = default);

        /// <summary>Riallinea occupato e numero di file con i valori reali e azzera le prenotazioni.</summary>
        Task<int> RecalculateAsync(int idCompany, long usedBytes, int fileCount, CancellationToken cancellationToken = default);
    }
}
