using SalesService.Models;

namespace SalesService.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync(string userId, string userRole);
        Task<Order?> GetOrderByIdAsync(int id, string userId, string userRole);
        Task<Order> CreateOrderAsync(Order order, string userId, string authToken);
        Task<bool> DeleteOrderAsync(int id, string userId, string userRole);
    }
}