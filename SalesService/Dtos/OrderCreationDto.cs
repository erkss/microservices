using System.ComponentModel.DataAnnotations;

namespace SalesService.Dtos 
{
    public class OrderCreationDto
    {
        [Required]
        public string? CustomerName { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}