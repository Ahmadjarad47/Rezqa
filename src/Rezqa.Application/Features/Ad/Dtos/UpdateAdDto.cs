using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Rezqa.Application.Features.Ad.Dtos;

public class UpdateAdDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = null!;

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(100)]
    public string Location { get; set; } = null!;

    [Required]
    [Phone]
    public string Phone { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    public bool isActive { get; set; }
    public IFormFileCollection? ImageUrl { get; set; }
}