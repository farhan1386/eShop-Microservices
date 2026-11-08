using Dapper;
using eShop.OrderService.BusinessLogic.Infrastructure;
using eShop.OrderService.BusinessLogic.Interfaces;
using eShop.OrderService.Models;

namespace eShop.OrderService.BusinessLogic.Repositories
{
    public class OrderItemRepository : BaseRepository, IOrderItemRepository
    {
        public OrderItemRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<OrderItemModel>> GetByOrder(Guid orderUid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_order_uid],
                    [f_product_uid],
                    [f_quantity],
                    [f_unit_price],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_order_item] WITH (NOLOCK) 
                WHERE 
                    [f_order_uid] = @OrderUid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryAsync<OrderItemModel>(sql, new { OrderUid = orderUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<OrderItemModel?> Find(Guid uid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_order_uid],
                    [f_product_uid],
                    [f_quantity],
                    [f_unit_price],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_order_item] WITH (NOLOCK) 
                WHERE 
                    [f_uid] = @Uid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryFirstOrDefaultAsync<OrderItemModel>(sql, new { Uid = uid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<OrderItemModel> Add(OrderItemModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_create_date = DateTime.UtcNow;
            model.f_create_by = _userUid;

            var sql = $@"
                INSERT INTO [{_schema}].[t_order_item] 
                (
                    [f_uid], 
                    [f_order_uid], 
                    [f_product_uid], 
                    [f_quantity], 
                    [f_unit_price], 
                    [f_create_date], 
                    [f_create_by]
                )
                VALUES 
                (
                    @f_uid, 
                    @f_order_uid, 
                    @f_product_uid, 
                    @f_quantity, 
                    @f_unit_price, 
                    @f_create_date, 
                    @f_create_by
                )";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<OrderItemModel> Update(OrderItemModel model)
        {
            model.f_update_date = DateTime.UtcNow;
            model.f_update_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_order_item]
                SET 
                    [f_quantity] = @f_quantity, 
                    [f_unit_price] = @f_unit_price, 
                    [f_update_date] = @f_update_date, 
                    [f_update_by] = @f_update_by
                WHERE 
                    [f_uid] = @f_uid AND 
                    [f_delete_date] IS NULL";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<int> Remove(OrderItemModel model)
        {
            model.f_delete_date = DateTime.UtcNow;
            model.f_delete_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_order_item]
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
