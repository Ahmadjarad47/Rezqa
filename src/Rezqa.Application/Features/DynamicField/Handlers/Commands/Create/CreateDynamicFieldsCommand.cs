using MediatR;
using Rezqa.Application.Features.DynamicField.Requests.Commands;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Entities;

namespace Rezqa.Application.Features.DynamicField.Handlers.Commands.Create;

public class CreateDynamicFieldsCommand : IRequestHandler<CreateDynamicFieldsRequest, List<int>>
{
    private readonly IDynamicFieldRepository _dynamicFieldRepository;

    public CreateDynamicFieldsCommand(IDynamicFieldRepository dynamicFieldRepository)
    {
        _dynamicFieldRepository = dynamicFieldRepository;
    }

    public async Task<List<int>> Handle(CreateDynamicFieldsRequest request, CancellationToken cancellationToken)
    {
        var createdIds = new List<int>();

        foreach (var fieldRequest in request.DynamicFields)
        {
            // Check if field with same name exists in the same category/subcategory
            var exists = await _dynamicFieldRepository.ExistsByNameAsync(fieldRequest.Name, fieldRequest.CategoryId, fieldRequest.SubCategoryId);
            if (exists)
            {
                throw new InvalidOperationException($"A dynamic field with name '{fieldRequest.Name}' already exists in this category.");
            }

            var dynamicField = new Rezqa.Domain.Entities.DynamicField
            {
                Title = fieldRequest.Title,
                Name = fieldRequest.Name,
                Type = fieldRequest.Type,
                CategoryId = fieldRequest.CategoryId,
                SubCategoryId = fieldRequest.SubCategoryId,
                shouldFilterbyParent = fieldRequest.shouldFilterbyParent,
                Options = fieldRequest.Options.Select(opt => new FieldOption
                {
                    Title = opt.Label,
                    Value = opt.Value,
                    ParentValue = !string.IsNullOrEmpty(opt.ParentValue) ? opt.ParentValue : null
                }).ToList()
            };

            var result = await _dynamicFieldRepository.AddAsync(dynamicField);
            createdIds.Add(result.Id);
        }

        return createdIds;
    }
}