using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_ExportService.Models;
using System.Globalization;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_ExportService
{
    /// <summary>
    /// Scrive i cartellini in un PDF (MigraDoc): un cartellino per sezione, ognuno da pagina nuova.
    /// Non legge dati: riceve i CartellinoPdf gia' pronti, cosi' lo usano sia l'export dell'utente sia quello dell'admin.
    /// I testi sono nella lingua dell'utente (AttendanceTrackingMessages).
    /// </summary>
    public static class TimeSheetPdfWriter
    {
        // segni nelle celle, spiegati dalla legenda sotto la tabella
        public const string SegnoInAttesa = "*";
        public const string SegnoProvvisorie = "°";

        private static readonly Color ColoreIntestazione = new Color(0x1F, 0x4E, 0x79);
        private static readonly Color ColoreFestivo = new Color(0xF2, 0xF2, 0xF2);
        private static readonly Color ColoreBordo = new Color(0xBF, 0xBF, 0xBF);
        private static readonly Color ColorePositivo = new Color(0x2E, 0x7D, 0x32);
        private static readonly Color ColoreNegativo = new Color(0xC6, 0x28, 0x28);
        private static readonly Color ColoreTenue = new Color(0x75, 0x75, 0x75);

        /// <summary>Crea il PDF in <paramref name="folder"/> e ne restituisce il nome del file.</summary>
        public static string Write(IEnumerable<CartellinoPdf> cartellini, string codice, string jobId, string folder)
        {
            PdfFontResolver.EnsureRegistered();

            var document = new Document();
            document.Info.Title = T("CartellinoPdf_Titolo");
            DefinisciStili(document);

            foreach (var cartellino in cartellini)
                AggiungiCartellino(document, cartellino);

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();

            var fileName = $"{codice}_{jobId}.pdf";
            renderer.PdfDocument.Save(Path.Combine(folder, fileName));
            return fileName;
        }

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

        private static void AggiungiCartellino(Document document, CartellinoPdf c)
        {
            var section = document.AddSection();
            section.PageSetup = document.DefaultPageSetup.Clone();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);

            AggiungiPiePagina(section);
            AggiungiIntestazione(section, c);
            AggiungiGiorni(section, c);
            AggiungiRiepilogo(section, c);
        }

        private static void AggiungiPiePagina(Section section)
        {
            var p = section.Footers.Primary.AddParagraph();
            p.Format.Font.Size = 7;
            p.Format.Font.Color = ColoreTenue;
            p.Format.AddTabStop(Unit.FromCentimeter(18), TabAlignment.Right);
            p.AddText(T("CartellinoPdf_GeneratoIl", DateTime.Now.ToString("g", CultureInfo.CurrentCulture)));
            p.AddTab();
            p.AddText(T("CartellinoPdf_Pagina") + " ");
            p.AddPageField();
            p.AddText(" / ");
            p.AddSectionPagesField();
        }

        private static void AggiungiIntestazione(Section section, CartellinoPdf c)
        {
            section.AddParagraph(T("CartellinoPdf_Titolo"), StyleNames.Heading1);

            var table = section.AddTable();
            table.AddColumn(Unit.FromCentimeter(3));
            table.AddColumn(Unit.FromCentimeter(15));

            RigaIntestazione(table, T("CartellinoPdf_Azienda"), c.Azienda);
            RigaIntestazione(table, T("CartellinoPdf_Dipendente"), c.Dipendente);
            RigaIntestazione(table, T("CartellinoPdf_Mese"), NomeMese(c.Anno, c.Mese));
            if (!string.IsNullOrEmpty(c.StatoPeriodo))
                RigaIntestazione(table, T("CartellinoPdf_Stato"), c.StatoPeriodo);
        }

        private static void RigaIntestazione(Table table, string etichetta, string valore)
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(etichetta).Format.Font.Color = ColoreTenue;
            row.Cells[1].AddParagraph(valore).Format.Font.Bold = true;
        }

        private static void AggiungiGiorni(Section section, CartellinoPdf c)
        {
            section.AddParagraph().Format.SpaceAfter = Unit.FromPoint(6);

            var table = section.AddTable();
            table.Borders.Color = ColoreBordo;
            table.Borders.Width = 0.5;
            table.LeftPadding = Unit.FromPoint(3);
            table.RightPadding = Unit.FromPoint(3);
            table.TopPadding = Unit.FromPoint(1.5);
            table.BottomPadding = Unit.FromPoint(1.5);

            table.AddColumn(Unit.FromCentimeter(2.0));
            table.AddColumn(Unit.FromCentimeter(5.4));
            table.AddColumn(Unit.FromCentimeter(4.4));
            table.AddColumn(Unit.FromCentimeter(1.9)).Format.Alignment = ParagraphAlignment.Right;
            table.AddColumn(Unit.FromCentimeter(1.9)).Format.Alignment = ParagraphAlignment.Right;
            table.AddColumn(Unit.FromCentimeter(2.4)).Format.Alignment = ParagraphAlignment.Right;

            var header = table.AddRow();
            header.HeadingFormat = true;   // ripetuta su ogni pagina
            header.Shading.Color = ColoreIntestazione;
            header.Format.Font.Bold = true;
            header.Format.Font.Color = Colors.White;
            header.Cells[0].AddParagraph(T("CartellinoPdf_ColGiorno"));
            header.Cells[1].AddParagraph(T("CartellinoPdf_ColTimbrature"));
            header.Cells[2].AddParagraph(T("CartellinoPdf_ColGiustificativi"));
            header.Cells[3].AddParagraph(T("CartellinoPdf_ColTeoriche"));
            header.Cells[4].AddParagraph(T("CartellinoPdf_ColLavorate"));
            header.Cells[5].AddParagraph(T("CartellinoPdf_ColInPiu"));

            var culture = CultureInfo.CurrentCulture;
            foreach (var g in c.Giorni)
            {
                var row = table.AddRow();
                if (g.Festivo)
                    row.Shading.Color = ColoreFestivo;

                row.Cells[0].AddParagraph(g.Data.ToString("ddd dd", culture));
                row.Cells[1].AddParagraph(g.Timbrature);
                row.Cells[2].AddParagraph(g.Giustificativi);
                row.Cells[3].AddParagraph(g.Min_Teo is int teo && teo != 0 ? HHMM(teo) : "");
                row.Cells[4].AddParagraph(g.Min_Lav is int lav && lav != 0 ? HHMM(lav) : "");

                if (g.Min_InPiu is int inPiu && inPiu != 0)
                {
                    var p = row.Cells[5].AddParagraph(HHMM(inPiu, true) + (g.InPiuProvvisorie ? SegnoProvvisorie : ""));
                    p.Format.Font.Color = g.InPiuProvvisorie ? ColoreTenue : (inPiu < 0 ? ColoreNegativo : ColorePositivo);
                }
            }

            var totali = table.AddRow();
            totali.Format.Font.Bold = true;
            totali.Cells[0].MergeRight = 2;
            totali.Cells[0].AddParagraph(T("CartellinoPdf_Totali"));
            totali.Cells[3].AddParagraph(HHMM(c.Giorni.Sum(g => g.Min_Teo ?? 0)));
            totali.Cells[4].AddParagraph(HHMM(c.Giorni.Sum(g => g.Min_Lav ?? 0)));
            var totInPiu = c.Giorni.Sum(g => g.Min_InPiu ?? 0);
            if (totInPiu != 0)
                totali.Cells[5].AddParagraph(HHMM(totInPiu, true));

            var legenda = section.AddParagraph();
            legenda.Format.SpaceBefore = Unit.FromPoint(3);
            legenda.Format.Font.Size = 7;
            legenda.Format.Font.Color = ColoreTenue;
            legenda.AddText(T("CartellinoPdf_LegendaInAttesa", SegnoInAttesa) + "  ·  " + T("CartellinoPdf_LegendaProvvisorie", SegnoProvvisorie));
        }

        private static void AggiungiRiepilogo(Section section, CartellinoPdf c)
        {
            section.AddParagraph(T("CartellinoPdf_Riepilogo"), StyleNames.Heading2);

            if (c.Riepilogo.Count == 0)
            {
                section.AddParagraph(T("CartellinoPdf_NessunCalcolo")).Format.Font.Color = ColoreTenue;
                return;
            }

            var table = section.AddTable();
            table.Borders.Bottom.Color = ColoreBordo;
            table.Borders.Bottom.Width = 0.5;
            table.TopPadding = Unit.FromPoint(1.5);
            table.BottomPadding = Unit.FromPoint(1.5);
            table.AddColumn(Unit.FromCentimeter(6.5));
            table.AddColumn(Unit.FromCentimeter(2.0)).Format.Alignment = ParagraphAlignment.Right;
            table.AddColumn(Unit.FromCentimeter(9.5));

            foreach (var v in c.Riepilogo)
            {
                var row = table.AddRow();
                row.KeepWith = 0;
                var etichetta = row.Cells[0].AddParagraph(v.Etichetta);
                if (v.Livello1)
                    etichetta.Format.LeftIndent = Unit.FromCentimeter(0.5);
                else
                    etichetta.Format.Font.Bold = true;

                row.Cells[1].AddParagraph(HHMM(v.Minuti, v.Saldo));
                var nota = row.Cells[2].AddParagraph(v.Nota ?? "");
                nota.Format.LeftIndent = Unit.FromCentimeter(0.3);
                nota.Format.Font.Color = ColoreTenue;
            }

            if (!string.IsNullOrEmpty(c.NoteHR))
            {
                var p = section.AddParagraph();
                p.Format.SpaceBefore = Unit.FromPoint(6);
                p.AddFormattedText(T("CartellinoPdf_NoteHR") + " ", TextFormat.Bold);
                p.AddText(c.NoteHR);
            }
        }

        /// <summary>Minuti in ore:minuti (come minutiToHHMM del client), con il + davanti ai positivi se richiesto.</summary>
        public static string HHMM(int minuti, bool conSegnoPiu = false)
        {
            var segno = minuti < 0 ? "-" : (conSegnoPiu && minuti > 0 ? "+" : "");
            var abs = Math.Abs(minuti);
            return $"{segno}{abs / 60}:{abs % 60:00}";
        }

        private static string NomeMese(int anno, int mese)
        {
            var nome = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mese);
            return (nome.Length > 0 ? char.ToUpper(nome[0]) + nome[1..] : nome) + " " + anno;
        }

        private static string T(string key, params object?[] args) => AttendanceTrackingMessages.Get(key, args);
    }
}
