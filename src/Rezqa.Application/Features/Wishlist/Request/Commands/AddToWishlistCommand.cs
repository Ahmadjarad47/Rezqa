using MediatR;
using Rezqa.Application.Features.Wishlist.Dtos;

namespace Rezqa.Application.Features.Wishlist.Request.Commands;

public record AddToWishlistCommand(string UserId, AddToWishlistDto Request) : IRequest<WishlistResponseDto>; 