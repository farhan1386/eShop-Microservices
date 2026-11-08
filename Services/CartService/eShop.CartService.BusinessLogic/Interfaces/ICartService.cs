using eShop.CartService.Models;

namespace eShop.CartService.BusinessLogic.Interfaces
{
    public interface ICartService
    {
        Task<CartModel> GetOrCreateCartAsync(Guid customerUid);
        Task<IEnumerable<CartItemModel>> GetCartItemsAsync(Guid customerUid);
        Task<bool> AddItemToCartAsync(Guid customerUid, Guid productUid, int quantity, decimal priceSnapshot);
        Task<bool> UpdateCartItemQuantityAsync(Guid cartItemUid, int quantity);
        Task<bool> RemoveItemFromCartAsync(Guid cartItemUid);
        Task<bool> ClearCartAsync(Guid customerUid);
    }
}