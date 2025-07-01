using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Rezqa.Application.Features.DynamicField.Requests.Commands;

public class UpdateDynamicFieldRequest : IRequest<bool>
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; } = null!;
    
    [Required]
    public string Name { get; set; } = null!;
    
    public string Type { get; set; } = "text";
    
    [Required]
    public int CategoryId { get; set; }
    
    public int? SubCategoryId { get; set; }
    
    public ICollection<FieldOptionRequest> Options { get; set; } = new List<FieldOptionRequest>();
} 