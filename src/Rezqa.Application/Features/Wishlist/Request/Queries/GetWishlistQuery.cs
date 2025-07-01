using MediatR;
using Rezqa.Application.Features.Wishlist.Dtos;

namespace Rezqa.Application.Features.Wishlist.Request.Queries;

public record GetWishlistQuery(string UserId) : IRequest<WishlistResponseDto>; 