using MediatR;
using Microsoft.EntityFrameworkCore;
using Rezqa.Application.Features.Category.Dtos;
using Rezqa.Application.Features.Category.Requests.Queries;
using Rezqa.Infrastructure.Persistence;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rezqa.Application.Features.Category.Handlers.Queries.GetById;

public class GetCategoryByIdQuery : IRequestHandler<GetCategoryByIdRequest, CategoryDto>
{
    private readonly ApplicationDbContext _context;

    public GetCategoryByIdQuery(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(GetCategoryByIdRequest request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Title = c.Title,
                Image = c.Image,
                Description = c.Description,
                CreatedBy = c.CreatedBy,
                UpdatedBy = c.UpdatedBy,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
            throw new KeyNotFoundException($"Category with ID {request.Id} not found.");

        return category;
    }
}