using ClosedXML.Excel;
using nvxapp.server.data.Localization;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.DipendentiImportService
{
    /*
     Testo di un file caricato dall'utente, da passare al LLM (che legge solo testo):
       fogli di calcolo xlsx/xlsm  una riga per riga del foglio, celle separate da tabulazione
       documenti docx, odt, ods    paragrafi su righe, tabelle con celle separate da tabulazione
       pdf                         testo delle pagine (le scansioni non hanno testo)
       html                        testo senza tag
       altri                       testo (csv, txt, json, xml...), se il file non e' binario
    */
    internal static class ImportFileText
    {
        private static readonly XNamespace W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        private static readonly XNamespace OdText = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
        private static readonly XNamespace OdTable = "urn:oasis:names:tc:opendocument:xmlns:table:1.0";

        /// <summary>tipo dell'immagine (JPEG, PNG, WEBP, GIF: quelli letti dai modelli con visione), null se non e' un'immagine</summary>
        public static string? MimeImmagine(byte[] dati)
        {
            bool Inizia(int offset, params byte[] firma) =>
                dati.Length >= offset + firma.Length && firma.Select((b, i) => dati[offset + i] == b).All(x => x);

            if (Inizia(0, 0xFF, 0xD8, 0xFF)) return "image/jpeg";
            if (Inizia(0, 0x89, 0x50, 0x4E, 0x47)) return "image/png";
            if (Inizia(0, (byte)'G', (byte)'I', (byte)'F', (byte)'8')) return "image/gif";
            if (Inizia(0, (byte)'R', (byte)'I', (byte)'F', (byte)'F') && Inizia(8, (byte)'W', (byte)'E', (byte)'B', (byte)'P')) return "image/webp";
            return null;
        }

        public static string Estrai(string nomeFile, byte[] dati)
        {
            var ext = Path.GetExtension(nomeFile ?? string.Empty).ToLowerInvariant();
            try
            {
                return ext switch
                {
                    ".xlsx" or ".xlsm" => Excel(dati),
                    ".docx" => Word(dati),
                    ".odt" or ".ods" => OpenDocument(dati),
                    ".pdf" => Pdf(dati),
                    ".htm" or ".html" => Html(Testo(dati)),
                    ".xls" or ".doc" => throw new UserMessageException(AttendanceTrackingMessages.Get("ImportFormatoVecchio")),
                    _ => Testo(dati)
                };
            }
            catch (UserMessageException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UserMessageException(AttendanceTrackingMessages.Get("ImportFileNonLeggibile"), ex);
            }
        }

        private static string Excel(byte[] dati)
        {
            var sb = new StringBuilder();
            using var wb = new XLWorkbook(new MemoryStream(dati));
            foreach (var ws in wb.Worksheets)
            {
                sb.AppendLine($"# {ws.Name}");
                foreach (var row in ws.RowsUsed())
                {
                    var ultima = row.LastCellUsed()?.Address.ColumnNumber ?? 0;
                    if (ultima == 0) continue;
                    var celle = row.Cells(1, ultima).Select(c => c.GetFormattedString().Trim().Replace('\t', ' ').Replace('\n', ' '));
                    sb.AppendLine(string.Join('\t', celle).TrimEnd());
                }
            }
            return sb.ToString();
        }

        private static string Word(byte[] dati)
        {
            using var zip = new ZipArchive(new MemoryStream(dati));
            var entry = zip.GetEntry("word/document.xml") ?? throw new InvalidDataException("word/document.xml mancante");
            using var stream = entry.Open();
            var body = XDocument.Load(stream).Root?.Element(W + "body");
            var sb = new StringBuilder();
            if (body != null)
                Xml(body, sb, W + "t", W + "tab", W + "p", W + "tc", W + "tr");
            return sb.ToString();
        }

        private static string OpenDocument(byte[] dati)
        {
            using var zip = new ZipArchive(new MemoryStream(dati));
            var entry = zip.GetEntry("content.xml") ?? throw new InvalidDataException("content.xml mancante");
            using var stream = entry.Open();
            var root = XDocument.Load(stream).Root;
            var sb = new StringBuilder();
            if (root != null)
                Xml(root, sb, null, OdText + "tab", OdText + "p", OdTable + "table-cell", OdTable + "table-row");
            return sb.ToString();
        }

        /// <summary>testo degli elementi: paragrafi su righe, celle separate da tabulazione, una riga per riga di tabella</summary>
        private static void Xml(XElement e, StringBuilder sb, XName? testo, XName tab, XName paragrafo, XName cella, XName riga)
        {
            foreach (var n in e.Nodes())
            {
                if (n is XText t)
                {
                    // docx: testo solo negli elementi w:t; odt: tutto il testo dei paragrafi
                    if (testo == null) sb.Append(t.Value);
                    continue;
                }
                if (n is not XElement el) continue;

                if (testo != null && el.Name == testo) { sb.Append(el.Value); continue; }
                if (el.Name == tab) { sb.Append('\t'); continue; }

                Xml(el, sb, testo, tab, paragrafo, cella, riga);

                if (el.Name == paragrafo) sb.Append(el.Ancestors(cella).Any() ? ' ' : '\n');
                else if (el.Name == cella)
                {
                    if (sb.Length > 0 && sb[^1] == ' ') sb.Length--;
                    sb.Append('\t');
                }
                else if (el.Name == riga) sb.Append('\n');
            }
        }

        private static string Pdf(byte[] dati)
        {
            var sb = new StringBuilder();
            using var doc = PdfDocument.Open(dati);
            foreach (var page in doc.GetPages())
                sb.AppendLine(ContentOrderTextExtractor.GetText(page));
            return sb.ToString();
        }

        private static string Html(string html)
        {
            var testo = Regex.Replace(html, @"<(script|style)[^>]*>.*?</\1>", " ", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            testo = Regex.Replace(testo, @"</t[dh]>", "\t", RegexOptions.IgnoreCase);
            testo = Regex.Replace(testo, @"<br\s*/?>|</(p|div|tr|li|h\d)>", "\n", RegexOptions.IgnoreCase);
            testo = Regex.Replace(testo, @"<[^>]+>", string.Empty);
            return WebUtility.HtmlDecode(testo);
        }

        /// <summary>testo UTF-8 (o Latin-1 se non e' UTF-8 valido); un file binario non e' leggibile</summary>
        private static string Testo(byte[] dati)
        {
            string testo;
            try
            {
                testo = new UTF8Encoding(false, true).GetString(dati);
            }
            catch (DecoderFallbackException)
            {
                testo = Encoding.Latin1.GetString(dati);
            }
            testo = testo.TrimStart('﻿');

            var controllo = testo.Count(c => char.IsControl(c) && c != '\r' && c != '\n' && c != '\t');
            if (testo.Length == 0 || controllo > testo.Length / 20)
                throw new UserMessageException(AttendanceTrackingMessages.Get("ImportFormatoNonSupportato"));
            return testo;
        }
    }
}
