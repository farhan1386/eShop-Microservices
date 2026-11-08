using eShop.CustomerService.BusinessLogic.Infrastructure;
using eShop.CustomerService.BusinessLogic.Interfaces;
using eShop.CustomerService.Models;
using Moq;

namespace eShop.CustomerService.Tests.CustomerServices
{
    public class CustomerService_Tests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICustomerRepository> _mockCustomerRepository;
        private readonly Mock<ICustomerAddressRepository> _mockAddressRepository;
        private readonly BusinessLogic.Services.CustomerService _sut;

        public CustomerService_Tests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _mockAddressRepository = new Mock<ICustomerAddressRepository>();

            _sut = new BusinessLogic.Services.CustomerService(
                _mockUnitOfWork.Object,
                _mockCustomerRepository.Object,
                _mockAddressRepository.Object
            );
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllCustomers_WhenCustomersExist()
        {
            // Arrange
            var expectedCustomers = new List<CustomerModel>
    {
        new CustomerModel { f_uid = Guid.NewGuid(), f_email = "aarav.sharma@eshop.com", f_fname = "Aarav", f_lname = "Sharma" },
        new CustomerModel { f_uid = Guid.NewGuid(), f_email = "priya.verma@eshop.com", f_fname = "Priya", f_lname = "Verma" }
    };

            _mockCustomerRepository
                .Setup(r => r.Get())
                .ReturnsAsync(expectedCustomers);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            var customerList = result.ToList();
            Assert.Equal(2, customerList.Count);
            Assert.Equal("aarav.sharma@eshop.com", customerList[0].f_email);
            Assert.Equal("priya.verma@eshop.com", customerList[1].f_email);
            _mockCustomerRepository.Verify(r => r.Get(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyCollection_WhenNoCustomersExist()
        {
            // Arrange
            _mockCustomerRepository
                .Setup(r => r.Get())
                .ReturnsAsync(Enumerable.Empty<CustomerModel>());

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            Assert.Empty(result);
            _mockCustomerRepository.Verify(r => r.Get(), Times.Once);
        }

        [Fact]
        public async Task GetProfileAsync_ShouldReturnCustomer_WhenCustomerExists()
        {
            // Arrange
            var customerUid = Guid.NewGuid();
            var expectedCustomer = new CustomerModel { f_uid = customerUid, f_email = "user@eshop.com" };

            _mockCustomerRepository
                .Setup(r => r.Find(customerUid))
                .ReturnsAsync(expectedCustomer);

            // Act
            var result = await _sut.GetProfileAsync(customerUid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customerUid, result.f_uid);
            Assert.Equal("user@eshop.com", result.f_email);
        }

        [Fact]
        public async Task GetProfileAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
        {
            // Arrange
            var customerUid = Guid.NewGuid();
            _mockCustomerRepository
                .Setup(r => r.Find(customerUid))
                .ReturnsAsync((CustomerModel?)null);

            // Act
            var result = await _sut.GetProfileAsync(customerUid);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task OnboardNewCustomerAsync_ShouldThrowException_WhenEmailAlreadyExists()
        {
            // Arrange
            var customer = new CustomerModel { f_email = "duplicate@eshop.com", f_password = "password123" };
            var address = new CustomerAddressModel();

            _mockCustomerRepository
                .Setup(r => r.IsEmailAvailable(customer.f_email))
                .ReturnsAsync(false);

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.OnboardNewCustomerAsync(customer, address));

            // Assert
            Assert.Equal($"The email address '{customer.f_email}' is already registered.", exception.Message);
            _mockUnitOfWork.Verify(u => u.BeginAsync(), Times.Never);
        }

        [Fact]
        public async Task OnboardNewCustomerAsync_ShouldReturnTrue_WhenCustomerAndAddressAreSavedSuccessfully()
        {
            // Arrange
            var customer = new CustomerModel { f_email = "newuser@eshop.com", f_password = "PlainPassword123!" };
            var address = new CustomerAddressModel();

            _mockCustomerRepository
                .Setup(r => r.IsEmailAvailable(customer.f_email))
                .ReturnsAsync(true);

            _mockCustomerRepository
                .Setup(r => r.Add(It.IsAny<CustomerModel>()))
                .ReturnsAsync((CustomerModel c) =>
                {
                    c.f_uid = Guid.NewGuid();
                    return c;
                });

            _mockAddressRepository
                .Setup(r => r.Add(It.IsAny<CustomerAddressModel>()))
                .ReturnsAsync((CustomerAddressModel a) => a);

            // Act
            var result = await _sut.OnboardNewCustomerAsync(customer, address);

            // Assert
            Assert.True(result);
            Assert.NotEqual("PlainPassword123!", customer.f_password);
            Assert.Equal(customer.f_uid, address.f_customer_uid);
            _mockUnitOfWork.Verify(u => u.BeginAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Never);
        }

        [Fact]
        public async Task OnboardNewCustomerAsync_ShouldRollbackAndRethrow_WhenCustomerFailsToSave()
        {
            // Arrange
            var customer = new CustomerModel { f_email = "failcustomer@eshop.com", f_password = "password123" };
            var address = new CustomerAddressModel();

            _mockCustomerRepository
                .Setup(r => r.IsEmailAvailable(customer.f_email))
                .ReturnsAsync(true);

            _mockCustomerRepository
                .Setup(r => r.Add(It.IsAny<CustomerModel>()))
                .ThrowsAsync(new Exception("Customer insert failed"));

            // Act
            await Assert.ThrowsAsync<Exception>(() =>
                _sut.OnboardNewCustomerAsync(customer, address));

            // Assert
            _mockAddressRepository.Verify(r => r.Add(It.IsAny<CustomerAddressModel>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.BeginAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task OnboardNewCustomerAsync_ShouldRollbackAndRethrow_WhenAddressFailsToSave()
        {
            // Arrange
            var customer = new CustomerModel { f_email = "rollback@eshop.com", f_password = "password123" };
            var address = new CustomerAddressModel();

            _mockCustomerRepository
                .Setup(r => r.IsEmailAvailable(customer.f_email))
                .ReturnsAsync(true);

            _mockCustomerRepository
                .Setup(r => r.Add(It.IsAny<CustomerModel>()))
                .ReturnsAsync((CustomerModel c) => c);

            _mockAddressRepository
                .Setup(r => r.Add(It.IsAny<CustomerAddressModel>()))
                .ThrowsAsync(new Exception("Address insert failed"));

            // Act
            await Assert.ThrowsAsync<Exception>(() =>
                _sut.OnboardNewCustomerAsync(customer, address));

            // Assert
            _mockUnitOfWork.Verify(u => u.BeginAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Never);
            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task ModifyContactDetailsAsync_ShouldReturnTrueAndUpdateFields_WhenCustomerExists()
        {
            // Arrange
            var customerUid = Guid.NewGuid();
            var existingProfile = new CustomerModel
            {
                f_uid = customerUid,
                f_fname = "OldFirst",
                f_lname = "OldLast",
                f_phone = "000000"
            };

            _mockCustomerRepository
                .Setup(r => r.Find(customerUid))
                .ReturnsAsync(existingProfile);

            _mockCustomerRepository
                .Setup(r => r.Update(It.IsAny<CustomerModel>()))
                .ReturnsAsync((CustomerModel c) => c);

            // Act
            var result = await _sut.ModifyContactDetailsAsync(customerUid, "John", "Doe", "9876543210");

            // Assert
            Assert.True(result);
            Assert.Equal("John", existingProfile.f_fname);
            Assert.Equal("Doe", existingProfile.f_lname);
            Assert.Equal("9876543210", existingProfile.f_phone);
            _mockCustomerRepository.Verify(r => r.Update(existingProfile), Times.Once);
        }

        [Fact]
        public async Task ModifyContactDetailsAsync_ShouldReturnFalse_WhenCustomerDoesNotExist()
        {
            // Arrange
            var customerUid = Guid.NewGuid();
            _mockCustomerRepository
                .Setup(r => r.Find(customerUid))
                .ReturnsAsync((CustomerModel?)null);

            // Act
            var result = await _sut.ModifyContactDetailsAsync(customerUid, "John", "Doe", "123456");

            // Assert
            Assert.False(result);
            _mockCustomerRepository.Verify(r => r.Update(It.IsAny<CustomerModel>()), Times.Never);
        }
    }
}
