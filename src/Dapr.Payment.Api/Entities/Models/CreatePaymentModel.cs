using System.ComponentModel.DataAnnotations;

namespace Dapr.Payment.Api.Entities.Models;

public class CreatePaymentModel
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    public Guid UserId { get; set; }
}
