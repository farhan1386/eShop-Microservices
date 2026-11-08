using eShop.ProductService.BusinessLogic.Interfaces;
using eShop.ProductService.Models;
using Moq;

namespace eShop.ProductService.Tests.ProductServices
{
    public class ProductService_Test
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly BusinessLogic.Services.ProductService _sut;

        public ProductService_Test()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _sut = new BusinessLogic.Services.ProductService(_mockProductRepository.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnProductList_WhenProductsExist()
        {
            // Arrange
            var products = new List<ProductModel>
            {
                new() { f_uid = Guid.NewGuid(), f_category_uid = Guid.NewGuid(), f_name = "Product 1", f_sku = "SKU001", f_price = 99.99m, f_attributes = "{}" },
                new() { f_uid = Guid.NewGuid(), f_category_uid = Guid.NewGuid(), f_name = "Product 2", f_sku = "SKU002", f_price = 149.50m, f_attributes = "{}" }
            };

            _mockProductRepository
                .Setup(r => r.Get())
                .ReturnsAsync(products);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockProductRepository.Verify(r => r.Get(), Times.Once);
        }

        [Fact]
        public async Task GetByUidAsync_ShouldReturnProduct_WhenProductExists()
        {
            // Arrange
            var targetUid = Guid.NewGuid();
            var product = new ProductModel
            {
                f_uid = targetUid,
                f_category_uid = Guid.NewGuid(),
                f_name = "Target Product",
                f_sku = "SKU999",
                f_price = 19.99m,
                f_attributes = "{}"
            };

            _mockProductRepository
                .Setup(r => r.Find(targetUid))
                .ReturnsAsync(product);

            // Act
            var result = await _sut.GetByUidAsync(targetUid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(targetUid, result.f_uid);
            Assert.Equal("Target Product", result.f_name);
        }

        [Fact]
        public async Task GetByUidAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            _mockProductRepository
                .Setup(r => r.Find(It.IsAny<Guid>()))
                .ReturnsAsync((ProductModel?)null);

            // Act
            var result = await _sut.GetByUidAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetBySkuAsync_ShouldReturnProduct_WhenSkuExists()
        {
            // Arrange
            var product = new ProductModel { f_uid = Guid.NewGuid(), f_name = "Keyboard", f_sku = "KB-001", f_price = 79.99m };

            _mockProductRepository
                .Setup(r => r.FindBySku("KB-001"))
                .ReturnsAsync(product);

            // Act
            var result = await _sut.GetBySkuAsync("KB-001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("KB-001", result.f_sku);
        }

        [Fact]
        public async Task GetByCategoryAsync_ShouldReturnMatchingProducts_WhenCategoryHasProducts()
        {
            // Arrange
            var categoryUid = Guid.NewGuid();
            var products = new List<ProductModel>
            {
                new() { f_uid = Guid.NewGuid(), f_category_uid = categoryUid, f_name = "Mouse", f_sku = "MS-001" }
            };

            _mockProductRepository
                .Setup(r => r.GetByCategory(categoryUid))
                .ReturnsAsync(products);

            // Act
            var result = await _sut.GetByCategoryAsync(categoryUid);

            // Assert
            Assert.Single(result);
            _mockProductRepository.Verify(r => r.GetByCategory(categoryUid), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnCreatedProduct_WhenSkuIsUnique()
        {
            // Arrange
            var newProduct = new ProductModel
            {
                f_category_uid = Guid.NewGuid(),
                f_name = "New Item",
                f_sku = "NEW-SKU",
                f_price = 45.00m,
                f_attributes = "{}"
            };

            _mockProductRepository
                .Setup(r => r.FindBySku(newProduct.f_sku))
                .ReturnsAsync((ProductModel?)null);

            _mockProductRepository
                .Setup(r => r.Add(It.IsAny<ProductModel>()))
                .ReturnsAsync((ProductModel p) =>
                {
                    p.f_uid = Guid.NewGuid();
                    return p;
                });

            // Act
            var result = await _sut.CreateAsync(newProduct);

            // Assert
            Assert.NotEqual(Guid.Empty, result.f_uid);
            Assert.Equal("New Item", result.f_name);
            _mockProductRepository.Verify(r => r.Add(newProduct), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenSkuAlreadyExists()
        {
            // Arrange
            var duplicateProduct = new ProductModel { f_name = "Duplicate", f_sku = "DUP-SKU" };

            _mockProductRepository
                .Setup(r => r.FindBySku("DUP-SKU"))
                .ReturnsAsync(new ProductModel { f_uid = Guid.NewGuid(), f_sku = "DUP-SKU" });

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.CreateAsync(duplicateProduct));

            // Assert
            Assert.Equal("A product with SKU 'DUP-SKU' already exists.", exception.Message);
            _mockProductRepository.Verify(r => r.Add(It.IsAny<ProductModel>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnUpdatedProduct_WhenProductExists()
        {
            // Arrange
            var targetUid = Guid.NewGuid();
            var existingProduct = new ProductModel { f_uid = targetUid, f_name = "Old Name", f_price = 10.00m };
            var updatePayload = new ProductModel
            {
                f_category_uid = Guid.NewGuid(),
                f_name = "Updated Name",
                f_price = 50.00m,
                f_attributes = "{\"color\":\"blue\"}"
            };

            _mockProductRepository
                .Setup(r => r.Find(targetUid))
                .ReturnsAsync(existingProduct);

            _mockProductRepository
                .Setup(r => r.Update(It.IsAny<ProductModel>()))
                .ReturnsAsync((ProductModel p) => p);

            // Act
            var result = await _sut.UpdateAsync(targetUid, updatePayload);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.f_name);
            Assert.Equal(50.00m, result.f_price);
            _mockProductRepository.Verify(r => r.Update(existingProduct), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            _mockProductRepository
                .Setup(r => r.Find(It.IsAny<Guid>()))
                .ReturnsAsync((ProductModel?)null);

            // Act
            var result = await _sut.UpdateAsync(Guid.NewGuid(), new ProductModel { f_name = "Ghost Product" });

            // Assert
            Assert.Null(result);
            _mockProductRepository.Verify(r => r.Update(It.IsAny<ProductModel>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnAffectedRowCount_WhenProductExists()
        {
            // Arrange
            var targetUid = Guid.NewGuid();
            var existingProduct = new ProductModel { f_uid = targetUid, f_name = "To Delete" };

            _mockProductRepository
                .Setup(r => r.Find(targetUid))
                .ReturnsAsync(existingProduct);

            _mockProductRepository
                .Setup(r => r.Remove(existingProduct))
                .ReturnsAsync(1);

            // Act
            var result = await _sut.DeleteAsync(targetUid);

            // Assert
            Assert.Equal(1, result);
            _mockProductRepository.Verify(r => r.Remove(existingProduct), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnZero_WhenProductDoesNotExist()
        {
            // Arrange
            _mockProductRepository
                .Setup(r => r.Find(It.IsAny<Guid>()))
                .ReturnsAsync((ProductModel?)null);

            // Act
            var result = await _sut.DeleteAsync(Guid.NewGuid());

            // Assert
            Assert.Equal(0, result);
            _mockProductRepository.Verify(r => r.Remove(It.IsAny<ProductModel>()), Times.Never);
        }
    }
}