using System.ComponentModel.DataAnnotations;

namespace Rezqa.Application.Features.User.Dtos;

public record LoginCommandRequestDTO(
    [Required] string Email,
    [Required] string Password
); 