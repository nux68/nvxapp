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
     Gestione dello spazio a disposizione (ANALISI_DOCUMENTALE.md §5.3).
        - pacchetti e valori predefiniti (dimensione massima del file, soglia di avviso): solo SuperUser
        - spazio delle aziende: SuperUser (tutte), centro (sue aziende; cambia pacchetto e limiti solo il DealerPowerAdmin),
          studio (sue aziende, sola lettura), azienda (solo la propria, sola lettura)
     Ruoli e nodi sono quelli del token, mai dal client.
    */
    public class StorageService : ServiceBase, IStorageService
    {
        private const long MB = 1024L * 1024L;

        private readonly PublicDbContext _db;
        private readonly IStorageQuotaService _storageQuotaService;
        private readonly IApplicationActivationAuthorizer _authorizer;

        public StorageService(IMapper mapper,
                              UserManager<ApplicationUser> userManager,
                              IAspNetUsersRepository aspNetUsersRepository,
                              IOptions<JwtParameter> jwtParameter,
                              IHttpContextAccessor httpContextAccessor,
                              IConfiguration configuration,
                              PublicDbContext db,
                              IStorageQuotaService storageQuotaService,
                              IApplicationActivationAuthorizer authorizer) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _db = db;
            _storageQuotaService = storageQuotaService;
            _authorizer = authorizer;
        }

        // ---------------------------------------------------------------------------
        // Pacchetti e valori predefiniti (SuperUser)

        public virtual async Task<GenericResult<StorageSettingsGetOutModel>> StorageSettingsGet(GenericRequest<StorageSettingsGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloSuperUser();
                return new StorageSettingsGetOutModel
                {
                    Settings = await SettingsAsync(),
                    Plans = await PlansAsync(onlyActive: false),
                    TotalUsedBytes = await _db.CompanyStorage.SumAsync(x => (long?)x.UsedBytes) ?? 0
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<StorageSettingsPutOutModel>> StorageSettingsPut(GenericRequest<StorageSettingsPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloSuperUser();
                var s = model.Data.Settings;
                if (s.MaxFileMB <= 0 || s.WarningPercent <= 0 || s.WarningPercent >= 100)
                    throw new UserMessageException(InfrastructureMessages.Get("StorageSettingsInvalid"));

                await SetSettingAsync(PublicDbContext.Key_StorageMaxFileMB, s.MaxFileMB);
                await SetSettingAsync(PublicDbContext.Key_StorageWarningPercent, s.WarningPercent);
                return new StorageSettingsPutOutModel { Settings = await SettingsAsync() };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<StoragePlanPutOutModel>> StoragePlanPut(GenericRequest<StoragePlanPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloSuperUser();
                var dati = model.Data.Plan;
                if (string.IsNullOrWhiteSpace(dati.Descrizione) || dati.QuotaMB <= 0 || (dati.MaxFileMB.HasValue && dati.MaxFileMB <= 0))
                    throw new UserMessageException(InfrastructureMessages.Get("StoragePlanInvalid"));

                var descrizione = dati.Descrizione.Trim();
                if (await _db.StoragePlan.AnyAsync(x => x.Descrizione == descrizione && x.Id != dati.Id))
                    throw new UserMessageException(InfrastructureMessages.Get("StoragePlanDuplicate", descrizione));

                StoragePlan plan;
                if (dati.Id > 0)
                {
                    plan = await _db.StoragePlan.AsTracking().FirstOrDefaultAsync(x => x.Id == dati.Id)
                           ?? throw new UserMessageException(InfrastructureMessages.Get("StoragePlanNotFound"));
                }
                else
                {
                    plan = new StoragePlan { CreationDate = DateTime.Now };
                    _db.StoragePlan.Add(plan);
                }

                // il predefinito resta sempre attivo; un pacchetto disattivato non puo' essere il predefinito
                if (plan.Predefinito && (!dati.Predefinito || !dati.Active))
                    throw new UserMessageException(InfrastructureMessages.Get("StoragePlanDefaultRequired"));

                plan.Descrizione = descrizione;
                plan.QuotaMB = dati.QuotaMB;
                plan.MaxFileMB = dati.MaxFileMB;
                plan.Ordine = dati.Ordine;
                plan.Active = dati.Active;
                plan.ModifiedDate = DateTime.Now;
                plan.ChangeUser = CurrentUserId;

                await using var transaction = await _db.Database.BeginTransactionAsync();
                if (dati.Predefinito && !plan.Predefinito)
                {
                    await _db.StoragePlan.Where(x => x.Predefinito).ExecuteUpdateAsync(u => u.SetProperty(x => x.Predefinito, false));
                    plan.Predefinito = true;
                }
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return new StoragePlanPutOutModel { Plan = (await PlansAsync(onlyActive: false)).First(x => x.Id == plan.Id) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<StoragePlanDeleteOutModel>> StoragePlanDelete(GenericRequest<StoragePlanDeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await SoloSuperUser();
                var plan = await _db.StoragePlan.AsTracking().FirstOrDefaultAsync(x => x.Id == model.Data.Id)
                           ?? throw new UserMessageException(InfrastructureMessages.Get("StoragePlanNotFound"));
                if (plan.Predefinito)
                    throw new UserMessageException(InfrastructureMessages.Get("StoragePlanDefaultRequired"));

                var retVal = new StoragePlanDeleteOutModel();
                if (await _db.CompanyStorage.AnyAsync(x => x.IdStoragePlan == plan.Id))
                {
                    // usato da aziende: si disattiva (non piu' assegnabile)
                    plan.Active = false;
                    plan.ModifiedDate = DateTime.Now;
                    retVal.Deleted = false;
                }
                else
                {
                    _db.StoragePlan.Remove(plan);
                    retVal.Deleted = true;
                }
                await _db.SaveChangesAsync();
                return retVal;
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

                var retVal = new CompanyStorageListOutModel { Plans = await PlansAsync(onlyActive: true) };
                foreach (var idCompany in aziende)
                    retVal.CompanyStorage.Add(await ModelAsync(idCompany));
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
                if ((dati.MaxFileMB.HasValue && dati.MaxFileMB <= 0) || (dati.WarningPercent.HasValue && (dati.WarningPercent <= 0 || dati.WarningPercent >= 100)))
                    throw new UserMessageException(InfrastructureMessages.Get("StorageSettingsInvalid"));

                var plan = await _db.StoragePlan.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dati.IdStoragePlan)
                           ?? throw new UserMessageException(InfrastructureMessages.Get("StoragePlanNotFound"));

                await _storageQuotaService.EnsureAsync(dati.IdCompany);
                var row = await _db.CompanyStorage.AsTracking().FirstAsync(x => x.IdCompany == dati.IdCompany);

                // un pacchetto disattivato non si assegna (resta solo a chi lo ha gia')
                if (!plan.Active && row.IdStoragePlan != plan.Id)
                    throw new UserMessageException(InfrastructureMessages.Get("StoragePlanInactive", plan.Descrizione));

                row.IdStoragePlan = plan.Id;
                row.MaxFileMB = dati.MaxFileMB;
                row.WarningPercent = dati.WarningPercent;
                row.ModifiedDate = DateTime.Now;
                row.ChangeUser = CurrentUserId;
                await _db.SaveChangesAsync();

                // un pacchetto piu' piccolo dello spazio gia' usato e' consentito: l'azienda risulta oltre quota
                var retVal = new CompanyStoragePutOutModel { CompanyStorage = await ModelAsync(dati.IdCompany) };
                if (retVal.CompanyStorage.OverQuota)
                    retVal.Messages.Add(new Message(InfrastructureMessages.Get("StoragePlanBelowUsed"), MessageType.Warning));
                return retVal;
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------

        // aziende di cui l'utente vede lo spazio; null = nessuna (ruolo non ammesso)
        private async Task<List<int>?> AziendeVisibili()
        {
            if (await _authorizer.IsSuperUserAsync())
                return await _db.Company.Select(x => x.Id).ToListAsync();

            if ((await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.DealerPowerAdmin)) || await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.DealerAdmin)))
                && int.TryParse(CurrentDealer, out var idDealer))
                return await _db.Company.Where(x => x.FinancialAdvisorNavigation!.IdDealer == idDealer).Select(x => x.Id).ToListAsync();

            if ((await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.FinancialAdvisorPowerAdmin)) || await _authorizer.CurrentUserHasRoleAsync(nameof(RoleCode.FinancialAdvisorAdmin)))
                && int.TryParse(CurrentFinancialAdvisor, out var idStudio))
                return await _db.Company.Where(x => x.IdFinancialAdvisor == idStudio).Select(x => x.Id).ToListAsync();

            return null;
        }

        private async Task<CompanyStorageModel> ModelAsync(int idCompany)
        {
            var status = await _storageQuotaService.StatusAsync(idCompany);
            var row = await _db.CompanyStorage.AsNoTracking().FirstAsync(x => x.IdCompany == idCompany);
            var company = await _db.Company.AsNoTracking().Include(x => x.FinancialAdvisorNavigation).FirstAsync(x => x.Id == idCompany);
            return new CompanyStorageModel
            {
                IdCompany = idCompany,
                Azienda = company.Descrizione ?? string.Empty,
                Studio = company.FinancialAdvisorNavigation?.Descrizione ?? string.Empty,
                IdStoragePlan = status.IdStoragePlan,
                Pacchetto = status.Pacchetto,
                QuotaMB = (int)(status.QuotaBytes / MB),
                UsedBytes = status.UsedBytes,
                FileCount = status.FileCount,
                UsedPercent = status.UsedPercent,
                WarningPercent = status.WarningPercent,
                MaxFileMB = (int)(status.MaxFileBytes / MB),
                MaxFileMBAzienda = row.MaxFileMB,
                WarningPercentAzienda = row.WarningPercent,
                OverWarning = status.UsedPercent >= status.WarningPercent,
                OverQuota = status.UsedBytes >= status.QuotaBytes,
                CanManage = await _authorizer.CanManageCompanyAsync(idCompany)
            };
        }

        private async Task<List<StoragePlanModel>> PlansAsync(bool onlyActive)
        {
            var usati = await _db.CompanyStorage.GroupBy(x => x.IdStoragePlan).Select(g => new { g.Key, Count = g.Count() }).ToListAsync();
            return (await _db.StoragePlan.AsNoTracking().Where(x => !onlyActive || x.Active).OrderBy(x => x.Ordine).ThenBy(x => x.QuotaMB).ToListAsync())
                   .Select(x => new StoragePlanModel
                   {
                       Id = x.Id,
                       Descrizione = x.Descrizione,
                       QuotaMB = x.QuotaMB,
                       MaxFileMB = x.MaxFileMB,
                       Ordine = x.Ordine,
                       Active = x.Active,
                       Predefinito = x.Predefinito,
                       Aziende = usati.FirstOrDefault(u => u.Key == x.Id)?.Count ?? 0
                   })
                   .ToList();
        }

        private async Task<StorageSettingsModel> SettingsAsync()
        {
            async Task<int> Valore(string key, int def)
            {
                var v = await _db.AppSetting.AsNoTracking().Where(x => x.Key == key).Select(x => x.Value).FirstOrDefaultAsync();
                return int.TryParse(v, out var n) && n > 0 ? n : def;
            }
            return new StorageSettingsModel
            {
                MaxFileMB = await Valore(PublicDbContext.Key_StorageMaxFileMB, 50),
                WarningPercent = await Valore(PublicDbContext.Key_StorageWarningPercent, 80)
            };
        }

        private async Task SetSettingAsync(string key, int value)
        {
            var row = await _db.AppSetting.AsTracking().FirstOrDefaultAsync(x => x.Key == key);
            if (row == null)
                _db.AppSetting.Add(new AppSetting { Key = key, Value = value.ToString(), ModifiedDate = DateTime.Now });
            else
            {
                row.Value = value.ToString();
                row.ModifiedDate = DateTime.Now;
            }
            await _db.SaveChangesAsync();
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
        Task<GenericResult<StoragePlanPutOutModel>> StoragePlanPut(GenericRequest<StoragePlanPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<StoragePlanDeleteOutModel>> StoragePlanDelete(GenericRequest<StoragePlanDeleteInModel> model, Boolean isSubProcess);
        Task<GenericResult<CompanyStorageListOutModel>> CompanyStorageList(GenericRequest<CompanyStorageListInModel> model, Boolean isSubProcess);
        Task<GenericResult<CompanyStorageGetOutModel>> CompanyStorageGet(GenericRequest<CompanyStorageGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<CompanyStoragePutOutModel>> CompanyStoragePut(GenericRequest<CompanyStoragePutInModel> model, Boolean isSubProcess);
    }
}
