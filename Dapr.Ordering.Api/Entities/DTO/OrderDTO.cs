using System.Text.Json.Serialization;
using Dapr.Ordering.Api.Entities.Enums;

namespace Dapr.Ordering.Api.Entities.DTO;

public class OrderDTO
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid? PaymentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime? OrderDate { get; set; }
    public OrderStatus OrderStatus { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? CompletedDate { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<OrderItemDTO>? Items { get; set; }
}
