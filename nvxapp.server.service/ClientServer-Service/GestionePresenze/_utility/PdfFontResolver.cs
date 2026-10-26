using PdfSharp.Fonts;
using PdfSharp.WPFonts;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    /// <summary>
    /// Font dei PDF generati con PDFsharp/MigraDoc: usa i Segoe WP inclusi nel pacchetto,
    /// cosi' i documenti si generano uguali su Windows e Linux senza dipendere dai font installati.
    /// Qualsiasi famiglia richiesta viene resa con Segoe WP (corsivo simulato).
    /// </summary>
    public class PdfFontResolver : IFontResolver
    {
        public const string FamilyName = "Segoe WP";

        private const string FaceRegular = "SegoeWP";
        private const string FaceBold = "SegoeWP-Bold";

        private static readonly object _lock = new();
        private static bool _registered;

        /// <summary>Registra il resolver una sola volta per tutta l'applicazione (PDFsharp lo vuole prima di ogni uso dei font).</summary>
        public static void EnsureRegistered()
        {
            if (_registered)
                return;

            lock (_lock)
            {
                if (_registered)
                    return;

                GlobalFontSettings.FontResolver ??= new PdfFontResolver();
                _registered = true;
            }
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            return new FontResolverInfo(isBold ? FaceBold : FaceRegular, false, isItalic);
        }

        public byte[]? GetFont(string faceName)
        {
            return faceName == FaceBold ? FontDataHelper.SegoeWPBold : FontDataHelper.SegoeWP;
        }
    }
}
