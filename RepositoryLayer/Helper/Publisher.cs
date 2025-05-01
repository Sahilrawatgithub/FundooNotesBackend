using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace RepositoryLayer.Helper
{
    public class Publisher
    {
        public void PublishToQueue(string queueName, string message)
        {
            try
            {
                Console.WriteLine("publisher is running");
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    UserName = "guest",
                    Password = "guest"
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);

                Console.WriteLine($" [x] Sent message to in consumer {queueName}: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing to RabbitMQ: {ex.Message}");
            }
        }
    }

}
