using System.ComponentModel.DataAnnotations;

namespace Rezqa.Application.Features.User.Dtos;

public record RegisterRequest(
    [Required] [StringLength(50)] string UserName,
    [Required] [EmailAddress] string Email,
    [Required] [Phone] string Phone,
    [Required] [StringLength(100, MinimumLength = 6)] string Password
);

public record LoginRequest(
    [Required] string UserName,
    [Required] string Password
);

public record AuthResponse(
    string AccessToken,
    string UserName,
    string Email,
    IList<string> Roles
);

public record RefreshTokenRequest(
    [Required] string RefreshToken
);

public record VerifyEmailRequest(
    [Required] string Token,
    [Required] string Email
);

public record ResetPasswordRequest(
    [Required] string Token,
    [Required] string Email,
    [Required] [StringLength(100, MinimumLength = 6)] string NewPassword
);

public record TokenResponse(
    string Token,
    DateTime ExpiresAt
); 