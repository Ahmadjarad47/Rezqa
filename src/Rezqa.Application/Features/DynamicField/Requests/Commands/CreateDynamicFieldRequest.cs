using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Rezqa.Application.Features.DynamicField.Requests.Commands;

public class CreateDynamicFieldRequest : IRequest<int>
{
    [Required]
    public string Title { get; set; } = null!;

    [Required]
    public string Name { get; set; } = null!;

    public string Type { get; set; } = "text";

    [Required]
    public int CategoryId { get; set; }

    public int? SubCategoryId { get; set; }
    public bool shouldFilterbyParent { get; set; }
    public ICollection<FieldOptionRequest> Options { get; set; } = new List<FieldOptionRequest>();
}

public class FieldOptionRequest
{
    [Required]
    public string Label { get; set; } = null!;

    [Required]
    public string Value { get; set; } = null!;

    public string? ParentValue { get; set; }
}