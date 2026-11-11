using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Storage
{
    /*
     Archiviazione dei file degli applicativi fuori dal database (ANALISI_DOCUMENTMANAGEMENT_APPLICATIVO.md [branch DocumentManagement] §5.1).
     Nel database restano solo i metadati e la chiave restituita da SaveAsync.
     Implementazione attuale: file system (decisione: su disco), cartella FileStorage:Root
     (in Docker il volume nvxapp_files montato su /app/Files). Si potranno aggiungere S3/MinIO o database
     senza toccare chi la usa.
    */
    public interface IFileStorage
    {
        /// <summary>Salva il contenuto e restituisce la chiave del file (percorso relativo: &lt;IdAzienda&gt;/&lt;aaaa&gt;/&lt;mm&gt;/&lt;guid&gt;).</summary>
        Task<string> SaveAsync(int idCompany, Stream content, CancellationToken cancellationToken = default);

        /// <summary>Apre il file in lettura; eccezione se non esiste.</summary>
        Task<Stream> OpenAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>Elimina il file (nessun errore se non esiste).</summary>
        Task DeleteAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>Elimina tutti i file dell'azienda (cancellazione fisica dell'azienda).</summary>
        Task DeleteCompanyAsync(int idCompany, CancellationToken cancellationToken = default);
    }

    public class FileSystemFileStorage : IFileStorage
    {
        // chiave: <IdAzienda>/<aaaa>/<mm>/<32 caratteri esadecimali>
        private static readonly Regex _validKey = new Regex(@"^[0-9]+/[0-9]{4}/[0-9]{2}/[0-9a-f]{32}$", RegexOptions.Compiled);

        private readonly string _root;

        public FileSystemFileStorage(IConfiguration configuration)
        {
            var root = configuration["FileStorage:Root"];
            _root = Path.GetFullPath(string.IsNullOrWhiteSpace(root) ? Path.Combine(AppContext.BaseDirectory, "Files") : root);
        }

        public async Task<string> SaveAsync(int idCompany, Stream content, CancellationToken cancellationToken = default)
        {
            if (idCompany <= 0)
                throw new ArgumentOutOfRangeException(nameof(idCompany));

            var now = DateTime.Now;
            var key = $"{idCompany}/{now:yyyy}/{now:MM}/{Guid.NewGuid():N}";
            var path = PathOf(key);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            // scrittura su un file temporaneo e poi rinomina: nessun file a meta' se qualcosa va storto
            var temp = path + ".tmp";
            try
            {
                await using (var file = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                    await content.CopyToAsync(file, cancellationToken);
                File.Move(temp, path);
            }
            catch
            {
                if (File.Exists(temp)) File.Delete(temp);
                throw;
            }
            return key;
        }

        public Task<Stream> OpenAsync(string key, CancellationToken cancellationToken = default)
            => Task.FromResult<Stream>(new FileStream(PathOf(key), FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true));

        public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            var path = PathOf(key);
            if (File.Exists(path))
                File.Delete(path);
            return Task.CompletedTask;
        }

        public Task DeleteCompanyAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            if (idCompany <= 0)
                return Task.CompletedTask;
            var dir = Path.Combine(_root, idCompany.ToString());
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
            return Task.CompletedTask;
        }

        // la chiave arriva dal database: si valida comunque, per non uscire mai dalla cartella radice
        private string PathOf(string key)
        {
            if (string.IsNullOrEmpty(key) || !_validKey.IsMatch(key))
                throw new InvalidOperationException($"Chiave di file non valida: '{key}'.");
            var path = Path.GetFullPath(Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar)));
            if (!path.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Chiave di file non valida: '{key}'.");
            return path;
        }
    }
}
