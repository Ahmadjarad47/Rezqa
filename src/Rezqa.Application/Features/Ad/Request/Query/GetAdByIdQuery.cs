using MediatR;
using Rezqa.Application.Features.Ad.Dtos;

namespace Rezqa.Application.Features.Ad.Request.Query;

public record GetAdByIdQuery(int Id) : IRequest<AdResponseDto>; 