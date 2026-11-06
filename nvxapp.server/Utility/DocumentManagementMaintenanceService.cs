using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.DocumentManagement;
using nvxapp.server.service.ClientServer_Service.DocumentManagement._utility;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage;
using Serilog;

namespace nvxapp.server.Utility
{
    /*
     Manutenzione del documentale, una volta al giorno dopo le 3 di notte (e al primo giro dopo l'avvio):
     svuota il cestino delle aziende con il documentale attivo, eliminando definitivamente i documenti nel cestino da
     piu' di Doc_Cfg.GiorniCestino giorni (0 = mai). I documenti oltre la conservazione NON si eliminano mai da soli
     (si segnalano negli elenchi).
    */
    public class DocumentManagementMaintenanceService : BackgroundService
    {
        private static readonly TimeSpan Intervallo = TimeSpan.FromMinutes(30);
        private readonly IServiceScopeFactory _scopeFactory;
        private DateTime? _ultimoGiorno;

        public DocumentManagementMaintenanceService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try { await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken); } catch (OperationCanceledException) { return; }

            using var timer = new PeriodicTimer(Intervallo);
            do
            {
                var adesso = DateTime.Now;
                if (_ultimoGiorno == null || (_ultimoGiorno < adesso.Date && adesso.Hour >= 3))
                {
                    try
                    {
                        await EseguiAsync(stoppingToken);
                    }
                    catch (OperationCanceledException) { return; }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Manutenzione del documentale non riuscita");
                    }
                    _ultimoGiorno = adesso.Date;
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task EseguiAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;
            var companyApplicationRepository = sp.GetRequiredService<ICompanyApplicationRepository>();
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            var tenancy = sp.GetRequiredService<TenancySettings>();
            var factory = sp.GetRequiredService<IApplicationDbContextFactory>();
            var fileStorage = sp.GetRequiredService<IFileStorage>();
            var quota = sp.GetRequiredService<IStorageQuotaService>();

            var aziende = await companyApplicationRepository
                                        .FindAll(x => x.ApplicationType == ApplicationType.DocumentManagement && x.Active)
                                        .Select(x => x.IdCompany).ToListAsync(cancellationToken);
            int totale = 0;
            foreach (var idCompany in aziende)
            {
                try
                {
                    var schema = tenancy.MultiTenant ? TenantSchemaName.For(idCompany, ApplicationType.DocumentManagement) : TenantSchemaName.Public;
                    // contesto dello schema dell'azienda (fuori da una richiesta): i dati passano dai repository creati su di esso
                    await using var db = (DocumentManagementDbContext)factory.Create(ApplicationType.DocumentManagement, schema);
                    var cfgRepository = new Doc_CfgRepository(db, sp, httpContextAccessor);
                    var documentoRepository = new Doc_DocumentoRepository(db, sp, httpContextAccessor);
                    var versioneRepository = new Doc_VersioneRepository(db, sp, httpContextAccessor);

                    var cfg = await cfgRepository.FindAll(x => x.Az_AnagraficaNavigation!.IdCompany == idCompany).FirstOrDefaultAsync(cancellationToken);
                    if (cfg == null || cfg.GiorniCestino <= 0)
                        continue;

                    var limite = DateTime.Now.AddDays(-cfg.GiorniCestino);
                    var ids = await documentoRepository.FindAll(x => x.IdAz_Anagrafica == cfg.IdAz_Anagrafica && x.NelCestino && x.DataCestino != null && x.DataCestino < limite)
                                      .Select(x => x.Id).ToListAsync(cancellationToken);
                    if (ids.Count == 0)
                        continue;

                    totale += await DocumentFiles.EliminaDocumentiAsync(documentoRepository, versioneRepository, fileStorage, quota, idCompany, ids, cancellationToken);
                }
                catch (Exception ex)
                {
                    // un'azienda con problemi non blocca le altre
                    Log.Error(ex, "Svuotamento del cestino del documentale non riuscito per l'azienda {IdCompany}", idCompany);
                }
            }
            Log.Information("Manutenzione del documentale: {Count} documenti eliminati dal cestino", totale);
        }
    }
}
