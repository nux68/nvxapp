using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.HubAI;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.infrastructure.Notifications
{
    
    public class LongJobNotifier : ILongJobNotifier
    {
        private readonly IHubContext<SignalRHub> _hubContext;

        public LongJobNotifier(IHubContext<SignalRHub> hubContext, ILogger<LongJobNotifier> logger)
        {
            _hubContext = hubContext;
        }

        public async Task LongJobProgressAsync(string userId, LongJobProgressUpdate update)
        {
            if (string.IsNullOrEmpty(userId))
            {
                Log.Warning("Attempted to send a progress update for JobId {JobId} without a userId.", update.JobId);
                return;
            }

            Log.Information("Sending SignalR update for JobId {JobId} to User {UserId}. Progress: {Progress}%, Message: {Message}",
                            update.JobId, userId, update.ProgressPercentage, update?.Message?.Text);

            // Send the update to the specific user who initiated the job.
            // The client must be listening for the "ReceiveJobProgress" event.
            await _hubContext.Clients.User(userId).SendAsync("LongJobProgress", update);
        }
    }

    
    public interface ILongJobNotifier
    {
        
        Task LongJobProgressAsync(string userId, LongJobProgressUpdate update);
    }


    
    public class LongJobProgressUpdate
    {
        [Required]
        public string JobId { get; set; } = string.Empty;
        [Required]
        public string JobType { get; set; } = string.Empty;  // permette di capire il tipo job 
        [Required]
        public object? Payload { get; set; }
        [Required]
        public LongJobCategory Category { get; set; }

        public int ProgressPercentage { get; set; }
        
        public Message? Message { get; set; }
        
        public bool IsFinished { get; set; }
    }


    public enum LongJobCategory
    {
          Calculation,
          FileGeneration

    }
}


