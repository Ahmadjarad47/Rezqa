using MediatR;
using Rezqa.Application.Features.DynamicField.Dtos;
using System.ComponentModel.DataAnnotations;

namespace Rezqa.Application.Features.DynamicField.Requests.Queries;

public class GetDynamicFieldsByCategoryRequest : IRequest<List<DynamicFieldDto>>
{
    [Required]
    public int CategoryId { get; set; }
    
    public int? SubCategoryId { get; set; }
} 