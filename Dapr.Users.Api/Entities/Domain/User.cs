using System.ComponentModel.DataAnnotations;
using Dapr.Core.Entities;
using Dapr.Users.Api.Entities.DTO;

namespace Dapr.Users.Api.Entities.Domain;

public class User : IDomainEntity<UserDTO>
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid? EntityId { get; set; } = Guid.NewGuid();

    [Required]
    public string? FullName { get; set; }

    [Required]
    public string? Email { get; set; }
    public string? Phone { get; set; }

    public UserDTO ToDTO() => new()
    {
        UserId = EntityId!.Value,
        FullName = FullName!,
        Email = Email!,
        Phone = Phone
    };
}
