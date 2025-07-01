using MediatR;
using Rezqa.Application.Features.Ad.Dtos;
using Rezqa.Application.Features.Ad.Request.Query;
using Rezqa.Application.Interfaces;

namespace Rezqa.Application.Features.Ad.Handlers.Query;

public class GetAdsByUserIdQueryHandler : IRequestHandler<GetAdsByUserIdQuery, List<AdDto>>
{
    private readonly IAdRepository _adRepository;

    public GetAdsByUserIdQueryHandler(IAdRepository adRepository)
    {
        _adRepository = adRepository;
    }

    public async Task<List<AdDto>> Handle(GetAdsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var ads = await _adRepository.GetAdsByUserIdAsync(request.UserId);

        return ads.Select(ad => new AdDto
        {
            Id = ad.Id,
            Title = ad.Title,
            Description = ad.Description,
            Price = ad.Price,
            ImageUrl = ad.ImageUrl,
            isActive = ad.isActive,
            UserName = ad.AppUsers.UserName,
            location = ad.location,
            categoryTitle=ad.Category.Title,
            CreatedAt = ad.CreatedAt,
            PhonNumber = ad.AppUsers.PhoneNumber,
            adFieldDtos = ad.FieldValues.Select(adField => new AdFieldDto
            {
                DynamicFieldId = adField.DynamicFieldId,
                Value = adField.Value,
            }).ToList()

        }).ToList();
    }
}