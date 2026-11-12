using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Assets;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Request;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Assets._utility
{
    /*
     Approvazione delle prenotazioni (AST-D4, ANALISI_INFRASTRUCTURE_RICHIESTE.md RICH-AST): parti comuni al servizio delle
     prenotazioni e al provider delle richieste comuni (AstRequestProvider).
        - approvano gli amministratori dell'azienda (CompanyPowerAdmin, CompanyAdmin): basta uno
        - scadenza: una richiesta ancora InAttesa all'ora di inizio diventa Annullata, con notifica al richiedente.
          Si applica in modo pigro (ScadiAsync) all'inizio di elenchi, contatori e salvataggi delle prenotazioni e delle
          richieste: i contatori del menu (RequestCount) si ricalcolano a ogni login e a ogni evento delle richieste, quindi la
          regola scatta appena qualcuno dell'azienda usa l'app, senza un servizio in background che apra gli schemi di tutte
          le aziende ogni pochi minuti (quello giornaliero delle restituzioni sarebbe troppo lento per una fascia oraria)
        - notifiche nella categoria comune "richieste" (rispettano Ast_Cfg.NotificheAttive); dopo ogni cambio l'avviso
          SignalR "RequestNotifier" a richiedente e amministratori (contatori e pagine delle richieste)
    */
    public class AstApprovazioni : IAstApprovazioni
    {
        public const string UrlRichieste = "/requests";
        public const string UrlRichiesteAdmin = "/requestsadmin";

        private readonly IAst_PrenotazioneRepository _Ast_Prenotazione;
        private readonly IAstUtility _utility;
        private readonly ICompanyRecipients _companyRecipients;
        private readonly IRequestNotifier _requestNotifier;
        private readonly IAst_Dip_AnagraficaReadRepository _Dip_Anagrafica;
        private readonly IAspNetUsersRepository _aspNetUsersRepository;

        public AstApprovazioni(IAst_PrenotazioneRepository ast_PrenotazioneRepository, IAstUtility utility,
                               ICompanyRecipients companyRecipients, IRequestNotifier requestNotifier,
                               IAst_Dip_AnagraficaReadRepository dip_AnagraficaRepository, IAspNetUsersRepository aspNetUsersRepository)
        {
            _Dip_Anagrafica = dip_AnagraficaRepository;
            _aspNetUsersRepository = aspNetUsersRepository;
            _Ast_Prenotazione = ast_PrenotazioneRepository;
            _utility = utility;
            _companyRecipients = companyRecipients;
            _requestNotifier = requestNotifier;
        }

        public async Task ScadiAsync(AstContesto ctx)
        {
            var ora = DateTime.Now;
            var scadute = await _Ast_Prenotazione.FindAll(x => x.Ast_RisorsaNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica
                                                              && x.Stato == AstStatoPrenotazione.InAttesa && x.Inizio <= ora)
                                                 .Include(x => x.Ast_RisorsaNavigation).AsTracking().ToListAsync();
            if (scadute.Count == 0)
                return;

            foreach (var p in scadute)
            {
                p.Stato = AstStatoPrenotazione.Annullata;
                p.DataAnnullamento = ora;
                p.ModifiedDate = ora;
                p.ChangeUser = ctx.IdUtente;
            }
            await _Ast_Prenotazione.SaveChange();

            foreach (var p in scadute)
            {
                var risorsa = p.Ast_RisorsaNavigation!.Descrizione;
                await NotificaAsync(ctx, new[] { p.IdAspNetUsers }, AssetsMessages.Get("BookingExpiredTitle", risorsa),
                                    AssetsMessages.Get("BookingExpiredText", risorsa, p.Inizio.ToString("dd/MM/yyyy HH:mm"), p.Fine.ToString("HH:mm")), UrlRichieste);
            }
            await AvvisaAsync(ctx, scadute.Select(x => x.IdAspNetUsers), null);
        }

        public async Task NuovaRichiestaAsync(AstContesto ctx, Ast_Prenotazione p, string risorsa, string richiedente)
        {
            var admin = await _companyRecipients.AdminsAsync(ctx.IdCompany);
            await NotificaAsync(ctx, admin.Where(x => x != p.IdAspNetUsers), AssetsMessages.Get("BookingPendingNewTitle", risorsa),
                                AssetsMessages.Get("BookingPendingNewText", richiedente, risorsa, p.Inizio.ToString("dd/MM/yyyy HH:mm"), p.Fine.ToString("HH:mm")),
                                UrlRichiesteAdmin);
            await AvvisaAsync(ctx, new[] { p.IdAspNetUsers }, p.Id, admin);
        }

        public async Task EsitoAsync(AstContesto ctx, Ast_Prenotazione p, string risorsa)
        {
            string inizio = p.Inizio.ToString("dd/MM/yyyy HH:mm"), fine = p.Fine.ToString("HH:mm");
            if (p.Stato == AstStatoPrenotazione.Confermata)
                await NotificaAsync(ctx, new[] { p.IdAspNetUsers }, AssetsMessages.Get("BookingApprovedTitle", risorsa),
                                    AssetsMessages.Get("BookingApprovedText", risorsa, inizio, fine), UrlRichieste);
            else if (p.Stato == AstStatoPrenotazione.Rifiutata)
                await NotificaAsync(ctx, new[] { p.IdAspNetUsers }, AssetsMessages.Get("BookingRejectedTitle", risorsa),
                                    AssetsMessages.Get("BookingRejectedText", risorsa, inizio, fine)
                                    + (string.IsNullOrEmpty(p.MotivoRifiuto) ? string.Empty : " " + AssetsMessages.Get("BookingRejectedReason", p.MotivoRifiuto)),
                                    UrlRichieste);
            await AvvisaAsync(ctx, new[] { p.IdAspNetUsers }, p.Id);
        }

        public async Task AvvisaAsync(AstContesto ctx, IEnumerable<string> utenti, int? idPrenotazione, List<string>? admin = null)
        {
            try
            {
                admin ??= await _companyRecipients.AdminsAsync(ctx.IdCompany);
                await _requestNotifier.NotifyAsync(utenti.Concat(admin), idPrenotazione == null ? null : AstRequestProvider.PrefixAst + ":" + idPrenotazione);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Prenotazioni: avviso delle richieste non inviato");
            }
        }

        // nominativo (dipendente) o nome utente di chi ha prenotato
        public async Task<Dictionary<string, string>> NomiAsync(AstContesto ctx, IEnumerable<string> utenti)
        {
            var ids = utenti.Distinct().ToList();
            var nomi = (await _Dip_Anagrafica.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.IdAspNetUsers != null && ids.Contains(x.IdAspNetUsers))
                                             .Select(x => new { x.IdAspNetUsers, x.Cognome, x.Nome }).ToListAsync())
                       .GroupBy(x => x.IdAspNetUsers!).ToDictionary(g => g.Key, g => AstUtility.Nominativo(g.First().Cognome, g.First().Nome));
            var mancanti = ids.Where(x => !nomi.ContainsKey(x)).ToList();
            if (mancanti.Count > 0)
                foreach (var u in await _aspNetUsersRepository.FindAll(x => mancanti.Contains(x.Id)).Select(x => new { x.Id, x.UserName }).ToListAsync())
                    nomi[u.Id] = u.UserName ?? string.Empty;
            return nomi;
        }

        private async Task NotificaAsync(AstContesto ctx, IEnumerable<string> utenti, string titolo, string messaggio, string url)
        {
            try
            {
                await _utility.NotificaAsync(ctx, NotificationCategory.Richieste, utenti, titolo, messaggio, url);
            }
            catch (Exception ex)
            {
                // la notifica non annulla l'operazione gia' salvata
                Log.Warning(ex, "Prenotazioni: notifica non inviata ({Titolo})", titolo);
            }
        }
    }

    public interface IAstApprovazioni
    {
        /// <summary>Richieste ancora in attesa con inizio passato: annullate, con notifica al richiedente.</summary>
        Task ScadiAsync(AstContesto ctx);

        /// <summary>Nuova richiesta (o modifica che la riporta in attesa): notifica agli amministratori e avviso SignalR.</summary>
        Task NuovaRichiestaAsync(AstContesto ctx, Ast_Prenotazione p, string risorsa, string richiedente);

        /// <summary>Esito (approvata / rifiutata) al richiedente e avviso SignalR.</summary>
        Task EsitoAsync(AstContesto ctx, Ast_Prenotazione p, string risorsa);

        Task<Dictionary<string, string>> NomiAsync(AstContesto ctx, IEnumerable<string> utenti);

        /// <summary>Solo avviso SignalR (contatori e pagine delle richieste) agli utenti indicati e agli amministratori.</summary>
        Task AvvisaAsync(AstContesto ctx, IEnumerable<string> utenti, int? idPrenotazione, List<string>? admin = null);
    }
}
