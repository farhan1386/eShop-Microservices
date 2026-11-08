using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eShop.CartService.Models;

namespace eShop.CartService.BusinessLogic.Interfaces
{
    public interface ICartRepository
    {
        Task<CartModel?> FindByCustomerUid(Guid customerUid);
        Task<CartModel> AddCart(CartModel model);

        Task<IEnumerable<CartItemModel>> GetItems(Guid cartUid);
        Task<CartItemModel?> FindItem(Guid uid);
        Task<CartItemModel?> FindItemByProduct(Guid cartUid, Guid productUid);
        Task<CartItemModel> AddItem(CartItemModel model);
        Task<CartItemModel> UpdateItem(CartItemModel model);
        Task<int> RemoveItem(CartItemModel model);
        Task<int> ClearItems(Guid cartUid);
    }
}
