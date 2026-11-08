using eShop.CartService.BusinessLogic.Infrastructure;
using eShop.CartService.BusinessLogic.Interfaces;
using eShop.CartService.Models;
using Moq;
using CartServiceImpl = eShop.CartService.BusinessLogic.Services.CartService;

namespace eShop.CartService.Tests.CartService
{
    public class CartService_Tests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICartRepository> _mockCartRepository;
        private readonly CartServiceImpl _cartService;

        public CartService_Tests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCartRepository = new Mock<ICartRepository>();

            _cartService = new CartServiceImpl(
                _mockUnitOfWork.Object,
                _mockCartRepository.Object
            );
        }

        [Fact]
        public async Task GetOrCreateCartAsync_ShouldReturnExistingCart_WhenCartExists()
        {
            // Arrange
            var customerUid = Guid.NewGuid();
            var existingCart = new CartModel { f_uid = Guid.NewGuid(), f_customer_uid = customerUid };

            _mockCartRepository
                .Setup(r => r.FindByCustomerUid(customerUid))
                .ReturnsAsync(existingCart);

            // Act
            var result = await _cartService.GetOrCreateCartAsync(customerUid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingCart.f_uid, result.f_uid);
            _mockCartRepository.Verify(r => r.AddCart(It.IsAny<CartModel>()), Times.Never);
        }

        [Fact]
        public async Task GetOrCreateCartAsync_ShouldCreateAndReturnNewCart_WhenCartDoesNotExist()
        {
            // Arrange
            var customerUid = Guid.NewGuid();
            var createdCart = new CartModel { f_uid = Guid.NewGuid(), f_customer_uid = customerUid };

            _mockCartRepository
                .Setup(r => r.FindByCustomerUid(customerUid))
                .ReturnsAsync((CartModel?)null);

            _mockCartRepository
                .Setup(r => r.AddCart(It.Is<CartModel>(c => c.f_customer_uid == customerUid)))
                .ReturnsAsync(createdCart);

            // Act
            var result = await _cartService.GetOrCreateCartAsync(customerUid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdCart.f_uid, result.f_uid);
            _mockCartRepository.Verify(r => r.AddCart(It.IsAny<CartModel>()), Times.Once);
        }

        [Fact]
        public async Task GetCartItemsAsync_ShouldReturnCollectionOfItems_WhenCalled()
        {
            // Arrange
            var customerUid = Guid.NewGuid();
            var cart = new CartModel { f_uid = Guid.NewGuid(), f_customer_uid = customerUid };
            var itemsList = new List<CartItemModel>
            {
                new CartItemModel { f_cart_uid = cart.f_uid, f_product_uid = Guid.NewGuid(), f_quantity = 2 },
                new CartItemModel { f_cart_uid = cart.f_uid, f_product_uid = Guid.NewGuid(), f_quantity = 1 }
            };

            _mockCartRepository
                .Setup(r => r.FindByCustomerUid(customerUid))
                .ReturnsAsync(cart);

            _mockCartRepository
                .Setup(r => r.GetItems(cart.f_uid))
                .ReturnsAsync(itemsList);

            // Act
            var result = await _cartService.GetCartItemsAsync(customerUid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(itemsList, result);
        }

        [Fact]
        public async Task AddItemToCartAsync_ShouldThrowArgumentException_WhenQuantityIsZeroOrNegative()
        {
            // Arrange
            var customerUid = Guid.NewGuid();
            var productUid = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _cartService.AddItemToCartAsync(customerUid, productUid, 0, 99.99m));
            Assert.Contains("Quantity must be greater than zero", exception.Message);
        }

        [Fact]
        public async Task AddItemToCartAsync_ShouldUpdateQuantity_WhenItemAlreadyExistsInCart()
        {
            // Arrange
            var customerUid = Guid.NewGuid();
            var productUid = Guid.NewGuid();
            var cart = new CartModel { f_uid = Guid.NewGuid(), f_customer_uid = customerUid };
            var existingItem = new CartItemModel { f_cart_uid = cart.f_uid, f_product_uid = productUid, f_quantity = 2, f_price_snapshot = 50.00m };

            _mockCartRepository
                .Setup(r => r.FindByCustomerUid(customerUid))
                .ReturnsAsync(cart);

            _mockCartRepository
                .Setup(r => r.FindItemByProduct(cart.f_uid, productUid))
                .ReturnsAsync(existingItem);

            // Act
            var result = await _cartService.AddItemToCartAsync(customerUid, productUid, 3, 55.00m);

            // Assert
            Assert.True(result);
            Assert.Equal(5, existingItem.f_quantity);
            Assert.Equal(55.00m, existingItem.f_price_snapshot);
            _mockUnitOfWork.Verify(u => u.BeginAsync(), Times.Once);
            _mockCartRepository.Verify(r => r.UpdateItem(existingItem), Times.Once);
            _mockCartRepository.Verify(r => r.AddItem(It.IsAny<CartItemModel>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task AddItemToCartAsync_ShouldAddNewItem_WhenItemDoesNotExistInCart()
        {
            // Arrange
            var customerUid = Guid.NewGuid();
            var productUid = Guid.NewGuid();
            var cart = new CartModel { f_uid = Guid.NewGuid(), f_customer_uid = customerUid };

            _mockCartRepository
                .Setup(r => r.FindByCustomerUid(customerUid))
                .ReturnsAsync(cart);

            _mockCartRepository
                .Setup(r => r.FindItemByProduct(cart.f_uid, productUid))
                .ReturnsAsync((CartItemModel?)null);

            // Act
            var result = await _cartService.AddItemToCartAsync(customerUid, productUid, 2, 45.00m);

            // Assert
            Assert.True(result);
            _mockUnitOfWork.Verify(u => u.BeginAsync(), Times.Once);
            _mockCartRepository.Verify(r => r.AddItem(It.Is<CartItemModel>(i =>
                i.f_cart_uid == cart.f_uid &&
                i.f_product_uid == productUid &&
                i.f_quantity == 2 &&
                i.f_price_snapshot == 45.00m
            )), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCartItemQuantityAsync_ShouldReturnFalse_WhenItemDoesNotExist()
        {
            // Arrange
            var cartItemUid = Guid.NewGuid();

            _mockCartRepository
                .Setup(r => r.FindItem(cartItemUid))
                .ReturnsAsync((CartItemModel?)null);

            // Act
            var result = await _cartService.UpdateCartItemQuantityAsync(cartItemUid, 5);

            // Assert
            Assert.False(result);
            _mockCartRepository.Verify(r => r.UpdateItem(It.IsAny<CartItemModel>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCartItemQuantityAsync_ShouldUpdateQuantity_WhenItemExists()
        {
            // Arrange
            var cartItemUid = Guid.NewGuid();
            var existingItem = new CartItemModel { f_uid = cartItemUid, f_quantity = 1 };

            _mockCartRepository
                .Setup(r => r.FindItem(cartItemUid))
                .ReturnsAsync(existingItem);

            // Act
            var result = await _cartService.UpdateCartItemQuantityAsync(cartItemUid, 10);

            // Assert
            Assert.True(result);
            Assert.Equal(10, existingItem.f_quantity);
            _mockCartRepository.Verify(r => r.UpdateItem(existingItem), Times.Once);
        }

        [Fact]
        public async Task RemoveItemFromCartAsync_ShouldReturnFalse_WhenItemDoesNotExist()
        {
            // Arrange
            var cartItemUid = Guid.NewGuid();

            _mockCartRepository
                .Setup(r => r.FindItem(cartItemUid))
                .ReturnsAsync((CartItemModel?)null);

            // Act
            var result = await _cartService.RemoveItemFromCartAsync(cartItemUid);

            // Assert
            Assert.False(result);
            _mockCartRepository.Verify(r => r.RemoveItem(It.IsAny<CartItemModel>()), Times.Never);
        }

        [Fact]
        public async Task RemoveItemFromCartAsync_ShouldReturnTrueAndRemoveItem_WhenItemExists()
        {
            // Arrange
            var cartItemUid = Guid.NewGuid();
            var existingItem = new CartItemModel { f_uid = cartItemUid };

            _mockCartRepository
                .Setup(r => r.FindItem(cartItemUid))
                .ReturnsAsync(existingItem);

            // Act
            var result = await _cartService.RemoveItemFromCartAsync(cartItemUid);

            // Assert
            Assert.True(result);
            _mockCartRepository.Verify(r => r.RemoveItem(existingItem), Times.Once);
        }

        [Fact]
        public async Task ClearCartAsync_ShouldReturnFalse_WhenCartDoesNotExistForCustomer()
        {
            // Arrange
            var customerUid = Guid.NewGuid();

            _mockCartRepository
                .Setup(r => r.FindByCustomerUid(customerUid))
                .ReturnsAsync((CartModel?)null);

            // Act
            var result = await _cartService.ClearCartAsync(customerUid);

            // Assert
            Assert.False(result);
            _mockCartRepository.Verify(r => r.ClearItems(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ClearCartAsync_ShouldReturnTrueAndClearItems_WhenCartExists()
        {
            // Arrange
            var customerUid = Guid.NewGuid();
            var cart = new CartModel { f_uid = Guid.NewGuid(), f_customer_uid = customerUid };
            _mockCartRepository
            .Setup(r => r.FindByCustomerUid(customerUid))
            .ReturnsAsync(cart);
            // Act
            var result = await _cartService.ClearCartAsync(customerUid);
            // Assert
            Assert.True(result);
            _mockCartRepository.Verify(r => r.ClearItems(cart.f_uid), Times.Once);
        }
    }
}