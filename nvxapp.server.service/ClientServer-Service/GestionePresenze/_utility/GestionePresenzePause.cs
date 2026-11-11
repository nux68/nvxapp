using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    /// <summary>
    /// Pause di un orario a coppie (ANALISI_ATTENDANCETRACKING_PAUSE.md §3.4-3.5, §6). Funzione pura.
    ///
    /// Pranzo: rilevata = stacchi tra i segmenti lavorati che toccano la finestra; dovuta = DurataMin se il lavorato
    /// lordo supera SogliaMin; Automatica (e niente giustificativo "pausa non fruita") detrae
    /// max(0, dovuta - rilevata - RetribuitaMin) a partire dall'inizio della finestra; Rilevata segnala soltanto.
    /// Breve: stacchi dentro le coppie fuori dalla finestra del pranzo.
    /// In entrambe i minuti timbrati fino a RetribuitaMin tornano lavorati (riaccredito).
    /// </summary>
    public static class GestionePresenzePause
    {
        public sealed class Esito
        {
            public int Rilevata { get; set; }
            public int Detratta { get; set; }
            public int Retribuita { get; set; }
            public bool Insufficiente { get; set; }
            public bool OltreMax { get; set; }
            public int? IdCausale_Pausa { get; set; }
        }

        public static Par_OrarioPausaModel? Pranzo(IEnumerable<Par_OrarioPausaModel> pause) =>
            pause.Where(p => p.Tipo == PausaTipo.Pranzo).OrderBy(p => p.NumPausa).FirstOrDefault();

        public static Par_OrarioPausaModel? Breve(IEnumerable<Par_OrarioPausaModel> pause) =>
            pause.Where(p => p.Tipo == PausaTipo.Breve).OrderBy(p => p.NumPausa).FirstOrDefault();

        /// <summary>Coppie dell'orario come intervalli del giorno (date complete, mezzanotte compresa).</summary>
        public static List<(DateTime Dal, DateTime Al)> IntervalliCoppie(DateTime giorno, IEnumerable<Par_OrarioIntervalloHHModel> coppie)
        {
            var tutte = TutteLeAncore(giorno, coppie);
            var retVal = new List<(DateTime, DateTime)>();
            for (int i = 0; i + 1 < tutte.Count; i += 2)
                retVal.Add((tutte[i].Ora, tutte[i + 1].Ora));
            return retVal;
        }

        // ancore di tutte le coppie (anche senza Use_4_Match): servono per la finestra predefinita
        private static List<GestionePresenzeSequenza.Ancora> TutteLeAncore(DateTime giorno, IEnumerable<Par_OrarioIntervalloHHModel> coppie) =>
            GestionePresenzeSequenza.Ancore(giorno, coppie.Select(c => new Par_OrarioIntervalloHHModel
            {
                NumCoppia = c.NumCoppia,
                Dalle = c.Dalle,
                Dalle_Limite_SX = c.Dalle_Limite_SX,
                Dalle_Limite_DX = c.Dalle_Limite_DX,
                Dalle_Use_4_Match = true,
                Alle = c.Alle,
                Alle_Limite_SX = c.Alle_Limite_SX,
                Alle_Limite_DX = c.Alle_Limite_DX,
                Alle_Use_4_Match = true
            }));

        /// <summary>
        /// Finestra del pranzo: quella indicata nella pausa oppure, se vuota, dal limite sinistro dell'uscita al limite
        /// destro del rientro dello stacco piu' lungo tra due coppie. Null se non determinabile (coppia unica senza finestra).
        /// </summary>
        public static (DateTime Dal, DateTime Al)? Finestra(DateTime giorno, Par_OrarioPausaModel pausa, IEnumerable<Par_OrarioIntervalloHHModel> coppie)
        {
            var ancore = TutteLeAncore(giorno, coppie);

            if (pausa.FinestraDalle.HasValue && pausa.FinestraAlle.HasValue)
            {
                var dal = giorno.Date + pausa.FinestraDalle.Value.ToTimeSpan();
                // orario a cavallo della mezzanotte: una finestra prima dell'inizio turno e' del giorno dopo
                if (ancore.Count > 0 && ancore[^1].Ora.Date > giorno.Date && dal < ancore[0].Ora)
                    dal = dal.AddDays(1);
                var durata = pausa.FinestraAlle.Value.ToTimeSpan() - pausa.FinestraDalle.Value.ToTimeSpan();
                if (durata <= TimeSpan.Zero)
                    durata += TimeSpan.FromDays(1);
                return (dal, dal + durata);
            }

            (DateTime, DateTime)? retVal = null;
            TimeSpan maxStacco = TimeSpan.Zero;
            for (int i = 1; i + 1 < ancore.Count; i += 2)
            {
                var stacco = ancore[i + 1].Ora - ancore[i].Ora;
                if (stacco > maxStacco)
                {
                    maxStacco = stacco;
                    retVal = (ancore[i].Dal, ancore[i + 1].Al);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Variazione delle ore teoriche dovuta al pranzo: se cade dentro una coppia (orario continuato) toglie la parte
        /// non retribuita; se cade nello stacco tra due coppie aggiunge la parte retribuita. Nessuna variazione se il
        /// teorico delle coppie non supera la soglia.
        /// </summary>
        public static TimeSpan VariazioneTeorico(DateTime giorno, TimeSpan teoricoCoppie, IEnumerable<Par_OrarioIntervalloHHModel> coppie, IEnumerable<Par_OrarioPausaModel> pause)
        {
            var pranzo = Pranzo(pause);
            if (pranzo == null || teoricoCoppie.TotalMinutes <= pranzo.SogliaMin)
                return TimeSpan.Zero;

            var finestra = Finestra(giorno, pranzo, coppie);
            if (finestra == null)
                return TimeSpan.Zero;

            var intervalli = IntervalliCoppie(giorno, coppie);
            bool nelloStacco = false;
            for (int i = 0; i + 1 < intervalli.Count; i++)
                if (Sovrapposizione(intervalli[i].Al, intervalli[i + 1].Dal, finestra.Value.Dal, finestra.Value.Al) > TimeSpan.Zero)
                    nelloStacco = true;

            return nelloStacco
                ? TimeSpan.FromMinutes(pranzo.RetribuitaMin)
                : -TimeSpan.FromMinutes(Math.Max(0, pranzo.DurataMin - pranzo.RetribuitaMin));
        }

        /// <param name="segmenti">segmenti lavorati (entrata, uscita arrotondate) in ordine</param>
        /// <param name="annullata">giustificativo "pausa non fruita" nel giorno</param>
        public static List<(DateTime Dal, DateTime Al)> Applica(DateTime giorno, List<(DateTime Dal, DateTime Al)> segmenti,
                                                                IEnumerable<Par_OrarioIntervalloHHModel> coppie, IEnumerable<Par_OrarioPausaModel> pause,
                                                                bool annullata, out Esito esito)
        {
            esito = new Esito();
            var listaPause = pause.ToList();
            var segs = segmenti.Where(s => s.Al > s.Dal).OrderBy(s => s.Dal).ToList();
            if (listaPause.Count == 0 || segs.Count == 0)
                return segs;

            var coppieList = coppie.ToList();
            var lordo = segs.Aggregate(TimeSpan.Zero, (acc, s) => acc + (s.Al - s.Dal));

            var stacchi = new List<(DateTime Dal, DateTime Al)>();
            for (int i = 0; i + 1 < segs.Count; i++)
                if (segs[i + 1].Dal > segs[i].Al)
                    stacchi.Add((segs[i].Al, segs[i + 1].Dal));

            var pranzo = Pranzo(listaPause);
            var breve = Breve(listaPause);
            var finestra = pranzo != null ? Finestra(giorno, pranzo, coppieList) : null;
            var intervalli = IntervalliCoppie(giorno, coppieList);

            var stacchiPranzo = finestra == null ? new List<(DateTime Dal, DateTime Al)>()
                : stacchi.Where(s => Sovrapposizione(s.Dal, s.Al, finestra.Value.Dal, finestra.Value.Al) > TimeSpan.Zero).ToList();
            var stacchiBrevi = stacchi.Where(s => !stacchiPranzo.Contains(s) &&
                                                  intervalli.Any(c => Sovrapposizione(s.Dal, s.Al, c.Dal, c.Al) > TimeSpan.Zero))
                                      .ToList();

            int rilevataPranzo = Minuti(stacchiPranzo);
            int rilevataBreve = breve != null ? Minuti(stacchiBrevi) : 0;
            var retVal = segs;

            if (pranzo != null && finestra != null)
            {
                int dovuta = lordo.TotalMinutes > pranzo.SogliaMin ? pranzo.DurataMin : 0;
                if (pranzo.Modalita == PausaModalita.Rilevata && !annullata && dovuta > rilevataPranzo)
                    esito.Insufficiente = true;
                if (pranzo.DurataMaxMin.HasValue && rilevataPranzo > pranzo.DurataMaxMin.Value)
                    esito.OltreMax = true;

                if (pranzo.Modalita == PausaModalita.Automatica && !annullata)
                {
                    int detrazione = Math.Max(0, dovuta - rilevataPranzo - pranzo.RetribuitaMin);
                    retVal = Togli(retVal, finestra.Value.Dal, detrazione, out int tolti);
                    esito.Detratta = tolti;
                }

                int riaccredito = Math.Min(rilevataPranzo, pranzo.RetribuitaMin);
                retVal = Aggiungi(retVal, stacchiPranzo, riaccredito);
                esito.Retribuita += riaccredito;
                esito.IdCausale_Pausa = pranzo.IdCausale_Pausa;
            }

            if (breve != null)
            {
                if (breve.DurataMaxMin.HasValue && rilevataBreve > breve.DurataMaxMin.Value)
                    esito.OltreMax = true;
                int riaccredito = Math.Min(rilevataBreve, breve.RetribuitaMin);
                retVal = Aggiungi(retVal, stacchiBrevi, riaccredito);
                esito.Retribuita += riaccredito;
            }

            esito.Rilevata = rilevataPranzo + rilevataBreve;
            return retVal;
        }

        private static int Minuti(IEnumerable<(DateTime Dal, DateTime Al)> intervalli) =>
            (int)intervalli.Aggregate(TimeSpan.Zero, (acc, s) => acc + (s.Al - s.Dal)).TotalMinutes;

        /// <summary>
        /// Toglie i minuti dai segmenti a partire da "inizio" andando avanti; se non bastano, dai segmenti precedenti
        /// andando indietro. Restituisce i minuti effettivamente tolti.
        /// </summary>
        private static List<(DateTime Dal, DateTime Al)> Togli(List<(DateTime Dal, DateTime Al)> segs, DateTime inizio, int minuti, out int tolti)
        {
            tolti = 0;
            var restano = TimeSpan.FromMinutes(minuti);
            var tagli = new List<(DateTime Dal, DateTime Al)>();

            foreach (var s in segs.Where(s => s.Al > inizio))
            {
                if (restano <= TimeSpan.Zero) break;
                var da = s.Dal > inizio ? s.Dal : inizio;
                var t = s.Al - da < restano ? s.Al - da : restano;
                tagli.Add((da, da + t));
                restano -= t;
            }
            foreach (var s in segs.Where(s => s.Dal < inizio).Reverse())
            {
                if (restano <= TimeSpan.Zero) break;
                var a = s.Al < inizio ? s.Al : inizio;
                var t = a - s.Dal < restano ? a - s.Dal : restano;
                tagli.Add((a - t, a));
                restano -= t;
            }

            tolti = minuti - (int)restano.TotalMinutes;
            var retVal = segs;
            foreach (var taglio in tagli)
                retVal = retVal.SelectMany(s => Sottrai(s, taglio)).ToList();
            return retVal;
        }

        // riaccredito: segmenti lavorati all'inizio degli stacchi della pausa, fino ai minuti indicati
        private static List<(DateTime Dal, DateTime Al)> Aggiungi(List<(DateTime Dal, DateTime Al)> segs, List<(DateTime Dal, DateTime Al)> stacchi, int minuti)
        {
            var restano = TimeSpan.FromMinutes(minuti);
            var retVal = new List<(DateTime Dal, DateTime Al)>(segs);
            foreach (var s in stacchi.OrderBy(s => s.Dal))
            {
                if (restano <= TimeSpan.Zero) break;
                var t = s.Al - s.Dal < restano ? s.Al - s.Dal : restano;
                retVal.Add((s.Dal, s.Dal + t));
                restano -= t;
            }
            return retVal.OrderBy(s => s.Dal).ToList();
        }

        private static IEnumerable<(DateTime Dal, DateTime Al)> Sottrai((DateTime Dal, DateTime Al) s, (DateTime Dal, DateTime Al) taglio)
        {
            if (taglio.Al <= s.Dal || taglio.Dal >= s.Al)
            {
                yield return s;
                yield break;
            }
            if (taglio.Dal > s.Dal) yield return (s.Dal, taglio.Dal);
            if (taglio.Al < s.Al) yield return (taglio.Al, s.Al);
        }

        public static TimeSpan Sovrapposizione(DateTime s1, DateTime e1, DateTime s2, DateTime e2)
        {
            var s = s1 > s2 ? s1 : s2;
            var e = e1 < e2 ? e1 : e2;
            return e > s ? e - s : TimeSpan.Zero;
        }
    }
}
