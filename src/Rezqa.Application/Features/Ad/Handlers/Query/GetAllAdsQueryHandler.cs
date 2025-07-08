using MediatR;
using Rezqa.Application.Features.Ad.Dtos;
using Rezqa.Application.Features.Ad.Request.Query;
using Rezqa.Application.Interfaces;
using Rezqa.Application.Common.Models;

namespace Rezqa.Application.Features.Ad.Handlers.Query;

public class GetAllAdsQueryHandler : IRequestHandler<GetAllAdsQuery, PaginatedResult<AdDto>>
{
    private readonly IAdRepository _adRepository;

    public GetAllAdsQueryHandler(IAdRepository adRepository)
    {
        _adRepository = adRepository;
    }

    public async Task<PaginatedResult<AdDto>> Handle(GetAllAdsQuery request, CancellationToken cancellationToken)
    {
        var ads = await _adRepository.GetAllAsync();
        var query = ads.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrEmpty(request.search))
        {
            query = query.Where(ad =>
                ad.Title.Contains(request.search) ||
                ad.Description.Contains(request.search));
        }

        var totalCount = query.Count();

        // Apply pagination
        var pagedAds = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        if (request.isfilterStop)
        {
            List<AdDto>? adDto = pagedAds.Where(m => m.isActive == true).Select(ad => new AdDto
            {
                Id = ad.Id,
                Title = ad.Title,
                Description = ad.Description,
                Price = ad.Price,
                ImageUrl = ad.ImageUrl,
                isActive = ad.isActive,
                UserName = ad.AppUsers.UserName,
                location = ad.location,
                CreatedAt = ad.CreatedAt,
                PhonNumber = ad.AppUsers.PhoneNumber,
                adFieldDtos = ad.FieldValues.Select(adField => new AdFieldDto
                {
                    DynamicFieldId = adField.DynamicFieldId,
                    Value = adField.Value,
                }).ToList()

            }).ToList();

            return new PaginatedResult<AdDto>
            {
                Items = adDto,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
        List<AdDto>? adDtos = pagedAds.Select(ad => new AdDto
        {
            Id = ad.Id,
            Title = ad.Title,
            Description = ad.Description,
            Price = ad.Price,
            ImageUrl = ad.ImageUrl,
            isActive = ad.isActive,
            UserName = ad.AppUsers.UserName,
            location = ad.location,
            CreatedAt = ad.CreatedAt,

            PhonNumber = ad.AppUsers.PhoneNumber,
            adFieldDtos = ad.FieldValues.Select(adField => new AdFieldDto
            {
                DynamicFieldId = adField.DynamicFieldId,
                Value = adField.Value,
            }).ToList()

        }).ToList();

        return new PaginatedResult<AdDto>
        {
            Items = adDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}