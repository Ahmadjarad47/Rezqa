namespace Rezqa.Application.Features.User.Dtos;
public record AuthResponseDto(
    bool IsSuccess,
    string Message,
    string? AccessToken = null,
    string? UserName = null,
    string? Email = null,
    IList<string>? Roles = null
);