using Dapper;
using eShop.CustomerService.BusinessLogic.Infrastructure;
using eShop.CustomerService.BusinessLogic.Interfaces;
using eShop.CustomerService.Models;

namespace eShop.CustomerService.BusinessLogic.Repositories
{
    public class CustomerRepository : BaseRepository, ICustomerRepository
    {
        public CustomerRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<CustomerModel>> Get()
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_email],
                    [f_password],
                    [f_fname],
                    [f_lname],
                    [f_phone],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_customer] WITH (NOLOCK) 
                WHERE 
                    [f_delete_date] IS NULL";

            return await _connection.QueryAsync<CustomerModel>(sql, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<CustomerModel?> Find(Guid uid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_email],
                    [f_password],
                    [f_fname],
                    [f_lname],
                    [f_phone],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_customer] WITH (NOLOCK) 
                WHERE 
                    [f_uid] = @Uid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryFirstOrDefaultAsync<CustomerModel>(sql, new { Uid = uid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<CustomerModel?> FindByEmail(string email)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_email],
                    [f_password],
                    [f_fname],
                    [f_lname],
                    [f_phone],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_customer] WITH (NOLOCK) 
                WHERE 
                    [f_email] = @Email AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryFirstOrDefaultAsync<CustomerModel>(sql, new { Email = email }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<bool> IsEmailAvailable(string email)
        {
            var sql = $@"
                SELECT TOP 1 
                    1 
                FROM 
                    [{_schema}].[t_customer] WITH (NOLOCK) 
                WHERE 
                    [f_email] = @Email";

            var exists = await _connection.ExecuteScalarAsync<int?>(sql, new { Email = email }, transaction: _transaction, commandTimeout: _commandTimeout);
            return !exists.HasValue;
        }

        public async Task<CustomerModel> Add(CustomerModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_create_date = DateTime.UtcNow;
            model.f_create_by = _userUid;

            var sql = $@"
                INSERT INTO [{_schema}].[t_customer] 
                (
                    [f_uid], 
                    [f_email], 
                    [f_password], 
                    [f_fname], 
                    [f_lname], 
                    [f_phone], 
                    [f_create_date], 
                    [f_create_by]
                )
                VALUES 
                (
                    @f_uid, 
                    @f_email, 
                    @f_password, 
                    @f_fname, 
                    @f_lname, 
                    @f_phone, 
                    @f_create_date, 
                    @f_create_by
                )";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<CustomerModel> Update(CustomerModel model)
        {
            model.f_update_date = DateTime.UtcNow;
            model.f_update_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_customer]
                SET 
                    [f_fname] = @f_fname, 
                    [f_lname] = @f_lname, 
                    [f_phone] = @f_phone, 
                    [f_update_date] = @f_update_date, 
                    [f_update_by] = @f_update_by
                WHERE 
                    [f_uid] = @f_uid AND 
                    [f_delete_date] IS NULL";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<bool> UpdatePassword(Guid uid, string passwordHash, Guid operatorUid)
        {
            var sql = $@"
                UPDATE 
                    [{_schema}].[t_customer]
                SET 
                    [f_password] = @PasswordHash, 
                    [f_update_date] = @UpdateDate, 
                    [f_update_by] = @OperatorUid
                WHERE 
                    [f_uid] = @Uid AND 
                    [f_delete_date] IS NULL";

            var affectedRows = await _connection.ExecuteAsync(sql, new { Uid = uid, PasswordHash = passwordHash, OperatorUid = operatorUid, UpdateDate = DateTime.UtcNow }, transaction: _transaction, commandTimeout: _commandTimeout);
            return affectedRows > 0;
        }

        public async Task<bool> UpdateLastLogin(Guid uid, DateTime timestamp)
        {
            var sql = $@"
                UPDATE 
                    [{_schema}].[t_customer] 
                SET 
                    [f_update_date] = @Timestamp 
                WHERE 
                    [f_uid] = @Uid AND 
                    [f_delete_date] IS NULL";

            var affectedRows = await _connection.ExecuteAsync(sql, new { Uid = uid, Timestamp = timestamp }, transaction: _transaction, commandTimeout: _commandTimeout);
            return affectedRows > 0;
        }

        public async Task<int> Remove(CustomerModel model)
        {
            model.f_delete_date = DateTime.UtcNow;
            model.f_delete_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_customer]
                SET 
                    [f_delete_date] = @f_delete_date, 
                    [f_delete_by] = @f_delete_by
                WHERE 
                    [f_uid] = @f_uid AND 
                    [f_delete_date] IS NULL";

            return await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
        }
    }
}