using MediatR;
using Rezqa.Application.Common.Models;
using Rezqa.Application.Features.Ad.Dtos;

namespace Rezqa.Application.Features.Ad.Request.Query;

public record SearchAdsQuery(
    string? SearchTerm,
    int? CategoryId,
    int? SubCategoryId,
    int PageSize,
    int PageNumber,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Location
    ) : IRequest<PaginatedResult<AdDto>>;