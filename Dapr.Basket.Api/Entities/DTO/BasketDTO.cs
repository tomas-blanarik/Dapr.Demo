using Dapr.Basket.Api.Entities.Models;

namespace Dapr.Basket.Api.Entities.DTO;

public class BasketDTO
{
    public string CustomerId { get; set; } = null!;
    public IList<BasketItem>? Items { get; set; }
}