using System.ComponentModel.DataAnnotations;
using Dapr.Client;
using Dapr.Core;
using Dapr.Core.Events.Users;
using Dapr.Core.Paging;
using Dapr.Core.Repositories;
using Dapr.Core.Repositories.Generic;
using Dapr.Core.Responses;
using Dapr.Users.Api.Entities.Domain;
using Dapr.Users.Api.Entities.DTO;
using Dapr.Users.Api.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Dapr.Users.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    public const string GetUserRoute = "users#get";

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDTO))]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> CreateAsync([FromBody, Bind] CreateUserModel model,
                                                 [FromServices] IGenericWriteRepository<User> repository,
                                                 [FromServices] DaprClient dapr,
                                                 CancellationToken ct)
    {
        User entity = await repository.CreateAsync(model.ToDomain, ct);
        UserDTO dto = entity.ToDTO();
        var createdEvent = new UserCreatedIntegrationEvent
        {
            CreatedDate = DateTime.UtcNow,
            UserId = dto.UserId
        };

        await dapr.PublishEventAsync(DaprConstants.Components.PubSub,
                                     nameof(UserCreatedIntegrationEvent),
                                     createdEvent,
                                     ct);

        return CreatedAtRoute(GetUserRoute, new { userId = dto.UserId }, dto);
    }

    [HttpGet("{userId:guid}", Name = GetUserRoute)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO))]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> GetAsync([FromRoute, Required] Guid? userId,
                                              [FromServices] IGenericReadRepository<User> repository,
                                              CancellationToken ct)
    {
        User entity = await repository.GetAsync(userId!.Value, ct);
        UserDTO dto = entity.ToDTO();
        return Ok(dto);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<UserDTO>))]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> ListAsync([FromQuery] PagingRequest request,
                                              [FromServices] IGenericReadRepository<User> repository,
                                              CancellationToken ct)
    {
        PagedResult<User> result = await repository.GetAllPagedAsync(request, ct: ct);
        PagedResult<UserDTO> dto = result.ConvertToDTO(x => x.ToDTO());
        return Ok(dto);
    }

    [HttpDelete("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> DeleteAsync([FromRoute, Required] Guid? userId,
                                                 [FromServices] GenericRepository<User> repository,
                                                 [FromServices] DaprClient dapr,
                                                 CancellationToken ct)
    {
        User user = await repository.GetAsync(userId!.Value, ct);
        await repository.DeleteAsync(userId!.Value, ct);
        var deletedEvent = new UserDeletedIntegrationEvent
        {
            DeletedDate = DateTime.UtcNow,
            UserId = user.EntityId!.Value
        };

        await dapr.PublishEventAsync(DaprConstants.Components.PubSub,
                                     nameof(UserDeletedIntegrationEvent),
                                     deletedEvent,
                                     ct);
        return NoContent();
    }

    [HttpDelete]
    [SwaggerOperation(Tags = new[] { "Developer Tools" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> DeleteAllAsync([FromServices] IGenericWriteRepository<User> repository,
                                                    CancellationToken ct)
    {
        await repository.DeleteAllAsync(ct);
        return NoContent();
    }
}
