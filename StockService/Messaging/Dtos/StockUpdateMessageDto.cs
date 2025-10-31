namespace StockService.Messaging.Dtos
{
    // Mensagem enviada de volta para SalesService
    public class StockUpdateMessage
    {
        public int OrderId { get; set; }
        public bool Success { get; set; }
        public string? NewStatus { get; set; } // "Confirmed", "InsufficientStock", "Canceled"
    }
}