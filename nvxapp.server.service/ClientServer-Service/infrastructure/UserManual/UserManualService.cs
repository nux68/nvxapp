using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.infrastructure.Notifications;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.UserManual
{
    public class UserManualGenerateInModel
    {
        public string Code { get; set; } = string.Empty;
        // nome del sito (SITE_NAME del client), nel pie' di pagina e in copertina
        public string? SiteName { get; set; }
        // menu dell'utente per questo manuale (sezioni e voci che vede, titoli tradotti): il manuale si compone da qui
        public List<UserManualMenuSection> Menu { get; set; } = new();
    }

    public class UserManualGenerateOutModel : ModelResult
    {
        public string JobId { get; set; } = string.Empty;
    }

    /*
     Manuali utente in PDF (vedi UserManual.cs): il manuale si compone nella lingua della richiesta e il PDF si scrive con
     un job in background (RunInBackground), con l'avanzamento e il link di download nella barra dei job (ILongJobNotifier).
     Un manuale di un applicativo si genera solo se l'applicativo e' attivo per l'azienda dell'utente.
    */
    public class UserManualService : ServiceBase, IUserManualService
    {
        public const string JobType = "UserManual_Pdf";

        private readonly IEnumerable<IUserManualProvider> _providers;
        private readonly ICompanyApplicationRepository _companyApplicationRepository;

        public UserManualService(IMapper mapper,
                                 UserManager<ApplicationUser> userManager,
                                 IAspNetUsersRepository aspNetUsersRepository,
                                 IOptions<JwtParameter> jwtParameter,
                                 IHttpContextAccessor httpContextAccessor,
                                 IConfiguration configuration,
                                 IEnumerable<IUserManualProvider> providers,
                                 ICompanyApplicationRepository companyApplicationRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _providers = providers;
            _companyApplicationRepository = companyApplicationRepository;
        }

        public virtual async Task<GenericResult<UserManualGenerateOutModel>> UserManualGenerate(GenericRequest<UserManualGenerateInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new UserManualGenerateOutModel();
                var provider = _providers.FirstOrDefault(p => string.Equals(p.Code, model.Data.Code, StringComparison.OrdinalIgnoreCase))
                               ?? throw new UserMessageException(InfrastructureMessages.Get("UserManualNotFound"));

                if (provider.Application is { } app)
                {
                    int.TryParse(this.CurrentCompany, out int idCompany);
                    if (!await _companyApplicationRepository.FindAll(x => x.IdCompany == idCompany && x.ApplicationType == app && x.Active).AnyAsync())
                        throw new UserMessageException(InfrastructureMessages.Get("UserManualNotActive"));
                }

                var userId = this.UserIdFirstConnection;
                if (string.IsNullOrEmpty(userId))
                    throw new UserMessageException(InfrastructureMessages.Get("UserManualNotFound"));

                // testo nella lingua della richiesta; nel job solo la scrittura del PDF
                // solo le parti per i ruoli dell'utente con cui si lavora (anche impersonificato)
                var utente = string.IsNullOrEmpty(CurrentUserId) ? null : await _userManager.FindByIdAsync(CurrentUserId);
                var ruoli = utente == null ? new List<string>() : (await _userManager.GetRolesAsync(utente)).ToList();
                var manuale = provider.Build(ruoli, model.Data.Menu ?? new List<UserManualMenuSection>());
                var sito = string.IsNullOrWhiteSpace(model.Data.SiteName) ? manuale.Title : model.Data.SiteName.Trim();
                var jobId = Guid.NewGuid().ToString();
                retVal.JobId = jobId;
                var payload = new { provider.Code };
                var testi = new
                {
                    Avviato = InfrastructureMessages.Get("UserManualStarted"),
                    Pronto = InfrastructureMessages.Get("UserManualReady"),
                    Errore = InfrastructureMessages.Get("UserManualFailed"),
                };
                var pdfTesti = UserManualPdf.Testi.Correnti();

                RunInBackground(async scope =>
                {
                    var notifier = scope.ServiceProvider.GetRequiredService<ILongJobNotifier>();
                    Task Notifica(int progress, string testo, MessageType tipo, JobFile? file = null, bool fine = false) =>
                        notifier.LongJobProgressAsync(userId, new LongJobProgressUpdate
                        {
                            JobId = jobId, JobType = JobType, Payload = payload, Category = LongJobCategory.FileGeneration,
                            DownloadUrl = file?.DownloadUrl, FileName = file?.FileName, ProgressPercentage = progress,
                            Message = new Message { Text = testo, MsgType = tipo }, IsFinished = fine
                        });
                    try
                    {
                        await Notifica(10, testi.Avviato, MessageType.Information);
                        var pdf = UserManualPdf.Crea(manuale, sito, pdfTesti);
                        var nome = manuale.Title;
                        var file = NVXSystem.SalvaFileJob(jobId, userId, nome, "pdf", pdf);
                        await Notifica(100, testi.Pronto, MessageType.Information, file, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Manuale {Code}: job {JobId} non riuscito", provider.Code, jobId);
                        await Notifica(100, testi.Errore, MessageType.Exception, null, true);
                    }
                });

                await Task.Delay(DelayAsyncMethod);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IUserManualService : IServiceBase
    {
        Task<GenericResult<UserManualGenerateOutModel>> UserManualGenerate(GenericRequest<UserManualGenerateInModel> model, Boolean isSubProcess);
    }
}
