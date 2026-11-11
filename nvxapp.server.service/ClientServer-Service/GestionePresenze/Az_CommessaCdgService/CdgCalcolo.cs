using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaCdgService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaCdgService
{
    /*
     Calcolo del controllo di gestione sulle commesse (ANALISI_ATTENDANCETRACKING_CONTROLLO_GESTIONE.md §4). Funzione pura:
     riceve le ore (dipendente, sottocommessa, mese), le note spese (dipendente, mese, sottocommessa predefinita del rapporto),
     le anagrafiche delle sottocommesse e i parametri della pagina (costi orari, ricavi).
        - costo personale della riga (dipendente, sottocommessa) = minuti / 60 x costo orario, arrotondato al centesimo;
          i totali sommano le righe arrotondate (cio' che si vede torna sempre)
        - note spese del dipendente nel mese ripartite sulle sottocommesse in proporzione ai minuti del mese; l'ultima quota
          prende il resto. Senza ore nel mese: sulla sottocommessa predefinita del rapporto, altrimenti non attribuite
        - margine = ricavo - costo; % solo con ricavo > 0
    */
    public static class CdgCalcolo
    {
        public sealed record Ore(string IdAspNetUsers, string NomeDipendente, int IdAz_SubCommessa, int Anno, int Mese, int Minuti);
        public sealed record Nota(string IdAspNetUsers, int Anno, int Mese, int? IdAz_SubCommessaPredefinita, decimal Importo);
        public sealed record SubCommessa(int IdAz_SubCommessa, string Descrizione, int IdAz_Commessa, string Commessa, string Cliente);

        public static decimal Arrotonda(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        public static CdgRisultato Calcola(IEnumerable<Ore> ore, IEnumerable<Nota> note, IReadOnlyDictionary<int, SubCommessa> subCommesse,
                                           IReadOnlyDictionary<string, string> nomi, CdgParametri p, DateTime dal, DateTime al, int giornateEscluse)
        {
            var oreList = ore.Where(o => o.Minuti > 0).ToList();
            var costi = p.CostiDipendenti.Where(c => !string.IsNullOrEmpty(c.IdAspNetUsers))
                                         .GroupBy(c => c.IdAspNetUsers).ToDictionary(g => g.Key, g => g.Last().CostoOrario);
            decimal predefinito = p.CostoOrarioPredefinito ?? 0m;
            decimal CostoOrario(string u) => costi.TryGetValue(u, out var c) ? c : predefinito;

            var retVal = new CdgRisultato { Dal = dal, Al = al, CostoOrarioPredefinito = predefinito, GiornateEscluse = giornateEscluse };

            // righe (dipendente, sottocommessa): minuti e note spese
            var minuti = new Dictionary<(string, int), int>();
            foreach (var o in oreList)
                minuti[(o.IdAspNetUsers, o.IdAz_SubCommessa)] = minuti.GetValueOrDefault((o.IdAspNetUsers, o.IdAz_SubCommessa)) + o.Minuti;
            var noteRiga = new Dictionary<(string, int), decimal>();
            void AggiungiNota(string u, int sub, decimal importo) => noteRiga[(u, sub)] = noteRiga.GetValueOrDefault((u, sub)) + importo;

            foreach (var gruppo in note.GroupBy(n => (n.IdAspNetUsers, n.Anno, n.Mese)))
            {
                var (u, anno, mese) = gruppo.Key;
                var oreMese = oreList.Where(o => o.IdAspNetUsers == u && o.Anno == anno && o.Mese == mese)
                                     .GroupBy(o => o.IdAz_SubCommessa)
                                     .Select(g => (Sub: g.Key, Minuti: g.Sum(x => x.Minuti)))
                                     .OrderBy(x => x.Sub)
                                     .ToList();
                int totaleMinuti = oreMese.Sum(x => x.Minuti);
                if (totaleMinuti > 0)
                {
                    decimal totale = gruppo.Sum(n => n.Importo);
                    decimal assegnato = 0m;
                    for (int i = 0; i < oreMese.Count; i++)
                    {
                        decimal quota = i == oreMese.Count - 1 ? totale - assegnato : Arrotonda(totale * oreMese[i].Minuti / totaleMinuti);
                        assegnato += quota;
                        AggiungiNota(u, oreMese[i].Sub, quota);
                    }
                }
                else
                {
                    foreach (var n in gruppo)
                    {
                        if (n.IdAz_SubCommessaPredefinita is int sub && subCommesse.ContainsKey(sub))
                            AggiungiNota(u, sub, n.Importo);
                        else
                            retVal.NoteSpeseNonAttribuite += n.Importo;
                    }
                }
            }

            var ricavi = p.Ricavi.GroupBy(r => r.IdAz_SubCommessa).ToDictionary(g => g.Key, g => g.Last().Ricavo);
            var chiavi = minuti.Keys.Union(noteRiga.Keys).ToList();
            string Nome(string u) => nomi.TryGetValue(u, out var n) ? n : oreList.FirstOrDefault(o => o.IdAspNetUsers == u)?.NomeDipendente ?? u;

            var commesse = new List<CdgCommessaModel>();
            foreach (var perCommessa in chiavi.Where(k => subCommesse.ContainsKey(k.Item2))
                                              .GroupBy(k => subCommesse[k.Item2].IdAz_Commessa))
            {
                var primaSub = subCommesse[perCommessa.First().Item2];
                var c = new CdgCommessaModel { IdAz_Commessa = perCommessa.Key, Descrizione = primaSub.Commessa, Cliente = primaSub.Cliente };
                foreach (var perSub in perCommessa.GroupBy(k => k.Item2))
                {
                    var info = subCommesse[perSub.Key];
                    var s = new CdgSubCommessaModel { IdAz_SubCommessa = perSub.Key, Descrizione = info.Descrizione };
                    foreach (var (u, sub) in perSub)
                    {
                        int m = minuti.GetValueOrDefault((u, sub));
                        decimal costoOrario = CostoOrario(u);
                        s.Dipendenti.Add(new CdgRigaDipendente
                        {
                            IdAspNetUsers = u,
                            NomeDipendente = Nome(u),
                            Minuti = m,
                            CostoOrario = costoOrario,
                            CostoPersonale = Arrotonda(m * costoOrario / 60m),
                            NoteSpese = noteRiga.GetValueOrDefault((u, sub)),
                        });
                    }
                    s.Dipendenti = s.Dipendenti.OrderBy(d => d.NomeDipendente, StringComparer.CurrentCultureIgnoreCase).ToList();
                    s.Minuti = s.Dipendenti.Sum(d => d.Minuti);
                    s.CostoPersonale = s.Dipendenti.Sum(d => d.CostoPersonale);
                    s.NoteSpese = s.Dipendenti.Sum(d => d.NoteSpese);
                    s.Ricavo = ricavi.TryGetValue(s.IdAz_SubCommessa, out var r) ? r : null;
                    Completa(s);
                    c.SubCommesse.Add(s);
                }
                c.SubCommesse = c.SubCommesse.OrderBy(s => s.Descrizione, StringComparer.CurrentCultureIgnoreCase).ToList();
                Somma(c, c.SubCommesse);
                commesse.Add(c);
            }

            // note di sottocommesse sconosciute (non dovrebbe succedere): non attribuite
            retVal.NoteSpeseNonAttribuite += noteRiga.Where(n => !subCommesse.ContainsKey(n.Key.Item2)).Sum(n => n.Value);

            if (p.IdsCommessa.Count > 0)
                commesse = commesse.Where(c => p.IdsCommessa.Contains(c.IdAz_Commessa)).ToList();
            retVal.Commesse = commesse.OrderBy(c => c.Cliente, StringComparer.CurrentCultureIgnoreCase)
                                      .ThenBy(c => c.Descrizione, StringComparer.CurrentCultureIgnoreCase).ToList();
            Somma(retVal.Totale, retVal.Commesse);

            // dipendenti delle commesse mostrate
            retVal.Dipendenti = retVal.Commesse.SelectMany(c => c.SubCommesse).SelectMany(s => s.Dipendenti)
                                       .GroupBy(d => d.IdAspNetUsers)
                                       .Select(g => new CdgDipendenteModel
                                       {
                                           IdAspNetUsers = g.Key,
                                           NomeDipendente = g.First().NomeDipendente,
                                           Minuti = g.Sum(d => d.Minuti),
                                           NoteSpese = g.Sum(d => d.NoteSpese),
                                           CostoOrario = CostoOrario(g.Key),
                                           DaPredefinito = !costi.ContainsKey(g.Key),
                                       })
                                       .OrderBy(d => d.NomeDipendente, StringComparer.CurrentCultureIgnoreCase)
                                       .ToList();
            retVal.DipendentiSenzaCosto = retVal.Dipendenti.Count(d => d.CostoOrario <= 0 && d.Minuti > 0);
            return retVal;
        }

        private static void Somma(CdgValori totale, IEnumerable<CdgValori> righe)
        {
            var lista = righe.ToList();
            totale.Minuti = lista.Sum(r => r.Minuti);
            totale.CostoPersonale = lista.Sum(r => r.CostoPersonale);
            totale.NoteSpese = lista.Sum(r => r.NoteSpese);
            totale.Ricavo = lista.Any(r => r.Ricavo.HasValue) ? lista.Sum(r => r.Ricavo ?? 0m) : null;
            Completa(totale);
        }

        private static void Completa(CdgValori v)
        {
            v.Costo = v.CostoPersonale + v.NoteSpese;
            v.Margine = v.Ricavo.HasValue ? v.Ricavo.Value - v.Costo : null;
            v.MarginePerc = v.Ricavo is decimal r && r > 0 ? Arrotonda(v.Margine!.Value / r * 100m) : null;
        }
    }
}
