using MediatR;
using Rezqa.Application.Features.Wishlist.Dtos;

namespace Rezqa.Application.Features.Wishlist.Request.Commands;

public record RemoveFromWishlistCommand(string UserId, RemoveFromWishlistDto Request) : IRequest<WishlistResponseDto>; 