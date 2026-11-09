using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
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
        // colori delle figure generate (copertina, banner dei capitoli, diagrammi di flusso)
        private static readonly Color BandaTesto = new(0xDC, 0xE6, 0xF2);   // testo tenue sulla banda scura
        private static readonly Color NodoSfondo = new(0xEE, 0xF2, 0xF8);   // riquadro dei passi del flusso
        // larghezza utile della pagina (21 cm meno i margini da 1,8 cm)
        private const double LarghezzaUtile = 17.4;

        public static byte[] Crea(UserManualDocument manuale, string sito, Testi testi)
        {
            UserManualPdfFontResolver.EnsureRegistered();

            var document = new Document();
            document.Info.Title = manuale.Title;
            var normal = document.Styles[StyleNames.Normal]!;
            normal.Font.Name = UserManualPdfFontResolver.FamilyName;
            normal.Font.Size = 10;
            normal.ParagraphFormat.SpaceAfter = Unit.FromPoint(4);

            var h1 = document.Styles[StyleNames.Heading1]!;
            h1.Font.Size = 16; h1.Font.Bold = true; h1.Font.Color = Primario;
            // capitoli di seguito, senza pagina nuova: il titolo resta con il testo che lo segue
            h1.ParagraphFormat.SpaceBefore = Unit.FromPoint(16); h1.ParagraphFormat.SpaceAfter = Unit.FromPoint(6);
            h1.ParagraphFormat.KeepWithNext = true;
            h1.ParagraphFormat.OutlineLevel = OutlineLevel.Level1;
            var h2 = document.Styles[StyleNames.Heading2]!;
            h2.Font.Size = 12; h2.Font.Bold = true; h2.Font.Color = Primario;
            h2.ParagraphFormat.SpaceBefore = Unit.FromPoint(8); h2.ParagraphFormat.SpaceAfter = Unit.FromPoint(3);
            h2.ParagraphFormat.KeepWithNext = true;
            h2.ParagraphFormat.OutlineLevel = OutlineLevel.Level2;

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
            pie.Format.AddTabStop(Unit.FromCentimeter(17.4), TabAlignment.Right);
            pie.AddText(sito + " - " + manuale.Title);
            pie.AddTab();
            pie.AddText(testi.Pagina + " ");
            pie.AddPageField();

            // copertina: una banda colorata in testa alla prima pagina (titolo, sottotitolo, sito e data)
            var copertina = section.AddTable();
            copertina.Borders.Width = 0;
            copertina.AddColumn(Unit.FromCentimeter(LarghezzaUtile));
            var cRiga = copertina.AddRow();
            cRiga.TopPadding = Unit.FromPoint(12);
            cRiga.BottomPadding = Unit.FromPoint(12);
            var cCella = cRiga.Cells[0];
            cCella.Shading.Color = Primario;
            cCella.Format.LeftIndent = Unit.FromCentimeter(0.45);
            cCella.Format.RightIndent = Unit.FromCentimeter(0.45);
            var titolo = cCella.AddParagraph(manuale.Title);
            titolo.Format.Font.Size = 22; titolo.Format.Font.Bold = true; titolo.Format.Font.Color = Colors.White;
            if (!string.IsNullOrWhiteSpace(manuale.Subtitle))
            {
                var sotto = cCella.AddParagraph(manuale.Subtitle!);
                sotto.Format.Font.Size = 12; sotto.Format.Font.Color = BandaTesto; sotto.Format.SpaceBefore = Unit.FromPoint(3);
            }
            var data = cCella.AddParagraph(sito + " · " + testi.GeneratoIl);
            data.Format.Font.Size = 9; data.Format.Font.Color = BandaTesto; data.Format.SpaceBefore = Unit.FromPoint(6);
            // spazio sotto la banda prima dell'indice
            section.AddParagraph().Format.SpaceAfter = Unit.FromPoint(8);

            // indice dei capitoli
            var capitoli = manuale.Blocks.Where(b => b.Type == UserManualBlockType.Chapter).ToList();
            if (capitoli.Count > 0)
            {
                var indice = section.AddParagraph(testi.Indice, StyleNames.Heading2);
                indice.Format.OutlineLevel = OutlineLevel.BodyText;
                int n = 0;
                foreach (var cap in capitoli)
                {
                    var p = section.AddParagraph();
                    p.Format.SpaceAfter = Unit.FromPoint(1);
                    p.Format.LeftIndent = Unit.FromCentimeter(0.4);
                    p.AddText($"{++n}. {cap.Text}");
                }
            }

            int capitolo = 0;
            var blocchi = manuale.Blocks;
            for (int i = 0; i < blocchi.Count; i++)
            {
                var b = blocchi[i];
                // passi consecutivi: prima il diagramma di flusso, poi i passi per esteso
                if (b.Type == UserManualBlockType.Step)
                {
                    var passi = new List<UserManualBlock>();
                    int j = i;
                    while (j < blocchi.Count && blocchi[j].Type == UserManualBlockType.Step) { passi.Add(blocchi[j]); j++; }
                    if (passi.Count >= 2 && passi.Count <= 6)
                        FlussoPassi(section, passi);
                    foreach (var p in passi)
                    {
                        var passo = section.AddParagraph();
                        passo.Format.LeftIndent = Unit.FromCentimeter(0.8);
                        passo.Format.FirstLineIndent = Unit.FromCentimeter(-0.6);
                        passo.Format.SpaceAfter = Unit.FromPoint(2);
                        passo.AddFormattedText(p.Number + ".  ", TextFormat.Bold);
                        Testo(passo, p.Text);
                    }
                    i = j - 1;
                    continue;
                }
                switch (b.Type)
                {
                    case UserManualBlockType.Chapter:
                        // banner del capitolo: barra colorata a sinistra e fondo tenue, mantenendo il titolo nell'indice del PDF
                        var cap = section.AddParagraph($"{++capitolo}. {b.Text}", StyleNames.Heading1);
                        cap.Format.Shading.Color = SfondoNota;
                        cap.Format.LeftIndent = Unit.FromCentimeter(0.3);
                        cap.Format.RightIndent = Unit.FromCentimeter(0.1);
                        cap.Format.Borders.Left.Width = 3;
                        cap.Format.Borders.Left.Color = Primario;
                        cap.Format.Borders.DistanceFromLeft = Unit.FromPoint(5);
                        cap.Format.Borders.DistanceFromTop = Unit.FromPoint(3);
                        cap.Format.Borders.DistanceFromBottom = Unit.FromPoint(3);
                        break;
                    case UserManualBlockType.Section:
                        section.AddParagraph(b.Text, StyleNames.Heading2);
                        break;
                    case UserManualBlockType.Subsection:
                        var sotto = section.AddParagraph();
                        sotto.Format.SpaceBefore = Unit.FromPoint(4);
                        sotto.Format.KeepWithNext = true;
                        sotto.AddFormattedText(b.Text, TextFormat.Bold);
                        break;
                    case UserManualBlockType.Image when b.Data != null:
                        // schermata della pagina, larghezza utile, con un bordo sottile; resta con il titolo che la precede
                        var cornice = section.AddParagraph();
                        cornice.Format.SpaceBefore = Unit.FromPoint(2);
                        cornice.Format.SpaceAfter = Unit.FromPoint(6);
                        cornice.Format.Alignment = ParagraphAlignment.Center;
                        var img = cornice.AddImage("base64:" + Convert.ToBase64String(b.Data));
                        img.LockAspectRatio = true;
                        img.Width = Unit.FromCentimeter(15.5);
                        img.LineFormat.Color = new Color(0xBF, 0xBF, 0xBF);
                        img.LineFormat.Width = 0.5;
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
                    // i passi (Step) sono gestiti prima dello switch, con il diagramma di flusso
                    case UserManualBlockType.Note:
                        var nota = section.AddParagraph();
                        nota.Format.Shading.Color = SfondoNota;
                        nota.Format.LeftIndent = Unit.FromCentimeter(0.3);
                        nota.Format.RightIndent = Unit.FromCentimeter(0.3);
                        nota.Format.SpaceBefore = Unit.FromPoint(4);
                        nota.Format.SpaceAfter = Unit.FromPoint(8);
                        Testo(nota, b.Text);
                        break;
                    case UserManualBlockType.Path:
                        // dove si trova la funzione nel menu: riga piccola e tenue sotto il titolo
                        var percorso = section.AddParagraph();
                        percorso.Format.Font.Size = 8.5;
                        percorso.Format.Font.Color = Tenue;
                        percorso.Format.Font.Italic = true;
                        percorso.Format.SpaceAfter = Unit.FromPoint(4);
                        percorso.Format.KeepWithNext = true;
                        percorso.AddText(b.Text);
                        break;
                    case UserManualBlockType.SeeAlso:
                        // funzioni collegate: riga con filetto a sinistra
                        var vedi = section.AddParagraph();
                        vedi.Format.SpaceBefore = Unit.FromPoint(4);
                        vedi.Format.SpaceAfter = Unit.FromPoint(8);
                        vedi.Format.LeftIndent = Unit.FromCentimeter(0.3);
                        vedi.Format.Borders.Left.Width = 2;
                        vedi.Format.Borders.Left.Color = Primario;
                        vedi.Format.Borders.DistanceFromLeft = Unit.FromPoint(5);
                        vedi.Format.Font.Size = 9;
                        Testo(vedi, b.Text);
                        break;
                    case UserManualBlockType.Table when b.Rows is { Count: > 0 }:
                        Tabella(section, b.Rows);
                        break;
                }
            }

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();
            using var ms = new MemoryStream();
            renderer.PdfDocument.Save(ms, false);
            return ms.ToArray();
        }

        // tabella (es. i campi di una pagina): intestazione colorata e ripetuta a ogni pagina, bordi sottili;
        // con due colonne la prima e' piu' stretta (nome / descrizione)
        private static void Tabella(Section section, List<string[]> righe)
        {
            int colonne = righe.Max(r => r.Length);
            var tabella = section.AddTable();
            tabella.Borders.Width = 0.5;
            tabella.Borders.Color = new Color(0xC8, 0xD2, 0xDE);
            tabella.Format.Font.Size = 9;
            tabella.LeftPadding = Unit.FromPoint(4);
            tabella.RightPadding = Unit.FromPoint(4);
            for (int c = 0; c < colonne; c++)
            {
                double larghezza = colonne == 2 ? LarghezzaUtile * (c == 0 ? 0.32 : 0.68) : LarghezzaUtile / colonne;
                tabella.AddColumn(Unit.FromCentimeter(larghezza));
            }
            for (int r = 0; r < righe.Count; r++)
            {
                var riga = tabella.AddRow();
                riga.TopPadding = Unit.FromPoint(2);
                riga.BottomPadding = Unit.FromPoint(2);
                if (r == 0)
                {
                    riga.HeadingFormat = true;
                    riga.Shading.Color = SfondoNota;
                    riga.Format.Font.Bold = true;
                    riga.Format.Font.Color = Primario;
                }
                for (int c = 0; c < colonne; c++)
                    Testo(riga.Cells[c].AddParagraph(), c < righe[r].Length ? righe[r][c] : string.Empty);
            }
            section.AddParagraph().Format.SpaceAfter = Unit.FromPoint(4);
        }

        // diagramma di flusso di una procedura: un riquadro numerato per ogni passo, collegati da frecce
        private static void FlussoPassi(Section section, List<UserManualBlock> passi)
        {
            int n = passi.Count;
            const double larghezzaFreccia = 0.6;
            double larghezzaNodo = (LarghezzaUtile - larghezzaFreccia * (n - 1)) / n;

            var tabella = section.AddTable();
            tabella.Borders.Width = 0;
            for (int k = 0; k < n; k++)
            {
                tabella.AddColumn(Unit.FromCentimeter(larghezzaNodo));
                if (k < n - 1)
                    tabella.AddColumn(Unit.FromCentimeter(larghezzaFreccia));
            }

            var riga = tabella.AddRow();
            riga.TopPadding = Unit.FromPoint(4);
            riga.BottomPadding = Unit.FromPoint(4);
            for (int k = 0; k < n; k++)
            {
                var nodo = riga.Cells[k * 2];
                nodo.Shading.Color = NodoSfondo;
                nodo.Borders.Width = 0.5;
                nodo.Borders.Color = Primario;
                nodo.VerticalAlignment = VerticalAlignment.Center;
                nodo.Format.Alignment = ParagraphAlignment.Center;
                var numero = nodo.AddParagraph((k + 1).ToString());
                numero.Format.Font.Bold = true; numero.Format.Font.Size = 13; numero.Format.Font.Color = Primario;
                numero.Format.SpaceAfter = Unit.FromPoint(1);
                var etichetta = nodo.AddParagraph(Etichetta(passi[k].Text));
                etichetta.Format.Font.Size = 7.5; etichetta.Format.Font.Color = Tenue;

                if (k < n - 1)
                {
                    var freccia = riga.Cells[k * 2 + 1];
                    freccia.VerticalAlignment = VerticalAlignment.Center;
                    freccia.Format.Alignment = ParagraphAlignment.Center;
                    var segno = freccia.AddParagraph(">");
                    segno.Format.Font.Size = 12; segno.Format.Font.Bold = true; segno.Format.Font.Color = Primario;
                }
            }
            section.AddParagraph().Format.SpaceAfter = Unit.FromPoint(4);
        }

        // etichetta breve per il nodo del flusso: prima frase del passo, senza grassetto
        private static string Etichetta(string testo)
        {
            testo = testo.Replace("**", "").Trim();
            int stop = testo.IndexOfAny(new[] { '.', ':', ';' });
            if (stop > 0 && stop <= 40)
                testo = testo[..stop];
            if (testo.Length > 38)
                testo = testo[..37].TrimEnd() + "...";
            return testo;
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
