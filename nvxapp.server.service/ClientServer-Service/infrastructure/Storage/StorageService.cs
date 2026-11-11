using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyApplication;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Storage
{
    /*
     Gestione dello spazio a disposizione (ANALISI_DOCUMENTMANAGEMENT_APPLICATIVO.md [branch DocumentManagement] §5.3).
        - valore predefinito della soglia di avviso: solo SuperUser
        - spazio delle aziende: SuperUser (tutte), centro (sue aziende; cambia la soglia solo il DealerPowerAdmin),
          studio (sue aziende, sola lettura), azienda (solo la propria, sola lettura)
     La quota e' quella della licenza Documentale (ANALISI_INFRASTRUCTURE_LICENZE.md §5.1): i pacchetti di spazio sono stati eliminati.
     Ruoli e nodi sono quelli del token, mai dal client.
    */
    public class StorageService : ServiceBase, IStorageService
    {
        private readonly ICompanyStorageRepository _companyStorageRepository;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly IStorageQuotaService _storageQuotaService;
        private readonly IApplicationActivationAuthorizer _authorizer;

        public StorageService(IMapper mapper,
                              UserManager<ApplicationUser> userManager,
                              IAspNetUsersRepository aspNetUsersRepository,
                              IOptions<JwtParameter> jwtParameter,
                              IHttpContextAccessor httpContextAccessor,
                              IConfiguration configuration,
                              ICompanyStorageRepository companyStorageRepository,
                              IAppSettingRepository appSettingRepository,
                              ICompanyRepository companyRepository,
                              ICompanyApplicationRepository companyApplicationRepository,
                              IStorageQuotaService storageQuotaService,
                              IApplicationActivationAuthorizer authorizer) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _companyStorageRepository = companyStorageRepository;
            _appSettingRepository = appSettingRepository;
            _companyRepository = companyRepository;
            _companyApplicationRepository = companyApplicationRepository;
            _storageQuotaService = storageQuotaService;
            _authorizer = authorizer;
        }

        // ---------------------------------------------------------------------------
        // Valori predefiniti (SuperUser)

        public virtual async Task<GenericResult<StorageSettingsGetOutModel>> StorageSettingsGet(GenericRequest<StorageSettingsGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloSuperUser();
                return new StorageSettingsGetOutModel
                {
                    Settings = await SettingsAsync(),
                    TotalUsedBytes = await _companyStorageRepository.FindAll(x => true).SumAsync(x => (long?)x.UsedBytes) ?? 0
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<StorageSettingsPutOutModel>> StorageSettingsPut(GenericRequest<StorageSettingsPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloSuperUser();
                var s = model.Data.Settings;
                if (s.WarningPercent <= 0 || s.WarningPercent >= 100)
                    throw new UserMessageException(InfrastructureMessages.Get("StorageSettingsInvalid"));

                await SetSettingAsync(PublicDbContext.Key_StorageWarningPercent, s.WarningPercent);
                return new StorageSettingsPutOutModel { Settings = await SettingsAsync() };
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------
        // Spazio delle aziende

        public virtual async Task<GenericResult<CompanyStorageListOutModel>> CompanyStorageList(GenericRequest<CompanyStorageListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var aziende = await AziendeVisibili()
                              ?? throw new UserMessageException(InfrastructureMessages.Get("StorageNotAllowed"));

                // solo le aziende con il Documentale attivo: le altre non usano lo spazio dei documenti
                var conDocumentale = await _companyApplicationRepository
                    .FindAll(x => x.ApplicationType == ApplicationType.DocumentManagement && x.Active && aziende.Contains(x.IdCompany))
                    .Select(x => x.IdCompany)
                    .ToListAsync();
                aziende = aziende.Where(conDocumentale.Contains).ToList();

                // e, con il sistema licenze acceso, solo quelle coperte da una licenza Documentale valida
                // (propria, dello studio o cumulativa); con il sistema spento nessuna e' "senza licenza"
                var retVal = new CompanyStorageListOutModel();
                foreach (var idCompany in aziende)
                {
                    var riga = await ModelAsync(idCompany);
                    if (!riga.SenzaLicenza)
                        retVal.CompanyStorage.Add(riga);
                }
                retVal.CompanyStorage = retVal.CompanyStorage.OrderBy(x => x.Studio).ThenBy(x => x.Azienda).ToList();
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<CompanyStorageGetOutModel>> CompanyStorageGet(GenericRequest<CompanyStorageGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                int idCompany = model.Data.IdCompany;
                if (idCompany <= 0)
                {
                    // azienda corrente: la vede chiunque vi lavori
                    if (!int.TryParse(CurrentCompany, out idCompany) || idCompany <= 0)
                        throw new UserMessageException(InfrastructureMessages.Get("NoCompanySelected"));
                }
                else
                {
                    var aziende = await AziendeVisibili();
                    if (aziende == null || !aziende.Contains(idCompany))
                        throw new UserMessageException(InfrastructureMessages.Get("StorageNotAllowed"));
                }
                return new CompanyStorageGetOutModel { CompanyStorage = await ModelAsync(idCompany) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<CompanyStoragePutOutModel>> CompanyStoragePut(GenericRequest<CompanyStoragePutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var dati = model.Data;
                if (!await _authorizer.CanManageCompanyAsync(dati.IdCompany))
                    throw new UserMessageException(InfrastructureMessages.Get("StorageNotAllowed"));
                if (dati.WarningPercent.HasValue && (dati.WarningPercent <= 0 || dati.WarningPercent >= 100))
                    throw new UserMessageException(InfrastructureMessages.Get("StorageSettingsInvalid"));

                await _storageQuotaService.EnsureAsync(dati.IdCompany);
                var row = await _companyStorageRepository.FindAll(x => x.IdCompany == dati.IdCompany).FirstAsync();

                row.WarningPercent = dati.WarningPercent;
                await _companyStorageRepository.UpdateAsync(row);

                return new CompanyStoragePutOutModel { CompanyStorage = await ModelAsync(dati.IdCompany) };
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------

        // aziende di cui l'utente vede lo spazio; null = nessuna (ruolo non ammesso)
        private async Task<List<int>?> AziendeVisibili()
        {
            if (await _authorizer.IsSuperUserAsync())
                return await _companyRepository.FindAll(x => true).Select(x => x.Id).ToListAsync();

            if ((await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.DealerPowerAdmin)) || await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.DealerAdmin)))
                && int.TryParse(CurrentDealer, out var idDealer))
                return await _companyRepository.FindAll(x => x.FinancialAdvisorNavigation!.IdDealer == idDealer).Select(x => x.Id).ToListAsync();

            if ((await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.FinancialAdvisorPowerAdmin)) || await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.FinancialAdvisorAdmin)))
                && int.TryParse(CurrentFinancialAdvisor, out var idStudio))
                return await _companyRepository.FindAll(x => x.IdFinancialAdvisor == idStudio).Select(x => x.Id).ToListAsync();

            return null;
        }

        private async Task<CompanyStorageModel> ModelAsync(int idCompany)
        {
            var status = await _storageQuotaService.StatusAsync(idCompany);
            var row = await _companyStorageRepository.FindAll(x => x.IdCompany == idCompany).FirstAsync();
            var company = await _companyRepository.FindAll(x => x.Id == idCompany).Include(x => x.FinancialAdvisorNavigation).FirstAsync();
            return new CompanyStorageModel
            {
                IdCompany = idCompany,
                Azienda = company.Descrizione ?? string.Empty,
                Studio = company.FinancialAdvisorNavigation?.Descrizione ?? string.Empty,
                Pacchetto = status.Pacchetto,
                QuotaMB = status.QuotaMB,
                Illimitato = status.Illimitato,
                SenzaLicenza = status.SenzaLicenza,
                UsedBytes = status.UsedBytes,
                FileCount = status.FileCount,
                UsedPercent = status.UsedPercent,
                WarningPercent = status.WarningPercent,
                WarningPercentAzienda = row.WarningPercent,
                OverWarning = !status.SenzaLicenza && status.UsedPercent >= status.WarningPercent,
                OverQuota = !status.SenzaLicenza && status.UsedBytes >= status.QuotaBytes,
                CanManage = await _authorizer.CanManageCompanyAsync(idCompany)
            };
        }

        private async Task<StorageSettingsModel> SettingsAsync()
        {
            async Task<int> Valore(string key, int def)
            {
                var v = await _appSettingRepository.GetValueAsync(key);
                return int.TryParse(v, out var n) && n > 0 ? n : def;
            }
            return new StorageSettingsModel
            {
                WarningPercent = await Valore(PublicDbContext.Key_StorageWarningPercent, 80)
            };
        }

        private async Task SetSettingAsync(string key, int value)
        {
            await _appSettingRepository.SetValueAsync(key, value.ToString());
        }

        private async Task SoloSuperUser()
        {
            if (!await _authorizer.IsSuperUserAsync())
                throw new UserMessageException(InfrastructureMessages.Get("StorageNotAllowed"));
        }
    }

    public interface IStorageService : IServiceBase
    {
        Task<GenericResult<StorageSettingsGetOutModel>> StorageSettingsGet(GenericRequest<StorageSettingsGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<StorageSettingsPutOutModel>> StorageSettingsPut(GenericRequest<StorageSettingsPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<CompanyStorageListOutModel>> CompanyStorageList(GenericRequest<CompanyStorageListInModel> model, Boolean isSubProcess);
        Task<GenericResult<CompanyStorageGetOutModel>> CompanyStorageGet(GenericRequest<CompanyStorageGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<CompanyStoragePutOutModel>> CompanyStoragePut(GenericRequest<CompanyStoragePutInModel> model, Boolean isSubProcess);
    }
}
