using eShop.Contracts.Enums;
using eShop.OrderService.BusinessLogic.Infrastructure;
using eShop.OrderService.BusinessLogic.Interfaces;
using eShop.OrderService.Models;
using Moq;

namespace eShop.OrderService.Tests.OrderServices
{
    public class OrderService_Tests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IOrderItemRepository> _mockOrderItemRepository;
        private readonly Mock<IOrderHistoryRepository> _mockOrderHistoryRepository;
        private readonly Mock<IOutboxEventRepository> _mockOutboxEventRepository;
        private readonly BusinessLogic.Services.OrderService _sut;

        public OrderService_Tests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockOrderItemRepository = new Mock<IOrderItemRepository>();
            _mockOrderHistoryRepository = new Mock<IOrderHistoryRepository>();
            _mockOutboxEventRepository = new Mock<IOutboxEventRepository>();

            _sut = new BusinessLogic.Services.OrderService(
                _mockUnitOfWork.Object,
                _mockOrderRepository.Object,
                _mockOrderItemRepository.Object,
                _mockOrderHistoryRepository.Object,
                _mockOutboxEventRepository.Object
            );
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllOrders_WhenOrdersExist()
        {
            // Arrange
            var orders = new List<OrderModel>
            {
                new() { f_uid = Guid.NewGuid(), f_customer_uid = Guid.NewGuid(), f_total_price = 150.00m, f_status = OrderStatus.Pending },
                new() { f_uid = Guid.NewGuid(), f_customer_uid = Guid.NewGuid(), f_total_price = 299.99m, f_status = OrderStatus.Paid }
            };

            _mockOrderRepository
                .Setup(r => r.Get())
                .ReturnsAsync(orders);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockOrderRepository.Verify(r => r.Get(), Times.Once);
        }

        [Fact]
        public async Task GetByUidAsync_ShouldReturnOrder_WhenOrderExists()
        {
            // Arrange
            var orderUid = Guid.NewGuid();
            var expectedOrder = new OrderModel { f_uid = orderUid, f_total_price = 79.99m, f_status = OrderStatus.Pending };

            _mockOrderRepository
                .Setup(r => r.Find(orderUid))
                .ReturnsAsync(expectedOrder);

            // Act
            var result = await _sut.GetByUidAsync(orderUid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderUid, result.f_uid);
            Assert.Equal(79.99m, result.f_total_price);
        }

        [Fact]
        public async Task GetByUidAsync_ShouldReturnNull_WhenOrderDoesNotExist()
        {
            // Arrange
            _mockOrderRepository
                .Setup(r => r.Find(It.IsAny<Guid>()))
                .ReturnsAsync((OrderModel?)null);

            // Act
            var result = await _sut.GetByUidAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByCustomerAsync_ShouldReturnCustomerOrders_WhenOrdersExist()
        {
            // Arrange
            var customerUid = Guid.NewGuid();
            var orders = new List<OrderModel>
            {
                new() { f_uid = Guid.NewGuid(), f_customer_uid = customerUid, f_total_price = 100.00m }
            };

            _mockOrderRepository
                .Setup(r => r.GetByCustomer(customerUid))
                .ReturnsAsync(orders);

            // Act
            var result = await _sut.GetByCustomerAsync(customerUid);

            // Assert
            Assert.Single(result);
            _mockOrderRepository.Verify(r => r.GetByCustomer(customerUid), Times.Once);
        }

        [Fact]
        public async Task PlaceOrderAsync_ShouldThrowInvalidOperationException_WhenItemsListIsEmpty()
        {
            // Arrange
            var order = new OrderModel { f_customer_uid = Guid.NewGuid() };
            var emptyItems = Enumerable.Empty<OrderItemModel>();

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.PlaceOrderAsync(order, emptyItems));

            // Assert
            Assert.Equal("An order must contain at least one item.", exception.Message);
            _mockUnitOfWork.Verify(u => u.BeginAsync(), Times.Never);
        }

        [Fact]
        public async Task PlaceOrderAsync_ShouldCalculateTotalSaveOrderItemsAndCommit_WhenValid()
        {
            // Arrange
            var customerUid = Guid.NewGuid();
            var generatedOrderUid = Guid.NewGuid();
            var order = new OrderModel { f_customer_uid = customerUid };
            var items = new List<OrderItemModel>
            {
                new() { f_product_uid = Guid.NewGuid(), f_quantity = 2, f_unit_price = 50.00m },
                new() { f_product_uid = Guid.NewGuid(), f_quantity = 1, f_unit_price = 25.50m }
            };

            _mockOrderRepository
                .Setup(r => r.Add(It.IsAny<OrderModel>()))
                .ReturnsAsync((OrderModel o) =>
                {
                    o.f_uid = generatedOrderUid;
                    return o;
                });

            _mockOrderItemRepository
                .Setup(r => r.Add(It.IsAny<OrderItemModel>()))
                .ReturnsAsync((OrderItemModel item) => item);

            _mockOrderHistoryRepository
                .Setup(r => r.Add(It.IsAny<OrderHistoryModel>()))
                .ReturnsAsync((OrderHistoryModel h) => h);

            _mockOutboxEventRepository
                .Setup(r => r.Add(It.IsAny<OutboxEventModel>()))
                .ReturnsAsync((OutboxEventModel o) => o);

            // Act
            var result = await _sut.PlaceOrderAsync(order, items);

            // Assert
            Assert.Equal(generatedOrderUid, result.f_uid);
            Assert.Equal(125.50m, result.f_total_price);
            Assert.Equal(OrderStatus.Pending, result.f_status);
            Assert.All(items, i => Assert.Equal(generatedOrderUid, i.f_order_uid));

            _mockOrderItemRepository.Verify(r => r.Add(It.IsAny<OrderItemModel>()), Times.Exactly(2));
            _mockOrderHistoryRepository.Verify(r => r.Add(It.Is<OrderHistoryModel>(h =>
                h.f_order_uid == generatedOrderUid &&
                h.f_new_status == OrderStatus.Pending.ToString()
            )), Times.Once);
            _mockOutboxEventRepository.Verify(r => r.Add(It.Is<OutboxEventModel>(o =>
                o.f_event_type == "OrderSubmittedEvent"
            )), Times.Once);

            _mockUnitOfWork.Verify(u => u.BeginAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Never);
        }

        [Fact]
        public async Task PlaceOrderAsync_ShouldRollbackAndRethrow_WhenOrderItemInsertFails()
        {
            // Arrange
            var order = new OrderModel { f_customer_uid = Guid.NewGuid() };
            var items = new List<OrderItemModel>
            {
                new() { f_product_uid = Guid.NewGuid(), f_quantity = 1, f_unit_price = 99.99m }
            };

            _mockOrderRepository
                .Setup(r => r.Add(It.IsAny<OrderModel>()))
                .ReturnsAsync((OrderModel o) =>
                {
                    o.f_uid = Guid.NewGuid();
                    return o;
                });

            _mockOrderItemRepository
                .Setup(r => r.Add(It.IsAny<OrderItemModel>()))
                .ThrowsAsync(new Exception("Database insert failed"));

            // Act
            await Assert.ThrowsAsync<Exception>(() =>
                _sut.PlaceOrderAsync(order, items));

            // Assert
            _mockUnitOfWork.Verify(u => u.BeginAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldUpdateStatusLogHistoryAndCommit_WhenOrderExists()
        {
            // Arrange
            var orderUid = Guid.NewGuid();
            var existingOrder = new OrderModel
            {
                f_uid = orderUid,
                f_status = OrderStatus.Pending
            };

            _mockOrderRepository
                .Setup(r => r.Find(orderUid))
                .ReturnsAsync(existingOrder);

            _mockOrderRepository
                .Setup(r => r.Update(It.IsAny<OrderModel>()))
                .ReturnsAsync((OrderModel o) => o);

            _mockOrderHistoryRepository
                .Setup(r => r.Add(It.IsAny<OrderHistoryModel>()))
                .ReturnsAsync((OrderHistoryModel h) => h);

            _mockOutboxEventRepository
                .Setup(r => r.Add(It.IsAny<OutboxEventModel>()))
                .ReturnsAsync((OutboxEventModel o) => o);

            // Act
            var result = await _sut.UpdateStatusAsync(orderUid, OrderStatus.Paid, "Payment verified");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OrderStatus.Paid, result.f_status);
            _mockOrderHistoryRepository.Verify(r => r.Add(It.Is<OrderHistoryModel>(h =>
                h.f_order_uid == orderUid &&
                h.f_old_status == OrderStatus.Pending.ToString() &&
                h.f_new_status == OrderStatus.Paid.ToString() &&
                h.f_comment == "Payment verified"
            )), Times.Once);
            _mockOutboxEventRepository.Verify(r => r.Add(It.Is<OutboxEventModel>(o =>
                o.f_event_type == "OrderPaidEvent"
            )), Times.Once);
            _mockUnitOfWork.Verify(u => u.BeginAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldReturnNull_WhenOrderDoesNotExist()
        {
            // Arrange
            _mockOrderRepository
                .Setup(r => r.Find(It.IsAny<Guid>()))
                .ReturnsAsync((OrderModel?)null);

            // Act
            var result = await _sut.UpdateStatusAsync(Guid.NewGuid(), OrderStatus.Paid);

            // Assert
            Assert.Null(result);
            _mockUnitOfWork.Verify(u => u.BeginAsync(), Times.Never);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldCancelLogHistorySoftDeleteAndCommit_WhenOrderExists()
        {
            // Arrange
            var orderUid = Guid.NewGuid();
            var existingOrder = new OrderModel
            {
                f_uid = orderUid,
                f_status = OrderStatus.Pending
            };

            _mockOrderRepository
                .Setup(r => r.Find(orderUid))
                .ReturnsAsync(existingOrder);

            _mockOrderRepository
                .Setup(r => r.Update(It.IsAny<OrderModel>()))
                .ReturnsAsync((OrderModel o) => o);

            _mockOrderRepository
                .Setup(r => r.Remove(existingOrder))
                .ReturnsAsync(1);

            _mockOrderHistoryRepository
                .Setup(r => r.Add(It.IsAny<OrderHistoryModel>()))
                .ReturnsAsync((OrderHistoryModel h) => h);

            // Act
            var result = await _sut.CancelOrderAsync(orderUid, "Customer requested cancellation");

            // Assert
            Assert.Equal(1, result);
            Assert.Equal(OrderStatus.Cancelled, existingOrder.f_status);
            _mockOrderHistoryRepository.Verify(r => r.Add(It.Is<OrderHistoryModel>(h =>
                h.f_order_uid == orderUid &&
                h.f_old_status == OrderStatus.Pending.ToString() &&
                h.f_new_status == OrderStatus.Cancelled.ToString() &&
                h.f_comment == "Customer requested cancellation"
            )), Times.Once);
            _mockUnitOfWork.Verify(u => u.BeginAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Never);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldReturnZero_WhenOrderDoesNotExist()
        {
            // Arrange
            _mockOrderRepository
                .Setup(r => r.Find(It.IsAny<Guid>()))
                .ReturnsAsync((OrderModel?)null);

            // Act
            var result = await _sut.CancelOrderAsync(Guid.NewGuid());

            // Assert
            Assert.Equal(0, result);
            _mockUnitOfWork.Verify(u => u.BeginAsync(), Times.Never);
        }
    }
}