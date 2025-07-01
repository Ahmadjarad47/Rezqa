using MediatR;
using System;

namespace Rezqa.Application.Features.SubCategory.Requests.Commands;

public class DeleteSubCategoryRequest : IRequest<bool>
{
    public int Id { get; set; }
} 