using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Initializer.CompanyInit
{
    // Dati comuni dell'azienda (schema Common): schema e Az_Anagrafica, sempre, prima di ogni applicativo.
    // Priority=5: prima di Infrastructure (10) e degli applicativi.
    public class CommonCompanyInitializer : ICompanyInitializer
    {
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly ILogger<CommonCompanyInitializer> _logger;

        public string Name => "Common";
        public int Priority => 5;
        public ApplicationType? Application => null; // eseguito sempre

        public CommonCompanyInitializer(ICommonCompanyUtility commonCompanyUtility, ILogger<CommonCompanyInitializer> logger)
        {
            _commonCompanyUtility = commonCompanyUtility;
            _logger = logger;
        }

        public async Task InitializeAsync(Company company)
        {
            var az_Anagrafica = await _commonCompanyUtility.GetAz_AnagraficaAsync(company.Id, createIfMissing: true);
            _logger.LogInformation("[CommonCompanyInitializer] Azienda {CompanyId}: anagrafica comune {IdAz_Anagrafica}.", company.Id, az_Anagrafica?.Id);
        }
    }
}
