using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Email;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Notification
{
    /*
     Pagina "Notifiche" e preferenze di invio dell'utente corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.4).
     Ogni utente vede solo le proprie notifiche (di tutte le aziende in cui lavora). Le preferenze mostrano le categorie
     delle funzioni base e degli applicativi attivi per l'azienda corrente.
    */
    public class NotificationService : ServiceBase, INotificationService
    {
        private const int Pagina = 50;

        private readonly INotificaRepository _notificaRepository;
        private readonly INotificaPreferenzaRepository _preferenzaRepository;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly INotificationCategories _categories;
        private readonly IEmailService _emailService;

        public NotificationService(IMapper mapper,
                                   UserManager<ApplicationUser> userManager,
                                   IAspNetUsersRepository aspNetUsersRepository,
                                   IOptions<JwtParameter> jwtParameter,
                                   IHttpContextAccessor httpContextAccessor,
                                   IConfiguration configuration,

                                   INotificaRepository notificaRepository,
                                   INotificaPreferenzaRepository preferenzaRepository,
                                   ICompanyApplicationRepository companyApplicationRepository,
                                   INotificationCategories categories,
                                   IEmailService emailService) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _notificaRepository = notificaRepository;
            _preferenzaRepository = preferenzaRepository;
            _companyApplicationRepository = companyApplicationRepository;
            _categories = categories;
            _emailService = emailService;
        }

        public virtual async Task<GenericResult<NotificationListOutModel>> NotificationList(GenericRequest<NotificationListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new NotificationListOutModel();
                var filtro = model.Data ?? new NotificationListInModel();
                var io = this.CurrentUserId;

                var query = _notificaRepository.FindAll(x => x.IdAspNetUsers == io);
                if (filtro.SoloNonLette)
                    query = query.Where(x => !x.Letta);
                var righe = await query.OrderByDescending(x => x.Id)
                                       .Skip(Math.Max(0, filtro.Pagina) * Pagina)
                                       .Take(Pagina + 1)
                                       .Select(x => new NotificationModel
                                       {
                                           Id = x.Id,
                                           Categoria = x.Categoria,
                                           Titolo = x.Titolo,
                                           Messaggio = x.Messaggio,
                                           Url = x.Url,
                                           UrlState = x.UrlState,
                                           Letta = x.Letta,
                                           Data = x.CreationDate
                                       })
                                       .ToListAsync();
                retVal.AltreDisponibili = righe.Count > Pagina;
                retVal.Notification = righe.Take(Pagina).ToList();
                retVal.NonLette = await NonLette();
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<NotificationCountOutModel>> NotificationCount(GenericRequest<NotificationCountInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () => new NotificationCountOutModel { NonLette = await NonLette() }, isSubProcess);
        }

        public virtual async Task<GenericResult<NotificationReadOutModel>> NotificationRead(GenericRequest<NotificationReadInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var io = this.CurrentUserId;
                var ids = model.Data?.Id ?? new List<int>();
                bool tutte = model.Data?.Tutte ?? false;
                var ora = DateTime.Now;
                await _notificaRepository.FindAll(x => x.IdAspNetUsers == io && !x.Letta && (tutte || ids.Contains(x.Id)))
                                         .ExecuteUpdateAsync(u => u.SetProperty(x => x.Letta, true).SetProperty(x => x.DataLettura, ora));
                return new NotificationReadOutModel { NonLette = await NonLette() };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<NotificationDeleteOutModel>> NotificationDelete(GenericRequest<NotificationDeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var io = this.CurrentUserId;
                var ids = model.Data?.Id ?? new List<int>();
                bool lette = model.Data?.Lette ?? false;
                await _notificaRepository.FindAll(x => x.IdAspNetUsers == io && ((lette && x.Letta) || ids.Contains(x.Id)))
                                         .ExecuteDeleteAsync();
                return new NotificationDeleteOutModel { NonLette = await NonLette() };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<NotificationPreferenceGetOutModel>> NotificationPreferenceGet(GenericRequest<NotificationPreferenceGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () => new NotificationPreferenceGetOutModel
            {
                Preference = await Preferenze(),
                EmailConfigured = _emailService.Configured
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<NotificationPreferencePutOutModel>> NotificationPreferencePut(GenericRequest<NotificationPreferencePutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var io = this.CurrentUserId;
                var categorie = await CategorieVisibili();
                var esistenti = await _preferenzaRepository.FindAll(x => x.IdAspNetUsers == io).ToListAsync();

                foreach (var p in model.Data?.Preference ?? new List<NotificationPreferenceModel>())
                {
                    var categoria = categorie.FirstOrDefault(c => c.Code == p.Categoria);
                    if (categoria == null)
                        continue;
                    var riga = esistenti.FirstOrDefault(x => x.Categoria == categoria.Code);
                    bool email = categoria.EmailAllowed && p.Email;
                    if (riga == null)
                        await _preferenzaRepository.CreateAsync(new NotificaPreferenza { IdAspNetUsers = io, Categoria = categoria.Code, App = p.App, Email = email });
                    else if (riga.App != p.App || riga.Email != email)
                    {
                        riga.App = p.App;
                        riga.Email = email;
                        await _preferenzaRepository.UpdateAsync(riga);
                    }
                }

                return new NotificationPreferencePutOutModel { Preference = await Preferenze(), EmailConfigured = _emailService.Configured };
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------

        private Task<int> NonLette()
        {
            var io = this.CurrentUserId;
            return _notificaRepository.FindAll(x => x.IdAspNetUsers == io && !x.Letta).CountAsync();
        }

        // categorie delle funzioni base e degli applicativi attivi per l'azienda corrente
        private async Task<List<NotificationCategory>> CategorieVisibili()
        {
            var attivi = new List<ApplicationType> { ApplicationType.Common };
            if (int.TryParse(this.CurrentCompany, out int idCompany) && idCompany > 0)
                attivi.AddRange(await _companyApplicationRepository.FindAll(x => x.IdCompany == idCompany && x.Active).Select(x => x.ApplicationType).ToListAsync());
            return _categories.All.Where(c => attivi.Contains(c.Application)).ToList();
        }

        private async Task<List<NotificationPreferenceModel>> Preferenze()
        {
            var io = this.CurrentUserId;
            var salvate = await _preferenzaRepository.FindAll(x => x.IdAspNetUsers == io).ToDictionaryAsync(x => x.Categoria);
            return (await CategorieVisibili()).Select(c =>
            {
                salvate.TryGetValue(c.Code, out var p);
                return new NotificationPreferenceModel
                {
                    Categoria = c.Code,
                    Titolo = c.Titolo,
                    App = p?.App ?? c.DefaultApp,
                    Email = c.EmailAllowed && (p?.Email ?? c.DefaultEmail),
                    EmailAllowed = c.EmailAllowed
                };
            }).ToList();
        }
    }

    public interface INotificationService : IServiceBase
    {
        Task<GenericResult<NotificationListOutModel>> NotificationList(GenericRequest<NotificationListInModel> model, Boolean isSubProcess);
        Task<GenericResult<NotificationCountOutModel>> NotificationCount(GenericRequest<NotificationCountInModel> model, Boolean isSubProcess);
        Task<GenericResult<NotificationReadOutModel>> NotificationRead(GenericRequest<NotificationReadInModel> model, Boolean isSubProcess);
        Task<GenericResult<NotificationDeleteOutModel>> NotificationDelete(GenericRequest<NotificationDeleteInModel> model, Boolean isSubProcess);
        Task<GenericResult<NotificationPreferenceGetOutModel>> NotificationPreferenceGet(GenericRequest<NotificationPreferenceGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<NotificationPreferencePutOutModel>> NotificationPreferencePut(GenericRequest<NotificationPreferencePutInModel> model, Boolean isSubProcess);
    }
}
