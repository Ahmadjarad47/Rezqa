using System;

namespace Rezqa.Application.Features.SubCategory.Dtos;

public class SubCategoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int CategoryId { get; set; }
    public string CategoryTitle { get; set; } = null!;
    public bool IsDeleted { get; set; }

}