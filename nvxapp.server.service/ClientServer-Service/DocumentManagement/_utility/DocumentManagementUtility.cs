using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.DocumentManagement;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Interfaces;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Tenant.DocumentManagement;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common;
using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyApplication;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage;
using System.Security.Claims;

namespace nvxapp.server.service.ClientServer_Service.DocumentManagement._utility
{
    /*
     Chi sta lavorando nel documentale (azienda e ruoli del token) e cosa puo' fare (ANALISI_DOCUMENTMANAGEMENT_APPLICATIVO.md §4):
        - CompanyPowerAdmin: configurazione (tipologie, cartelle, impostazioni) e archivio aziendale
        - CompanyAdmin     : archivio aziendale
        - documenti del personale: il CompanyPowerAdmin e' sempre abilitato (vede e carica); il CompanyAdmin solo con
          abilitazione esplicita per utente (Doc_UtenteAbilitazione), che gestiscono il CompanyPowerAdmin, il SuperUser
          e il DealerPowerAdmin del centro
        - dipendente (User): i propri documenti pubblicati, quelli di cui e' destinatario e le cartelle visibili agli utenti
    */
    public sealed class DocContesto
    {
        public int IdCompany { get; init; }
        public int IdAz_Anagrafica { get; init; }
        public string IdUtente { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public bool PowerAdmin { get; init; }
        public bool Admin { get; init; }
        public bool VedePersonale { get; init; }
        public bool CaricaPersonale { get; init; }
        public bool GestisceAbilitazioni { get; init; }
        // dipendente collegato all'utente corrente nell'azienda (null se non lo e')
        public int? IdDip_Anagrafica { get; init; }

        public bool AmministraArchivio => PowerAdmin || Admin;
    }

    public interface IDocumentManagementUtility
    {
        /// <summary>Contesto della richiesta; UserMessageException se non c'e' un'azienda selezionata.</summary>
        Task<DocContesto> ContestoAsync();

        /// <summary>Configurazione, tipologie e cartelle predefinite dell'azienda (se mancano).</summary>
        Task EnsureDefaultsAsync(int idAz_Anagrafica);

        /// <summary>Il documento e' visibile all'utente del contesto.</summary>
        IQueryable<Doc_Documento> Visibili(DocContesto ctx);

        /// <summary>Scrive nel registro degli accessi.</summary>
        Task LogAsync(DocContesto ctx, Doc_LogAzione azione, int? idDocumento, int? versione = null, string? dettaglio = null);
    }

    public class DocumentManagementUtility : IDocumentManagementUtility
    {
        private readonly IDoc_CfgRepository _cfgRepository;
        private readonly IDoc_Par_TipoRepository _tipoRepository;
        private readonly IDoc_CartellaRepository _cartellaRepository;
        private readonly IDoc_UtenteAbilitazioneRepository _abilitazioneRepository;
        private readonly IDoc_DocumentoRepository _documentoRepository;
        private readonly IDoc_LogRepository _logRepository;
        private readonly IDoc_Dip_AnagraficaReadRepository _dipendenteReadRepository;
        private readonly ICommonCompanyUtility _commonCompanyUtility;
        private readonly IApplicationActivationAuthorizer _authorizer;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        private DocContesto? _contesto;

        public DocumentManagementUtility(IDoc_CfgRepository cfgRepository,
                                         IDoc_Par_TipoRepository tipoRepository,
                                         IDoc_CartellaRepository cartellaRepository,
                                         IDoc_UtenteAbilitazioneRepository abilitazioneRepository,
                                         IDoc_DocumentoRepository documentoRepository,
                                         IDoc_LogRepository logRepository,
                                         IDoc_Dip_AnagraficaReadRepository dipendenteReadRepository,
                                         ICommonCompanyUtility commonCompanyUtility,
                                         IApplicationActivationAuthorizer authorizer,
                                         IHttpContextAccessor httpContextAccessor,
                                         UserManager<ApplicationUser> userManager)
        {
            _cfgRepository = cfgRepository;
            _tipoRepository = tipoRepository;
            _cartellaRepository = cartellaRepository;
            _abilitazioneRepository = abilitazioneRepository;
            _documentoRepository = documentoRepository;
            _logRepository = logRepository;
            _dipendenteReadRepository = dipendenteReadRepository;
            _commonCompanyUtility = commonCompanyUtility;
            _authorizer = authorizer;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<DocContesto> ContestoAsync()
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
            var abil = await _abilitazioneRepository.FindAll(x => x.IdAz_Anagrafica == az.Id && x.IdAspNetUsers == idUtente).FirstOrDefaultAsync();
            bool powerAdmin = ruoli.Contains(nameof(RoleCode.CompanyPowerAdmin));
            bool admin = ruoli.Contains(nameof(RoleCode.CompanyAdmin));
            // il CompanyPowerAdmin e' sempre abilitato; il CompanyAdmin solo se abilitato esplicitamente
            bool vede = powerAdmin || (admin && (abil?.VedePersonale ?? false));
            bool carica = powerAdmin || (admin && (abil?.CaricaPersonale ?? false));

            _contesto = new DocContesto
            {
                IdCompany = idCompany,
                IdAz_Anagrafica = az.Id,
                IdUtente = idUtente,
                UserName = appUser?.UserName ?? string.Empty,
                PowerAdmin = powerAdmin,
                Admin = admin,
                VedePersonale = vede || carica,
                CaricaPersonale = carica,
                GestisceAbilitazioni = powerAdmin || await _authorizer.CanManageCompanyAsync(idCompany),
                IdDip_Anagrafica = string.IsNullOrEmpty(idUtente) ? null
                    : await _dipendenteReadRepository.FindAll(x => x.IdAz_Anagrafica == az.Id && x.IdAspNetUsers == idUtente).Select(x => (int?)x.Id).FirstOrDefaultAsync()
            };
            return _contesto;
        }

        public IQueryable<Doc_Documento> Visibili(DocContesto ctx)
        {
            var q = _documentoRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
            var idUtente = ctx.IdUtente;
            var idDip = ctx.IdDip_Anagrafica;

            return q.Where(d =>
                // documenti del personale: solo chi e' abilitato, oppure il dipendente stesso (pubblicati)
                (d.Doc_Par_TipoNavigation!.Categoria == Doc_Categoria.Personale || d.IdDip_Anagrafica != null)
                    ? (ctx.VedePersonale
                       || (d.Stato == Doc_Stato.Pubblicato && idDip != null && d.IdDip_Anagrafica == idDip)
                       || (d.Stato == Doc_Stato.Pubblicato && d.Doc_Destinatario!.Any(r => r.IdAspNetUsers == idUtente)))
                    // archivio aziendale e clienti: amministratori; utenti solo cartelle visibili o se destinatari
                    : (ctx.AmministraArchivio
                       || (d.Stato == Doc_Stato.Pubblicato && d.Doc_CartellaNavigation != null && d.Doc_CartellaNavigation.VisibileUtenti)
                       || (d.Stato == Doc_Stato.Pubblicato && d.Doc_Destinatario!.Any(r => r.IdAspNetUsers == idUtente))));
        }

        public async Task LogAsync(DocContesto ctx, Doc_LogAzione azione, int? idDocumento, int? versione = null, string? dettaglio = null)
        {
            // il registro porta l'utente del contesto (i repository dell'utilita' non sono campi del servizio)
            ((ICurrentUser)_logRepository).CurrentUserId = ctx.IdUtente;
            await _logRepository.CreateAsync(new Doc_Log
            {
                IdDoc_Documento = idDocumento,
                Versione = versione,
                IdAspNetUsers = ctx.IdUtente,
                UserName = ctx.UserName,
                Azione = azione,
                Data = DateTime.Now,
                Dettaglio = dettaglio?.Length > 500 ? dettaglio[..500] : dettaglio,
                Ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                CreationDate = DateTime.Now,
                ChangeUser = ctx.IdUtente
            });
        }

        public async Task EnsureDefaultsAsync(int idAz_Anagrafica)
        {
            if (await _cfgRepository.FindAll(x => x.IdAz_Anagrafica == idAz_Anagrafica).AnyAsync())
                return;

            // tutto in un unico salvataggio (i repository condividono il contesto della richiesta): con due richieste
            // contemporanee l'indice unico di Doc_Cfg ne fa fallire una sola, senza valori predefiniti doppi
            var now = DateTime.Now;
            await _cfgRepository.CreateAsync(new Doc_Cfg { IdAz_Anagrafica = idAz_Anagrafica, CreationDate = now }, false);

            // tipologie predefinite (codice, chiave del testo, categoria, anni di conservazione, scadenza, presa visione, competenza)
            var tipi = new (string Codice, string Chiave, Doc_Categoria Cat, int? Anni, bool Scad, bool Presa, bool Comp)[]
            {
                ("CEDOLINO",  "DefaultTypePayslip",      Doc_Categoria.Personale, 5,    false, true,  true),
                ("CU",        "DefaultTypeCU",           Doc_Categoria.Personale, 10,   false, true,  true),
                ("CONTRATTO", "DefaultTypeContract",     Doc_Categoria.Personale, 10,   false, true,  false),
                ("CERTMED",   "DefaultTypeMedical",      Doc_Categoria.Personale, 5,    true,  false, false),
                ("FORMAZ",    "DefaultTypeTraining",     Doc_Categoria.Personale, 10,   true,  true,  false),
                ("COMUNIC",   "DefaultTypeNotice",       Doc_Categoria.Personale, 5,    false, true,  false),
                ("F24",       "DefaultTypeF24",          Doc_Categoria.Azienda,   10,   false, false, true),
                ("DURC",      "DefaultTypeDURC",         Doc_Categoria.Azienda,   5,    true,  false, false),
                ("VISURA",    "DefaultTypeRegistry",     Doc_Categoria.Azienda,   null, true,  false, false),
                ("CONTRCLI",  "DefaultTypeCustomerContract", Doc_Categoria.Cliente, 10, true,  false, false),
                ("GENERICO",  "DefaultTypeGeneric",      Doc_Categoria.Azienda,   null, false, false, false),
            };
            int ordine = 0;
            foreach (var t in tipi)
                await _tipoRepository.CreateAsync(new Doc_Par_Tipo
                {
                    IdAz_Anagrafica = idAz_Anagrafica, Codice = t.Codice, Descrizione = DocumentManagementMessages.Get(t.Chiave),
                    Categoria = t.Cat, AnniConservazione = t.Anni, HaScadenza = t.Scad, PresaVisioneRichiesta = t.Presa, HaCompetenza = t.Comp,
                    // firma proposta solo per il contratto (all'invio si puo' sempre togliere o aggiungere)
                    FirmaRichiesta = t.Codice == "CONTRATTO",
                    Ordine = (ordine += 10), Active = true, CreationDate = now
                }, false);

            ordine = 0;
            foreach (var (chiave, visibile) in new[] { ("DefaultFolderPersonnel", false), ("DefaultFolderAdministration", false),
                                                      ("DefaultFolderCustomers", false), ("DefaultFolderNotices", true) })
                await _cartellaRepository.CreateAsync(new Doc_Cartella
                {
                    IdAz_Anagrafica = idAz_Anagrafica, Descrizione = DocumentManagementMessages.Get(chiave),
                    Ordine = (ordine += 10), VisibileUtenti = visibile, DiSistema = true, CreationDate = now
                }, false);

            try
            {
                await _cfgRepository.SaveChange();
            }
            catch (DbUpdateException ex)
            {
                // due richieste contemporanee: l'altra ha gia' creato i valori predefiniti; si scartano quelli non salvati
                ex.Entries.FirstOrDefault()?.Context.ChangeTracker.Clear();
            }
        }
    }
}
