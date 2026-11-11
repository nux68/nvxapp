using nvxapp.server.data.Entities.Tenant;
using System.Diagnostics;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.FormuleEngine
{
    /*
     Facciata "x" delle formule (ANALISI_ATTENDANCETRACKING_FORMULE.md §4.3).
     La formula vede SOLO questo oggetto: niente DB, niente altri dipendenti, niente CLR.
     Lavora sui dati in memoria del calcolo del giorno (causali e importi del dizionario del motore, result,
     timbrature, giustificativi, orario): quello che la formula cambia viene poi scritto dal motore
     (ScriviCausali / SaveData) come tutto il resto del calcolo.
     Le durate sono in MINUTI interi; causali e giustificativi si indicano con il CODICE.
     Solo i membri marcati [FormulaApi] fanno parte dell'API documentata (catalogo, AI, controllo membri).
    */

    /// <summary>Dati del giorno passati dal motore alla formula.</summary>
    public class FormulaDatiGiorno
    {
        public Dip_AnagraficaModel? Anagrafica { get; set; }
        public required Dip_RapportoLavoroModel Rapporto { get; set; }
        public DateTime Giorno { get; set; }
        public FormulaFase Fase { get; set; }
        public Dip_GG_ResultModel? Result { get; set; }
        public required Dictionary<int, TimeSpan> Causali { get; set; }
        public required Dictionary<int, decimal> Importi { get; set; }
        public List<Dip_GG_TimbraturaModel> Timbrature { get; set; } = new();
        public List<Dip_GG_GiustificativiModel> Giustificativi { get; set; } = new();
        public Par_OrarioModel? Orario { get; set; }
        public List<Par_OrarioIntervalloHHModel> Fasce { get; set; } = new();
        public Par_ProfiloOrarioModel? Profilo { get; set; }
        public bool Festivo { get; set; }
    }

    /// <summary>Codici dell'azienda: causali e giustificativi per codice (maiuscolo).</summary>
    public class FormulaCodici
    {
        public Dictionary<string, (int Id, TipoValore Tipo)> Causali { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<int, string> CodiceCausale { get; } = new();
        public Dictionary<string, int> Giustificativi { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Testo con i codici esistenti, prima quelli simili a quello sbagliato (per il messaggio d'errore).</summary>
        public static string Suggerimenti(string? sbagliato, IEnumerable<string> codici)
        {
            var tutti = codici.OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
            if (tutti.Count == 0) return "Nessun codice definito.";
            var s = (sbagliato ?? string.Empty).Trim();
            var pref = s.Length >= 2 ? s[..2] : s;
            var simili = tutti.Where(c => s.Length > 0 && (c.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                                                          s.Contains(c, StringComparison.OrdinalIgnoreCase) ||
                                                          c.StartsWith(pref, StringComparison.OrdinalIgnoreCase)))
                              .ToList();
            static string Elenco(List<string> l) => string.Join(", ", l.Take(12)) + (l.Count > 12 ? ", ..." : ".");
            var altri = tutti.Except(simili).ToList();
            if (simili.Count == 0) return "Codici disponibili: " + Elenco(tutti);
            return "Forse: " + Elenco(simili) + (altri.Count > 0 ? " Altri codici: " + Elenco(altri) : "");
        }
    }

    [DebuggerNonUserCode]
    public class FormulaContext
    {
        private readonly FormulaDatiGiorno _d;
        private readonly Action<FormulaLogTipo, string> _log;
        private readonly bool _debug;

        public FormulaContext(FormulaDatiGiorno dati, FormulaCodici codici, Action<FormulaLogTipo, string> log, bool debug)
        {
            _d = dati;
            _log = log;
            _debug = debug;
            Attiva = new FormulaAttiva(dati);
            Causale = new FormulaCausale(dati, codici);
            Giustificativo = new FormulaGiustificativo(dati, codici);
            Timbrate = new FormulaTimbrate(dati);
            Orario = new FormulaOrario(dati);
            Result = new FormulaResult(dati);
            Stato = new FormulaStato(log);
            Time = new FormulaTime();
            Rapporto = new FormulaRapporto(dati);
            Dipendente = new FormulaDipendente(dati);
        }

        [FormulaApi("Giorno in calcolo (data)", InEvidenza = true)]
        public DateTime Giorno => _d.Giorno;

        [FormulaApi("Giorno della settimana: 1 = lunedi' ... 7 = domenica")]
        public int GiornoSettimana => _d.Giorno.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)_d.Giorno.DayOfWeek;

        [FormulaApi("Giorno festivo (calendario aziendale o domenica festiva del profilo orario)", InEvidenza = true)]
        public bool Festivo => _d.Festivo || (_d.Result?.Festivo ?? false);

        [FormulaApi("Fase del calcolo in cui gira la formula: \"Giorno\" o \"Finale\"")]
        public string Fase => _d.Fase.ToString();

        [FormulaApi("Filtro di applicabilita' della formula (giorni, mesi, festivo)")] public FormulaAttiva Attiva { get; }
        [FormulaApi("Causali del giorno: lettura e modifica dei minuti e degli importi")] public FormulaCausale Causale { get; }
        [FormulaApi("Giustificativi del giorno (sola lettura)")] public FormulaGiustificativo Giustificativo { get; }
        [FormulaApi("Timbrature del giorno (sola lettura)")] public FormulaTimbrate Timbrate { get; }
        [FormulaApi("Orario teorico del giorno")] public FormulaOrario Orario { get; }
        [FormulaApi("Totali del giorno calcolati dal motore (sola lettura)")] public FormulaResult Result { get; }
        [FormulaApi("Avvisi e anomalie sul giorno")] public FormulaStato Stato { get; }
        [FormulaApi("Funzioni su minuti e ore")] public FormulaTime Time { get; }
        [FormulaApi("Dati del rapporto di lavoro")] public FormulaRapporto Rapporto { get; }
        [FormulaApi("Dati del dipendente")] public FormulaDipendente Dipendente { get; }

        [FormulaApi("Scrive un messaggio nel log della formula", InEvidenza = true, Firma = "Log(messaggio)")]
        public void Log(object? messaggio) => _log(FormulaLogTipo.Info, messaggio?.ToString() ?? string.Empty);

        [FormulaApi("Scrive un messaggio di debug: solo con \"Prova formula\"", Firma = "Debug(messaggio)")]
        public void Debug(object? messaggio)
        {
            if (_debug)
                _log(FormulaLogTipo.Debug, messaggio?.ToString() ?? string.Empty);
        }

        internal bool WarningAlzato => Stato.WarningAlzato;
        internal bool AnomaliaAlzata => Stato.AnomaliaAlzata;
    }

    // ── x.Attiva ─────────────────────────────────────────────────────────────────────
    // Come il legacy: Sempre(true) applica sempre; altrimenti devono valere giorno della settimana, mese e festivo.
    // Un gruppo senza nessuna scelta vale "tutti" (nel legacy andavano impostati tutti a true).
    [DebuggerNonUserCode]
    public class FormulaAttiva
    {
        private readonly FormulaDatiGiorno _d;
        private bool _sempre;
        private readonly bool[] _giorni = new bool[8];   // 1..7
        private readonly bool[] _mesi = new bool[13];    // 1..12
        private bool? _festivo, _nonFestivo;

        public FormulaAttiva(FormulaDatiGiorno d) { _d = d; }

        [FormulaApi("Applica sempre la formula (le altre scelte vengono ignorate)", InEvidenza = true, Firma = "Sempre(true)")]
        public void Sempre(bool v) => _sempre = v;
        [FormulaApi("Applica il lunedi'", Firma = "Lun(true)")] public void Lun(bool v) => _giorni[1] = v;
        [FormulaApi("Applica il martedi'", Firma = "Mar(true)")] public void Mar(bool v) => _giorni[2] = v;
        [FormulaApi("Applica il mercoledi'", Firma = "Mer(true)")] public void Mer(bool v) => _giorni[3] = v;
        [FormulaApi("Applica il giovedi'", Firma = "Gio(true)")] public void Gio(bool v) => _giorni[4] = v;
        [FormulaApi("Applica il venerdi'", Firma = "Ven(true)")] public void Ven(bool v) => _giorni[5] = v;
        [FormulaApi("Applica il sabato", Firma = "Sab(true)")] public void Sab(bool v) => _giorni[6] = v;
        [FormulaApi("Applica la domenica", Firma = "Dom(true)")] public void Dom(bool v) => _giorni[7] = v;
        [FormulaApi("Applica nel mese indicato (1-12)", Firma = "Mese(numero, true)")] public void Mese(int mese, bool v) { if (mese >= 1 && mese <= 12) _mesi[mese] = v; }
        [FormulaApi("Applica nei giorni festivi", Firma = "Festivo(true)")] public void Festivo(bool v) => _festivo = v;
        [FormulaApi("Applica nei giorni non festivi", Firma = "NonFestivo(true)")] public void NonFestivo(bool v) => _nonFestivo = v;

        [FormulaApi("true se la formula va applicata al giorno in calcolo", InEvidenza = true, Firma = "ApplicaLaFormula()")]
        public bool ApplicaLaFormula()
        {
            if (_sempre) return true;
            int gs = _d.Giorno.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)_d.Giorno.DayOfWeek;
            bool giornoOk = !_giorni.Any(x => x) || _giorni[gs];
            bool meseOk = !_mesi.Any(x => x) || _mesi[_d.Giorno.Month];
            bool festivo = _d.Festivo || (_d.Result?.Festivo ?? false);
            bool festOk = (_festivo == null && _nonFestivo == null) || (festivo ? _festivo == true : _nonFestivo == true);
            return giornoOk && meseOk && festOk;
        }
    }

    // ── x.Causale ────────────────────────────────────────────────────────────────────
    [DebuggerNonUserCode]
    public class FormulaCausale
    {
        private readonly FormulaDatiGiorno _d;
        private readonly FormulaCodici _c;

        public FormulaCausale(FormulaDatiGiorno d, FormulaCodici c) { _d = d; _c = c; }

        private (int Id, TipoValore Tipo) Trova(string? codice)
        {
            if (string.IsNullOrWhiteSpace(codice) || !_c.Causali.TryGetValue(codice.Trim(), out var c))
                throw new FormulaException($"Causale '{codice}' inesistente. {FormulaCodici.Suggerimenti(codice, _c.Causali.Keys)}");
            return c;
        }

        private int IdOrario(string? codice)
        {
            var c = Trova(codice);
            if (c.Tipo == TipoValore.Importo)
                throw new FormulaException($"La causale '{codice}' e' a importo: usare GetImporto/SetImporto");
            return c.Id;
        }

        [FormulaApi("Minuti della causale nel giorno (0 se assente)", InEvidenza = true, Firma = "GetMin(codice)")]
        public int GetMin(string codice)
        {
            var id = IdOrario(codice);
            return _d.Causali.TryGetValue(id, out var v) ? (int)Math.Round(v.TotalMinutes) : 0;
        }

        [FormulaApi("Imposta i minuti della causale (0 = la toglie)", InEvidenza = true, Firma = "SetMin(codice, minuti)")]
        public void SetMin(string codice, int minuti)
        {
            var id = IdOrario(codice);
            if (minuti <= 0) _d.Causali.Remove(id);
            else _d.Causali[id] = TimeSpan.FromMinutes(minuti);
        }

        [FormulaApi("Aggiunge minuti alla causale (negativi = toglie, senza scendere sotto 0)", Firma = "AddMin(codice, minuti)")]
        public void AddMin(string codice, int minuti) => SetMin(codice, Math.Max(0, GetMin(codice) + minuti));

        [FormulaApi("Sposta tutti i minuti da una causale a un'altra", InEvidenza = true, Firma = "Sposta(da, a)")]
        public void Sposta(string da, string a) => Sposta(da, a, GetMin(da));

        [FormulaApi("Sposta al massimo i minuti indicati da una causale a un'altra", Firma = "Sposta(da, a, minuti)")]
        public void Sposta(string da, string a, int minuti)
        {
            int m = Math.Min(Math.Max(0, minuti), GetMin(da));
            if (m <= 0) return;
            SetMin(da, GetMin(da) - m);
            AddMin(a, m);
        }

        [FormulaApi("Copia tutti i minuti di una causale su un'altra (si sommano)", Firma = "Copia(da, a)")]
        public void Copia(string da, string a) => AddMin(a, GetMin(da));

        [FormulaApi("Copia al massimo i minuti indicati di una causale su un'altra", Firma = "Copia(da, a, minuti)")]
        public void Copia(string da, string a, int minuti) => AddMin(a, Math.Min(Math.Max(0, minuti), GetMin(da)));

        [FormulaApi("Lascia sulla causale al massimo maxMinuti e sposta il resto su un'altra (es. ORDI oltre 4 ore su STR1)", InEvidenza = true, Firma = "TogliEccedenza(codice, maxMinuti, codiceDestinazione)")]
        public void TogliEccedenza(string codice, int maxMinuti, string codiceDestinazione)
        {
            int ecc = GetMin(codice) - Math.Max(0, maxMinuti);
            if (ecc > 0) Sposta(codice, codiceDestinazione, ecc);
        }

        [FormulaApi("true se la causale e' presente nel giorno (minuti o importo)", Firma = "Esiste(codice)")]
        public bool Esiste(string codice)
        {
            var c = Trova(codice);
            return c.Tipo == TipoValore.Importo
                ? _d.Importi.TryGetValue(c.Id, out var i) && i != 0
                : _d.Causali.TryGetValue(c.Id, out var v) && v > TimeSpan.Zero;
        }

        [FormulaApi("Codici delle causali presenti nel giorno", Firma = "Elenco()")]
        public string[] Elenco()
            => _d.Causali.Where(x => x.Value > TimeSpan.Zero).Select(x => x.Key)
                         .Concat(_d.Importi.Where(x => x.Value != 0).Select(x => x.Key))
                         .Distinct()
                         .Select(id => _c.CodiceCausale.TryGetValue(id, out var cod) ? cod : id.ToString())
                         .ToArray();

        [FormulaApi("Importo della causale a importo (0 se assente)", Firma = "GetImporto(codice)")]
        public double GetImporto(string codice)
        {
            var c = Trova(codice);
            return _d.Importi.TryGetValue(c.Id, out var v) ? (double)v : 0;
        }

        [FormulaApi("Imposta l'importo della causale a importo (0 = lo toglie)", Firma = "SetImporto(codice, importo)")]
        public void SetImporto(string codice, double importo)
        {
            var c = Trova(codice);
            if (c.Tipo != TipoValore.Importo)
                throw new FormulaException($"La causale '{codice}' e' a ore: usare GetMin/SetMin");
            if (importo == 0) _d.Importi.Remove(c.Id);
            else _d.Importi[c.Id] = Math.Round((decimal)importo, 2);
        }
    }

    // ── x.Giustificativo ─────────────────────────────────────────────────────────────
    [DebuggerNonUserCode]
    public class FormulaGiustificativo
    {
        private readonly FormulaDatiGiorno _d;
        private readonly FormulaCodici _c;

        public FormulaGiustificativo(FormulaDatiGiorno d, FormulaCodici c) { _d = d; _c = c; }

        private IEnumerable<Dip_GG_GiustificativiModel> DelCodice(string? codice)
        {
            if (string.IsNullOrWhiteSpace(codice) || !_c.Giustificativi.TryGetValue(codice.Trim(), out var id))
                throw new FormulaException($"Giustificativo '{codice}' inesistente. {FormulaCodici.Suggerimenti(codice, _c.Giustificativi.Keys)}");
            return _d.Giustificativi.Where(g => g.IdPar_Giustificativi == id);
        }

        [FormulaApi("Minuti del giustificativo nel giorno (0 se assente)", InEvidenza = true, Firma = "GetMin(codice)")]
        public int GetMin(string codice) => (int)Math.Round(DelCodice(codice).Sum(g => g.Hours?.TotalMinutes ?? 0));

        [FormulaApi("Importo del giustificativo a importo (0 se assente)", Firma = "GetImporto(codice)")]
        public double GetImporto(string codice) => (double)DelCodice(codice).Sum(g => g.Importo ?? 0);

        [FormulaApi("true se il giustificativo e' presente nel giorno", Firma = "Esiste(codice)")]
        public bool Esiste(string codice) => DelCodice(codice).Any();

        [FormulaApi("Codici dei giustificativi presenti nel giorno", Firma = "Elenco()")]
        public string[] Elenco()
        {
            var perId = _c.Giustificativi.ToDictionary(x => x.Value, x => x.Key);
            return _d.Giustificativi.Select(g => perId.TryGetValue(g.IdPar_Giustificativi, out var cod) ? cod : g.IdPar_Giustificativi.ToString())
                                    .Distinct().ToArray();
        }
    }

    // ── x.Timbrate ───────────────────────────────────────────────────────────────────
    public class FormulaTimbratura
    {
        [FormulaApi("Ora della timbratura arrotondata, in minuti da mezzanotte del giorno (oltre 1440 se del giorno dopo)")]
        public int Ora { get; init; }
        [FormulaApi("Verso: \"E\" entrata, \"U\" uscita, \"\" senza verso")]
        public string Verso { get; init; } = string.Empty;
    }

    [DebuggerNonUserCode]
    public class FormulaTimbrate
    {
        private readonly List<FormulaTimbratura> _t;

        public FormulaTimbrate(FormulaDatiGiorno d)
        {
            _t = d.Timbrature.Where(t => t.TimbraturaArrotondata.HasValue)
                             .OrderBy(t => t.TimbraturaArrotondata)
                             .Select(t => new FormulaTimbratura
                             {
                                 Ora = (int)Math.Round((t.TimbraturaArrotondata!.Value - d.Giorno.Date).TotalMinutes),
                                 Verso = t.TimbraturaTipo == TipoTimbratura.Entrata ? "E" : t.TimbraturaTipo == TipoTimbratura.Uscita ? "U" : ""
                             })
                             .ToList();
        }

        [FormulaApi("Numero di timbrature del giorno", InEvidenza = true)]
        public int Count => _t.Count;

        [FormulaApi("Timbratura i-esima (da 0): { Ora, Verso }", Firma = "Get(indice)")]
        public FormulaTimbratura Get(int indice)
        {
            if (indice < 0 || indice >= _t.Count)
                throw new FormulaException($"Timbratura {indice} inesistente (il giorno ne ha {_t.Count})");
            return _t[indice];
        }

        [FormulaApi("Ora della prima timbratura in minuti (-1 se nessuna)", Firma = "Prima()")]
        public int Prima() => _t.Count > 0 ? _t[0].Ora : -1;

        [FormulaApi("Ora dell'ultima timbratura in minuti (-1 se nessuna)", Firma = "Ultima()")]
        public int Ultima() => _t.Count > 0 ? _t[^1].Ora : -1;

        [FormulaApi("Minuti lavorati tra le coppie di timbrature (entrata-uscita in sequenza)", InEvidenza = true, Firma = "MinutiLavorati()")]
        public int MinutiLavorati() => MinutiLavoratiTra(int.MinValue / 2, int.MaxValue / 2);

        [FormulaApi("Minuti lavorati dentro la fascia [dalle, alle] (minuti da mezzanotte)", Firma = "MinutiLavoratiTra(dalle, alle)")]
        public int MinutiLavoratiTra(int dalle, int alle)
        {
            int tot = 0;
            for (int i = 0; i + 1 < _t.Count; i += 2)
            {
                int inizio = Math.Max(_t[i].Ora, dalle);
                int fine = Math.Min(_t[i + 1].Ora, alle);
                if (fine > inizio) tot += fine - inizio;
            }
            return tot;
        }
    }

    // ── x.Orario ─────────────────────────────────────────────────────────────────────
    public class FormulaFascia
    {
        [FormulaApi("Inizio fascia in minuti da mezzanotte")] public int Dalle { get; init; }
        [FormulaApi("Fine fascia in minuti da mezzanotte")] public int Alle { get; init; }
    }

    [DebuggerNonUserCode]
    public class FormulaOrario
    {
        private readonly FormulaDatiGiorno _d;
        public FormulaOrario(FormulaDatiGiorno d) { _d = d; }

        [FormulaApi("Codice dell'orario del giorno (\"\" se nessuno)")]
        public string Codice => _d.Orario?.Codice ?? string.Empty;

        [FormulaApi("Minuti teorici del giorno", InEvidenza = true)]
        public int MinTeorici => _d.Result != null ? (int)_d.Result.HH_Teo.ToTimeSpan().TotalMinutes : 0;

        [FormulaApi("Fasce dell'orario: elenco di { Dalle, Alle } in minuti")]
        public FormulaFascia[] Fasce => _d.Fasce.Where(f => f.Dalle.HasValue && f.Alle.HasValue)
                                                .Select(f => new FormulaFascia
                                                {
                                                    Dalle = (int)f.Dalle!.Value.ToTimeSpan().TotalMinutes,
                                                    Alle = (int)f.Alle!.Value.ToTimeSpan().TotalMinutes
                                                }).ToArray();

        [FormulaApi("Codice del profilo orario del giorno (\"\" se nessuno)")]
        public string ProfiloCodice => _d.Profilo?.Codice ?? string.Empty;
    }

    // ── x.Result ─────────────────────────────────────────────────────────────────────
    [DebuggerNonUserCode]
    public class FormulaResult
    {
        private readonly FormulaDatiGiorno _d;
        public FormulaResult(FormulaDatiGiorno d) { _d = d; }

        private int M(Func<Dip_GG_ResultModel, int> f) => _d.Result != null ? f(_d.Result) : 0;

        [FormulaApi("Minuti teorici")] public int HH_Teo => M(r => (int)r.HH_Teo.ToTimeSpan().TotalMinutes);
        [FormulaApi("Minuti lavorati", InEvidenza = true)] public int HH_Lav => M(r => (int)r.HH_Lav.ToTimeSpan().TotalMinutes);
        [FormulaApi("Minuti lavorabili")] public int Min_Lavorabili => M(r => r.Min_Lavorabili);
        [FormulaApi("Minuti di assenza")] public int Min_Assenze => M(r => r.Min_Assenze);
        [FormulaApi("Minuti ordinari")] public int Min_Ord => M(r => r.Min_Ord);
        [FormulaApi("Minuti in eccedenza", InEvidenza = true)] public int Min_Ecc => M(r => r.Min_Ecc);
        [FormulaApi("Minuti in eccedenza notturni")] public int Min_EccNott => M(r => r.Min_EccNott);
        [FormulaApi("Minuti di supplementare (fase Finale)")] public int Min_Suppl => M(r => r.Min_Suppl);
        [FormulaApi("Minuti di supplementare in eccedenza (fase Finale)")] public int Min_SupplEcc => M(r => r.Min_SupplEcc);
        [FormulaApi("Minuti di straordinario (fase Finale)")] public int Min_Strao => M(r => r.Min_Strao);
        [FormulaApi("Minuti di straordinario notturno (fase Finale)")] public int Min_StraoNott => M(r => r.Min_StraoNott);
        [FormulaApi("Minuti di straordinario festivo (fase Finale)")] public int Min_StraoFest => M(r => r.Min_StraoFest);
        [FormulaApi("Minuti di straordinario festivo notturno (fase Finale)")] public int Min_StraoFestNott => M(r => r.Min_StraoFestNott);
        [FormulaApi("Minuti di eccedenza pagati come ordinario (fase Finale)")] public int Min_EccOrd => M(r => r.Min_EccOrd);
        [FormulaApi("Minuti di eccedenza non riconosciuti (fase Finale)")] public int Min_NonRiconosciute => M(r => r.Min_NonRiconosciute);
        [FormulaApi("Eccedenza destinata alla banca ore (fase Finale)")] public bool BancaOre => _d.Result?.BancaOre ?? false;
    }

    // ── x.Stato ──────────────────────────────────────────────────────────────────────
    [DebuggerNonUserCode]
    public class FormulaStato
    {
        private readonly Action<FormulaLogTipo, string> _log;
        internal bool WarningAlzato { get; private set; }
        internal bool AnomaliaAlzata { get; private set; }

        public FormulaStato(Action<FormulaLogTipo, string> log) { _log = log; }

        [FormulaApi("Mette un avviso sul giorno (visibile nel cartellino) e scrive il messaggio nel log", InEvidenza = true, Firma = "Warning(messaggio)")]
        public void Warning(object? messaggio)
        {
            WarningAlzato = true;
            _log(FormulaLogTipo.Warning, messaggio?.ToString() ?? string.Empty);
        }

        [FormulaApi("Mette il giorno in anomalia (errore nel cartellino) e scrive il messaggio nel log", Firma = "Anomalia(messaggio)")]
        public void Anomalia(object? messaggio)
        {
            AnomaliaAlzata = true;
            _log(FormulaLogTipo.Errore, messaggio?.ToString() ?? string.Empty);
        }
    }

    // ── x.Time ───────────────────────────────────────────────────────────────────────
    [DebuggerNonUserCode]
    public class FormulaTime
    {
        [FormulaApi("Minuti da ore e minuti (es. Min(4, 30) = 270)", InEvidenza = true, Firma = "Min(ore, minuti)")]
        public int Min(int ore, int minuti) => ore * 60 + minuti;

        [FormulaApi("Minuti in formato \"hh:mm\"", Firma = "HHMM(minuti)")]
        public string HHMM(int minuti)
        {
            var segno = minuti < 0 ? "-" : "";
            minuti = Math.Abs(minuti);
            return $"{segno}{minuti / 60:00}:{minuti % 60:00}";
        }

        [FormulaApi("Arrotonda i minuti al passo indicato; modo: \"eccesso\", \"difetto\" o \"vicino\"", Firma = "Arrotonda(minuti, passo, modo)")]
        public int Arrotonda(int minuti, int passo, string modo)
        {
            if (passo <= 0) return minuti;
            double q = (double)minuti / passo;
            return (modo ?? "").Trim().ToLowerInvariant() switch
            {
                "eccesso" => (int)Math.Ceiling(q) * passo,
                "difetto" => (int)Math.Floor(q) * passo,
                _ => (int)Math.Round(q, MidpointRounding.AwayFromZero) * passo
            };
        }
    }

    // ── x.Rapporto / x.Dipendente ────────────────────────────────────────────────────
    [DebuggerNonUserCode]
    public class FormulaRapporto
    {
        private readonly FormulaDatiGiorno _d;
        public FormulaRapporto(FormulaDatiGiorno d) { _d = d; }

        [FormulaApi("Tipo di contratto (es. \"FullTime\", \"PartTime\")")] public string TipoContratto => _d.Rapporto.TipoContratto.ToString();
        [FormulaApi("Ore settimanali da contratto (0 se non indicate)")] public double OreSettimanali => (double)(_d.Rapporto.OreSettimanaliContratto ?? 0);
        [FormulaApi("CCNL del rapporto")] public string Ccnl => _d.Rapporto.Ccnl ?? string.Empty;
    }

    [DebuggerNonUserCode]
    public class FormulaDipendente
    {
        private readonly FormulaDatiGiorno _d;
        public FormulaDipendente(FormulaDatiGiorno d) { _d = d; }

        [FormulaApi("Matricola del dipendente")] public string Matricola => _d.Anagrafica?.Matricola ?? string.Empty;
    }
}
