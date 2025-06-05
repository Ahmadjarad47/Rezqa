using System.ComponentModel.DataAnnotations;

namespace Rezqa.Application.Features.User.Dtos;

public record RegisterCommandRequestDTO(
    [Required][StringLength(50)] string UserName,
    [Required][EmailAddress] string Email,
    [Required][Phone] string? PhoneNumber,
    [Required][StringLength(100, MinimumLength = 6)] string Password
);