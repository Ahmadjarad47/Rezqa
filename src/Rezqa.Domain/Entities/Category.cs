using Rezqa.Domain.Common;
using System;

namespace Rezqa.Domain.Entities;

public class Category : BaseEntity
{
   
    public string Title { get; set; } = null!;
    public string? Image { get; set; }
    public string Description { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    public ICollection<DynamicField> DynamicFields { get; set; } = new List<DynamicField>();
}