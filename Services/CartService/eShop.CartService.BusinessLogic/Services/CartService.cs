using eShop.CartService.BusinessLogic.Infrastructure;
using eShop.CartService.BusinessLogic.Interfaces;
using eShop.CartService.Models;

namespace eShop.CartService.BusinessLogic.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICartRepository _cartRepository;

        public CartService(IUnitOfWork unitOfWork, ICartRepository cartRepository)
        {
            _unitOfWork = unitOfWork;
            _cartRepository = cartRepository;
        }

        public async Task<CartModel> GetOrCreateCartAsync(Guid customerUid)
        {
            var cart = await _cartRepository.FindByCustomerUid(customerUid);
            if (cart is not null)
            {
                return cart;
            }

            var newCart = new CartModel
            {
                f_customer_uid = customerUid
            };

            return await _cartRepository.AddCart(newCart);
        }

        public async Task<IEnumerable<CartItemModel>> GetCartItemsAsync(Guid customerUid)
        {
            var cart = await GetOrCreateCartAsync(customerUid);
            return await _cartRepository.GetItems(cart.f_uid);
        }

        public async Task<bool> AddItemToCartAsync(Guid customerUid, Guid productUid, int quantity, decimal priceSnapshot)
        {
            if (quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than zero.");
            }

            await _unitOfWork.BeginAsync();

            try
            {
                var cart = await GetOrCreateCartAsync(customerUid);
                var existingItem = await _cartRepository.FindItemByProduct(cart.f_uid, productUid);

                if (existingItem is not null)
                {
                    existingItem.f_quantity += quantity;
                    existingItem.f_price_snapshot = priceSnapshot;
                    await _cartRepository.UpdateItem(existingItem);
                }
                else
                {
                    var newItem = new CartItemModel
                    {
                        f_cart_uid = cart.f_uid,
                        f_product_uid = productUid,
                        f_quantity = quantity,
                        f_price_snapshot = priceSnapshot
                    };
                    await _cartRepository.AddItem(newItem);
                }

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateCartItemQuantityAsync(Guid cartItemUid, int quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than zero.");
            }

            var item = await _cartRepository.FindItem(cartItemUid);
            if (item is null)
            {
                return false;
            }

            item.f_quantity = quantity;
            await _cartRepository.UpdateItem(item);
            return true;
        }

        public async Task<bool> RemoveItemFromCartAsync(Guid cartItemUid)
        {
            var item = await _cartRepository.FindItem(cartItemUid);
            if (item is null)
            {
                return false;
            }

            await _cartRepository.RemoveItem(item);
            return true;
        }

        public async Task<bool> ClearCartAsync(Guid customerUid)
        {
            var cart = await _cartRepository.FindByCustomerUid(customerUid);
            if (cart is null)
            {
                return false;
            }

            await _cartRepository.ClearItems(cart.f_uid);
            return true;
        }
    }
}