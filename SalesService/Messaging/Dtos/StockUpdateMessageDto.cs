namespace SalesService.Messaging.Dtos
{
    public class StockUpdateMessageDto
    {
        public int OrderId { get; set; }
        public string? NewStatus { get; set; } // "Confirmed", "InsufficientStock", "Canceled"
    }
}