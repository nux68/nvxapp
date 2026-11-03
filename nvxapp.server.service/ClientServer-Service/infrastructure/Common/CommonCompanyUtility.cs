using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Tenant.Common;
using System.Collections.Concurrent;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Common
{
    /*
     Dati comuni dell'azienda (schema Common, tenant_<IdAzienda>_0 o public).
     Az_Anagrafica e' l'ancora dell'azienda a cui si agganciano le tabelle comuni e quelle degli
     applicativi: si crea qui (una volta sola), mai dai singoli applicativi.
     L'azienda e' quella della richiesta (claim company) o di TenantScope: idCompany deve coincidere.
    */
    public interface ICommonCompanyUtility
    {
        /// <summary>
        /// Anagrafica comune dell'azienda; con createIfMissing la crea (e in multi-tenant crea/migra lo schema Common).
        /// Senza createIfMissing restituisce null se manca.
        /// </summary>
        Task<Az_Anagrafica?> GetAz_AnagraficaAsync(int idCompany, bool createIfMissing);

        /// <summary>Anagrafica comune dell'azienda della richiesta (claim company), creata se manca.</summary>
        Task<Az_Anagrafica> CurrentAz_AnagraficaAsync(string? currentCompany);
    }

    public class CommonCompanyUtility : ICommonCompanyUtility
    {
        private readonly IAz_AnagraficaRepository _az_AnagraficaRepository;
        private readonly ITenantProvisioningService _tenantProvisioningService;

        // aziende con lo schema Common gia' verificato in questa istanza
        private static readonly ConcurrentDictionary<int, byte> _ensured = new();

        public CommonCompanyUtility(IAz_AnagraficaRepository az_AnagraficaRepository,
                                    ITenantProvisioningService tenantProvisioningService)
        {
            _az_AnagraficaRepository = az_AnagraficaRepository;
            _tenantProvisioningService = tenantProvisioningService;
        }

        public async Task<Az_Anagrafica?> GetAz_AnagraficaAsync(int idCompany, bool createIfMissing)
        {
            if (idCompany <= 0)
                throw new UserMessageException(InfrastructureMessages.Get("NoCompanySelected"));

            if (createIfMissing && !_ensured.ContainsKey(idCompany))
            {
                // azienda creata dopo l'avvio: lo schema Common potrebbe non esserci ancora
                await _tenantProvisioningService.EnsureApplicationAsync(idCompany, ApplicationType.Common);
                _ensured.TryAdd(idCompany, 0);
            }

            var az_Anagrafica = await _az_AnagraficaRepository.FindAll(x => x.IdCompany == idCompany).FirstOrDefaultAsync();
            if (az_Anagrafica == null && createIfMissing)
                az_Anagrafica = await _az_AnagraficaRepository.UpsertAsync(new Az_Anagrafica { IdCompany = idCompany });

            return az_Anagrafica;
        }

        public async Task<Az_Anagrafica> CurrentAz_AnagraficaAsync(string? currentCompany)
        {
            if (!int.TryParse(currentCompany, out int idCompany) || idCompany <= 0)
                throw new UserMessageException(InfrastructureMessages.Get("NoCompanySelected"));

            return (await GetAz_AnagraficaAsync(idCompany, true))!;
        }
    }
}
