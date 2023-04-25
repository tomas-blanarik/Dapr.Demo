using Dapr.Core.Events;
using Dapr.Ordering.Api.Entities.Domain;
using Dapr.Ordering.Api.Entities.Enums;

namespace Dapr.Ordering.Api.Entities.Events;

public class UserCheckoutAcceptedIntegrationEvent : IntegrationEvent
{
    public BasketDTO Basket { get; set; } = null!;

    public Order ToDomain()
        => new()
        {
            UserId = UserId,
            Date = DateTime.UtcNow,
            Status = OrderStatus.Created,
            Amount = Basket.Items.Sum(x => x.Price * x.Quantity),
            Items = Basket.Items.Select(x => x.ToDomain()).ToList()
        };
}

public class BasketDTO
{
    public IList<BasketItemDTO> Items { get; set; } = null!;
}

public class BasketItemDTO
{
    public string ProductName { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public OrderItem ToDomain()
        => new()
        {
            ProductName = ProductName,
            Price = Price,
            Quantity = Quantity   
        };
}