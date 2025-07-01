using MediatR;
using Rezqa.Application.Features.SubCategory.Dtos;
using Rezqa.Application.Common.Models;

namespace Rezqa.Application.Features.SubCategory.Requests.Queries;

public class GetAllSubCategoriesRequest : IRequest<PaginatedResult<SubCategoryDto>>
{
    public bool isPagnationStop { get; set; } = false;
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public bool isActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}