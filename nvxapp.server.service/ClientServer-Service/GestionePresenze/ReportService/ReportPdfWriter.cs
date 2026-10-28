using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using System.Globalization;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ReportService
{
    /// <summary>
    /// Scrive un ReportPdf (MigraDoc) con lo stile del cartellino (TimeSheetPdfWriter): titolo, intestazione,
    /// tabelle con la testata ripetuta su ogni pagina, righe di gruppo e di totale, note in fondo.
    /// Non legge dati: riceve il report gia' pronto.
    /// </summary>
    public static class ReportPdfWriter
    {
        private static readonly Color ColoreIntestazione = new Color(0x1F, 0x4E, 0x79);
        private static readonly Color ColoreGruppo = new Color(0xE3, 0xEB, 0xF3);
        private static readonly Color ColoreTotale = new Color(0xF2, 0xF2, 0xF2);
        private static readonly Color ColoreBordo = new Color(0xBF, 0xBF, 0xBF);
        private static readonly Color ColoreTenue = new Color(0x75, 0x75, 0x75);

        private const double MargineCm = 1.2;

        /// <summary>Crea il PDF in <paramref name="folder"/> e ne restituisce il nome del file.</summary>
        public static string Write(ReportPdf report, string codice, string jobId, string folder)
        {
            PdfFontResolver.EnsureRegistered();

            var document = new Document();
            document.Info.Title = report.Titolo;
            DefinisciStili(document);

            // A4 con le misure esplicite: Orientation sul PageSetup clonato non gira la pagina
            var section = document.AddSection();
            section.PageSetup = document.DefaultPageSetup.Clone();
            section.PageSetup.PageWidth = Unit.FromCentimeter(report.Orizzontale ? 29.7 : 21.0);
            section.PageSetup.PageHeight = Unit.FromCentimeter(report.Orizzontale ? 21.0 : 29.7);
            section.PageSetup.Orientation = Orientation.Portrait;
            section.PageSetup.TopMargin = Unit.FromCentimeter(MargineCm);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(MargineCm + 0.6);   // spazio per il pie' di pagina
            section.PageSetup.FooterDistance = Unit.FromCentimeter(0.7);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(MargineCm);
            section.PageSetup.RightMargin = Unit.FromCentimeter(MargineCm);

            AggiungiPiePagina(section, report.Orizzontale);
            AggiungiIntestazione(section, report);
            foreach (var tabella in report.Tabelle)
                AggiungiTabella(section, tabella);
            AggiungiNote(section, report.Note);

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();

            var fileName = $"{codice}_{jobId}.pdf";
            renderer.PdfDocument.Save(Path.Combine(folder, fileName));
            return fileName;
        }

        /// <summary>Larghezza utile della pagina (cm), per dividere le colonne.</summary>
        public static double LarghezzaUtileCm(bool orizzontale) => (orizzontale ? 29.7 : 21.0) - 2 * MargineCm;

        private static void DefinisciStili(Document document)
        {
            var normal = document.Styles[StyleNames.Normal]!;
            normal.Font.Name = PdfFontResolver.FamilyName;
            normal.Font.Size = 9;

            var titolo = document.Styles[StyleNames.Heading1]!;
            titolo.Font.Size = 15;
            titolo.Font.Bold = true;
            titolo.Font.Color = ColoreIntestazione;
            titolo.ParagraphFormat.SpaceAfter = Unit.FromPoint(6);

            var sottotitolo = document.Styles[StyleNames.Heading2]!;
            sottotitolo.Font.Size = 11;
            sottotitolo.Font.Bold = true;
            sottotitolo.Font.Color = ColoreIntestazione;
            sottotitolo.ParagraphFormat.SpaceBefore = Unit.FromPoint(12);
            sottotitolo.ParagraphFormat.SpaceAfter = Unit.FromPoint(4);
            sottotitolo.ParagraphFormat.KeepWithNext = true;
        }

        private static void AggiungiPiePagina(Section section, bool orizzontale)
        {
            var p = section.Footers.Primary.AddParagraph();
            p.Format.Font.Size = 7;
            p.Format.Font.Color = ColoreTenue;
            p.Format.AddTabStop(Unit.FromCentimeter(LarghezzaUtileCm(orizzontale)), TabAlignment.Right);
            p.AddText(T("CartellinoPdf_GeneratoIl", DateTime.Now.ToString("g", CultureInfo.CurrentCulture)));
            p.AddTab();
            p.AddText(T("CartellinoPdf_Pagina") + " ");
            p.AddPageField();
            p.AddText(" / ");
            p.AddSectionPagesField();
        }

        private static void AggiungiIntestazione(Section section, ReportPdf report)
        {
            section.AddParagraph(report.Titolo, StyleNames.Heading1);
            if (report.Intestazione.Count == 0)
                return;

            var table = section.AddTable();
            table.AddColumn(Unit.FromCentimeter(3.5));
            table.AddColumn(Unit.FromCentimeter(LarghezzaUtileCm(report.Orizzontale) - 3.5));
            foreach (var voce in report.Intestazione)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(voce.Key).Format.Font.Color = ColoreTenue;
                row.Cells[1].AddParagraph(voce.Value).Format.Font.Bold = true;
            }
        }

        private static void AggiungiTabella(Section section, ReportPdf_Tabella t)
        {
            if (!string.IsNullOrEmpty(t.Titolo))
                section.AddParagraph(t.Titolo, StyleNames.Heading2);
            else
                section.AddParagraph().Format.SpaceAfter = Unit.FromPoint(6);

            if (t.Righe.Count == 0)
            {
                section.AddParagraph(T("Report_NessunDato")).Format.Font.Color = ColoreTenue;
                return;
            }

            var table = section.AddTable();
            table.Borders.Color = ColoreBordo;
            table.Borders.Width = 0.5;
            table.LeftPadding = Unit.FromPoint(2.5);
            table.RightPadding = Unit.FromPoint(2.5);
            table.TopPadding = Unit.FromPoint(1.5);
            table.BottomPadding = Unit.FromPoint(1.5);
            if (t.FontSize != null)
                table.Format.Font.Size = Unit.FromPoint(t.FontSize.Value);

            foreach (var c in t.Colonne)
                table.AddColumn(Unit.FromCentimeter(c.LarghezzaCm)).Format.Alignment = Allineamento(c.Allineamento);

            var header = table.AddRow();
            header.HeadingFormat = true;   // ripetuta su ogni pagina
            header.Shading.Color = ColoreIntestazione;
            header.Format.Font.Bold = true;
            header.Format.Font.Color = Colors.White;
            header.VerticalAlignment = VerticalAlignment.Center;
            for (int i = 0; i < t.Colonne.Count; i++)
                AggiungiTesto(header.Cells[i], t.Colonne[i].Titolo);

            foreach (var r in t.Righe)
            {
                var row = table.AddRow();
                row.VerticalAlignment = VerticalAlignment.Center;
                switch (r.Tipo)
                {
                    case ReportPdf_TipoRiga.Gruppo:
                        row.Shading.Color = ColoreGruppo;
                        row.Format.Font.Bold = true;
                        row.KeepWith = 1;   // il gruppo non resta da solo in fondo alla pagina
                        break;
                    case ReportPdf_TipoRiga.Totale:
                        row.Shading.Color = ColoreTotale;
                        row.Format.Font.Bold = true;
                        break;
                }

                int col = 0;
                foreach (var cella in r.Celle)
                {
                    if (col >= t.Colonne.Count)
                        break;
                    var cell = row.Cells[col];
                    if (cella.Unisci > 0)
                        cell.MergeRight = Math.Min(cella.Unisci, t.Colonne.Count - col - 1);
                    if (Colore(cella.Sfondo) is Color sfondo)
                        cell.Shading.Color = sfondo;
                    var p = AggiungiTesto(cell, cella.Testo);
                    if (Colore(cella.Colore) is Color colore)
                        p.Format.Font.Color = colore;
                    else if (cella.Tenue)
                        p.Format.Font.Color = ColoreTenue;
                    col += 1 + cell.MergeRight;
                }
            }
        }

        private static Paragraph AggiungiTesto(Cell cell, string testo)
        {
            var p = cell.AddParagraph();
            var righe = (testo ?? string.Empty).Split('\n');
            for (int i = 0; i < righe.Length; i++)
            {
                if (i > 0)
                    p.AddLineBreak();
                p.AddText(righe[i]);
            }
            return p;
        }

        private static void AggiungiNote(Section section, List<string> note)
        {
            foreach (var nota in note)
            {
                var p = section.AddParagraph(nota);
                p.Format.SpaceBefore = Unit.FromPoint(4);
                p.Format.Font.Size = 7.5;
                p.Format.Font.Color = ColoreTenue;
            }
        }

        private static ParagraphAlignment Allineamento(ReportPdf_Allineamento a) => a switch
        {
            ReportPdf_Allineamento.Destra => ParagraphAlignment.Right,
            ReportPdf_Allineamento.Centro => ParagraphAlignment.Center,
            _ => ParagraphAlignment.Left
        };

        // "#RRGGBB" o "#RGB" (es. i colori dei giustificativi); altrimenti null
        private static Color? Colore(string? hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return null;
            var s = hex.Trim().TrimStart('#');
            if (s.Length == 3)
                s = string.Concat(s.Select(ch => $"{ch}{ch}"));
            if (s.Length != 6 || !uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
                return null;
            return new Color((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        private static string T(string key, params object?[] args) => AttendanceTrackingMessages.Get(key, args);
    }
}
