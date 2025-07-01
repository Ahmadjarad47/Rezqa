using MediatR;
using Rezqa.Application.Features.SubCategory.Dtos;
using Rezqa.Application.Features.SubCategory.Requests.Queries;
using Rezqa.Application.Interfaces;
using Rezqa.Application.Common.Models;
using Rezqa.Domain.Entities;

namespace Rezqa.Application.Features.SubCategory.Handlers.Queries.GetAll;

public class GetAllSubCategoriesQuery : IRequestHandler<GetAllSubCategoriesRequest, PaginatedResult<SubCategoryDto>>
{
    private readonly ISubCategoryRepository _subCategoryRepository;

    public GetAllSubCategoriesQuery(ISubCategoryRepository subCategoryRepository)
    {
        _subCategoryRepository = subCategoryRepository;
    }

    public async Task<PaginatedResult<SubCategoryDto>> Handle(GetAllSubCategoriesRequest request, CancellationToken cancellationToken)
    {
        var allSubCategories = await _subCategoryRepository.GetAllAsync();

        // Apply filtering
        var filteredSubCategories = allSubCategories.AsQueryable();

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            filteredSubCategories = filteredSubCategories.Where(sc => sc.Title.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (request.CategoryId.HasValue)
        {
            filteredSubCategories = filteredSubCategories.Where(sc => sc.CategoryId == request.CategoryId.Value);
        }
        if (request.isPagnationStop)
        {
            var subCategoryDto = filteredSubCategories.Select(sc => new SubCategoryDto
            {
                Id = sc.Id,
                Title = sc.Title,
                CategoryId = sc.CategoryId,
                CategoryTitle = sc.Category.Title ?? string.Empty,

                IsDeleted = sc.IsDeleted
            }).ToList();

            return new PaginatedResult<SubCategoryDto>
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = 0,
                Items = subCategoryDto,
            };
        }

        var totalCount = filteredSubCategories.Count();

        // Apply pagination
        var pagedSubCategories = filteredSubCategories
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var subCategoryDtos = pagedSubCategories.Select(sc => new SubCategoryDto
        {
            Id = sc.Id,
            Title = sc.Title,
            CategoryId = sc.CategoryId,
            CategoryTitle = sc.Category.Title ?? string.Empty,

            IsDeleted = sc.IsDeleted
        }).ToList();

        return new PaginatedResult<SubCategoryDto>
        {
            Items = subCategoryDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}