using System.Text.Json.Serialization;
using Dapr.Payment.Api.Entities.Enums;

namespace Dapr.Payment.Api.Entities.DTO;

public class PaymentDTO
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedDate { get; set; }
    public PaymentStatusEnum PaymentStatus { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? CompletedDate { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }
}
