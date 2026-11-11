using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Digest
{
    /*
     Generazione dei riepiloghi (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-AI.2 e INF-AI.3).
        - azienda: uno per ogni amministratore attivo (CompanyPowerAdmin, CompanyAdmin) con le sezioni dei provider delle
          funzioni base e degli applicativi attivi, viste da lui; nessuna sezione = nessun riepilogo
        - studio: uno per studio con le aziende dello studio che hanno almeno una sezione critica (solo le sezioni critiche,
          viste dall'azienda); nessuna azienda con criticita' = nessun riepilogo. Lo vedono gli utenti dello studio
          (FinancialAdvisorPowerAdmin, FinancialAdvisorAdmin) non disabilitati
        - il testo lo scrive IDigestWriter (AI o testo semplice); il riepilogo si salva e si invia come notifica della
          categoria "riepilogo" (l'utente puo' disattivarla dalle preferenze) con il link alla pagina del riepilogo
        - periodo: gli ultimi 7 giorni, fino al giorno della generazione compreso
        - generazione automatica (settimanale): non si rigenera un riepilogo automatico gia' fatto per lo stesso giorno;
          "Genera ora" (manuale) ne crea sempre uno nuovo
     Un provider che fallisce non blocca il riepilogo (la sua sezione manca, errore nel log).
    */
    public class DigestGenerator : IDigestGenerator
    {
        public const string UrlDettaglio = "/digestdetail";
        public static readonly string[] RuoliStudio = { nameof(RoleCode.FinancialAdvisorPowerAdmin), nameof(RoleCode.FinancialAdvisorAdmin) };

        private readonly IEnumerable<IDigestProvider> _providers;
        private readonly IDigestWriter _writer;
        private readonly IRiepilogoRepository _riepilogoRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly IFinancialAdvisorRepository _financialAdvisorRepository;
        private readonly IUserFinancialAdvisorRepository _userFinancialAdvisorRepository;
        private readonly IAspNetRolesRepository _aspNetRolesRepository;
        private readonly IAspNetUserRolesRepository _aspNetUserRolesRepository;
        private readonly ICompanyRecipients _companyRecipients;
        private readonly INotificationSender _notificationSender;

        public DigestGenerator(IEnumerable<IDigestProvider> providers,
                               IDigestWriter writer,
                               IRiepilogoRepository riepilogoRepository,
                               ICompanyRepository companyRepository,
                               ICompanyApplicationRepository companyApplicationRepository,
                               IFinancialAdvisorRepository financialAdvisorRepository,
                               IUserFinancialAdvisorRepository userFinancialAdvisorRepository,
                               IAspNetRolesRepository aspNetRolesRepository,
                               IAspNetUserRolesRepository aspNetUserRolesRepository,
                               ICompanyRecipients companyRecipients,
                               INotificationSender notificationSender)
        {
            _providers = providers;
            _writer = writer;
            _riepilogoRepository = riepilogoRepository;
            _companyRepository = companyRepository;
            _companyApplicationRepository = companyApplicationRepository;
            _financialAdvisorRepository = financialAdvisorRepository;
            _userFinancialAdvisorRepository = userFinancialAdvisorRepository;
            _aspNetRolesRepository = aspNetRolesRepository;
            _aspNetUserRolesRepository = aspNetUserRolesRepository;
            _companyRecipients = companyRecipients;
            _notificationSender = notificationSender;
        }

        /// <summary>periodo del riepilogo generato il giorno indicato: gli ultimi 7 giorni compreso quello</summary>
        public static (DateTime Dal, DateTime Al) Periodo(DateTime adesso) => (adesso.Date.AddDays(-6), adesso.Date);

        public async Task<int> GenerateWeeklyAsync(DateTime adesso)
        {
            int generati = 0;
            foreach (var idCompany in await _companyRepository.FindAll(x => true).OrderBy(x => x.Id).Select(x => x.Id).ToListAsync())
            {
                try { generati += (await GenerateCompanyAsync(idCompany, adesso, false)).Count; }
                catch (Exception ex) { Log.Error(ex, "Riepilogo dell'azienda {IdCompany} non generato", idCompany); }
            }
            foreach (var idStudio in await _financialAdvisorRepository.FindAll(x => true).OrderBy(x => x.Id).Select(x => x.Id).ToListAsync())
            {
                try { generati += await GenerateStudioAsync(idStudio, adesso, false) != null ? 1 : 0; }
                catch (Exception ex) { Log.Error(ex, "Riepilogo dello studio {IdFinancialAdvisor} non generato", idStudio); }
            }
            return generati;
        }

        public async Task<List<Riepilogo>> GenerateCompanyAsync(int idCompany, DateTime adesso, bool manuale)
        {
            var retVal = new List<Riepilogo>();
            var azienda = await _companyRepository.FindAll(x => x.Id == idCompany).Select(x => x.Descrizione).FirstOrDefaultAsync();
            if (azienda == null)
                return retVal;
            var (dal, al) = Periodo(adesso);
            var providers = await ProvidersAttiviAsync(idCompany);

            foreach (var idUtente in await _companyRecipients.AdminsAsync(idCompany))
            {
                if (!manuale && await _riepilogoRepository.FindAll(x => x.Tipo == TipoRiepilogo.Azienda && x.IdCompany == idCompany
                                                                        && x.IdAspNetUsers == idUtente && x.Al == al && !x.Manuale).AnyAsync())
                    continue;

                var sezioni = await SezioniAsync(providers, idCompany, idUtente, dal, al);
                if (sezioni.Count == 0)
                    continue;

                var fatti = new DigestFacts { Dal = dal, Al = al, Soggetto = azienda, Sezioni = sezioni };
                var riepilogo = await SalvaAsync(new Riepilogo
                {
                    Tipo = TipoRiepilogo.Azienda,
                    IdCompany = idCompany,
                    IdAspNetUsers = idUtente,
                    Titolo = Taglia(InfrastructureMessages.Get("DigestTitleCompany", azienda, Data(dal), Data(al)), 200),
                    Manuale = manuale,
                }, fatti);
                await NotificaAsync(riepilogo, new[] { idUtente }, idCompany);
                retVal.Add(riepilogo);
            }
            return retVal;
        }

        public async Task<Riepilogo?> GenerateStudioAsync(int idFinancialAdvisor, DateTime adesso, bool manuale)
        {
            var studio = await _financialAdvisorRepository.FindAll(x => x.Id == idFinancialAdvisor).Select(x => x.Descrizione).FirstOrDefaultAsync();
            if (studio == null)
                return null;
            var utenti = await StudioUsersAsync(idFinancialAdvisor);
            if (utenti.Count == 0)
                return null;
            var (dal, al) = Periodo(adesso);
            if (!manuale && await _riepilogoRepository.FindAll(x => x.Tipo == TipoRiepilogo.Studio && x.IdFinancialAdvisor == idFinancialAdvisor
                                                                    && x.Al == al && !x.Manuale).AnyAsync())
                return null;

            var aziende = new List<DigestCompanyFacts>();
            var elenco = await _companyRepository.FindAll(x => x.IdFinancialAdvisor == idFinancialAdvisor)
                                                 .OrderBy(x => x.Descrizione).Select(x => new { x.Id, x.Descrizione }).ToListAsync();
            foreach (var c in elenco)
            {
                var critiche = (await SezioniAsync(await ProvidersAttiviAsync(c.Id), c.Id, null, dal, al)).Where(s => s.Critica).ToList();
                if (critiche.Count > 0)
                    aziende.Add(new DigestCompanyFacts { IdCompany = c.Id, Azienda = c.Descrizione ?? c.Id.ToString(), Sezioni = critiche });
            }
            if (aziende.Count == 0)
                return null;

            var fatti = new DigestFacts { Dal = dal, Al = al, Soggetto = studio, Aziende = aziende };
            var riepilogo = await SalvaAsync(new Riepilogo
            {
                Tipo = TipoRiepilogo.Studio,
                IdFinancialAdvisor = idFinancialAdvisor,
                Titolo = Taglia(InfrastructureMessages.Get("DigestTitleStudio", studio, Data(dal), Data(al)), 200),
                Manuale = manuale,
            }, fatti);
            await NotificaAsync(riepilogo, utenti, null);
            return riepilogo;
        }

        /// <summary>utenti dello studio che ricevono il riepilogo (ruoli dello studio, collegamento non disabilitato)</summary>
        public async Task<List<string>> StudioUsersAsync(int idFinancialAdvisor)
        {
            var idRuoli = await _aspNetRolesRepository.FindAll(r => RuoliStudio.Contains(r.Name!)).Select(r => r.Id).ToListAsync();
            var utenti = await _userFinancialAdvisorRepository.FindAll(x => x.IdFinancialAdvisor == idFinancialAdvisor && !x.Disabled)
                                                              .Select(x => x.IdAspNetUsers).ToListAsync();
            return await _aspNetUserRolesRepository.FindAll(x => idRuoli.Contains(x.RoleId) && utenti.Contains(x.UserId))
                                                   .Select(x => x.UserId).Distinct().ToListAsync();
        }

        // ---------------------------------------------------------------------------

        private async Task<List<IDigestProvider>> ProvidersAttiviAsync(int idCompany)
        {
            var attivi = await _companyApplicationRepository.FindAll(x => x.IdCompany == idCompany && x.Active).Select(x => x.ApplicationType).ToListAsync();
            return _providers.Where(p => p.Application == ApplicationType.Common || attivi.Contains(p.Application)).ToList();
        }

        private static async Task<List<DigestSection>> SezioniAsync(List<IDigestProvider> providers, int idCompany, string? idUtente, DateTime dal, DateTime al)
        {
            var sezioni = new List<DigestSection>();
            foreach (var p in providers)
            {
                try
                {
                    var s = await p.SectionAsync(idCompany, idUtente, dal, al);
                    if (s != null && s.Righe.Count > 0)
                        sezioni.Add(s);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Riepilogo: sezione di {Provider} non disponibile per l'azienda {IdCompany}", p.GetType().Name, idCompany);
                }
            }
            // prima le sezioni critiche
            return sezioni.OrderByDescending(s => s.Critica).ThenBy(s => s.Ordine).ThenBy(s => s.Codice).ToList();
        }

        private async Task<Riepilogo> SalvaAsync(Riepilogo riepilogo, DigestFacts fatti)
        {
            var testo = await _writer.WriteAsync(fatti);
            riepilogo.Dal = fatti.Dal;
            riepilogo.Al = fatti.Al;
            riepilogo.Testo = testo.Testo;
            riepilogo.Fatti = DigestWriter.Serialize(fatti);
            riepilogo.GeneratoConAI = testo.GeneratoConAI;
            riepilogo.ProviderAI = testo.ProviderAI == null ? null : Taglia(testo.ProviderAI, 50);
            return await _riepilogoRepository.CreateAsync(riepilogo);
        }

        private async Task NotificaAsync(Riepilogo riepilogo, IEnumerable<string> utenti, int? idCompany)
        {
            const int max = 900;
            await _notificationSender.SendAsync(new NotificationMessage
            {
                UserIds = utenti,
                IdCompany = idCompany,
                Categoria = NotificationCategory.Riepilogo.Code,
                Titolo = riepilogo.Titolo,
                Messaggio = riepilogo.Testo.Length > max ? riepilogo.Testo[..max].TrimEnd() + "..." : riepilogo.Testo,
                Url = UrlDettaglio,
                UrlState = new { id = riepilogo.Id },
            });
        }

        private static string Data(DateTime d) => d.ToString("dd/MM/yyyy");

        private static string Taglia(string s, int max) => s.Length > max ? s[..max] : s;
    }

    public interface IDigestGenerator
    {
        /// <summary>generazione automatica: riepiloghi di tutte le aziende e di tutti gli studi; restituisce quanti ne ha creati</summary>
        Task<int> GenerateWeeklyAsync(DateTime adesso);

        /// <summary>riepiloghi degli amministratori dell'azienda (manuale = "Genera ora")</summary>
        Task<List<Riepilogo>> GenerateCompanyAsync(int idCompany, DateTime adesso, bool manuale);

        /// <summary>riepilogo dello studio (null = nessuna azienda con criticita' o nessun destinatario)</summary>
        Task<Riepilogo?> GenerateStudioAsync(int idFinancialAdvisor, DateTime adesso, bool manuale);

        /// <summary>utenti dello studio che ricevono il riepilogo</summary>
        Task<List<string>> StudioUsersAsync(int idFinancialAdvisor);
    }
}
