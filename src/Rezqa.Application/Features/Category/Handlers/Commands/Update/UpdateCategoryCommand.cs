using MediatR;
using Microsoft.EntityFrameworkCore;
using Rezqa.Domain.Common.Interfaces;
using Rezqa.Application.Features.Category.Requests.Commands;
using Rezqa.Infrastructure.Persistence;
using System;

namespace Rezqa.Application.Features.Category.Handlers.Commands.Update;

public class UpdateCategoryCommand : IRequestHandler<UpdateCategoryRequest, bool>
{
    private readonly ApplicationDbContext _context;
    private readonly IFileService _fileService;

    public UpdateCategoryCommand(ApplicationDbContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    public async Task<bool> Handle(UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FindAsync(new object[] { request.Id }, cancellationToken);
        if (category == null)
            return false;

        category.Title = request.Title;
        category.Description = request.Description;
        category.UpdatedBy = request.UpdatedBy;
        category.UpdatedAt = DateTime.UtcNow;

        if (request.Image != null)
        {
            if (!string.IsNullOrEmpty(category.Image))
            {
                await _fileService.DeleteFileAsync(category.Image);
            }
            category.Image = await _fileService.SaveFileAsync(request.Image, "categories");
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}