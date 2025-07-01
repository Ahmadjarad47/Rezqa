using MediatR;
using System;

namespace Rezqa.Application.Features.SubCategory.Requests.Commands;

public class UpdateSubCategoryRequest : IRequest<bool>
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int CategoryId { get; set; }
} 