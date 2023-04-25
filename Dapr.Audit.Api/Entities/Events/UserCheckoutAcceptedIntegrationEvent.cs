using Dapr.Core.Events;

namespace Dapr.Audit.Api.Entities.Events;

public class UserCheckoutAcceptedIntegrationEvent : IntegrationEvent
{
    public BasketDTO Basket { get; set; } = null!;
}

public class BasketDTO
{
    public IList<BasketItem> Items { get; set; } = null!;
}

public class BasketItem
{
    public string ProductName { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}