using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Crm.Models;
using PdfSharp.Fonts;
using PdfSharp.WPFonts;
using System.Globalization;

namespace nvxapp.server.service.ClientServer_Service.Crm._utility
{
    /// <summary>
    /// Font dei PDF del CRM (Segoe WP inclusi nel pacchetto, come le presenze): uguali su Windows e Linux.
    /// Il resolver di PDFsharp e' unico per tutta l'applicazione: si registra solo se nessun altro l'ha gia' fatto
    /// (nell'applicativo completo puo' averlo registrato prima quello delle presenze, equivalente).
    /// </summary>
    public class CrmPdfFontResolver : IFontResolver
    {
        public const string FamilyName = "Segoe WP";
        private const string FaceRegular = "SegoeWP";
        private const string FaceBold = "SegoeWP-Bold";
        private static readonly object _lock = new();
        private static bool _registered;

        public static void EnsureRegistered()
        {
            if (_registered)
                return;
            lock (_lock)
            {
                if (_registered)
                    return;
                GlobalFontSettings.FontResolver ??= new CrmPdfFontResolver();
                _registered = true;
            }
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic) => new(isBold ? FaceBold : FaceRegular, false, isItalic);

        public byte[]? GetFont(string faceName) => faceName == FaceBold ? FontDataHelper.SegoeWPBold : FontDataHelper.SegoeWP;
    }

    /// <summary>PDF dell'offerta: intestazione dell'azienda, cliente, righe, totali e condizioni.</summary>
    public static class CrmOffertaPdf
    {
        private static readonly Color ColoreIntestazione = new(0x1F, 0x4E, 0x79);
        private static readonly Color ColoreTesta = new(0xE3, 0xEB, 0xF3);
        private static readonly Color ColoreBordo = new(0xBF, 0xBF, 0xBF);
        private static readonly Color ColoreTenue = new(0x75, 0x75, 0x75);
        private const double LarghezzaCm = 18.0;

        public static byte[] Crea(Crm_OffertaModel o, string azienda, Az_Cliente cliente, string? contatto, string valuta, string? condizioni)
        {
            CrmPdfFontResolver.EnsureRegistered();
            var c = CultureInfo.CurrentCulture;
            string Importo(decimal v) => v.ToString("N2", c) + " " + valuta;

            var document = new Document();
            document.Info.Title = CrmMessages.Get("OfferPdfTitle", o.NumeroCompleto);
            var normal = document.Styles[StyleNames.Normal]!;
            normal.Font.Name = CrmPdfFontResolver.FamilyName;
            normal.Font.Size = 9;

            var section = document.AddSection();
            section.PageSetup = document.DefaultPageSetup.Clone();
            section.PageSetup.PageWidth = Unit.FromCentimeter(21.0);
            section.PageSetup.PageHeight = Unit.FromCentimeter(29.7);
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(2);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);

            var pie = section.Footers.Primary.AddParagraph();
            pie.Format.Font.Size = 7;
            pie.Format.Font.Color = ColoreTenue;
            pie.Format.Alignment = ParagraphAlignment.Right;
            pie.AddText(CrmMessages.Get("OfferPdfPage") + " ");
            pie.AddPageField();
            pie.AddText(" / ");
            pie.AddNumPagesField();

            // azienda e titolo
            var az = section.AddParagraph(azienda);
            az.Format.Font.Size = 14;
            az.Format.Font.Bold = true;
            az.Format.Font.Color = ColoreIntestazione;
            var titolo = section.AddParagraph(CrmMessages.Get("OfferPdfTitle", o.NumeroCompleto));
            titolo.Format.Font.Size = 12;
            titolo.Format.Font.Bold = true;
            titolo.Format.SpaceBefore = Unit.FromPoint(10);
            var date = section.AddParagraph(CrmMessages.Get("OfferPdfDate", o.Data.ToString("d", c))
                                            + (o.ValidaFinoAl.HasValue ? "   " + CrmMessages.Get("OfferPdfValidUntil", o.ValidaFinoAl.Value.ToString("d", c)) : string.Empty));
            date.Format.Font.Color = ColoreTenue;
            date.Format.SpaceAfter = Unit.FromPoint(10);

            // cliente
            var dest = section.AddParagraph();
            dest.Format.LeftIndent = Unit.FromCentimeter(10);
            dest.Format.SpaceAfter = Unit.FromPoint(12);
            dest.AddFormattedText(CrmMessages.Get("OfferPdfCustomer"), TextFormat.Bold);
            dest.AddLineBreak();
            dest.AddText(cliente.Descrizione);
            foreach (var riga in new[] { cliente.Indirizzo, string.Join(" ", new[] { cliente.Cap, cliente.Citta, cliente.Provincia != null ? "(" + cliente.Provincia + ")" : null }.Where(x => !string.IsNullOrWhiteSpace(x))),
                                         cliente.PartitaIva != null ? CrmMessages.Get("OfferPdfVat", cliente.PartitaIva) : null,
                                         contatto != null ? CrmMessages.Get("OfferPdfAttention", contatto) : null })
                if (!string.IsNullOrWhiteSpace(riga))
                {
                    dest.AddLineBreak();
                    dest.AddText(riga);
                }

            if (!string.IsNullOrWhiteSpace(o.Oggetto))
            {
                var ogg = section.AddParagraph();
                ogg.AddFormattedText(CrmMessages.Get("OfferPdfSubject") + " ", TextFormat.Bold);
                ogg.AddText(o.Oggetto);
                ogg.Format.SpaceAfter = Unit.FromPoint(8);
            }

            // righe
            var t = section.AddTable();
            t.Borders.Color = ColoreBordo;
            t.Borders.Width = 0.5;
            var larghezze = new[] { 8.2, 1.8, 1.2, 2.4, 1.4, 3.0 };
            foreach (var l in larghezze)
                t.AddColumn(Unit.FromCentimeter(l));
            var testa = t.AddRow();
            testa.HeadingFormat = true;
            testa.Shading.Color = ColoreTesta;
            testa.Format.Font.Bold = true;
            var titoli = new[] { "OfferPdfColDescription", "OfferPdfColQuantity", "OfferPdfColUm", "OfferPdfColPrice", "OfferPdfColDiscount", "OfferPdfColAmount" };
            for (int i = 0; i < titoli.Length; i++)
            {
                testa.Cells[i].AddParagraph(CrmMessages.Get(titoli[i]));
                if (i != 0 && i != 2) testa.Cells[i].Format.Alignment = ParagraphAlignment.Right;
            }
            foreach (var r in o.Righe)
            {
                var row = t.AddRow();
                row.Cells[0].AddParagraph(r.Descrizione);
                row.Cells[1].AddParagraph(r.Quantita.ToString("0.##", c)).Format.Alignment = ParagraphAlignment.Right;
                row.Cells[2].AddParagraph(r.Um ?? string.Empty);
                row.Cells[3].AddParagraph(r.Prezzo.ToString("N2", c)).Format.Alignment = ParagraphAlignment.Right;
                row.Cells[4].AddParagraph(r.ScontoPerc != 0 ? r.ScontoPerc.ToString("0.##", c) + "%" : string.Empty).Format.Alignment = ParagraphAlignment.Right;
                row.Cells[5].AddParagraph(r.Importo.ToString("N2", c)).Format.Alignment = ParagraphAlignment.Right;
            }

            // totali
            var tot = section.AddTable();
            tot.AddColumn(Unit.FromCentimeter(LarghezzaCm - 6));
            tot.AddColumn(Unit.FromCentimeter(3));
            tot.AddColumn(Unit.FromCentimeter(3));
            foreach (var (chiave, valore, grassetto) in new[] { ("OfferPdfTaxable", o.Imponibile, false), ("OfferPdfVatTotal", o.Iva, false), ("OfferPdfTotal", o.Totale, true) })
            {
                var row = tot.AddRow();
                row.TopPadding = Unit.FromPoint(2);
                row.Cells[1].AddParagraph(CrmMessages.Get(chiave)).Format.Font.Bold = grassetto;
                var p = row.Cells[2].AddParagraph(Importo(valore));
                p.Format.Alignment = ParagraphAlignment.Right;
                p.Format.Font.Bold = grassetto;
            }

            foreach (var testo in new[] { o.Note, condizioni })
                if (!string.IsNullOrWhiteSpace(testo))
                {
                    var p = section.AddParagraph(testo);
                    p.Format.SpaceBefore = Unit.FromPoint(12);
                }

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();
            using var ms = new MemoryStream();
            renderer.PdfDocument.Save(ms, false);
            return ms.ToArray();
        }
    }
}
