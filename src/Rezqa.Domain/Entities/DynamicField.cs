namespace Rezqa.Domain.Entities;


public class DynamicField
{
    public int Id { get; set; }

    public string Title { get; set; } = null!; // "اختر السيارة"
    public string Name { get; set; } = null!; // "car_brand"
    public string Type { get; set; } = "text"; // "select", "text", "number", etc.

    public int CategoryId { get; set; }
    public Category Category { get; set; }

    public int? SubCategoryId { get; set; }
    public SubCategory? SubCategory { get; set; }

    public bool shouldFilterbyParent { get; set; }
    public ICollection<FieldOption> Options { get; set; } = new List<FieldOption>();
}