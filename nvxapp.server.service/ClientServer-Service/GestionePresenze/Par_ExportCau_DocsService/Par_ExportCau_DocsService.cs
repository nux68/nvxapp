using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.DipendentiImportService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_DocsService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_DocsService
{
    /*
     Documentazione del modello di export (tracciato del programma paghe, manuale, file di esempio): si carica
     dalla pagina del modello e va all'AI per ottenere la ricetta del formato. Il file e' salvato nel database;
     il testo si estrae al caricamento (ImportFileText, come l'importazione dei dipendenti): un file senza testo
     (es. PDF scansionato) si rifiuta, perche' l'AI non ne leggerebbe nulla.
    */
    public class Par_ExportCau_DocsService : ServiceBase, IPar_ExportCau_DocsService
    {
        public const int MaxByteFile = 5 * 1024 * 1024;
        public const int MaxDocumentiPerModello = 10;
        public static readonly string[] EstensioniAmmesse = { ".pdf", ".docx", ".odt", ".xlsx", ".txt", ".csv", ".xml", ".json", ".md" };

        private readonly IPar_ExportCau_DocsRepository _par_ExportCau_DocsRepository;
        private readonly IPar_ExportCauRepository _par_ExportCauRepository;

        public Par_ExportCau_DocsService(IMapper mapper,
                                         UserManager<ApplicationUser> userManager,
                                         IAspNetUsersRepository aspNetUsersRepository,
                                         IOptions<JwtParameter> jwtParameter,
                                         IHttpContextAccessor httpContextAccessor,
                                         IConfiguration configuration,
                                         IPar_ExportCau_DocsRepository par_ExportCau_DocsRepository,
                                         IPar_ExportCauRepository par_ExportCauRepository)
            : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_ExportCau_DocsRepository = par_ExportCau_DocsRepository;
            _par_ExportCauRepository = par_ExportCauRepository;
        }

        public virtual async Task<GenericResult<Par_ExportCau_Docs_GetAll_OutModel>> Par_ExportCau_Docs_GetAll(GenericRequest<Par_ExportCau_Docs_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_ExportCau_Docs_GetAll_OutModel();
                // senza contenuto: per l'elenco basta la descrizione del file
                retVal.Par_ExportCau_Docs = _par_ExportCau_DocsRepository.FindAll(x => x.IdPar_ExportCau == model.Data.IdPar_ExportCau)
                    .OrderBy(x => x.Ordine).ThenBy(x => x.Id)
                    .Select(x => new Par_ExportCau_DocsModel
                    {
                        Id = x.Id,
                        IdPar_ExportCau = x.IdPar_ExportCau,
                        NomeFile = x.NomeFile,
                        ContentType = x.ContentType,
                        Dimensione = x.Dimensione,
                        Descrizione = x.Descrizione,
                        Ordine = x.Ordine,
                        LunghezzaTesto = x.Testo != null ? x.Testo.Length : 0,
                        CreationDate = x.CreationDate
                    })
                    .ToList();
                await Task.CompletedTask;
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ExportCau_Docs_Get_OutModel>> Par_ExportCau_Docs_Get(GenericRequest<Par_ExportCau_Docs_Get_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_ExportCau_Docs_Get_OutModel();
                var doc = await _par_ExportCau_DocsRepository.FindByIdAsync(model.Data.Id)
                          ?? throw new UserMessageException(AttendanceTrackingMessages.Get("ExportDocNonTrovato"));
                retVal.Par_ExportCau_Doc = ToModel(doc);
                retVal.Contenuto = Convert.ToBase64String(doc.Contenuto);
                retVal.Testo = doc.Testo;
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ExportCau_Docs_Put_OutModel>> Par_ExportCau_Docs_Put(GenericRequest<Par_ExportCau_Docs_Put_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Par_ExportCau_Docs_Put_OutModel();
                var d = model.Data;

                if (d.IdPar_ExportCau <= 0 || await _par_ExportCauRepository.FindByIdAsync(d.IdPar_ExportCau) == null)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ExportDocModelloNonSalvato"));

                var nomeFile = Path.GetFileName(d.NomeFile ?? string.Empty).Trim();
                var ext = Path.GetExtension(nomeFile).ToLowerInvariant();
                if (nomeFile.Length == 0 || !EstensioniAmmesse.Contains(ext))
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ExportDocTipoNonAmmesso", string.Join(", ", EstensioniAmmesse)));
                if (nomeFile.Length > 255)
                    nomeFile = nomeFile.Substring(nomeFile.Length - 255);

                byte[] dati;
                try
                {
                    dati = Convert.FromBase64String(d.Contenuto ?? string.Empty);
                }
                catch (FormatException)
                {
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ImportFileNonLeggibile"));
                }
                if (dati.Length == 0)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ImportFileVuoto"));
                if (dati.Length > MaxByteFile)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ImportFileTroppoGrande", MaxByteFile / (1024 * 1024)));

                var esistenti = _par_ExportCau_DocsRepository.FindAll(x => x.IdPar_ExportCau == d.IdPar_ExportCau).Select(x => x.Ordine).ToList();
                if (esistenti.Count >= MaxDocumentiPerModello)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ExportDocTroppi", MaxDocumentiPerModello));

                // il testo e' quello che va all'AI: senza testo il documento non servirebbe
                var testo = ImportFileText.Estrai(nomeFile, dati).Trim();
                if (testo.Length == 0)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ExportDocSenzaTesto", nomeFile));

                var doc = new Par_ExportCau_Docs
                {
                    IdPar_ExportCau = d.IdPar_ExportCau,
                    NomeFile = nomeFile,
                    ContentType = string.IsNullOrWhiteSpace(d.ContentType) ? "application/octet-stream" : d.ContentType.Trim(),
                    Dimensione = dati.Length,
                    Contenuto = dati,
                    Testo = testo,
                    Descrizione = string.IsNullOrWhiteSpace(d.Descrizione) ? null : d.Descrizione.Trim()[..Math.Min(d.Descrizione.Trim().Length, 200)],
                    Ordine = esistenti.Count == 0 ? 1 : esistenti.Max() + 1
                };
                doc = await _par_ExportCau_DocsRepository.UpsertAsync(doc);
                retVal.Par_ExportCau_Doc = ToModel(doc);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Par_ExportCau_Docs_Delete_OutModel>> Par_ExportCau_Docs_Delete(GenericRequest<Par_ExportCau_Docs_Delete_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var doc = await _par_ExportCau_DocsRepository.FindByIdAsync(model.Data.Id);
                if (doc != null)
                    await _par_ExportCau_DocsRepository.DeleteAsync(doc);
                return new Par_ExportCau_Docs_Delete_OutModel();
            }, isSubProcess);
        }

        private static Par_ExportCau_DocsModel ToModel(Par_ExportCau_Docs x) => new()
        {
            Id = x.Id,
            IdPar_ExportCau = x.IdPar_ExportCau,
            NomeFile = x.NomeFile,
            ContentType = x.ContentType,
            Dimensione = x.Dimensione,
            Descrizione = x.Descrizione,
            Ordine = x.Ordine,
            LunghezzaTesto = x.Testo?.Length ?? 0,
            CreationDate = x.CreationDate
        };
    }

    public interface IPar_ExportCau_DocsService : IServiceBase
    {
        Task<GenericResult<Par_ExportCau_Docs_GetAll_OutModel>> Par_ExportCau_Docs_GetAll(GenericRequest<Par_ExportCau_Docs_GetAll_InModel> model, bool isSubProcess);
        Task<GenericResult<Par_ExportCau_Docs_Get_OutModel>> Par_ExportCau_Docs_Get(GenericRequest<Par_ExportCau_Docs_Get_InModel> model, bool isSubProcess);
        Task<GenericResult<Par_ExportCau_Docs_Put_OutModel>> Par_ExportCau_Docs_Put(GenericRequest<Par_ExportCau_Docs_Put_InModel> model, bool isSubProcess);
        Task<GenericResult<Par_ExportCau_Docs_Delete_OutModel>> Par_ExportCau_Docs_Delete(GenericRequest<Par_ExportCau_Docs_Delete_InModel> model, bool isSubProcess);
    }
}
