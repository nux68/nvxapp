using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Tenant.DocumentManagement;
using nvxapp.server.data.Repositories.Tenant.DocumentManagement;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.DocumentManagement._utility
{
    /*
     Eliminazione definitiva dei file del documentale (versioni e documenti): file nello storage, righe nel
     database e spazio liberato. Usata dal cestino (servizio e svuotamento automatico) e dalla potatura delle versioni.
    */
    public static class DocumentFiles
    {
        /// <summary>Elimina le versioni indicate (file e righe) e libera lo spazio.</summary>
        public static async Task EliminaVersioniAsync(IDoc_VersioneRepository versioneRepository, IFileStorage fileStorage, IStorageQuotaService quota,
                                                      int idCompany, List<Doc_Versione> versioni, CancellationToken cancellationToken = default)
        {
            if (versioni.Count == 0)
                return;

            var ids = versioni.Select(v => v.Id).ToList();
            await versioneRepository.FindAll(v => ids.Contains(v.Id)).ExecuteDeleteAsync(cancellationToken);
            await quota.FreeAsync(idCompany, versioni.Sum(v => v.Dimensione), versioni.Count, cancellationToken);

            // i file dopo il database: un file rimasto e' solo spazio sprecato, un file mancante sarebbe un documento rotto
            foreach (var v in versioni)
            {
                try { await fileStorage.DeleteAsync(v.StorageKey, cancellationToken); }
                catch (Exception ex) { Log.Error(ex, "[Documentale] File {Key} non eliminato", v.StorageKey); }
            }
        }

        /// <summary>Elimina definitivamente i documenti (con tutte le versioni) e libera lo spazio.</summary>
        public static async Task<int> EliminaDocumentiAsync(IDoc_DocumentoRepository documentoRepository, IDoc_VersioneRepository versioneRepository, IFileStorage fileStorage, IStorageQuotaService quota,
                                                            int idCompany, List<int> idDocumenti, CancellationToken cancellationToken = default)
        {
            if (idDocumenti.Count == 0)
                return 0;

            var versioni = await versioneRepository.FindAll(v => idDocumenti.Contains(v.IdDoc_Documento)).ToListAsync(cancellationToken);
            await EliminaVersioniAsync(versioneRepository, fileStorage, quota, idCompany, versioni, cancellationToken);
            return await documentoRepository.FindAll(d => idDocumenti.Contains(d.Id)).ExecuteDeleteAsync(cancellationToken);
        }
    }
}
