using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Extensions;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    /// <summary>
    /// Interpretazione delle timbrature di un giorno con gli intervalli di riconoscimento dell'orario
    /// (ANALISI_ATTENDANCETRACKING_PAUSE.md §3.3). Funzione pura: niente DB, niente stato.
    ///
    /// Le ore attese (Dalle/Alle con *_Use_4_Match) sono le "ancore", in ordine cronologico, con il loro intervallo
    /// di riconoscimento. Passi:
    ///  1. timbrature doppie (a meno di MinutiDoppia dalla precedente) ignorate, con avviso;
    ///  2. abbinamento ordinato alle ancore dentro l'intervallo (programmazione dinamica: massimo numero di ancore,
    ///     a parita' minima distanza dall'ora attesa);
    ///  3. ancore rimaste vuote: prendono una timbratura fuori intervallo nella zona adiacente (uscita anticipata,
    ///     entrata in ritardo...), con avviso; quelle facoltative (pausa automatica) possono restare vuote in coppia;
    ///  4. timbrature rimaste: dentro una coppia sono uscite intermedie (U poi E), negli altri tratti lavoro (E poi U);
    ///     un numero dispari nello stesso tratto rende la giornata non interpretabile.
    /// </summary>
    public static class GestionePresenzeSequenza
    {
        public const int MinutiDoppia = 2;

        public sealed class Ancora
        {
            public int NumCoppia { get; init; }
            public bool Entrata { get; init; }
            public DateTime Ora { get; init; }
            public DateTime Dal { get; init; }
            public DateTime Al { get; init; }
            public TimeRoundInterval Arrotondamento { get; init; }
            public RoundDirection Arrotondamento_Verso { get; init; }
        }

        public sealed class Risultato
        {
            /// <summary>verso di ogni timbratura (stesso ordine dell'input)</summary>
            public TipoTimbratura[] Verso { get; init; } = Array.Empty<TipoTimbratura>();
            /// <summary>ancora abbinata (per l'arrotondamento), -1 se la timbratura non e' un'ancora</summary>
            public int[] IndiceAncora { get; init; } = Array.Empty<int>();
            /// <summary>timbratura doppia, esclusa dal calcolo</summary>
            public bool[] Ignorata { get; init; } = Array.Empty<bool>();
            /// <summary>ancore senza timbratura (indici in Ancore)</summary>
            public List<int> AncoreVuote { get; } = new();
            /// <summary>ancore vuote ammesse: coppie facoltative vuote entrambe (pausa automatica)</summary>
            public HashSet<int> VuoteAmmesse { get; } = new();
            public List<Ancora> Ancore { get; init; } = new();
            public bool TimbratureMancanti { get; set; }
            public bool NonInterpretabile { get; set; }
            /// <summary>doppia timbratura ignorata o ancora presa fuori dal suo intervallo</summary>
            public bool Avviso { get; set; }
        }

        /// <summary>
        /// Ancore del giorno dalle coppie dell'orario, con ore e intervalli come date complete: un'ora che precede
        /// la precedente e' del giorno dopo (turni a cavallo della mezzanotte).
        /// </summary>
        public static List<Ancora> Ancore(DateTime giorno, IEnumerable<Par_OrarioIntervalloHHModel> coppie)
        {
            var retVal = new List<Ancora>();
            DateTime? precedente = null;

            DateTime Successiva(TimeOnly ora)
            {
                var t = giorno.Date + ora.ToTimeSpan();
                while (precedente.HasValue && t < precedente.Value)
                    t = t.AddDays(1);
                precedente = t;
                return t;
            }

            Ancora Crea(int numCoppia, bool entrata, DateTime ora, TimeOnly? sx, TimeOnly? dx, TimeRoundInterval arr, RoundDirection verso)
            {
                int oraMin = (int)TimeOnly.FromDateTime(ora).ToTimeSpan().TotalMinutes;
                int prima = sx.HasValue ? Mod(oraMin - (int)sx.Value.ToTimeSpan().TotalMinutes) : 0;
                int dopo = dx.HasValue ? Mod((int)dx.Value.ToTimeSpan().TotalMinutes - oraMin) : 0;
                return new Ancora
                {
                    NumCoppia = numCoppia,
                    Entrata = entrata,
                    Ora = ora,
                    Dal = ora.AddMinutes(-prima),
                    Al = ora.AddMinutes(dopo),
                    Arrotondamento = arr,
                    Arrotondamento_Verso = verso
                };
            }

            foreach (var c in coppie.Where(c => c.Dalle.HasValue && c.Alle.HasValue && c.Dalle.Value != c.Alle.Value)
                                    .OrderBy(c => c.NumCoppia))
            {
                var dalle = Successiva(c.Dalle!.Value);
                var alle = Successiva(c.Alle!.Value);
                if (c.Dalle_Use_4_Match)
                    retVal.Add(Crea(c.NumCoppia, true, dalle, c.Dalle_Limite_SX, c.Dalle_Limite_DX, c.Dalle_Arrotondamento, c.Dalle_Arrotondamento_Verso));
                if (c.Alle_Use_4_Match)
                    retVal.Add(Crea(c.NumCoppia, false, alle, c.Alle_Limite_SX, c.Alle_Limite_DX, c.Alle_Arrotondamento, c.Alle_Arrotondamento_Verso));
            }
            return retVal;
        }

        private static int Mod(int minuti) => ((minuti % 1440) + 1440) % 1440;

        /// <param name="timbrature">orari delle timbrature valide del giorno, in ordine crescente</param>
        /// <param name="ancore">da <see cref="Ancore"/></param>
        /// <param name="facoltative">indici di ancore che possono restare vuote, a coppie (uscita k, entrata k+1)</param>
        public static Risultato Interpreta(IReadOnlyList<DateTime> timbrature, List<Ancora> ancore, IReadOnlyCollection<(int Uscita, int Entrata)>? facoltative = null)
        {
            int n = timbrature.Count, m = ancore.Count;
            var verso = Enumerable.Repeat(TipoTimbratura.SenzaVerso, n).ToArray();
            var indiceAncora = Enumerable.Repeat(-1, n).ToArray();
            var ignorata = new bool[n];
            var res = new Risultato { Verso = verso, IndiceAncora = indiceAncora, Ignorata = ignorata, Ancore = ancore };

            // 1) doppie
            for (int i = 1; i < n; i++)
            {
                int prec = i - 1;
                while (prec >= 0 && ignorata[prec]) prec--;
                if (prec >= 0 && (timbrature[i] - timbrature[prec]).TotalMinutes < MinutiDoppia)
                {
                    ignorata[i] = true;
                    res.Avviso = true;
                }
            }
            var valide = Enumerable.Range(0, n).Where(i => !ignorata[i]).ToList();

            // giorno senza ancore (es. RIPOSO): tutto lavoro fuori orario, a coppie E/U
            if (m == 0)
            {
                AccoppiaTratto(valide, verso, entrataPrima: true, res);
                return res;
            }

            // 2) abbinamento ordinato dentro gli intervalli
            var ancoraDi = AbbinaOrdinato(valide.Select(i => timbrature[i]).ToList(), ancore);
            var timbraturaDi = Enumerable.Repeat(-1, m).ToArray();     // ancora -> indice timbratura
            for (int v = 0; v < valide.Count; v++)
                if (ancoraDi[v] >= 0)
                    timbraturaDi[ancoraDi[v]] = valide[v];

            var libere = new SortedSet<int>(valide.Where(i => !timbraturaDi.Contains(i)));

            DateTime Eff(int k) => timbraturaDi[k] >= 0 ? timbrature[timbraturaDi[k]] : ancore[k].Ora;
            DateTime EffPrima(int k) => k > 0 ? Eff(k - 1) : DateTime.MinValue;
            DateTime EffDopo(int k) => k < m - 1 ? Eff(k + 1) : DateTime.MaxValue;
            List<int> Tra(DateTime da, DateTime a, bool daIncluso = false) =>
                libere.Where(i => (daIncluso ? timbrature[i] >= da : timbrature[i] > da) && timbrature[i] < a).ToList();

            // 3) ancore vuote: timbratura fuori intervallo nella zona adiacente (dispari = manca proprio quella)
            for (int k = 0; k < m; k++)
            {
                if (timbraturaDi[k] >= 0)
                    continue;

                int scelta = -1;
                var prima = Tra(EffPrima(k), ancore[k].Ora);
                var dopo = Tra(ancore[k].Ora, EffDopo(k), daIncluso: true);
                if (ancore[k].Entrata)
                {
                    // entrata in ritardo (prima timbratura dentro la coppia) o in anticipo (ultima prima dell'ora)
                    if (dopo.Count % 2 == 1) scelta = dopo.First();
                    else if (prima.Count % 2 == 1) scelta = prima.Last();
                }
                else
                {
                    // uscita anticipata (ultima timbratura dentro la coppia) o in ritardo (prima dopo l'ora)
                    if (prima.Count % 2 == 1) scelta = prima.Last();
                    else if (dopo.Count % 2 == 1) scelta = dopo.First();
                }

                if (scelta >= 0)
                {
                    timbraturaDi[k] = scelta;
                    libere.Remove(scelta);
                    res.Avviso = true;
                }
            }

            // ancore ancora vuote: ammesse solo le coppie facoltative vuote entrambe (pausa automatica)
            for (int k = 0; k < m; k++)
                if (timbraturaDi[k] < 0)
                    res.AncoreVuote.Add(k);

            var vuoteAmmesse = res.VuoteAmmesse;
            if (facoltative != null)
                foreach (var (u, e) in facoltative)
                    if (res.AncoreVuote.Contains(u) && res.AncoreVuote.Contains(e))
                    {
                        vuoteAmmesse.Add(u);
                        vuoteAmmesse.Add(e);
                    }

            if (res.AncoreVuote.Any(k => !vuoteAmmesse.Contains(k)))
            {
                // giorno incompleto: nessun verso, il calcolo non conta ore finche' non si sistemano le timbrature
                res.TimbratureMancanti = true;
                return res;
            }

            for (int k = 0; k < m; k++)
            {
                if (timbraturaDi[k] < 0) continue;
                verso[timbraturaDi[k]] = ancore[k].Entrata ? TipoTimbratura.Entrata : TipoTimbratura.Uscita;
                indiceAncora[timbraturaDi[k]] = k;
            }

            // 4) timbrature rimaste, per tratto tra due ancore abbinate consecutive
            var abbinate = Enumerable.Range(0, m).Where(k => timbraturaDi[k] >= 0).ToList();
            if (abbinate.Count == 0)
            {
                AccoppiaTratto(libere.ToList(), verso, entrataPrima: true, res);
                return res;
            }
            var confini = new List<(DateTime Da, DateTime A, bool DentroCoppia)>();
            confini.Add((DateTime.MinValue, Eff(abbinate[0]), false));                       // prima dell'orario
            for (int x = 0; x + 1 < abbinate.Count; x++)
                confini.Add((Eff(abbinate[x]), Eff(abbinate[x + 1]), ancore[abbinate[x]].Entrata));
            confini.Add((Eff(abbinate[^1]), DateTime.MaxValue, false));                       // dopo l'orario

            foreach (var (da, a, dentroCoppia) in confini)
            {
                var tratto = libere.Where(i => timbrature[i] > da && timbrature[i] < a).ToList();
                // timbratura allo stesso minuto di un'ancora: la si considera nel tratto successivo
                tratto.AddRange(libere.Where(i => timbrature[i] == da && !tratto.Contains(i)));
                tratto = tratto.Distinct().OrderBy(i => timbrature[i]).ToList();
                AccoppiaTratto(tratto, verso, entrataPrima: !dentroCoppia, res);
                foreach (var i in tratto) libere.Remove(i);
            }

            return res;
        }

        // coppie in ordine cronologico: E/U fuori dalle coppie, U/E (uscita intermedia) dentro una coppia
        private static void AccoppiaTratto(List<int> tratto, TipoTimbratura[] verso, bool entrataPrima, Risultato res)
        {
            if (tratto.Count % 2 == 1)
            {
                res.NonInterpretabile = true;
                return;
            }
            for (int x = 0; x + 1 < tratto.Count; x += 2)
            {
                verso[tratto[x]] = entrataPrima ? TipoTimbratura.Entrata : TipoTimbratura.Uscita;
                verso[tratto[x + 1]] = entrataPrima ? TipoTimbratura.Uscita : TipoTimbratura.Entrata;
            }
        }

        /// <summary>
        /// Abbinamento che rispetta l'ordine (timbrature e ancore crescenti), solo dentro gli intervalli:
        /// massimizza le ancore abbinate, a parita' minimizza la distanza totale dalle ore attese.
        /// Restituisce per ogni timbratura l'indice dell'ancora (-1 = libera).
        /// </summary>
        private static int[] AbbinaOrdinato(List<DateTime> t, List<Ancora> a)
        {
            int n = t.Count, m = a.Count;
            var num = new int[n + 1, m + 1];
            var dist = new double[n + 1, m + 1];
            var mossa = new byte[n + 1, m + 1];   // 1 = salta timbratura, 2 = salta ancora, 3 = abbina

            bool Meglio(int n1, double d1, int n2, double d2) => n1 > n2 || (n1 == n2 && d1 < d2);

            for (int i = 0; i <= n; i++)
                for (int j = 0; j <= m; j++)
                {
                    if (i == 0 && j == 0) continue;
                    int bn = -1; double bd = double.MaxValue; byte bm = 0;
                    if (i > 0 && Meglio(num[i - 1, j], dist[i - 1, j], bn, bd)) { bn = num[i - 1, j]; bd = dist[i - 1, j]; bm = 1; }
                    if (j > 0 && Meglio(num[i, j - 1], dist[i, j - 1], bn, bd)) { bn = num[i, j - 1]; bd = dist[i, j - 1]; bm = 2; }
                    if (i > 0 && j > 0 && t[i - 1] >= a[j - 1].Dal && t[i - 1] <= a[j - 1].Al)
                    {
                        int cn = num[i - 1, j - 1] + 1;
                        double cd = dist[i - 1, j - 1] + Math.Abs((t[i - 1] - a[j - 1].Ora).TotalMinutes);
                        if (Meglio(cn, cd, bn, bd)) { bn = cn; bd = cd; bm = 3; }
                    }
                    num[i, j] = bn; dist[i, j] = bd; mossa[i, j] = bm;
                }

            var retVal = Enumerable.Repeat(-1, n).ToArray();
            for (int i = n, j = m; i > 0 || j > 0;)
            {
                switch (mossa[i, j])
                {
                    case 3: retVal[i - 1] = j - 1; i--; j--; break;
                    case 1: i--; break;
                    default: j--; break;
                }
            }
            return retVal;
        }

        /// <summary>Arrotondamento di una timbratura abbinata all'ancora; le altre restano come sono.</summary>
        public static DateTime Arrotonda(DateTime timbratura, Ancora? ancora) =>
            ancora == null ? timbratura : Roundings.RoundDateTime(timbratura, new TimeRoundOptions(ancora.Arrotondamento, ancora.Arrotondamento_Verso));
    }
}
