using MediatR;
using System;

namespace Rezqa.Application.Features.Category.Requests.Commands;

public class DeleteCategoryRequest : IRequest<bool>
{
    public Guid Id { get; set; }
} 