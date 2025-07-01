using MediatR;
using Rezqa.Application.Features.SubCategory.Dtos;
using System;

namespace Rezqa.Application.Features.SubCategory.Requests.Queries;

public class GetSubCategoryByIdRequest : IRequest<SubCategoryDto>
{
    public int Id { get; set; }
} 