using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Crm;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Crm.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;
using nvxapp.server.service.ServerModels;
using System.Security.Claims;

namespace nvxapp.server.service.ClientServer_Service.Crm._utility
{
    /*
     Chi sta lavorando nel CRM (azienda e ruoli) e cosa vede (ANALISI_CRM.md §4):
        - CompanyPowerAdmin: configurazione e tutto il resto
        - CompanyAdmin     : vede tutto, assegna i responsabili, report
        - User (commerciale): con la visibilita' "solo i propri" (predefinita) vede i clienti di cui e' responsabile,
          quelli senza responsabile ("condivisi") e quelli in cui e' coinvolto (opportunita' o attivita' sue);
          con "tutti vedono tutto" vede tutto. Lavora sui dati che vede.
     Le regole valgono lato server per ogni servizio (le pagine le rispecchiano soltanto).
    */
    public sealed class CrmContesto
    {
        public int IdCompany { get; init; }
        public int IdAz_Anagrafica { get; init; }
        public string IdUtente { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public bool PowerAdmin { get; init; }
        public bool Admin { get; init; }
        public bool User { get; init; }
        public bool VedeTutto { get; init; }

        public bool Amministratore => PowerAdmin || Admin;
    }

    public interface ICrmUtility
    {
        /// <summary>Contesto della richiesta; UserMessageException se non c'e' un'azienda o l'utente non ha un ruolo dell'azienda.</summary>
        Task<CrmContesto> ContestoAsync();

        /// <summary>Configurazione e tabelle predefinite dell'azienda (se mancano).</summary>
        Task EnsureDefaultsAsync(int idAz_Anagrafica);

        /// <summary>Clienti dell'azienda visibili all'utente.</summary>
        IQueryable<Az_Cliente> ClientiVisibili(CrmContesto ctx);
        IQueryable<Crm_Opportunita> OpportunitaVisibili(CrmContesto ctx);
        IQueryable<Crm_Attivita> AttivitaVisibili(CrmContesto ctx);
        IQueryable<Crm_Lead> LeadVisibili(CrmContesto ctx);
        IQueryable<Crm_Offerta> OfferteVisibili(CrmContesto ctx);

        /// <summary>Cliente visibile o UserMessageException.</summary>
        Task<Az_Cliente> ClienteAsync(CrmContesto ctx, int idAz_Cliente);

        /// <summary>Utenti dell'azienda proponibili come responsabili e assegnatari.</summary>
        Task<List<Crm_UtenteModel>> UtentiAsync(CrmContesto ctx);

        /// <summary>Nomi utente per id (per gli elenchi).</summary>
        Task<Dictionary<string, string>> NomiAsync(IEnumerable<string?> ids);

        Task<Crm_Cfg> CfgAsync(CrmContesto ctx);
    }

    public class CrmUtility : ICrmUtility
    {
        private static readonly string[] RuoliAzienda = { nameof(RoleCode.CompanyPowerAdmin), nameof(RoleCode.CompanyAdmin), nameof(RoleCode.User) };

        private readonly ICrm_CfgRepository _Crm_Cfg;
        private readonly ICrm_Par_FaseRepository _Crm_Par_Fase;
        private readonly ICrm_Par_TipoAttivitaRepository _Crm_Par_TipoAttivita;
        private readonly ICrm_Par_MotivoPerditaRepository _Crm_Par_MotivoPerdita;
        private readonly ICrm_Par_FonteRepository _Crm_Par_Fonte;
        private readonly ICrm_Az_ClienteReadRepository _Az_Cliente;
        private readonly ICrm_AttivitaRepository _Crm_Attivita;
        private readonly ICrm_ClienteDatiRepository _Crm_ClienteDati;
        private readonly ICrm_LeadRepository _Crm_Lead;
        private readonly ICrm_OffertaRepository _Crm_Offerta;
        private readonly ICrm_OpportunitaRepository _Crm_Opportunita;
        private readonly IAspNetRolesRepository _AspNetRoles;
        private readonly IAspNetUsersRepository _AspNetUsers;
        private readonly IUserCompanyRepository _UserCompany;
        private readonly IAspNetUserRolesRepository _AspNetUserRoles;
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        private CrmContesto? _contesto;

        public CrmUtility(ICrm_CfgRepository crm_CfgRepository, ICrm_Par_FaseRepository crm_Par_FaseRepository, ICrm_Par_TipoAttivitaRepository crm_Par_TipoAttivitaRepository, ICrm_Par_MotivoPerditaRepository crm_Par_MotivoPerditaRepository, ICrm_Par_FonteRepository crm_Par_FonteRepository, ICrm_Az_ClienteReadRepository az_ClienteRepository, ICrm_AttivitaRepository crm_AttivitaRepository, ICrm_ClienteDatiRepository crm_ClienteDatiRepository, ICrm_LeadRepository crm_LeadRepository, ICrm_OffertaRepository crm_OffertaRepository, ICrm_OpportunitaRepository crm_OpportunitaRepository, IAspNetRolesRepository aspNetRolesRepository, IAspNetUsersRepository aspNetUsersRepository, IUserCompanyRepository userCompanyRepository, IAspNetUserRolesRepository aspNetUserRolesRepository,
                          ICommonCompanyUtility commonCompanyUtility,
                          IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _Crm_Cfg = crm_CfgRepository;
            _Crm_Par_Fase = crm_Par_FaseRepository;
            _Crm_Par_TipoAttivita = crm_Par_TipoAttivitaRepository;
            _Crm_Par_MotivoPerdita = crm_Par_MotivoPerditaRepository;
            _Crm_Par_Fonte = crm_Par_FonteRepository;
            _Az_Cliente = az_ClienteRepository;
            _Crm_Attivita = crm_AttivitaRepository;
            _Crm_ClienteDati = crm_ClienteDatiRepository;
            _Crm_Lead = crm_LeadRepository;
            _Crm_Offerta = crm_OffertaRepository;
            _Crm_Opportunita = crm_OpportunitaRepository;
            _AspNetRoles = aspNetRolesRepository;
            _AspNetUsers = aspNetUsersRepository;
            _UserCompany = userCompanyRepository;
            _AspNetUserRoles = aspNetUserRolesRepository;
            _commonCompanyUtility = commonCompanyUtility;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<CrmContesto> ContestoAsync()
        {
            if (_contesto != null)
                return _contesto;

            var user = _httpContextAccessor.HttpContext?.User;
            var idUtente = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            if (!int.TryParse(user?.FindFirst(TenantSchemaAccessor.CompanyClaim)?.Value, out var idCompany) || idCompany <= 0)
                throw new UserMessageException(InfrastructureMessages.Get("NoCompanySelected"));

            var az = await _commonCompanyUtility.GetAz_AnagraficaAsync(idCompany, true);
            await EnsureDefaultsAsync(az!.Id);

            var appUser = string.IsNullOrEmpty(idUtente) ? null : await _userManager.FindByIdAsync(idUtente);
            var ruoli = appUser == null ? new List<string>() : (await _userManager.GetRolesAsync(appUser)).ToList();
            bool powerAdmin = ruoli.Contains(nameof(RoleCode.CompanyPowerAdmin));
            bool admin = ruoli.Contains(nameof(RoleCode.CompanyAdmin));
            bool utente = ruoli.Contains(nameof(RoleCode.User));
            if (!powerAdmin && !admin && !utente)
                throw new UserMessageException(CrmMessages.Get("NotAllowed"));

            var cfg = await _Crm_Cfg.FindAll(_ => true).FirstAsync(x => x.IdAz_Anagrafica == az.Id);
            _contesto = new CrmContesto
            {
                IdCompany = idCompany,
                IdAz_Anagrafica = az.Id,
                IdUtente = idUtente,
                UserName = appUser?.UserName ?? string.Empty,
                PowerAdmin = powerAdmin,
                Admin = admin,
                User = utente,
                VedeTutto = powerAdmin || admin || cfg.Visibilita == Crm_Visibilita.Tutti,
            };
            return _contesto;
        }

        public Task<Crm_Cfg> CfgAsync(CrmContesto ctx) => _Crm_Cfg.FindAll(_ => true).FirstAsync(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica);

        public IQueryable<Az_Cliente> ClientiVisibili(CrmContesto ctx)
        {
            var q = _Az_Cliente.FindAll(c => c.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
            if (ctx.VedeTutto)
                return q;
            // sottoquery dai repository in variabili locali: EF le incorpora nella query (una chiamata a FindAll
            // scritta dentro la lambda non si traduce in SQL)
            var me = ctx.IdUtente;
            var dati = _Crm_ClienteDati.FindAll(_ => true);
            var opportunita = _Crm_Opportunita.FindAll(_ => true);
            var attivita = _Crm_Attivita.FindAll(_ => true);
            return q.Where(c => !dati.Any(d => d.IdAz_Cliente == c.Id && d.IdAspNetUsersResponsabile != null && d.IdAspNetUsersResponsabile != me)
                                || opportunita.Any(o => o.IdAz_Cliente == c.Id && o.IdAspNetUsersResponsabile == me)
                                || attivita.Any(a => a.IdAz_Cliente == c.Id && a.IdAspNetUsersAssegnatario == me));
        }

        public IQueryable<Crm_Opportunita> OpportunitaVisibili(CrmContesto ctx)
        {
            var q = _Crm_Opportunita.FindAll(o => o.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
            if (ctx.VedeTutto)
                return q;
            var me = ctx.IdUtente;
            var clienti = ClientiVisibili(ctx).Select(c => c.Id);
            return q.Where(o => o.IdAspNetUsersResponsabile == me || clienti.Contains(o.IdAz_Cliente));
        }

        public IQueryable<Crm_Attivita> AttivitaVisibili(CrmContesto ctx)
        {
            var q = _Crm_Attivita.FindAll(a => a.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
            if (ctx.VedeTutto)
                return q;
            var me = ctx.IdUtente;
            var clienti = ClientiVisibili(ctx).Select(c => c.Id);
            var lead = LeadVisibili(ctx).Select(l => l.Id);
            return q.Where(a => a.IdAspNetUsersAssegnatario == me
                                || (a.IdAz_Cliente != null && clienti.Contains(a.IdAz_Cliente.Value))
                                || (a.IdCrm_Lead != null && lead.Contains(a.IdCrm_Lead.Value)));
        }

        public IQueryable<Crm_Lead> LeadVisibili(CrmContesto ctx)
        {
            var q = _Crm_Lead.FindAll(l => l.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
            if (ctx.VedeTutto)
                return q;
            var me = ctx.IdUtente;
            return q.Where(l => l.IdAspNetUsersResponsabile == null || l.IdAspNetUsersResponsabile == me);
        }

        public IQueryable<Crm_Offerta> OfferteVisibili(CrmContesto ctx)
        {
            var q = _Crm_Offerta.FindAll(o => o.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
            if (ctx.VedeTutto)
                return q;
            var clienti = ClientiVisibili(ctx).Select(c => c.Id);
            return q.Where(o => clienti.Contains(o.IdAz_Cliente));
        }

        public async Task<Az_Cliente> ClienteAsync(CrmContesto ctx, int idAz_Cliente)
            => await ClientiVisibili(ctx).AsNoTracking().FirstOrDefaultAsync(c => c.Id == idAz_Cliente)
               ?? throw new UserMessageException(CrmMessages.Get("CustomerNotFound"));

        public async Task<List<Crm_UtenteModel>> UtentiAsync(CrmContesto ctx)
        {
            var idRuoli = await _AspNetRoles.FindAll(r => RuoliAzienda.Contains(r.Name!)).Select(r => r.Id).ToListAsync();
            // sottoquery dai repository in variabili locali: EF le incorpora nella query (una chiamata a FindAll
            // scritta dentro la lambda non si traduce in SQL)
            var ruoliUtenti = _AspNetUserRoles.FindAll(_ => true);
            var ids = await _UserCompany.FindAll(x => x.IdCompany == ctx.IdCompany && !x.Disabled)
                                     .Where(x => ruoliUtenti.Any(r => r.UserId == x.IdAspNetUsers && idRuoli.Contains(r.RoleId)))
                                     .Select(x => x.IdAspNetUsers).Distinct().ToListAsync();
            return await _AspNetUsers.FindAll(u => ids.Contains(u.Id))
                                  .OrderBy(u => u.UserName)
                                  .Select(u => new Crm_UtenteModel { Id = u.Id, UserName = u.UserName ?? u.Id })
                                  .ToListAsync();
        }

        public async Task<Dictionary<string, string>> NomiAsync(IEnumerable<string?> ids)
        {
            var lista = ids.Where(x => !string.IsNullOrEmpty(x)).Select(x => x!).Distinct().ToList();
            if (lista.Count == 0)
                return new Dictionary<string, string>();
            return await _AspNetUsers.FindAll(u => lista.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Id);
        }

        public async Task EnsureDefaultsAsync(int idAz_Anagrafica)
        {
            if (await _Crm_Cfg.FindAll(_ => true).AnyAsync(x => x.IdAz_Anagrafica == idAz_Anagrafica))
                return;

            var now = DateTime.Now;
            await _Crm_Cfg.CreateAsync(new Crm_Cfg { IdAz_Anagrafica = idAz_Anagrafica, OffertaAnno = now.Year, CreationDate = now }, false);

            int ordine = 0;
            foreach (var (chiave, prob, tipo) in new[]
                     {
                         ("DefaultPhaseNew", 10, Crm_FaseTipo.Aperta), ("DefaultPhaseQualified", 25, Crm_FaseTipo.Aperta),
                         ("DefaultPhaseProposal", 50, Crm_FaseTipo.Aperta), ("DefaultPhaseNegotiation", 75, Crm_FaseTipo.Aperta),
                         ("DefaultPhaseWon", 100, Crm_FaseTipo.Vinta), ("DefaultPhaseLost", 0, Crm_FaseTipo.Persa),
                     })
                await _Crm_Par_Fase.CreateAsync(new Crm_Par_Fase { IdAz_Anagrafica = idAz_Anagrafica, Descrizione = CrmMessages.Get(chiave), Probabilita = prob, Tipo = tipo, Ordine = (ordine += 10), CreationDate = now }, false);

            ordine = 0;
            foreach (var (chiave, icona, colore, durata) in new[]
                     {
                         ("DefaultActivityCall", "call", "primary", 15), ("DefaultActivityEmail", "mail", "tertiary", 10),
                         ("DefaultActivityMeeting", "people", "success", 60), ("DefaultActivityTask", "checkbox", "warning", 30),
                     })
                await _Crm_Par_TipoAttivita.CreateAsync(new Crm_Par_TipoAttivita { IdAz_Anagrafica = idAz_Anagrafica, Descrizione = CrmMessages.Get(chiave), Icona = icona, Colore = colore, DurataMinuti = durata, Ordine = (ordine += 10), CreationDate = now }, false);

            ordine = 0;
            foreach (var chiave in new[] { "DefaultLossPrice", "DefaultLossCompetitor", "DefaultLossNoBudget", "DefaultLossNoAnswer", "DefaultLossOther" })
                await _Crm_Par_MotivoPerdita.CreateAsync(new Crm_Par_MotivoPerdita { IdAz_Anagrafica = idAz_Anagrafica, Descrizione = CrmMessages.Get(chiave), Ordine = (ordine += 10), CreationDate = now }, false);

            ordine = 0;
            foreach (var chiave in new[] { "DefaultSourceWebsite", "DefaultSourceReferral", "DefaultSourceFair", "DefaultSourcePhone", "DefaultSourceOther" })
                await _Crm_Par_Fonte.CreateAsync(new Crm_Par_Fonte { IdAz_Anagrafica = idAz_Anagrafica, Descrizione = CrmMessages.Get(chiave), Ordine = (ordine += 10), CreationDate = now }, false);

            try
            {
                await _Crm_Cfg.SaveChange();
            }
            catch (DbUpdateException)
            {
                // due richieste contemporanee: l'altra ha gia' creato i valori predefiniti; questa si ripete
                throw new UserMessageException(CrmMessages.Get("RetryLater"));
            }
        }
    }
}
