using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using nvxapp.server.Base;
using nvxapp.server.service.ClientServer_Service.infrastructure.MyMokeLongJob.Models;
using System.Security.Claims;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.infrastructure.Notifications;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.infrastructure.MyMokeLongJob
{
    
    public class MyMokeLongJobService : ServiceBase, IMyMokeLongJobService
    {
        
        private readonly ILongJobNotifier _longJobNotifier;

        public MyMokeLongJobService(
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IAspNetUsersRepository aspNetUsersRepository,
            IOptions<JwtParameter> jwtParameter,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            
            ILongJobNotifier longJobNotifier) // Injected the abstraction
            : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
        
            _longJobNotifier = longJobNotifier;
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
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            Log.Information("Background task for job {JobId} is starting.", jobId);
                            await _longJobNotifier.LongJobProgressAsync(userId, 
                                                                        new LongJobProgressUpdate { JobId = jobId.ToString(), 
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
                                                                                                        ProgressPercentage = progress, 
                                                                                                        Message = new Message { Text = $"Processing step {i} of 5...", MsgType = MessageType.Information } 
                                                                                                      }
                                                                            );
                            }

                            Log.Information("Background task for job {JobId} has finished successfully.", jobId);
                            await _longJobNotifier.LongJobProgressAsync(userId, 
                                                                        new LongJobProgressUpdate { JobId = jobId.ToString(), 
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
    }

    
    public interface IMyMokeLongJobService : IServiceBase
    {
        Task<GenericResult<MyMokeLongJobOutModel>> StartJob(GenericRequest<MyMokeLongJobInModel> model, bool isSubProcess);
    }
}