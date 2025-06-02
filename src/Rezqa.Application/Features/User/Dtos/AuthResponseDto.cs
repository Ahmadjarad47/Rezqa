namespace Rezqa.Application.Features.User.Dtos;

public record AuthResponseDto(
    string AccessToken,
    string UserName,
    string Email,
    IList<string> Roles
); 