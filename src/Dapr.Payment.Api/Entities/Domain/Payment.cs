using System.ComponentModel.DataAnnotations;
using Dapr.Core.Entities;
using Dapr.Payment.Api.Entities.DTO;
using Dapr.Payment.Api.Entities.Enums;

namespace Dapr.Payment.Api.Entities.Domain;

public class Payment : IDomainEntity<PaymentDTO>
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid? EntityId { get; set; } = Guid.NewGuid();

    [Required]
    public DateTime? CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    [Required]
    public Guid? OrderId { get; set; }

    [Required]
    public Guid? UserId { get; set; }

    [Required]
    public PaymentStatus Status { get; set; }
    public string? Error { get; set; }

    public PaymentDTO ToDTO() => new()
    {
        CompletedDate = CompletedDate,
        CreatedDate = CreatedDate!.Value,
        Error = Error,
        OrderId = OrderId!.Value,
        UserId = UserId!.Value,
        PaymentId = EntityId!.Value,
        PaymentStatus = Status
    };
}
