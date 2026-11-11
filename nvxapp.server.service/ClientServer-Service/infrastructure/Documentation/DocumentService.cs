using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Documentation.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Documentation
{
    /*
     Documenti dei livelli SuperUser, Dealer, FinancialAdvisor e Company
     (specifica: nvxapp.server/docs/ANALISI_DOCUMENTMANAGEMENT_DOCUMENTAZIONE_AI.md [branch DocumentManagement]).
     Il livello e il nodo del documento li decide il server dal ruolo e dal token (LevelScope); il client
     sceglie solo tra i livelli che l'utente gestisce. Modificano SuperUser e PowerAdmin, gli Admin leggono.
     Sezione TicketAi: documentazione per le risposte automatiche AI ai ticket, con i tipi di richiesta
     a cui risponde (nessun tipo = tutti).
     Il contenuto e' testo: si scrive o si estrae dal file caricato (DocumentFileText), che resta come originale.
    */
    public class DocumentService : ServiceBase, IDocumentService
    {
        public const int ContenutoMaxLength = 100_000;
        public const long FileMaxSize = 10L * 1024 * 1024;
        public static readonly string[] FileExtensions = { ".pdf", ".docx", ".odt", ".ods", ".xlsx", ".xlsm", ".txt", ".md", ".csv", ".htm", ".html", ".json", ".xml" };

        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentTicketTypeRepository _documentTicketTypeRepository;
        private readonly ITicketTypeRepository _ticketTypeRepository;
        private readonly ITicketTypeLevelRepository _ticketTypeLevelRepository;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;

        public DocumentService(IMapper mapper,
                               UserManager<ApplicationUser> userManager,
                               IAspNetUsersRepository aspNetUsersRepository,
                               IOptions<JwtParameter> jwtParameter,
                               IHttpContextAccessor httpContextAccessor,
                               IConfiguration configuration,

                               IDocumentRepository documentRepository,
                               IDocumentTicketTypeRepository documentTicketTypeRepository,
                               ITicketTypeRepository ticketTypeRepository,
                               ITicketTypeLevelRepository ticketTypeLevelRepository,
                               ICompanyApplicationRepository companyApplicationRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _documentRepository = documentRepository;
            _documentTicketTypeRepository = documentTicketTypeRepository;
            _ticketTypeRepository = ticketTypeRepository;
            _ticketTypeLevelRepository = ticketTypeLevelRepository;
            _companyApplicationRepository = companyApplicationRepository;
        }

        public virtual async Task<GenericResult<DocumentScopeOutModel>> DocumentScope(GenericRequest<DocumentScopeInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new DocumentScopeOutModel();

                foreach (var scope in await Scopes())
                {
                    retVal.Scope.Add(new DocumentScopeModel
                    {
                        Level = scope.Level,
                        CanEdit = scope.CanEdit,
                        TicketType = model.Data.Section == DocumentSection.TicketAi ? TicketTypes(scope) : new List<TicketTypeOptionModel>(),
                    });
                }

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<DocumentListOutModel>> DocumentList(GenericRequest<DocumentListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new DocumentListOutModel();
                var scope = await Scope(model.Data.Level);
                var section = model.Data.Section;

                var documents = _documentRepository.FindAll(d => d.Section == section && d.Level == scope.Level
                                                                 && d.IdDealer == scope.IdDealer
                                                                 && d.IdFinancialAdvisor == scope.IdFinancialAdvisor
                                                                 && d.IdCompany == scope.IdCompany)
                                                   .OrderBy(d => d.Ordine).ThenBy(d => d.Titolo)
                                                   // contenuto e file restano nel database
                                                   .Select(d => new DocumentListModel
                                                   {
                                                       Id = d.Id,
                                                       Titolo = d.Titolo,
                                                       Descrizione = d.Descrizione,
                                                       FileName = d.FileName,
                                                       ApplicationType = d.ApplicationType,
                                                       IncludeSubLevels = d.IncludeSubLevels,
                                                       Ordine = d.Ordine,
                                                       Active = d.Active,
                                                       ContenutoLength = d.Contenuto.Length,
                                                       ModifiedDate = d.ModifiedDate,
                                                   })
                                                   .ToList();

                var ids = documents.Select(d => d.Id).ToList();
                var types = _documentTicketTypeRepository.FindAll(x => ids.Contains(x.IdDocument)).ToList();
                foreach (var d in documents)
                    d.IdTicketType = types.Where(t => t.IdDocument == d.Id).Select(t => t.IdTicketType).ToList();

                retVal.Document = documents;

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<DocumentGetOutModel>> DocumentGet(GenericRequest<DocumentGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new DocumentGetOutModel();

                var (document, scope) = await OwnedDocument(model.Data.Id, false);
                retVal.Document = ToEditModel(document, scope);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<DocumentPutOutModel>> DocumentPut(GenericRequest<DocumentPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new DocumentPutOutModel();
                var edit = model.Data.Document;

                // nuovo: livello scelto tra quelli dell'utente; esistente: quello del documento
                Document document;
                LevelScope scope;
                if (edit.Id == 0)
                {
                    scope = await Scope(edit.Level, true);
                    document = new Document
                    {
                        Section = edit.Section,
                        Level = scope.Level,
                        IdDealer = scope.IdDealer,
                        IdFinancialAdvisor = scope.IdFinancialAdvisor,
                        IdCompany = scope.IdCompany,
                    };
                }
                else
                    (document, scope) = await OwnedDocument(edit.Id, true);

                if (!Enum.IsDefined(document.Section))
                    throw new UserMessageException(InfrastructureMessages.Get("DocumentDataNotValid"));

                string titolo = (edit.Titolo ?? string.Empty).Trim();
                string? descrizione = string.IsNullOrWhiteSpace(edit.Descrizione) ? null : edit.Descrizione.Trim();
                string contenuto = (edit.Contenuto ?? string.Empty).Trim();
                if (titolo.Length == 0 || titolo.Length > 200 || (descrizione?.Length ?? 0) > 1000 || contenuto.Length == 0)
                    throw new UserMessageException(InfrastructureMessages.Get("DocumentDataNotValid"));
                if (contenuto.Length > ContenutoMaxLength)
                    throw new UserMessageException(InfrastructureMessages.Get("DocumentContentTooLong", ContenutoMaxLength));
                if (edit.ApplicationType != null && !Enum.IsDefined(edit.ApplicationType.Value))
                    throw new UserMessageException(InfrastructureMessages.Get("DocumentDataNotValid"));

                // tipi di richiesta: solo quelli proponibili al livello
                var idTypes = (edit.IdTicketType ?? new List<int>()).Distinct().ToList();
                if (document.Section != DocumentSection.TicketAi)
                    idTypes.Clear();
                var available = TicketTypes(scope).Select(t => t.Id).ToHashSet();
                var existingTypes = edit.Id == 0 ? new List<DocumentTicketType>() : _documentTicketTypeRepository.FindAll(x => x.IdDocument == document.Id).ToList();
                // un tipo gia' collegato e poi disattivato puo' restare
                if (idTypes.Any(t => !available.Contains(t) && !existingTypes.Any(e => e.IdTicketType == t)))
                    throw new UserMessageException(InfrastructureMessages.Get("DocumentTicketTypeNotValid"));

                document.Titolo = titolo;
                document.Descrizione = descrizione;
                document.Contenuto = contenuto;
                document.ApplicationType = edit.ApplicationType;
                document.IncludeSubLevels = edit.IncludeSubLevels;
                document.Ordine = edit.Ordine;
                document.Active = edit.Active;

                if (edit.File != null && edit.File.Length > 0)
                {
                    string fileName = Path.GetFileName(edit.FileName ?? string.Empty);
                    CheckFile(fileName, edit.File);
                    document.FileName = fileName;
                    document.ContentType = string.IsNullOrWhiteSpace(edit.ContentType) ? "application/octet-stream" : edit.ContentType;
                    document.Size = edit.File.LongLength;
                    document.File = edit.File;
                }
                else if (edit.RemoveFile)
                {
                    document.FileName = null;
                    document.ContentType = null;
                    document.Size = 0;
                    document.File = null;
                }

                if (document.Id == 0)
                    await _documentRepository.CreateAsync(document);
                else
                    await _documentRepository.UpdateAsync(document);

                // tipi: si sostituiscono
                var toRemove = existingTypes.Where(e => !idTypes.Contains(e.IdTicketType)).ToList();
                if (toRemove.Count > 0)
                    await _documentTicketTypeRepository.DeleteRangeAsync(toRemove);
                foreach (var idType in idTypes.Where(t => !existingTypes.Any(e => e.IdTicketType == t)))
                    await _documentTicketTypeRepository.CreateAsync(new DocumentTicketType { IdDocument = document.Id, IdTicketType = idType }, false);
                await _documentTicketTypeRepository.SaveChange();

                retVal.Document = ToEditModel(document, scope);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<DocumentDeleteOutModel>> DocumentDelete(GenericRequest<DocumentDeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new DocumentDeleteOutModel();

                // i legami con i tipi si eliminano in cascata; le risposte AI che lo citano tengono il titolo
                var (document, _) = await OwnedDocument(model.Data.Id, true);
                await _documentRepository.DeleteAsync(document);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<DocumentFileTextOutModel>> DocumentFileText(GenericRequest<DocumentFileTextInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new DocumentFileTextOutModel();

                // serve un livello modificabile: e' il primo passo del caricamento
                if (!(await Scopes()).Any(s => s.CanEdit))
                    throw new UserMessageException(InfrastructureMessages.Get("DocumentOperationNotAllowed"));

                string fileName = Path.GetFileName(model.Data.FileName ?? string.Empty);
                var file = model.Data.File ?? Array.Empty<byte>();
                CheckFile(fileName, file);

                string text = FileText.Estrai(fileName, file).Trim();
                if (text.Length == 0)
                    throw new UserMessageException(InfrastructureMessages.Get("DocumentFileNoText", fileName));
                if (text.Length > ContenutoMaxLength)
                    throw new UserMessageException(InfrastructureMessages.Get("DocumentContentTooLong", ContenutoMaxLength));

                retVal.Text = text;

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<DocumentFileGetOutModel>> DocumentFileGet(GenericRequest<DocumentFileGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new DocumentFileGetOutModel();

                var (document, _) = await OwnedDocument(model.Data.Id, false);
                if (document.File == null || document.File.Length == 0)
                    throw new UserMessageException(InfrastructureMessages.Get("FileNotFound"));

                retVal.FileName = document.FileName ?? "documento";
                retVal.ContentType = document.ContentType ?? "application/octet-stream";
                retVal.File = document.File;

                return retVal;
            }, isSubProcess);
        }


        // livelli dell'utente corrente (ruoli letti tutti, nodo dal token)
        private async Task<List<LevelScope>> Scopes()
        {
            var user = await _userManager.FindByIdAsync(this.CurrentUserId)
                       ?? throw new UserMessageException(InfrastructureMessages.Get("UserNotFound"));
            var roles = await _userManager.GetRolesAsync(user);
            return LevelScope.For(roles, this.CurrentDealer, this.CurrentFinancialAdvisor, this.CurrentCompany);
        }

        private async Task<LevelScope> Scope(TicketLevel level, bool edit = false)
        {
            var scope = (await Scopes()).FirstOrDefault(s => s.Level == level);
            if (scope == null || (edit && !scope.CanEdit))
                throw new UserMessageException(InfrastructureMessages.Get("DocumentOperationNotAllowed"));
            return scope;
        }

        // documento del nodo di uno dei livelli dell'utente (altrimenti "non trovato", come i ticket non visibili)
        private async Task<(Document document, LevelScope scope)> OwnedDocument(int id, bool edit)
        {
            var document = await _documentRepository.FindByIdAsync(id)
                           ?? throw new UserMessageException(InfrastructureMessages.Get("DocumentNotFound"));
            var scope = (await Scopes()).FirstOrDefault(s => s.Owns(document.Level, document.IdDealer, document.IdFinancialAdvisor, document.IdCompany))
                        ?? throw new UserMessageException(InfrastructureMessages.Get("DocumentNotFound"));
            if (edit && !scope.CanEdit)
                throw new UserMessageException(InfrastructureMessages.Get("DocumentOperationNotAllowed"));
            return (document, scope);
        }

        /*
         Tipi di richiesta attivi proponibili per i documenti del livello: il SuperUser (documenti per il DEV
         e, con "vale anche per i livelli sotto", per tutti) vede tutti i tipi; gli altri livelli quelli
         abilitati al livello; l'azienda solo quelli generali o dei suoi applicativi attivi.
        */
        private List<TicketTypeOptionModel> TicketTypes(LevelScope scope)
        {
            var types = _ticketTypeRepository.FindAll(t => t.Active).OrderBy(t => t.Ordine).ThenBy(t => t.Codice).ToList();
            if (scope.Level != TicketLevel.SuperUser)
            {
                var level = scope.Level;
                var enabled = _ticketTypeLevelRepository.FindAll(l => l.Level == level).Select(l => l.IdTicketType).ToHashSet();
                types = types.Where(t => enabled.Contains(t.Id)).ToList();
            }
            if (scope.IdCompany != null)
            {
                var applications = _companyApplicationRepository.ActiveApplications(scope.IdCompany.Value);
                types = types.Where(t => t.ApplicationType == null || applications.Contains(t.ApplicationType.Value)).ToList();
            }
            return types.Select(t => new TicketTypeOptionModel { Id = t.Id, ApplicationType = t.ApplicationType, Descrizione = t.Descrizione }).ToList();
        }

        private DocumentEditModel ToEditModel(Document document, LevelScope scope)
        {
            var retVal = _mapper.Map<DocumentEditModel>(document);
            retVal.IdTicketType = _documentTicketTypeRepository.FindAll(x => x.IdDocument == document.Id).Select(x => x.IdTicketType).ToList();
            retVal.CanEdit = scope.CanEdit;
            return retVal;
        }

        private static void CheckFile(string fileName, byte[] file)
        {
            if (file.Length == 0)
                throw new UserMessageException(InfrastructureMessages.Get("TicketAttachEmpty", fileName));
            if (file.LongLength > FileMaxSize)
                throw new UserMessageException(InfrastructureMessages.Get("DocumentFileTooLarge", fileName, FileMaxSize / (1024 * 1024)));
            if (!FileExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant()))
                throw new UserMessageException(InfrastructureMessages.Get("DocumentFileExtensionNotAllowed", fileName));
        }
    }

    public interface IDocumentService : IServiceBase
    {
        Task<GenericResult<DocumentScopeOutModel>> DocumentScope(GenericRequest<DocumentScopeInModel> model, Boolean isSubProcess);
        Task<GenericResult<DocumentListOutModel>> DocumentList(GenericRequest<DocumentListInModel> model, Boolean isSubProcess);
        Task<GenericResult<DocumentGetOutModel>> DocumentGet(GenericRequest<DocumentGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<DocumentPutOutModel>> DocumentPut(GenericRequest<DocumentPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<DocumentDeleteOutModel>> DocumentDelete(GenericRequest<DocumentDeleteInModel> model, Boolean isSubProcess);
        Task<GenericResult<DocumentFileTextOutModel>> DocumentFileText(GenericRequest<DocumentFileTextInModel> model, Boolean isSubProcess);
        Task<GenericResult<DocumentFileGetOutModel>> DocumentFileGet(GenericRequest<DocumentFileGetInModel> model, Boolean isSubProcess);
    }
}
