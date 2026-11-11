using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.DocumentManagement;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.DocumentManagement;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Acknowledgement;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage;
using nvxapp.server.service.Helpers;
using Serilog;
using System.Security.Cryptography;

namespace nvxapp.server.service.ClientServer_Service.DocumentManagement._utility
{
    /*
     Canale di presa visione del documentale (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, DM-FIRMA.1): quando un modulo chiede una
     presa visione (IAcknowledgementService.RequestAsync) e il documentale e' attivo, la richiesta diventa un documento del personale
     rivolto al destinatario, con presa visione richiesta e firma elettronica semplice (FES) se la tipologia la propone.
        - disponibile: documentale attivo per l'azienda, gia' inizializzato, tipologia "PRESAVIS" non disattivata
          (per non usarlo basta disattivare la tipologia: le richieste tornano al canale base "conferma")
        - documento: il file passato dal modulo (request.Contenuto/NomeFile) oppure un verbale PDF generato con titolo e testo
          della richiesta (DocumentManagementVerbale, DM-FIRMA.2)
        - riferimento nel canale: "<IdCompany>:<Doc_Destinatario.Id>" (gli id del documentale sono per schema: da soli non
          sarebbero univoci tra le aziende)
        - conferma: Doc_DocumentoService, alla firma (o alla presa visione se la firma non e' richiesta), chiama
          IAcknowledgementService.ConfirmFromChannelAsync con data, IP e browser
     Il documento si scrive con un contesto creato per lo schema dell'azienda della richiesta: il canale funziona anche fuori
     da una richiesta HTTP o con un'altra azienda selezionata nel token.
    */
    public class DocumentManagementAcknowledgementChannel : IAcknowledgementChannel
    {
        public const string CodeFes = "doc.fes";
        public const string CodiceTipo = "PRESAVIS";
        public const string PaginaDestinatario = "/docmine";

        private readonly TenancySettings _tenancy;
        private readonly IApplicationDbContextFactory _factory;
        private readonly ITenantProvisioningService _provisioning;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IAspNetUsersRepository _usersRepository;
        private readonly IPresaVisioneRepository _presaVisioneRepository;
        private readonly IFileStorage _fileStorage;
        private readonly IStorageQuotaService _quota;

        public DocumentManagementAcknowledgementChannel(TenancySettings tenancy,
                                                        IApplicationDbContextFactory factory,
                                                        ITenantProvisioningService provisioning,
                                                        IServiceProvider serviceProvider,
                                                        IHttpContextAccessor httpContextAccessor,
                                                        ICompanyApplicationRepository companyApplicationRepository,
                                                        ICompanyRepository companyRepository,
                                                        IAspNetUsersRepository usersRepository,
                                                        IPresaVisioneRepository presaVisioneRepository,
                                                        IFileStorage fileStorage,
                                                        IStorageQuotaService quota)
        {
            _tenancy = tenancy;
            _factory = factory;
            _provisioning = provisioning;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
            _companyApplicationRepository = companyApplicationRepository;
            _companyRepository = companyRepository;
            _usersRepository = usersRepository;
            _presaVisioneRepository = presaVisioneRepository;
            _fileStorage = fileStorage;
            _quota = quota;
        }

        public string Code => CodeFes;
        public int Priority => 10;
        public string? UrlPerUtente => PaginaDestinatario;

        /// <summary>riferimento nel canale del destinatario di un documento</summary>
        public static string IdCanale(int idCompany, int idDoc_Destinatario) => $"{idCompany}:{idDoc_Destinatario}";

        public async Task<bool> AvailableAsync(int idCompany)
        {
            if (!await _companyApplicationRepository.FindAll(x => x.IdCompany == idCompany && x.ApplicationType == ApplicationType.DocumentManagement && x.Active).AnyAsync())
                return false;
            await using var db = await DocumentManagementSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, CancellationToken.None);
            if (db == null)
                return false;
            var idAz = await new Doc_Az_AnagraficaReadRepository(db, _serviceProvider, _httpContextAccessor)
                             .FindAll(x => x.IdCompany == idCompany).Select(x => (int?)x.Id).FirstOrDefaultAsync();
            if (!idAz.HasValue || !await new Doc_CfgRepository(db, _serviceProvider, _httpContextAccessor).FindAll(x => x.IdAz_Anagrafica == idAz).AnyAsync())
                return false;
            // tipologia mancante: si crea alla prima richiesta; disattivata: il documentale non si usa per le prese visione
            var attiva = await new Doc_Par_TipoRepository(db, _serviceProvider, _httpContextAccessor)
                               .FindAll(x => x.IdAz_Anagrafica == idAz && x.Codice == CodiceTipo).Select(x => (bool?)x.Active).FirstOrDefaultAsync();
            return attiva ?? true;
        }

        public async Task<string?> StartAsync(PresaVisione presaVisione, AcknowledgementRequest request)
        {
            try
            {
                var idCanale = await CreaDocumentoAsync(presaVisione, request);
                // si conferma in "I miei documenti": la pagina delle prese visione apre quella
                await _presaVisioneRepository.FindAll(x => x.Id == presaVisione.Id)
                         .ExecuteUpdateAsync(u => u.SetProperty(x => x.Url, PaginaDestinatario).SetProperty(x => x.UrlState, (string?)null));
                presaVisione.Url = PaginaDestinatario;
                presaVisione.UrlState = null;
                return idCanale;
            }
            catch (Exception ex)
            {
                // documento non creato (es. spazio esaurito): la richiesta resta confermabile con il canale base
                Log.Warning(ex, "[Documentale] Presa visione {Id} non creata nel documentale: si usa il canale base", presaVisione.Id);
                await _presaVisioneRepository.FindAll(x => x.Id == presaVisione.Id)
                         .ExecuteUpdateAsync(u => u.SetProperty(x => x.Canale, ConfirmationChannel.CodeConferma));
                presaVisione.Canale = ConfirmationChannel.CodeConferma;
                return null;
            }
        }

        private async Task<string> CreaDocumentoAsync(PresaVisione pv, AcknowledgementRequest request)
        {
            int idCompany = pv.IdCompany;
            await using var db = await DocumentManagementSchema.ContextAsync(idCompany, _tenancy, _factory, _provisioning, CancellationToken.None)
                                 ?? throw new InvalidOperationException("Schema del documentale non presente");
            var anagrafica = new Doc_Az_AnagraficaReadRepository(db, _serviceProvider, _httpContextAccessor);
            var tipoRepository = new Doc_Par_TipoRepository(db, _serviceProvider, _httpContextAccessor);
            var dipendenteRepository = new Doc_Dip_AnagraficaReadRepository(db, _serviceProvider, _httpContextAccessor);
            var documentoRepository = new Doc_DocumentoRepository(db, _serviceProvider, _httpContextAccessor);
            var logRepository = new Doc_LogRepository(db, _serviceProvider, _httpContextAccessor);

            var idAz = await anagrafica.FindAll(x => x.IdCompany == idCompany).Select(x => (int?)x.Id).FirstOrDefaultAsync()
                       ?? throw new InvalidOperationException("Anagrafica dell'azienda non trovata");
            var tipo = await TipologiaAsync(tipoRepository, idAz);

            var dip = await dipendenteRepository.FindAll(x => x.IdAz_Anagrafica == idAz && x.IdAspNetUsers == pv.IdAspNetUsers)
                                                .Select(x => new { x.Id, x.Cognome, x.Nome }).FirstOrDefaultAsync();
            var richiedente = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var now = DateTime.Now;

            // file del modulo oppure verbale generato con il testo della richiesta
            string nomeFile;
            string contentType;
            byte[] bytes;
            if (request.Contenuto != null && request.Contenuto.Length > 0)
            {
                nomeFile = Path.GetFileName((request.NomeFile ?? string.Empty).Replace('\\', '/'));
                if (string.IsNullOrWhiteSpace(nomeFile))
                    nomeFile = "documento.pdf";
                bytes = request.Contenuto;
                contentType = Path.GetExtension(nomeFile).Equals(".pdf", StringComparison.OrdinalIgnoreCase) ? "application/pdf" : "application/octet-stream";
            }
            else
            {
                string destinatario = dip != null
                    ? $"{dip.Cognome} {dip.Nome}".Trim()
                    : await _usersRepository.FindAll(u => u.Id == pv.IdAspNetUsers).Select(u => u.UserName).FirstOrDefaultAsync() ?? pv.IdAspNetUsers;
                var azienda = await _companyRepository.FindAll(x => x.Id == idCompany).Select(x => x.Descrizione).FirstOrDefaultAsync();
                bytes = DocumentManagementVerbale.Crea(new DocumentManagementVerbale.Dati
                {
                    Azienda = azienda,
                    Titolo = pv.Titolo,
                    Testo = pv.Testo,
                    Destinatario = destinatario,
                    Data = now,
                    Modello = DocumentManagementVerbale.ModelloDaFonte(pv.Fonte),
                    Firma = tipo.FirmaRichiesta
                });
                nomeFile = NomeFile(pv.Titolo) + ".pdf";
                contentType = "application/pdf";
            }
            if (nomeFile.Length > 255)
                nomeFile = nomeFile[^255..];

            string? testo = null;
            try { testo = FileText.Estrai(nomeFile, bytes); } catch { /* file senza testo leggibile: si cerca per titolo */ }
            if (testo != null && testo.Length > 200_000)
                testo = testo[..200_000];
            var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

            await _quota.ReserveAsync(idCompany, bytes.Length);
            string? key = null;
            bool confermato = false;
            try
            {
                using (var ms = new MemoryStream(bytes, writable: false))
                    key = await _fileStorage.SaveAsync(idCompany, ms);

                var destinatarioDoc = new Doc_Destinatario
                {
                    IdDip_Anagrafica = dip?.Id,
                    IdAspNetUsers = pv.IdAspNetUsers,
                    PresaVisioneRichiesta = true,
                    FirmaRichiesta = tipo.FirmaRichiesta,
                    CreationDate = now
                };
                var doc = new Doc_Documento
                {
                    IdAz_Anagrafica = idAz,
                    IdDoc_Par_Tipo = tipo.Id,
                    IdDip_Anagrafica = dip?.Id,
                    Titolo = AcknowledgementService.Taglia(pv.Titolo, 200)!,
                    Descrizione = string.IsNullOrWhiteSpace(pv.Testo) ? null : AcknowledgementService.Taglia(pv.Testo.Trim(), 1000),
                    DataDocumento = now.Date,
                    Stato = Doc_Stato.Pubblicato,
                    Origine = Doc_Origine.Azienda,
                    FirmaRichiesta = tipo.FirmaRichiesta,
                    VersioneCorrente = 1,
                    Testo = string.IsNullOrWhiteSpace(testo) ? null : testo,
                    CreationDate = now,
                    ChangeUser = richiedente,
                    Doc_Versione = new List<Doc_Versione>
                    {
                        new Doc_Versione
                        {
                            Numero = 1, StorageKey = key, NomeFile = nomeFile, ContentType = contentType, Dimensione = bytes.Length, Hash = hash,
                            IdAspNetUsersAutore = richiedente, CreationDate = now, ChangeUser = richiedente
                        }
                    },
                    Doc_Destinatario = new List<Doc_Destinatario> { destinatarioDoc }
                };
                doc = await documentoRepository.CreateAsync(doc);
                await _quota.ConfirmAsync(idCompany, bytes.Length);
                confermato = true;

                await logRepository.CreateAsync(new Doc_Log
                {
                    IdDoc_Documento = doc.Id,
                    Versione = 1,
                    IdAspNetUsers = richiedente,
                    Azione = Doc_LogAzione.Caricamento,
                    Data = now,
                    Dettaglio = AcknowledgementService.Taglia(DocumentManagementMessages.Get("AckLogDetail", pv.Fonte, nomeFile), 500),
                    Ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    CreationDate = now,
                    ChangeUser = richiedente
                });
                return IdCanale(idCompany, destinatarioDoc.Id);
            }
            finally
            {
                if (!confermato)
                {
                    await _quota.ReleaseAsync(idCompany, bytes.Length);
                    if (key != null)
                        try { await _fileStorage.DeleteAsync(key); } catch { /* resta solo spazio sprecato */ }
                }
            }
        }

        // tipologia dedicata alle prese visione: si crea alla prima richiesta (anche nelle aziende inizializzate prima)
        private static async Task<Doc_Par_Tipo> TipologiaAsync(Doc_Par_TipoRepository tipoRepository, int idAz)
        {
            var tipo = await tipoRepository.FindAll(x => x.IdAz_Anagrafica == idAz && x.Codice == CodiceTipo).FirstOrDefaultAsync();
            if (tipo != null)
                return tipo;
            try
            {
                return await tipoRepository.CreateAsync(NuovaTipologia(idAz, 200));
            }
            catch (DbUpdateException ex)
            {
                // creata nel frattempo da un'altra richiesta (indice unico su azienda e codice)
                ex.Entries.FirstOrDefault()?.Context.ChangeTracker.Clear();
                return await tipoRepository.FindAll(x => x.IdAz_Anagrafica == idAz && x.Codice == CodiceTipo).FirstAsync();
            }
        }

        /// <summary>tipologia "Verbali e prese visione": personale, presa visione e firma (FES) proposte, conservazione 10 anni</summary>
        public static Doc_Par_Tipo NuovaTipologia(int idAz, int ordine) => new Doc_Par_Tipo
        {
            IdAz_Anagrafica = idAz,
            Codice = CodiceTipo,
            Descrizione = DocumentManagementMessages.Get("DefaultTypeAcknowledgement"),
            Categoria = Doc_Categoria.Personale,
            AnniConservazione = 10,
            PresaVisioneRichiesta = true,
            FirmaRichiesta = true,
            Ordine = ordine,
            Active = true,
            CreationDate = DateTime.Now
        };

        private static string NomeFile(string titolo)
        {
            var nome = new string(titolo.Select(c => Path.GetInvalidFileNameChars().Contains(c) || char.IsControl(c) ? '_' : c).ToArray()).Trim();
            if (nome.Length > 100)
                nome = nome[..100];
            return string.IsNullOrWhiteSpace(nome) ? "presa_visione" : nome;
        }
    }
}
