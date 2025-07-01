using MediatR;
using System;

namespace Rezqa.Application.Features.Category.Requests.Commands;

public class DeleteCategoryRequest : IRequest<bool>
{
    public int Id { get; set; }
} 