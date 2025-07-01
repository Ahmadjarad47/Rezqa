using MediatR;
using Microsoft.EntityFrameworkCore;
using Rezqa.Application.Common.Models;
using Rezqa.Application.Features.DynamicField.Dtos;
using Rezqa.Application.Features.DynamicField.Requests.Queries;
using Rezqa.Application.Interfaces;

namespace Rezqa.Application.Features.DynamicField.Handlers.Queries;

public class GetAllDynamicFieldsQuery : IRequestHandler<GetAllDynamicFieldsRequest, PaginatedResult<DynamicFieldDto>>
{
    private readonly IDynamicFieldRepository _dynamicFieldRepository;

    public GetAllDynamicFieldsQuery(IDynamicFieldRepository dynamicFieldRepository)
    {
        _dynamicFieldRepository = dynamicFieldRepository;
    }

    public async Task<PaginatedResult<DynamicFieldDto>> Handle(GetAllDynamicFieldsRequest request, CancellationToken cancellationToken)
    {
        var query = await _dynamicFieldRepository.GetAllAsync();
        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(df => df.Title.Contains(request.SearchTerm) || df.Name.Contains(request.SearchTerm));
        }

        if (!string.IsNullOrEmpty(request.Type))
        {
            query = query.Where(df => df.Type == request.Type);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(df => df.CategoryId == request.CategoryId.Value);
        }

        if (request.SubCategoryId.HasValue)
        {
            query = query.Where(df => df.SubCategoryId == request.SubCategoryId.Value);
        }

        var totalCount = query.Count();

        var dynamicFields = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(df => new DynamicFieldDto
            {
                Id = df.Id,
                Title = df.Title,
                Name = df.Name,
                Type = df.Type,
                CategoryId = df.CategoryId,
                CategoryTitle = df.Category.Title,
                SubCategoryId = df.SubCategoryId,
                shouldFilterbyParent = df.shouldFilterbyParent,
                SubCategoryTitle = df.SubCategory != null ? df.SubCategory.Title : null,
                Options = df.Options.Select(opt => new FieldOptionDto
                {
                    Id = opt.Id,
                    Label = opt.Title,
                    Value = opt.Value,
                    ParentValue = opt.ParentValue,
                    DynamicFieldId = opt.DynamicFieldId
                }).ToList()
            })
            .ToList();

        return new PaginatedResult<DynamicFieldDto>
        {
            Items = dynamicFields,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

