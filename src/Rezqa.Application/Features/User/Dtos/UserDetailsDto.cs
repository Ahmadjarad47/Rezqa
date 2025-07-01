namespace Rezqa.Application.Features.User.Dtos;

public class UserDetailsDto
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ImageUrl { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool LockoutEnabled { get; set; }
    public bool IsBlocked { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }

    public List<string> Roles { get; set; } = new();
    public int TotalAds { get; set; }
    public int ActiveAds { get; set; }
    public int InactiveAds { get; set; }
}