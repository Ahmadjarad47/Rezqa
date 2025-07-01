using MediatR;
using Rezqa.Application.Features.DynamicField.Requests.Commands;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Entities;

namespace Rezqa.Application.Features.DynamicField.Handlers.Commands.Update;

public class UpdateDynamicFieldCommand : IRequestHandler<UpdateDynamicFieldRequest, bool>
{
    private readonly IDynamicFieldRepository _dynamicFieldRepository;

    public UpdateDynamicFieldCommand(IDynamicFieldRepository dynamicFieldRepository)
    {
        _dynamicFieldRepository = dynamicFieldRepository;
    }

    public async Task<bool> Handle(UpdateDynamicFieldRequest request, CancellationToken cancellationToken)
    {
        // Get the existing dynamic field
        var existingField = await _dynamicFieldRepository.GetByIdAsync(request.Id);
        if (existingField == null)
        {
            return false;
        }

        // Check if another field with the same name exists in the same category/subcategory
        var exists = await _dynamicFieldRepository.ExistsByNameAsync(request.Name, request.CategoryId, request.SubCategoryId);
        if (exists && existingField.Name != request.Name)
        {
            throw new InvalidOperationException($"A dynamic field with name '{request.Name}' already exists in this category.");
        }

        // Update the dynamic field properties
        existingField.Title = request.Title;
        existingField.Name = request.Name;
        existingField.Type = request.Type;
        existingField.CategoryId = request.CategoryId;
        existingField.SubCategoryId = request.SubCategoryId;

        // Clear existing options and add new ones
        existingField.Options.Clear();
        existingField.Options = request.Options.Select(opt => new FieldOption
        {
            Title = opt.Label,
            Value = opt.Value,
            ParentValue = !string.IsNullOrEmpty(opt.ParentValue) ? opt.ParentValue : null,
            DynamicFieldId = existingField.Id
        }).ToList();

        // Update the entity in the database
        await _dynamicFieldRepository.UpdateAsync(existingField);
        
        return true;
    }
}
