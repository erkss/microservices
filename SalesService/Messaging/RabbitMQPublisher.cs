using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace SalesService.Messaging
{
    public class RabbitMQPublisher
    {
        private readonly string _hostname = "localhost";
        private readonly string _queueName = "orders_queue";

        public void PublishOrder(object order)
        {
            var factory = new ConnectionFactory() { HostName = _hostname };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var message = JsonSerializer.Serialize(order);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                                 routingKey: _queueName,
                                 basicProperties: null,
                                 body: body);
        }
    }
}