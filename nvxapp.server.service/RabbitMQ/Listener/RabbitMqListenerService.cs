using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Text;

namespace nvxapp.server.service.RabbitMQ.Listener
{
    public abstract class RabbitMqListenerService : IHostedService, IRabbitMqListenerService
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly IServiceProvider _serviceProvider;
        private readonly iRabbitMqConnection _rabbitMqConnection;

        public RabbitMqListenerService(IConfiguration configuration,
                                       IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            using (var scope = _serviceProvider.CreateScope())
            {
                _rabbitMqConnection = scope.ServiceProvider.GetRequiredService<iRabbitMqConnection>();
            }

            _cts = new CancellationTokenSource();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _rabbitMqConnection.Start();
            _ = Task.Run(async () => await ConsumeAsync(HandleMessageAsync, cancellationToken), cancellationToken);
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel(); // Segnala che l'operazione deve essere annullata

            await _rabbitMqConnection.Stop();
        }
        public async Task ConsumeAsync(Func<object, Task> messageHandler, CancellationToken cancellationToken)
        {
            if (_rabbitMqConnection._channel == null)
                throw new InvalidOperationException("RabbitMQ non è connesso!");


            string queueName = QueueName;

            QueueDeclareOk queueDeclareResult = await _rabbitMqConnection._channel.QueueDeclareAsync(queue: queueName,
                                                                                                     durable: false,
                                                                                                     exclusive: false,
                                                                                                     autoDelete: false);


            await _rabbitMqConnection._channel.QueueBindAsync(queue: queueName,
                                                              exchange: Exchange ,
                                                              routingKey: RoutingKey);


            Console.WriteLine(" [*] Waiting for logs.");

            var consumer = new AsyncEventingBasicConsumer(_rabbitMqConnection._channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    byte[] body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($" [x] {message}");

                    // Chiama il message handler fornito
                    await messageHandler(message);
                }
                catch (Exception ex)
                {
                    // Gestisci l'eccezione qui (log, retry, ecc.)
                    Log.Error($"Errore durante l'elaborazione del messaggio: {ex}");
                }
            };

            //serve per indentificare il consumer, magari per spegnerlo
            string consumerTag = await _rabbitMqConnection._channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);
            try
            {
                // Attendi finché non viene richiesta la cancellazione
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("ConsumeAsync cancellato.");
            }
            finally
            {
                // Annulla la sottoscrizione al consumer
                await _rabbitMqConnection._channel.BasicCancelAsync(consumerTag);
            }
        }


        protected abstract Task HandleMessageAsync(object message);


        public abstract string QueueName { get; }
        public abstract string Exchange { get; }
        public abstract string RoutingKey { get; }

    }

    public class RabbitMqListenerService_Demo : RabbitMqListenerService
    {

        public RabbitMqListenerService_Demo(IConfiguration configuration,
                                            IServiceProvider serviceProvider) : base(configuration, serviceProvider) { }


        protected override async Task HandleMessageAsync(object message)
        {
            // Qui elabora il messaggio ricevuto da RabbitMQ.
            Console.WriteLine($"Messaggio ricevuto: {message}");

            // Esempio: salva il messaggio in un database, elabora dati, ecc.
            //await _myDatabaseService.SaveMessageAsync(message); // ipotetico servizio di database

            // Gestisci eventuali eccezioni qui dentro!
            await Task.CompletedTask; // Assicurati di restituire un Task
        }

        

        public override string QueueName => "my-queue";
        public override string Exchange => "logs";
        public override string RoutingKey => string.Empty;

    }


}
