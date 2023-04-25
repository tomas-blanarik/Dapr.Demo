using Dapr.Basket.Api.Entities.DTO;

namespace Dapr.Basket.Api.Repositories;

public interface IBasketRepository
{
    Task<BasketDTO> GetAsync(string customerId, CancellationToken ct = default);
    Task<BasketDTO> UpdateAsync(BasketDTO basket, CancellationToken ct = default);
    Task DeleteAsync(string customerId, CancellationToken ct = default);
}