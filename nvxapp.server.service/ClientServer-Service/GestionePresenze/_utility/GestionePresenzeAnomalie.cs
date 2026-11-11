using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_FestivitaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    /*
     Anomalie delle timbrature, giornate da sistemare prima dell'export paghe e previsione delle assenze
     (ANALISI_ATTENDANCETRACKING_ANOMALIE_AI.md, AT-AI.1-3). Solo regole deterministiche sui dati gia' salvati:
     timbrature, giustificativi, risultato del calcolo (Dip_GG_Result) e orario del giorno risolto dal profilo con la
     stessa regola del motore (GestionePresenzeOreTeoriche). Nessuna scrittura.
     Si legge solo con i repository: nella richiesta quelli dallo scope, fuori dalla richiesta (riepilogo AI) quelli
     costruiti sul contesto dello schema dell'azienda (Crea).
     I chiamanti passano i dipendenti gia' filtrati per visibilita' (azienda, reparti amministrati).
    */
    public class GestionePresenzeAnomalie : IGestionePresenzeAnomalie
    {
        // ── soglie delle regole (§3 dell'analisi) ───────────────────────────────
        public const int ToleranzaFuoriOrarioMin = 30;   // finestra dell'orario senza limiti di riconoscimento
        public const int ToleranzaRitardoMin = 5;        // ritardo oltre l'inizio della prima coppia senza limite destro
        public const int RitardiRicorrentiMin = 3;       // ritardi nel periodo per segnalare il dipendente
        public const int LavoroContinuoMaxMin = 360;     // tratto di lavoro senza pausa (D.Lgs 66/2003 art. 8)
        public const int PausaMinimaMin = 10;            // stacco minimo che interrompe il tratto continuo
        public const int PausaMassimaMin = 120;          // stacco oltre il quale la pausa e' eccessiva

        // ── previsione (§5 dell'analisi) ────────────────────────────────────────
        public const int StoricoGiorni = 365;
        public const int MinimoGiornateTasso = 20;
        public const decimal SogliaPiccoPerc = 0.20m;

        private static readonly StatoRichiesta[] StatiValidi = { StatoRichiesta.Diretta, StatoRichiesta.Approvata };
        private static readonly StatoRichiesta[] StatiInAttesa = { StatoRichiesta.Immessa, StatoRichiesta.ApprovazioneInCorso, StatoRichiesta.ParzialmenteApprovata };

        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly IDip_GG_TimbraturaRepository _dip_GG_TimbraturaRepository;
        private readonly IDip_GG_GiustificativiRepository _dip_GG_GiustificativiRepository;
        private readonly IDip_GG_ResultRepository _dip_GG_ResultRepository;
        private readonly IDip_ProfiloOrarioRepository _dip_ProfiloOrarioRepository;
        private readonly IPar_ProfiloOrarioRepository _par_ProfiloOrarioRepository;
        private readonly IPar_ProfiloOrarioGGRepository _par_ProfiloOrarioGGRepository;
        private readonly IPar_OrarioRepository _par_OrarioRepository;
        private readonly IPar_OrarioIntervalloHHRepository _par_OrarioIntervalloHHRepository;
        private readonly IPar_OrarioPausaRepository _par_OrarioPausaRepository;
        private readonly IPar_FestivitaRepository _par_FestivitaRepository;
        private readonly IPar_GiustificativiRepository _par_GiustificativiRepository;

        public GestionePresenzeAnomalie(IDip_AnagraficaRepository dip_AnagraficaRepository,
                                        IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                        IDip_GG_TimbraturaRepository dip_GG_TimbraturaRepository,
                                        IDip_GG_GiustificativiRepository dip_GG_GiustificativiRepository,
                                        IDip_GG_ResultRepository dip_GG_ResultRepository,
                                        IDip_ProfiloOrarioRepository dip_ProfiloOrarioRepository,
                                        IPar_ProfiloOrarioRepository par_ProfiloOrarioRepository,
                                        IPar_ProfiloOrarioGGRepository par_ProfiloOrarioGGRepository,
                                        IPar_OrarioRepository par_OrarioRepository,
                                        IPar_OrarioIntervalloHHRepository par_OrarioIntervalloHHRepository,
                                        IPar_OrarioPausaRepository par_OrarioPausaRepository,
                                        IPar_FestivitaRepository par_FestivitaRepository,
                                        IPar_GiustificativiRepository par_GiustificativiRepository)
        {
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _dip_GG_TimbraturaRepository = dip_GG_TimbraturaRepository;
            _dip_GG_GiustificativiRepository = dip_GG_GiustificativiRepository;
            _dip_GG_ResultRepository = dip_GG_ResultRepository;
            _dip_ProfiloOrarioRepository = dip_ProfiloOrarioRepository;
            _par_ProfiloOrarioRepository = par_ProfiloOrarioRepository;
            _par_ProfiloOrarioGGRepository = par_ProfiloOrarioGGRepository;
            _par_OrarioRepository = par_OrarioRepository;
            _par_OrarioIntervalloHHRepository = par_OrarioIntervalloHHRepository;
            _par_OrarioPausaRepository = par_OrarioPausaRepository;
            _par_FestivitaRepository = par_FestivitaRepository;
            _par_GiustificativiRepository = par_GiustificativiRepository;
        }

        /// <summary>istanza sui repository di un contesto creato fuori dalla richiesta (IApplicationDbContextFactory)</summary>
        public static GestionePresenzeAnomalie Crea(AttendanceTrackingDbContext db, IServiceProvider sp, IHttpContextAccessor hca) => new(
            new Dip_AnagraficaRepository(db, sp, hca),
            new Dip_RapportoLavoroRepository(db, sp, hca),
            new Dip_GG_TimbraturaRepository(db, sp, hca),
            new Dip_GG_GiustificativiRepository(db, sp, hca),
            new Dip_GG_ResultRepository(db, sp, hca),
            new Dip_ProfiloOrarioRepository(db, sp, hca),
            new Par_ProfiloOrarioRepository(db, sp, hca),
            new Par_ProfiloOrarioGGRepository(db, sp, hca),
            new Par_OrarioRepository(db, sp, hca),
            new Par_OrarioIntervalloHHRepository(db, sp, hca),
            new Par_OrarioPausaRepository(db, sp, hca),
            new Par_FestivitaRepository(db, sp, hca),
            new Par_GiustificativiRepository(db, sp, hca));

        // =====================================================================================
        // Anomalie e giornate da sistemare (AT-AI.1, AT-AI.2)
        // =====================================================================================

        /// <summary>
        /// Anomalie dei dipendenti indicati nei giorni da dal ad al, esclusi oggi e i giorni futuri (giornata in corso)
        /// e i giorni dei mesi approvati (Locked).
        /// </summary>
        public AnomalieRisultato Analizza(IReadOnlyCollection<int> idDip_Anagrafica, DateTime dal, DateTime al, DateTime oggi)
        {
            dal = dal.Date;
            al = al.Date;
            var ultimo = al < oggi.Date ? al : oggi.Date.AddDays(-1);
            var retVal = new AnomalieRisultato { Dal = dal, Al = al };
            if (idDip_Anagrafica.Count == 0 || ultimo < dal)
                return retVal;

            var anagrafiche = Anagrafiche(idDip_Anagrafica);
            var rapporti = Rapporti(idDip_Anagrafica, dal, ultimo);
            var idRapporti = rapporti.Select(r => r.Id).ToList();
            var dopo = ultimo.AddDays(1);

            var timbrature = _dip_GG_TimbraturaRepository.FindAll(t => idRapporti.Contains(t.IdDip_RapportoLavoro)
                                                                      && t.GiornoCompetenza >= dal && t.GiornoCompetenza < dopo)
                                                        .Select(t => new { t.IdDip_RapportoLavoro, t.GiornoCompetenza, t.Timbratura, t.RichiestaStato })
                                                        .ToList()
                                                        .ToLookup(t => (t.IdDip_RapportoLavoro, t.GiornoCompetenza.Date));
            var giustificativi = _dip_GG_GiustificativiRepository.FindAll(g => idRapporti.Contains(g.IdDip_RapportoLavoro)
                                                                              && g.Data >= dal && g.Data < dopo)
                                                                .Select(g => new { g.IdDip_RapportoLavoro, g.Data, g.RichiestaStato })
                                                                .ToList()
                                                                .ToLookup(g => (g.IdDip_RapportoLavoro, g.Data.Date));
            var risultati = _dip_GG_ResultRepository.FindAll(r => idRapporti.Contains(r.IdDip_RapportoLavoro) && r.Data >= dal && r.Data < dopo)
                                                    .Select(r => new { r.IdDip_RapportoLavoro, r.Data, r.Stato, r.HH_Teo })
                                                    .ToList()
                                                    .GroupBy(r => (r.IdDip_RapportoLavoro, r.Data.Date))
                                                    .ToDictionary(g => g.Key, g => g.First());
            var orari = OrariGiorno(rapporti, dal, ultimo);

            // ritardi per dipendente: si segnalano solo se ricorrenti
            var ritardi = new Dictionary<int, List<(AnomaliaGiornoModel Giorno, int Minuti)>>();

            foreach (var rapporto in rapporti)
            {
                if (!anagrafiche.TryGetValue(rapporto.IdDip_Anagrafica, out var dip))
                    continue;
                var inizio = rapporto.DataAss.HasValue && rapporto.DataAss.Value.Date > dal ? rapporto.DataAss.Value.Date : dal;
                var fine = rapporto.DataLic.HasValue && rapporto.DataLic.Value.Date < ultimo ? rapporto.DataLic.Value.Date : ultimo;

                for (var giorno = inizio; giorno <= fine; giorno = giorno.AddDays(1))
                {
                    var chiave = (rapporto.Id, giorno);
                    risultati.TryGetValue(chiave, out var risultato);
                    if (risultato != null && risultato.Stato.HasFlag(GG_ResultStato.Locked))
                        continue;   // mese approvato: non si corregge piu'

                    orari.TryGetValue(chiave, out var orario);
                    var delGiorno = timbrature[chiave].ToList();
                    var valide = delGiorno.Where(t => StatiValidi.Contains(t.RichiestaStato)).Select(t => t.Timbratura).OrderBy(t => t).ToList();
                    var giustDelGiorno = giustificativi[chiave].ToList();
                    bool giustValido = giustDelGiorno.Any(g => StatiValidi.Contains(g.RichiestaStato));
                    bool inAttesa = delGiorno.Any(t => StatiInAttesa.Contains(t.RichiestaStato)) || giustDelGiorno.Any(g => StatiInAttesa.Contains(g.RichiestaStato));
                    bool calcolato = risultato != null && !risultato.Stato.HasFlag(GG_ResultStato.Init);   // Init = non ancora ricalcolata
                    int teo = calcolato ? (int)risultato!.HH_Teo.ToTimeSpan().TotalMinutes : orario?.MinTeo ?? 0;
                    bool aCoppie = orario?.Orario.TimbratureTipo == OrarioTimbratureTipo.IntervalloOrario;

                    var g = new AnomaliaGiornoModel
                    {
                        IdDip_RapportoLavoro = rapporto.Id,
                        IdDip_Anagrafica = dip.Id,
                        IdAspNetUsers = dip.IdAspNetUsers,
                        NomeDipendente = dip.Nome,
                        Data = giorno,
                        MinTeo = teo,
                        Timbrature = valide.Select(t => t.ToString("HH:mm")).ToList(),
                    };
                    retVal.GiornateAnalizzate++;

                    bool dispari = aCoppie && valide.Count % 2 == 1;
                    if (dispari)
                        g.Aggiungi(AnomaliaTipo.TimbratureDispari, AnomaliaGravita.Errore, valide.Count);

                    bool vuota = teo > 0 && valide.Count == 0 && !giustValido && !inAttesa;
                    if (vuota)
                        g.Aggiungi(AnomaliaTipo.GiornataVuota, AnomaliaGravita.Errore, Ore(teo));

                    if (inAttesa)
                        g.Aggiungi(AnomaliaTipo.RichiestaInAttesa, AnomaliaGravita.Errore);

                    if (!calcolato)
                    {
                        if (teo > 0 || valide.Count > 0)
                            g.Aggiungi(AnomaliaTipo.NonCalcolata, AnomaliaGravita.Errore);
                    }
                    else if (risultato!.Stato.HasFlag(GG_ResultStato.Err))
                    {
                        // dettagli dell'errore del motore non gia' spiegati dalle regole precedenti
                        var cause = new List<string>();
                        if (risultato.Stato.HasFlag(GG_ResultStato.Err_TimbratureMancanti) && !dispari && !vuota)
                            cause.Add(AttendanceTrackingMessages.Get("Anomalia_ErrTimbratureMancanti"));
                        if (risultato.Stato.HasFlag(GG_ResultStato.Err_2) && !vuota)
                            cause.Add(AttendanceTrackingMessages.Get("Anomalia_ErrOreMancanti"));
                        if (risultato.Stato.HasFlag(GG_ResultStato.Err_4))
                            cause.Add(AttendanceTrackingMessages.Get("Anomalia_ErrFormule"));
                        if (risultato.Stato.HasFlag(GG_ResultStato.Err_5))
                            cause.Add(AttendanceTrackingMessages.Get("Anomalia_ErrSequenza"));
                        bool spiegato = dispari || vuota;
                        if (cause.Count > 0 || !spiegato)
                            g.Aggiungi(AnomaliaTipo.ErroreCalcolo, AnomaliaGravita.Errore,
                                       cause.Count > 0 ? string.Join(", ", cause) : AttendanceTrackingMessages.Get("Anomalia_ErrGenerico"));
                    }

                    if (valide.Count > 0 && orario != null && teo == 0)
                        g.Aggiungi(AnomaliaTipo.GiornoNonLavorativo, AnomaliaGravita.Avviso, string.Join(", ", g.Timbrature));

                    if (aCoppie && orario!.Ancore.Count > 0 && valide.Count > 0 && teo > 0)
                    {
                        var prima = orario.Ancore[0];
                        var ultima = orario.Ancore[^1];
                        var finestraDal = prima.Dal < prima.Ora ? prima.Dal : prima.Ora.AddMinutes(-ToleranzaFuoriOrarioMin);
                        var finestraAl = ultima.Al > ultima.Ora ? ultima.Al : ultima.Ora.AddMinutes(ToleranzaFuoriOrarioMin);
                        var fuori = valide.Where(t => t < finestraDal || t > finestraAl).ToList();
                        if (fuori.Count > 0)
                            g.Aggiungi(AnomaliaTipo.FuoriOrario, AnomaliaGravita.Avviso,
                                       string.Join(", ", fuori.Select(t => t.ToString("HH:mm"))), finestraDal.ToString("HH:mm"), finestraAl.ToString("HH:mm"));

                        // ritardo: prima timbratura oltre l'inizio della prima coppia (limite destro o tolleranza)
                        var entrata = orario.Ancore.FirstOrDefault(a => a.Entrata);
                        if (entrata != null)
                        {
                            var limite = entrata.Al > entrata.Ora ? entrata.Al : entrata.Ora.AddMinutes(ToleranzaRitardoMin);
                            var primaTimbratura = valide[0];
                            if (primaTimbratura > limite && primaTimbratura < finestraAl)
                            {
                                if (!ritardi.TryGetValue(dip.Id, out var lista))
                                    ritardi[dip.Id] = lista = new();
                                lista.Add((g, (int)(primaTimbratura - entrata.Ora).TotalMinutes));
                            }
                        }
                    }

                    if (aCoppie && valide.Count >= 2 && valide.Count % 2 == 0)
                        Pause(g, valide, orario!, risultato?.Stato, calcolato);

                    if (g.Anomalie.Count > 0)
                        retVal.Giornate.Add(g);
                }
            }

            // ritardi ricorrenti: il dipendente e i giorni dei ritardi
            foreach (var (idDip, lista) in ritardi.Where(r => r.Value.Count >= RitardiRicorrentiMin))
            {
                var dip = anagrafiche[idDip];
                retVal.RitardiRicorrenti.Add(new AnomaliaRitardiModel
                {
                    IdDip_Anagrafica = idDip,
                    IdAspNetUsers = dip.IdAspNetUsers,
                    NomeDipendente = dip.Nome,
                    Ritardi = lista.Count,
                    RitardoMedioMin = (int)Math.Round(lista.Average(r => r.Minuti), MidpointRounding.AwayFromZero),
                    Giorni = lista.Select(r => r.Giorno.Data).OrderBy(d => d).ToList(),
                });
                foreach (var (giorno, minuti) in lista)
                {
                    giorno.Aggiungi(AnomaliaTipo.Ritardo, AnomaliaGravita.Avviso, minuti);
                    if (!retVal.Giornate.Contains(giorno))
                        retVal.Giornate.Add(giorno);
                }
            }

            retVal.Giornate = retVal.Giornate.OrderBy(g => g.Data).ThenBy(g => g.NomeDipendente, StringComparer.CurrentCultureIgnoreCase).ToList();
            retVal.RitardiRicorrenti = retVal.RitardiRicorrenti.OrderByDescending(r => r.Ritardi).ThenBy(r => r.NomeDipendente).ToList();
            retVal.DipendentiAnalizzati = rapporti.Select(r => r.IdDip_Anagrafica).Distinct().Count();
            retVal.GiornateDaSistemare = retVal.Giornate.Count(g => g.DaSistemare);
            retVal.GiornateDaVerificare = retVal.Giornate.Count(g => !g.DaSistemare);
            retVal.DipendentiCoinvolti = retVal.Giornate.Select(g => g.IdDip_Anagrafica).Distinct().Count();
            retVal.Conteggi = retVal.Giornate.SelectMany(g => g.Anomalie)
                                             .GroupBy(a => a.Tipo)
                                             .Select(x => new AnomaliaConteggioModel
                                             {
                                                 Tipo = x.Key,
                                                 Gravita = x.First().Gravita,
                                                 Numero = x.Count(),
                                                 Descrizione = AttendanceTrackingMessages.Get("AnomaliaTipo_" + x.Key)
                                             })
                                             .OrderByDescending(x => x.Gravita).ThenByDescending(x => x.Numero)
                                             .ToList();
            return retVal;
        }

        // pause: con pause configurate nell'orario valgono gli avvisi del motore, altrimenti le soglie fisse
        private static void Pause(AnomaliaGiornoModel g, List<DateTime> valide, OrarioGiorno orario, GG_ResultStato? stato, bool calcolato)
        {
            if (orario.Pause.Count > 0)
            {
                if (calcolato && stato!.Value.HasFlag(GG_ResultStato.Warning_3))
                    g.Aggiungi(AnomaliaTipo.PausaMancante, AnomaliaGravita.Avviso, AttendanceTrackingMessages.Get("Anomalia_PausaDaOrario"));
                if (calcolato && stato!.Value.HasFlag(GG_ResultStato.Warning_4))
                    g.Aggiungi(AnomaliaTipo.PausaEccessiva, AnomaliaGravita.Avviso, AttendanceTrackingMessages.Get("Anomalia_PausaDaOrario"));
                return;
            }

            // segmenti lavorati a coppie (entrata-uscita) e stacchi tra un'uscita e il rientro
            int continuo = 0, maxContinuo = 0, maxStacco = 0;
            for (int i = 0; i + 1 < valide.Count; i += 2)
            {
                int durata = (int)(valide[i + 1] - valide[i]).TotalMinutes;
                if (i >= 2)
                {
                    int stacco = (int)(valide[i] - valide[i - 1]).TotalMinutes;
                    maxStacco = Math.Max(maxStacco, stacco);
                    if (stacco >= PausaMinimaMin)
                        continuo = 0;
                }
                continuo += durata;
                maxContinuo = Math.Max(maxContinuo, continuo);
            }
            if (maxContinuo > LavoroContinuoMaxMin)
                g.Aggiungi(AnomaliaTipo.PausaMancante, AnomaliaGravita.Avviso, AttendanceTrackingMessages.Get("Anomalia_LavoroContinuo", Ore(maxContinuo)));
            if (maxStacco > PausaMassimaMin)
                g.Aggiungi(AnomaliaTipo.PausaEccessiva, AnomaliaGravita.Avviso, AttendanceTrackingMessages.Get("Anomalia_StaccoLungo", Ore(maxStacco)));
        }

        // =====================================================================================
        // Previsione delle assenze (AT-AI.3)
        // =====================================================================================

        /// <summary>
        /// Previsione per i giorni da oggi a oggi+giorni-1: tassi di assenza per giorno della settimana e fattore del mese
        /// sullo storico degli ultimi 365 giorni, massimo con le assenze gia' pianificate (piano ferie).
        /// </summary>
        public PrevisioneRisultato Prevedi(IReadOnlyCollection<int> idDip_Anagrafica, DateTime oggi, int giorni)
        {
            oggi = oggi.Date;
            giorni = Math.Clamp(giorni, 1, 92);
            var retVal = new PrevisioneRisultato
            {
                Dal = oggi,
                Al = oggi.AddDays(giorni - 1),
                StoricoDal = oggi.AddDays(-StoricoGiorni),
                StoricoAl = oggi.AddDays(-1),
                SogliaPiccoPerc = SogliaPiccoPerc * 100,
                MinimoGiornate = MinimoGiornateTasso,
            };
            if (idDip_Anagrafica.Count == 0)
                return retVal;

            var anagrafiche = Anagrafiche(idDip_Anagrafica);

            // ── storico ──
            var rapportiStorico = Rapporti(idDip_Anagrafica, retVal.StoricoDal, retVal.StoricoAl).Select(r => r.Id).ToList();
            var storico = _dip_GG_ResultRepository.FindAll(r => rapportiStorico.Contains(r.IdDip_RapportoLavoro)
                                                               && r.Data >= retVal.StoricoDal && r.Data < oggi)
                                                  .Select(r => new { r.Data, r.HH_Teo, r.Min_Assenze, r.Stato })
                                                  .ToList()
                                                  .Where(r => !r.Stato.HasFlag(GG_ResultStato.Init) && r.HH_Teo.ToTimeSpan().TotalMinutes > 0)
                                                  .Select(r => new
                                                  {
                                                      r.Data,
                                                      Quota = Math.Min(1m, (decimal)Math.Max(0, r.Min_Assenze) / (decimal)r.HH_Teo.ToTimeSpan().TotalMinutes)
                                                  })
                                                  .ToList();

            retVal.GiornateStorico = storico.Count;
            decimal media = storico.Count > 0 ? storico.Sum(s => s.Quota) / storico.Count : 0m;
            retVal.TassoMedio = Math.Round(media * 100, 2);

            var tassoGiorno = new Dictionary<int, decimal>();
            for (int g = 1; g <= 7; g++)
            {
                var delGiorno = storico.Where(s => NumGiorno(s.Data) == g).ToList();
                bool usato = delGiorno.Count >= MinimoGiornateTasso;
                decimal tasso = usato ? delGiorno.Sum(s => s.Quota) / delGiorno.Count : media;
                tassoGiorno[g] = tasso;
                retVal.TassiSettimana.Add(new PrevisioneTassoModel { Valore = g, Giornate = delGiorno.Count, Tasso = Math.Round(tasso * 100, 2), DaStorico = usato });
            }
            var fattoreMese = new Dictionary<int, decimal>();
            for (int m = 1; m <= 12; m++)
            {
                var delMese = storico.Where(s => s.Data.Month == m).ToList();
                bool usato = delMese.Count >= MinimoGiornateTasso && media > 0;
                decimal fattore = usato ? (delMese.Sum(s => s.Quota) / delMese.Count) / media : 1m;
                fattoreMese[m] = fattore;
                retVal.FattoriMese.Add(new PrevisioneTassoModel { Valore = m, Giornate = delMese.Count, Tasso = Math.Round(fattore, 3), DaStorico = usato });
            }

            // ── giorni previsti ──
            var rapporti = Rapporti(idDip_Anagrafica, retVal.Dal, retVal.Al);
            var idRapporti = rapporti.Select(r => r.Id).ToList();
            var orari = OrariGiorno(rapporti, retVal.Dal, retVal.Al);
            var idPiano = _par_GiustificativiRepository.FindAll(p => p.VisualizzaInPianoFerie).Select(p => p.Id).ToList();
            var dopo = retVal.Al.AddDays(1);
            var statiPiano = StatiValidi.Concat(StatiInAttesa).ToArray();
            var pianificate = _dip_GG_GiustificativiRepository.FindAll(g => idRapporti.Contains(g.IdDip_RapportoLavoro)
                                                                           && idPiano.Contains(g.IdPar_Giustificativi)
                                                                           && statiPiano.Contains(g.RichiestaStato)
                                                                           && g.Data >= retVal.Dal && g.Data < dopo)
                                                             .Select(g => new { g.IdDip_RapportoLavoro, g.Data })
                                                             .ToList()
                                                             .ToLookup(g => g.Data.Date, g => g.IdDip_RapportoLavoro);

            for (var giorno = retVal.Dal; giorno <= retVal.Al; giorno = giorno.AddDays(1))
            {
                var p = new PrevisioneGiornoModel { Data = giorno };
                decimal attese = 0m;
                var assentiPianificati = pianificate[giorno].ToHashSet();
                var nomi = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
                var contati = new HashSet<int>();
                foreach (var r in rapporti)
                {
                    if ((r.DataAss.HasValue && r.DataAss.Value.Date > giorno) || (r.DataLic.HasValue && r.DataLic.Value.Date < giorno))
                        continue;
                    if (!orari.TryGetValue((r.Id, giorno), out var orario) || orario.MinTeo <= 0)
                        continue;   // giorno non lavorativo per il dipendente
                    if (!contati.Add(r.IdDip_Anagrafica))
                        continue;   // un dipendente conta una volta
                    p.InServizio++;
                    attese += tassoGiorno[NumGiorno(giorno)] * fattoreMese[giorno.Month];
                    if (assentiPianificati.Contains(r.Id))
                    {
                        p.Pianificate++;
                        if (anagrafiche.TryGetValue(r.IdDip_Anagrafica, out var dip))
                            nomi.Add(dip.Nome);
                    }
                }
                p.Statistiche = Math.Round(attese, 1, MidpointRounding.AwayFromZero);
                p.Previste = Math.Max(p.Pianificate, p.Statistiche);
                p.Pianificati = nomi.ToList();
                retVal.Giorni.Add(p);
            }

            var lavorativi = retVal.Giorni.Where(g => g.InServizio > 0).ToList();
            decimal mediaPianificate = lavorativi.Count > 0 ? (decimal)lavorativi.Sum(g => g.Pianificate) / lavorativi.Count : 0m;
            foreach (var g in lavorativi)
                g.Picco = g.Previste / g.InServizio >= SogliaPiccoPerc
                          || (g.Pianificate >= 2 && mediaPianificate > 0 && g.Pianificate >= 2 * mediaPianificate);
            retVal.MediaPreviste = lavorativi.Count > 0 ? Math.Round(lavorativi.Average(g => g.Previste), 1, MidpointRounding.AwayFromZero) : 0m;
            return retVal;
        }

        // =====================================================================================
        // dati comuni
        // =====================================================================================

        private sealed record Dipendente(int Id, string IdAspNetUsers, string Nome);

        private Dictionary<int, Dipendente> Anagrafiche(IReadOnlyCollection<int> ids) =>
            _dip_AnagraficaRepository.FindAll(a => ids.Contains(a.Id))
                                     .Select(a => new { a.Id, a.IdAspNetUsers, a.Cognome, a.Nome })
                                     .ToList()
                                     .ToDictionary(a => a.Id, a => new Dipendente(a.Id, a.IdAspNetUsers ?? string.Empty, NomeDipendente(a.Cognome, a.Nome)));

        public static string NomeDipendente(string? cognome, string? nome)
        {
            var testo = $"{cognome} {nome}".Trim();
            return string.IsNullOrEmpty(testo) ? "-" : testo;
        }

        private List<Dip_RapportoLavoro> Rapporti(IReadOnlyCollection<int> idDip_Anagrafica, DateTime dal, DateTime al) =>
            _dip_RapportoLavoroRepository.FindAll(r => idDip_Anagrafica.Contains(r.IdDip_Anagrafica)
                                                       && (r.DataAss == null || r.DataAss <= al)
                                                       && (r.DataLic == null || r.DataLic >= dal))
                                         .ToList();

        // Convenzione dei profili: 1=Lun ... 7=Dom
        private static int NumGiorno(DateTime d) => d.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)d.DayOfWeek;

        private static string Ore(int minuti) => $"{minuti / 60}:{Math.Abs(minuti % 60):D2}";

        /// <summary>orario del giorno di un rapporto: testata, ancore delle coppie, pause, minuti teorici</summary>
        public sealed class OrarioGiorno
        {
            public Par_Orario Orario { get; init; } = null!;
            public List<GestionePresenzeSequenza.Ancora> Ancore { get; init; } = new();
            public List<Par_OrarioPausaModel> Pause { get; init; } = new();
            public int MinTeo { get; init; }
        }

        /*
         Orario di ogni (rapporto, giorno): stessa regola del motore (TimeSheet_EngineService.Get_OrariSchema_4User) e di
         GestionePresenzeOreTeoriche: profilo del rapporto attivo nel giorno, giorno del ciclo, orario con ZOrder piu' basso,
         orario festivo nelle festivita' del calendario; teorico = somma delle coppie piu' la variazione della pausa.
        */
        private Dictionary<(int, DateTime), OrarioGiorno> OrariGiorno(List<Dip_RapportoLavoro> rapporti, DateTime dal, DateTime al)
        {
            var retVal = new Dictionary<(int, DateTime), OrarioGiorno>();
            var idRapporti = rapporti.Select(r => r.Id).ToList();
            if (idRapporti.Count == 0)
                return retVal;

            var profili = _dip_ProfiloOrarioRepository.FindAll(p => idRapporti.Contains(p.IdDip_RapportoLavoro) && p.IdPar_ProfiloOrario != null
                                                                    && p.Dal <= al && p.Al >= dal)
                                                      .ToList();
            var idProfili = profili.Select(p => p.IdPar_ProfiloOrario!.Value).Distinct().ToList();
            var parProfili = _par_ProfiloOrarioRepository.FindAll(p => idProfili.Contains(p.Id)).ToList().ToDictionary(p => p.Id);
            var parProfiliGG = _par_ProfiloOrarioGGRepository.FindAll(g => idProfili.Contains(g.IdPar_ProfiloOrario)).ToList();
            var idOrari = parProfiliGG.Select(g => g.IdPar_Orario).Concat(parProfili.Values.Select(p => p.IdPar_Orario_Festivo)).Distinct().ToList();
            var orari = _par_OrarioRepository.FindAll(o => idOrari.Contains(o.Id)).ToList().ToDictionary(o => o.Id);
            var coppie = _par_OrarioIntervalloHHRepository.FindAll(c => idOrari.Contains(c.IdPar_Orario)).ToList()
                                                          .Select(ModelloCoppia).ToLookup(c => c.IdPar_Orario);
            var pause = _par_OrarioPausaRepository.FindAll(p => idOrari.Contains(p.IdPar_Orario)).ToList()
                                                  .Select(ModelloPausa).ToLookup(p => p.IdPar_Orario);
            var idAziende = parProfili.Values.Select(p => p.IdAz_Anagrafica).Distinct().ToList();
            var festivita = _par_FestivitaRepository.FindAll(f => idAziende.Contains(f.IdAz_Anagrafica)).ToList()
                                                    .Select(f => new Par_FestivitaModel { TipoFestivita = f.TipoFestivita, Data = f.Data })
                                                    .ToList();

            foreach (var rapporto in rapporti)
            {
                foreach (var profilo in profili.Where(p => p.IdDip_RapportoLavoro == rapporto.Id))
                {
                    if (!parProfili.TryGetValue(profilo.IdPar_ProfiloOrario!.Value, out var parProfilo))
                        continue;
                    var inizio = profilo.Dal.Date > dal ? profilo.Dal.Date : dal;
                    var fine = profilo.Al.Date < al ? profilo.Al.Date : al;
                    for (var giorno = inizio; giorno <= fine; giorno = giorno.AddDays(1))
                    {
                        if (retVal.ContainsKey((rapporto.Id, giorno)))
                            continue;   // profili sovrapposti: vale il primo, come nel motore
                        int numGiorno = parProfilo.TipoProfilo == TipoProfilo.Settimanale
                            ? NumGiorno(giorno)
                            : parProfilo.NumGiorniCiclo > 0 ? ((giorno - profilo.Dal.Date).Days + profilo.NumGiornoPartenzaCiclo) % parProfilo.NumGiorniCiclo : 0;
                        var gg = parProfiliGG.Where(x => x.IdPar_ProfiloOrario == parProfilo.Id && x.NumGiorno == numGiorno)
                                             .OrderBy(x => x.ZOrder).FirstOrDefault();
                        int? idOrario = gg?.IdPar_Orario;
                        if (parProfilo.IdPar_Orario_Festivo > 0 && GestionePresenzeFestivita.IsFestivita(festivita, giorno))
                            idOrario = parProfilo.IdPar_Orario_Festivo;
                        if (idOrario == null || !orari.TryGetValue(idOrario.Value, out var orario))
                            continue;

                        var coppieOrario = coppie[orario.Id].ToList();
                        var pauseOrario = pause[orario.Id].ToList();
                        int teo;
                        var ancore = new List<GestionePresenzeSequenza.Ancora>();
                        if (orario.TimbratureTipo == OrarioTimbratureTipo.IntervalloOrario)
                        {
                            var ore = TimeSpan.Zero;
                            foreach (var c in coppieOrario.Where(c => c.Dalle.HasValue && c.Alle.HasValue))
                            {
                                var durata = c.Alle!.Value.ToTimeSpan() - c.Dalle!.Value.ToTimeSpan();
                                if (durata > TimeSpan.Zero)
                                    ore += durata;
                            }
                            if (pauseOrario.Count > 0)
                            {
                                ore += GestionePresenzePause.VariazioneTeorico(giorno, ore, coppieOrario, pauseOrario);
                                if (ore < TimeSpan.Zero)
                                    ore = TimeSpan.Zero;
                            }
                            teo = (int)ore.TotalMinutes;
                            ancore = GestionePresenzeSequenza.Ancore(giorno, coppieOrario.Select(c =>
                            {
                                c.Dalle_Use_4_Match = true;
                                c.Alle_Use_4_Match = true;
                                return c;
                            }).ToList());
                        }
                        else
                            teo = (int)orario.Hh_Teo_MonteOre.ToTimeSpan().TotalMinutes;

                        retVal[(rapporto.Id, giorno)] = new OrarioGiorno { Orario = orario, Ancore = ancore, Pause = pauseOrario, MinTeo = teo };
                    }
                }
            }
            return retVal;
        }

        // copia del modello della coppia (le ancore modificano Use_4_Match: niente oggetti condivisi tra i giorni)
        private static Par_OrarioIntervalloHHModel ModelloCoppia(Par_OrarioIntervalloHH c) => new()
        {
            IdPar_Orario = c.IdPar_Orario,
            NumCoppia = c.NumCoppia,
            Dalle = c.Dalle,
            Dalle_Limite_SX = c.Dalle_Limite_SX,
            Dalle_Limite_DX = c.Dalle_Limite_DX,
            Dalle_Use_4_Match = c.Dalle_Use_4_Match,
            Alle = c.Alle,
            Alle_Limite_SX = c.Alle_Limite_SX,
            Alle_Limite_DX = c.Alle_Limite_DX,
            Alle_Use_4_Match = c.Alle_Use_4_Match
        };

        private static Par_OrarioPausaModel ModelloPausa(Par_OrarioPausa p) => new()
        {
            Id = p.Id,
            IdPar_Orario = p.IdPar_Orario,
            NumPausa = p.NumPausa,
            Tipo = p.Tipo,
            Modalita = p.Modalita,
            DurataMin = p.DurataMin,
            DurataMaxMin = p.DurataMaxMin,
            SogliaMin = p.SogliaMin,
            FinestraDalle = p.FinestraDalle,
            FinestraAlle = p.FinestraAlle,
            RetribuitaMin = p.RetribuitaMin,
            IdCausale_Pausa = p.IdCausale_Pausa
        };
    }

    public interface IGestionePresenzeAnomalie
    {
        AnomalieRisultato Analizza(IReadOnlyCollection<int> idDip_Anagrafica, DateTime dal, DateTime al, DateTime oggi);
        PrevisioneRisultato Prevedi(IReadOnlyCollection<int> idDip_Anagrafica, DateTime oggi, int giorni);
    }

    // ────────────────────────────────────────────────────────────────────────────────── modelli

    /// <summary>tipi di anomalia (§3 dell'analisi); il testo e' nelle resx (AnomaliaTipo_*, Anomalia_*)</summary>
    public enum AnomaliaTipo
    {
        TimbratureDispari = 0,
        GiornataVuota = 1,
        ErroreCalcolo = 2,
        NonCalcolata = 3,
        RichiestaInAttesa = 4,
        FuoriOrario = 5,
        GiornoNonLavorativo = 6,
        PausaMancante = 7,
        PausaEccessiva = 8,
        Ritardo = 9,
    }

    /// <summary>Errore = da sistemare prima dell'export paghe; Avviso = da verificare</summary>
    public enum AnomaliaGravita
    {
        Avviso = 0,
        Errore = 1,
    }

    public class AnomaliaModel
    {
        public AnomaliaTipo Tipo { get; set; }
        public AnomaliaGravita Gravita { get; set; }
        public string Descrizione { get; set; } = string.Empty;
    }

    public class AnomaliaGiornoModel
    {
        public int IdDip_RapportoLavoro { get; set; }
        public int IdDip_Anagrafica { get; set; }
        public string IdAspNetUsers { get; set; } = string.Empty;
        public string NomeDipendente { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public int MinTeo { get; set; }
        /// <summary>timbrature valide del giorno (HH:mm)</summary>
        public List<string> Timbrature { get; set; } = new();
        public List<AnomaliaModel> Anomalie { get; set; } = new();
        public bool DaSistemare => Anomalie.Any(a => a.Gravita == AnomaliaGravita.Errore);

        public void Aggiungi(AnomaliaTipo tipo, AnomaliaGravita gravita, params object?[] args)
            => Anomalie.Add(new AnomaliaModel { Tipo = tipo, Gravita = gravita, Descrizione = AttendanceTrackingMessages.Get("Anomalia_" + tipo, args) });
    }

    public class AnomaliaRitardiModel
    {
        public int IdDip_Anagrafica { get; set; }
        public string IdAspNetUsers { get; set; } = string.Empty;
        public string NomeDipendente { get; set; } = string.Empty;
        public int Ritardi { get; set; }
        public int RitardoMedioMin { get; set; }
        public List<DateTime> Giorni { get; set; } = new();
    }

    public class AnomaliaConteggioModel
    {
        public AnomaliaTipo Tipo { get; set; }
        public AnomaliaGravita Gravita { get; set; }
        public int Numero { get; set; }
        public string Descrizione { get; set; } = string.Empty;
    }

    public class AnomalieRisultato
    {
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        public int DipendentiAnalizzati { get; set; }
        public int GiornateAnalizzate { get; set; }
        public int GiornateDaSistemare { get; set; }
        public int GiornateDaVerificare { get; set; }
        public int DipendentiCoinvolti { get; set; }
        public List<AnomaliaConteggioModel> Conteggi { get; set; } = new();
        public List<AnomaliaGiornoModel> Giornate { get; set; } = new();
        public List<AnomaliaRitardiModel> RitardiRicorrenti { get; set; } = new();
    }

    public class PrevisioneTassoModel
    {
        /// <summary>giorno della settimana (1=Lun ... 7=Dom) o mese (1-12)</summary>
        public int Valore { get; set; }
        public int Giornate { get; set; }
        /// <summary>tasso in % (giorno della settimana) o fattore (mese)</summary>
        public decimal Tasso { get; set; }
        /// <summary>false = poche giornate: si usa il tasso medio (o fattore 1)</summary>
        public bool DaStorico { get; set; }
    }

    public class PrevisioneGiornoModel
    {
        public DateTime Data { get; set; }
        /// <summary>dipendenti attesi al lavoro (in servizio con ore teoriche)</summary>
        public int InServizio { get; set; }
        /// <summary>assenze gia' nel piano ferie (approvate o in attesa)</summary>
        public int Pianificate { get; set; }
        /// <summary>assenze attese dalla statistica</summary>
        public decimal Statistiche { get; set; }
        /// <summary>max(pianificate, statistiche)</summary>
        public decimal Previste { get; set; }
        public bool Picco { get; set; }
        public List<string> Pianificati { get; set; } = new();
    }

    public class PrevisioneRisultato
    {
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }
        public DateTime StoricoDal { get; set; }
        public DateTime StoricoAl { get; set; }
        public int GiornateStorico { get; set; }
        public int MinimoGiornate { get; set; }
        /// <summary>tasso medio di assenza in %</summary>
        public decimal TassoMedio { get; set; }
        public decimal SogliaPiccoPerc { get; set; }
        public decimal MediaPreviste { get; set; }
        public List<PrevisioneTassoModel> TassiSettimana { get; set; } = new();
        public List<PrevisioneTassoModel> FattoriMese { get; set; } = new();
        public List<PrevisioneGiornoModel> Giorni { get; set; } = new();
    }
}
