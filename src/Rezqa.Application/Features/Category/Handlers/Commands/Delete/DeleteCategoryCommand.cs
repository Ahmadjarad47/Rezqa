using MediatR;
using Rezqa.Domain.Common.Interfaces;
using Rezqa.Application.Features.Category.Requests.Commands;
using Rezqa.Infrastructure.Persistence;
using System;

namespace Rezqa.Application.Features.Category.Handlers.Commands.Delete;

public class DeleteCategoryCommand : IRequestHandler<DeleteCategoryRequest, bool>
{
    private readonly ApplicationDbContext _context;
    private readonly IFileService _fileService;

    public DeleteCategoryCommand(ApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task<bool> Handle(DeleteCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FindAsync(new object[] { request.Id }, cancellationToken);
        if (category == null)
            return false;

        if (!string.IsNullOrEmpty(category.Image))
        {
            await _fileService.DeleteFileAsync(category.Image);
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}