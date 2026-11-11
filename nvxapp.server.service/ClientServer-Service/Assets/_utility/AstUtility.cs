using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Tenant.Assets;
using nvxapp.server.service.ClientServer_Service.Assets.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using System.Security.Claims;

namespace nvxapp.server.service.ClientServer_Service.Assets._utility
{
    /*
     Chi sta lavorando in Risorse aziendali (azienda e ruoli), ANALISI_ASSETS_APPLICATIVO.md:
        - CompanyPowerAdmin: configurazione del modulo e delle risorse prenotabili; tutto il resto
        - CompanyAdmin     : veicoli e beni, assegnazioni, consegne e restituzioni, scadenze, multe e costi; vede tutte le prenotazioni
        - User             : i beni e il veicolo assegnati a lui, le proprie multe; prenota e gestisce le proprie prenotazioni
     Le regole valgono lato server per ogni servizio (le pagine le rispecchiano soltanto).
    */
    public sealed class AstContesto
    {
        public int IdCompany { get; init; }
        public int IdAz_Anagrafica { get; init; }
        public string IdUtente { get; init; } = string.Empty;
        public bool PowerAdmin { get; init; }
        public bool Admin { get; init; }
        public bool User { get; init; }

        public bool Amministratore => PowerAdmin || Admin;
    }

    public interface IAstUtility
    {
        /// <summary>Contesto della richiesta; UserMessageException se non c'e' un'azienda o l'utente non ha un ruolo dell'azienda.</summary>
        Task<AstContesto> ContestoAsync();

        /// <summary>Configurazione predefinita dell'azienda (se manca).</summary>
        Task EnsureDefaultsAsync(int idAz_Anagrafica);

        Task<Ast_Cfg> CfgAsync(AstContesto ctx);

        /// <summary>UserMessageException se l'utente non e' amministratore dell'azienda.</summary>
        void RichiediAmministratore(AstContesto ctx);

        /// <summary>Dipendente (Common) collegato all'utente nell'azienda, se c'e'.</summary>
        Task<int?> MioDipendenteAsync(AstContesto ctx);

        /// <summary>Dipendenti dell'azienda (Common) per le scelte nelle assegnazioni.</summary>
        Task<List<Ast_DipendenteModel>> DipendentiAsync(AstContesto ctx);

        /// <summary>Sedi dell'azienda (Common).</summary>
        Task<List<Ast_LookupModel>> SediAsync(AstContesto ctx);

        /// <summary>Notifica del modulo (categoria "generale"): non parte se le notifiche del modulo sono disattivate.</summary>
        Task<bool> NotificaAsync(AstContesto ctx, IEnumerable<string> userIds, string titolo, string? messaggio, string? url, object? urlState = null);
    }

    public class AstUtility : IAstUtility
    {
        private readonly IAst_CfgRepository _Ast_Cfg;
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAst_Dip_AnagraficaReadRepository _Dip_Anagrafica;
        private readonly IAst_Az_SediReadRepository _Az_Sedi;
        private readonly INotificationSender _notificationSender;

        private AstContesto? _contesto;

        public AstUtility(IAst_CfgRepository ast_CfgRepository, ICommonCompanyUtility commonCompanyUtility,
                          IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager,
                          IAst_Dip_AnagraficaReadRepository dip_AnagraficaRepository, IAst_Az_SediReadRepository az_SediRepository,
                          INotificationSender notificationSender)
        {
            _Ast_Cfg = ast_CfgRepository;
            _commonCompanyUtility = commonCompanyUtility;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _Dip_Anagrafica = dip_AnagraficaRepository;
            _Az_Sedi = az_SediRepository;
            _notificationSender = notificationSender;
        }

        public async Task<AstContesto> ContestoAsync()
        {
            if (_contesto != null)
                return _contesto;

            var user = _httpContextAccessor.HttpContext?.User;
            var idUtente = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            if (!int.TryParse(user?.FindFirst(TenantSchemaAccessor.CompanyClaim)?.Value, out var idCompany) || idCompany <= 0)
                throw new UserMessageException(InfrastructureMessages.Get("NoCompanySelected"));

            var appUser = string.IsNullOrEmpty(idUtente) ? null : await _userManager.FindByIdAsync(idUtente);
            var ruoli = appUser == null ? new List<string>() : (await _userManager.GetRolesAsync(appUser)).ToList();
            bool powerAdmin = ruoli.Contains(nameof(RoleCode.CompanyPowerAdmin));
            bool admin = ruoli.Contains(nameof(RoleCode.CompanyAdmin));
            bool utente = ruoli.Contains(nameof(RoleCode.User));
            if (!powerAdmin && !admin && !utente)
                throw new UserMessageException(AssetsMessages.Get("NotAllowed"));

            var az = await _commonCompanyUtility.GetAz_AnagraficaAsync(idCompany, true);
            await EnsureDefaultsAsync(az!.Id);

            _contesto = new AstContesto
            {
                IdCompany = idCompany,
                IdAz_Anagrafica = az.Id,
                IdUtente = idUtente,
                PowerAdmin = powerAdmin,
                Admin = admin,
                User = utente,
            };
            return _contesto;
        }

        public async Task EnsureDefaultsAsync(int idAz_Anagrafica)
        {
            if (await _Ast_Cfg.FindAll(_ => true).AnyAsync(x => x.IdAz_Anagrafica == idAz_Anagrafica))
                return;
            await _Ast_Cfg.CreateAsync(new Ast_Cfg { IdAz_Anagrafica = idAz_Anagrafica, CreationDate = DateTime.Now });
        }

        public Task<Ast_Cfg> CfgAsync(AstContesto ctx) => _Ast_Cfg.FindAll(_ => true).FirstAsync(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica);

        public void RichiediAmministratore(AstContesto ctx)
        {
            if (!ctx.Amministratore)
                throw new UserMessageException(AssetsMessages.Get("NotAllowed"));
        }

        public async Task<int?> MioDipendenteAsync(AstContesto ctx)
        {
            if (string.IsNullOrEmpty(ctx.IdUtente))
                return null;
            var id = await _Dip_Anagrafica.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.IdAspNetUsers == ctx.IdUtente)
                                          .Select(x => x.Id).FirstOrDefaultAsync();
            return id == 0 ? null : id;
        }

        public async Task<List<Ast_DipendenteModel>> DipendentiAsync(AstContesto ctx)
            => (await _Dip_Anagrafica.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                     .Select(x => new { x.Id, x.Cognome, x.Nome, x.IdAspNetUsers, x.Attivo, x.DataCessazione })
                                     .ToListAsync())
               .Select(x => new Ast_DipendenteModel
               {
                   Id = x.Id,
                   Nominativo = Nominativo(x.Cognome, x.Nome),
                   HaUtente = x.IdAspNetUsers != null,
                   Attivo = x.Attivo,
                   DataCessazione = x.DataCessazione,
               })
               .OrderBy(x => x.Nominativo)
               .ToList();

        public async Task<List<Ast_LookupModel>> SediAsync(AstContesto ctx)
            => await _Az_Sedi.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                             .OrderBy(x => x.Descrizione)
                             .Select(x => new Ast_LookupModel { Id = x.Id, Descrizione = x.Descrizione ?? string.Empty })
                             .ToListAsync();

        public async Task<bool> NotificaAsync(AstContesto ctx, IEnumerable<string> userIds, string titolo, string? messaggio, string? url, object? urlState = null)
        {
            var utenti = userIds.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            if (utenti.Count == 0 || !(await CfgAsync(ctx)).NotificheAttive)
                return false;
            await _notificationSender.SendAsync(new NotificationMessage
            {
                UserIds = utenti,
                IdCompany = ctx.IdCompany,
                Categoria = NotificationCategory.Generale.Code,
                Titolo = titolo,
                Messaggio = messaggio,
                Url = url,
                UrlState = urlState,
            });
            return true;
        }

        public static string Nominativo(string? cognome, string? nome) => ((cognome ?? string.Empty) + " " + (nome ?? string.Empty)).Trim();

        public static string? Testo(string? s, int max)
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;
            s = s.Trim();
            return s.Length > max ? s[..max] : s;
        }
    }
}
