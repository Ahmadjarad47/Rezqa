using MediatR;
using Rezqa.Application.Features.SubCategory.Dtos;
using Rezqa.Application.Features.SubCategory.Requests.Queries;
using Rezqa.Application.Interfaces;

namespace Rezqa.Application.Features.SubCategory.Handlers.Queries.GetById;

public class GetSubCategoryByIdQuery : IRequestHandler<GetSubCategoryByIdRequest, SubCategoryDto>
{
    private readonly ISubCategoryRepository _subCategoryRepository;

    public GetSubCategoryByIdQuery(ISubCategoryRepository subCategoryRepository)
    {
        _subCategoryRepository = subCategoryRepository;
    }

    public async Task<SubCategoryDto> Handle(GetSubCategoryByIdRequest request, CancellationToken cancellationToken)
    {
        var subCategory = await _subCategoryRepository.GetByIdAsync(request.Id);

        if (subCategory == null)
            return null!;

        return new SubCategoryDto
        {
            Id = subCategory.Id,
            Title = subCategory.Title,
            CategoryId = subCategory.CategoryId,
            CategoryTitle = subCategory.Category?.Title ?? string.Empty
        };
    }
} 