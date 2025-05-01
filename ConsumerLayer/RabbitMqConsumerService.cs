using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using ConsumerLayer.Helper;
using System.Net.Mail;
using System.Text;
using ConsumerLayer.DTO;
using Microsoft.Extensions.Configuration;

namespace ConsumerLayer
{
    public class RabbitMqConsumerService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IConfiguration _configuration;

        public RabbitMqConsumerService(IConfiguration configuration)
        {
            _configuration = configuration;
            var factory = new ConnectionFactory()
            {
                HostName = "localhost", // or from config
                DispatchConsumersAsync = false
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "fundoo",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (sender, args) =>
            {
                try
                {
                    var body = args.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var emailMessage = JsonConvert.DeserializeObject<EmailMessage>(message);

                    Console.WriteLine($" [x] Received message: {message}");
                    GmailOtpSender GmailSender = new GmailOtpSender(_configuration);
                    GmailSender.SendEmail(emailMessage.Email, emailMessage.Subject, emailMessage.Body);

                    Console.WriteLine($" [x] Email sent to: {emailMessage.Email}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                }
            };

            _channel.BasicConsume(queue: "fundoo", autoAck: true, consumer: consumer);

            // Keep service alive until stoppingToken is triggered
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
;