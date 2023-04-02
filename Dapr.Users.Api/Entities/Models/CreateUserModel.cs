using System.ComponentModel.DataAnnotations;
using Dapr.Core.Entities;
using Dapr.Users.Api.Entities.Domain;

namespace Dapr.Users.Api.Entities.Models;

public class CreateUserModel : IModel<User>
{
    [Required]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }

    public User ToDomain() => new()
    {
        FullName = FullName,
        Email = Email,
        Phone = Phone
    };
}
