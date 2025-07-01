using MediatR;
using Rezqa.Application.Features.Ad.Dtos;
using Rezqa.Application.Features.Ad.Request.Query;
using Rezqa.Application.Interfaces;

namespace Rezqa.Application.Features.Ad.Handlers.Query;

public class GetAdByIdQueryHandler : IRequestHandler<GetAdByIdQuery, AdResponseDto>
{
    private readonly IAdRepository _adRepository;

    public GetAdByIdQueryHandler(IAdRepository adRepository)
    {
        _adRepository = adRepository;
    }

    public async Task<AdResponseDto> Handle(GetAdByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var ad = await _adRepository.GetByIdAsync(request.Id);
            if (ad == null)
            {
                return new AdResponseDto(false, "Ad not found", null, new List<string> { "Ad with the specified ID does not exist" });
            }

            var adDto = new AdDto
            {
                Id = ad.Id,
                Title = ad.Title,
                Description = ad.Description,
                Price = ad.Price,
                isActive = ad.isActive,
                ImageUrl = ad.ImageUrl,
                UserName = ad.AppUsers.UserName,
                location = ad.location,
                CreatedAt = ad.CreatedAt,
                categoryTitle = ad.Category.Title,
                PhonNumber = ad.AppUsers.PhoneNumber,
                adFieldDtos = ad.FieldValues.Select(adField => new AdFieldDto
                {
                    Title = adField.DynamicField.Title,
                    DynamicFieldId = adField.DynamicFieldId,
                    Value = adField.Value,
                }).ToList()
            };

            return new AdResponseDto(true, "Ad retrieved successfully", adDto);
        }
        catch (Exception ex)
        {
            return new AdResponseDto(false, "Failed to retrieve ad", null, new List<string> { ex.Message });
        }
    }
}