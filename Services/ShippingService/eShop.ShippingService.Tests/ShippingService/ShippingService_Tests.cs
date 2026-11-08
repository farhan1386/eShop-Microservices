using eShop.ShippingService.BusinessLogic.Infrastructure;
using eShop.ShippingService.BusinessLogic.Interfaces;
using eShop.ShippingService.Models;
using Moq;
using ShippingServiceImpl = eShop.ShippingService.BusinessLogic.Services.ShippingService;

namespace eShop.ShippingService.Tests.ShippingService
{
    public class ShippingService_Tests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IShippingRepository> _mockShippingRepository;
        private readonly ShippingServiceImpl _shippingService;

        public ShippingService_Tests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockShippingRepository = new Mock<IShippingRepository>();

            _shippingService = new ShippingServiceImpl(
                _mockUnitOfWork.Object,
                _mockShippingRepository.Object
            );
        }

        [Fact]
        public async Task GetAllManifestsAsync_ShouldReturnCollection_WhenCalled()
        {
            // Arrange
            var mockManifests = new List<ShippingModel>
            {
                new ShippingModel { f_carrier_name = "DHL" },
                new ShippingModel { f_carrier_name = "FedEx" }
            };

            _mockShippingRepository
                .Setup(r => r.Get())
                .ReturnsAsync(mockManifests);

            // Act
            var result = await _shippingService.GetAllManifestsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockManifests, result);
        }

        [Fact]
        public async Task GetManifestAsync_ShouldReturnManifest_WhenManifestExists()
        {
            // Arrange
            var manifestUid = Guid.NewGuid();
            var mockManifest = new ShippingModel { f_uid = manifestUid, f_carrier_name = "DHL" };

            _mockShippingRepository
                .Setup(r => r.Find(manifestUid))
                .ReturnsAsync(mockManifest);

            // Act
            var result = await _shippingService.GetManifestAsync(manifestUid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(manifestUid, result.f_uid);
        }

        [Fact]
        public async Task GetManifestByOrderAsync_ShouldReturnManifest_WhenManifestExistsForOrder()
        {
            // Arrange
            var orderUid = Guid.NewGuid();
            var mockManifest = new ShippingModel { f_order_uid = orderUid, f_carrier_name = "FedEx" };

            _mockShippingRepository
                .Setup(r => r.FindByOrderUid(orderUid))
                .ReturnsAsync(mockManifest);

            // Act
            var result = await _shippingService.GetManifestByOrderAsync(orderUid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderUid, result.f_order_uid);
        }

        [Fact]
        public async Task InitiateShippingAsync_ShouldThrowException_WhenOrderIsAlreadyTracked()
        {
            // Arrange
            var manifest = new ShippingModel { f_order_uid = Guid.NewGuid() };
            var initialHistory = new ShippingHistoryModel();

            _mockShippingRepository
                .Setup(r => r.IsOrderTracked(manifest.f_order_uid))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _shippingService.InitiateShippingAsync(manifest, initialHistory));
            Assert.Contains("already has a shipping manifest setup", exception.Message);
        }

        [Fact]
        public async Task InitiateShippingAsync_ShouldSucceedAndCommit_WhenOrderIsNotTracked()
        {
            // Arrange
            var manifest = new ShippingModel { f_order_uid = Guid.NewGuid() };
            var initialHistory = new ShippingHistoryModel();

            _mockShippingRepository
                .Setup(r => r.IsOrderTracked(manifest.f_order_uid))
                .ReturnsAsync(false);

            _mockShippingRepository
                .Setup(r => r.Add(manifest))
                .ReturnsAsync(manifest);

            _mockShippingRepository
                .Setup(r => r.AddHistory(initialHistory))
                .ReturnsAsync(initialHistory);

            // Act
            var result = await _shippingService.InitiateShippingAsync(manifest, initialHistory);

            // Assert
            Assert.True(result);
            _mockUnitOfWork.Verify(u => u.BeginAsync(), Times.Once);
            _mockShippingRepository.Verify(r => r.Add(manifest), Times.Once);
            _mockShippingRepository.Verify(r => r.AddHistory(initialHistory), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateTrackingStateAsync_ShouldReturnFalse_WhenManifestDoesNotExist()
        {
            // Arrange
            var manifestUid = Guid.NewGuid();

            _mockShippingRepository
                .Setup(r => r.Find(manifestUid))
                .ReturnsAsync((ShippingModel?)null);

            // Act
            var result = await _shippingService.UpdateTrackingStateAsync(manifestUid, "InTransit", "Hub 1", "Left facility", "TRK123", DateTime.UtcNow.AddDays(2));

            // Assert
            Assert.False(result);
            _mockUnitOfWork.Verify(u => u.BeginAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateTrackingStateAsync_ShouldUpdateManifestAndAddHistory_WhenManifestExists()
        {
            // Arrange
            var manifestUid = Guid.NewGuid();
            var manifest = new ShippingModel { f_uid = manifestUid, f_shipping_status = "Pending" };

            _mockShippingRepository
                .Setup(r => r.Find(manifestUid))
                .ReturnsAsync(manifest);

            // Act
            var result = await _shippingService.UpdateTrackingStateAsync(manifestUid, "InTransit", "Hub 1", "Left facility", "TRK123", DateTime.UtcNow.AddDays(2));

            // Assert
            Assert.True(result);
            Assert.Equal("InTransit", manifest.f_shipping_status);
            Assert.Equal("TRK123", manifest.f_tracking_number);
            _mockUnitOfWork.Verify(u => u.BeginAsync(), Times.Once);
            _mockShippingRepository.Verify(r => r.Update(manifest), Times.Once);
            _mockShippingRepository.Verify(r => r.AddHistory(It.Is<ShippingHistoryModel>(h =>
                h.f_shipping_manifest_uid == manifestUid &&
                h.f_old_status == "Pending" &&
                h.f_new_status == "InTransit" &&
                h.f_status_location == "Hub 1" &&
                h.f_comment == "Left facility"
            )), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task GetTrackingHistoryAsync_ShouldReturnHistoryCollection_WhenCalled()
        {
            // Arrange
            var manifestUid = Guid.NewGuid();
            var mockHistory = new List<ShippingHistoryModel>
            {
                new ShippingHistoryModel { f_shipping_manifest_uid = manifestUid, f_new_status = "Pending" },
                new ShippingHistoryModel { f_shipping_manifest_uid = manifestUid, f_new_status = "InTransit" }
            };

            _mockShippingRepository
                .Setup(r => r.GetHistory(manifestUid))
                .ReturnsAsync(mockHistory);

            // Act
            var result = await _shippingService.GetTrackingHistoryAsync(manifestUid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockHistory, result);
        }
    }
}