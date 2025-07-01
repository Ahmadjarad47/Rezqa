using MediatR;
using Rezqa.Application.Features.Ad.Dtos;

namespace Rezqa.Application.Features.Ad.Request.Commands;

public record DeleteAdCommand(int Id,string userId) : IRequest<AdResponseDto>;