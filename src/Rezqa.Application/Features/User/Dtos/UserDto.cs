namespace Rezqa.Application.Features.User.Dtos;

public class UserDto
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string ImageUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public bool isConfirmeEmail { get; set; }
    public bool isBlocked { get; set; }
    public DateTimeOffset lockoutEnd { get; set; }
    public List<string> Roles { get; set; } = new();
}