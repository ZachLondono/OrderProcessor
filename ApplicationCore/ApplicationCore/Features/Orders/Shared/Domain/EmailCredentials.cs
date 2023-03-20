using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Features.Orders.Shared.Domain;

internal class EmailCredentials {

    [Required]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

}
