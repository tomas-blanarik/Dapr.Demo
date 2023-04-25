using System.ComponentModel.DataAnnotations;
using Dapr.Core.Entities;
using Dapr.Ordering.Api.Entities.DTO;

namespace Dapr.Ordering.Api.Entities.Domain;

public class OrderItem : IDomainEntity<OrderItemDTO>
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid? EntityId { get; set; } = Guid.NewGuid();

    [Required]
    public string ProductName { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public OrderItemDTO ToDTO()
        => new()
        {
            OrderItemId = EntityId!.Value,
            ProductName = ProductName,
            Price = Price,
            Quantity = Quantity
        };
}