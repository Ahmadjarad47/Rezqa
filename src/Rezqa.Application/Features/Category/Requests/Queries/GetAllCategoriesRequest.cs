using MediatR;
using Rezqa.Application.Features.Category.Dtos;
using System.Collections.Generic;

namespace Rezqa.Application.Features.Category.Requests.Queries;

public class GetAllCategoriesRequest : IRequest<List<CategoryDto>>
{
} 