using MediatR;
using Rezqa.Application.Features.Wishlist.Dtos;

namespace Rezqa.Application.Features.Wishlist.Request.Commands;

public record ClearWishlistCommand(string UserId) : IRequest<WishlistResponseDto>; 