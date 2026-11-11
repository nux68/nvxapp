using ClosedXML.Excel;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Payments.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.UserManual;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Payments
{
    /// <summary>contenuto di un rendiconto o di un riepilogo, gia' nella lingua dell'utente (ANALISI_PAGAMENTI_CENTRI.md §5)</summary>
    public class PaymentDocument
    {
        public string Titolo { get; set; } = string.Empty;
        public string Sottotitolo { get; set; } = string.Empty;
        /// <summary>coppie etichetta / valore</summary>
        public List<string[]> Riepilogo { get; set; } = new();
        public List<PaymentDocumentTable> Tabelle { get; set; } = new();
        public string? Nota { get; set; }
        /// <summary>testi del pie' di pagina; vuoti = dai resx (nel job si valorizzano prima con TestiPiePagina)</summary>
        public string? Generato { get; set; }
        public string? Pagina { get; set; }

        /// <summary>fissa i testi del pie' di pagina nella lingua corrente (da chiamare nella richiesta, prima del job)</summary>
        public PaymentDocument TestiPiePagina()
        {
            Generato ??= InfrastructureMessages.Get("PaymentPdfGenerated", PaymentSettlementManager.Data(DateTime.Now));
            Pagina ??= InfrastructureMessages.Get("PaymentPdfPage");
            return this;
        }
    }

    public class PaymentDocumentTable
    {
        public string Titolo { get; set; } = string.Empty;
        /// <summary>la prima riga e' l'intestazione</summary>
        public List<string[]> Righe { get; set; } = new();
        public HashSet<int> ColonneADestra { get; set; } = new();
        /// <summary>true = la prima riga non si stampa (tabella etichetta / valore)</summary>
        public bool SenzaIntestazione { get; set; }
    }

    /*
     PDF del rendiconto o del riepilogo mensile/annuale di un centro (MigraDoc, stessi font del manuale: funziona anche su Linux).
    */
    public static class PaymentReportPdf
    {
        private static readonly Color Primario = new(0x1F, 0x4E, 0x79);
        private static readonly Color Tenue = new(0x75, 0x75, 0x75);
        private static readonly Color Sfondo = new(0xE3, 0xEB, 0xF3);
        private static readonly Color Bordo = new(0xC8, 0xD2, 0xDE);
        private const double LarghezzaUtile = 17.4; // cm, A4 con margini di 1.8

        public static byte[] Crea(PaymentDocument doc)
        {
            UserManualPdfFontResolver.EnsureRegistered();

            var document = new MigraDoc.DocumentObjectModel.Document();
            document.Info.Title = doc.Titolo;
            var normal = document.Styles[StyleNames.Normal]!;
            normal.Font.Name = UserManualPdfFontResolver.FamilyName;
            normal.Font.Size = 9.5;
            normal.ParagraphFormat.SpaceAfter = Unit.FromPoint(3);

            var section = document.AddSection();
            section.PageSetup = document.DefaultPageSetup.Clone();
            section.PageSetup.PageWidth = Unit.FromCentimeter(21.0);
            section.PageSetup.PageHeight = Unit.FromCentimeter(29.7);
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.6);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.8);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.8);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.8);

            var pie = section.Footers.Primary.AddParagraph();
            pie.Format.Font.Size = 8;
            pie.Format.Font.Color = Tenue;
            pie.Format.AddTabStop(Unit.FromCentimeter(LarghezzaUtile), TabAlignment.Right);
            doc.TestiPiePagina();
            pie.AddText(doc.Titolo + " - " + doc.Generato);
            pie.AddTab();
            pie.AddText(doc.Pagina + " ");
            pie.AddPageField();

            var titolo = section.AddParagraph(doc.Titolo);
            titolo.Format.Font.Size = 16;
            titolo.Format.Font.Bold = true;
            titolo.Format.Font.Color = Primario;
            var sotto = section.AddParagraph(doc.Sottotitolo);
            sotto.Format.Font.Size = 11;
            sotto.Format.SpaceAfter = Unit.FromPoint(10);

            if (doc.Riepilogo.Count > 0)
            {
                var righe = new List<string[]> { new[] { string.Empty, string.Empty } };
                righe.AddRange(doc.Riepilogo);
                Tabella(section, new PaymentDocumentTable { Righe = righe, SenzaIntestazione = true, ColonneADestra = { 1 } });
            }

            foreach (var t in doc.Tabelle)
            {
                if (t.Righe.Count <= 1)
                    continue;
                var h = section.AddParagraph(t.Titolo);
                h.Format.Font.Size = 12;
                h.Format.Font.Bold = true;
                h.Format.Font.Color = Primario;
                h.Format.SpaceBefore = Unit.FromPoint(10);
                h.Format.KeepWithNext = true;
                Tabella(section, t);
            }

            if (!string.IsNullOrWhiteSpace(doc.Nota))
            {
                var nota = section.AddParagraph(doc.Nota);
                nota.Format.Font.Size = 8;
                nota.Format.Font.Color = Tenue;
                nota.Format.SpaceBefore = Unit.FromPoint(8);
            }

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();
            using var ms = new MemoryStream();
            renderer.PdfDocument.Save(ms, false);
            return ms.ToArray();
        }

        private static void Tabella(Section section, PaymentDocumentTable t)
        {
            int colonne = t.Righe.Max(r => r.Length);
            var tabella = section.AddTable();
            tabella.Borders.Width = 0.5;
            tabella.Borders.Color = Bordo;
            tabella.Format.Font.Size = 9;
            tabella.LeftPadding = Unit.FromPoint(4);
            tabella.RightPadding = Unit.FromPoint(4);
            for (int c = 0; c < colonne; c++)
            {
                double larghezza = colonne == 2 ? LarghezzaUtile * (c == 0 ? 0.55 : 0.45)
                                 : colonne == 5 ? LarghezzaUtile * new[] { 0.13, 0.32, 0.2, 0.2, 0.15 }[c]
                                 : LarghezzaUtile / colonne;
                var col = tabella.AddColumn(Unit.FromCentimeter(larghezza));
                if (t.ColonneADestra.Contains(c))
                    col.Format.Alignment = ParagraphAlignment.Right;
            }
            for (int r = t.SenzaIntestazione ? 1 : 0; r < t.Righe.Count; r++)
            {
                var riga = tabella.AddRow();
                riga.TopPadding = Unit.FromPoint(2);
                riga.BottomPadding = Unit.FromPoint(2);
                if (r == 0)
                {
                    riga.HeadingFormat = true;
                    riga.Shading.Color = Sfondo;
                    riga.Format.Font.Bold = true;
                    riga.Format.Font.Color = Primario;
                }
                for (int c = 0; c < colonne; c++)
                    riga.Cells[c].AddParagraph(c < t.Righe[r].Length ? t.Righe[r][c] : string.Empty);
            }
            section.AddParagraph().Format.SpaceAfter = Unit.FromPoint(2);
        }
    }

    /*
     Excel dei pagamenti di un periodo (un centro o tutti), con importi e date come valori veri.
     I testi (intestazioni, nomi di funzioni e stati) si preparano nella lingua dell'utente con Testi.Correnti() prima del job:
     nel job si scrive solo il file.
    */
    public static class PaymentReportExcel
    {
        public sealed class Testi
        {
            public string FoglioRiepilogo { get; init; } = string.Empty;
            public string FoglioDettaglio { get; init; } = string.Empty;
            public string[] IntestazioneRiepilogo { get; init; } = Array.Empty<string>();
            public string[] IntestazioneDettaglio { get; init; } = Array.Empty<string>();
            public string SenzaCentro { get; init; } = string.Empty;
            public Dictionary<int, string> Funzioni { get; init; } = new();
            public Dictionary<int, string> StatiPagamento { get; init; } = new();
            public Dictionary<int, string> StatiRendiconto { get; init; } = new();

            public static Testi Correnti() => new()
            {
                FoglioRiepilogo = InfrastructureMessages.Get("PaymentXlsSummary"),
                FoglioDettaglio = InfrastructureMessages.Get("PaymentPdfDetail"),
                IntestazioneRiepilogo = new[]
                {
                    InfrastructureMessages.Get("PaymentPdfDealer"), InfrastructureMessages.Get("PaymentPdfPayments"), InfrastructureMessages.Get("PaymentPdfCollected"),
                    InfrastructureMessages.Get("PaymentPdfRefunded"), InfrastructureMessages.Get("PaymentPdfFee") + " %", InfrastructureMessages.Get("PaymentPdfFee"),
                    InfrastructureMessages.Get("PaymentPdfDealerShareEstimated"), InfrastructureMessages.Get("PaymentPdfPaid"), InfrastructureMessages.Get("PaymentPdfToPay")
                },
                IntestazioneDettaglio = new[]
                {
                    InfrastructureMessages.Get("PaymentPdfColDate"), InfrastructureMessages.Get("PaymentPdfDealer"), InfrastructureMessages.Get("PaymentPdfColSubject"),
                    InfrastructureMessages.Get("PaymentPdfColFunction"), InfrastructureMessages.Get("PaymentXlsLevel"), InfrastructureMessages.Get("PaymentXlsFrom"),
                    InfrastructureMessages.Get("PaymentXlsTo"), InfrastructureMessages.Get("PaymentPdfColAmount"), InfrastructureMessages.Get("PaymentXlsCurrency"),
                    InfrastructureMessages.Get("PaymentXlsPaymentState"), "Debug", InfrastructureMessages.Get("PaymentPdfSettlement")
                },
                SenzaCentro = InfrastructureMessages.Get("PaymentNoDealer"),
                Funzioni = Enum.GetValues<AmbitoLicenza>().ToDictionary(a => (int)a, PaymentSettlementManager.NomeFunzione),
                StatiPagamento = Enum.GetValues<StatoPagamento>().ToDictionary(x => (int)x, x => InfrastructureMessages.Get("PaymentState_" + (int)x)),
                StatiRendiconto = Enum.GetValues<StatoRendiconto>().ToDictionary(x => (int)x, PaymentSettlementManager.NomeStato)
            };
        }

        public static byte[] Crea(string titolo, List<PaymentRowModel> pagamenti, List<PaymentDealerSummaryModel> riepilogo, Testi testi)
        {
            using var wb = new XLWorkbook();

            var ws = wb.Worksheets.Add(Foglio(testi.FoglioRiepilogo));
            ws.Cell(1, 1).Value = titolo;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            Intestazione(ws, 3, testi.IntestazioneRiepilogo);
            int row = 4;
            foreach (var r in riepilogo)
            {
                ws.Cell(row, 1).Value = r.Dealer ?? testi.SenzaCentro;
                ws.Cell(row, 2).Value = r.NumeroPagamenti;
                ws.Cell(row, 3).Value = r.Incassato;
                ws.Cell(row, 4).Value = r.Rimborsato;
                ws.Cell(row, 5).Value = r.PercentualePiattaforma;
                ws.Cell(row, 6).Value = r.QuotaPiattaforma;
                ws.Cell(row, 7).Value = r.QuotaCentro;
                ws.Cell(row, 8).Value = r.Liquidato;
                ws.Cell(row, 9).Value = r.DaLiquidare;
                row++;
            }
            ws.Range(4, 3, Math.Max(row - 1, 4), 9).Style.NumberFormat.Format = "#,##0.00";
            ws.Columns().AdjustToContents();

            var wp = wb.Worksheets.Add(Foglio(testi.FoglioDettaglio));
            Intestazione(wp, 1, testi.IntestazioneDettaglio);
            row = 2;
            foreach (var p in pagamenti)
            {
                if (p.DataPagamento != null)
                    wp.Cell(row, 1).Value = p.DataPagamento.Value;
                wp.Cell(row, 2).Value = p.Dealer ?? testi.SenzaCentro;
                wp.Cell(row, 3).Value = p.Soggetto ?? string.Empty;
                wp.Cell(row, 4).Value = testi.Funzioni.TryGetValue(p.Ambito, out var f) ? f : p.Ambito.ToString();
                wp.Cell(row, 5).Value = ((LivelloLicenza)p.Livello).ToString();
                wp.Cell(row, 6).Value = p.DataInizioCoperta;
                wp.Cell(row, 7).Value = p.DataFineCoperta;
                wp.Cell(row, 8).Value = p.Importo;
                wp.Cell(row, 9).Value = p.Valuta;
                wp.Cell(row, 10).Value = testi.StatiPagamento.TryGetValue(p.Stato, out var sp) ? sp : p.Stato.ToString();
                wp.Cell(row, 11).Value = p.Debug ? "x" : string.Empty;
                wp.Cell(row, 12).Value = p.StatoRendiconto != null && testi.StatiRendiconto.TryGetValue(p.StatoRendiconto.Value, out var sr) ? sr : string.Empty;
                row++;
            }
            if (row > 2)
            {
                wp.Range(2, 1, row - 1, 1).Style.DateFormat.Format = "dd/MM/yyyy";
                wp.Range(2, 6, row - 1, 7).Style.DateFormat.Format = "dd/MM/yyyy";
                wp.Range(2, 8, row - 1, 8).Style.NumberFormat.Format = "#,##0.00";
            }
            wp.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // i nomi dei fogli Excel: al massimo 31 caratteri, senza : \ / ? * [ ]
        private static string Foglio(string nome)
        {
            var pulito = new string(nome.Where(c => !":\\/?*[]".Contains(c)).ToArray());
            return pulito.Length > 31 ? pulito[..31] : pulito;
        }

        private static void Intestazione(IXLWorksheet ws, int riga, string[] titoli)
        {
            for (int i = 0; i < titoli.Length; i++)
            {
                var cell = ws.Cell(riga, i + 1);
                cell.Value = titoli[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E3EBF3");
            }
        }
    }
}
