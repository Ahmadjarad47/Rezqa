using MediatR;
using Microsoft.EntityFrameworkCore;
using Rezqa.Application.Features.DynamicField.Dtos;
using Rezqa.Application.Features.DynamicField.Requests.Queries;
using Rezqa.Application.Interfaces;

namespace Rezqa.Application.Features.DynamicField.Handlers.Queries;

public class GetDynamicFieldByIdQuery : IRequestHandler<GetDynamicFieldByIdRequest, DynamicFieldDto?>
{
    private readonly IDynamicFieldRepository _dynamicFieldRepository;

    public GetDynamicFieldByIdQuery(IDynamicFieldRepository dynamicFieldRepository)
    {
        _dynamicFieldRepository = dynamicFieldRepository;
    }

    public async Task<DynamicFieldDto?> Handle(GetDynamicFieldByIdRequest request, CancellationToken cancellationToken)
    {
        var dynamicField = await _dynamicFieldRepository.GetByIdAsync(request.Id);

        if (dynamicField == null)
            return null;

        return new DynamicFieldDto
        {
            Id = dynamicField.Id,
            Title = dynamicField.Title,
            Name = dynamicField.Name,
            Type = dynamicField.Type,
            CategoryId = dynamicField.CategoryId,
            CategoryTitle = dynamicField.Category.Title,
            SubCategoryId = dynamicField.SubCategoryId,
            SubCategoryTitle = dynamicField.SubCategory?.Title,
            Options = dynamicField.Options.Select(opt => new FieldOptionDto
            {
                Id = opt.Id,
                Label = opt.Title,
                Value = opt.Value,
                DynamicFieldId = opt.DynamicFieldId
            }).ToList()
        };
    }
}

