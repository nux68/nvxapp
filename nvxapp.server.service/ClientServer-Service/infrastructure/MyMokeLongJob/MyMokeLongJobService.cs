using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.infrastructure.MyMokeLongJob.Models;
using nvxapp.server.service.ClientServer_Service.infrastructure.Notifications;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.infrastructure.MyMokeLongJob
{
    
    public class MyMokeLongJobService : ServiceBase, IMyMokeLongJobService
    {
        
        private ILongJobNotifier? _longJobNotifier;

        public MyMokeLongJobService(
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IAspNetUsersRepository aspNetUsersRepository,
            IOptions<JwtParameter> jwtParameter,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration
            
            //ILongJobNotifier longJobNotifier
            ) // Injected the abstraction
            : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
        
            //_longJobNotifier = longJobNotifier;
        }

        
        public virtual async Task<GenericResult<MyMokeLongJobOutModel>> StartJob(GenericRequest<MyMokeLongJobInModel> model, bool isSubProcess)
        {
            #pragma warning disable 1998
            return await ExecuteAction(model, async () =>
            #pragma warning restore 1998
            {
                var outModel = new MyMokeLongJobOutModel();
                var jobId = Guid.NewGuid();
                outModel.JobId = jobId.ToString();

                // Get the current user's ID to send targeted SignalR notifications
                var userId =this.UserIdFirstConnection;

                if (string.IsNullOrEmpty(userId))
                {
                    Log.Information("Could not find user ID. Unable to send SignalR notifications for job {JobId}.", jobId);
                    outModel.Messages.Add(new Message("User not identified; cannot start job.", MessageType.Error));
                }
                else
                {
                    Log.Information("Request to start long-running job {JobId} for user {UserId} received.", jobId, userId);

                    // Fire and forget the background task
                    RunInBackground(async scope =>
                    {
                        _longJobNotifier = scope.ServiceProvider.GetRequiredService<ILongJobNotifier>();

                        try
                        {
                            Log.Information("Background task for job {JobId} is starting.", jobId);
                            await _longJobNotifier.LongJobProgressAsync(userId, 
                                                                        new LongJobProgressUpdate { JobId = jobId.ToString(), 
                                                                                                    JobType = "MyMokeLongJob",
                                                                                                    Category = LongJobCategory.Calculation,
                                                                                                    Payload = null,
                                                                                                    ProgressPercentage = 0, 
                                                                                                    Message = new Message { Text = "Job is starting...", MsgType = MessageType.Information } 
                                                                                                   }
                                                                       );

                            // Simulate a long process with 5 steps
                            for (int i = 1; i <= 5; i++)
                            {
                                await Task.Delay(3000); // 3-second delay for each step
                                int progress = i * 20;
                                Log.Information("Job {JobId}: Progress step {Step}/5 ({Progress}%)", jobId, i, progress);

                                await _longJobNotifier.LongJobProgressAsync(userId, 
                                                                            new LongJobProgressUpdate { JobId = jobId.ToString(), 
                                                                                                        JobType = "MyMokeLongJob",
                                                                                                        Category = LongJobCategory.Calculation,
                                                                                                        Payload = null,
                                                                                                        ProgressPercentage = progress, 
                                                                                                        Message = new Message { Text = $"Processing step {i} of 5...", MsgType = MessageType.Information } 
                                                                                                      }
                                                                            );
                            }

                            Log.Information("Background task for job {JobId} has finished successfully.", jobId);
                            await _longJobNotifier.LongJobProgressAsync(userId, 
                                                                        new LongJobProgressUpdate { JobId = jobId.ToString(), 
                                                                                                    JobType = "MyMokeLongJob",
                                                                                                    Category = LongJobCategory.Calculation,
                                                                                                    Payload = null,
                                                                                                    ProgressPercentage = 100, 
                                                                                                    Message = new Message { Text = "Job completed successfully.", MsgType = MessageType.Information },
                                                                                                    IsFinished = true 
                                                                                                  }
                                                                       );
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Background task for job {JobId} failed.", jobId);
                            await _longJobNotifier.LongJobProgressAsync(userId, 
                                                                        new LongJobProgressUpdate { JobId = jobId.ToString(), 
                                                                                                    JobType = "MyMokeLongJob",
                                                                                                    Payload = null,
                                                                                                    ProgressPercentage = 100, 
                                                                                                    Message = new Message { Text = $"Job failed: {ex.Message}", MsgType = MessageType.Exception },
                                                                                                    IsFinished = true 
                                                                                                  }
                                                                        );
                        }
                    });

                    
                    outModel.Messages.Add(new Message($"Job started with ID: {outModel.JobId}", MessageType.Information));

                    

                }
                
                return outModel;
            }, isSubProcess);
        }
    

        public virtual async Task<GenericResult<MyMokeLongJobOutModel>> ExportJob(GenericRequest<MyMokeLongJobInModel> model, bool isSubProcess)
        {
            #pragma warning disable 1998
            return await ExecuteAction(model, async () =>
            #pragma warning restore 1998
            {
                var outModel = new MyMokeLongJobOutModel();
                var jobId = Guid.NewGuid();
                outModel.JobId = jobId.ToString();

                // Get the current user's ID to send targeted SignalR notifications
                var userId =this.UserIdFirstConnection;

                if (string.IsNullOrEmpty(userId))
                {
                    Log.Information("Could not find user ID. Unable to send SignalR notifications for job {JobId}.", jobId);
                    outModel.Messages.Add(new Message("User not identified; cannot start job.", MessageType.Error));
                }
                else
                {
                    Log.Information("Request to start long-running job {JobId} for user {UserId} received.", jobId, userId);

                    // Fire and forget the background task
                    RunInBackground(async scope =>
                    {
                        _longJobNotifier = scope.ServiceProvider.GetRequiredService<ILongJobNotifier>();

                        try
                        {
                            string? downloadUrl = null;

                            Log.Information("Background task for job {JobId} is starting.", jobId);
                            await _longJobNotifier.LongJobProgressAsync(userId, 
                                                                        new LongJobProgressUpdate { JobId = jobId.ToString(), 
                                                                                                    JobType = "MyMokeExportJob",
                                                                                                    Category = LongJobCategory.FileGeneration,
                                                                                                    DownloadUrl = downloadUrl,
                                                                                                    Payload = null,
                                                                                                    ProgressPercentage = 0, 
                                                                                                    Message = new Message { Text = "Export is starting...", MsgType = MessageType.Information } 
                                                                                                   }
                                                                       );

                            // Simulate a long process with 5 steps
                            for (int i = 1; i <= 5; i++)
                            {
                                await Task.Delay(3000); // 3-second delay for each step
                                int progress = i * 20;
                                Log.Information("Job {JobId}: Progress step {Step}/5 ({Progress}%)", jobId, i, progress);

                                await _longJobNotifier.LongJobProgressAsync(userId, 
                                                                            new LongJobProgressUpdate { JobId = jobId.ToString(), 
                                                                                                        JobType = "MyMokeExportJob",
                                                                                                        Category = LongJobCategory.FileGeneration,
                                                                                                        Payload = null,
                                                                                                        ProgressPercentage = progress, 
                                                                                                        Message = new Message { Text = $"Export Processing step {i} of 5...", MsgType = MessageType.Information } 
                                                                                                      }
                                                                            );
                            }

                            var exportsFolder = NVXSystem.ExportFolder;
                            string fileName;
                            fileName = await WriteTxtFile("MokeExport", jobId.ToString(), exportsFolder);

                            Log.Information("Background task for job {JobId} has finished successfully.", jobId);
                            await _longJobNotifier.LongJobProgressAsync(userId, 
                                                                        new LongJobProgressUpdate { JobId = jobId.ToString(), 
                                                                                                    JobType = "MyMokeExportJob",
                                                                                                    Category = LongJobCategory.FileGeneration,
                                                                                                    Payload = null,
                                                                                                    DownloadUrl = $"{NVXSystem.DownloadURL}{fileName}",
                                                                                                    ProgressPercentage = 100, 
                                                                                                    Message = new Message { Text = "Export completato, clicca per scaricare", MsgType = MessageType.Information },
                                                                                                    IsFinished = true 
                                                                                                  }
                                                                       );
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Background task for job {JobId} failed.", jobId);
                            await _longJobNotifier.LongJobProgressAsync(userId, 
                                                                        new LongJobProgressUpdate { JobId = jobId.ToString(), 
                                                                                                    JobType = "MyMokeExportJob",
                                                                                                    Category = LongJobCategory.FileGeneration,
                                                                                                    Payload = null,
                                                                                                    ProgressPercentage = 100, 
                                                                                                    Message = new Message { Text = $"Export failed: {ex.Message}", MsgType = MessageType.Exception },
                                                                                                    IsFinished = true 
                                                                                                  }
                                                                        );
                        }
                    });

                    
                    outModel.Messages.Add(new Message($"Export started with ID: {outModel.JobId}", MessageType.Information));

                    

                }
                
                return outModel;
            }, isSubProcess);
        }
        

        private static async Task<string> WriteTxtFile(string codice,string jobId, string folder)
        {
            const string separator = "\t";
            var sb = new StringBuilder();

            sb.AppendLine(string.Join(separator, "ciao", "ciao", "ciao", "ciao", "ciao"));

            

            var fileName = $"{codice}_{jobId}.txt";
            var filePath = Path.Combine(folder, fileName);
            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
            return fileName;
        }

    }

    
    public interface IMyMokeLongJobService : IServiceBase
    {
        Task<GenericResult<MyMokeLongJobOutModel>> StartJob(GenericRequest<MyMokeLongJobInModel> model, bool isSubProcess);
        Task<GenericResult<MyMokeLongJobOutModel>> ExportJob(GenericRequest<MyMokeLongJobInModel> model, bool isSubProcess);
    }
}