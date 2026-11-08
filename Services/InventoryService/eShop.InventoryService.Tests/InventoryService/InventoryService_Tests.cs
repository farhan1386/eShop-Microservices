using eShop.InventoryService.BusinessLogic.Interfaces;
using eShop.InventoryService.Models;
using Moq;

namespace eShop.InventoryService.Tests.InventoryService
{
    public class InventoryService_Tests
    {
        private readonly Mock<IInventoryRepository> _mockInventoryRepository;
        private readonly BusinessLogic.Services.InventoryService _sut;

        public InventoryService_Tests()
        {
            _mockInventoryRepository = new Mock<IInventoryRepository>();
            _sut = new BusinessLogic.Services.InventoryService(_mockInventoryRepository.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllStockRecords_WhenRecordsExist()
        {
            // Arrange
            var stocks = new List<InventoryStockModel>
            {
                new() { f_uid = Guid.NewGuid(), f_product_uid = Guid.NewGuid(), f_quantity = 50, f_reserved = 5 },
                new() { f_uid = Guid.NewGuid(), f_product_uid = Guid.NewGuid(), f_quantity = 100, f_reserved = 0 }
            };

            _mockInventoryRepository
                .Setup(r => r.Get())
                .ReturnsAsync(stocks);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockInventoryRepository.Verify(r => r.Get(), Times.Once);
        }

        [Fact]
        public async Task GetByUidAsync_ShouldReturnStock_WhenRecordExists()
        {
            // Arrange
            var uid = Guid.NewGuid();
            var expected = new InventoryStockModel { f_uid = uid, f_product_uid = Guid.NewGuid(), f_quantity = 40, f_reserved = 10 };

            _mockInventoryRepository
                .Setup(r => r.Find(uid))
                .ReturnsAsync(expected);

            // Act
            var result = await _sut.GetByUidAsync(uid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(uid, result.f_uid);
            Assert.Equal(30, result.AvailableQuantity);
        }

        [Fact]
        public async Task GetByProductAsync_ShouldReturnStock_WhenProductStockExists()
        {
            // Arrange
            var productUid = Guid.NewGuid();
            var expected = new InventoryStockModel { f_uid = Guid.NewGuid(), f_product_uid = productUid, f_quantity = 25, f_reserved = 5 };

            _mockInventoryRepository
                .Setup(r => r.FindByProduct(productUid))
                .ReturnsAsync(expected);

            // Act
            var result = await _sut.GetByProductAsync(productUid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productUid, result.f_product_uid);
        }

        [Fact]
        public async Task InitializeStockAsync_ShouldThrowInvalidOperationException_WhenProductAlreadyHasInventory()
        {
            // Arrange
            var productUid = Guid.NewGuid();
            var stock = new InventoryStockModel { f_product_uid = productUid, f_quantity = 20 };

            _mockInventoryRepository
                .Setup(r => r.FindByProduct(productUid))
                .ReturnsAsync(new InventoryStockModel { f_uid = Guid.NewGuid(), f_product_uid = productUid });

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.InitializeStockAsync(stock));

            // Assert
            Assert.Equal($"Inventory record already exists for product '{productUid}'.", exception.Message);
            _mockInventoryRepository.Verify(r => r.Add(It.IsAny<InventoryStockModel>()), Times.Never);
        }

        [Fact]
        public async Task InitializeStockAsync_ShouldThrowInvalidOperationException_WhenQuantityIsNegative()
        {
            // Arrange
            var productUid = Guid.NewGuid();
            var stock = new InventoryStockModel { f_product_uid = productUid, f_quantity = -5 };

            _mockInventoryRepository
                .Setup(r => r.FindByProduct(productUid))
                .ReturnsAsync((InventoryStockModel?)null);

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.InitializeStockAsync(stock));

            // Assert
            Assert.Equal("Initial stock quantity cannot be negative.", exception.Message);
            _mockInventoryRepository.Verify(r => r.Add(It.IsAny<InventoryStockModel>()), Times.Never);
        }

        [Fact]
        public async Task InitializeStockAsync_ShouldCreateRecordWithZeroReserved_WhenValid()
        {
            // Arrange
            var generatedUid = Guid.NewGuid();
            var stock = new InventoryStockModel { f_product_uid = Guid.NewGuid(), f_quantity = 50, f_reserved = 99 };

            _mockInventoryRepository
                .Setup(r => r.FindByProduct(stock.f_product_uid))
                .ReturnsAsync((InventoryStockModel?)null);

            _mockInventoryRepository
                .Setup(r => r.Add(It.IsAny<InventoryStockModel>()))
                .ReturnsAsync((InventoryStockModel s) =>
                {
                    s.f_uid = generatedUid;
                    return s;
                });

            // Act
            var result = await _sut.InitializeStockAsync(stock);

            // Assert
            Assert.Equal(generatedUid, result.f_uid);
            Assert.Equal(50, result.f_quantity);
            Assert.Equal(0, result.f_reserved);
            _mockInventoryRepository.Verify(r => r.Add(stock), Times.Once);
        }

        [Fact]
        public async Task RestockAsync_ShouldIncreaseTotalQuantity_WhenProductExists()
        {
            // Arrange
            var productUid = Guid.NewGuid();
            var existing = new InventoryStockModel { f_uid = Guid.NewGuid(), f_product_uid = productUid, f_quantity = 10, f_reserved = 2 };

            _mockInventoryRepository
                .Setup(r => r.FindByProduct(productUid))
                .ReturnsAsync(existing);

            _mockInventoryRepository
                .Setup(r => r.Update(It.IsAny<InventoryStockModel>()))
                .ReturnsAsync((InventoryStockModel s) => s);

            // Act
            var result = await _sut.RestockAsync(productUid, 15);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(25, result.f_quantity);
            Assert.Equal(23, result.AvailableQuantity);
            _mockInventoryRepository.Verify(r => r.Update(existing), Times.Once);
        }

        [Fact]
        public async Task ReserveStockAsync_ShouldIncreaseReserved_WhenAvailableStockIsSufficient()
        {
            // Arrange
            var productUid = Guid.NewGuid();
            var existing = new InventoryStockModel { f_uid = Guid.NewGuid(), f_product_uid = productUid, f_quantity = 10, f_reserved = 3 };

            _mockInventoryRepository
                .Setup(r => r.FindByProduct(productUid))
                .ReturnsAsync(existing);

            _mockInventoryRepository
                .Setup(r => r.Update(It.IsAny<InventoryStockModel>()))
                .ReturnsAsync((InventoryStockModel s) => s);

            // Act
            var result = await _sut.ReserveStockAsync(productUid, 5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(8, result.f_reserved);
            Assert.Equal(2, result.AvailableQuantity);
        }

        [Fact]
        public async Task ReserveStockAsync_ShouldThrowInvalidOperationException_WhenAvailableStockIsInsufficient()
        {
            // Arrange
            var productUid = Guid.NewGuid();
            var existing = new InventoryStockModel { f_uid = Guid.NewGuid(), f_product_uid = productUid, f_quantity = 10, f_reserved = 8 };

            _mockInventoryRepository
                .Setup(r => r.FindByProduct(productUid))
                .ReturnsAsync(existing);

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.ReserveStockAsync(productUid, 5));

            // Assert
            Assert.Contains("Insufficient available stock", exception.Message);
            _mockInventoryRepository.Verify(r => r.Update(It.IsAny<InventoryStockModel>()), Times.Never);
        }

        [Fact]
        public async Task ReserveStockAsync_ShouldThrowInvalidOperationException_WhenConcurrencyConflictOccurs()
        {
            // Arrange
            var productUid = Guid.NewGuid();
            var existing = new InventoryStockModel { f_uid = Guid.NewGuid(), f_product_uid = productUid, f_quantity = 10, f_reserved = 0 };

            _mockInventoryRepository
                .Setup(r => r.FindByProduct(productUid))
                .ReturnsAsync(existing);

            _mockInventoryRepository
                .Setup(r => r.Update(It.IsAny<InventoryStockModel>()))
                .ReturnsAsync((InventoryStockModel?)null);

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.ReserveStockAsync(productUid, 2));

            // Assert
            Assert.Equal("Concurrency conflict occurred while reserving stock. Please retry.", exception.Message);
        }

        [Fact]
        public async Task ReleaseReservedStockAsync_ShouldDecreaseReserved_WhenReservedIsSufficient()
        {
            // Arrange
            var productUid = Guid.NewGuid();
            var existing = new InventoryStockModel { f_uid = Guid.NewGuid(), f_product_uid = productUid, f_quantity = 10, f_reserved = 6 };

            _mockInventoryRepository
                .Setup(r => r.FindByProduct(productUid))
                .ReturnsAsync(existing);

            _mockInventoryRepository
                .Setup(r => r.Update(It.IsAny<InventoryStockModel>())).ReturnsAsync((InventoryStockModel s) => s);
            // Act
            var result = await _sut.ReleaseReservedStockAsync(productUid, 4);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.f_reserved);
            Assert.Equal(8, result.AvailableQuantity);
        }
        [Fact]
        public async Task ConfirmDeductStockAsync_ShouldDecreaseBothQuantityAndReserved_WhenValid()
        {
            // Arrange
            var productUid = Guid.NewGuid();
            var existing = new InventoryStockModel { f_uid = Guid.NewGuid(), f_product_uid = productUid, f_quantity = 10, f_reserved = 4 };

            _mockInventoryRepository
                .Setup(r => r.FindByProduct(productUid))
                .ReturnsAsync(existing);

            _mockInventoryRepository
                .Setup(r => r.Update(It.IsAny<InventoryStockModel>()))
                .ReturnsAsync((InventoryStockModel s) => s);

            // Act
            var result = await _sut.ConfirmDeductStockAsync(productUid, 3);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(7, result.f_quantity);
            Assert.Equal(1, result.f_reserved);
            Assert.Equal(6, result.AvailableQuantity);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnAffectedRowCount_WhenStockExists()
        {
            // Arrange
            var uid = Guid.NewGuid();
            var existing = new InventoryStockModel { f_uid = uid, f_quantity = 5 };

            _mockInventoryRepository
                .Setup(r => r.Find(uid))
                .ReturnsAsync(existing);

            _mockInventoryRepository
                .Setup(r => r.Remove(existing))
                .ReturnsAsync(1);

            // Act
            var result = await _sut.DeleteAsync(uid);

            // Assert
            Assert.Equal(1, result);
            _mockInventoryRepository.Verify(r => r.Remove(existing), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnZero_WhenStockDoesNotExist()
        {
            // Arrange
            _mockInventoryRepository
                .Setup(r => r.Find(It.IsAny<Guid>()))
                .ReturnsAsync((InventoryStockModel?)null);

            // Act
            var result = await _sut.DeleteAsync(Guid.NewGuid());

            // Assert
            Assert.Equal(0, result);
            _mockInventoryRepository.Verify(r => r.Remove(It.IsAny<InventoryStockModel>()), Times.Never);
        }
    }
}