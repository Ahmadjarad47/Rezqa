using System.ComponentModel.DataAnnotations;

namespace Rezqa.Application.Features.Ad.Dtos;

public class AdDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public string PhonNumber { get; set; }
    public string categoryTitle { get; set; }
    public string location { get; set; }
    public string[]? ImageUrl { get; set; }
    public Guid UserId { get; set; }
    public bool isActive { get; set; }
    public string UserName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<AdFieldDto> adFieldDtos { get; set; }
}
public class AdFieldDto
{
    public string Title { get; set; }
    public int DynamicFieldId { get; set; }
    public string Value { get; set; }
}