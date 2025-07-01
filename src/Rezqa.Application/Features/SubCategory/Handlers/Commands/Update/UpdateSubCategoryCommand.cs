using MediatR;
using Rezqa.Application.Features.SubCategory.Requests.Commands;
using Rezqa.Application.Interfaces;

namespace Rezqa.Application.Features.SubCategory.Handlers.Commands.Update;

public class UpdateSubCategoryCommand : IRequestHandler<UpdateSubCategoryRequest, bool>
{
    private readonly ISubCategoryRepository _subCategoryRepository;

    public UpdateSubCategoryCommand(ISubCategoryRepository subCategoryRepository)
    {
        _subCategoryRepository = subCategoryRepository;
    }

    public async Task<bool> Handle(UpdateSubCategoryRequest request, CancellationToken cancellationToken)
    {
        var subCategory = await _subCategoryRepository.GetByIdAsync(request.Id);
        if (subCategory == null)
            return false;

        // Validate that the category exists
        var categoryExists = await _subCategoryRepository.CategoryExistsAsync(request.CategoryId);

        if (!categoryExists)
            throw new ArgumentException($"Category with ID {request.CategoryId} not found.");

        subCategory.Title = request.Title;
        subCategory.CategoryId = request.CategoryId;

        await _subCategoryRepository.UpdateAsync(subCategory);
        return true;
    }
} 