using Microsoft.AspNetCore.Http;
using System;

namespace Rezqa.Application.Features.Category.Dtos;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Image { get; set; }
    public string Description { get; set; } = null!;
    public string CreatedBy { get; set; } = null!;
    public string? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 