namespace SalesService.Messaging.Dtos
{
    public class StockUpdateOrderTo
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}