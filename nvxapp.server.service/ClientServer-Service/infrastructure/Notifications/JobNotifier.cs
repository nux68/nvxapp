using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.HubAI;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.infrastructure.Notifications
{
    public class JobNotifier : IJobNotifier
    {
        private readonly IHubContext<SignalRHub> _hubContext;

        public JobNotifier(IHubContext<SignalRHub> hubContext, ILogger<LongJobNotifier> logger)
        {
            _hubContext = hubContext;
        }

        public async Task JobNotifierAsync(string userId, JobNotifierData update)
        {
            if (string.IsNullOrEmpty(userId))
            {
                Log.Warning("Attempted to send a progress update for JobId {JobId} without a userId.");
                return;
            }

            Log.Information("Sending SignalR update for User {UserId}. Message: {Message}",  userId,  update?.Message?.Text);

            
            await _hubContext.Clients.User(userId).SendAsync("JobNotifierAsync", update);
        }

    }


    public class JobNotifierData
    {
        
        [Required]
        public string JobType { get; set; } = string.Empty;  // permette di capire il tipo job 
        [Required]
        public object? Payload { get; set; }
        
        public Message? Message { get; set; }
        
    }


    public interface IJobNotifier
    {
        
        Task JobNotifierAsync(string userId, JobNotifierData update);
    }

}
