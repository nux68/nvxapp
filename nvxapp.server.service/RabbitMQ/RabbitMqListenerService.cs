using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using nvxapp.server.service.RabbitMQ.Listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.RabbitMQ
{
    public class RabbitMqListenerService : IHostedService , IRabbitMqListenerService
    {
        //private readonly ILogger<RabbitMqListenerService> _logger;
       // public IServiceProvider ServiceProvider { get; }

        public RabbitMqListenerService(/*IServiceProvider serviceProvider */ /*ILogger<RabbitMqListenerService> logger*/)
        {
            Console.WriteLine("AAAAAAAAAAAAAA");

            //_logger = logger;
            //ServiceProvider = serviceProvider;
        }

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    using (var scope = ServiceProvider.CreateScope())
        //    {
        //        //var scopedService = scope.ServiceProvider.GetRequiredService<IMyScopedService>();
        //        // your usual code
        //    }

        //    //_logger.LogInformation("RabbitMqListenerService è stato avviato.");
        //    await Task.Delay(Timeout.Infinite, stoppingToken);
        //}

        //private Task _backgroundTask;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //_backgroundTask = Task.Run(async () =>
            //{
            //    while (!cancellationToken.IsCancellationRequested)
            //    {
            //        // Logica del servizio in background
            //        Console.WriteLine("Servizio Custom in esecuzione...");
            //        await Task.Delay(1000); // Aspetta 1 secondo
            //    }
            //});

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Servizio Custom arrestato.");
            return Task.CompletedTask;
        }

    }

    
}
