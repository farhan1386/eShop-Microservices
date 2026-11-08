using eShop.AuthService.BusinessLogic.Infrastructure;
using eShop.AuthService.BusinessLogic.Interfaces;
using eShop.AuthService.Models;
using eShop.AuthService.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace eShop.AuthService.BusinessLogic.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IAuthRepository authRepository, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _authRepository = authRepository;
            _configuration = configuration;
        }

        public async Task<bool> RegisterAsync(RegisterUserDto dto)
        {
            var existingUser = await _authRepository.FindByUsername(dto.Username);
            if (existingUser is not null)
            {
                throw new InvalidOperationException($"Username '{dto.Username}' is already taken.");
            }

            var saltBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            string salt = Convert.ToBase64String(saltBytes);

            string passwordHash;
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = dto.Password + salt;
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                passwordHash = Convert.ToBase64String(hashedBytes);
            }

            var userCredentials = new UserCredentialsModel
            {
                f_customer_uid = dto.CustomerUid,
                f_username = dto.Username,
                f_password_hash = passwordHash,
                f_salt = salt,
                f_is_locked_out = false,
                f_failed_login_attempts = 0
            };

            await _authRepository.AddCredentials(userCredentials);
            return true;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            var user = await _authRepository.FindByUsername(dto.Username);
            if (user is null || user.f_is_locked_out)
            {
                return null;
            }

            string computedHash;
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = dto.Password + user.f_salt;
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                computedHash = Convert.ToBase64String(hashedBytes);
            }

            if (user.f_password_hash != computedHash)
            {
                user.f_failed_login_attempts++;
                if (user.f_failed_login_attempts >= 5)
                {
                    user.f_is_locked_out = true;
                }
                await _authRepository.UpdateCredentials(user);
                return null;
            }

            user.f_failed_login_attempts = 0;
            user.f_last_login_date = DateTime.UtcNow;
            await _authRepository.UpdateCredentials(user);

            var roles = await _authRepository.GetUserRoles(user.f_uid);
            return await GenerateTokensAsync(user, roles);
        }

        public async Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            var tokenRecord = await _authRepository.FindRefreshToken(dto.RefreshToken);
            if (tokenRecord is null || tokenRecord.f_is_revoked || tokenRecord.f_expires_date < DateTime.UtcNow)
            {
                return null;
            }

            var user = await _authRepository.FindByCustomerUid(tokenRecord.f_user_credentials_uid);
            if (user is null || user.f_is_locked_out)
            {
                return null;
            }

            await _unitOfWork.BeginAsync();
            try
            {
                tokenRecord.f_is_revoked = true;
                await _authRepository.UpdateRefreshToken(tokenRecord);

                var roles = await _authRepository.GetUserRoles(user.f_uid);
                var response = await GenerateTokensAsync(user, roles);

                await _unitOfWork.CommitAsync();
                return response;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var tokenRecord = await _authRepository.FindRefreshToken(token);
            if (tokenRecord is null || tokenRecord.f_is_revoked)
            {
                return false;
            }

            tokenRecord.f_is_revoked = true;
            await _authRepository.UpdateRefreshToken(tokenRecord);
            return true;
        }

        public async Task<RoleModel> CreateRoleAsync(CreateRoleDto dto)
        {
            var existingRole = await _authRepository.FindRoleByName(dto.Name);
            if (existingRole is not null)
            {
                return existingRole;
            }

            var role = new RoleModel
            {
                f_name = dto.Name,
                f_description = dto.Description
            };

            return await _authRepository.AddRole(role);
        }

        public async Task<bool> AssignRoleToUserAsync(AssignRoleDto dto)
        {
            var userRole = new UserRoleModel
            {
                f_user_credentials_uid = dto.UserCredentialsUid,
                f_role_uid = dto.RoleUid
            };

            await _authRepository.AddUserRole(userRole);
            return true;
        }

        public Task<IEnumerable<RoleModel>> GetUserRolesAsync(Guid userCredentialsUid)
        {
            return _authRepository.GetUserRoles(userCredentialsUid);
        }

        private async Task<LoginResponseDto> GenerateTokensAsync(UserCredentialsModel user, IEnumerable<RoleModel> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.f_uid.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.f_username),
                new Claim("customer_uid", user.f_customer_uid.ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.f_name));
            }

            var jwtSecret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret configuration missing.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "15"));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );

            string accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshTokenBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(refreshTokenBytes);
            }
            string refreshTokenValue = Convert.ToBase64String(refreshTokenBytes);

            var refreshToken = new RefreshTokenModel
            {
                f_user_credentials_uid = user.f_uid,
                f_token = refreshTokenValue,
                f_expires_date = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshExpiryDays"] ?? "7")),
                f_is_revoked = false
            };

            await _authRepository.AddRefreshToken(refreshToken);

            return new LoginResponseDto(accessToken, refreshTokenValue, expiresAt);
        }
    }
}