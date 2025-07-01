using MediatR;
using Rezqa.Application.Features.Category.Requests.Commands;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Common.Interfaces;
using Rezqa.Domain.Entities;

namespace Rezqa.Application.Features.Category.Handlers.Commands.Create;

public class CreateCategoryCommand : IRequestHandler<CreateCategoryRequest, int>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IFileService fileService;
    public CreateCategoryCommand(ICategoryRepository categoryRepository, IFileService fileService)
    {
        _categoryRepository = categoryRepository;
        this.fileService = fileService;
    }

    public async Task<int> Handle(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = new Rezqa.Domain.Entities.Category
        {
            Title = request.Title,
            Description = request.Title,
            IsActive = request.IsActive,
            Image = await fileService.SaveFileAsync(request.Image,"category")
        }
        ;

        var result = await _categoryRepository.AddAsync(category);
        return result.Id;
    }
}