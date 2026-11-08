using Dapper;
using eShop.PaymentService.BusinessLogic.Infrastructure;
using eShop.PaymentService.BusinessLogic.Interfaces;
using eShop.PaymentService.Models;

namespace eShop.PaymentService.BusinessLogic.Repositories
{
    public class PaymentRepository : BaseRepository, IPaymentRepository
    {
        public PaymentRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<PaymentModel>> Get()
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_order_uid],
                    [f_amount],
                    [f_currency],
                    [f_method],
                    [f_gateway_name],
                    [f_status],
                    [f_gateway_txid],
                    [f_payment_date],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_payment] WITH (NOLOCK) 
                WHERE 
                    [f_delete_date] IS NULL";

            return await _connection.QueryAsync<PaymentModel>(sql, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<PaymentModel?> Find(Guid uid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_order_uid],
                    [f_amount],
                    [f_currency],
                    [f_method],
                    [f_gateway_name],
                    [f_status],
                    [f_gateway_txid],
                    [f_payment_date],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_payment] WITH (NOLOCK) 
                WHERE 
                    [f_uid] = @Uid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryFirstOrDefaultAsync<PaymentModel>(sql, new { Uid = uid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<IEnumerable<PaymentModel>> GetByOrder(Guid orderUid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_order_uid],
                    [f_amount],
                    [f_currency],
                    [f_method],
                    [f_gateway_name],
                    [f_status],
                    [f_gateway_txid],
                    [f_payment_date],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_payment] WITH (NOLOCK) 
                WHERE 
                    [f_order_uid] = @OrderUid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryAsync<PaymentModel>(sql, new { OrderUid = orderUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<PaymentModel> Add(PaymentModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_create_date = DateTime.UtcNow;
            model.f_create_by = _userUid;

            var sql = $@"
                INSERT INTO [{_schema}].[t_payment] 
                (
                    [f_uid], 
                    [f_order_uid], 
                    [f_amount], 
                    [f_currency], 
                    [f_method], 
                    [f_gateway_name], 
                    [f_status], 
                    [f_gateway_txid], 
                    [f_payment_date], 
                    [f_create_date], 
                    [f_create_by]
                )
                VALUES 
                (
                    @f_uid, 
                    @f_order_uid, 
                    @f_amount, 
                    @f_currency, 
                    @f_method, 
                    @f_gateway_name, 
                    @f_status, 
                    @f_gateway_txid, 
                    @f_payment_date, 
                    @f_create_date, 
                    @f_create_by
                )";

            var parameters = new
            {
                model.f_uid,
                model.f_order_uid,
                model.f_amount,
                model.f_currency,
                model.f_method,
                model.f_gateway_name,
                f_status = model.f_status.ToString(),
                model.f_gateway_txid,
                model.f_payment_date,
                model.f_create_date,
                model.f_create_by
            };

            await _connection.ExecuteAsync(sql, parameters, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<PaymentModel> Update(PaymentModel model)
        {
            model.f_update_date = DateTime.UtcNow;
            model.f_update_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_payment]
                SET 
                    [f_status] = @f_status, 
                    [f_gateway_txid] = @f_gateway_txid, 
                    [f_payment_date] = @f_payment_date, 
                    [f_update_date] = @f_update_date, 
                    [f_update_by] = @f_update_by
                WHERE 
                    [f_uid] = @f_uid AND 
                    [f_delete_date] IS NULL";

            var parameters = new
            {
                model.f_uid,
                f_status = model.f_status.ToString(),
                model.f_gateway_txid,
                model.f_payment_date,
                model.f_update_date,
                model.f_update_by
            };

            await _connection.ExecuteAsync(sql, parameters, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<int> Remove(PaymentModel model)
        {
            model.f_delete_date = DateTime.UtcNow;
            model.f_delete_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_payment]
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