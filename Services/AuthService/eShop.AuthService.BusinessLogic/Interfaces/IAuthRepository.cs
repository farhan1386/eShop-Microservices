using eShop.AuthService.Models;

namespace eShop.AuthService.BusinessLogic.Interfaces
{
    public interface IAuthRepository
    {
        Task<UserCredentialsModel?> FindByUsername(string username);
        Task<UserCredentialsModel?> FindByCustomerUid(Guid customerUid);
        Task<UserCredentialsModel> AddCredentials(UserCredentialsModel model);
        Task<UserCredentialsModel> UpdateCredentials(UserCredentialsModel model);

        Task<RefreshTokenModel?> FindRefreshToken(string token);
        Task<RefreshTokenModel> AddRefreshToken(RefreshTokenModel model);
        Task<RefreshTokenModel> UpdateRefreshToken(RefreshTokenModel model);
        Task<int> RevokeAllUserTokens(Guid userCredentialsUid);

        Task<RoleModel?> FindRoleByUid(Guid uid);
        Task<RoleModel?> FindRoleByName(string name);
        Task<RoleModel> AddRole(RoleModel model);
        Task<IEnumerable<RoleModel>> GetRoles();

        Task<UserRoleModel> AddUserRole(UserRoleModel model);
        Task<IEnumerable<RoleModel>> GetUserRoles(Guid userCredentialsUid);
        Task<int> RemoveUserRole(Guid userCredentialsUid, Guid roleUid);
    }
}