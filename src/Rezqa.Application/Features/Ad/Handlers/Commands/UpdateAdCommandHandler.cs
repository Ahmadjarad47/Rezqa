using MediatR;
using Rezqa.Application.Features.Ad.Dtos;
using Rezqa.Application.Features.Ad.Request.Commands;
using Rezqa.Application.Interfaces;
using Rezqa.Domain.Common.Interfaces;

namespace Rezqa.Application.Features.Ad.Handlers.Commands;

public class UpdateAdCommandHandler : IRequestHandler<UpdateAdCommand, AdResponseDto>
{
    private readonly IAdRepository _adRepository;
    private readonly IFileService fileService;
    public UpdateAdCommandHandler(IAdRepository adRepository, IFileService fileService)
    {
        _adRepository = adRepository;
        this.fileService = fileService;
    }

    public async Task<AdResponseDto> Handle(UpdateAdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingAd = await _adRepository.GetByIdAsync(request.Request.Id);
            if (existingAd == null)
            {
                return new AdResponseDto(false, "Ad not found", null, new List<string> { "Ad with the specified ID does not exist" });
            }
            foreach (var item in existingAd.ImageUrl)
            {
                await fileService.DeleteFileAsync(item);
            }
            existingAd.ImageUrl = new string[request.Request.ImageUrl.Count];
            foreach (var item in request.Request.ImageUrl)
            {
                string file = await fileService.SaveFileAsync(item, existingAd.AppUsers.UserName!);
                existingAd.ImageUrl.Append(file);
            }

            // Update the ad properties
            existingAd.Title = request.Request.Title;
            existingAd.Description = request.Request.Description;
            existingAd.Price = request.Request.Price;



            await _adRepository.UpdateAsync(existingAd);

            var adDto = new AdDto
            {
                Id = existingAd.Id,
                Title = existingAd.Title,
                Description = existingAd.Description,
                Price = existingAd.Price,
                ImageUrl = existingAd.ImageUrl,
                UserId = existingAd.UserId,
                UserName = existingAd.AppUsers?.UserName ?? "Unknown",
                CreatedAt = DateTime.UtcNow, // This should come from the entity
                UpdatedAt = DateTime.UtcNow
            };

            return new AdResponseDto(true, "Ad updated successfully", adDto);
        }
        catch (Exception ex)
        {
            return new AdResponseDto(false, "Failed to update ad", null, new List<string> { ex.Message });
        }
    }
}