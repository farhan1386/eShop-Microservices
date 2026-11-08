using eShop.ProductService.BusinessLogic.Interfaces;
using eShop.ProductService.BusinessLogic.Services;
using eShop.ProductService.Models;
using Moq;

namespace eShop.ProductService.Tests.ProductServices
{
    public class ProductCategoryService_Test
    {
        private readonly Mock<IProductCategoryRepository> _mockCategoryRepository;
        private readonly ProductCategoryService _sut;

        public ProductCategoryService_Test()
        {
            _mockCategoryRepository = new Mock<IProductCategoryRepository>();
            _sut = new ProductCategoryService(_mockCategoryRepository.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnCategoryList_WhenCategoriesExist()
        {
            // Arrange
            var categories = new List<ProductCategory>
            {
                new() { f_uid = Guid.NewGuid(), f_name = "Electronics" },
                new() { f_uid = Guid.NewGuid(), f_name = "Books" }
            };

            _mockCategoryRepository
                .Setup(r => r.Get())
                .ReturnsAsync(categories);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockCategoryRepository.Verify(r => r.Get(), Times.Once);
        }

        [Fact]
        public async Task GetByUidAsync_ShouldReturnCategory_WhenCategoryExists()
        {
            // Arrange
            var targetUid = Guid.NewGuid();
            var category = new ProductCategory
            {
                f_uid = targetUid,
                f_name = "Home Appliances"
            };

            _mockCategoryRepository
                .Setup(r => r.Find(targetUid))
                .ReturnsAsync(category);

            // Act
            var result = await _sut.GetByUidAsync(targetUid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(targetUid, result.f_uid);
            Assert.Equal("Home Appliances", result.f_name);
        }

        [Fact]
        public async Task GetByUidAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
        {
            // Arrange
            _mockCategoryRepository
                .Setup(r => r.Find(It.IsAny<Guid>()))
                .ReturnsAsync((ProductCategory?)null);

            // Act
            var result = await _sut.GetByUidAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnCreatedCategory_WhenSuccessful()
        {
            // Arrange
            var newCategory = new ProductCategory { f_name = "New Category" };

            _mockCategoryRepository
                .Setup(r => r.Add(It.IsAny<ProductCategory>()))
                .ReturnsAsync((ProductCategory c) =>
                {
                    c.f_uid = Guid.NewGuid();
                    return c;
                });

            // Act
            var result = await _sut.CreateAsync(newCategory);

            // Assert
            Assert.NotEqual(Guid.Empty, result.f_uid);
            Assert.Equal("New Category", result.f_name);
            _mockCategoryRepository.Verify(r => r.Add(newCategory), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnUpdatedCategory_WhenCategoryExists()
        {
            // Arrange
            var targetUid = Guid.NewGuid();
            var existingCategory = new ProductCategory { f_uid = targetUid, f_name = "Old Name" };
            var updatePayload = new ProductCategory { f_name = "Updated Name" };

            _mockCategoryRepository
                .Setup(r => r.Find(targetUid))
                .ReturnsAsync(existingCategory);

            _mockCategoryRepository
                .Setup(r => r.Update(It.IsAny<ProductCategory>()))
                .ReturnsAsync((ProductCategory c) => c);

            // Act
            var result = await _sut.UpdateAsync(targetUid, updatePayload);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.f_name);
            _mockCategoryRepository.Verify(r => r.Update(existingCategory), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
        {
            // Arrange
            _mockCategoryRepository
                .Setup(r => r.Find(It.IsAny<Guid>()))
                .ReturnsAsync((ProductCategory?)null);

            // Act
            var result = await _sut.UpdateAsync(Guid.NewGuid(), new ProductCategory { f_name = "Ghost Category" });

            // Assert
            Assert.Null(result);
            _mockCategoryRepository.Verify(r => r.Update(It.IsAny<ProductCategory>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnAffectedRowCount_WhenCategoryExists()
        {
            // Arrange
            var targetUid = Guid.NewGuid();
            var existingCategory = new ProductCategory { f_uid = targetUid, f_name = "To Delete" };

            _mockCategoryRepository
                .Setup(r => r.Find(targetUid))
                .ReturnsAsync(existingCategory);

            _mockCategoryRepository
                .Setup(r => r.Remove(existingCategory))
                .ReturnsAsync(1);

            // Act
            var result = await _sut.DeleteAsync(targetUid);

            // Assert
            Assert.Equal(1, result);
            _mockCategoryRepository.Verify(r => r.Remove(existingCategory), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnZero_WhenCategoryDoesNotExist()
        {
            // Arrange
            _mockCategoryRepository
                .Setup(r => r.Find(It.IsAny<Guid>()))
                .ReturnsAsync((ProductCategory?)null);

            // Act
            var result = await _sut.DeleteAsync(Guid.NewGuid());

            // Assert
            Assert.Equal(0, result);
            _mockCategoryRepository.Verify(r => r.Remove(It.IsAny<ProductCategory>()), Times.Never);
        }
    }
}