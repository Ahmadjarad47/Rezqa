using MediatR;
using Rezqa.Application.Features.Category.Dtos;
using Rezqa.Application.Features.Category.Requests.Queries;
using Rezqa.Application.Interfaces;

namespace Rezqa.Application.Features.Category.Handlers.Queries.GetById;

public class GetCategoryByIdQuery : IRequestHandler<GetCategoryByIdRequest, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdQuery(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDto> Handle(GetCategoryByIdRequest request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id);

        if (category == null)
            return null!;

        return new CategoryDto
        {
            Id = category.Id,
            Title = category.Title
        };
    }
} 