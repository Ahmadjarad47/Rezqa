using MediatR;
using Rezqa.Application.Features.Category.Requests.Commands;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Common.Interfaces;
using Rezqa.Domain.Entities;

namespace Rezqa.Application.Features.Category.Handlers.Commands.Update;

public class UpdateCategoryCommand : IRequestHandler<UpdateCategoryRequest, bool>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IFileService _fileService;

    public UpdateCategoryCommand(ICategoryRepository categoryRepository, IFileService fileService)
    {
        _categoryRepository = categoryRepository;
        _fileService = fileService;
    }

    public async Task<bool> Handle(UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id);
        if (category == null)
            return false;

        category.Title = request.Title;
        category.Description = request.Description;
        category.IsActive = request.IsActive;

        if (request.Image != null)
        {
            // Delete old image if exists
            if (!string.IsNullOrEmpty(category.Image))
            {
                await _fileService.DeleteFileAsync(category.Image);
            }
            category.Image = await _fileService.SaveFileAsync(request.Image, "categories");
        }

        await _categoryRepository.UpdateAsync(category);
        return true;
    }
} 