using Dapper;
using eShop.OrderService.BusinessLogic.Infrastructure;
using eShop.OrderService.BusinessLogic.Interfaces;
using eShop.OrderService.Models;

namespace eShop.OrderService.BusinessLogic.Repositories
{
    public class OrderRepository : BaseRepository, IOrderRepository
    {
        public OrderRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<OrderModel>> Get()
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_customer_uid],
                    [f_order_date],
                    [f_total_price],
                    [f_status],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_order] WITH (NOLOCK) 
                WHERE 
                    [f_delete_date] IS NULL";

            return await _connection.QueryAsync<OrderModel>(sql, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<OrderModel?> Find(Guid uid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_customer_uid],
                    [f_order_date],
                    [f_total_price],
                    [f_status],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_order] WITH (NOLOCK) 
                WHERE 
                    [f_uid] = @Uid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryFirstOrDefaultAsync<OrderModel>(sql, new { Uid = uid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<IEnumerable<OrderModel>> GetByCustomer(Guid customerUid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_customer_uid],
                    [f_order_date],
                    [f_total_price],
                    [f_status],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_order] WITH (NOLOCK) 
                WHERE 
                    [f_customer_uid] = @CustomerUid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryAsync<OrderModel>(sql, new { CustomerUid = customerUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<OrderModel> Add(OrderModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_order_date = DateTime.UtcNow;
            model.f_create_date = DateTime.UtcNow;
            model.f_create_by = _userUid;

            var sql = $@"
                INSERT INTO [{_schema}].[t_order] 
                (
                    [f_uid], 
                    [f_customer_uid], 
                    [f_order_date], 
                    [f_total_price], 
                    [f_status], 
                    [f_create_date], 
                    [f_create_by]
                )
                VALUES 
                (
                    @f_uid, 
                    @f_customer_uid, 
                    @f_order_date, 
                    @f_total_price, 
                    @f_status, 
                    @f_create_date, 
                    @f_create_by
                )";

            var parameters = new
            {
                model.f_uid,
                model.f_customer_uid,
                model.f_order_date,
                model.f_total_price,
                f_status = model.f_status.ToString(),
                model.f_create_date,
                model.f_create_by
            };

            await _connection.ExecuteAsync(sql, parameters, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<OrderModel> Update(OrderModel model)
        {
            model.f_update_date = DateTime.UtcNow;
            model.f_update_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_order]
                SET 
                    [f_total_price] = @f_total_price, 
                    [f_status] = @f_status, 
                    [f_update_date] = @f_update_date, 
                    [f_update_by] = @f_update_by
                WHERE 
                    [f_uid] = @f_uid AND 
                    [f_delete_date] IS NULL";

            var parameters = new
            {
                model.f_uid,
                model.f_total_price,
                f_status = model.f_status.ToString(),
                model.f_update_date,
                model.f_update_by
            };

            await _connection.ExecuteAsync(sql, parameters, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<int> Remove(OrderModel model)
        {
            model.f_delete_date = DateTime.UtcNow;
            model.f_delete_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_order]
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