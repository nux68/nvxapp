using System.Text.Json;
using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.UserDashboard.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.UserDashboard
{
    /*
     Dashboard dell'utente corrente nell'azienda corrente (ANALISI_INFRASTRUCTURE_WIDGET.md).
     Il catalogo dei widget lo dichiara il client di ogni ramo: il server salva solo le chiavi scelte, nell'ordine,
     dopo il controllo del formato. I dati mostrati dai widget arrivano dai servizi di ogni applicativo, che
     ricontrollano i permessi come per le loro pagine.
     Configurano la propria dashboard gli utenti con ruolo User, CompanyAdmin o CompanyPowerAdmin in un'azienda;
     gli altri vedono i widget predefiniti.
    */
    public class UserDashboardService : ServiceBase, IUserDashboardService
    {
        public static readonly string[] RuoliConfigurabili = { "User", "CompanyAdmin", "CompanyPowerAdmin" };
        public const int MaxWidget = 50;
        private static readonly Regex FormatoChiave = new Regex(@"^[A-Za-z0-9][A-Za-z0-9._-]{0,99}$", RegexOptions.Compiled);

        private readonly IUserDashboardRepository _userDashboardRepository;

        public UserDashboardService(IMapper mapper,
                                    UserManager<ApplicationUser> userManager,
                                    IAspNetUsersRepository aspNetUsersRepository,
                                    IOptions<JwtParameter> jwtParameter,
                                    IHttpContextAccessor httpContextAccessor,
                                    IConfiguration configuration,

                                    IUserDashboardRepository userDashboardRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _userDashboardRepository = userDashboardRepository;
        }

        public virtual async Task<GenericResult<UserDashboardGetOutModel>> UserDashboardGet(GenericRequest<UserDashboardGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new UserDashboardGetOutModel();

                int? idCompany = await AziendaConfigurabile();
                retVal.Configurabile = idCompany != null;
                if (idCompany != null)
                {
                    var riga = _userDashboardRepository.Dashboard(this.CurrentUserId, idCompany.Value);
                    retVal.Configurata = riga != null;
                    retVal.Widgets = riga != null ? Leggi(riga.Widgets) : new List<string>();
                }

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<UserDashboardPutOutModel>> UserDashboardPut(GenericRequest<UserDashboardPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new UserDashboardPutOutModel();

                int idCompany = await AziendaConfigurabile()
                                ?? throw new UserMessageException(InfrastructureMessages.Get("DashboardNotConfigurable"));

                // chiavi richieste: formato valido, senza doppioni, nell'ordine ricevuto
                var widgets = (model.Data.Widgets ?? new List<string>())
                    .Select(x => (x ?? string.Empty).Trim())
                    .Distinct()
                    .ToList();
                if (widgets.Count > MaxWidget)
                    throw new UserMessageException(InfrastructureMessages.Get("DashboardTooManyWidgets", MaxWidget));
                foreach (var w in widgets)
                    if (!FormatoChiave.IsMatch(w))
                        throw new UserMessageException(InfrastructureMessages.Get("DashboardWidgetInvalid", w));

                var json = JsonSerializer.Serialize(widgets);
                var riga = _userDashboardRepository.Dashboard(this.CurrentUserId, idCompany);
                if (riga == null)
                    await _userDashboardRepository.CreateAsync(new data.Entities.Public.UserDashboard
                    {
                        IdAspNetUsers = this.CurrentUserId,
                        IdCompany = idCompany,
                        Widgets = json
                    });
                else
                {
                    riga.Widgets = json;
                    await _userDashboardRepository.UpdateAsync(riga);
                }

                retVal.Configurabile = true;
                retVal.Configurata = true;
                retVal.Widgets = widgets;

                return retVal;
            }, isSubProcess);
        }

        // azienda corrente, se l'utente vi lavora con un ruolo che configura la dashboard; altrimenti null
        private async Task<int?> AziendaConfigurabile()
        {
            if (!int.TryParse(this.CurrentCompany, out int idCompany) || idCompany <= 0)
                return null;

            var user = await _userManager.FindByIdAsync(this.CurrentUserId);
            if (user == null)
                return null;

            var ruoli = await _userManager.GetRolesAsync(user);
            return ruoli.Any(r => RuoliConfigurabili.Contains(r)) ? idCompany : null;
        }

        private static List<string> Leggi(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<string>();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch (JsonException)
            {
                return new List<string>();
            }
        }
    }

    public interface IUserDashboardService : IServiceBase
    {
        Task<GenericResult<UserDashboardGetOutModel>> UserDashboardGet(GenericRequest<UserDashboardGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<UserDashboardPutOutModel>> UserDashboardPut(GenericRequest<UserDashboardPutInModel> model, Boolean isSubProcess);
    }
}
