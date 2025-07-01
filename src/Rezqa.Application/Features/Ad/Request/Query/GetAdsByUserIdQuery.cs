using MediatR;
using Rezqa.Application.Features.Ad.Dtos;

namespace Rezqa.Application.Features.Ad.Request.Query;

public record GetAdsByUserIdQuery(Guid UserId) : IRequest<List<AdDto>>; 