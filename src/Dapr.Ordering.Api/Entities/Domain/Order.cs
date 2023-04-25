using System.ComponentModel.DataAnnotations;
using Dapr.Core.Entities;
using Dapr.Ordering.Api.Entities.DTO;
using Dapr.Ordering.Api.Entities.Enums;

namespace Dapr.Ordering.Api.Entities.Domain;

public class Order : IDomainEntity<OrderDTO>
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid? EntityId { get; set; } = Guid.NewGuid();

    [Required]
    [Range(0, int.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public DateTime? Date { get; set; }
    public DateTime? CompletedDate { get; set; }
    public OrderStatus Status { get; set; }

    [Required]
    public Guid? UserId { get; set; }
    public Guid? PaymentId { get; set; }
    public string? Error { get; set; }
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public OrderDTO ToDTO() => new()
    {
        OrderId = EntityId!.Value,
        Amount = Amount,
        OrderDate = Date,
        OrderStatus = Status,
        UserId = UserId!.Value,
        PaymentId = PaymentId,
        CompletedDate = CompletedDate,
        Error = Error,
        Items = Items.Count > 0 ? Items.Select(x => x.ToDTO()).ToList() : null
    };
}
