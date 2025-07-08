using MediatR;
using Rezqa.Application.Features.Category.Dtos;
using Rezqa.Application.Features.Category.Requests.Queries;
using Rezqa.Application.Interfaces;
using Rezqa.Application.Common.Models;

namespace Rezqa.Application.Features.Category.Handlers.Queries.GetAll;

public class GetAllCategoriesQuery : IRequestHandler<GetAllCategoriesRequest, PaginatedResult<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllCategoriesQuery(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<PaginatedResult<CategoryDto>> Handle(GetAllCategoriesRequest request, CancellationToken cancellationToken)
    {
        var allCategories = await _categoryRepository.GetAllAsync();
        if (request.isPagnationStop)
        {
            var categoryDto = allCategories.Where(m=>m.IsActive).Select(c => new CategoryDto
            {
                Id = c.Id,
                Title = c.Title,
                Image = c.Image,
                CreatedAt = c.CreatedAt,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedBy = c.CreatedBy,
                UpdatedAt = c.UpdatedAt,
                UpdatedBy = c.UpdatedBy,
            }).ToList();
            return new PaginatedResult<CategoryDto>
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = 0,
                Items = categoryDto,
            };

        }
        // Apply filtering
        var filteredCategories = allCategories.AsQueryable();

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            filteredCategories = filteredCategories.Where(c => c.Title.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (request.IsActive.HasValue)
        {
            filteredCategories = filteredCategories.Where(c => c.IsActive == request.IsActive.Value);
        }

        var totalCount = filteredCategories.Count();

        // Apply pagination
        var pagedCategories = filteredCategories
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var categoryDtos = pagedCategories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Title = c.Title,
            Image = c.Image,
            CreatedAt = c.CreatedAt,
            Description = c.Description,
            IsActive = c.IsActive,
            CreatedBy = c.CreatedBy,
            UpdatedAt = c.UpdatedAt,
            UpdatedBy = c.UpdatedBy,
        }).ToList();

        return new PaginatedResult<CategoryDto>
        {
            Items = categoryDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}