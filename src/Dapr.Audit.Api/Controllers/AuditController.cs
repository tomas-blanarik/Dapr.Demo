using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Dapr.Audit.Api.Entities.Domain;
using Dapr.Audit.Api.Entities.DTO;
using Dapr.Audit.Api.Queries;
using Dapr.Core.Extensions;
using Dapr.Core.Paging;
using Dapr.Core.Repositories;
using Dapr.Core.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Dapr.Audit.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class AuditController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<AuditItemDTO>))]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> ListAsync([FromQuery] ListAuditItemsQuery request,
                                               [FromServices] IGenericReadRepository<AuditItem> repository,
                                               CancellationToken ct)
    {
        Expression<Func<AuditItem, bool>> searchExpression = _ => true;
        if (request.UserId is not null)
        {
            searchExpression = x => x.UserId == request.UserId;
        }

        if (request.CreatedAt is not null)
        {
            searchExpression = searchExpression.AddExpr(x => x.EventDate == request.CreatedAt);
        }

        if (request.EventType is not null)
        {
            searchExpression = searchExpression.AddExpr(x => x.EventType == request.EventType);
        }

        Expression<Func<AuditItem, DateTime?>>? orderByExpression = null;
        if (request.OrderByDate == true)
        {
            orderByExpression = x => x.EventDate;
        }

        PagedResult<AuditItem> result = await repository.GetAllPagedAsync(request, searchExpression, orderByExpression, ct);
        PagedResult<AuditItemDTO> resultDTO = result.ConvertToDTO(x => x.ToDTO());
        return Ok(resultDTO);
    }

    [HttpDelete("{auditItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> DeleteAsync([FromRoute, Required] Guid? auditItemId,
                                                 [FromServices] IGenericWriteRepository<AuditItem> repository,
                                                 CancellationToken ct)
    {
        await repository.DeleteAsync(auditItemId!.Value, ct);
        return Ok();
    }

    [HttpDelete]
    [SwaggerOperation(Tags = new[] { "Developer Tools" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> DeleteAsync([FromServices] IGenericWriteRepository<AuditItem> repository,
                                                 CancellationToken ct)
    {
        await repository.DeleteAllAsync(ct);
        return NoContent();
    }
}
