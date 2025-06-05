using System;

namespace Rezqa.Domain.Entities;

public class Category
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