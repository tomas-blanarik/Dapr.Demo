using Dapr.Audit.Api.Entities.Domain;
using Dapr.Core;
using Dapr.Core.Events;
using Dapr.Core.Events.Orders;
using Dapr.Core.Events.Payments;
using Dapr.Core.Events.Users;
using Dapr.Core.Extensions;
using Dapr.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Dapr.Audit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly ILogger<EventsController> _logger;

    public EventsController(ILogger<EventsController> logger)
    {
        _logger = logger;
    }

    #region Orders

    [HttpPost(nameof(OrderCreatedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(OrderCreatedIntegrationEvent))]
    public async Task HandleAsync(OrderCreatedIntegrationEvent @event,
                                  [FromServices] IGenericWriteRepository<AuditItem> repository,
                                  CancellationToken ct) => await ProcessEventAsync(@event, repository, ct);

    [HttpPost(nameof(OrderDeletedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(OrderDeletedIntegrationEvent))]
    public async Task HandleAsync(OrderDeletedIntegrationEvent @event,
                                  [FromServices] IGenericWriteRepository<AuditItem> repository,
                                  CancellationToken ct) => await ProcessEventAsync(@event, repository, ct);

    [HttpPost(nameof(OrderCompletedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(OrderCompletedIntegrationEvent))]
    public async Task HandleAsync(OrderCompletedIntegrationEvent @event,
                                  [FromServices] IGenericWriteRepository<AuditItem> repository,
                                  CancellationToken ct) => await ProcessEventAsync(@event, repository, ct);

    [HttpPost(nameof(OrderFailedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(OrderFailedIntegrationEvent))]
    public async Task HandleAsync(OrderFailedIntegrationEvent @event,
                                  [FromServices] IGenericWriteRepository<AuditItem> repository,
                                  CancellationToken ct) => await ProcessEventAsync(@event, repository, ct);

    #endregion

    #region Users

    [HttpPost(nameof(UserCreatedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(UserCreatedIntegrationEvent))]
    public async Task HandleAsync(UserCreatedIntegrationEvent @event,
                                  [FromServices] IGenericWriteRepository<AuditItem> repository,
                                  CancellationToken ct) => await ProcessEventAsync(@event, repository, ct);

    [HttpPost(nameof(UserDeletedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(UserDeletedIntegrationEvent))]
    public async Task HandleAsync(UserDeletedIntegrationEvent @event,
                                  [FromServices] IGenericWriteRepository<AuditItem> repository,
                                  CancellationToken ct) => await ProcessEventAsync(@event, repository, ct);

    #endregion

    #region Payments

    [HttpPost(nameof(PaymentCreatedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(PaymentCreatedIntegrationEvent))]
    public async Task HandleAsync(PaymentCreatedIntegrationEvent @event,
                                  [FromServices] IGenericWriteRepository<AuditItem> repository,
                                  CancellationToken ct) => await ProcessEventAsync(@event, repository, ct);

    [HttpPost(nameof(PaymentDeletedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(PaymentDeletedIntegrationEvent))]
    public async Task HandleAsync(PaymentDeletedIntegrationEvent @event,
                                  [FromServices] IGenericWriteRepository<AuditItem> repository,
                                  CancellationToken ct) => await ProcessEventAsync(@event, repository, ct);

    [HttpPost(nameof(PaymentCompletedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(PaymentCompletedIntegrationEvent))]
    public async Task HandleAsync(PaymentCompletedIntegrationEvent @event,
                                  [FromServices] IGenericWriteRepository<AuditItem> repository,
                                  CancellationToken ct) => await ProcessEventAsync(@event, repository, ct);

    [HttpPost(nameof(PaymentFailedIntegrationEvent))]
    [Topic(DaprConstants.Components.PubSub, nameof(PaymentFailedIntegrationEvent))]
    public async Task HandleAsync(PaymentFailedIntegrationEvent @event,
                                  [FromServices] IGenericWriteRepository<AuditItem> repository,
                                  CancellationToken ct) => await ProcessEventAsync(@event, repository, ct);

    #endregion

    private async Task ProcessEventAsync<TEvent>(TEvent @event,
                                                 IGenericWriteRepository<AuditItem> repository,
                                                 CancellationToken ct) where TEvent : IntegrationEvent
    {
        AuditItem entity = await repository.CreateAsync(() =>
        {
            return new()
            {
                EventDate = @event.EventDate,
                EventId = @event.EventId,
                UserId = @event.UserId,
                EventType = typeof(TEvent).FullName,
                Metadata = @event.TrySerializeToJson()
            };
        }, ct);

        LogEventCreation(entity);
    }

    private void LogEventCreation(AuditItem entity)
    {
        _logger.LogInformation("Successfully stored new audit item '{AuditId}' from date '{EventDate}'", entity.EntityId, entity.EventDate);
        _logger.LogInformation("-- Additional information: EventType='{EventType}', EventId='{EventId}'", entity.EventType, entity.EventId);
        _logger.LogInformation("-- Metadata: {EventMetadata}", entity.Metadata ?? "N/A");
    }
}