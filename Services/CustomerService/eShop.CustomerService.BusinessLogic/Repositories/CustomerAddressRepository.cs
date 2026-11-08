using Dapper;
using eShop.CustomerService.BusinessLogic.Infrastructure;
using eShop.CustomerService.BusinessLogic.Interfaces;
using eShop.CustomerService.Models;

namespace eShop.CustomerService.BusinessLogic.Repositories
{
    public class CustomerAddressRepository : BaseRepository, ICustomerAddressRepository
    {
        public CustomerAddressRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<CustomerAddressModel>> GetByCustomer(Guid customerUid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_customer_uid],
                    [f_street],
                    [f_city],
                    [f_state],
                    [f_postal_code],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_customer_address] WITH (NOLOCK)
                WHERE 
                    [f_customer_uid] = @CustomerUid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryAsync<CustomerAddressModel>(sql, new { CustomerUid = customerUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<CustomerAddressModel?> Find(Guid uid, Guid customerUid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_customer_uid],
                    [f_street],
                    [f_city],
                    [f_state],
                    [f_postal_code],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_customer_address] WITH (NOLOCK)
                WHERE 
                    [f_uid] = @Uid AND 
                    [f_customer_uid] = @CustomerUid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryFirstOrDefaultAsync<CustomerAddressModel>(sql, new { Uid = uid, CustomerUid = customerUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<CustomerAddressModel> Add(CustomerAddressModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_create_date = DateTime.UtcNow;
            model.f_create_by = _userUid;

            var sql = $@"
                INSERT INTO [{_schema}].[t_customer_address] 
                (
                    [f_uid], 
                    [f_customer_uid], 
                    [f_street], 
                    [f_city], 
                    [f_state], 
                    [f_postal_code], 
                    [f_create_date], 
                    [f_create_by]
                )
                VALUES 
                (
                    @f_uid, 
                    @f_customer_uid, 
                    @f_street, 
                    @f_city, 
                    @f_state, 
                    @f_postal_code, 
                    @f_create_date, 
                    @f_create_by
                )";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<CustomerAddressModel> Update(CustomerAddressModel model)
        {
            model.f_update_date = DateTime.UtcNow;
            model.f_update_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_customer_address]
                SET 
                    [f_street] = @f_street, 
                    [f_city] = @f_city, 
                    [f_state] = @f_state, 
                    [f_postal_code] = @f_postal_code, 
                    [f_update_date] = @f_update_date, 
                    [f_update_by] = @f_update_by
                WHERE 
                    [f_uid] = @f_uid AND 
                    [f_customer_uid] = @f_customer_uid AND 
                    [f_delete_date] IS NULL";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<bool> SetDefault(Guid customerUid, Guid addressUid, Guid operatorUid)
        {
            var sql = $@"
                UPDATE 
                    [{_schema}].[t_customer_address] 
                SET 
                    [f_update_date] = @Now,
                    [f_update_by] = @OperatorUid
                WHERE 
                    [f_uid] = @AddressUid AND 
                    [f_customer_uid] = @CustomerUid AND
                    [f_delete_date] IS NULL";

            var affectedRows = await _connection.ExecuteAsync(sql, new { AddressUid = addressUid, CustomerUid = customerUid, OperatorUid = operatorUid, Now = DateTime.UtcNow }, transaction: _transaction, commandTimeout: _commandTimeout);
            return affectedRows > 0;
        }

        public async Task<int> Remove(CustomerAddressModel model)
        {
            model.f_delete_date = DateTime.UtcNow;
            model.f_delete_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_customer_address]
                SET 
                    [f_delete_date] = @f_delete_date, 
                    [f_delete_by] = @f_delete_by
                WHERE 
                    [f_uid] = @f_uid AND 
                    [f_customer_uid] = @f_customer_uid AND 
                    [f_delete_date] IS NULL";

            return await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
        }
    }
}