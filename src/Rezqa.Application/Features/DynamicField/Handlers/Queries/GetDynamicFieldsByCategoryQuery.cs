using MediatR;
using Microsoft.EntityFrameworkCore;
using Rezqa.Application.Features.DynamicField.Dtos;
using Rezqa.Application.Features.DynamicField.Requests.Queries;
using Rezqa.Application.Interfaces;

namespace Rezqa.Application.Features.DynamicField.Handlers.Queries;

public class GetDynamicFieldsByCategoryQuery : IRequestHandler<GetDynamicFieldsByCategoryRequest, List<DynamicFieldDto>>
{
    private readonly IDynamicFieldRepository _dynamicFieldRepository;

    public GetDynamicFieldsByCategoryQuery(IDynamicFieldRepository dynamicFieldRepository)
    {
        _dynamicFieldRepository = dynamicFieldRepository;
    }

    public async Task<List<DynamicFieldDto>> Handle(GetDynamicFieldsByCategoryRequest request, CancellationToken cancellationToken)
    {
        var query = await _dynamicFieldRepository.GetByCategoryAsync(request.CategoryId, request.SubCategoryId);

        if (request.SubCategoryId.HasValue)
        {
            query = query.Where(df => df.SubCategoryId == request.SubCategoryId.Value).ToList();
        }

        var dynamicFields = query
            .Select(df => new DynamicFieldDto
            {
                Id = df.Id,
                Title = df.Title,
                Name = df.Name,
                Type = df.Type,
                CategoryId = df.CategoryId,
                CategoryTitle = df.Category.Title,
                SubCategoryId = df.SubCategoryId,
                SubCategoryTitle = df.SubCategory != null ? df.SubCategory.Title : null,
                Options = df.Options.Select(opt => new FieldOptionDto
                {
                    Id = opt.Id,
                    Label = opt.Title,
                    Value = opt.Value,
                    DynamicFieldId = opt.DynamicFieldId
                }).ToList()
            })
            .ToList();

        return dynamicFields;
    }
}