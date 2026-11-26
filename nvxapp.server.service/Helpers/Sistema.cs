using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace nvxapp.server.service.Helpers
{
    public static class NVXSystem
    {

        public static string DownloadURL { get; } = "Account/Download/";

        public static string ExportFolder
        {
            get
            {
                string exportsFolder = Path.Combine(Path.GetTempPath(), "nvxapp_exports");
                Directory.CreateDirectory(exportsFolder);
                return exportsFolder;
            }
        }


        /*
         File prodotti dai job in background (export, cartellini, report...). Il tipo del file non e' noto a priori
         (es. lo sceglie l'AI): l'estensione passa sempre dalla lista EstensioniAmmesse e il nome su disco e'
         {jobId}.{ext}, senza testo che arriva dall'utente o dall'AI. Accanto al file si scrive {file}.meta con
         l'utente che ha lanciato il job e il nome da mostrare: il download lo controlla (Account/Download).
         I file si cancellano dopo Export:RetentionHours (JobFileCleanupService).
        */

        public static readonly IReadOnlySet<string> EstensioniAmmesse = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "csv", "txt", "tsv", "json", "xml", "xlsx", "pdf", "dat", "prn", "zip"
        };

        private const string EstensioneMeta = ".meta";

        /// <summary>Salva il file del job e lo registra: owner = utente a cui arrivano le notifiche del job (UserIdFirstConnection).</summary>
        public static JobFile SalvaFileJob(string jobId, string owner, string nomeVisibile, string estensione, byte[] contenuto)
        {
            var fileName = NomeFileJob(jobId, estensione);
            File.WriteAllBytes(Path.Combine(ExportFolder, fileName), contenuto);
            return RegistraFileJob(fileName, owner, nomeVisibile);
        }

        /// <summary>Registra un file gia' scritto in ExportFolder da un writer (es. PDF): owner e nome da mostrare.</summary>
        public static JobFile RegistraFileJob(string fileName, string owner, string nomeVisibile)
        {
            if (!NomeFileJobValido(fileName))
                throw new ArgumentException($"Nome file del job non valido: {fileName}", nameof(fileName));

            var nome = NomeVisibilePulito(nomeVisibile, Path.GetExtension(fileName).TrimStart('.'));
            var info = new JobFileInfo { Owner = owner, FileName = nome, Creato = DateTime.UtcNow };
            File.WriteAllText(Path.Combine(ExportFolder, fileName + EstensioneMeta), JsonSerializer.Serialize(info));

            return new JobFile($"{DownloadURL}{fileName}", nome);
        }

        /// <summary>Nome su disco del file di un job: {jobId}.{ext}, con l'estensione controllata.</summary>
        public static string NomeFileJob(string jobId, string estensione)
        {
            var ext = (estensione ?? string.Empty).Trim().TrimStart('.').ToLowerInvariant();
            if (!EstensioniAmmesse.Contains(ext))
                throw new ArgumentException($"Estensione non ammessa: {estensione}", nameof(estensione));
            if (!Guid.TryParse(jobId, out var guid))
                throw new ArgumentException($"JobId non valido: {jobId}", nameof(jobId));
            return $"{guid}.{ext}";
        }

        /// <summary>Nome semplice (niente cartelle) con estensione ammessa: l'unico che il download accetta.</summary>
        public static bool NomeFileJobValido(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName
                || fileName.Contains("..") || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return false;
            return EstensioniAmmesse.Contains(Path.GetExtension(fileName).TrimStart('.'));
        }

        /// <summary>Dati di registrazione del file; null per i file scritti senza registrazione (job non ancora adeguati).</summary>
        public static JobFileInfo? InfoFileJob(string fileName)
        {
            var path = Path.Combine(ExportFolder, fileName + EstensioneMeta);
            if (!File.Exists(path))
                return null;
            try
            {
                return JsonSerializer.Deserialize<JobFileInfo>(File.ReadAllText(path));
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>Cancella i file (e le registrazioni) piu' vecchi di <paramref name="eta"/>; restituisce quanti file ha cancellato.</summary>
        public static int PulisciFileJob(TimeSpan eta)
        {
            var limite = DateTime.UtcNow - eta;
            var cancellati = 0;
            foreach (var file in new DirectoryInfo(ExportFolder).EnumerateFiles())
            {
                if (file.LastWriteTimeUtc >= limite)
                    continue;
                try
                {
                    file.Delete();
                    cancellati++;
                }
                catch (IOException)
                {
                    // in uso (download in corso): si riprova al giro successivo
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
            return cancellati;
        }

        private static string NomeVisibilePulito(string? nome, string estensione)
        {
            var nonValidi = Path.GetInvalidFileNameChars();
            var pulito = new string((nome ?? string.Empty).Select(c => nonValidi.Contains(c) ? '_' : c).ToArray()).Trim();
            if (pulito.Length == 0)
                pulito = "export";
            if (!pulito.EndsWith("." + estensione, StringComparison.OrdinalIgnoreCase))
                pulito += "." + estensione;
            return pulito;
        }

    }

    /*
     Collegamento temporaneo al file di un job, per scaricarlo con una normale navigazione (senza sessione):
     nell'app installata sul telefono il download costruito in memoria (blob) a volte apre il file o non fa nulla,
     mentre un indirizzo del server con "allegato" scarica sempre. Il codice "<scadenza>.<firma>" (HMAC-SHA256 con la
     chiave dell'installazione su scadenza e nome del file) lo chiede l'utente che ha lanciato il job, gia' collegato
     (Account/DownloadCode), e vale 15 minuti solo per quel file: il client lo chiede appena il job finisce, cosi' al tocco
     il download parte subito, dentro il gesto dell'utente (Chrome blocca i download che arrivano dopo un'attesa).
    */
    public static class JobFileLink
    {
        public static readonly TimeSpan Durata = TimeSpan.FromMinutes(15);

        public static string Code(string key, string fileName, DateTime expiresUtc)
        {
            string scadenza = new DateTimeOffset(expiresUtc, TimeSpan.Zero).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);
            return scadenza + "." + Firma(key, scadenza, fileName);
        }

        public static bool Check(string key, string fileName, string? code, DateTime nowUtc)
        {
            var parti = (code ?? string.Empty).Trim().Split('.');
            if (parti.Length != 2 || !long.TryParse(parti[0], out var secondi))
                return false;
            var attesa = Encoding.ASCII.GetBytes(Firma(key, parti[0], fileName));
            var ricevuta = Encoding.ASCII.GetBytes(parti[1]);
            return CryptographicOperations.FixedTimeEquals(attesa, ricevuta)
                   && DateTimeOffset.FromUnixTimeSeconds(secondi).UtcDateTime >= nowUtc;
        }

        private static string Firma(string key, string scadenza, string fileName)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("job-download:" + key));
            var firma = hmac.ComputeHash(Encoding.UTF8.GetBytes(scadenza + "|" + fileName));
            return Convert.ToBase64String(firma, 0, 24).Replace('+', '-').Replace('/', '_');
        }
    }

    /// <summary>File di un job: URL da mettere in LongJobProgressUpdate.DownloadUrl e nome da mostrare (FileName).</summary>
    public record JobFile(string DownloadUrl, string FileName);

    public class JobFileInfo
    {
        public string Owner { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime Creato { get; set; }
    }

}
