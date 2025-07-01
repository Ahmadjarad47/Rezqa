using MediatR;
using Rezqa.Application.Features.Ad.Dtos;

namespace Rezqa.Application.Features.Ad.Request.Commands;

public record UpdateAdCommand(UpdateAdDto Request) : IRequest<AdResponseDto>; 