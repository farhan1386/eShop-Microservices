using eShop.AuthService.Models;
using eShop.AuthService.Models.DTOs;

namespace eShop.AuthService.BusinessLogic.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterUserDto dto);
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);
        Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto dto);
        Task<bool> RevokeTokenAsync(string token);
        Task<RoleModel> CreateRoleAsync(CreateRoleDto dto);
        Task<bool> AssignRoleToUserAsync(AssignRoleDto dto);
        Task<IEnumerable<RoleModel>> GetUserRolesAsync(Guid userCredentialsUid);
    }
}
