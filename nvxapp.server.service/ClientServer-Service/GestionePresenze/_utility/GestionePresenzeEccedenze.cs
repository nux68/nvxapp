using nvxapp.server.data.Entities.Tenant.GestionePresenze;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    /*
     Qualifica delle ore lavorate oltre il lavorabile (straordinario / lavoro supplementare).
     Logica pura, senza accesso ai dati: il motore (TimeSheet_EngineService) prepara i giorni
     con i valori della fase giornaliera e poi chiede la qualifica dei periodi.

     Fase giornaliera (nel motore, per ogni giorno):
        Lavorabili = teorico +/- giustificativi
        Ecc        = lavorato - lavorabili   (>0: arrotondato per difetto e oltre la franchigia, <0: ore mancanti)
        EccNott    = parte dell'eccedenza svolta in fascia notturna (fuori dalle coppie dell'orario)

     Fase di periodo (qui), sul periodo del profilo (giorno, settimana lun-dom, mese):
        Ecc_P   = max(0, somma Ecc)                       le ore mancanti di un giorno compensano quelle in piu' di un altro
        X       = lavorato del periodo (+ assenze, se il profilo le conta per la soglia)
        Soglia  = soglia full-time del periodo
                    full-time: il teorico del periodo (non oltre la soglia del profilo, se impostata)
                    part-time: giornaliero  = soglia giornaliera FT
                               settimanale = soglia settimanale FT
                               mensile     = teorico del mese riproporzionato al full-time
        Strao   = X - Soglia, tra 0 e Ecc_P
        resto   = Ecc_P - Strao: supplementare (part-time) oppure ore eccedenti non straordinarie (full-time)

     Le ore qualificate sono attribuite ai giorni partendo dall'ultimo con ore in piu' (le ore oltre la
     soglia sono le ultime lavorate): prima lo straordinario, poi il resto. Le ore in piu' dei primi giorni
     che non ricevono una qualifica sono "compensate" dalle ore mancanti del periodo.
     Lo straordinario del giorno e' notturno per la parte EccNott e festivo se il giorno e' festivo.

     Supplementare: entro il limite % delle ore settimanali contrattuali e' "standard", oltre e' "eccedente";
     il limite si consuma in ordine cronologico nella settimana lun-dom.

     Giorni bloccati (mese approvato, o fuori dal ricalcolo): mantengono la loro qualifica, che viene
     tolta da quella del periodo prima di distribuire il resto.
    */
    public static class GestionePresenzeEccedenze
    {
        public class Giorno
        {
            public DateTime Data { get; set; }
            public bool Bloccato { get; set; }

            // fase giornaliera
            public int Teo { get; set; }
            public int Lavorabili { get; set; }
            public int Assenze { get; set; }
            public int Ecc { get; set; }
            public int EccNott { get; set; }
            public bool Festivo { get; set; }

            // qualifica
            public bool Provvisoria { get; set; }
            public int Suppl { get; set; }
            public int SupplEcc { get; set; }
            public int Strao { get; set; }
            public int StraoNott { get; set; }
            public int StraoFest { get; set; }
            public int StraoFestNott { get; set; }
            public int EccOrd { get; set; }
            public int NonRiconosciute { get; set; }

            public int Lavorato => Lavorabili + Ecc;
            public int StraoTot => Strao + StraoNott + StraoFest + StraoFestNott;
            public int SupplTot => Suppl + SupplEcc;
            public int Qualificate => StraoTot + SupplTot + EccOrd;

            public void AzzeraQualifica()
            {
                Provvisoria = false;
                Suppl = SupplEcc = Strao = StraoNott = StraoFest = StraoFestNott = EccOrd = NonRiconosciute = 0;
            }
        }

        public class Parametri
        {
            public StraoTipoConteggio TipoConteggio { get; set; }
            public bool PartTime { get; set; }
            public int SogliaGiornalieraFT { get; set; }     // minuti, 0 = non impostata
            public int SogliaSettimanaleFT { get; set; }     // minuti, 0 = non impostata
            public decimal SupplLimitePerc { get; set; }
            public int? OreSettimanaliContratto { get; set; } // minuti
            public bool AssenzeContanoSoglia { get; set; }
        }

        // ── periodi ──────────────────────────────────────────────────────────

        public static DateTime InizioSettimana(DateTime d)
        {
            int diff = ((int)d.DayOfWeek + 6) % 7; // lunedi' = 0
            return d.Date.AddDays(-diff);
        }

        public static (DateTime Dal, DateTime Al) Periodo(DateTime giorno, StraoTipoConteggio tipo)
        {
            giorno = giorno.Date;
            switch (tipo)
            {
                case StraoTipoConteggio.Settimanale:
                    var lun = InizioSettimana(giorno);
                    return (lun, lun.AddDays(6));
                case StraoTipoConteggio.Mensile:
                    var primo = new DateTime(giorno.Year, giorno.Month, 1);
                    return (primo, primo.AddMonths(1).AddDays(-1));
                default:
                    return (giorno, giorno);
            }
        }

        // ── qualifica di un periodo ─────────────────────────────────────────

        /// <summary>
        /// Qualifica le ore in piu' di un periodo. periodoAl e' l'ultimo giorno del periodo (anche se fuori dal rapporto):
        /// se non e' ancora passato il periodo e' in corso e le ore restano provvisorie.
        /// </summary>
        public static void QualificaPeriodo(List<Giorno> giorni, DateTime periodoAl, Parametri p, DateTime oggi)
        {
            var liberi = giorni.Where(g => !g.Bloccato).OrderBy(g => g.Data).ToList();
            foreach (var g in liberi)
                g.AzzeraQualifica();

            if (giorni.Count == 0)
                return;

            if (periodoAl.Date >= oggi.Date)
            {
                foreach (var g in liberi.Where(g => g.Ecc > 0))
                    g.Provvisoria = true;
                return;
            }

            int teo = giorni.Sum(g => g.Teo);
            int assenze = giorni.Sum(g => g.Assenze);
            int lavorato = giorni.Sum(g => g.Lavorato);
            int ecc = Math.Max(0, giorni.Sum(g => g.Ecc));

            int x = lavorato + (p.AssenzeContanoSoglia ? assenze : 0);
            int soglia = SogliaPeriodo(p, teo, giorni.Count);

            int strao = Math.Clamp(x - soglia, 0, ecc);
            int resto = ecc - strao;
            int suppl = p.PartTime ? resto : 0;
            int eccOrd = p.PartTime ? 0 : resto;

            // quanto e' gia' attribuito ai giorni bloccati
            var bloccati = giorni.Where(g => g.Bloccato).ToList();
            strao = Math.Max(0, strao - bloccati.Sum(g => g.StraoTot));
            suppl = Math.Max(0, suppl - bloccati.Sum(g => g.SupplTot));
            eccOrd = Math.Max(0, eccOrd - bloccati.Sum(g => g.EccOrd));

            // distribuzione a ritroso: le ultime ore lavorate sono quelle oltre la soglia
            foreach (var g in liberi.Where(g => g.Ecc > 0).OrderByDescending(g => g.Data))
            {
                int capacita = g.Ecc;

                int s = Math.Min(capacita, strao); strao -= s; capacita -= s;
                int su = Math.Min(capacita, suppl); suppl -= su; capacita -= su;
                int eo = Math.Min(capacita, eccOrd); eccOrd -= eo;

                AssegnaStraordinario(g, s);
                g.Suppl = su;
                g.EccOrd = eo;

                if (strao == 0 && suppl == 0 && eccOrd == 0)
                    break;
            }
        }

        private static int SogliaPeriodo(Parametri p, int teo, int numGiorni)
        {
            switch (p.TipoConteggio)
            {
                case StraoTipoConteggio.Settimanale:
                    if (p.PartTime)
                        return p.SogliaSettimanaleFT > 0 ? p.SogliaSettimanaleFT : teo;
                    return p.SogliaSettimanaleFT > 0 ? Math.Min(teo, p.SogliaSettimanaleFT) : teo;

                case StraoTipoConteggio.Mensile:
                    if (!p.PartTime)
                        return teo;
                    {
                        // teorico del mese riportato al full-time: teo * (soglia FT settimanale / contratto settimanale)
                        decimal settimane = numGiorni / 7m;
                        decimal contratto = p.OreSettimanaliContratto ?? (settimane > 0 ? teo / settimane : 0);
                        int sogliaSett = p.SogliaSettimanaleFT > 0 ? p.SogliaSettimanaleFT : 40 * 60;
                        if (contratto <= 0)
                            return (int)Math.Round(sogliaSett * settimane);
                        return (int)Math.Round(teo * (sogliaSett / contratto));
                    }

                default: // giornaliero
                    if (p.PartTime)
                        return p.SogliaGiornalieraFT > 0 ? p.SogliaGiornalieraFT : teo;
                    return p.SogliaGiornalieraFT > 0 ? Math.Min(teo, p.SogliaGiornalieraFT) : teo;
            }
        }

        private static void AssegnaStraordinario(Giorno g, int minuti)
        {
            int nott = Math.Min(minuti, g.EccNott);
            if (g.Festivo)
            {
                g.StraoFestNott = nott;
                g.StraoFest = minuti - nott;
            }
            else
            {
                g.StraoNott = nott;
                g.Strao = minuti - nott;
            }
        }

        // ── supplementare: limite % sulla settimana ─────────────────────────

        /// <summary>
        /// Separa il supplementare eccedente il limite settimanale (perc. delle ore contrattuali).
        /// giorniSettimana: tutti i giorni noti della settimana lun-dom; si modificano solo i non bloccati.
        /// </summary>
        public static void SeparaSupplementare(List<Giorno> giorniSettimana, Parametri p)
        {
            int contratto = p.OreSettimanaliContratto ?? giorniSettimana.Sum(g => g.Teo);
            int limite = (int)Math.Round(contratto * p.SupplLimitePerc / 100m);

            int usato = 0;
            foreach (var g in giorniSettimana.OrderBy(g => g.Data))
            {
                if (g.Bloccato)
                {
                    usato += g.Suppl;
                    continue;
                }

                int totale = g.Suppl + g.SupplEcc;
                int std = Math.Clamp(limite - usato, 0, totale);
                g.Suppl = std;
                g.SupplEcc = totale - std;
                usato += std;
            }
        }

        // ── tetto delle ore riconosciute nel mese (decisione HR) ────────────

        /// <summary>
        /// Riduce le ore qualificate del mese al tetto indicato, togliendole dagli ultimi giorni
        /// (le ultime ore lavorate). Le ore tolte diventano "non riconosciute".
        /// </summary>
        public static void ApplicaTetto(List<Giorno> giorniMese, int tetto)
        {
            int eccesso = giorniMese.Sum(g => g.Qualificate) - Math.Max(0, tetto);
            if (eccesso <= 0)
                return;

            foreach (var g in giorniMese.Where(g => !g.Bloccato).OrderByDescending(g => g.Data))
            {
                if (eccesso <= 0) break;

                // nel giorno le ultime ore sono lo straordinario, poi il supplementare, poi le eccedenti
                int t;
                t = Math.Min(g.StraoFestNott, eccesso); g.StraoFestNott -= t; g.NonRiconosciute += t; eccesso -= t;
                t = Math.Min(g.StraoFest, eccesso);     g.StraoFest -= t;     g.NonRiconosciute += t; eccesso -= t;
                t = Math.Min(g.StraoNott, eccesso);     g.StraoNott -= t;     g.NonRiconosciute += t; eccesso -= t;
                t = Math.Min(g.Strao, eccesso);         g.Strao -= t;         g.NonRiconosciute += t; eccesso -= t;
                t = Math.Min(g.SupplEcc, eccesso);      g.SupplEcc -= t;      g.NonRiconosciute += t; eccesso -= t;
                t = Math.Min(g.Suppl, eccesso);         g.Suppl -= t;         g.NonRiconosciute += t; eccesso -= t;
                t = Math.Min(g.EccOrd, eccesso);        g.EccOrd -= t;        g.NonRiconosciute += t; eccesso -= t;
            }
        }
    }
}
