using ClosedXML.Excel;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_ExportService.Models;
using nvxapp.server.service.Helpers;
using System.Globalization;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_ExportService
{
    /*
     Applica la ricetta del formato (prodotta dall'AI) ai metadati reali: i numeri li scrive il server, quindi sono
     sempre esatti e non ci sono limiti di dipendenti. Una riga di dettaglio per ogni causale di ogni dipendente,
     intestazione e piede facoltativi; testo (campi separati o a larghezza fissa) oppure XLSX (ClosedXML).
     Senza accessi a database: si prova da solo.
    */
    public static class ExportRicettaRenderer
    {
        /// <summary>sorgenti dei campi che la ricetta puo' usare (sono anche nel prompt dell'AI)</summary>
        public static readonly IReadOnlyDictionary<string, string> Sorgenti = new Dictionary<string, string>
        {
            ["costante"] = "testo fisso (Valore)",
            ["azienda.descrizione"] = "ragione sociale dell'azienda",
            ["modello.codice"] = "codice del modello di export",
            ["modello.descrizione"] = "descrizione del modello di export",
            ["periodo.dal"] = "data di inizio del periodo esportato (data)",
            ["periodo.al"] = "data di fine del periodo esportato (data)",
            ["generato"] = "data di creazione del file (data)",
            ["anagrafica.idDipendente"] = "identificativo interno del dipendente (numero)",
            ["anagrafica.cognome"] = "cognome",
            ["anagrafica.nome"] = "nome",
            ["anagrafica.cognomeNome"] = "cognome e nome separati da uno spazio",
            ["anagrafica.matricola"] = "matricola paghe",
            ["anagrafica.codiceFiscale"] = "codice fiscale",
            ["anagrafica.email"] = "email",
            ["anagrafica.username"] = "nome utente",
            ["causale.codice"] = "codice della causale/voce paghe",
            ["causale.periodo"] = "giorno (giornaliera) o primo giorno del mese (mensile) della causale (data)",
            ["causale.tipoUnita"] = "unita' del valore: Ore, Giorni, Importo",
            ["causale.valore"] = "quantita': ore decimali, giorni o euro (numero, puo' essere negativo)",
            ["riga.numero"] = "numero progressivo della riga di dettaglio (numero)",
            ["dipendente.numeroCausali"] = "numero di causali del dipendente nella riga (numero, con una riga per dipendente)",
            ["totale.righe"] = "numero di righe di dettaglio (numero, per il piede)",
            ["totale.valore"] = "somma dei valori di dettaglio (numero, per il piede)",
        };

        private static readonly string[] Date = { "periodo.dal", "periodo.al", "generato", "causale.periodo" };
        private static readonly string[] Numeri = { "anagrafica.idDipendente", "causale.valore", "riga.numero", "dipendente.numeroCausali", "totale.righe", "totale.valore" };
        private static readonly string[] Codifiche = { "utf-8", "utf-8-bom", "windows-1252", "iso-8859-1", "ascii" };

        /// <summary>errori della ricetta (vuoto = valida): l'AI puo' sbagliare, quindi si controlla tutto prima di usarla</summary>
        public static List<string> Valida(ExportRicetta r)
        {
            var errori = new List<string>();
            if (r.Formato is not ("testo" or "xlsx")) errori.Add($"formato '{r.Formato}' non previsto (testo, xlsx)");
            var ext = (r.Estensione ?? string.Empty).Trim().TrimStart('.').ToLowerInvariant();
            if (r.Formato == "xlsx" && ext != "xlsx") errori.Add("con il formato xlsx l'estensione deve essere xlsx");
            if (!NVXSystem.EstensioniAmmesse.Contains(ext) || ext is "pdf" or "zip") errori.Add($"estensione '{r.Estensione}' non ammessa");
            if (!Codifiche.Contains((r.Codifica ?? string.Empty).ToLowerInvariant())) errori.Add($"codifica '{r.Codifica}' non prevista ({string.Join(", ", Codifiche)})");
            if (r.FineRiga is not ("CRLF" or "LF")) errori.Add($"fine riga '{r.FineRiga}' non previsto (CRLF, LF)");
            if (r.RigaPer is not ("causale" or "dipendente")) errori.Add($"riga per '{r.RigaPer}' non prevista (causale, dipendente)");
            if (r.RigaPer == "dipendente")
            {
                // una riga per dipendente: i dati della causale stanno solo nei campi ripetuti
                if (r.CampiCausale.Count == 0) errori.Add("con una riga per dipendente servono i campi ripetuti per ogni causale (campiCausale)");
                foreach (var c in r.Campi.Where(c => c.Campo.StartsWith("causale.")))
                    errori.Add($"con una riga per dipendente il campo '{c.Campo}' va tra i campi ripetuti per causale");
                foreach (var c in r.CampiCausale.Where(c => !c.Campo.StartsWith("causale.") && c.Campo != "costante"))
                    errori.Add($"tra i campi ripetuti per causale il campo '{c.Campo}' non e' previsto (solo causale.* o costante)");
            }
            if (r.Separatore is { Length: > 5 }) errori.Add("separatore troppo lungo");
            if (r.Campi.Count == 0) errori.Add("nessun campo nella riga di dettaglio");
            foreach (var c in r.Campi.Concat(r.CampiCausale).Concat(r.Intestazione.SelectMany(x => x)).Concat(r.Piede.SelectMany(x => x)))
            {
                if (!Sorgenti.ContainsKey(c.Campo)) errori.Add($"campo '{c.Campo}' non previsto");
                if (c.Lunghezza is < 1 or > 500) errori.Add($"lunghezza {c.Lunghezza} del campo '{c.Campo}' non valida");
                if (c.Riempi is { Length: > 1 }) errori.Add($"riempimento '{c.Riempi}' del campo '{c.Campo}': un solo carattere");
                if (c.Decimali is < 0 or > 6) errori.Add($"decimali del campo '{c.Campo}' non validi");
                if (c.Formato != null && Date.Contains(c.Campo))
                {
                    try { _ = new DateTime(2026, 1, 31).ToString(c.Formato, CultureInfo.InvariantCulture); }
                    catch (FormatException) { errori.Add($"formato data '{c.Formato}' non valido"); }
                }
            }
            if (r.Campi.Count > 100 || r.CampiCausale.Count > 50 || r.Intestazione.Count > 20 || r.Piede.Count > 20) errori.Add("ricetta troppo grande");
            return errori;
        }

        /// <summary>file prodotto: contenuto ed estensione</summary>
        public static (byte[] Contenuto, string Estensione) Scrivi(ExportRicetta r, ExportMetadati m)
        {
            var ext = r.Estensione.Trim().TrimStart('.').ToLowerInvariant();
            var righe = Righe(r, m);

            if (r.Formato == "xlsx")
                return (Xlsx(righe), "xlsx");

            var separatore = string.IsNullOrEmpty(r.Separatore) ? null : r.Separatore.Replace("\\t", "\t");
            var fineRiga = r.FineRiga == "LF" ? "\n" : "\r\n";
            var sb = new StringBuilder();
            foreach (var riga in righe)
            {
                var celle = riga.Select(x => separatore != null ? Csv(x.Testo, separatore) : x.Testo);
                sb.Append(string.Join(separatore ?? string.Empty, celle)).Append(fineRiga);
            }
            var contenuto = Codifica(r.Codifica).GetBytes(sb.ToString());
            if (r.Codifica.Equals("utf-8-bom", StringComparison.OrdinalIgnoreCase))
                contenuto = Encoding.UTF8.GetPreamble().Concat(contenuto).ToArray();
            return (contenuto, ext);
        }

        /// <summary>prime righe del file come testo (anteprima nella pagina del modello); xlsx: celle separate da " | "</summary>
        public static List<string> Anteprima(ExportRicetta r, ExportMetadati m, int maxRighe)
        {
            var separatore = r.Formato == "xlsx" ? " | " : string.IsNullOrEmpty(r.Separatore) ? string.Empty : r.Separatore.Replace("\\t", "\t");
            return Righe(r, m).Take(maxRighe)
                              .Select(riga => string.Join(separatore, riga.Select(x => r.Formato != "xlsx" && separatore.Length > 0 ? Csv(x.Testo, separatore) : x.Testo)))
                              .ToList();
        }

        // righe come liste di celle (valore formattato + valore tipizzato per l'xlsx)
        private static List<List<(string Testo, object? Tipizzato)>> Righe(ExportRicetta r, ExportMetadati m)
        {
            var perDipendente = r.RigaPer == "dipendente";
            var dettaglio = m.Dipendenti.SelectMany(d => d.Causali.Select(c => (d, c))).ToList();
            var contesto = new Contesto(m, perDipendente ? m.Dipendenti.Count : dettaglio.Count, dettaglio.Sum(x => x.c.Valore));
            var righe = new List<List<(string Testo, object? Tipizzato)>>();
            foreach (var riga in r.Intestazione)
                righe.Add(riga.Select(c => Cella(c, contesto, null, null, 0)).ToList());
            if (r.TitoliColonne)
                righe.Add(r.Campi.Concat(perDipendente ? r.CampiCausale : Enumerable.Empty<ExportRicetta_Campo>())
                                 .Select(c => ((c.Titolo ?? c.Campo), (object?)(c.Titolo ?? c.Campo))).ToList());
            var n = 0;
            if (perDipendente)
            {
                // una riga per dipendente: i suoi dati, poi i campi di ogni causale uno dopo l'altro
                foreach (var d in m.Dipendenti)
                {
                    n++;
                    var riga = r.Campi.Select(f => Cella(f, contesto, d, null, n)).ToList();
                    foreach (var c in d.Causali)
                        riga.AddRange(r.CampiCausale.Select(f => Cella(f, contesto, d, c, n)));
                    righe.Add(riga);
                }
            }
            else
                foreach (var (d, c) in dettaglio)
                {
                    n++;
                    righe.Add(r.Campi.Select(f => Cella(f, contesto, d, c, n)).ToList());
                }
            foreach (var riga in r.Piede)
                righe.Add(riga.Select(c => Cella(c, contesto, null, null, 0)).ToList());
            return righe;
        }

        private record Contesto(ExportMetadati M, int TotaleRighe, decimal TotaleValore);

        private static (string, object?) Cella(ExportRicetta_Campo f, Contesto ctx, ExportMetadati_Dipendente? d, ExportMetadati_Causale? c, int numero)
        {
            var a = d?.Anagrafica;
            object? valore = f.Campo switch
            {
                "costante" => f.Valore ?? string.Empty,
                "azienda.descrizione" => ctx.M.Azienda.Descrizione,
                "modello.codice" => ctx.M.ModelloExport.Codice,
                "modello.descrizione" => ctx.M.ModelloExport.Descrizione,
                "periodo.dal" => DataDa(ctx.M.Periodo.Dal),
                "periodo.al" => DataDa(ctx.M.Periodo.Al),
                "generato" => ctx.M.GeneratoIl,
                "anagrafica.idDipendente" => (decimal?)a?.IdDipendente,
                "anagrafica.cognome" => a?.Cognome,
                "anagrafica.nome" => a?.Nome,
                "anagrafica.cognomeNome" => a == null ? null : $"{a.Cognome} {a.Nome}".Trim(),
                "anagrafica.matricola" => a?.Matricola,
                "anagrafica.codiceFiscale" => a?.CodiceFiscale,
                "anagrafica.email" => a?.Email,
                "anagrafica.username" => a?.Username,
                "causale.codice" => c?.Codice,
                "causale.periodo" => c == null ? null : DataDa(c.Periodo.Length == 7 ? c.Periodo + "-01" : c.Periodo),
                "causale.tipoUnita" => c?.TipoUnita,
                "causale.valore" => c?.Valore,
                "riga.numero" => (decimal)numero,
                "dipendente.numeroCausali" => d == null ? null : (decimal?)d.Causali.Count,
                "totale.righe" => (decimal)ctx.TotaleRighe,
                "totale.valore" => ctx.TotaleValore,
                _ => null
            };

            string testo;
            bool numeroCampo = Numeri.Contains(f.Campo);
            if (valore is DateTime data)
            {
                var formato = f.Formato ?? (f.Campo == "causale.periodo" && c?.Periodo.Length == 7 ? "yyyy-MM" : "yyyy-MM-dd");
                testo = data.ToString(formato, CultureInfo.InvariantCulture);
            }
            else if (valore is decimal num)
                testo = Numero(num, f);
            else
                testo = valore?.ToString() ?? string.Empty;

            if (f.Lunghezza is int lunghezza)
                testo = Allinea(testo, lunghezza, f, numeroCampo);

            object? tipizzato = valore is decimal dec && f.Lunghezza == null ? dec : testo;
            return (testo, tipizzato);
        }

        private static string Numero(decimal valore, ExportRicetta_Campo f)
        {
            var decimali = f.Decimali ?? (f.Campo is "causale.valore" or "totale.valore" ? 2 : 0);
            var assoluto = Math.Round(Math.Abs(valore), decimali, MidpointRounding.AwayFromZero);
            var testo = assoluto.ToString("F" + decimali, CultureInfo.InvariantCulture);
            var sep = f.SeparatoreDecimali ?? ",";
            if (decimali > 0)
                testo = testo.Replace(".", sep);
            if (valore >= 0 || f.Segno == "nessuno")
                return testo;
            return f.Segno == "finale" ? testo + "-" : "-" + testo;
        }

        private static string Allinea(string testo, int lunghezza, ExportRicetta_Campo f, bool numero)
        {
            var riempi = string.IsNullOrEmpty(f.Riempi) ? ' ' : f.Riempi[0];
            var destra = f.Allinea switch { "destra" => true, "sinistra" => false, _ => numero };
            if (testo.Length >= lunghezza)
                return destra ? testo[^lunghezza..] : testo[..lunghezza];
            // numeri con zeri a sinistra: il segno resta davanti (-0001250)
            if (destra && riempi == '0' && testo.StartsWith('-'))
                return "-" + testo[1..].PadLeft(lunghezza - 1, '0');
            return destra ? testo.PadLeft(lunghezza, riempi) : testo.PadRight(lunghezza, riempi);
        }

        private static string Csv(string testo, string separatore) =>
            testo.Contains(separatore) || testo.Contains('"') || testo.Contains('\n')
                ? "\"" + testo.Replace("\"", "\"\"") + "\""
                : testo;

        private static Encoding Codifica(string codifica)
        {
            switch (codifica.ToLowerInvariant())
            {
                case "windows-1252":
                case "iso-8859-1":
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    return Encoding.GetEncoding(codifica.ToLowerInvariant());
                case "ascii":
                    return Encoding.ASCII;
                default:
                    return new UTF8Encoding(false);
            }
        }

        private static DateTime DataDa(string yyyyMMdd) => DateTime.ParseExact(yyyyMMdd, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        private static byte[] Xlsx(List<List<(string Testo, object? Tipizzato)>> righe)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Export");
            for (int i = 0; i < righe.Count; i++)
                for (int j = 0; j < righe[i].Count; j++)
                {
                    var cella = ws.Cell(i + 1, j + 1);
                    if (righe[i][j].Tipizzato is decimal dec)
                        cella.Value = dec;
                    else
                        cella.Value = righe[i][j].Testo;
                }
            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
