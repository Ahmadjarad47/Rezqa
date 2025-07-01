using MediatR;
using System;

namespace Rezqa.Application.Features.SubCategory.Requests.Commands;

public class CreateSubCategoryRequest : IRequest<int>
{
    public string Title { get; set; } = null!;
    public int CategoryId { get; set; }
} 