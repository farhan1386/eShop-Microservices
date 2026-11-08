using Dapper;
using eShop.AuthService.BusinessLogic.Infrastructure;
using eShop.AuthService.BusinessLogic.Interfaces;
using eShop.AuthService.Models;

namespace eShop.AuthService.BusinessLogic.Repositories
{
    public class AuthRepository : BaseRepository, IAuthRepository
    {
        public AuthRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<UserCredentialsModel?> FindByUsername(string username)
        {
            var sql = $@"
                SELECT 
                    [f_uid], [f_iid], [f_customer_uid], [f_username], [f_password_hash], [f_salt], 
                    [f_is_locked_out], [f_failed_login_attempts], [f_last_login_date], [f_create_date], [f_update_date]
                FROM 
                    [{_schema}].[t_user_credentials] WITH (NOLOCK) 
                WHERE 
                    [f_username] = @Username";

            return await _connection.QueryFirstOrDefaultAsync<UserCredentialsModel>(sql, new { Username = username }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<UserCredentialsModel?> FindByCustomerUid(Guid customerUid)
        {
            var sql = $@"
                SELECT 
                    [f_uid], [f_iid], [f_customer_uid], [f_username], [f_password_hash], [f_salt], 
                    [f_is_locked_out], [f_failed_login_attempts], [f_last_login_date], [f_create_date], [f_update_date]
                FROM 
                    [{_schema}].[t_user_credentials] WITH (NOLOCK) 
                WHERE 
                    [f_customer_uid] = @CustomerUid";

            return await _connection.QueryFirstOrDefaultAsync<UserCredentialsModel>(sql, new { CustomerUid = customerUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<UserCredentialsModel> AddCredentials(UserCredentialsModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_create_date = DateTime.UtcNow;

            var sql = $@"
                INSERT INTO [{_schema}].[t_user_credentials] 
                (
                    [f_uid], [f_customer_uid], [f_username], [f_password_hash], [f_salt], 
                    [f_is_locked_out], [f_failed_login_attempts], [f_last_login_date], [f_create_date]
                )
                VALUES 
                (
                    @f_uid, @f_customer_uid, @f_username, @f_password_hash, @f_salt, 
                    @f_is_locked_out, @f_failed_login_attempts, @f_last_login_date, @f_create_date
                )";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<UserCredentialsModel> UpdateCredentials(UserCredentialsModel model)
        {
            model.f_update_date = DateTime.UtcNow;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_user_credentials]
                SET 
                    [f_password_hash] = @f_password_hash, 
                    [f_salt] = @f_salt,
                    [f_is_locked_out] = @f_is_locked_out,
                    [f_failed_login_attempts] = @f_failed_login_attempts,
                    [f_last_login_date] = @f_last_login_date,
                    [f_update_date] = @f_update_date
                WHERE 
                    [f_uid] = @f_uid";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<RefreshTokenModel?> FindRefreshToken(string token)
        {
            var sql = $@"
                SELECT 
                    [f_uid], [f_iid], [f_user_credentials_uid], [f_token], [f_expires_date], [f_is_revoked], [f_create_date]
                FROM 
                    [{_schema}].[t_refresh_token] WITH (NOLOCK) 
                WHERE 
                    [f_token] = @Token";

            return await _connection.QueryFirstOrDefaultAsync<RefreshTokenModel>(sql, new { Token = token }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<RefreshTokenModel> AddRefreshToken(RefreshTokenModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_create_date = DateTime.UtcNow;

            var sql = $@"
                INSERT INTO [{_schema}].[t_refresh_token] 
                (
                    [f_uid], [f_user_credentials_uid], [f_token], [f_expires_date], [f_is_revoked], [f_create_date]
                )
                VALUES 
                (
                    @f_uid, @f_user_credentials_uid, @f_token, @f_expires_date, @f_is_revoked, @f_create_date
                )";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<RefreshTokenModel> UpdateRefreshToken(RefreshTokenModel model)
        {
            var sql = $@"
                UPDATE 
                    [{_schema}].[t_refresh_token]
                SET 
                    [f_is_revoked] = @f_is_revoked
                WHERE 
                    [f_uid] = @f_uid";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<int> RevokeAllUserTokens(Guid userCredentialsUid)
        {
            var sql = $@"
                UPDATE 
                    [{_schema}].[t_refresh_token]
                SET 
                    [f_is_revoked] = 1
                WHERE 
                    [f_user_credentials_uid] = @UserCredentialsUid AND [f_is_revoked] = 0";

            return await _connection.ExecuteAsync(sql, new { UserCredentialsUid = userCredentialsUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<RoleModel?> FindRoleByUid(Guid uid)
        {
            var sql = $@"
                SELECT 
                    [f_uid], [f_iid], [f_name], [f_description], [f_create_date]
                FROM 
                    [{_schema}].[t_role] WITH (NOLOCK) 
                WHERE 
                    [f_uid] = @Uid";

            return await _connection.QueryFirstOrDefaultAsync<RoleModel>(sql, new { Uid = uid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<RoleModel?> FindRoleByName(string name)
        {
            var sql = $@"
                SELECT 
                    [f_uid], [f_iid], [f_name], [f_description], [f_create_date]
                FROM 
                    [{_schema}].[t_role] WITH (NOLOCK) 
                WHERE 
                    [f_name] = @Name";

            return await _connection.QueryFirstOrDefaultAsync<RoleModel>(sql, new { Name = name }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<RoleModel> AddRole(RoleModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_create_date = DateTime.UtcNow;

            var sql = $@"
                INSERT INTO [{_schema}].[t_role] 
                (
                    [f_uid], [f_name], [f_description], [f_create_date]
                )
                VALUES 
                (
                    @f_uid, @f_name, @f_description, @f_create_date
                )";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<IEnumerable<RoleModel>> GetRoles()
        {
            var sql = $@"
                SELECT 
                    [f_uid], [f_iid], [f_name], [f_description], [f_create_date]
                FROM 
                    [{_schema}].[t_role] WITH (NOLOCK)";

            return await _connection.QueryAsync<RoleModel>(sql, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<UserRoleModel> AddUserRole(UserRoleModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_create_date = DateTime.UtcNow;

            var sql = $@"
                INSERT INTO [{_schema}].[t_user_role] 
                (
                    [f_uid], [f_user_credentials_uid], [f_role_uid], [f_create_date]
                )
                VALUES 
                (
                    @f_uid, @f_user_credentials_uid, @f_role_uid, @f_create_date
                )";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<IEnumerable<RoleModel>> GetUserRoles(Guid userCredentialsUid)
        {
            var sql = $@"
                SELECT 
                    r.[f_uid], r.[f_iid], r.[f_name], r.[f_description], r.[f_create_date]
                FROM 
                    [{_schema}].[t_role] r WITH (NOLOCK)
                INNER JOIN
                    [{_schema}].[t_user_role] ur WITH (NOLOCK) ON r.[f_uid] = ur.[f_role_uid]
                WHERE 
                    ur.[f_user_credentials_uid] = @UserCredentialsUid";

            return await _connection.QueryAsync<RoleModel>(sql, new { UserCredentialsUid = userCredentialsUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<int> RemoveUserRole(Guid userCredentialsUid, Guid roleUid)
        {
            var sql = $@"
                DELETE FROM 
                    [{_schema}].[t_user_role]
                WHERE 
                    [f_user_credentials_uid] = @UserCredentialsUid AND [f_role_uid] = @RoleUid";

            return await _connection.ExecuteAsync(sql, new { UserCredentialsUid = userCredentialsUid, RoleUid = roleUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }
    }
}