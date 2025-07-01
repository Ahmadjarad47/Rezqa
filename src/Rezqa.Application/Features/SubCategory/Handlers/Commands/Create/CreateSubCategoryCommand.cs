using MediatR;
using Rezqa.Application.Features.SubCategory.Requests.Commands;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Entities;
using System;

namespace Rezqa.Application.Features.SubCategory.Handlers.Commands.Create;

public class CreateSubCategoryCommand : IRequestHandler<CreateSubCategoryRequest, int>
{
    private readonly ISubCategoryRepository _subCategoryRepository;
    private readonly ICategoryRepository _categoryRepository;

    public CreateSubCategoryCommand(
        ISubCategoryRepository subCategoryRepository,
        ICategoryRepository categoryRepository)
    {
        _subCategoryRepository = subCategoryRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<int> Handle(CreateSubCategoryRequest request, CancellationToken cancellationToken)
    {
        // Validate that the category exists
        var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);

        if (!categoryExists)
            throw new ArgumentException($"Category with ID {request.CategoryId} not found.");

        var subCategory = new Rezqa.Domain.Entities.SubCategory
        {
            Title = request.Title,
            CategoryId = request.CategoryId
        };

        var result = await _subCategoryRepository.AddAsync(subCategory);
        return result.Id;
    }
}