using Microsoft.AspNetCore.Http;
using Rezqa.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Rezqa.Application.Features.Ad.Dtos;

public class CreateAdDto
{
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(400, MinimumLength = 10)]
    public string Description { get; set; } = null!;

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
    public decimal Price { get; set; }

    [MaxFileSizeAttribute(30 * 1024 * 1024)]
    public IFormFileCollection? ImageUrl { get; set; }

    [Required]
    [StringLength(70, MinimumLength = 10)]
    public string location { get; set; }

    public int CategoryId { get; set; }

    public int SubCategoryId { get; set; }

    public Guid UserId { get; set; } = Guid.Empty;

    public ICollection<AdFieldValue> fieldValues { get; set; }
}
public class AdFieldValue
{
    public int DynamicFieldId { get; set; }
    public string? Value { get; set; }
}