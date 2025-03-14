//////////using Microsoft.Extensions.Hosting;
//////////using RabbitMQ.Client;
//////////using RabbitMQ.Client.Events;
//////////using System;
//////////using System.Collections.Generic;
//////////using System.Linq;
//////////using System.Text;
//////////using System.Threading.Tasks;

//////////namespace nvxapp.server.service.RabbitMQ.Listener
//////////{
//////////    public class RabbitMqService : IHostedService, IRabbitMqListenerService
//////////    {
//////////        private readonly ConnectionFactory _factory;
//////////        private IConnection? _connection;
//////////        private IChannel? _channel;

//////////        public RabbitMqService()
//////////        {
//////////            _factory = new ConnectionFactory { HostName = "localhost" };
//////////        }

//////////        public async Task StartAsync(CancellationToken cancellationToken)
//////////        {
//////////            _connection = await _factory.CreateConnectionAsync();
//////////            _channel = await _connection.CreateChannelAsync();
//////////        }

//////////        public Task SendMessageAsync(string message)
//////////        {
//////////            if (_channel == null)
//////////                throw new InvalidOperationException("RabbitMQ non è connesso!");

//////////            var body = Encoding.UTF8.GetBytes(message);

//////////            _channel.ExchangeDeclareAsync(exchange: "logs", type: ExchangeType.Fanout);
//////////            _channel.BasicPublishAsync(exchange: "logs", routingKey: string.Empty, body: body);

//////////            return Task.CompletedTask;
//////////        }

//////////        public async Task ConsumeAsync(Func<string, Task> messageHandler)
//////////        {
//////////            if (_channel == null)
//////////                throw new InvalidOperationException("RabbitMQ non è connesso!");

//////////            // declare a server-named queue
//////////            QueueDeclareOk queueDeclareResult = await _channel.QueueDeclareAsync();
//////////            string queueName = queueDeclareResult.QueueName;
//////////            await _channel.QueueBindAsync(queue: queueName, exchange: "logs", routingKey: string.Empty);

//////////            Console.WriteLine(" [*] Waiting for logs.");

//////////            var consumer = new AsyncEventingBasicConsumer(_channel);
//////////            consumer.ReceivedAsync += (model, ea) =>
//////////            {
//////////                byte[] body = ea.Body.ToArray();
//////////                var message = Encoding.UTF8.GetString(body);
//////////                Console.WriteLine($" [x] {message}");
//////////                return Task.CompletedTask;
//////////            };

//////////            await _channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);

//////////            await Task.CompletedTask;
//////////        }

//////////        public async Task StopAsync(CancellationToken cancellationToken)
//////////        {
//////////            _channel?.CloseAsync();
//////////            _connection?.CloseAsync();
//////////            await Task.CompletedTask;
//////////        }
//////////    }
//////////}
