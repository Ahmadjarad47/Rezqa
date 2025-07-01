using MediatR;
using Rezqa.Application.Features.DynamicField.Dtos;
using Rezqa.Application.Common.Models;

namespace Rezqa.Application.Features.DynamicField.Requests.Queries;

public class GetAllDynamicFieldsRequest : IRequest<PaginatedResult<DynamicFieldDto>>
{
    public string? SearchTerm { get; set; }
    public string? Type { get; set; }
    public int? CategoryId { get; set; }
    public int? SubCategoryId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}