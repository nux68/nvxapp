using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.HumanResources._utility;
using nvxapp.server.service.ClientServer_Service.HumanResources.Models;
using nvxapp.server.service.ClientServer_Service.infrastructure.Notifications;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.HumanResources
{
    /*
     Cedolini e CU (HR-CED, ANALISI_HUMANRESOURCES_APPLICATIVO.md §4.2):
        - ufficio del personale (CompanyPowerAdmin/CompanyAdmin) e studio paghe (pagine lato studio, IdCompany nella richiesta):
          caricamento del PDF unico, divisione per dipendente in un job, verifica degli scarti, pubblicazione in un job,
          report delle conferme di presa visione
        - dipendente (qualsiasi utente dell'azienda collegato a un Dip_Anagrafica): "I miei cedolini"
     La logica sta in HrCedoliniCore; qui permessi, scope dell'azienda e job (RunInBackground + ILongJobNotifier).
    */
    public class Hr_CedolinoService : ServiceBase, IHr_CedolinoService
    {
        public const string JobDivisione = "Hr_CedoliniDivisione";
        public const string JobPubblicazione = "Hr_CedoliniPubblicazione";

        private readonly IHrUtility _utility;
        private readonly IServiceScopeFactory _scopeFactory;

        public Hr_CedolinoService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IHrUtility utility,
                                  IServiceScopeFactory scopeFactory) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _utility = utility;
            _scopeFactory = scopeFactory;
        }

        // lavoro sull'azienda della richiesta (o su quella indicata dallo studio) con la logica dei cedolini
        private async Task<T> Azienda<T>(int? idCompany, Func<IHrCedoliniCore, HrContesto, Task<T>> work)
        {
            var ctx = await _utility.ContestoCedoliniAsync(idCompany);
            return await HrAzienda.RunAsync(_scopeFactory, ctx, (sp, c) => work(sp.GetRequiredService<IHrCedoliniCore>(), c));
        }

        public virtual async Task<GenericResult<Hr_StudioAziendeOutModel>> Hr_StudioAziende(GenericRequest<Hr_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () => new Hr_StudioAziendeOutModel
            {
                Aziende = (await _utility.AziendeStudioAsync()).Select(x => new Hr_StudioAziendaModel { IdCompany = x.IdCompany, Descrizione = x.Descrizione }).ToList()
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_CaricamentoListOutModel>> Hr_CaricamentoList(GenericRequest<Hr_CaricamentoListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () => new Hr_CaricamentoListOutModel
            {
                Caricamenti = await Azienda(model.Data.IdCompany, (core, ctx) => core.ListAsync(ctx, model.Data.Anno))
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_CaricamentoGetOutModel>> Hr_CaricamentoGet(GenericRequest<Hr_CedIdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () => await Azienda(model.Data.IdCompany, (core, ctx) => core.GetAsync(ctx, model.Data.Id)), isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_CaricamentoUploadOutModel>> Hr_CaricamentoUpload(GenericRequest<Hr_CaricamentoUploadModel> model, Stream content, string fileName, long length, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var userId = UserIdFirstConnection;
                var ctxRichiesta = await _utility.ContestoCedoliniAsync(model.Data.IdCompany);
                var (caricamento, ctx) = await HrAzienda.RunAsync(_scopeFactory, ctxRichiesta, async (sp, c) =>
                    (await sp.GetRequiredService<IHrCedoliniCore>().CreaAsync(c, model.Data, content, fileName, length), c));

                var jobId = Guid.NewGuid().ToString();
                var testi = new { Avviato = HumanResourcesMessages.Get("JobSplitStarted"), Fine = HumanResourcesMessages.Get("JobSplitDone"), Errore = HumanResourcesMessages.Get("JobSplitFailed") };
                RunInBackground(async scope =>
                {
                    var notifier = scope.ServiceProvider.GetRequiredService<ILongJobNotifier>();
                    Task Notifica(int progress, string testo, MessageType tipo, bool fine = false) =>
                        notifier.LongJobProgressAsync(userId, new LongJobProgressUpdate
                        {
                            JobId = jobId, JobType = JobDivisione, Payload = new { caricamento.Id, ctx.IdCompany }, Category = LongJobCategory.Calculation,
                            ProgressPercentage = progress, Message = new Message { Text = testo, MsgType = tipo }, IsFinished = fine
                        });
                    try
                    {
                        await Notifica(5, testi.Avviato, MessageType.Information);
                        await HrAzienda.RunAsync(_scopeFactory, ctx, async (sp, c) =>
                        {
                            await sp.GetRequiredService<IHrCedoliniCore>().DividiAsync(c, caricamento.Id, p => Notifica(p, testi.Avviato, MessageType.Information));
                            return 0;
                        });
                        await Notifica(100, testi.Fine, MessageType.Information, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[HR] Job {JobId} di divisione dei cedolini non riuscito", jobId);
                        await Notifica(100, testi.Errore, MessageType.Exception, true);
                    }
                });

                var outModel = new Hr_CaricamentoUploadOutModel { JobId = jobId };
                outModel.Caricamento.Id = caricamento.Id;
                outModel.Caricamento.Stato = (int)caricamento.Stato;
                return outModel;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_CaricamentoGetOutModel>> Hr_ScartoAssegna(GenericRequest<Hr_ScartoAssegnaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
                await Azienda(model.Data.IdCompany, (core, ctx) => core.AssegnaAsync(ctx, model.Data.IdScarto, model.Data.IdDip_Anagrafica)), isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_CaricamentoGetOutModel>> Hr_ScartoIgnora(GenericRequest<Hr_CedIdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () => await Azienda(model.Data.IdCompany, (core, ctx) => core.IgnoraAsync(ctx, model.Data.Id)), isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_JobOutModel>> Hr_CaricamentoPubblica(GenericRequest<Hr_CedIdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var userId = UserIdFirstConnection;
                var ctxRichiesta = await _utility.ContestoCedoliniAsync(model.Data.IdCompany);
                var ctx = await HrAzienda.RunAsync(_scopeFactory, ctxRichiesta, async (sp, c) =>
                {
                    var car = await sp.GetRequiredService<IHrCedoliniCore>().CaricamentoAsync(c, model.Data.Id);
                    if (car.Stato != data.Entities.Tenant.HumanResources.Hr_StatoCaricamento.DaPubblicare && car.Stato != data.Entities.Tenant.HumanResources.Hr_StatoCaricamento.DaVerificare)
                        throw new UserMessageException(HumanResourcesMessages.Get("PayslipNotPublishable"));
                    return c;
                });

                var jobId = Guid.NewGuid().ToString();
                var testi = new { Avviato = HumanResourcesMessages.Get("JobPublishStarted"), Fine = HumanResourcesMessages.Get("JobPublishDone"), Errore = HumanResourcesMessages.Get("JobPublishFailed") };
                RunInBackground(async scope =>
                {
                    var notifier = scope.ServiceProvider.GetRequiredService<ILongJobNotifier>();
                    Task Notifica(int progress, string testo, MessageType tipo, bool fine = false) =>
                        notifier.LongJobProgressAsync(userId, new LongJobProgressUpdate
                        {
                            JobId = jobId, JobType = JobPubblicazione, Payload = new { model.Data.Id, ctx.IdCompany }, Category = LongJobCategory.Calculation,
                            ProgressPercentage = progress, Message = new Message { Text = testo, MsgType = tipo }, IsFinished = fine
                        });
                    try
                    {
                        await Notifica(5, testi.Avviato, MessageType.Information);
                        await HrAzienda.RunAsync(_scopeFactory, ctx, (sp, c) =>
                            sp.GetRequiredService<IHrCedoliniCore>().PubblicaAsync(c, model.Data.Id, p => Notifica(p, testi.Avviato, MessageType.Information)));
                        await Notifica(100, testi.Fine, MessageType.Information, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[HR] Job {JobId} di pubblicazione dei cedolini non riuscito", jobId);
                        await Notifica(100, testi.Errore, MessageType.Exception, true);
                    }
                });
                return new Hr_JobOutModel { JobId = jobId };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_IdOutModel>> Hr_CaricamentoDelete(GenericRequest<Hr_CedIdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                await Azienda(model.Data.IdCompany, async (core, ctx) => { await core.DeleteAsync(ctx, model.Data.Id); return 0; });
                return new Hr_IdOutModel { Id = model.Data.Id };
            }, isSubProcess);
        }

        // documento di un dipendente per l'ufficio del personale o lo studio
        public virtual Task<Hr_FileDownload> Hr_DocDownload(int id, int? idCompany)
            => Azienda(idCompany, (core, ctx) => core.DownloadAsync(ctx, id, false));

        // ------------------------------------------------------------------------------------------------ area del dipendente
        public virtual async Task<GenericResult<Hr_MieiDocumentiOutModel>> Hr_MieiDocumenti(GenericRequest<Hr_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                return await HrAzienda.RunAsync(_scopeFactory, ctx, (sp, c) => sp.GetRequiredService<IHrCedoliniCore>().MieiAsync(c));
            }, isSubProcess);
        }

        public virtual async Task<Hr_FileDownload> Hr_MioDocDownload(int id)
        {
            var ctx = await _utility.ContestoAsync();
            return await HrAzienda.RunAsync(_scopeFactory, ctx, (sp, c) => sp.GetRequiredService<IHrCedoliniCore>().DownloadAsync(c, id, true));
        }
    }

    public interface IHr_CedolinoService : IServiceBase
    {
        Task<GenericResult<Hr_StudioAziendeOutModel>> Hr_StudioAziende(GenericRequest<Hr_EmptyInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_CaricamentoListOutModel>> Hr_CaricamentoList(GenericRequest<Hr_CaricamentoListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_CaricamentoGetOutModel>> Hr_CaricamentoGet(GenericRequest<Hr_CedIdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_CaricamentoUploadOutModel>> Hr_CaricamentoUpload(GenericRequest<Hr_CaricamentoUploadModel> model, Stream content, string fileName, long length, Boolean isSubProcess);
        Task<GenericResult<Hr_CaricamentoGetOutModel>> Hr_ScartoAssegna(GenericRequest<Hr_ScartoAssegnaInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_CaricamentoGetOutModel>> Hr_ScartoIgnora(GenericRequest<Hr_CedIdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_JobOutModel>> Hr_CaricamentoPubblica(GenericRequest<Hr_CedIdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_IdOutModel>> Hr_CaricamentoDelete(GenericRequest<Hr_CedIdInModel> model, Boolean isSubProcess);
        Task<Hr_FileDownload> Hr_DocDownload(int id, int? idCompany);
        Task<GenericResult<Hr_MieiDocumentiOutModel>> Hr_MieiDocumenti(GenericRequest<Hr_EmptyInModel> model, Boolean isSubProcess);
        Task<Hr_FileDownload> Hr_MioDocDownload(int id);
    }
}
