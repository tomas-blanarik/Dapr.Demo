using System.ComponentModel.DataAnnotations;

namespace Dapr.Payment.Api.Entities.Models;

public class PaymentFailedModel
{
    [Required]
    public string Reason { get; set; } = null!;
}
