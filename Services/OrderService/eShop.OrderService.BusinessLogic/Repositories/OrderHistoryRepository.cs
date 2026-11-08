using Dapper;
using eShop.OrderService.BusinessLogic.Infrastructure;
using eShop.OrderService.BusinessLogic.Interfaces;
using eShop.OrderService.Models;

namespace eShop.OrderService.BusinessLogic.Repositories
{
    public class OrderHistoryRepository : BaseRepository, IOrderHistoryRepository
    {
        public OrderHistoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<OrderHistoryModel>> GetByOrder(Guid orderUid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_order_uid],
                    [f_old_status],
                    [f_new_status],
                    [f_comment],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_order_history] WITH (NOLOCK) 
                WHERE 
                    [f_order_uid] = @OrderUid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryAsync<OrderHistoryModel>(sql, new { OrderUid = orderUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<OrderHistoryModel> Add(OrderHistoryModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_create_date = DateTime.UtcNow;
            model.f_create_by = _userUid;

            var sql = $@"
                INSERT INTO [{_schema}].[t_order_history] 
                (
                    [f_uid], 
                    [f_order_uid], 
                    [f_old_status], 
                    [f_new_status], 
                    [f_comment], 
                    [f_create_date], 
                    [f_create_by]
                )
                VALUES 
                (
                    @f_uid, 
                    @f_order_uid, 
                    @f_old_status, 
                    @f_new_status, 
                    @f_comment, 
                    @f_create_date, 
                    @f_create_by
                )";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }
    }
}