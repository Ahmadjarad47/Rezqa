using MediatR;
using Rezqa.Application.Features.SubCategory.Requests.Commands;
using Rezqa.Application.Interfaces;

namespace Rezqa.Application.Features.SubCategory.Handlers.Commands.Delete;

public class DeleteSubCategoryCommand : IRequestHandler<DeleteSubCategoryRequest, bool>
{
    private readonly ISubCategoryRepository _subCategoryRepository;

    public DeleteSubCategoryCommand(ISubCategoryRepository subCategoryRepository)
    {
        _subCategoryRepository = subCategoryRepository;
    }

    public async Task<bool> Handle(DeleteSubCategoryRequest request, CancellationToken cancellationToken)
    {
        var subCategory = await _subCategoryRepository.GetByIdAsync(request.Id);
        if (subCategory == null)
            return false;

        await _subCategoryRepository.DeleteAsync(subCategory);
        return true;
    }
} 