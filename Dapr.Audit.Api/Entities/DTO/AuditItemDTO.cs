using System.Text.Json.Serialization;

namespace Dapr.Audit.Api.Entities.DTO;

public class AuditItemDTO
{
    public Guid AuditItemId { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public DateTime EventDate { get; set; }
    public string EventType { get; set; } = null!;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public MetadataWrapperDTO? Metadata { get; set; }
}
