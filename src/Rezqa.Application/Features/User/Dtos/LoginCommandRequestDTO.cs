using System.ComponentModel.DataAnnotations;

namespace Rezqa.Application.Features.User.Dtos;

public record LoginCommandRequestDTO(
    [Required] string UserName,
    [Required] string Password
); 