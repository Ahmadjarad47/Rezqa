using MediatR;
using Microsoft.AspNetCore.Http;
using System;

namespace Rezqa.Application.Features.Category.Requests.Commands;

public class UpdateCategoryRequest : IRequest<bool>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public IFormFile? Image { get; set; }
    public string Description { get; set; } = null!;
    public string UpdatedBy { get; set; } = null!;
} 