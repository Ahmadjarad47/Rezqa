using MediatR;
using Rezqa.Domain.Common.Interfaces;
using Rezqa.Application.Features.Category.Requests.Commands;
using Rezqa.Infrastructure.Persistence;
using System;

namespace Rezqa.Application.Features.Category.Handlers.Commands.Create;

public class CreateCategoryCommand : IRequestHandler<CreateCategoryRequest, Guid>
{
    private readonly ApplicationDbContext _context;
    private readonly IFileService _fileService;

    public CreateCategoryCommand(ApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task<Guid> Handle(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        if (request.Image == null)
            throw new ArgumentException("Image is required", nameof(request.Image));

        var category = new Domain.Entities.Category
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            Image = await _fileService.SaveFileAsync(request.Image, "categories")
        };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}