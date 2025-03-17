using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Serilog;

namespace nvxapp.server.service.RabbitMQ
{
    public class RabbitMqConnection: iRabbitMqConnection
    {
        private readonly IConfiguration? _configuration;
        private readonly CancellationTokenSource _cts = new();
        protected readonly ConnectionFactory? _factory = null;
        public IConnection? _connection;
        public IChannel? _channel { get; set; }

        public RabbitMqConnection(IConfiguration configuration)
        {

            _configuration = configuration;

            _cts = new CancellationTokenSource(); // Inizializza il token

            if (_configuration != null)
            {
                int port = 5672;
                string? sPort = _configuration["RabbitQm:Connection:Port"];
                if (!string.IsNullOrEmpty(sPort))
                    port = int.Parse(sPort);

                _factory = new ConnectionFactory
                {
                    HostName = _configuration["RabbitQm:Connection:HostName"] ?? "localhost",
                    Port = port,
                    UserName = _configuration["RabbitQm:Connection:UserName"] ?? "guest",
                    Password = _configuration["RabbitQm:Connection:Password"] ?? "guest"
                };
            }
        }

        public async Task Start()
        {
            try
            {
                if (_factory != null)
                {
                    _connection = await _factory.CreateConnectionAsync();
                    _channel = await _connection.CreateChannelAsync();
                }
                else
                {
                    Log.Error("StartAsync RabbitMq:(1)Impossibile stabilira la connessione con il server");
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "StartAsync RabbitMq:(2)Impossibile stabilira la connessione con il server");
            }
        }

        public async Task Stop(int Milliseconds)
        {
            _cts.Cancel(); // Segnala che l'operazione deve essere annullata

            // Attendi un breve periodo di tempo per consentire a ConsumeAsync di terminare
            //await Task.Delay(TimeSpan.FromSeconds(5)); // Tempo ragionevole
            await Task.Delay(TimeSpan.FromMilliseconds(Milliseconds)); // Tempo ragionevole

            if (_channel != null && _channel.IsOpen)
            {
                await _channel.CloseAsync();
            }

            if (_connection != null && _connection.IsOpen)
            {
                await _connection.CloseAsync();
            }

        }

    }

    public interface iRabbitMqConnection
    {
        public  Task Start();
        public Task Stop(int Milliseconds);
        public IChannel? _channel { get; set; }
    }

}
