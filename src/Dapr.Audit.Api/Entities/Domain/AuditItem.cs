using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Dapr.Audit.Api.Entities.DTO;
using Dapr.Core.Entities;
using Dapr.Core.Events;

namespace Dapr.Audit.Api.Entities.Domain;

public class AuditItem : IDomainEntity<AuditItemDTO>
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid? EntityId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid? EventId { get; set; }

    [Required]
    public Guid? UserId { get; set; }

    [Required]
    public DateTime? EventDate { get; set; }

    [Required]
    public string? EventType { get; set; }
    public string? Metadata { get; set; }

    public AuditItemDTO ToDTO() => new()
    {
        AuditItemId = EntityId!.Value,
        EventDate = EventDate!.Value,
        EventId = EventId!.Value,
        UserId = UserId!.Value,
        EventType = EventType!,
        Metadata = TryDeserializeMetadata()
    };

    private MetadataWrapperDTO? TryDeserializeMetadata()
    {
        if (Metadata is null) return null;
        var wrapper = new MetadataWrapperDTO();
        try
        {
            Assembly assembly = typeof(IntegrationEvent).Assembly;
            var metadata = JsonSerializer.Deserialize(Metadata!, assembly.GetType(EventType!)!);
            wrapper.Value = metadata;
        }
        catch
        {
            // ignore
            return null;
        }

        return wrapper;
    }
}
