using MediatR;
using Rezqa.Application.Common.Models;
using Rezqa.Application.Features.Ad.Dtos;
using Rezqa.Application.Features.Ad.Request.Query;
using Rezqa.Application.Interfaces;

namespace Rezqa.Application.Features.Ad.Handlers.Query;

public class SearchAdsQueryHandler : IRequestHandler<SearchAdsQuery, PaginatedResult<AdDto>>
{
    private readonly IAdRepository _adRepository;

    public SearchAdsQueryHandler(IAdRepository adRepository)
    {
        _adRepository = adRepository;
    }

    public async Task<PaginatedResult<AdDto>> Handle(SearchAdsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var paginatedAds = await _adRepository.SearchAdsAsync(request, cancellationToken);

            List<AdDto> adDtos = paginatedAds.Items.Select(ad => new AdDto
            {
                Id = ad.Id,
                Title = ad.Title,
                Description = ad.Description,
                Price = ad.Price,
                isActive = ad.isActive,
                ImageUrl = ad.ImageUrl,
                UserName = ad.AppUsers?.UserName,
                location = ad.location,
                CreatedAt = ad.CreatedAt,
                categoryTitle = ad.Category?.Title,
                PhonNumber = ad.AppUsers?.PhoneNumber,
                adFieldDtos = ad.FieldValues?.Select(adField => new AdFieldDto
                {
                    Title = adField.DynamicField?.Title,
                    DynamicFieldId = adField.DynamicFieldId,
                    Value = adField.Value,
                }).ToList() ?? new List<AdFieldDto>()
            }).ToList();

            return new PaginatedResult<AdDto>
            {
                Items = adDtos,
                PageNumber = paginatedAds.PageNumber,
                PageSize = paginatedAds.PageSize,
                TotalCount = paginatedAds.TotalCount
            };
        }
        catch (Exception ex)
        {
            return new PaginatedResult<AdDto>
            {
                Items = new List<AdDto>(),
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 0
            };
        }
    }
}