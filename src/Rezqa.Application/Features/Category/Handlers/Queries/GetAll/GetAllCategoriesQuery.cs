using MediatR;
using Microsoft.EntityFrameworkCore;
using Rezqa.Application.Features.Category.Dtos;
using Rezqa.Application.Features.Category.Requests.Queries;
using Rezqa.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rezqa.Application.Features.Category.Handlers.Queries.GetAll;

public class GetAllCategoriesQuery : IRequestHandler<GetAllCategoriesRequest, List<CategoryDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAllCategoriesQuery(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> Handle(GetAllCategoriesRequest request, CancellationToken cancellationToken)
    {
        return await _context.Categories
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
            .ToListAsync(cancellationToken);
    }
}