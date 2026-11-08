using eShop.Contracts.Enums;
using eShop.PaymentService.BusinessLogic.Interfaces;
using eShop.PaymentService.Models;
using Moq;

namespace eShop.PaymentService.Tests.PaymentService
{
    public class PaymentService_Tests
    {
        private readonly Mock<IPaymentRepository> _mockPaymentRepository;
        private readonly BusinessLogic.Services.PaymentService _sut;

        public PaymentService_Tests()
        {
            _mockPaymentRepository = new Mock<IPaymentRepository>();
            _sut = new BusinessLogic.Services.PaymentService(_mockPaymentRepository.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllPayments_WhenPaymentsExist()
        {
            // Arrange
            var payments = new List<PaymentModel>
            {
                new() { f_uid = Guid.NewGuid(), f_order_uid = Guid.NewGuid(), f_amount = 150.00m, f_status = PaymentStatus.Pending },
                new() { f_uid = Guid.NewGuid(), f_order_uid = Guid.NewGuid(), f_amount = 299.99m, f_status = PaymentStatus.Completed }
            };

            _mockPaymentRepository
                .Setup(r => r.Get())
                .ReturnsAsync(payments);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockPaymentRepository.Verify(r => r.Get(), Times.Once);
        }

        [Fact]
        public async Task GetByUidAsync_ShouldReturnPayment_WhenPaymentExists()
        {
            // Arrange
            var paymentUid = Guid.NewGuid();
            var expectedPayment = new PaymentModel
            {
                f_uid = paymentUid,
                f_order_uid = Guid.NewGuid(),
                f_amount = 79.99m,
                f_status = PaymentStatus.Pending
            };

            _mockPaymentRepository
                .Setup(r => r.Find(paymentUid))
                .ReturnsAsync(expectedPayment);

            // Act
            var result = await _sut.GetByUidAsync(paymentUid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(paymentUid, result.f_uid);
            Assert.Equal(79.99m, result.f_amount);
        }

        [Fact]
        public async Task GetByUidAsync_ShouldReturnNull_WhenPaymentDoesNotExist()
        {
            // Arrange
            _mockPaymentRepository
                .Setup(r => r.Find(It.IsAny<Guid>()))
                .ReturnsAsync((PaymentModel?)null);

            // Act
            var result = await _sut.GetByUidAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByOrderAsync_ShouldReturnPayments_WhenOrderHasPayments()
        {
            // Arrange
            var orderUid = Guid.NewGuid();
            var payments = new List<PaymentModel>
            {
                new() { f_uid = Guid.NewGuid(), f_order_uid = orderUid, f_amount = 125.50m }
            };

            _mockPaymentRepository
                .Setup(r => r.GetByOrder(orderUid))
                .ReturnsAsync(payments);

            // Act
            var result = await _sut.GetByOrderAsync(orderUid);

            // Assert
            Assert.Single(result);
            _mockPaymentRepository.Verify(r => r.GetByOrder(orderUid), Times.Once);
        }

        [Fact]
        public async Task InitiatePaymentAsync_ShouldThrowInvalidOperationException_WhenAmountIsZeroOrNegative()
        {
            // Arrange
            var invalidPayment = new PaymentModel
            {
                f_order_uid = Guid.NewGuid(),
                f_amount = 0m,
                f_currency = "INR",
                f_method = "UPI",
                f_gateway_name = "Razorpay"
            };

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.InitiatePaymentAsync(invalidPayment));

            // Assert
            Assert.Equal("Payment amount must be greater than zero.", exception.Message);
            _mockPaymentRepository.Verify(r => r.Add(It.IsAny<PaymentModel>()), Times.Never);
        }

        [Fact]
        public async Task InitiatePaymentAsync_ShouldSetPendingStatusAndSave_WhenAmountIsValid()
        {
            // Arrange
            var generatedUid = Guid.NewGuid();
            var payment = new PaymentModel
            {
                f_order_uid = Guid.NewGuid(),
                f_amount = 499.00m,
                f_currency = "INR",
                f_method = "Card",
                f_gateway_name = "Razorpay"
            };

            _mockPaymentRepository
                .Setup(r => r.Add(It.IsAny<PaymentModel>()))
                .ReturnsAsync((PaymentModel p) =>
                {
                    p.f_uid = generatedUid;
                    return p;
                });

            // Act
            var result = await _sut.InitiatePaymentAsync(payment);

            // Assert
            Assert.Equal(generatedUid, result.f_uid);
            Assert.Equal(PaymentStatus.Pending, result.f_status);
            _mockPaymentRepository.Verify(r => r.Add(payment), Times.Once);
        }

        [Fact]
        public async Task CompletePaymentAsync_ShouldUpdateStatusTxIdAndPaymentDate_WhenPaymentExists()
        {
            // Arrange
            var paymentUid = Guid.NewGuid();
            var existingPayment = new PaymentModel
            {
                f_uid = paymentUid,
                f_amount = 499.00m,
                f_status = PaymentStatus.Pending
            };

            _mockPaymentRepository
                .Setup(r => r.Find(paymentUid))
                .ReturnsAsync(existingPayment);

            _mockPaymentRepository
                .Setup(r => r.Update(It.IsAny<PaymentModel>()))
                .ReturnsAsync((PaymentModel p) => p);

            // Act
            var result = await _sut.CompletePaymentAsync(paymentUid, "pay_Rzp123456789");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PaymentStatus.Completed, result.f_status);
            Assert.Equal("pay_Rzp123456789", result.f_gateway_txid);
            Assert.NotNull(result.f_payment_date);
            _mockPaymentRepository.Verify(r => r.Update(existingPayment), Times.Once);
        }

        [Fact]
        public async Task CompletePaymentAsync_ShouldReturnNull_WhenPaymentDoesNotExist()
        {
            // Arrange
            _mockPaymentRepository
                .Setup(r => r.Find(It.IsAny<Guid>()))
                .ReturnsAsync((PaymentModel?)null);

            // Act
            var result = await _sut.CompletePaymentAsync(Guid.NewGuid(), "pay_Rzp123456789");

            // Assert
            Assert.Null(result);
            _mockPaymentRepository.Verify(r => r.Update(It.IsAny<PaymentModel>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldUpdateStatusAndTxId_WhenPaymentExists()
        {
            // Arrange
            var paymentUid = Guid.NewGuid();
            var existingPayment = new PaymentModel
            {
                f_uid = paymentUid,
                f_status = PaymentStatus.Pending
            };

            _mockPaymentRepository
                .Setup(r => r.Find(paymentUid))
                .ReturnsAsync(existingPayment);

            _mockPaymentRepository
                .Setup(r => r.Update(It.IsAny<PaymentModel>()))
                .ReturnsAsync((PaymentModel p) => p);

            // Act
            var result = await _sut.UpdateStatusAsync(paymentUid, PaymentStatus.Failed, "tx_failed_001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PaymentStatus.Failed, result.f_status);
            Assert.Equal("tx_failed_001", result.f_gateway_txid);
            _mockPaymentRepository.Verify(r => r.Update(existingPayment), Times.Once);
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldReturnNull_WhenPaymentDoesNotExist()
        {
            // Arrange
            _mockPaymentRepository
                .Setup(r => r.Find(It.IsAny<Guid>()))
                .ReturnsAsync((PaymentModel?)null);

            // Act
            var result = await _sut.UpdateStatusAsync(Guid.NewGuid(), PaymentStatus.Refunded);

            // Assert
            Assert.Null(result);
            _mockPaymentRepository.Verify(r => r.Update(It.IsAny<PaymentModel>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnAffectedRowCount_WhenPaymentExists()
        {
            // Arrange
            var paymentUid = Guid.NewGuid();
            var existingPayment = new PaymentModel { f_uid = paymentUid, f_amount = 100.00m };

            _mockPaymentRepository
                .Setup(r => r.Find(paymentUid))
                .ReturnsAsync(existingPayment);

            _mockPaymentRepository
                .Setup(r => r.Remove(existingPayment))
                .ReturnsAsync(1);

            // Act
            var result = await _sut.DeleteAsync(paymentUid);

            // Assert
            Assert.Equal(1, result);
            _mockPaymentRepository.Verify(r => r.Remove(existingPayment), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnZero_WhenPaymentDoesNotExist()
        {
            // Arrange
            _mockPaymentRepository
                .Setup(r => r.Find(It.IsAny<Guid>()))
                .ReturnsAsync((PaymentModel?)null);

            // Act
            var result = await _sut.DeleteAsync(Guid.NewGuid());

            // Assert
            Assert.Equal(0, result);
            _mockPaymentRepository.Verify(r => r.Remove(It.IsAny<PaymentModel>()), Times.Never);
        }
    }
}