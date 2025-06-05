using MediatR;
using Rezqa.Application.Features.Category.Dtos;
using System;

namespace Rezqa.Application.Features.Category.Requests.Queries;

public class GetCategoryByIdRequest : IRequest<CategoryDto>
{
    public Guid Id { get; set; }
} 