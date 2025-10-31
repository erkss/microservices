using Microsoft.EntityFrameworkCore;
using SalesService.Data;
using SalesService.Models;
using SalesService.Messaging;
using SalesService.Messaging.Dtos;

namespace SalesService.Services
{
    public class OrderService(OrderContext context, RabbitMQPublisher publisher, IHttpClientFactory _clientFactory) : IOrderService
    {
        private readonly OrderContext _context = context;
        private readonly RabbitMQPublisher _publisher = publisher;
        private readonly IHttpClientFactory _clientFactory = _clientFactory;

        // Metodo para a chamada HTTP
        private async Task<bool> ProductExists(int productId, string authToken)
        {
            var client = _clientFactory.CreateClient("StockService");

            if (!string.IsNullOrEmpty(authToken))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
            }

            var response = await client.GetAsync($"/api/products/{productId}");

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync(string userId, string userRole)
        {
            IQueryable<Order> query = _context.Orders!;

            if(!string.Equals(userRole, "admin", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(o => o.UserId == userId);
            }

            return await query.ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id, string userId, string userRole)
        {
            IQueryable<Order> query = _context.Orders!;

            if (!string.Equals(userRole, "admin", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(o => o.UserId == userId);
            }

            return await query.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order> CreateOrderAsync(Order order, string userId, string authToken)
        {
            if(order.Quantity <= 0)
            {
                throw new ArgumentException("Order quantity must be greater than zero.");
            }

            if (!await ProductExists(order.ProductId, authToken))
            {
                throw new ArgumentException($"Product with ID '{order.ProductId}' was not found in stock.");
            }

            order.UserId = userId;

            try
            {
                _context.Orders!.Add(order);
                await _context.SaveChangesAsync();

                var orderDto = new StockUpdateOrderTo
                {
                    Id = order.Id,
                    ProductId = order.ProductId,
                    Quantity = order.Quantity
                };

                _publisher.PublishOrder(orderDto);
            
                return order;
            }
            catch(DbUpdateException ex)
            {
                throw new ApplicationException("An error occurred while creating the order", ex);
            }
        }

        public async Task<bool> DeleteOrderAsync(int id, string userId, string userRole)
        {
            try
            {
                var order = await _context.Orders!.FindAsync(id);
                if (order == null)
                {
                    return false;
                }

                if (!string.Equals(userRole, "admin", StringComparison.OrdinalIgnoreCase) && order.UserId != userId)
                {
                    return false; 
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                return true;
            }
            catch(DbUpdateConcurrencyException)
            {
                return false;
            }
            catch(DbUpdateException)
            {
                return false;
            }
        }
    }
}