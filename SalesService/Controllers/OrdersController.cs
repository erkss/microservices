using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesService.Services;
using SalesService.Models;
using SalesService.Dtos;
using System.Security.Claims;

namespace SalesService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController(IOrderService orderService) : ControllerBase
    {
        private readonly IOrderService _orderService = orderService;

        // Extrair Claims 
        private (string UserId, string UserRole) GetUserClaims()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
            return (userId, userRole);
        }

        // GET: api/orders
        [HttpGet]
        [Authorize(Roles = "user, admin")] 
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var (userId, userRole) = GetUserClaims();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Token does not contain a valid user identifier.");

            var orders = await _orderService.GetAllOrdersAsync(userId, userRole);
            return Ok(orders);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "user, admin")] 
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var (userId, userRole) = GetUserClaims();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Token does not contain a valid user identifier.");

            var order = await _orderService.GetOrderByIdAsync(id, userId, userRole);
            if (order == null)
                return NotFound();

            return Ok(order);
        }

        // POST: api/orders
        [HttpPost]
        [Authorize(Roles = "user, admin")] 
        public async Task<ActionResult<Order>> CreateOrder(OrderCreationDto orderDto)
        {
            var (userId,_) = GetUserClaims();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Token does not contain a valid user identifier.");

            var authToken = GetAuthToken();
            
            try 
            {
                if (string.IsNullOrWhiteSpace(orderDto.CustomerName))
                {
                    return BadRequest("CustomerName cannot be null or empty.");
                }

                var newOrder = new Order 
                {
                    CustomerName = orderDto.CustomerName,
                    ProductId = orderDto.ProductId,
                    Quantity = orderDto.Quantity
                };

                var createdOrder = await _orderService.CreateOrderAsync(newOrder, userId, authToken); 

                if(createdOrder == null)
                {
                    return StatusCode(500, "An internal error occurred while creating the order. Please try again.");
                }

                return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, createdOrder);
            }
            catch(ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(Exception)
            {
                return StatusCode(500, "An unexpected error has occurred.");
            }
        }
        
        // DELETE: api/orders/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var (userId, userRole) = GetUserClaims();
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _orderService.DeleteOrderAsync(id, userId, userRole);

            if (!result)
            {
                return NotFound();
            }
                
            return NoContent();
        }

        private string GetAuthToken()
        {
            if (Request.Headers.TryGetValue("Authorization", out var headerValue))
            {
                return headerValue.ToString().Replace("Bearer ", "");
            }

            return string.Empty;
        }
    } 
}