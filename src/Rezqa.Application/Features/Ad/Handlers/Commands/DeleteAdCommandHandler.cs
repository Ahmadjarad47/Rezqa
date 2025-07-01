using MediatR;
using Rezqa.Application.Features.Ad.Dtos;
using Rezqa.Application.Features.Ad.Request.Commands;
using Rezqa.Application.Interfaces;

namespace Rezqa.Application.Features.Ad.Handlers.Commands;

public class DeleteAdCommandHandler : IRequestHandler<DeleteAdCommand, AdResponseDto>
{
    private readonly IAdRepository _adRepository;

    public DeleteAdCommandHandler(IAdRepository adRepository)
    {
        _adRepository = adRepository;
    }

    public async Task<AdResponseDto> Handle(DeleteAdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingAd = await _adRepository.GetByIdAsync(request.Id);
            if (existingAd == null || existingAd.UserId.ToString() != request.userId)
            {
                return new AdResponseDto(false, "Ad not found", null, new List<string> { "Ad with the specified ID does not exist" });
            }

            await _adRepository.DeleteAsync(existingAd);

            return new AdResponseDto(true, "Ad deleted successfully", null);
        }
        catch (Exception ex)
        {
            return new AdResponseDto(false, "Failed to delete ad", null, new List<string> { ex.Message });
        }
    }
}