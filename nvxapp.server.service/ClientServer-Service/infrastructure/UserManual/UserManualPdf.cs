using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using nvxapp.server.data.Localization;
using PdfSharp.Fonts;
using PdfSharp.WPFonts;
using System.Globalization;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.UserManual
{
    /// <summary>
    /// Font dei PDF dei manuali (Segoe WP inclusi nel pacchetto): uguali su Windows e Linux. Il resolver di PDFsharp e'
    /// unico per l'applicazione: si registra solo se nessun altro ramo l'ha gia' fatto (quelli dei rami sono equivalenti).
    /// </summary>
    public class UserManualPdfFontResolver : IFontResolver
    {
        public const string FamilyName = "Segoe WP";
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
                GlobalFontSettings.FontResolver ??= new UserManualPdfFontResolver();
                _registered = true;
            }
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic) => new(isBold ? "SegoeWP-Bold" : "SegoeWP", false, isItalic);

        public byte[]? GetFont(string faceName) => faceName == "SegoeWP-Bold" ? FontDataHelper.SegoeWPBold : FontDataHelper.SegoeWP;
    }

    /// <summary>PDF del manuale: copertina, indice dei capitoli, capitoli con paragrafi, elenchi, passaggi e note.</summary>
    public static class UserManualPdf
    {
        // testi fissi del PDF, presi nella lingua della richiesta (il PDF si scrive in un job in background)
        public sealed class Testi
        {
            public string Pagina { get; init; } = string.Empty;
            public string GeneratoIl { get; init; } = string.Empty;
            public string Indice { get; init; } = string.Empty;

            public static Testi Correnti() => new()
            {
                Pagina = InfrastructureMessages.Get("UserManualPage"),
                GeneratoIl = InfrastructureMessages.Get("UserManualGeneratedOn", DateTime.Now.ToString("d", CultureInfo.CurrentCulture)),
                Indice = InfrastructureMessages.Get("UserManualContents"),
            };
        }

        private static readonly Color Primario = new(0x1F, 0x4E, 0x79);
        private static readonly Color Tenue = new(0x75, 0x75, 0x75);
        private static readonly Color SfondoNota = new(0xE3, 0xEB, 0xF3);

        public static byte[] Crea(UserManualDocument manuale, string sito, Testi testi)
        {
            UserManualPdfFontResolver.EnsureRegistered();

            var document = new Document();
            document.Info.Title = manuale.Title;
            var normal = document.Styles[StyleNames.Normal]!;
            normal.Font.Name = UserManualPdfFontResolver.FamilyName;
            normal.Font.Size = 10;
            normal.ParagraphFormat.SpaceAfter = Unit.FromPoint(5);
            normal.ParagraphFormat.LineSpacingRule = MigraDoc.DocumentObjectModel.LineSpacingRule.OnePtFive;

            var h1 = document.Styles[StyleNames.Heading1]!;
            h1.Font.Size = 16; h1.Font.Bold = true; h1.Font.Color = Primario;
            h1.ParagraphFormat.SpaceBefore = Unit.FromPoint(6); h1.ParagraphFormat.SpaceAfter = Unit.FromPoint(10);
            h1.ParagraphFormat.PageBreakBefore = true; h1.ParagraphFormat.KeepWithNext = true;
            h1.ParagraphFormat.OutlineLevel = OutlineLevel.Level1;
            var h2 = document.Styles[StyleNames.Heading2]!;
            h2.Font.Size = 12; h2.Font.Bold = true; h2.Font.Color = Primario;
            h2.ParagraphFormat.SpaceBefore = Unit.FromPoint(12); h2.ParagraphFormat.SpaceAfter = Unit.FromPoint(4);
            h2.ParagraphFormat.KeepWithNext = true;
            h2.ParagraphFormat.OutlineLevel = OutlineLevel.Level2;

            var section = document.AddSection();
            section.PageSetup = document.DefaultPageSetup.Clone();
            section.PageSetup.PageWidth = Unit.FromCentimeter(21.0);
            section.PageSetup.PageHeight = Unit.FromCentimeter(29.7);
            section.PageSetup.TopMargin = Unit.FromCentimeter(2);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(2);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(2.2);
            section.PageSetup.RightMargin = Unit.FromCentimeter(2.2);
            section.PageSetup.DifferentFirstPageHeaderFooter = true;

            var pie = section.Footers.Primary.AddParagraph();
            pie.Format.Font.Size = 8;
            pie.Format.Font.Color = Tenue;
            pie.Format.AddTabStop(Unit.FromCentimeter(16.6), TabAlignment.Right);
            pie.AddText(sito + " - " + manuale.Title);
            pie.AddTab();
            pie.AddText(testi.Pagina + " ");
            pie.AddPageField();

            // copertina
            var spazio = section.AddParagraph();
            spazio.Format.SpaceBefore = Unit.FromCentimeter(7);
            var titolo = section.AddParagraph(manuale.Title);
            titolo.Format.Font.Size = 26; titolo.Format.Font.Bold = true; titolo.Format.Font.Color = Primario;
            if (!string.IsNullOrWhiteSpace(manuale.Subtitle))
            {
                var sotto = section.AddParagraph(manuale.Subtitle!);
                sotto.Format.Font.Size = 13; sotto.Format.Font.Color = Tenue; sotto.Format.SpaceBefore = Unit.FromPoint(8);
            }
            var data = section.AddParagraph(sito + " · " + testi.GeneratoIl);
            data.Format.SpaceBefore = Unit.FromCentimeter(2); data.Format.Font.Color = Tenue;

            // indice dei capitoli
            var capitoli = manuale.Blocks.Where(b => b.Type == UserManualBlockType.Chapter).ToList();
            if (capitoli.Count > 0)
            {
                var indice = section.AddParagraph(testi.Indice, StyleNames.Heading1);
                indice.Format.OutlineLevel = OutlineLevel.BodyText;
                int n = 0;
                foreach (var cap in capitoli)
                {
                    var p = section.AddParagraph();
                    p.Format.SpaceAfter = Unit.FromPoint(3);
                    p.AddText($"{++n}. {cap.Text}");
                }
            }

            int capitolo = 0;
            foreach (var b in manuale.Blocks)
            {
                switch (b.Type)
                {
                    case UserManualBlockType.Chapter:
                        section.AddParagraph($"{++capitolo}. {b.Text}", StyleNames.Heading1);
                        break;
                    case UserManualBlockType.Section:
                        section.AddParagraph(b.Text, StyleNames.Heading2);
                        break;
                    case UserManualBlockType.Paragraph:
                        Testo(section.AddParagraph(), b.Text);
                        break;
                    case UserManualBlockType.Bullet:
                        var punto = section.AddParagraph();
                        punto.Format.LeftIndent = Unit.FromCentimeter(0.8);
                        punto.Format.FirstLineIndent = Unit.FromCentimeter(-0.5);
                        punto.Format.SpaceAfter = Unit.FromPoint(2);
                        punto.AddText("•  ");
                        Testo(punto, b.Text);
                        break;
                    case UserManualBlockType.Step:
                        var passo = section.AddParagraph();
                        passo.Format.LeftIndent = Unit.FromCentimeter(0.8);
                        passo.Format.FirstLineIndent = Unit.FromCentimeter(-0.6);
                        passo.Format.SpaceAfter = Unit.FromPoint(2);
                        passo.AddFormattedText(b.Number + ".  ", TextFormat.Bold);
                        Testo(passo, b.Text);
                        break;
                    case UserManualBlockType.Note:
                        var nota = section.AddParagraph();
                        nota.Format.Shading.Color = SfondoNota;
                        nota.Format.LeftIndent = Unit.FromCentimeter(0.3);
                        nota.Format.RightIndent = Unit.FromCentimeter(0.3);
                        nota.Format.SpaceBefore = Unit.FromPoint(4);
                        nota.Format.SpaceAfter = Unit.FromPoint(8);
                        Testo(nota, b.Text);
                        break;
                }
            }

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();
            using var ms = new MemoryStream();
            renderer.PdfDocument.Save(ms, false);
            return ms.ToArray();
        }

        // testo con le parti **in grassetto**
        private static void Testo(Paragraph p, string testo)
        {
            var parti = testo.Split("**");
            for (int i = 0; i < parti.Length; i++)
            {
                if (parti[i].Length == 0)
                    continue;
                if (i % 2 == 1)
                    p.AddFormattedText(parti[i], TextFormat.Bold);
                else
                    p.AddText(parti[i]);
            }
        }
    }
}
