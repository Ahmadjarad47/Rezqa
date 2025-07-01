using MediatR;
using Rezqa.Application.Features.Ad.Dtos;
using Rezqa.Application.Common.Models;

namespace Rezqa.Application.Features.Ad.Request.Query;

public record GetAllAdsQuery : PaginatedRequest, IRequest<PaginatedResult<AdDto>>
{
    public bool isfilterStop { get; set; } = false;
};