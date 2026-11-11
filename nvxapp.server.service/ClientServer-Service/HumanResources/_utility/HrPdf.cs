using System.Text.RegularExpressions;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using PigDocument = UglyToad.PdfPig.PdfDocument;
using SharpDocument = PdfSharp.Pdf.PdfDocument;
using SharpReader = PdfSharp.Pdf.IO.PdfReader;
using SharpOpenMode = PdfSharp.Pdf.IO.PdfDocumentOpenMode;

namespace nvxapp.server.service.ClientServer_Service.HumanResources._utility
{
    /*
     PDF dei cedolini (HR-CED), con le librerie gia' presenti nel progetto:
        - PdfPig  : testo di ogni pagina (come FileText del documentale e della chat AI); le scansioni non hanno testo
        - PDFsharp: nuovo PDF con alcune pagine del PDF unico (il documento del dipendente)
    */
    public static class HrPdf
    {
        // codice fiscale delle persone fisiche, anche con omocodia (cifre sostituite da lettere)
        private static readonly Regex _codiceFiscale = new(@"[A-Z]{6}[0-9LMNPQRSTUV]{2}[A-EHLMPR-T][0-9LMNPQRSTUV]{2}[A-Z][0-9LMNPQRSTUV]{3}[A-Z]", RegexOptions.Compiled);
        // "Matricola" seguita dal codice (anche "Matr.", "N. matricola: 0012")
        private static readonly Regex _matricola = new(@"\bmatr(?:icola)?\.?\s*(?:n\.?|nr\.?|num\.?)?\s*[:\-]?\s*([A-Z0-9][A-Z0-9\-/]{0,19})", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>Testo di ogni pagina (indice 0 = pagina 1).</summary>
        public static List<string> TestoPagine(byte[] pdf)
        {
            using var doc = PigDocument.Open(pdf);
            var pagine = new List<string>();
            foreach (var page in doc.GetPages())
            {
                string testo;
                try { testo = ContentOrderTextExtractor.GetText(page); }
                catch { testo = string.Empty; }
                pagine.Add(testo ?? string.Empty);
            }
            return pagine;
        }

        /// <summary>Codici fiscali trovati nel testo (maiuscolo, senza spazi tra i caratteri del codice).</summary>
        public static HashSet<string> CodiciFiscali(string testo)
        {
            var maiuscolo = testo.ToUpperInvariant();
            var trovati = _codiceFiscale.Matches(maiuscolo).Select(m => m.Value).ToHashSet();
            // codici scritti con spazi tra i caratteri (alcuni gestionali li separano a gruppi)
            var compatto = Regex.Replace(maiuscolo, @"\s+", string.Empty);
            foreach (Match m in _codiceFiscale.Matches(compatto))
                trovati.Add(m.Value);
            return trovati;
        }

        /// <summary>Matricole indicate nel testo con l'etichetta "Matricola".</summary>
        public static HashSet<string> Matricole(string testo)
            => _matricola.Matches(testo).Select(m => NormalizzaMatricola(m.Groups[1].Value)).Where(x => x.Length > 0).ToHashSet();

        public static string NormalizzaMatricola(string? m) => (m ?? string.Empty).Trim().TrimStart('0').ToUpperInvariant();

        /// <summary>Nuovo PDF con le pagine indicate (1, 2, ...) del PDF di partenza.</summary>
        public static byte[] Estrai(byte[] pdf, IEnumerable<int> pagine)
        {
            using var input = SharpReader.Open(new MemoryStream(pdf), SharpOpenMode.Import);
            using var output = new SharpDocument();
            foreach (var p in pagine.Distinct().OrderBy(x => x))
                if (p >= 1 && p <= input.PageCount)
                    output.AddPage(input.Pages[p - 1]);
            using var ms = new MemoryStream();
            output.Save(ms, false);
            return ms.ToArray();
        }

        public static string PagineTesto(IEnumerable<int> pagine) => string.Join(",", pagine.Distinct().OrderBy(x => x));

        public static List<int> PagineLista(string? pagine)
            => (pagine ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                       .Select(x => int.TryParse(x, out var n) ? n : 0).Where(x => x > 0).Distinct().OrderBy(x => x).ToList();

        public static string Taglia(string? s, int max) => s == null ? string.Empty : (s.Length > max ? s[..max] : s);
    }
}
