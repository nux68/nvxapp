using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Crm;
using nvxapp.server.service.ClientServer_Service.Crm._utility;
using nvxapp.server.service.ClientServer_Service.Crm.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Crm
{
    /*
     Note e allegati di clienti, opportunita' (e lead per le note; offerte per gli allegati), ANALISI_CRM_APPLICATIVO.md §5.2.
     Gli allegati stanno sempre qui (anche con il documentale attivo), su disco con IFileStorage; non contano nello
     spazio del documentale. Note: le modifica e le cancella l'autore o un amministratore.
    */
    public class Crm_NotaAllegatoService : ServiceBase, ICrm_NotaAllegatoService
    {
        // dimensione massima di un allegato del CRM
        public const long MaxAllegato = 20L * 1024 * 1024;

        private readonly ICrm_NotaRepository _Crm_Nota;
        private readonly ICrm_AllegatoRepository _Crm_Allegato;
        private readonly ICrmUtility _utility;
        private readonly IFileStorage _fileStorage;

        public Crm_NotaAllegatoService(IMapper mapper,
                                       UserManager<ApplicationUser> userManager,
                                       IAspNetUsersRepository aspNetUsersRepository,
                                       IOptions<JwtParameter> jwtParameter,
                                       IHttpContextAccessor httpContextAccessor,
                                       IConfiguration configuration,
                                       ICrm_NotaRepository crm_NotaRepository,
                                       ICrm_AllegatoRepository crm_AllegatoRepository,
                                       ICrmUtility utility,
                                       IFileStorage fileStorage) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Crm_Nota = crm_NotaRepository;
            _Crm_Allegato = crm_AllegatoRepository;
            _utility = utility;
            _fileStorage = fileStorage;
        }

        public virtual async Task<GenericResult<Crm_NotaPutOutModel>> Crm_NotaPut(GenericRequest<Crm_NotaPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var d = model.Data.Nota;
                if (string.IsNullOrWhiteSpace(d.Testo))
                    throw new UserMessageException(CrmMessages.Get("NoteTextRequired"));

                Crm_Nota n;
                if (d.Id != 0)
                {
                    n = await _Crm_Nota.FindAll(_ => true).AsTracking().FirstOrDefaultAsync(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                        ?? throw new UserMessageException(CrmMessages.Get("NoteNotFound"));
                    await ControllaCollegamento(ctx, n.IdAz_Cliente, n.IdCrm_Opportunita, n.IdCrm_Lead, null);
                    if (!ctx.Amministratore && n.IdAspNetUsersAutore != ctx.IdUtente)
                        throw new UserMessageException(CrmMessages.Get("NoteNotOwner"));
                }
                else
                {
                    await ControllaCollegamento(ctx, d.IdAz_Cliente, d.IdCrm_Opportunita, d.IdCrm_Lead, null);
                    n = new Crm_Nota
                    {
                        IdAz_Anagrafica = ctx.IdAz_Anagrafica, IdAz_Cliente = d.IdAz_Cliente, IdCrm_Opportunita = d.IdCrm_Opportunita, IdCrm_Lead = d.IdCrm_Lead,
                        IdAspNetUsersAutore = ctx.IdUtente, CreationDate = DateTime.Now
                    };
                    await _Crm_Nota.CreateAsync(n, false);
                }
                n.Testo = d.Testo.Trim();
                n.Fissata = d.Fissata;
                n.ModifiedDate = DateTime.Now;
                n.ChangeUser = ctx.IdUtente;
                await _Crm_Nota.SaveChange();
                return new Crm_NotaPutOutModel
                {
                    Nota = (await CrmLists.NoteAsync(_Crm_Nota.FindAll(x => x.Id == n.Id), ctx, _utility)).First()
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_IdOutModel>> Crm_NotaDelete(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var n = await _Crm_Nota.FindAll(_ => true).AsTracking().FirstOrDefaultAsync(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                        ?? throw new UserMessageException(CrmMessages.Get("NoteNotFound"));
                await ControllaCollegamento(ctx, n.IdAz_Cliente, n.IdCrm_Opportunita, n.IdCrm_Lead, null);
                if (!ctx.Amministratore && n.IdAspNetUsersAutore != ctx.IdUtente)
                    throw new UserMessageException(CrmMessages.Get("NoteNotOwner"));
                await _Crm_Nota.DeleteAsync(n);
                await _Crm_Nota.SaveChange();
                return new Crm_IdOutModel { Id = model.Data.Id };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_AllegatoUploadOutModel>> Crm_AllegatoUpload(GenericRequest<Crm_AllegatoUploadModel> model, Stream content, string fileName, string? contentType, long length, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var d = model.Data;
                if (new[] { d.IdAz_Cliente.HasValue, d.IdCrm_Opportunita.HasValue, d.IdCrm_Offerta.HasValue }.Count(x => x) != 1)
                    throw new UserMessageException(CrmMessages.Get("AttachmentLinkRequired"));
                if (length <= 0)
                    throw new UserMessageException(CrmMessages.Get("AttachmentEmpty"));
                if (length > MaxAllegato)
                    throw new UserMessageException(CrmMessages.Get("AttachmentTooLarge", MaxAllegato / (1024 * 1024)));
                await ControllaCollegamento(ctx, d.IdAz_Cliente, d.IdCrm_Opportunita, null, d.IdCrm_Offerta);

                var key = await _fileStorage.SaveAsync(ctx.IdCompany, content);
                var a = new Crm_Allegato
                {
                    IdAz_Anagrafica = ctx.IdAz_Anagrafica, IdAz_Cliente = d.IdAz_Cliente, IdCrm_Opportunita = d.IdCrm_Opportunita, IdCrm_Offerta = d.IdCrm_Offerta,
                    NomeFile = Path.GetFileName(fileName) is { Length: > 0 } nome ? (nome.Length > 260 ? nome[^260..] : nome) : "file",
                    ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
                    Dimensione = length, StorageKey = key, IdAspNetUsersAutore = ctx.IdUtente, CreationDate = DateTime.Now, ChangeUser = ctx.IdUtente
                };
                await _Crm_Allegato.CreateAsync(a, false);
                try
                {
                    await _Crm_Nota.SaveChange();
                }
                catch
                {
                    await _fileStorage.DeleteAsync(key);
                    throw;
                }
                return new Crm_AllegatoUploadOutModel
                {
                    Allegato = (await CrmLists.AllegatiAsync(_Crm_Allegato.FindAll(x => x.Id == a.Id), _utility)).First()
                };
            }, isSubProcess);
        }

        public virtual async Task<Crm_FileDownload> Crm_AllegatoDownload(int id)
        {
            var ctx = await _utility.ContestoAsync();
            var a = await Allegato(ctx, id, false);
            return new Crm_FileDownload { Content = await _fileStorage.OpenAsync(a.StorageKey), NomeFile = a.NomeFile, ContentType = a.ContentType };
        }

        public virtual async Task<GenericResult<Crm_IdOutModel>> Crm_AllegatoDelete(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var a = await Allegato(ctx, model.Data.Id, true);
                if (!ctx.Amministratore && a.IdAspNetUsersAutore != ctx.IdUtente)
                    throw new UserMessageException(CrmMessages.Get("AttachmentNotOwner"));
                await _Crm_Allegato.DeleteAsync(a);
                await _Crm_Nota.SaveChange();
                try { await _fileStorage.DeleteAsync(a.StorageKey); }
                catch (Exception ex) { Log.Error(ex, "[CRM] File {Key} non eliminato", a.StorageKey); }
                return new Crm_IdOutModel { Id = model.Data.Id };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ utilita'
        private async Task<Crm_Allegato> Allegato(CrmContesto ctx, int id, bool tracking)
        {
            var q = _Crm_Allegato.FindAll(x => x.Id == id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
            var a = await (tracking ? q.AsTracking() : q.AsNoTracking()).FirstOrDefaultAsync() ?? throw new UserMessageException(CrmMessages.Get("AttachmentNotFound"));
            await ControllaCollegamento(ctx, a.IdAz_Cliente, a.IdCrm_Opportunita, null, a.IdCrm_Offerta);
            return a;
        }

        // l'oggetto a cui si collega la nota o l'allegato dev'essere visibile all'utente
        private async Task ControllaCollegamento(CrmContesto ctx, int? idCliente, int? idOpportunita, int? idLead, int? idOfferta)
        {
            if (idCliente is null && idOpportunita is null && idLead is null && idOfferta is null)
                throw new UserMessageException(CrmMessages.Get("AttachmentLinkRequired"));
            if (idCliente is int c)
                await _utility.ClienteAsync(ctx, c);
            if (idOpportunita is int o && !await _utility.OpportunitaVisibili(ctx).AnyAsync(x => x.Id == o))
                throw new UserMessageException(CrmMessages.Get("OpportunityNotFound"));
            if (idLead is int l && !await _utility.LeadVisibili(ctx).AnyAsync(x => x.Id == l))
                throw new UserMessageException(CrmMessages.Get("LeadNotFound"));
            if (idOfferta is int f && !await _utility.OfferteVisibili(ctx).AnyAsync(x => x.Id == f))
                throw new UserMessageException(CrmMessages.Get("OfferNotFound"));
        }
    }

    public interface ICrm_NotaAllegatoService : IServiceBase
    {
        Task<GenericResult<Crm_NotaPutOutModel>> Crm_NotaPut(GenericRequest<Crm_NotaPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_IdOutModel>> Crm_NotaDelete(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_AllegatoUploadOutModel>> Crm_AllegatoUpload(GenericRequest<Crm_AllegatoUploadModel> model, Stream content, string fileName, string? contentType, long length, Boolean isSubProcess);
        Task<Crm_FileDownload> Crm_AllegatoDownload(int id);
        Task<GenericResult<Crm_IdOutModel>> Crm_AllegatoDelete(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess);
    }
}
