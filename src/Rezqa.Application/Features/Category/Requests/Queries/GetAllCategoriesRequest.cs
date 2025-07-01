using MediatR;
using Rezqa.Application.Features.Category.Dtos;
using Rezqa.Application.Common.Models;

namespace Rezqa.Application.Features.Category.Requests.Queries;

public class GetAllCategoriesRequest : IRequest<PaginatedResult<CategoryDto>>
{
    public bool isPagnationStop { get; set; } = false;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}