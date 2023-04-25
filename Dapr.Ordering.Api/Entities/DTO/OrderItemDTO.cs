namespace Dapr.Ordering.Api.Entities.DTO;

public class OrderItemDTO
{
    public Guid OrderItemId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}