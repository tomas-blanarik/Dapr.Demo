using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Dapr.Client;
using Dapr.Core;
using Dapr.Core.Events.Orders;
using Dapr.Core.Exceptions;
using Dapr.Core.Extensions;
using Dapr.Core.Paging;
using Dapr.Core.Repositories;
using Dapr.Core.Repositories.Generic;
using Dapr.Core.Responses;
using Dapr.Ordering.Api.Entities.Domain;
using Dapr.Ordering.Api.Entities.DTO;
using Dapr.Ordering.Api.Entities.Models;
using Dapr.Ordering.Api.Queries;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Dapr.Ordering.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    public const string GetOrderRoute = "orders#get";

    /*
    
    4. payment record is created after order creation
    5. payment is updated successfully - finish order
    6. payment is failed - order failed
    7. 
    */

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OrderDTO))]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> CreateAsync([FromBody, Bind] CreateOrderModel model,
                                                 [FromServices] IGenericWriteRepository<Order> repository,
                                                 [FromServices] DaprClient dapr,
                                                 [FromServices] ILogger<OrdersController> logger,
                                                 CancellationToken ct)
    {
        UserDTO? user = null;
        try
        {
            user = await dapr.InvokeMethodAsync<UserDTO>(HttpMethod.Get,
                                                         DaprConstants.Services.UsersService,
                                                         $"/api/users/{model.UserId}",
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

        Order entity = await repository.CreateAsync(model.ToDomain, ct);
        OrderDTO dto = entity.ToDTO();

        // publish event
        var createdEvent = new OrderCreatedIntegrationEvent
        {
            Amount = dto.Amount,
            CreatedDate = dto.OrderDate!.Value,
            OrderId = dto.OrderId,
            UserId = user!.UserId
        };

        await dapr.PublishEventAsync(DaprConstants.Components.PubSub,
                                     nameof(OrderCreatedIntegrationEvent),
                                     createdEvent,
                                     ct);

        return CreatedAtRoute(GetOrderRoute, new { orderId = dto.OrderId }, dto);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<OrderDTO>))]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> ListAsync([FromQuery] ListOrdersQuery request,
                                               [FromServices] IGenericReadRepository<Order> repository,
                                               CancellationToken ct)
    {
        Expression<Func<Order, bool>> searchExpression = _ => true;
        if (request.UserId is not null)
        {
            searchExpression = x => x.UserId == request.UserId;
        }

        PagedResult<Order> result = await repository.GetAllPagedAsync(request, searchExpression, ct);
        PagedResult<OrderDTO> dto = result.ConvertToDTO(x => x.ToDTO());
        return Ok(dto);
    }

    [HttpGet("{orderId:guid}", Name = GetOrderRoute)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDTO))]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> GetAsync([FromRoute, Required] Guid? orderId,
                                              [FromServices] IGenericReadRepository<Order> repository,
                                              CancellationToken ct)
    {
        Order entity = await repository.GetAsync(orderId!.Value, ct);
        OrderDTO dto = entity.ToDTO();
        return Ok(dto);
    }

    [HttpDelete("{orderId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> DeleteAsync([FromRoute, Required] Guid? orderId,
                                                 [FromServices] GenericRepository<Order> repository,
                                                 [FromServices] DaprClient dapr,
                                                 CancellationToken ct)
    {
        Order order = await repository.GetAsync(orderId!.Value, ct);
        await repository.DeleteAsync(orderId!.Value, ct);
        var deletedEvent = new OrderDeletedIntegrationEvent
        {
            DeletedDate = DateTime.UtcNow,
            OrderId = orderId!.Value,
            UserId = order.UserId!.Value
        };

        await dapr.PublishEventAsync(DaprConstants.Components.PubSub,
                                     nameof(OrderDeletedIntegrationEvent),
                                     deletedEvent,
                                     ct);

        return NoContent();
    }

    [HttpDelete]
    [SwaggerOperation(Tags = new[] { "Developer Tools" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> DeleteAllAsync([FromServices] IGenericWriteRepository<Order> repository,
                                                    CancellationToken ct)
    {
        await repository.DeleteAllAsync(ct);
        return NoContent();
    }
}
