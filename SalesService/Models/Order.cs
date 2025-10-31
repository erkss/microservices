using System.ComponentModel.DataAnnotations;

namespace SalesService.Models
{
    public class Order
    {
        public int Id { get; set; } 

        [Required(ErrorMessage = "User ID is required.")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required.")]
        public string CustomerName { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Range(1, int.MaxValue, ErrorMessage = "ProductId is required and must be greater than zero.")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity is required and must be greater than zero.")]
        public int Quantity { get; set; }

        public string Status { get; set; } = "Pending";
    }
}