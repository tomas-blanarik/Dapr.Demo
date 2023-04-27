using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Dapr.Client;
using Dapr.Core;
using Dapr.Core.Events.Orders;
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

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OrderDTO))]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    [Obsolete("This method is no longer supported, orders are created using events")]
    public IActionResult CreateAsync([FromBody, Bind] CreateOrderModel model,
                                     CancellationToken ct)
    {
        return StatusCode(StatusCodes.Status410Gone);
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
                                              [FromServices] IGenericReadRepository<Order> repository)
    {
        Order entity = await repository.GetWithChildrenAsync(orderId!.Value, "Items");
        OrderDTO dto = entity.ToDTO();
        return Ok(dto);
    }

    [HttpDelete("{orderId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> DeleteAsync([FromRoute, Required] Guid? orderId,
                                                 [FromServices] GenericRepository<Order> repository,
                                                 [FromServices] IGenericWriteRepository<OrderItem> itemRepository,
                                                 [FromServices] DaprClient dapr,
                                                 CancellationToken ct)
    {
        Order order = await repository.GetWithChildrenAsync(orderId!.Value, "Items");
        if (order.Items.Any())
        {
            foreach (var item in order.Items) await itemRepository.DeleteAsync(item.EntityId!.Value, ct);
        }

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
