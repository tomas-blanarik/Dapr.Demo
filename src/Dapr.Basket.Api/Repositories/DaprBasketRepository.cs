using Dapr.Basket.Api.Entities.DTO;
using Dapr.Client;
using Dapr.Core;

namespace Dapr.Basket.Api.Repositories;

public class DaprBasketRepository(DaprClient daprClient) : IBasketRepository
{
    private readonly DaprClient _daprClient = daprClient;

    public Task DeleteAsync(string customerId, CancellationToken ct = default)
        => _daprClient.DeleteStateAsync(DaprConstants.Components.StateStore, customerId, cancellationToken: ct);

    public Task<BasketDTO> GetAsync(string customerId, CancellationToken ct = default)
        => _daprClient.GetStateAsync<BasketDTO>(DaprConstants.Components.StateStore, customerId, cancellationToken: ct);

    public async Task<BasketDTO> UpdateAsync(BasketDTO basket, CancellationToken ct = default)
    {
        var state = await _daprClient.GetStateEntryAsync<BasketDTO>(DaprConstants.Components.StateStore, basket.CustomerId, cancellationToken: ct);
        state.Value = basket;

        await state.SaveAsync(cancellationToken: ct);
        return await GetAsync(basket.CustomerId, ct);
    }
}