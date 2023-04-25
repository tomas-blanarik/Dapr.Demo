namespace Dapr.Basket.Api.Entities.Models;

public class UpdateBasketModel
{
    public IList<BasketItem> Items { get; set; } = new List<BasketItem>();
}
