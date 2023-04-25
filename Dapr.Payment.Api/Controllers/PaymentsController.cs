using System.ComponentModel.DataAnnotations;
using Dapr.Client;
using Dapr.Core;
using Dapr.Core.Events.Payments;
using Dapr.Core.Exceptions;
using Dapr.Core.Repositories;
using Dapr.Core.Repositories.Generic;
using Dapr.Core.Responses;
using Dapr.Payment.Api.Entities.DTO;
using Dapr.Payment.Api.Entities.Enums;
using Dapr.Payment.Api.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Domain = Dapr.Payment.Api.Entities.Domain;

namespace Dapr.Payment.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    [HttpPost("{paymentId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentDTO))]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> CompleteAsync([FromRoute, Required] Guid? paymentId,
                                                   [FromServices] GenericRepository<Domain.Payment> repository,
                                                   [FromServices] DaprClient dapr,
                                                   CancellationToken ct)
    {
        Domain.Payment entity = await repository.UpdateAsync(paymentId!.Value, toUpdate =>
        {
            if (toUpdate.Status == PaymentStatus.Completed || toUpdate.Status == PaymentStatus.Failed)
            {
                throw new ConflictException(string.Format("Payment is already: {0}", toUpdate.Status.ToString()));
            }

            toUpdate.Status = PaymentStatus.Completed;
            toUpdate.CompletedDate = DateTime.UtcNow;
        }, ct);

        PaymentDTO dto = entity.ToDTO();
        var completedEvent = new PaymentCompletedIntegrationEvent
        {
            CompletedDate = dto.CompletedDate!.Value,
            PaymentId = dto.PaymentId,
            UserId = dto.UserId
        };

        await dapr.PublishEventAsync(DaprConstants.Components.PubSub,
                                     nameof(PaymentCompletedIntegrationEvent),
                                     completedEvent,
                                     ct);

        return Ok(dto);
    }

    [HttpPost("{paymentId:guid}/failed")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentDTO))]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> FailedAsync([FromRoute, Required] Guid? paymentId,
                                                 [FromBody, Bind] PaymentFailedModel model,
                                                 [FromServices] GenericRepository<Domain.Payment> repository,
                                                 [FromServices] DaprClient dapr,
                                                 CancellationToken ct)
    {
        Domain.Payment entity = await repository.UpdateAsync(paymentId!.Value, toUpdate =>
        {
            if (toUpdate.Status == PaymentStatus.Completed || toUpdate.Status == PaymentStatus.Failed)
            {
                throw new ConflictException(string.Format("Payment is already: {0}", toUpdate.Status.ToString()));
            }

            toUpdate.Status = PaymentStatus.Failed;
            toUpdate.CompletedDate = null;
            toUpdate.Error = model.Reason;
        }, ct);

        PaymentDTO dto = entity.ToDTO();
        var failedEvent = new PaymentFailedIntegrationEvent
        {
            FailedDate = DateTime.UtcNow,
            PaymentId = dto.PaymentId,
            Reason = model.Reason,
            UserId = dto.UserId
        };

        await dapr.PublishEventAsync(DaprConstants.Components.PubSub,
                                     nameof(PaymentFailedIntegrationEvent),
                                     failedEvent,
                                     ct);

        return Ok(dto);
    }

    [HttpGet("{paymentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentDTO))]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> GetAsync([FromRoute, Required] Guid? paymentId,
                                              [FromServices] IGenericReadRepository<Domain.Payment> repository,
                                              CancellationToken ct)
    {
        Domain.Payment entity = await repository.GetAsync(paymentId!.Value, ct);
        PaymentDTO dto = entity.ToDTO();
        return Ok(dto);
    }

    [HttpDelete("{paymentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> DeleteAsync([FromRoute, Required] Guid? paymentId,
                                                 [FromServices] GenericRepository<Domain.Payment> repository,
                                                 [FromServices] DaprClient dapr,
                                                 CancellationToken ct)
    {
        Domain.Payment entity = await repository.GetAsync(paymentId!.Value, ct);
        await repository.DeleteAsync(paymentId!.Value, ct);
        var deletedEvent = new PaymentDeletedIntegrationEvent
        {
            DeletedDate = DateTime.UtcNow,
            PaymentId = paymentId!.Value,
            UserId = entity.UserId!.Value
        };

        await dapr.PublishEventAsync(DaprConstants.Components.PubSub,
                                     nameof(PaymentDeletedIntegrationEvent),
                                     deletedEvent,
                                     ct);

        return NoContent();
    }

    [HttpDelete]
    [SwaggerOperation(Tags = new[] { "Developer Tools" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesDefaultResponseType(typeof(ErrorResponse))]
    public async Task<IActionResult> DeleteAllAsync([FromServices] IGenericWriteRepository<Domain.Payment> repository,
                                                    CancellationToken ct)
    {
        await repository.DeleteAllAsync(ct);
        return NoContent();
    }
}
