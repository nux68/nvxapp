using System.Globalization;
using System.Resources;

namespace nvxapp.server.data.Localization
{
    /*
     Testi del server nella lingua dell'utente.
     La lingua e' CultureInfo.CurrentUICulture, impostata per ogni richiesta da UseRequestLocalization
     (header Accept-Language mandato dal client). Senza richiesta (job in background) vale l'italiano.
     I testi stanno in file .resx: <Nome>.resx (italiano, lingua di riferimento) e <Nome>.en.resx.
     Una chiave mancante nella lingua dell'utente usa il testo italiano; una chiave inesistente
     restituisce la chiave stessa.
     Ogni ramo applicativo ha i propri .resx (e la propria istanza di ServerText), cosi' i file dei vari
     rami non si toccano nei merge. Per Infrastructure: InfrastructureMessages.
    */
    public sealed class ServerText
    {
        private readonly ResourceManager _resourceManager;

        public ServerText(string baseName, Type typeInAssembly)
        {
            _resourceManager = new ResourceManager(baseName, typeInAssembly.Assembly);
        }

        /// <summary>testo della chiave, con eventuali parametri {0}, {1}... (string.Format)</summary>
        public string Get(string key, params object?[] args)
        {
            var text = _resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
            return args.Length == 0 ? text : string.Format(CultureInfo.CurrentCulture, text, args);
        }
    }

    /// <summary>Testi del server di Infrastructure (Localization/InfrastructureMessages*.resx).</summary>
    public static class InfrastructureMessages
    {
        private static readonly ServerText _text =
            new("nvxapp.server.data.Localization.InfrastructureMessages", typeof(InfrastructureMessages));

        public static string Get(string key, params object?[] args) => _text.Get(key, args);
    }
}
