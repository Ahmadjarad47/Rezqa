using MediatR;
using Microsoft.AspNetCore.Http;
using System;

namespace Rezqa.Application.Features.Category.Requests.Commands;

public class CreateCategoryRequest : IRequest<int>
{
    public string Title { get; set; } = null!;
    public IFormFile? Image { get; set; }
    public string Description { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}