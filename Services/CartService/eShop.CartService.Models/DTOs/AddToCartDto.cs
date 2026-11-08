using System;

namespace eShop.CartService.Models.DTOs
{
    public record AddToCartDto(Guid CustomerUid, Guid ProductUid, int Quantity, decimal PriceSnapshot);

    public record UpdateCartItemDto(Guid CartItemUid, int Quantity);

    public record RemoveFromCartDto(Guid CartItemUid);

    public record ClearCartDto(Guid CustomerUid);
}