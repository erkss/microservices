using System.Text;
using System.Text.Json;
using StockService.Messaging.Dtos;
using StockService.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace StockService.Messaging
{
    public class RabbitMQConsumer
    {
        private readonly string _hostname = "localhost";
        private readonly string _queueName = "orders_queue";
        private readonly string _updateQueueName = "stock_updates";
        private readonly IServiceProvider _serviceProvider;

        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMQConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Start()
        {
            var factory = new ConnectionFactory() { HostName = _hostname };
            
            // Mantém a conexão e o canal abertos enquanto o serviço roda
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declara as filas
            _channel.QueueDeclare(queue: _queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            _channel.QueueDeclare(queue: _updateQueueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    var order = JsonSerializer.Deserialize<StockUpdateOrderDto>(message);

                    if (order != null)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<StockContext>();

                        var product = await context.Products!.FindAsync(order.ProductId);

                        string responseStatus;
                        
                        if (product != null)
                        {

                            if (product.Quantity >= order.Quantity)
                            {
                                product.Quantity -= order.Quantity;
                                await context.SaveChangesAsync();
                                responseStatus = "Confirmed";

                                Console.WriteLine($"Pedido {order.Id} processado. Estoque atualizado.");
                            }
                            else
                            {
                                responseStatus = "InsufficientStock";
                                Console.WriteLine($"Pedido {order.Id} cancelado: estoque insuficiente.");
                            }
                        }
                        else
                        {
                            responseStatus = "Canceled";
                            Console.WriteLine($"Pedido {order.Id} cancelado: produto não encontrado.");
                        }

                        // Envia atualização de volta ao SalesService
                        var updateMessage = new StockUpdateMessage
                        {
                            OrderId = order.Id,
                            NewStatus = responseStatus
                        };
                        var updateBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(updateMessage));

                        _channel.BasicPublish(exchange: "",
                                             routingKey: _updateQueueName,
                                             basicProperties: null,
                                             body: updateBody);

                        Console.WriteLine($"Pedido {order.Id} processado. Estoque atualizado: {responseStatus}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar pedido: {ex.Message}");
                }
            };

            _channel.BasicConsume(queue: _queueName,
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine("RabbitMQ Consumer (StockService) iniciado. Aguardando pedidos...");
        }
    }
}