using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Infrastructure.UserManual;

namespace nvxapp.server.service.ClientServer_Service.DocumentManagement._utility
{
    /*
     Verbale PDF per le prese visione chieste dagli altri moduli senza un file (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, DM-FIRMA.2).
     Il contenuto specifico (beni o DPI consegnati, testo dell'accordo...) lo scrive il modulo nel testo della richiesta; il
     documentale aggiunge intestazione (azienda, data, destinatario) e la dichiarazione del modello, scelto dalla "fonte"
     della richiesta:
        - contiene "dpi"                         -> consegna dei dispositivi di protezione individuale
        - contiene "beni", "consegna" o "asset"  -> consegna di beni
        - contiene "accordo" o "agreement"       -> accordo
        - altrimenti                             -> presa visione generica
     Il PDF si genera nella richiesta (pochi KB, nessun job): e' il documento da firmare, non un export.
    */
    public static class DocumentManagementVerbale
    {
        public enum ModelloVerbale
        {
            Generico,
            ConsegnaBeni,
            ConsegnaDpi,
            Accordo,
        }

        public sealed class Dati
        {
            public string? Azienda { get; init; }
            public string Titolo { get; init; } = string.Empty;
            public string? Testo { get; init; }
            public string Destinatario { get; init; } = string.Empty;
            public DateTime Data { get; init; }
            public ModelloVerbale Modello { get; init; }
            /// <summary>al destinatario si chiede la firma (FES) oltre alla presa visione</summary>
            public bool Firma { get; init; }
        }

        private static readonly Color Primario = new(0x1F, 0x4E, 0x79);
        private static readonly Color Tenue = new(0x75, 0x75, 0x75);

        public static ModelloVerbale ModelloDaFonte(string? fonte)
        {
            var f = (fonte ?? string.Empty).ToLowerInvariant();
            if (f.Contains("dpi"))
                return ModelloVerbale.ConsegnaDpi;
            if (f.Contains("beni") || f.Contains("consegna") || f.Contains("asset"))
                return ModelloVerbale.ConsegnaBeni;
            if (f.Contains("accordo") || f.Contains("agreement"))
                return ModelloVerbale.Accordo;
            return ModelloVerbale.Generico;
        }

        public static string Dichiarazione(ModelloVerbale modello) => DocumentManagementMessages.Get(modello switch
        {
            ModelloVerbale.ConsegnaDpi => "AckDeclarationPpe",
            ModelloVerbale.ConsegnaBeni => "AckDeclarationGoods",
            ModelloVerbale.Accordo => "AckDeclarationAgreement",
            _ => "AckDeclarationGeneric"
        });

        public static byte[] Crea(Dati dati)
        {
            UserManualPdfFontResolver.EnsureRegistered();

            var document = new Document();
            document.Info.Title = dati.Titolo;
            var normal = document.Styles[StyleNames.Normal]!;
            normal.Font.Name = UserManualPdfFontResolver.FamilyName;
            normal.Font.Size = 10.5;
            normal.ParagraphFormat.SpaceAfter = Unit.FromPoint(4);

            var section = document.AddSection();
            section.PageSetup = document.DefaultPageSetup.Clone();
            section.PageSetup.PageWidth = Unit.FromCentimeter(21.0);
            section.PageSetup.PageHeight = Unit.FromCentimeter(29.7);
            section.PageSetup.TopMargin = Unit.FromCentimeter(2);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(2);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(2);
            section.PageSetup.RightMargin = Unit.FromCentimeter(2);

            var pie = section.Footers.Primary.AddParagraph(DocumentManagementMessages.Get("AckPdfFooter", dati.Data.ToString("g")));
            pie.Format.Font.Size = 8;
            pie.Format.Font.Color = Tenue;

            if (!string.IsNullOrWhiteSpace(dati.Azienda))
            {
                var az = section.AddParagraph(dati.Azienda);
                az.Format.Font.Bold = true;
                az.Format.SpaceAfter = Unit.FromPoint(14);
            }

            var titolo = section.AddParagraph(dati.Titolo);
            titolo.Format.Font.Size = 16;
            titolo.Format.Font.Bold = true;
            titolo.Format.Font.Color = Primario;
            titolo.Format.SpaceAfter = Unit.FromPoint(10);

            section.AddParagraph(DocumentManagementMessages.Get("AckPdfRecipient", dati.Destinatario));
            var data = section.AddParagraph(DocumentManagementMessages.Get("AckPdfDate", dati.Data.ToString("d")));
            data.Format.SpaceAfter = Unit.FromPoint(12);

            // testo del modulo: una riga per paragrafo (gli elenchi "- ..." restano come sono)
            foreach (var riga in (dati.Testo ?? string.Empty).Replace("\r\n", "\n").Split('\n'))
            {
                var p = section.AddParagraph(riga.TrimEnd());
                if (riga.TrimStart().StartsWith("- "))
                    p.Format.LeftIndent = Unit.FromCentimeter(0.5);
            }

            var dich = section.AddParagraph(Dichiarazione(dati.Modello));
            dich.Format.SpaceBefore = Unit.FromPoint(16);
            dich.Format.Font.Italic = true;

            var modo = section.AddParagraph(DocumentManagementMessages.Get(dati.Firma ? "AckPdfHowSign" : "AckPdfHowAcknowledge"));
            modo.Format.SpaceBefore = Unit.FromPoint(12);
            modo.Format.Font.Size = 9;
            modo.Format.Font.Color = Tenue;

            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();
            using var ms = new MemoryStream();
            renderer.PdfDocument.Save(ms, false);
            return ms.ToArray();
        }
    }
}
