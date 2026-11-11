using nvxapp.server.service.ClientServer_Service.Infrastructure.License.Seed;
using Serilog;

namespace nvxapp.server.Utility
{
    // Listini iniziali delle licenze all'avvio, dopo le migration (ANALISI_INFRASTRUCTURE_LICENZE.md §15.5): li dichiarano i rami
    // (ILicenseCatalogSeed) e ognuno si crea una sola volta. Un errore non ferma l'avvio: si scrive nel log.
    public static class LicenseCatalogSeedRunner
    {
        public static async Task RunAsync(IServiceProvider services)
        {
            try
            {
                using var scope = services.CreateScope();
                var creati = await scope.ServiceProvider.GetRequiredService<ILicenseCatalogSeeder>().SeedAsync();
                if (creati > 0)
                    Log.Information("Licenze: {Count} listini iniziali creati", creati);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Licenze: creazione dei listini iniziali non riuscita");
            }
        }
    }
}
