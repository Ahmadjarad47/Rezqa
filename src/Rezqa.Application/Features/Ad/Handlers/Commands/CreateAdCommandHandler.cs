using MediatR;
using Microsoft.AspNetCore.Identity;
using Rezqa.Application.Features.Ad.Dtos;
using Rezqa.Application.Features.Ad.Request.Commands;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Common.Interfaces;
using Rezqa.Domain.Entities;

namespace Rezqa.Application.Features.Ad.Handlers.Commands;

public class CreateAdCommandHandler : IRequestHandler<CreateAdCommand, AdResponseDto>
{
    private readonly IAdRepository _adRepository;
    private readonly IFileService fileService;
    private readonly UserManager<AppUsers> _userManager;

    public CreateAdCommandHandler(
        IAdRepository adRepository,
        IFileService fileService,
        UserManager<AppUsers> userManager)
    {
        _adRepository = adRepository;
        this.fileService = fileService;
        _userManager = userManager;
    }

    public async Task<AdResponseDto> Handle(CreateAdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify user exists
            var user = await _userManager.FindByIdAsync(request.Request.UserId.ToString());
            if (user == null)
            {
                return new AdResponseDto(false, "User not found", null, new List<string> { "The specified user does not exist" });
            }

            string[] images = new string[request.Request.ImageUrl!.Count];
            int i = 0;
            foreach (var item in request.Request.ImageUrl)
            {
                images[i] = await fileService.SaveFileAsync(item, request.Request.UserId.ToString());
                ++i;
            }
            var ad = new Domain.Entities.Ad
            {
                Title = request.Request.Title,
                Description = request.Request.Description,
                Price = request.Request.Price,
                CategoryId = request.Request.CategoryId,
                SubCategoryId = request.Request.SubCategoryId,
                FieldValues = request.Request.fieldValues.Select(fv => new Domain.Entities.AdFieldValue
                {
                    DynamicFieldId = fv.DynamicFieldId,
                    Value = fv.Value
                }).ToList(),
                location = request.Request.location,
                ImageUrl = images,
                UserId = request.Request.UserId
            };

            var createdAd = await _adRepository.AddAsync(ad);

            var adDto = new AdDto
            {
                Id = createdAd.Id,
                Title = createdAd.Title,
                Description = createdAd.Description,
                Price = createdAd.Price,
                ImageUrl = createdAd.ImageUrl,
                UserName = createdAd.AppUsers?.UserName ?? "Unknown",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            return new AdResponseDto(true, "Ad created successfully", adDto);
        }
        catch (Exception ex)
        {
            return new AdResponseDto(false, "Failed to create ad", null, new List<string> { ex.Message });
        }
    }
}