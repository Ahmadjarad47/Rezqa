using MediatR;
using Rezqa.Application.Features.Category.Requests.Commands;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Common.Interfaces;

namespace Rezqa.Application.Features.Category.Handlers.Commands.Delete;

public class DeleteCategoryCommand : IRequestHandler<DeleteCategoryRequest, bool>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IFileService _fileService;

    public DeleteCategoryCommand(ICategoryRepository categoryRepository, IFileService fileService)
    {
        _categoryRepository = categoryRepository;
        _fileService = fileService;
    }

    public async Task<bool> Handle(DeleteCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id);
        if (category == null)
            return false;

        if (!string.IsNullOrEmpty(category.Image))
        {
            await _fileService.DeleteFileAsync(category.Image);
        }

        await _categoryRepository.DeleteAsync(category);
        return true;
    }
} 