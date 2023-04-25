using System.ComponentModel.DataAnnotations;
using Dapr.Core.Entities;
using Dapr.Ordering.Api.Entities.Domain;
using Dapr.Ordering.Api.Entities.Enums;

namespace Dapr.Ordering.Api.Entities.Models;

public class CreateOrderModel : IModel<Order>
{
    [Required]
    public Guid? UserId { get; set; }

    [Range(0, int.MaxValue)]
    public decimal Amount { get; set; }

    public Order ToDomain() => new()
    {
        UserId = UserId,
        Amount = Amount,
        Date = DateTime.UtcNow,
        Status = OrderStatus.Created
    };
}
