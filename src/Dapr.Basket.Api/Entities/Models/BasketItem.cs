namespace Dapr.Basket.Api.Entities.Models;

public class BasketItem
{
    public string ProductName { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}