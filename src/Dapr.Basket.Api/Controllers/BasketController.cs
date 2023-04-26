using System.ComponentModel.DataAnnotations;
using Dapr.Basket.Api.Entities.DTO;
using Dapr.Basket.Api.Entities.Events;
using Dapr.Basket.Api.Entities.Models;
using Dapr.Basket.Api.Repositories;
using Dapr.Client;
using Dapr.Core;
using Dapr.Core.Exceptions;
using Dapr.Core.Extensions;
using Dapr.Core.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Dapr.Basket.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BasketController : ControllerBase
{
    private readonly IBasketRepository _basketRepository;

    public BasketController(IBasketRepository basketRepository)
    {
        _basketRepository = basketRepository;
    }

    [HttpGet("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BasketDTO))]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> GetBasketAsync([FromRoute, Required] Guid? userId,
                                                    [FromQuery] bool initEmpty = true,
                                                    CancellationToken ct = default)
    {
        var basket = await _basketRepository.GetAsync(userId!.ToString()!, ct);
        if (basket is null && initEmpty)
        {
            basket = new() { CustomerId = userId!.Value.ToString() };
            await _basketRepository.UpdateAsync(basket, ct);
        }

        return basket is null ? throw new DomainEntityNotFoundException(typeof(BasketDTO), userId!.Value) : Ok(basket);
    }

    [HttpPut("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BasketDTO))]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> UpdateBasketAsync([FromRoute, Required] Guid? userId,
                                                       [FromBody, Bind] UpdateBasketModel model,
                                                       CancellationToken ct)
    {
        var basket = await _basketRepository.GetAsync(userId!.Value.ToString()!, ct);
        basket ??= new();

        basket.CustomerId = userId!.ToString()!;
        basket.Items = model.Items;

        var updated = await _basketRepository.UpdateAsync(basket, ct);
        return Ok(updated);
    }

    [HttpPost("{userId:guid}/checkout")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> CheckoutAsync([FromRoute, Required] Guid? userId,
                                                   [FromServices] DaprClient dapr,
                                                   [FromServices] ILogger<BasketController> logger,
                                                   CancellationToken ct,
                                                   [FromQuery] bool useWorkflow = false)
    {
        UserDTO? user = null;
        try
        {
            user = await dapr.InvokeMethodAsync<UserDTO>(HttpMethod.Get,
                                                         DaprConstants.Services.UsersService,
                                                         $"/api/users/{userId}",
                                                         ct);

            if (user is null)
            {
                throw new InvalidOperationException("User is null");
            }

            logger.LogInformation("Successfully retrieved user: {UserId}", user.UserId);
        }
        catch (InvocationException ex)
        {
            if (ex.TryParseErrorResponse(out var response))
            {
                logger.LogDaprInvocationError(ex, response);
                return this.GetResultFromDaprErrorResponse(response!);
            }

            logger.LogDaprInvocationError(ex);
            throw new UnexpectedErrorException("Cannot read error response from dapr result");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ExceptionMessage}", ex.Message);
            throw new UnexpectedErrorException(ex.Message);
        }

        var basket = await _basketRepository.GetAsync(user.UserId.ToString(), ct)
            ?? throw new DomainEntityNotFoundException(typeof(BasketDTO), user.UserId);
        var @event = new UserCheckoutAcceptedIntegrationEvent
        {
            Basket = basket,
            UserId = user.UserId,
            UseWorkflow = useWorkflow
        };

        await dapr.PublishEventAsync(DaprConstants.Components.PubSub, nameof(UserCheckoutAcceptedIntegrationEvent), @event, ct);
        return Accepted();
    }

    [HttpDelete("{userId:guid}")]
    [SwaggerOperation(Tags = new[] { "Developer Tools" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> DeleteAllAsync([FromRoute, Required] Guid? userId,
                                                    CancellationToken ct)
    {
        await _basketRepository.DeleteAsync(userId!.ToString()!, ct);
        return NoContent();
    }
}