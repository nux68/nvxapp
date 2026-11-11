using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.DocumentManagement;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Tenant.DocumentManagement;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.DocumentManagement._utility;
using nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_DocumentoService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Acknowledgement;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Email;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Security.Cryptography;

namespace nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_DocumentoService
{
    /*
     Documenti dell'azienda (ANALISI_DOCUMENTMANAGEMENT_APPLICATIVO.md §6-§7): elenco con ricerca nel testo, dettaglio, caricamento con
     versioni, metadati, cestino ed eliminazione definitiva, download con registro, presa visione.
        - visibilita': IDocumentManagementUtility.Visibili (documenti del personale solo con abilitazione o al dipendente)
        - modifica: documenti del personale con l'abilitazione "carica", gli altri CompanyPowerAdmin e CompanyAdmin
        - file: IFileStorage (su disco), spazio controllato a ogni caricamento (IStorageQuotaService: oltre quota si blocca)
        - testo estratto dal file per la ricerca; nessun contenuto viene inviato all'AI (decisione del 04/11/2026)
        - firma elettronica semplice (ANALISI_DOCUMENTMANAGEMENT_FIRMA_DIGITALE.md §4.2): la tipologia propone la richiesta, l'invio la decide
          (si puo' togliere o aggiungere, anche dopo); la firma vale per la versione corrente e una nuova versione la azzera
        - prese visione degli altri moduli (DM-FIRMA, DocumentManagementAcknowledgementChannel): la firma, o la presa visione
          se la firma non e' richiesta, conferma la richiesta del modulo (IAcknowledgementService.ConfirmFromChannelAsync)
    */
    public class Doc_DocumentoService : ServiceBase, IDoc_DocumentoService
    {
        private const int MaxRighe = 500;
        private const int MaxTesto = 200_000;   // il vettore di ricerca di PostgreSQL ha un limite di dimensione

        private static readonly HashSet<string> _estensioni = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".p7m", ".png", ".jpg", ".jpeg", ".gif", ".webp", ".tif", ".tiff", ".bmp",
            ".doc", ".docx", ".odt", ".rtf", ".txt", ".xls", ".xlsx", ".ods", ".csv", ".ppt", ".pptx", ".odp",
            ".xml", ".zip", ".eml", ".msg"
        };

        private readonly IDoc_DocumentoRepository _documentoRepository;
        private readonly IDoc_VersioneRepository _versioneRepository;
        private readonly IDoc_DestinatarioRepository _destinatarioRepository;
        private readonly IDoc_LogRepository _logRepository;
        private readonly IDoc_Par_TipoRepository _tipoRepository;
        private readonly IDoc_CartellaRepository _cartellaRepository;
        private readonly IDoc_CfgRepository _cfgRepository;
        private readonly IDoc_Dip_AnagraficaReadRepository _dipendenteReadRepository;
        private readonly IDoc_Az_ClienteReadRepository _clienteReadRepository;
        private readonly IAspNetUsersRepository _usersRepository;
        private readonly IDocumentManagementUtility _utility;
        private readonly IFileStorage _fileStorage;
        private readonly IStorageQuotaService _quota;
        private readonly IEmailService _emailService;
        private readonly IServiceProvider _serviceProvider;

        public Doc_DocumentoService(IMapper mapper,
                                    UserManager<ApplicationUser> userManager,
                                    IAspNetUsersRepository aspNetUsersRepository,
                                    IOptions<JwtParameter> jwtParameter,
                                    IHttpContextAccessor httpContextAccessor,
                                    IConfiguration configuration,
                                    IDoc_DocumentoRepository documentoRepository,
                                    IDoc_VersioneRepository versioneRepository,
                                    IDoc_DestinatarioRepository destinatarioRepository,
                                    IDoc_LogRepository logRepository,
                                    IDoc_Par_TipoRepository tipoRepository,
                                    IDoc_CartellaRepository cartellaRepository,
                                    IDoc_CfgRepository cfgRepository,
                                    IDoc_Dip_AnagraficaReadRepository dipendenteReadRepository,
                                    IDoc_Az_ClienteReadRepository clienteReadRepository,
                                    IDocumentManagementUtility utility,
                                    IFileStorage fileStorage,
                                    IStorageQuotaService quota,
                                    IEmailService emailService,
                                    IServiceProvider serviceProvider) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _documentoRepository = documentoRepository;
            _versioneRepository = versioneRepository;
            _destinatarioRepository = destinatarioRepository;
            _logRepository = logRepository;
            _tipoRepository = tipoRepository;
            _cartellaRepository = cartellaRepository;
            _cfgRepository = cfgRepository;
            _dipendenteReadRepository = dipendenteReadRepository;
            _clienteReadRepository = clienteReadRepository;
            _usersRepository = aspNetUsersRepository;
            _utility = utility;
            _fileStorage = fileStorage;
            _quota = quota;
            _emailService = emailService;
            _serviceProvider = serviceProvider;
        }

        // ---------------------------------------------------------------------------
        // Elenco e dettaglio

        public virtual async Task<GenericResult<Doc_DocumentoListOutModel>> Doc_DocumentoList(GenericRequest<Doc_DocumentoListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var f = model.Data;
                var q = _utility.Visibili(ctx);

                if (f.Cestino)
                {
                    // il cestino lo vede solo chi puo' modificare
                    if (!ctx.AmministraArchivio && !ctx.CaricaPersonale)
                        throw new UserMessageException(DocumentManagementMessages.Get("NotAllowed"));
                    q = q.Where(x => x.NelCestino);
                }
                else
                {
                    q = q.Where(x => !x.NelCestino);
                }

                if (f.Miei)
                {
                    var idDip = ctx.IdDip_Anagrafica;
                    var idUtente = ctx.IdUtente;
                    q = q.Where(x => (idDip != null && x.IdDip_Anagrafica == idDip) || x.Doc_Destinatario!.Any(r => r.IdAspNetUsers == idUtente));
                }
                if (f.IdDoc_Cartella.HasValue) q = q.Where(x => x.IdDoc_Cartella == f.IdDoc_Cartella);
                if (f.IdDoc_Par_Tipo.HasValue) q = q.Where(x => x.IdDoc_Par_Tipo == f.IdDoc_Par_Tipo);
                if (f.IdDip_Anagrafica.HasValue) q = q.Where(x => x.IdDip_Anagrafica == f.IdDip_Anagrafica);
                if (f.IdAz_Cliente.HasValue) q = q.Where(x => x.IdAz_Cliente == f.IdAz_Cliente);
                if (f.Categoria.HasValue) q = q.Where(x => x.Doc_Par_TipoNavigation!.Categoria == f.Categoria);
                if (f.Anno.HasValue) q = q.Where(x => x.Anno == f.Anno || (x.Anno == null && x.DataDocumento != null && x.DataDocumento.Value.Year == f.Anno));
                if (f.ScadenzaEntroGiorni.HasValue)
                {
                    var limite = DateTime.Today.AddDays(f.ScadenzaEntroGiorni.Value);
                    q = q.Where(x => x.DataScadenza != null && x.DataScadenza <= limite);
                }
                if (!string.IsNullOrWhiteSpace(f.Testo))
                {
                    var testo = f.Testo.Trim();
                    var like = "%" + testo.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
                    q = q.Where(x => x.Ricerca!.Matches(EF.Functions.WebSearchToTsQuery("italian", testo)) || EF.Functions.ILike(x.Titolo, like));
                }

                var righe = await Proietta(q.OrderByDescending(x => x.DataDocumento ?? x.CreationDate).ThenByDescending(x => x.Id), ctx)
                                  .Take(MaxRighe + 1).ToListAsync();
                var retVal = new Doc_DocumentoListOutModel { Troncato = righe.Count > MaxRighe, Documenti = righe.Take(MaxRighe).ToList() };
                await CompletaConservazione(retVal.Documenti);
                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Doc_DocumentoGetOutModel>> Doc_DocumentoGet(GenericRequest<Doc_DocumentoGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var retVal = new Doc_DocumentoGetOutModel { Dettaglio = await DettaglioAsync(ctx, model.Data.Id) };

                await _utility.LogAsync(ctx, Doc_LogAzione.Visualizzazione, model.Data.Id);
                await SegnaDestinatarioAsync(ctx, model.Data.Id, apertura: true, download: false);
                return retVal;
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------
        // Caricamento, nuova versione, metadati

        public virtual async Task<GenericResult<Doc_DocumentoPutOutModel>> Doc_DocumentoUpload(GenericRequest<Doc_DocumentoEditModel> model, Stream file, string nomeFile, string? contentType, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var dati = model.Data;
                var tipo = await ValidaAsync(ctx, dati, nuovo: true);

                var caricato = await CaricaFileAsync(ctx, file, nomeFile, contentType);
                var retVal = new Doc_DocumentoPutOutModel();
                var doc = await CreaDocumentoAsync(ctx, dati, tipo, caricato);
                if (await _versioneRepository.FindAll(v => v.Hash == caricato.Hash && v.IdDoc_Documento != doc.Id && v.Doc_DocumentoNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AnyAsync())
                    retVal.Messages.Add(new Message(DocumentManagementMessages.Get("DuplicateFile"), MessageType.Warning));
                retVal.Dettaglio = await DettaglioAsync(ctx, doc.Id);
                return retVal;
            }, isSubProcess);
        }

        /*
         Invio multiplo: lo stesso file come documento del personale a piu' dipendenti. Si crea UN documento per ogni
         dipendente scelto (con la sua copia del file: cancellarne uno non tocca gli altri), con le stesse regole del
         caricamento singolo (permesso CaricaPersonale, tipologia del personale, destinatario e presa visione, registro,
         avviso email). Lo spazio si verifica prima per il totale; un dipendente non valido non blocca gli altri.
        */
        public virtual async Task<GenericResult<Doc_DocumentoUploadMultiploOutModel>> Doc_DocumentoUploadMultiplo(GenericRequest<Doc_DocumentoUploadMultiploInModel> model, Stream file, string nomeFile, string? contentType, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                if (!ctx.CaricaPersonale)
                    throw new UserMessageException(DocumentManagementMessages.Get("PersonnelNotAllowed"));
                var ids = (model.Data.IdDip_Anagrafica ?? new List<int>()).Distinct().ToList();
                if (ids.Count == 0)
                    throw new UserMessageException(DocumentManagementMessages.Get("MultiNoEmployees"));
                var dati = model.Data.Documento ?? new Doc_DocumentoEditModel();

                // tipologia e dati controllati una volta (con il primo dipendente): solo documenti del personale
                dati.IdDip_Anagrafica = ids[0];
                var tipo = await ValidaAsync(ctx, dati, nuovo: true);
                if (tipo.Categoria != Doc_Categoria.Personale)
                    throw new UserMessageException(DocumentManagementMessages.Get("MultiPersonnelOnly"));

                var letto = await LeggiFileAsync(file, nomeFile);
                // spazio per tutte le copie: se non basta non si carica nulla
                long totale = (long)letto.Bytes.Length * ids.Count;
                await _quota.ReserveAsync(ctx.IdCompany, totale);
                await _quota.ReleaseAsync(ctx.IdCompany, totale);

                var dipendenti = await _dipendenteReadRepository.FindAll(x => ids.Contains(x.Id) && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                                               .Select(x => new { x.Id, x.Cognome, x.Nome }).ToListAsync();
                var retVal = new Doc_DocumentoUploadMultiploOutModel();
                foreach (var id in ids)
                {
                    var dip = dipendenti.FirstOrDefault(d => d.Id == id);
                    var nome = dip == null ? id.ToString() : $"{dip.Cognome} {dip.Nome}".Trim();
                    if (dip == null)
                    {
                        retVal.Errori.Add(DocumentManagementMessages.Get("MultiEmployeeError", nome, DocumentManagementMessages.Get("EmployeeRequired")));
                        continue;
                    }
                    try
                    {
                        var copia = Copia(dati);
                        copia.IdDip_Anagrafica = id;
                        var caricato = await SalvaFileAsync(ctx, letto, contentType);
                        await CreaDocumentoAsync(ctx, copia, tipo, caricato);
                        retVal.Creati++;
                    }
                    catch (UserMessageException ex)
                    {
                        retVal.Errori.Add(DocumentManagementMessages.Get("MultiEmployeeError", nome, ex.Message));
                    }
                }
                retVal.Messages.Add(new Message(DocumentManagementMessages.Get("MultiDone", retVal.Creati, ids.Count),
                                                retVal.Errori.Count == 0 ? MessageType.Information : MessageType.Warning));
                return retVal;
            }, isSubProcess);
        }

        private static Doc_DocumentoEditModel Copia(Doc_DocumentoEditModel d) => new()
        {
            IdDoc_Par_Tipo = d.IdDoc_Par_Tipo, IdDoc_Cartella = d.IdDoc_Cartella, IdDip_Anagrafica = d.IdDip_Anagrafica, IdAz_Cliente = d.IdAz_Cliente,
            Titolo = d.Titolo, Descrizione = d.Descrizione, DataDocumento = d.DataDocumento, Anno = d.Anno, Mese = d.Mese,
            DataScadenza = d.DataScadenza, Stato = d.Stato, FirmaRichiesta = d.FirmaRichiesta, NoteVersione = d.NoteVersione
        };

        // documento nuovo con la prima versione, destinatario, spazio confermato, registro e avviso (caricamento singolo e multiplo)
        private async Task<Doc_Documento> CreaDocumentoAsync(DocContesto ctx, Doc_DocumentoEditModel dati, Doc_Par_Tipo tipo, FileCaricato caricato)
        {
            try
            {
                var doc = new Doc_Documento
                {
                    IdAz_Anagrafica = ctx.IdAz_Anagrafica,
                    Origine = Doc_Origine.Azienda,
                    VersioneCorrente = 1,
                    Testo = caricato.Testo,
                    CreationDate = DateTime.Now,
                    ChangeUser = ctx.IdUtente
                };
                Applica(doc, dati, tipo);
                doc.Doc_Versione = new List<Doc_Versione> { caricato.Versione(1, dati.NoteVersione, ctx.IdUtente) };
                // documento con la prima versione (stesso salvataggio)
                doc = await _documentoRepository.CreateAsync(doc);

                await AggiornaDestinatarioDipendenteAsync(doc, tipo);
                await _quota.ConfirmAsync(ctx.IdCompany, caricato.Dimensione);
                caricato.Confermato = true;

                await _utility.LogAsync(ctx, Doc_LogAzione.Caricamento, doc.Id, 1, caricato.NomeFile);
                await NotificaAsync(ctx, doc, tipo);
                return doc;
            }
            finally
            {
                await caricato.AnnullaSeNonConfermatoAsync(_fileStorage, _quota, ctx.IdCompany);
            }
        }

        public virtual async Task<GenericResult<Doc_DocumentoPutOutModel>> Doc_DocumentoNewVersion(GenericRequest<Doc_DocumentoIdInModel> model, Stream file, string nomeFile, string? contentType, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var doc = await DocumentoModificabileAsync(ctx, model.Data.Id);

                var caricato = await CaricaFileAsync(ctx, file, nomeFile, contentType);
                try
                {
                    int numero = (await _versioneRepository.FindAll(v => v.IdDoc_Documento == doc.Id).MaxAsync(v => (int?)v.Numero) ?? 0) + 1;
                    // versione e documento nello stesso salvataggio (i repository condividono il contesto della richiesta)
                    await _versioneRepository.CreateAsync(caricato.Versione(numero, null, ctx.IdUtente, doc.Id), false);
                    doc.VersioneCorrente = numero;
                    doc.Testo = caricato.Testo;
                    doc.ModifiedDate = DateTime.Now;
                    doc.ChangeUser = ctx.IdUtente;
                    await _documentoRepository.UpdateAsync(doc);

                    await _quota.ConfirmAsync(ctx.IdCompany, caricato.Dimensione);
                    caricato.Confermato = true;

                    // la nuova versione va riletta (e rifirmata): si azzerano prese visione e firme.
                    // La prova delle firme precedenti resta nel registro (azione Firma, con versione e hash)
                    await _destinatarioRepository.FindAll(r => r.IdDoc_Documento == doc.Id)
                             .ExecuteUpdateAsync(s => s.SetProperty(r => r.DataPresaVisione, (DateTime?)null).SetProperty(r => r.DataPrimaApertura, (DateTime?)null)
                                                       .SetProperty(r => r.DataFirma, (DateTime?)null).SetProperty(r => r.FirmaVersione, (int?)null)
                                                       .SetProperty(r => r.FirmaHash, (string?)null).SetProperty(r => r.FirmaIp, (string?)null)
                                                       .SetProperty(r => r.FirmaUserAgent, (string?)null).SetProperty(r => r.IdAspNetUsersFirma, (string?)null));

                    await PotaVersioniAsync(ctx, doc.Id);
                    await _utility.LogAsync(ctx, Doc_LogAzione.NuovaVersione, doc.Id, numero, caricato.NomeFile);
                }
                finally
                {
                    await caricato.AnnullaSeNonConfermatoAsync(_fileStorage, _quota, ctx.IdCompany);
                }
                return new Doc_DocumentoPutOutModel { Dettaglio = await DettaglioAsync(ctx, doc.Id) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Doc_DocumentoPutOutModel>> Doc_DocumentoPut(GenericRequest<Doc_DocumentoPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var dati = model.Data.Documento;
                var doc = await DocumentoModificabileAsync(ctx, dati.Id);
                var tipo = await ValidaAsync(ctx, dati, nuovo: false);

                var firmaPrima = doc.FirmaRichiesta;
                Applica(doc, dati, tipo);
                doc.ModifiedDate = DateTime.Now;
                doc.ChangeUser = ctx.IdUtente;
                await _documentoRepository.UpdateAsync(doc);

                await AggiornaDestinatarioDipendenteAsync(doc, tipo);
                await _utility.LogAsync(ctx, Doc_LogAzione.Modifica, doc.Id);
                if (doc.FirmaRichiesta != firmaPrima)
                    await _utility.LogAsync(ctx, Doc_LogAzione.RichiestaFirma, doc.Id, doc.VersioneCorrente,
                                            DocumentManagementMessages.Get(doc.FirmaRichiesta ? "SignRequestAdded" : "SignRequestRemoved"));
                return new Doc_DocumentoPutOutModel { Dettaglio = await DettaglioAsync(ctx, doc.Id) };
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------
        // Cestino

        public virtual async Task<GenericResult<Doc_DocumentoIdOutModel>> Doc_DocumentoCestino(GenericRequest<Doc_DocumentoIdInModel> model, Boolean isSubProcess)
            => await CambiaCestino(model, isSubProcess, true);

        public virtual async Task<GenericResult<Doc_DocumentoIdOutModel>> Doc_DocumentoRipristina(GenericRequest<Doc_DocumentoIdInModel> model, Boolean isSubProcess)
            => await CambiaCestino(model, isSubProcess, false);

        private async Task<GenericResult<Doc_DocumentoIdOutModel>> CambiaCestino(GenericRequest<Doc_DocumentoIdInModel> model, Boolean isSubProcess, bool cestino)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var doc = await DocumentoModificabileAsync(ctx, model.Data.Id, nelCestino: !cestino);
                doc.NelCestino = cestino;
                doc.DataCestino = cestino ? DateTime.Now : null;
                doc.ModifiedDate = DateTime.Now;
                doc.ChangeUser = ctx.IdUtente;
                await _documentoRepository.UpdateAsync(doc);
                await _utility.LogAsync(ctx, cestino ? Doc_LogAzione.Cestino : Doc_LogAzione.Ripristino, doc.Id);
                return new Doc_DocumentoIdOutModel { Id = doc.Id };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Doc_DocumentoIdOutModel>> Doc_DocumentoElimina(GenericRequest<Doc_DocumentoIdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var doc = await DocumentoModificabileAsync(ctx, model.Data.Id, nelCestino: true);
                await _utility.LogAsync(ctx, Doc_LogAzione.Eliminazione, doc.Id, null, doc.Titolo);
                await DocumentFiles.EliminaDocumentiAsync(_documentoRepository, _versioneRepository, _fileStorage, _quota, ctx.IdCompany, new List<int> { doc.Id });
                return new Doc_DocumentoIdOutModel { Id = model.Data.Id };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Doc_SvuotaCestinoOutModel>> Doc_SvuotaCestino(GenericRequest<Doc_SvuotaCestinoInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                if (!ctx.AmministraArchivio && !ctx.CaricaPersonale)
                    throw new UserMessageException(DocumentManagementMessages.Get("NotAllowed"));

                // solo i documenti che l'utente puo' modificare
                var nelCestino = await _utility.Visibili(ctx).Where(x => x.NelCestino)
                                               .Select(x => new { x.Id, Personale = x.Doc_Par_TipoNavigation!.Categoria == Doc_Categoria.Personale || x.IdDip_Anagrafica != null })
                                               .ToListAsync();
                var ids = nelCestino.Where(x => x.Personale ? ctx.CaricaPersonale : ctx.AmministraArchivio).Select(x => x.Id).ToList();
                foreach (var id in ids)
                    await _utility.LogAsync(ctx, Doc_LogAzione.Eliminazione, id, null, "cestino svuotato");
                var eliminati = await DocumentFiles.EliminaDocumentiAsync(_documentoRepository, _versioneRepository, _fileStorage, _quota, ctx.IdCompany, ids);
                return new Doc_SvuotaCestinoOutModel { Eliminati = eliminati };
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------
        // Download, presa visione, registro

        public async Task<Doc_FileDownload> Doc_DocumentoDownload(int idDocumento, int? numeroVersione)
        {
            var ctx = await _utility.ContestoAsync();
            var doc = await _utility.Visibili(ctx).FirstOrDefaultAsync(x => x.Id == idDocumento)
                      ?? throw new UserMessageException(DocumentManagementMessages.Get("DocumentNotFound"));
            var numero = numeroVersione ?? doc.VersioneCorrente;
            var versione = await _versioneRepository.FindAll(v => v.IdDoc_Documento == doc.Id && v.Numero == numero).FirstOrDefaultAsync()
                           ?? throw new UserMessageException(DocumentManagementMessages.Get("VersionNotFound"));

            // versioni precedenti: solo chi puo' modificare il documento
            if (numero != doc.VersioneCorrente && !PuoModificare(ctx, doc, await _tipoRepository.FindAll(t => t.Id == doc.IdDoc_Par_Tipo).FirstAsync()))
                throw new UserMessageException(DocumentManagementMessages.Get("NotAllowed"));

            var stream = await _fileStorage.OpenAsync(versione.StorageKey);
            await _utility.LogAsync(ctx, Doc_LogAzione.Download, doc.Id, numero, versione.NomeFile);
            await SegnaDestinatarioAsync(ctx, doc.Id, apertura: true, download: true);
            return new Doc_FileDownload { Content = stream, NomeFile = versione.NomeFile, ContentType = versione.ContentType };
        }

        public virtual async Task<GenericResult<Doc_DocumentoIdOutModel>> Doc_DocumentoPresaVisione(GenericRequest<Doc_DocumentoIdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var doc = await _utility.Visibili(ctx).FirstOrDefaultAsync(x => x.Id == model.Data.Id)
                          ?? throw new UserMessageException(DocumentManagementMessages.Get("DocumentNotFound"));
                var idDip = ctx.IdDip_Anagrafica;
                var aggiornati = await _destinatarioRepository
                    .FindAll(r => r.IdDoc_Documento == doc.Id && (r.IdAspNetUsers == ctx.IdUtente || (idDip != null && r.IdDip_Anagrafica == idDip)) && r.DataPresaVisione == null)
                    .ExecuteUpdateAsync(s => s.SetProperty(r => r.DataPresaVisione, DateTime.Now));
                if (aggiornati == 0)
                    throw new UserMessageException(DocumentManagementMessages.Get("NotRecipient"));
                await _utility.LogAsync(ctx, Doc_LogAzione.PresaVisione, doc.Id, doc.VersioneCorrente);
                var http = _httpContextAccessor.HttpContext;
                await ConfermaPreseVisioneAsync(ctx, doc.Id, DateTime.Now, http?.Connection?.RemoteIpAddress?.ToString(), http?.Request.Headers.UserAgent.ToString());
                return new Doc_DocumentoIdOutModel { Id = doc.Id };
            }, isSubProcess);
        }

        /*
         Firma elettronica semplice del destinatario sulla versione che ha davanti (FES, ANALISI_DOCUMENTMANAGEMENT_FIRMA_DIGITALE.md §4.2).
         Si registrano data, versione, hash SHA-256 del file, IP, user agent e utente; firmare vale anche come presa visione.
         Se nel frattempo e' arrivata una nuova versione la firma si rifiuta: va riaperto il documento.
        */
        public virtual async Task<GenericResult<Doc_DocumentoIdOutModel>> Doc_DocumentoFirma(GenericRequest<Doc_DocumentoFirmaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var doc = await _utility.Visibili(ctx).FirstOrDefaultAsync(x => x.Id == model.Data.Id && !x.NelCestino)
                          ?? throw new UserMessageException(DocumentManagementMessages.Get("DocumentNotFound"));
                if (model.Data.Versione != doc.VersioneCorrente)
                    throw new UserMessageException(DocumentManagementMessages.Get("SignVersionChanged"));
                var versione = await _versioneRepository.FindAll(v => v.IdDoc_Documento == doc.Id && v.Numero == doc.VersioneCorrente).FirstOrDefaultAsync()
                               ?? throw new UserMessageException(DocumentManagementMessages.Get("VersionNotFound"));

                // si firma solo cio' che si e' aperto: download della versione corrente da parte di chi firma
                var idUtenteLog = ctx.IdUtente;
                if (!await _logRepository.FindAll(l => l.IdDoc_Documento == doc.Id && l.Versione == doc.VersioneCorrente
                                                       && l.Azione == Doc_LogAzione.Download && l.IdAspNetUsers == idUtenteLog).AnyAsync())
                    throw new UserMessageException(DocumentManagementMessages.Get("SignDownloadFirst"));

                var http = _httpContextAccessor.HttpContext;
                var ip = http?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = http?.Request.Headers.UserAgent.ToString();
                if (userAgent != null && userAgent.Length > 300) userAgent = userAgent[..300];
                var now = DateTime.Now;
                var idDip = ctx.IdDip_Anagrafica;
                var idUtente = ctx.IdUtente;
                var numero = versione.Numero;
                var hash = versione.Hash;
                var aggiornati = await _destinatarioRepository
                    .FindAll(r => r.IdDoc_Documento == doc.Id && (r.IdAspNetUsers == idUtente || (idDip != null && r.IdDip_Anagrafica == idDip))
                                  && r.FirmaRichiesta && r.DataFirma == null)
                    .ExecuteUpdateAsync(s => s.SetProperty(r => r.DataFirma, now).SetProperty(r => r.FirmaVersione, numero)
                                              .SetProperty(r => r.FirmaHash, hash).SetProperty(r => r.FirmaIp, ip)
                                              .SetProperty(r => r.FirmaUserAgent, userAgent).SetProperty(r => r.IdAspNetUsersFirma, idUtente)
                                              .SetProperty(r => r.DataPresaVisione, r => r.DataPresaVisione ?? now));
                if (aggiornati == 0)
                    throw new UserMessageException(DocumentManagementMessages.Get("NotSignatory"));
                await _utility.LogAsync(ctx, Doc_LogAzione.Firma, doc.Id, numero, "FES sha256:" + hash);
                await ConfermaPreseVisioneAsync(ctx, doc.Id, now, ip, userAgent);
                return new Doc_DocumentoIdOutModel { Id = doc.Id };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Doc_LogListOutModel>> Doc_LogList(GenericRequest<Doc_LogListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var q = _logRepository.FindAll(x => true);
                if (model.Data.IdDoc_Documento.HasValue)
                {
                    var doc = await _utility.Visibili(ctx).FirstOrDefaultAsync(x => x.Id == model.Data.IdDoc_Documento)
                              ?? throw new UserMessageException(DocumentManagementMessages.Get("DocumentNotFound"));
                    if (!PuoModificare(ctx, doc, await _tipoRepository.FindAll(t => t.Id == doc.IdDoc_Par_Tipo).FirstAsync()))
                        throw new UserMessageException(DocumentManagementMessages.Get("NotAllowed"));
                    q = q.Where(x => x.IdDoc_Documento == doc.Id);
                }
                else
                {
                    // registro completo: CompanyPowerAdmin abilitato ai documenti del personale
                    if (!(ctx.PowerAdmin && ctx.VedePersonale))
                        throw new UserMessageException(DocumentManagementMessages.Get("NotAllowed"));
                    var idDocAzienda = _documentoRepository.FindAll(d => d.IdAz_Anagrafica == ctx.IdAz_Anagrafica).Select(d => (int?)d.Id);
                    q = q.Where(x => x.IdDoc_Documento == null || idDocAzienda.Contains(x.IdDoc_Documento));
                }

                return new Doc_LogListOutModel
                {
                    Log = await q.OrderByDescending(x => x.Data).Take(2000)
                                 .Select(x => new Doc_LogModel { Data = x.Data, UserName = x.UserName, Azione = x.Azione, Versione = x.Versione, Dettaglio = x.Dettaglio })
                                 .ToListAsync()
                };
            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------
        // Supporto

        private IQueryable<Doc_DocumentoListModel> Proietta(IQueryable<Doc_Documento> q, DocContesto ctx)
        {
            var idUtente = ctx.IdUtente;
            var idDip = ctx.IdDip_Anagrafica;
            return q.Select(x => new Doc_DocumentoListModel
            {
                Id = x.Id,
                Titolo = x.Titolo,
                IdDoc_Par_Tipo = x.IdDoc_Par_Tipo,
                Tipo = x.Doc_Par_TipoNavigation!.Descrizione,
                Categoria = x.Doc_Par_TipoNavigation.Categoria,
                IdDoc_Cartella = x.IdDoc_Cartella,
                Cartella = x.Doc_CartellaNavigation != null ? x.Doc_CartellaNavigation.Descrizione : null,
                IdDip_Anagrafica = x.IdDip_Anagrafica,
                Dipendente = x.Dip_AnagraficaNavigation != null ? (x.Dip_AnagraficaNavigation.Cognome + " " + x.Dip_AnagraficaNavigation.Nome) : null,
                IdAz_Cliente = x.IdAz_Cliente,
                Cliente = x.Az_ClienteNavigation != null ? x.Az_ClienteNavigation.Descrizione : null,
                DataDocumento = x.DataDocumento,
                Anno = x.Anno,
                Mese = x.Mese,
                DataScadenza = x.DataScadenza,
                Stato = x.Stato,
                VersioneCorrente = x.VersioneCorrente,
                NomeFile = x.Doc_Versione!.Where(v => v.Numero == x.VersioneCorrente).Select(v => v.NomeFile).FirstOrDefault() ?? string.Empty,
                Dimensione = x.Doc_Versione!.Where(v => v.Numero == x.VersioneCorrente).Select(v => v.Dimensione).FirstOrDefault(),
                DataCaricamento = x.CreationDate,
                NelCestino = x.NelCestino,
                PresaVisioneRichiesta = x.Doc_Destinatario!.Any(r => (r.IdAspNetUsers == idUtente || (idDip != null && r.IdDip_Anagrafica == idDip)) && r.PresaVisioneRichiesta),
                DataPresaVisione = x.Doc_Destinatario!.Where(r => r.IdAspNetUsers == idUtente || (idDip != null && r.IdDip_Anagrafica == idDip)).Select(r => r.DataPresaVisione).FirstOrDefault(),
                FirmaRichiesta = x.Doc_Destinatario!.Any(r => (r.IdAspNetUsers == idUtente || (idDip != null && r.IdDip_Anagrafica == idDip)) && r.FirmaRichiesta),
                DataFirma = x.Doc_Destinatario!.Where(r => r.IdAspNetUsers == idUtente || (idDip != null && r.IdDip_Anagrafica == idDip)).Select(r => r.DataFirma).FirstOrDefault(),
                FirmaDocumento = x.FirmaRichiesta,
                Firmatari = x.Doc_Destinatario!.Count(r => r.FirmaRichiesta),
                Firmati = x.Doc_Destinatario!.Count(r => r.FirmaRichiesta && r.DataFirma != null),
                UltimaFirma = x.Doc_Destinatario!.Where(r => r.FirmaRichiesta).Max(r => r.DataFirma),
                Nuovo = x.Doc_Destinatario!.Any(r => (r.IdAspNetUsers == idUtente || (idDip != null && r.IdDip_Anagrafica == idDip)) && r.DataPrimaApertura == null),
            });
        }

        // documenti oltre gli anni di conservazione della tipologia: solo segnalati, mai eliminati (decisione del 04/11/2026)
        private async Task CompletaConservazione(List<Doc_DocumentoListModel> documenti)
        {
            var idTipi = documenti.Select(d => d.IdDoc_Par_Tipo).Distinct().ToList();
            var anni = await _tipoRepository.FindAll(t => idTipi.Contains(t.Id) && t.AnniConservazione != null)
                                .ToDictionaryAsync(t => t.Id, t => t.AnniConservazione!.Value);
            foreach (var d in documenti)
                if (anni.TryGetValue(d.IdDoc_Par_Tipo, out var a))
                    d.OltreConservazione = (d.DataDocumento ?? d.DataCaricamento ?? DateTime.Today).AddYears(a) < DateTime.Today;
        }

        private async Task<Doc_DocumentoDettaglioModel> DettaglioAsync(DocContesto ctx, int id)
        {
            var doc = await _utility.Visibili(ctx).FirstOrDefaultAsync(x => x.Id == id)
                      ?? throw new UserMessageException(DocumentManagementMessages.Get("DocumentNotFound"));
            var tipo = await _tipoRepository.FindAll(t => t.Id == doc.IdDoc_Par_Tipo).FirstAsync();
            bool modificabile = PuoModificare(ctx, doc, tipo);

            var riepilogo = await Proietta(_documentoRepository.FindAll(x => x.Id == id), ctx).FirstAsync();
            await CompletaConservazione(new List<Doc_DocumentoListModel> { riepilogo });

            var versioni = await _versioneRepository.FindAll(v => v.IdDoc_Documento == id).OrderByDescending(v => v.Numero).ToListAsync();
            var autori = versioni.Select(v => v.IdAspNetUsersAutore).Where(x => x != null).Distinct().ToList();
            var nomi = await _usersRepository.FindAll(u => autori.Contains(u.Id)).Select(u => new { u.Id, u.UserName }).ToListAsync();

            var retVal = new Doc_DocumentoDettaglioModel
            {
                Modificabile = modificabile,
                Riepilogo = riepilogo,
                Documento = new Doc_DocumentoEditModel
                {
                    Id = doc.Id, IdDoc_Par_Tipo = doc.IdDoc_Par_Tipo, IdDoc_Cartella = doc.IdDoc_Cartella, IdDip_Anagrafica = doc.IdDip_Anagrafica,
                    IdAz_Cliente = doc.IdAz_Cliente, Titolo = doc.Titolo, Descrizione = doc.Descrizione, DataDocumento = doc.DataDocumento,
                    Anno = doc.Anno, Mese = doc.Mese, DataScadenza = doc.DataScadenza, Stato = doc.Stato, FirmaRichiesta = doc.FirmaRichiesta
                },
                // versioni precedenti: solo chi puo' modificare
                Versioni = versioni.Where(v => modificabile || v.Numero == doc.VersioneCorrente)
                                   .Select(v => new Doc_VersioneModel
                                   {
                                       Id = v.Id, Numero = v.Numero, NomeFile = v.NomeFile, ContentType = v.ContentType, Dimensione = v.Dimensione,
                                       Hash = v.Hash, Data = v.CreationDate, Note = v.Note,
                                       Autore = nomi.FirstOrDefault(n => n.Id == v.IdAspNetUsersAutore)?.UserName
                                   })
                                   .ToList()
            };

            // destinatari e prese visione: chi puo' modificare
            if (modificabile)
            {
                var dest = await _destinatarioRepository.FindAll(r => r.IdDoc_Documento == id)
                                    .Select(r => new { r, Dip = r.Dip_AnagraficaNavigation }).ToListAsync();
                var idUtenti = dest.Select(d => d.r.IdAspNetUsers).Where(x => x != null).Distinct().ToList();
                var nomiDest = await _usersRepository.FindAll(u => idUtenti.Contains(u.Id)).Select(u => new { u.Id, u.UserName }).ToListAsync();
                retVal.Destinatari = dest.Select(d => new Doc_DestinatarioModel
                {
                    Id = d.r.Id,
                    Nominativo = d.Dip != null ? $"{d.Dip.Cognome} {d.Dip.Nome}".Trim() : nomiDest.FirstOrDefault(n => n.Id == d.r.IdAspNetUsers)?.UserName ?? string.Empty,
                    PresaVisioneRichiesta = d.r.PresaVisioneRichiesta,
                    DataPrimaApertura = d.r.DataPrimaApertura,
                    DataPresaVisione = d.r.DataPresaVisione,
                    DataDownload = d.r.DataDownload,
                    FirmaRichiesta = d.r.FirmaRichiesta,
                    DataFirma = d.r.DataFirma,
                    FirmaVersione = d.r.FirmaVersione,
                    FirmaHash = d.r.FirmaHash,
                    FirmaIp = d.r.FirmaIp
                }).ToList();
            }
            return retVal;
        }

        private static bool PuoModificare(DocContesto ctx, Doc_Documento doc, Doc_Par_Tipo tipo)
            => (tipo.Categoria == Doc_Categoria.Personale || doc.IdDip_Anagrafica != null) ? ctx.CaricaPersonale : ctx.AmministraArchivio;

        private async Task<Doc_Documento> DocumentoModificabileAsync(DocContesto ctx, int id, bool? nelCestino = false)
        {
            var visibile = await _utility.Visibili(ctx).AnyAsync(x => x.Id == id);
            var doc = visibile ? await _documentoRepository.FindAll(x => x.Id == id).FirstOrDefaultAsync() : null;
            if (doc == null)
                throw new UserMessageException(DocumentManagementMessages.Get("DocumentNotFound"));
            if (nelCestino.HasValue && doc.NelCestino != nelCestino.Value)
                throw new UserMessageException(DocumentManagementMessages.Get(doc.NelCestino ? "DocumentInTrash" : "DocumentNotInTrash"));
            var tipo = await _tipoRepository.FindAll(t => t.Id == doc.IdDoc_Par_Tipo).FirstAsync();
            if (!PuoModificare(ctx, doc, tipo))
                throw new UserMessageException(DocumentManagementMessages.Get("NotAllowed"));
            return doc;
        }

        // coerenza dei dati con la tipologia e permessi di scrittura per la categoria
        private async Task<Doc_Par_Tipo> ValidaAsync(DocContesto ctx, Doc_DocumentoEditModel dati, bool nuovo)
        {
            var tipo = await _tipoRepository.FindAll(t => t.Id == dati.IdDoc_Par_Tipo && t.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
                       ?? throw new UserMessageException(DocumentManagementMessages.Get("TypeNotFound"));
            if (nuovo && !tipo.Active)
                throw new UserMessageException(DocumentManagementMessages.Get("TypeInactive", tipo.Descrizione));
            if (string.IsNullOrWhiteSpace(dati.Titolo))
                throw new UserMessageException(DocumentManagementMessages.Get("TitleRequired"));

            if (tipo.Categoria == Doc_Categoria.Personale)
            {
                if (!ctx.CaricaPersonale)
                    throw new UserMessageException(DocumentManagementMessages.Get("PersonnelNotAllowed"));
                if (!dati.IdDip_Anagrafica.HasValue || !await _dipendenteReadRepository.FindAll(x => x.Id == dati.IdDip_Anagrafica && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AnyAsync())
                    throw new UserMessageException(DocumentManagementMessages.Get("EmployeeRequired"));
            }
            else
            {
                if (!ctx.AmministraArchivio)
                    throw new UserMessageException(DocumentManagementMessages.Get("NotAllowed"));
                dati.IdDip_Anagrafica = null;
            }

            if (tipo.Categoria == Doc_Categoria.Cliente)
            {
                if (!dati.IdAz_Cliente.HasValue || !await _clienteReadRepository.FindAll(x => x.Id == dati.IdAz_Cliente && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AnyAsync())
                    throw new UserMessageException(DocumentManagementMessages.Get("CustomerRequired"));
            }
            else if (dati.IdAz_Cliente.HasValue && !await _clienteReadRepository.FindAll(x => x.Id == dati.IdAz_Cliente && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AnyAsync())
            {
                throw new UserMessageException(DocumentManagementMessages.Get("CustomerRequired"));
            }

            if (dati.IdDoc_Cartella.HasValue && !await _cartellaRepository.FindAll(x => x.Id == dati.IdDoc_Cartella && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AnyAsync())
                throw new UserMessageException(DocumentManagementMessages.Get("FolderNotFound"));
            if (dati.Mese.HasValue && (dati.Mese < 1 || dati.Mese > 12))
                throw new UserMessageException(DocumentManagementMessages.Get("PeriodInvalid"));
            if (dati.Anno.HasValue && (dati.Anno < 1900 || dati.Anno > 2200))
                throw new UserMessageException(DocumentManagementMessages.Get("PeriodInvalid"));
            return tipo;
        }

        private static void Applica(Doc_Documento doc, Doc_DocumentoEditModel dati, Doc_Par_Tipo tipo)
        {
            doc.IdDoc_Par_Tipo = tipo.Id;
            doc.IdDoc_Cartella = dati.IdDoc_Cartella;
            doc.IdDip_Anagrafica = dati.IdDip_Anagrafica;
            doc.IdAz_Cliente = dati.IdAz_Cliente;
            doc.Titolo = dati.Titolo.Trim().Length > 200 ? dati.Titolo.Trim()[..200] : dati.Titolo.Trim();
            doc.Descrizione = string.IsNullOrWhiteSpace(dati.Descrizione) ? null : dati.Descrizione.Trim();
            doc.DataDocumento = dati.DataDocumento?.Date;
            doc.Anno = tipo.HaCompetenza ? dati.Anno : null;
            doc.Mese = tipo.HaCompetenza ? dati.Mese : null;
            doc.DataScadenza = tipo.HaScadenza ? dati.DataScadenza?.Date : null;
            doc.Stato = dati.Stato;
            // richiesta di firma: scelta all'invio; se non indicata, quella della tipologia (documento nuovo) o quella gia' presente
            if (dati.FirmaRichiesta.HasValue)
                doc.FirmaRichiesta = dati.FirmaRichiesta.Value;
            else if (doc.Id == 0)
                doc.FirmaRichiesta = tipo.FirmaRichiesta;
        }

        // il dipendente del documento del personale e' sempre destinatario (con la presa visione della tipologia
        // e la richiesta di firma del documento)
        private async Task AggiornaDestinatarioDipendenteAsync(Doc_Documento doc, Doc_Par_Tipo tipo)
        {
            // richiesta di firma tolta o aggiunta: vale per tutti i destinatari (le firme gia' fatte restano come prova)
            var firma = doc.FirmaRichiesta;
            await _destinatarioRepository.FindAll(r => r.IdDoc_Documento == doc.Id && r.FirmaRichiesta != firma)
                     .ExecuteUpdateAsync(s => s.SetProperty(r => r.FirmaRichiesta, firma));

            // destinatari "dipendente" non piu' validi (dipendente cambiato o tolto)
            await _destinatarioRepository.FindAll(r => r.IdDoc_Documento == doc.Id && r.IdDip_Anagrafica != null && r.IdDip_Anagrafica != doc.IdDip_Anagrafica).ExecuteDeleteAsync();
            if (!doc.IdDip_Anagrafica.HasValue || await _destinatarioRepository.FindAll(r => r.IdDoc_Documento == doc.Id && r.IdDip_Anagrafica == doc.IdDip_Anagrafica).AnyAsync())
                return;

            var idUtente = await _dipendenteReadRepository.FindAll(x => x.Id == doc.IdDip_Anagrafica).Select(x => x.IdAspNetUsers).FirstOrDefaultAsync();
            await _destinatarioRepository.CreateAsync(new Doc_Destinatario
            {
                IdDoc_Documento = doc.Id,
                IdDip_Anagrafica = doc.IdDip_Anagrafica,
                IdAspNetUsers = idUtente,
                PresaVisioneRichiesta = tipo.PresaVisioneRichiesta,
                FirmaRichiesta = doc.FirmaRichiesta,
                CreationDate = DateTime.Now
            });
        }

        private async Task SegnaDestinatarioAsync(DocContesto ctx, int idDocumento, bool apertura, bool download)
        {
            var idDip = ctx.IdDip_Anagrafica;
            var q = _destinatarioRepository.FindAll(r => r.IdDoc_Documento == idDocumento && (r.IdAspNetUsers == ctx.IdUtente || (idDip != null && r.IdDip_Anagrafica == idDip)));
            var now = DateTime.Now;
            if (apertura)
                await q.Where(r => r.DataPrimaApertura == null).ExecuteUpdateAsync(s => s.SetProperty(r => r.DataPrimaApertura, now));
            if (download)
                await q.ExecuteUpdateAsync(s => s.SetProperty(r => r.DataDownload, now));
        }

        /*
         Prese visione chieste da altri moduli tramite il documentale (DM-FIRMA): il destinatario che ha completato quanto
         richiesto (firma se richiesta, altrimenti presa visione) conferma la richiesta del modulo. Un errore qui non annulla
         la firma: si registra e la conferma si puo' recuperare con una nuova firma o presa visione.
        */
        private async Task ConfermaPreseVisioneAsync(DocContesto ctx, int idDocumento, DateTime data, string? ip, string? userAgent)
        {
            try
            {
                var idDip = ctx.IdDip_Anagrafica;
                var idUtente = ctx.IdUtente;
                var completati = await _destinatarioRepository
                    .FindAll(r => r.IdDoc_Documento == idDocumento && (r.IdAspNetUsers == idUtente || (idDip != null && r.IdDip_Anagrafica == idDip))
                                  && r.DataPresaVisione != null && (!r.FirmaRichiesta || r.DataFirma != null))
                    .Select(r => r.Id).ToListAsync();
                if (completati.Count == 0)
                    return;
                // risolta qui e non nel costruttore: IAcknowledgementService dipende dai canali, tra cui quello del documentale
                var acknowledgement = _serviceProvider.GetRequiredService<IAcknowledgementService>();
                foreach (var id in completati)
                    await acknowledgement.ConfirmFromChannelAsync(DocumentManagementAcknowledgementChannel.CodeFes,
                                                                  DocumentManagementAcknowledgementChannel.IdCanale(ctx.IdCompany, id), data, ip, userAgent);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "[Documentale] Presa visione del documento {Id} non confermata al modulo", idDocumento);
            }
        }

        // conserva solo le ultime N versioni (impostazione dell'azienda), tranne nei documenti con presa visione o firma
        // richiesta e in quelli firmati almeno una volta (le versioni firmate sono prova: non si eliminano mai)
        private async Task PotaVersioniAsync(DocContesto ctx, int idDocumento)
        {
            var cfg = await _cfgRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstAsync();
            if (cfg.VersioniDaConservare <= 0
                || await _destinatarioRepository.FindAll(r => r.IdDoc_Documento == idDocumento && (r.PresaVisioneRichiesta || r.FirmaRichiesta)).AnyAsync()
                || await _logRepository.FindAll(l => l.IdDoc_Documento == idDocumento && l.Azione == Doc_LogAzione.Firma).AnyAsync())
                return;
            var vecchie = await _versioneRepository.FindAll(v => v.IdDoc_Documento == idDocumento)
                                   .OrderByDescending(v => v.Numero).Skip(cfg.VersioniDaConservare).ToListAsync();
            await DocumentFiles.EliminaVersioniAsync(_versioneRepository, _fileStorage, _quota, ctx.IdCompany, vecchie);
        }

        // email al dipendente destinatario di un documento pubblicato (se l'email e' configurata e l'azienda lo prevede)
        private async Task NotificaAsync(DocContesto ctx, Doc_Documento doc, Doc_Par_Tipo tipo)
        {
            try
            {
                if (doc.Stato != Doc_Stato.Pubblicato || !doc.IdDip_Anagrafica.HasValue || !_emailService.Configured)
                    return;
                var cfg = await _cfgRepository.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstAsync();
                if (!cfg.NotificheEmail)
                    return;
                var dip = await _dipendenteReadRepository.FindAll(x => x.Id == doc.IdDip_Anagrafica).FirstAsync();
                var email = dip.Email;
                if (string.IsNullOrWhiteSpace(email) && dip.IdAspNetUsers != null)
                    email = await _usersRepository.FindAll(u => u.Id == dip.IdAspNetUsers).Select(u => u.Email).FirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(email))
                    return;

                var oggetto = DocumentManagementMessages.Get("NotifySubject", tipo.Descrizione);
                var chiave = doc.FirmaRichiesta ? "NotifyTextSign" : tipo.PresaVisioneRichiesta ? "NotifyTextAcknowledge" : "NotifyText";
                var testo = DocumentManagementMessages.Get(chiave, $"{dip.Cognome} {dip.Nome}".Trim(), doc.Titolo);
                await _emailService.SendAsync(email, oggetto, System.Net.WebUtility.HtmlEncode(testo), testo);
            }
            catch (Exception ex)
            {
                // l'avviso non deve far fallire il caricamento
                Serilog.Log.Error(ex, "[Documentale] Avviso per il documento {Id} non inviato", doc.Id);
            }
        }

        // ---------------------------------------------------------------------------
        // Caricamento del file: controlli, spazio prenotato, hash, testo, salvataggio

        private sealed class FileCaricato
        {
            public string Key { get; init; } = string.Empty;
            public string NomeFile { get; init; } = string.Empty;
            public string ContentType { get; init; } = string.Empty;
            public long Dimensione { get; init; }
            public string Hash { get; init; } = string.Empty;
            public string? Testo { get; init; }
            public bool Confermato { get; set; }

            public Doc_Versione Versione(int numero, string? note, string idUtente, int idDocumento = 0) => new Doc_Versione
            {
                IdDoc_Documento = idDocumento, Numero = numero, StorageKey = Key, NomeFile = NomeFile, ContentType = ContentType,
                Dimensione = Dimensione, Hash = Hash, IdAspNetUsersAutore = idUtente,
                Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(), CreationDate = DateTime.Now, ChangeUser = idUtente
            };

            public async Task AnnullaSeNonConfermatoAsync(IFileStorage storage, IStorageQuotaService quota, int idCompany)
            {
                if (Confermato)
                    return;
                await quota.ReleaseAsync(idCompany, Dimensione);
                try { await storage.DeleteAsync(Key); } catch { /* resta solo spazio sprecato: lo segnala il riallineamento notturno */ }
            }
        }

        // file letto e controllato (nome, estensione, non vuoto), non ancora salvato
        private sealed class FileLetto
        {
            public string Nome { get; init; } = string.Empty;
            public byte[] Bytes { get; init; } = Array.Empty<byte>();
        }

        private async Task<FileCaricato> CaricaFileAsync(DocContesto ctx, Stream file, string nomeFile, string? contentType)
            => await SalvaFileAsync(ctx, await LeggiFileAsync(file, nomeFile), contentType);

        private async Task<FileLetto> LeggiFileAsync(Stream file, string nomeFile)
        {
            var nome = Path.GetFileName((nomeFile ?? string.Empty).Replace('\\', '/'));
            if (string.IsNullOrWhiteSpace(nome) || nome.Length > 255)
                throw new UserMessageException(DocumentManagementMessages.Get("FileNameInvalid"));
            if (!_estensioni.Contains(Path.GetExtension(nome)))
                throw new UserMessageException(DocumentManagementMessages.Get("FileTypeNotAllowed", Path.GetExtension(nome)));

            // il file si legge in memoria (al massimo la dimensione consentita): serve per hash e testo
            using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer);
            var bytes = buffer.ToArray();
            if (bytes.Length == 0)
                throw new UserMessageException(DocumentManagementMessages.Get("FileEmpty"));
            return new FileLetto { Nome = nome, Bytes = bytes };
        }

        // spazio prenotato, testo, hash e salvataggio di una copia del file
        private async Task<FileCaricato> SalvaFileAsync(DocContesto ctx, FileLetto letto, string? contentType)
        {
            var nome = letto.Nome;
            var bytes = letto.Bytes;
            using var buffer = new MemoryStream(bytes, writable: false);

            // spazio: oltre quota o file troppo grande si blocca qui, prima di salvare
            await _quota.ReserveAsync(ctx.IdCompany, bytes.Length);
            try
            {
                string? testo = null;
                try
                {
                    testo = FileText.Estrai(nome, bytes);
                    if (testo != null && testo.Length > MaxTesto)
                        testo = testo[..MaxTesto];
                }
                catch (Exception ex)
                {
                    // file senza testo leggibile (scansione, formato non gestito): il documento si cerca solo per titolo
                    Serilog.Log.Information("[Documentale] Testo non estratto da {File}: {Message}", nome, ex.Message);
                }

                var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                buffer.Position = 0;
                var key = await _fileStorage.SaveAsync(ctx.IdCompany, buffer);
                return new FileCaricato
                {
                    Key = key, NomeFile = nome, Dimensione = bytes.Length, Hash = hash, Testo = string.IsNullOrWhiteSpace(testo) ? null : testo,
                    ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
                };
            }
            catch
            {
                await _quota.ReleaseAsync(ctx.IdCompany, bytes.Length);
                throw;
            }
        }
    }

    public interface IDoc_DocumentoService : IServiceBase
    {
        Task<GenericResult<Doc_DocumentoListOutModel>> Doc_DocumentoList(GenericRequest<Doc_DocumentoListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Doc_DocumentoGetOutModel>> Doc_DocumentoGet(GenericRequest<Doc_DocumentoGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<Doc_DocumentoPutOutModel>> Doc_DocumentoUpload(GenericRequest<Doc_DocumentoEditModel> model, Stream file, string nomeFile, string? contentType, Boolean isSubProcess);
        Task<GenericResult<Doc_DocumentoUploadMultiploOutModel>> Doc_DocumentoUploadMultiplo(GenericRequest<Doc_DocumentoUploadMultiploInModel> model, Stream file, string nomeFile, string? contentType, Boolean isSubProcess);
        Task<GenericResult<Doc_DocumentoPutOutModel>> Doc_DocumentoNewVersion(GenericRequest<Doc_DocumentoIdInModel> model, Stream file, string nomeFile, string? contentType, Boolean isSubProcess);
        Task<GenericResult<Doc_DocumentoPutOutModel>> Doc_DocumentoPut(GenericRequest<Doc_DocumentoPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Doc_DocumentoIdOutModel>> Doc_DocumentoCestino(GenericRequest<Doc_DocumentoIdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Doc_DocumentoIdOutModel>> Doc_DocumentoRipristina(GenericRequest<Doc_DocumentoIdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Doc_DocumentoIdOutModel>> Doc_DocumentoElimina(GenericRequest<Doc_DocumentoIdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Doc_SvuotaCestinoOutModel>> Doc_SvuotaCestino(GenericRequest<Doc_SvuotaCestinoInModel> model, Boolean isSubProcess);
        Task<Doc_FileDownload> Doc_DocumentoDownload(int idDocumento, int? numeroVersione);
        Task<GenericResult<Doc_DocumentoIdOutModel>> Doc_DocumentoPresaVisione(GenericRequest<Doc_DocumentoIdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Doc_DocumentoIdOutModel>> Doc_DocumentoFirma(GenericRequest<Doc_DocumentoFirmaInModel> model, Boolean isSubProcess);
        Task<GenericResult<Doc_LogListOutModel>> Doc_LogList(GenericRequest<Doc_LogListInModel> model, Boolean isSubProcess);
    }
}
