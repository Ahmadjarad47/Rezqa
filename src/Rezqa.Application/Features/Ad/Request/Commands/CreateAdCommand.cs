using MediatR;
using Rezqa.Application.Features.Ad.Dtos;

namespace Rezqa.Application.Features.Ad.Request.Commands;

public record CreateAdCommand(CreateAdDto Request) : IRequest<AdResponseDto>; 