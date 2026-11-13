using System.Globalization;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ReportService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService
{
    /*
     Report PDF dei turni (AT-TURNI, ANALISI_ATTENDANCETRACKING_TURNI.md): riempie il modello comune ReportPdf (scritto da
     ReportPdfWriter, come i report delle pagine HR), a partire dagli stessi dati delle pagine:
       - Reparto (Pianificazione turni): dipendenti x giorni con i turni di ogni giorno (anche piu' d'uno), righe delle
         coperture minime e legenda dei turni; settimana in verticale, mese in orizzontale con carattere piccolo
       - Miei (I miei turni): un giorno per riga con turni, orari e ore, totale del periodo e legenda
    */
    public static class TurniReportPdf
    {
        private const string ColoreFineSettimana = "#F2F2F2";
        private const string ColoreInsufficiente = "#C62828";

        public static ReportPdf Reparto(Dip_Turno_PianificazioneOutModel p, DateTime dal, DateTime al)
        {
            var giorni = Giorni(dal, al);
            bool orizzontale = giorni.Count > 7;
            var report = new ReportPdf { Titolo = T("TurniReport_TitoloReparto"), Orizzontale = orizzontale };
            report.Intestazione.Add(Voce(T("TurniReport_Reparto"), p.Reparto ?? string.Empty));
            report.Intestazione.Add(Voce(T("TurniReport_Periodo"), Periodo(dal, al)));

            double larghezza = ReportPdfWriter.LarghezzaUtileCm(orizzontale);
            double colNome = orizzontale ? 3.4 : 4.0;
            double colGiorno = (larghezza - colNome) / giorni.Count;
            var tabella = new ReportPdf_Tabella { FontSize = orizzontale ? 6 : null };
            tabella.Colonne.Add(new ReportPdf_Colonna(T("TurniReport_Dipendente"), colNome));
            foreach (var g in giorni)
                tabella.Colonne.Add(new ReportPdf_Colonna(IntestazioneGiorno(g), colGiorno, ReportPdf_Allineamento.Centro));

            var turni = p.Turni.ToDictionary(t => t.Id);
            foreach (var dip in p.Dipendenti)
            {
                var riga = new ReportPdf_Riga { Celle = { new ReportPdf_Cella(dip.Nominativo) } };
                foreach (var g in giorni)
                {
                    var delGiorno = p.Assegnazioni.Where(a => a.IdDip_RapportoLavoro == dip.IdDip_RapportoLavoro && a.Data.Date == g)
                                                  .Select(a => turni.TryGetValue(a.IdPar_Turno, out var t) ? t : null)
                                                  .Where(t => t != null).Select(t => t!)
                                                  .OrderBy(t => t.OrarioTesto ?? string.Empty).ToList();
                    var cella = new ReportPdf_Cella(string.Join("\n", delGiorno.Select(t => t.Codice)));
                    if (delGiorno.Count == 1 && !string.IsNullOrEmpty(delGiorno[0].Colore))
                    {
                        cella.Sfondo = delGiorno[0].Colore;
                        cella.Colore = "#FFFFFF";
                    }
                    else if (delGiorno.Count == 0 && FineSettimana(g))
                        cella.Sfondo = ColoreFineSettimana;
                    riga.Celle.Add(cella);
                }
                tabella.Righe.Add(riga);
            }

            // coperture minime: assegnati/minimo per turno e giorno (rosse se insufficienti)
            foreach (var tr in p.TurniReparto.Where(t => t.MinPersone > 0))
            {
                if (!turni.TryGetValue(tr.IdPar_Turno, out var turno))
                    continue;
                var riga = new ReportPdf_Riga { Tipo = ReportPdf_TipoRiga.Totale, Celle = { new ReportPdf_Cella(T("TurniReport_Copertura", turno.Codice)) } };
                foreach (var g in giorni)
                {
                    var c = p.Coperture.FirstOrDefault(x => x.IdPar_Turno == tr.IdPar_Turno && x.Data.Date == g);
                    riga.Celle.Add(c == null ? new ReportPdf_Cella(string.Empty)
                                             : new ReportPdf_Cella($"{c.Assegnati}/{c.MinPersone}") { Colore = c.Insufficiente ? ColoreInsufficiente : null });
                }
                tabella.Righe.Add(riga);
            }
            report.Tabelle.Add(tabella);

            var usati = p.Assegnazioni.Select(a => a.IdPar_Turno).Distinct().Where(turni.ContainsKey).Select(id => turni[id]).ToList();
            AggiungiLegenda(report, usati, orizzontale);
            if (p.Assegnazioni.Count == 0)
                report.Note.Add(T("TurniReport_Nessuno"));
            if (p.CopertureInsufficienti > 0)
                report.Note.Add(T("TurniReport_CopertureInsufficienti", p.CopertureInsufficienti));
            return report;
        }

        public static ReportPdf Miei(string nominativo, List<Dip_TurnoUserModel> mieiTurni, List<Par_TurnoModel> modelli, DateTime dal, DateTime al)
        {
            var report = new ReportPdf { Titolo = T("TurniReport_TitoloMiei") };
            report.Intestazione.Add(Voce(T("TurniReport_Dipendente"), nominativo));
            report.Intestazione.Add(Voce(T("TurniReport_Periodo"), Periodo(dal, al)));

            var tabella = new ReportPdf_Tabella();
            tabella.Colonne.Add(new ReportPdf_Colonna(T("TurniReport_Data"), 2.6));
            tabella.Colonne.Add(new ReportPdf_Colonna(T("TurniReport_Giorno"), 2.6));
            tabella.Colonne.Add(new ReportPdf_Colonna(T("TurniReport_Turni"), 3.6));
            // l'orario occupa il resto della pagina (stessa larghezza della legenda)
            tabella.Colonne.Add(new ReportPdf_Colonna(T("TurniReport_Orario"), ReportPdfWriter.LarghezzaUtileCm(false) - 2.6 - 2.6 - 3.6 - 2.2));
            tabella.Colonne.Add(new ReportPdf_Colonna(T("TurniReport_Ore"), 2.2, ReportPdf_Allineamento.Destra));

            var turni = modelli.ToDictionary(t => t.Id);
            int totale = 0;
            foreach (var g in Giorni(dal, al))
            {
                var delGiorno = mieiTurni.Where(t => t.Data.Date == g)
                                         .Select(t => turni.TryGetValue(t.IdPar_Turno, out var m) ? m : null)
                                         .Where(m => m != null).Select(m => m!)
                                         .OrderBy(m => m.OrarioTesto ?? string.Empty).ToList();
                int minuti = delGiorno.Sum(m => m.MinutiTeorici);
                totale += minuti;
                var riga = new ReportPdf_Riga(g.ToString("dd/MM/yyyy"), NomeGiorno(g),
                                              string.Join("\n", delGiorno.Select(m => m.Codice)),
                                              string.Join("\n", delGiorno.Select(m => string.IsNullOrEmpty(m.OrarioTesto) ? m.Descrizione : m.OrarioTesto)),
                                              delGiorno.Count > 0 ? Ore(minuti) : string.Empty);
                if (delGiorno.Count == 1 && !string.IsNullOrEmpty(delGiorno[0].Colore))
                {
                    riga.Celle[2].Sfondo = delGiorno[0].Colore;
                    riga.Celle[2].Colore = "#FFFFFF";
                }
                if (FineSettimana(g))
                    foreach (var c in riga.Celle.Where(c => c.Sfondo == null))
                        c.Sfondo = ColoreFineSettimana;
                tabella.Righe.Add(riga);
            }
            tabella.Righe.Add(new ReportPdf_Riga(ReportPdf_TipoRiga.Totale, new ReportPdf_Cella(T("TurniReport_Totale"), 3), new ReportPdf_Cella(Ore(totale))));
            report.Tabelle.Add(tabella);

            AggiungiLegenda(report, mieiTurni.Select(t => t.IdPar_Turno).Distinct().Where(turni.ContainsKey).Select(id => turni[id]).ToList(), false);
            if (mieiTurni.Count == 0)
                report.Note.Add(T("TurniReport_Nessuno"));
            return report;
        }

        // legenda: turni usati nel periodo con descrizione, orario e ore
        private static void AggiungiLegenda(ReportPdf report, List<Par_TurnoModel> usati, bool orizzontale)
        {
            if (usati.Count == 0)
                return;
            double larghezza = ReportPdfWriter.LarghezzaUtileCm(orizzontale);
            var legenda = new ReportPdf_Tabella { Titolo = T("TurniReport_Legenda") };
            legenda.Colonne.Add(new ReportPdf_Colonna(T("TurniReport_Turni"), 3.0));
            legenda.Colonne.Add(new ReportPdf_Colonna(T("TurniReport_Descrizione"), Math.Max(4.0, larghezza - 3.0 - 6.0 - 2.2)));
            legenda.Colonne.Add(new ReportPdf_Colonna(T("TurniReport_Orario"), 6.0));
            legenda.Colonne.Add(new ReportPdf_Colonna(T("TurniReport_Ore"), 2.2, ReportPdf_Allineamento.Destra));
            foreach (var t in usati.OrderBy(t => t.OrarioTesto ?? string.Empty))
            {
                var riga = new ReportPdf_Riga(t.Codice, t.Descrizione, string.IsNullOrEmpty(t.OrarioTesto) ? T("TurniReport_Riposo") : t.OrarioTesto!, Ore(t.MinutiTeorici));
                if (!string.IsNullOrEmpty(t.Colore))
                {
                    riga.Celle[0].Sfondo = t.Colore;
                    riga.Celle[0].Colore = "#FFFFFF";
                }
                legenda.Righe.Add(riga);
            }
            report.Tabelle.Add(legenda);
        }

        private static List<DateTime> Giorni(DateTime dal, DateTime al)
        {
            var giorni = new List<DateTime>();
            for (var g = dal.Date; g <= al.Date; g = g.AddDays(1))
                giorni.Add(g);
            return giorni;
        }

        private static bool FineSettimana(DateTime g) => g.DayOfWeek == DayOfWeek.Saturday || g.DayOfWeek == DayOfWeek.Sunday;

        private static string IntestazioneGiorno(DateTime g)
            => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(g.DayOfWeek) + "\n" + g.Day;

        private static string NomeGiorno(DateTime g)
        {
            var nome = CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(g.DayOfWeek);
            return nome.Length > 0 ? char.ToUpper(nome[0]) + nome[1..] : nome;
        }

        private static string Periodo(DateTime dal, DateTime al) => $"{dal:dd/MM/yyyy} - {al:dd/MM/yyyy}";

        private static string Ore(int minuti) => $"{minuti / 60}:{minuti % 60:00}";

        private static KeyValuePair<string, string> Voce(string etichetta, string valore) => new KeyValuePair<string, string>(etichetta, valore);

        private static string T(string key, params object?[] args) => AttendanceTrackingMessages.Get(key, args);
    }
}
