using eShop.AuthService.BusinessLogic.Infrastructure;
using eShop.AuthService.BusinessLogic.Interfaces;
using eShop.AuthService.Models;
using eShop.AuthService.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Moq;
using AuthServiceInstance = eShop.AuthService.BusinessLogic.Services.AuthService;

namespace eShop.AuthService.Tests.AuthService
{
    public class AuthService_Tests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IAuthRepository> _mockAuthRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthServiceInstance _authService;

        public AuthService_Tests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockAuthRepository = new Mock<IAuthRepository>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(c => c["Jwt:Secret"]).Returns("SuperSecretLongKeyEnforcingEncryptionComplianceCheck123!");
            _mockConfiguration.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("15");
            _mockConfiguration.Setup(c => c["Jwt:RefreshExpiryDays"]).Returns("7");
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("eShop.Auth");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("eShop.Gateway");

            _authService = new AuthServiceInstance(
                _mockUnitOfWork.Object,
                _mockAuthRepository.Object,
                _mockConfiguration.Object
            );
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowException_WhenUsernameAlreadyExists()
        {
            // Arrange
            var dto = new RegisterUserDto("existing_user", "SecurePassword123!", Guid.NewGuid());
            var existingUser = new UserCredentialsModel { f_username = dto.Username };

            _mockAuthRepository
                .Setup(r => r.FindByUsername(dto.Username))
                .ReturnsAsync(existingUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(dto));
            Assert.Contains($"Username '{dto.Username}' is already taken", exception.Message);
        }

        [Fact]
        public async Task RegisterAsync_ShouldSuccessfullyHashPassword_WhenInputIsValid()
        {
            // Arrange
            var dto = new RegisterUserDto("new_user", "SecurePassword123!", Guid.NewGuid());

            _mockAuthRepository
                .Setup(r => r.FindByUsername(dto.Username))
                .ReturnsAsync((UserCredentialsModel?)null);

            _mockAuthRepository
                .Setup(r => r.AddCredentials(It.IsAny<UserCredentialsModel>()))
                .ReturnsAsync(new UserCredentialsModel());

            // Act
            var result = await _authService.RegisterAsync(dto);

            // Assert
            Assert.True(result);
            _mockAuthRepository.Verify(r => r.AddCredentials(It.Is<UserCredentialsModel>(u =>
                u.f_username == dto.Username &&
                u.f_password_hash != dto.Password &&
                !string.IsNullOrEmpty(u.f_salt)
            )), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var dto = new LoginRequestDto("unknown_user", "WrongPassword!");

            _mockAuthRepository
                .Setup(r => r.FindByUsername(dto.Username))
                .ReturnsAsync((UserCredentialsModel?)null);

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenAccountIsLockedOut()
        {
            // Arrange
            var dto = new LoginRequestDto("locked_user", "SomePassword!");
            var lockedUser = new UserCredentialsModel { f_username = dto.Username, f_is_locked_out = true };

            _mockAuthRepository
                .Setup(r => r.FindByUsername(dto.Username))
                .ReturnsAsync(lockedUser);

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ShouldIncrementFailedAttempts_WhenPasswordIsIncorrect()
        {
            // Arrange
            var saltBytes = new byte[] { 1, 2, 3, 4, 5 };
            string salt = Convert.ToBase64String(saltBytes);
            string incorrectPasswordHash = "InvalidHashValue1234567=";

            var existingUser = new UserCredentialsModel
            {
                f_uid = Guid.NewGuid(),
                f_username = "target_user",
                f_password_hash = incorrectPasswordHash,
                f_salt = salt,
                f_failed_login_attempts = 0,
                f_is_locked_out = false
            };

            var loginDto = new LoginRequestDto("target_user", "WrongPasswordEntered!");

            _mockAuthRepository
                .Setup(r => r.FindByUsername(loginDto.Username))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.Null(result);
            _mockAuthRepository.Verify(r => r.UpdateCredentials(It.Is<UserCredentialsModel>(u =>
                u.f_failed_login_attempts == 1
            )), Times.Once);
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnNull_WhenTokenIsExpiredOrRevoked()
        {
            // Arrange
            var dto = new RefreshTokenRequestDto("expired_or_revoked_token");
            var tokenRecord = new RefreshTokenModel { f_token = dto.RefreshToken, f_is_revoked = true };

            _mockAuthRepository
                .Setup(r => r.FindRefreshToken(dto.RefreshToken))
                .ReturnsAsync(tokenRecord);

            // Act
            var result = await _authService.RefreshTokenAsync(dto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RevokeTokenAsync_ShouldReturnFalse_WhenTokenDoesNotExist()
        {
            // Arrange
            string token = "non_existent_token";

            _mockAuthRepository
                .Setup(r => r.FindRefreshToken(token))
                .ReturnsAsync((RefreshTokenModel?)null);

            // Act
            var result = await _authService.RevokeTokenAsync(token);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RevokeTokenAsync_ShouldReturnTrue_WhenTokenIsSuccessfullyRevoked()
        {
            // Arrange
            string token = "active_token";
            var tokenRecord = new RefreshTokenModel { f_token = token, f_is_revoked = false };

            _mockAuthRepository
                .Setup(r => r.FindRefreshToken(token))
                .ReturnsAsync(tokenRecord);

            // Act
            var result = await _authService.RevokeTokenAsync(token);

            // Assert
            Assert.True(result);
            _mockAuthRepository.Verify(r => r.UpdateRefreshToken(It.Is<RefreshTokenModel>(t => t.f_is_revoked == true)), Times.Once);
        }

        [Fact]
        public async Task CreateRoleAsync_ShouldReturnExistingRole_WhenRoleNameAlreadyExists()
        {
            // Arrange
            var dto = new CreateRoleDto("Administrator", "Admin access");
            var existingRole = new RoleModel { f_name = dto.Name, f_description = dto.Description };

            _mockAuthRepository
                .Setup(r => r.FindRoleByName(dto.Name))
                .ReturnsAsync(existingRole);

            // Act
            var result = await _authService.CreateRoleAsync(dto);

            // Assert
            Assert.Equal(dto.Name, result.f_name);
            _mockAuthRepository.Verify(r => r.AddRole(It.IsAny<RoleModel>()), Times.Never);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_ShouldReturnTrue_WhenAssignmentSucceeds()
        {
            // Arrange
            var dto = new AssignRoleDto(Guid.NewGuid(), Guid.NewGuid());

            _mockAuthRepository
                .Setup(r => r.AddUserRole(It.IsAny<UserRoleModel>()))
                .ReturnsAsync(new UserRoleModel());

            // Act
            var result = await _authService.AssignRoleToUserAsync(dto);

            // Assert
            Assert.True(result);
            _mockAuthRepository.Verify(r => r.AddUserRole(It.Is<UserRoleModel>(ur =>
                ur.f_user_credentials_uid == dto.UserCredentialsUid &&
                ur.f_role_uid == dto.RoleUid
            )), Times.Once);
        }

        [Fact]
        public async Task GetUserRolesAsync_ShouldReturnRolesCollection_WhenCalled()
        {
            // Arrange
            var userCredentialsUid = Guid.NewGuid();
            var rolesList = new List<RoleModel>
            {
                new RoleModel { f_name = "User" },
                new RoleModel { f_name = "Manager" }
            };

            _mockAuthRepository
                .Setup(r => r.GetUserRoles(userCredentialsUid))
                .ReturnsAsync(rolesList);

            // Act
            var result = await _authService.GetUserRolesAsync(userCredentialsUid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(rolesList, result);
        }
    }
}