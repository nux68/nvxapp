using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace nvxapp.server.service.RabbitMQ.Listener
{

    //public class ListenerBase
    //{
    //    //private readonly IConfiguration? _configuration;

    //    //protected readonly ConnectionFactory? _factory = null;
    //    //protected IConnection? _connection;
    //    //protected IChannel? _channel;

    //    public ListenerBase(IConfiguration configuration)
    //    {

    //        //_configuration = configuration;

    //        //if (_configuration != null)
    //        //{
    //        //    int port = 5672;
    //        //    string? sPort = _configuration["RabbitQm:Connection:Port"];
    //        //    if (!string.IsNullOrEmpty(sPort))
    //        //        port = int.Parse(sPort);


    //        //    _factory = new ConnectionFactory
    //        //    {
    //        //        HostName = _configuration["RabbitQm:Connection:HostName"] ?? "localhost",
    //        //        Port = port,
    //        //        UserName = _configuration["RabbitQm:Connection:UserName"] ?? "guest",
    //        //        Password = _configuration["RabbitQm:Connection:Password"] ?? "guest"
    //        //    };
    //        //}


    //    }
    //}

    public class ListenerTemplate : /*ListenerBase,*/ IHostedService, IRabbitMqListenerService
    {
        //protected readonly ConnectionFactory? _factory = null;
        //private readonly IConfiguration? _configuration;
        //protected IConnection? _connection;
        //protected IChannel? _channel;
        
        private readonly CancellationTokenSource _cts = new(); // Aggiungi questo campo alla classe
        private readonly IServiceProvider _serviceProvider;
        private readonly iRabbitMqConnection _rabbitMqConnection;

        public ListenerTemplate(IConfiguration configuration, 
                                //iRabbitMqConnection rabbitMqConnection,
                                IServiceProvider serviceProvider) // : base(configuration)
        {

            _serviceProvider = serviceProvider;
            
            using (var scope = _serviceProvider.CreateScope())
            {
                _rabbitMqConnection = scope.ServiceProvider.GetRequiredService<iRabbitMqConnection>();
            }

            //////////Rabbit
            _cts = new CancellationTokenSource(); // Inizializza il token
            
            //_configuration = configuration;

            //if (_configuration != null)
            //{
            //    int port = 5672;
            //    string? sPort = _configuration["RabbitQm:Connection:Port"];
            //    if (!string.IsNullOrEmpty(sPort))
            //        port = int.Parse(sPort);


            //    _factory = new ConnectionFactory
            //    {
            //        HostName = _configuration["RabbitQm:Connection:HostName"] ?? "localhost",
            //        Port = port,
            //        UserName = _configuration["RabbitQm:Connection:UserName"] ?? "guest",
            //        Password = _configuration["RabbitQm:Connection:Password"] ?? "guest"
            //    };
            //}
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            await _rabbitMqConnection.Start();

            //try
            //{
            //    if (_factory != null)
            //    {
            //        _connection = await _factory.CreateConnectionAsync();
            //        _channel = await _connection.CreateChannelAsync();

            // Avvia la ricezione dei messaggi.  Passa una funzione che gestirà i messaggi.
            // Importante: usa il token di cancellazione per interrompere l'ascolto quando necessario.
            _ = Task.Run(async () => await ConsumeAsync(HandleMessageAsync, cancellationToken), cancellationToken); // Ignora il warning

            //    }
            //    else
            //    {
            //        Log.Error("StartAsync RabbitMq:(1)Impossibile stabilira la connessione con il server");
            //    }
            //}
            //catch (Exception e)
            //{
            //    Log.Error(e, "StartAsync RabbitMq:(2)Impossibile stabilira la connessione con il server");
            //}
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel(); // Segnala che l'operazione deve essere annullata

            await _rabbitMqConnection.Start();

            //////// Attendi un breve periodo di tempo per consentire a ConsumeAsync di terminare
            //////await Task.Delay(TimeSpan.FromSeconds(5)); // Tempo ragionevole

            //////if (_channel != null && _channel.IsOpen)
            //////{
            //////    await _channel.CloseAsync();
            //////}

            //////if (_connection != null && _connection.IsOpen)
            //////{
            //////    await _connection.CloseAsync();
            //////}

        }

        protected virtual  async Task HandleMessageAsync(object message)
        {
            // Qui elabora il messaggio ricevuto da RabbitMQ.
            Console.WriteLine($"Messaggio ricevuto: {message}");

            // Esempio: salva il messaggio in un database, elabora dati, ecc.
            //await _myDatabaseService.SaveMessageAsync(message); // ipotetico servizio di database

            // Gestisci eventuali eccezioni qui dentro!
            await Task.CompletedTask; // Assicurati di restituire un Task
        }

        public async Task ConsumeAsync(Func<object, Task> messageHandler, CancellationToken cancellationToken)
        {
            if (_rabbitMqConnection._channel == null)
                throw new InvalidOperationException("RabbitMQ non è connesso!");


            await _rabbitMqConnection._channel.QueueDeclareAsync(queue: "my-durable-queue", durable: false, exclusive: false, autoDelete: false);

            QueueDeclareOk queueDeclareResult = await _rabbitMqConnection._channel.QueueDeclareAsync();
            string queueName = "my-durable-queue"; //queueDeclareResult.QueueName;
            await _rabbitMqConnection._channel.QueueBindAsync(queue: queueName, exchange: "logs", routingKey: string.Empty);


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

    }


    //public class MyListener: ListenerTemplate
    //{
    //    public MyListener(IConfiguration configuration):base(configuration)
    //    {

    //    }
    //}
}
