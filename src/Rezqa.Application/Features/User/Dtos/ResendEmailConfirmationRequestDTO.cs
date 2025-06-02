
using System.ComponentModel.DataAnnotations;

namespace Rezqa.Application.Features.User.Dtos;


public class ResendEmailConfirmationDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}