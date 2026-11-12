using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Assets;
using nvxapp.server.data.Repositories.Tenant.Common;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Request;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Assets._utility
{
    /*
     Approvazione delle prenotazioni (AST-D4, AST-RICH; ANALISI_INFRASTRUCTURE_RICHIESTE.md §3bis): parti comuni al servizio
     delle prenotazioni e al gestore delle richieste comuni (AstRichiestaTipoHandler).
        - la richiesta sta in Dip_GG_Richiesta (Common, tipo PrenotazioneRisorsa, Dati = {"idAst_Prenotazione": n}) e il ciclo
          (approvatori = amministratori dell'azienda, notifiche "richieste", SignalR "RequestNotifier") lo fa il motore comune
          RichiestaCommonService; qui si tiene allineato lo stato della prenotazione (InAttesa / Confermata / Rifiutata / Annullata)
        - scadenza: una richiesta ancora in attesa all'ora di inizio si annulla (motore: ScadiAsync, con notifica al richiedente)
          e la prenotazione diventa Annullata. Si applica in modo pigro (ScadiAsync) all'inizio di elenchi, salvataggi e azioni
          delle prenotazioni e, tramite AstRichiestaTipoHandler.StrategiaAsync, a ogni conteggio ed elenco delle richieste comuni
          (i contatori del menu si ricalcolano a ogni login e a ogni evento delle richieste): niente servizio in background che
          apra gli schemi di tutte le aziende ogni pochi minuti
    */
    public class AstApprovazioni : IAstApprovazioni
    {
        private readonly IAst_PrenotazioneRepository _Ast_Prenotazione;
        private readonly IDip_GG_RichiestaCommonRepository _Dip_GG_Richiesta;
        private readonly IRichiestaCommonService _richieste;
        private readonly IAstUtility _utility;
        private readonly IAst_Dip_AnagraficaReadRepository _Dip_Anagrafica;
        private readonly IAspNetUsersRepository _aspNetUsersRepository;

        public AstApprovazioni(IAst_PrenotazioneRepository ast_PrenotazioneRepository, IDip_GG_RichiestaCommonRepository dip_GG_RichiestaRepository,
                               IRichiestaCommonService richieste, IAstUtility utility,
                               IAst_Dip_AnagraficaReadRepository dip_AnagraficaRepository, IAspNetUsersRepository aspNetUsersRepository)
        {
            _Ast_Prenotazione = ast_PrenotazioneRepository;
            _Dip_GG_Richiesta = dip_GG_RichiestaRepository;
            _richieste = richieste;
            _utility = utility;
            _Dip_Anagrafica = dip_AnagraficaRepository;
            _aspNetUsersRepository = aspNetUsersRepository;
        }

        // ------------------------------------------------------------------------------------------------ dati della richiesta

        public static string Dati(int idAst_Prenotazione) => JsonSerializer.Serialize(new DatiRichiesta { idAst_Prenotazione = idAst_Prenotazione });

        public static int? IdPrenotazione(Dip_GG_Richiesta r)
        {
            try
            {
                return string.IsNullOrWhiteSpace(r.Dati) ? null : JsonSerializer.Deserialize<DatiRichiesta>(r.Dati)?.idAst_Prenotazione;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private sealed class DatiRichiesta
        {
            public int idAst_Prenotazione { get; set; }
        }

        /// <summary>descrizione della prenotazione negli avvisi del motore (risorsa e periodo)</summary>
        public async Task<RichiestaAvviso> AvvisoAsync(AstContesto ctx, Ast_Prenotazione p, string risorsa)
            => new RichiestaAvviso
            {
                Descrizione = AssetsMessages.Get("RequestBookingDescription", risorsa, p.Inizio.ToString("dd/MM/yyyy HH:mm"), p.Fine.ToString("HH:mm")),
                Notifica = (await _utility.CfgAsync(ctx)).NotificheAttive,
            };

        // ------------------------------------------------------------------------------------------------ ciclo

        public async Task CreaRichiestaAsync(AstContesto ctx, Ast_Prenotazione p, string risorsa, Func<Task>? compensa = null)
        {
            // la prenotazione e' gia' salvata (InAttesa, fascia occupata): se la richiesta non si crea (es. nessun amministratore
            // oltre al richiedente) si compensa: la nuova prenotazione si toglie, una modificata torna com'era (compensa)
            try
            {
                var r = await _richieste.CreaAsync(new RichiestaNuova
                {
                    Tipo = TipoRichiesta.PrenotazioneRisorsa,
                    IdDip_Anagrafica = await _utility.MioDipendenteAsync(ctx),
                    Dal = p.Inizio,
                    Al = p.Fine,
                    Dati = Dati(p.Id),
                    Avviso = await AvvisoAsync(ctx, p, risorsa),
                });
                p.IdDip_GG_Richiesta = r.Id;
                await _Ast_Prenotazione.FindAll(x => x.Id == p.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.IdDip_GG_Richiesta, r.Id));
            }
            catch
            {
                if (compensa != null)
                    await compensa();
                else
                    await _Ast_Prenotazione.FindAll(x => x.Id == p.Id).ExecuteDeleteAsync();
                throw;
            }
        }

        /// <summary>Modifica di una prenotazione ancora in attesa: la stessa richiesta segue il nuovo periodo.</summary>
        public async Task AggiornaPeriodoAsync(Dip_GG_Richiesta r, Ast_Prenotazione p)
        {
            r.Data = p.Inizio;
            r.DataA = p.Fine;
            await _Dip_GG_Richiesta.UpdateAsync(r);
        }

        public async Task<FaseRichiesta> ApprovaRichiestaAsync(AstContesto ctx, Dip_GG_Richiesta r, Ast_Prenotazione p, string risorsa)
            => await _richieste.ApprovaAsync(r, await AvvisoAsync(ctx, p, risorsa));

        public async Task<FaseRichiesta> RifiutaRichiestaAsync(AstContesto ctx, Dip_GG_Richiesta r, Ast_Prenotazione p, string risorsa, string? motivo)
            => await _richieste.RifiutaAsync(r, motivo, await AvvisoAsync(ctx, p, risorsa));

        /// <summary>
        /// Annullamento della richiesta del richiedente collegata alla prenotazione, se c'e' (null = nessuna richiesta da annullare):
        /// in attesa si ritira; approvata apre la richiesta di annullamento da approvare (salvo sostituita: si chiude)
        /// </summary>
        public async Task<EsitoAnnullamento?> AnnullaRichiestaAsync(AstContesto ctx, Ast_Prenotazione p, string risorsa, bool sostituita = false)
        {
            var r = await RichiestaAsync(p.IdDip_GG_Richiesta);
            if (r == null || r.IdAspNetUsers != ctx.IdUtente || !RichiestaRegole.PuoAnnullare(r))
                return null;
            return await _richieste.AnnullaAsync(r, await AvvisoAsync(ctx, p, risorsa), sostituita);
        }

        /// <summary>
        /// stato della prenotazione espresso con il ciclo unico delle richieste (RichiestaRegole.TestoStato, richiesta-stato.ts):
        /// la prenotazione e' la fonte (es. annullata da un amministratore o scaduta) e la richiesta da' la fase dell'annullamento
        /// </summary>
        public static (StatoRichiesta Richiesta, StatoRichiesta? Revoca) StatoComeRichiesta(Ast_Prenotazione p, Dip_GG_Richiesta? r)
            => p.Stato switch
            {
                AstStatoPrenotazione.InAttesa => (StatoRichiesta.Immessa, null),
                AstStatoPrenotazione.Rifiutata => (StatoRichiesta.Rifiutata, null),
                AstStatoPrenotazione.Annullata => r?.RevocaStato == StatoRichiesta.Approvata ? (StatoRichiesta.Approvata, StatoRichiesta.Approvata)
                                                                                             : (StatoRichiesta.Cancellata, null),
                _ => r == null ? (StatoRichiesta.Diretta, null) : (StatoRichiesta.Approvata, r.RevocaStato),
            };

        /// <summary>la richiesta di annullamento della prenotazione (approvata) e' in attesa di decisione</summary>
        public static bool RevocaInAttesa(Dip_GG_Richiesta? r)
            => r?.RevocaStato != null && RichiestaRegole.StatiInAttesa.Contains(r.RevocaStato.Value);

        /// <summary>
        /// prenotazioni che l'amministratore collegato puo' decidere adesso (id prenotazione -> id richiesta): in attesa prima
        /// dell'inizio, oppure confermate con la richiesta di annullamento in attesa prima della fine (come la pagina delle richieste)
        /// </summary>
        public async Task<Dictionary<int, int>> DaDecidereAsync(AstContesto ctx, IEnumerable<Ast_Prenotazione> prenotazioni)
        {
            if (!ctx.Amministratore)
                return new Dictionary<int, int>();
            var ora = DateTime.Now;
            var candidate = prenotazioni.Where(x => x.IdDip_GG_Richiesta != null
                                                    && ((x.Stato == AstStatoPrenotazione.InAttesa && x.Inizio > ora)
                                                        || (x.Stato == AstStatoPrenotazione.Confermata && x.Fine > ora))).ToList();
            if (candidate.Count == 0)
                return new Dictionary<int, int>();
            var ids = candidate.Select(x => x.IdDip_GG_Richiesta!.Value).Distinct().ToList();
            var richieste = await _Dip_GG_Richiesta.FindAll(x => ids.Contains(x.Id)).AsNoTracking().ToDictionaryAsync(x => x.Id);
            var res = new Dictionary<int, int>();
            foreach (var p in candidate)
            {
                if (!richieste.TryGetValue(p.IdDip_GG_Richiesta!.Value, out var r)
                    || !RichiestaRegole.PuoDecidere(r, ctx.IdUtente, true, StrategiaApprovazione.Singola))
                    continue;
                bool revoca = RevocaInAttesa(r);
                if ((p.Stato == AstStatoPrenotazione.InAttesa && !revoca) || (p.Stato == AstStatoPrenotazione.Confermata && revoca))
                    res[p.Id] = r.Id;
            }
            return res;
        }

        /// <summary>
        /// stato dell'annullamento (revoca) delle prenotazioni confermate (o annullate: revocate) con richiesta:
        /// id prenotazione -> RevocaStato (assente = mai chiesto)
        /// </summary>
        public async Task<Dictionary<int, StatoRichiesta>> RevocheAsync(IEnumerable<Ast_Prenotazione> prenotazioni)
        {
            var conRichiesta = prenotazioni.Where(x => (x.Stato == AstStatoPrenotazione.Confermata || x.Stato == AstStatoPrenotazione.Annullata)
                                                       && x.IdDip_GG_Richiesta != null).ToList();
            if (conRichiesta.Count == 0)
                return new Dictionary<int, StatoRichiesta>();
            var ids = conRichiesta.Select(x => x.IdDip_GG_Richiesta!.Value).Distinct().ToList();
            var revoche = await _Dip_GG_Richiesta.FindAll(x => ids.Contains(x.Id) && x.RevocaStato != null)
                                                 .Select(x => new { x.Id, x.RevocaStato }).ToDictionaryAsync(x => x.Id, x => x.RevocaStato!.Value);
            return conRichiesta.Where(x => revoche.ContainsKey(x.IdDip_GG_Richiesta!.Value))
                               .ToDictionary(x => x.Id, x => revoche[x.IdDip_GG_Richiesta!.Value]);
        }

        /// <summary>id delle prenotazioni con la richiesta di annullamento in attesa</summary>
        public async Task<HashSet<int>> RevocheInAttesaAsync(IEnumerable<Ast_Prenotazione> prenotazioni)
        {
            var conRichiesta = prenotazioni.Where(x => x.Stato == AstStatoPrenotazione.Confermata && x.IdDip_GG_Richiesta != null).ToList();
            if (conRichiesta.Count == 0)
                return new HashSet<int>();
            var ids = conRichiesta.Select(x => x.IdDip_GG_Richiesta!.Value).Distinct().ToList();
            var stati = RichiestaRegole.StatiInAttesa;
            var revoche = (await _Dip_GG_Richiesta.FindAll(x => ids.Contains(x.Id) && x.RevocaStato != null && stati.Contains(x.RevocaStato.Value))
                                                  .Select(x => x.Id).ToListAsync()).ToHashSet();
            return conRichiesta.Where(x => revoche.Contains(x.IdDip_GG_Richiesta!.Value)).Select(x => x.Id).ToHashSet();
        }

        public async Task<Dip_GG_Richiesta?> RichiestaAsync(int? idDip_GG_Richiesta)
            => idDip_GG_Richiesta == null ? null : await _Dip_GG_Richiesta.FindAll(x => x.Id == idDip_GG_Richiesta).FirstOrDefaultAsync();

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
                try
                {
                    var r = await RichiestaAsync(p.IdDip_GG_Richiesta);
                    if (r != null)
                        await _richieste.ScadiAsync(r, await AvvisoAsync(ctx, p, p.Ast_RisorsaNavigation!.Descrizione), ctx.IdCompany);
                }
                catch (Exception ex)
                {
                    // la prenotazione e' gia' annullata: l'errore resta nel log, la richiesta si puo' annullare dalle pagine
                    Log.Warning(ex, "Prenotazione {Id}: scadenza della richiesta non registrata", p.Id);
                }
            }
        }

        // ------------------------------------------------------------------------------------------------ letture

        /// <summary>motivo del rifiuto (dalla cronologia della richiesta) per le prenotazioni rifiutate, per id di prenotazione</summary>
        public async Task<Dictionary<int, string>> MotiviAsync(IEnumerable<Ast_Prenotazione> prenotazioni)
        {
            var rifiutate = prenotazioni.Where(x => x.Stato == AstStatoPrenotazione.Rifiutata && x.IdDip_GG_Richiesta != null).ToList();
            if (rifiutate.Count == 0)
                return new Dictionary<int, string>();
            var ids = rifiutate.Select(x => x.IdDip_GG_Richiesta!.Value).Distinct().ToList();
            var richieste = await _Dip_GG_Richiesta.FindAll(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
            var res = new Dictionary<int, string>();
            foreach (var p in rifiutate)
                if (richieste.TryGetValue(p.IdDip_GG_Richiesta!.Value, out var r) && Motivo(r) is string m)
                    res[p.Id] = m;
            return res;
        }

        public static string? Motivo(Dip_GG_Richiesta r)
            => RichiestaRegole.Cronologia(r.RichiestaApprovazioneData)
                              .Where(x => x.RichiestaStato == StatoRichiesta.Rifiutata && !string.IsNullOrWhiteSpace(x.Motivo))
                              .OrderByDescending(x => x.Data).Select(x => x.Motivo).FirstOrDefault();

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
    }

    public interface IAstApprovazioni
    {
        Task<RichiestaAvviso> AvvisoAsync(AstContesto ctx, Ast_Prenotazione p, string risorsa);

        /// <summary>Richiesta di approvazione per la prenotazione (gia' salvata InAttesa); se non si crea, la prenotazione si toglie.</summary>
        Task CreaRichiestaAsync(AstContesto ctx, Ast_Prenotazione p, string risorsa, Func<Task>? compensa = null);

        Task AggiornaPeriodoAsync(Dip_GG_Richiesta r, Ast_Prenotazione p);

        Task<FaseRichiesta> ApprovaRichiestaAsync(AstContesto ctx, Dip_GG_Richiesta r, Ast_Prenotazione p, string risorsa);

        Task<FaseRichiesta> RifiutaRichiestaAsync(AstContesto ctx, Dip_GG_Richiesta r, Ast_Prenotazione p, string risorsa, string? motivo);

        Task<EsitoAnnullamento?> AnnullaRichiestaAsync(AstContesto ctx, Ast_Prenotazione p, string risorsa, bool sostituita = false);

        Task<HashSet<int>> RevocheInAttesaAsync(IEnumerable<Ast_Prenotazione> prenotazioni);

        Task<Dictionary<int, StatoRichiesta>> RevocheAsync(IEnumerable<Ast_Prenotazione> prenotazioni);

        Task<Dictionary<int, int>> DaDecidereAsync(AstContesto ctx, IEnumerable<Ast_Prenotazione> prenotazioni);

        Task<Dip_GG_Richiesta?> RichiestaAsync(int? idDip_GG_Richiesta);

        /// <summary>Prenotazioni in attesa con inizio passato: annullate, con la scadenza della richiesta (notifica al richiedente).</summary>
        Task ScadiAsync(AstContesto ctx);

        Task<Dictionary<int, string>> MotiviAsync(IEnumerable<Ast_Prenotazione> prenotazioni);

        Task<Dictionary<string, string>> NomiAsync(AstContesto ctx, IEnumerable<string> utenti);
    }
}
