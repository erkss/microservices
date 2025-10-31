using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SalesService.Data;
using SalesService.Messaging.Dtos;

namespace SalesService.Messaging
{
    public class RabbitMQConsumer
    {
        private readonly string _hostname = "localhost";
        private readonly string _queueName = "stock_updates";
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

            // Mantemos a conexão e o canal abertos enquanto o serviço estiver rodando
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: _queueName,
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
                    var update = JsonSerializer.Deserialize<StockUpdateMessageDto>(message);

                    if (update != null)
                    {
                        // Cria um escopo para pegar o DbContext corretamente (scoped)
                        using var scope = _serviceProvider.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<OrderContext>();

                        var order = await context.Orders!.FindAsync(update.OrderId);

                        if (order != null && update.NewStatus != null)
                        {
                            order.Status = update.NewStatus;
                            await context.SaveChangesAsync();
                            Console.WriteLine($"Pedido {order.Id} atualizado para {order.Status}");
                        }
                        else
                        {
                            Console.WriteLine($"Pedido {update.OrderId} não encontrado.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar mensagem do RabbitMQ: {ex.Message}");
                }
            };

            _channel.BasicConsume(queue: _queueName,
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine("RabbitMQ Consumer (SalesService) iniciado. Aguardando atualizações de estoque...");
        }
    }
}