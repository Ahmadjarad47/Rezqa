using System;

namespace Rezqa.Application.Features.DynamicField.Dtos;

public class DynamicFieldDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public int CategoryId { get; set; }
    public string CategoryTitle { get; set; } = null!;
    public int? SubCategoryId { get; set; }
    public string? SubCategoryTitle { get; set; }
    public bool shouldFilterbyParent { get; set; }
    public ICollection<FieldOptionDto> Options { get; set; } = new List<FieldOptionDto>();

}

public class FieldOptionDto
{
    public int Id { get; set; }
    public string Label { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string? ParentValue { get; set; }

    public int DynamicFieldId { get; set; }
}