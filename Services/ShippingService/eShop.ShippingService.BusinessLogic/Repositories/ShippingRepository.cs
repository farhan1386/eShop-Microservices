using Dapper;
using eShop.ShippingService.BusinessLogic.Infrastructure;
using eShop.ShippingService.BusinessLogic.Interfaces;
using eShop.ShippingService.Models;

namespace eShop.ShippingService.BusinessLogic.Repositories
{
    public class ShippingRepository : BaseRepository, IShippingRepository
    {
        public ShippingRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<ShippingModel>> Get()
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_order_uid],
                    [f_carrier_name],
                    [f_tracking_number],
                    [f_shipping_status],
                    [f_estimated_delivery],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_shipping_manifest] WITH (NOLOCK) 
                WHERE 
                    [f_delete_date] IS NULL";

            return await _connection.QueryAsync<ShippingModel>(sql, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<ShippingModel?> Find(Guid uid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_order_uid],
                    [f_carrier_name],
                    [f_tracking_number],
                    [f_shipping_status],
                    [f_estimated_delivery],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_shipping_manifest] WITH (NOLOCK) 
                WHERE 
                    [f_uid] = @Uid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryFirstOrDefaultAsync<ShippingModel>(sql, new { Uid = uid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<ShippingModel?> FindByOrderUid(Guid orderUid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_order_uid],
                    [f_carrier_name],
                    [f_tracking_number],
                    [f_shipping_status],
                    [f_estimated_delivery],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_shipping_manifest] WITH (NOLOCK) 
                WHERE 
                    [f_order_uid] = @OrderUid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryFirstOrDefaultAsync<ShippingModel>(sql, new { OrderUid = orderUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<bool> IsOrderTracked(Guid orderUid)
        {
            var sql = $@"
                SELECT TOP 1 
                    1 
                FROM 
                    [{_schema}].[t_shipping_manifest] WITH (NOLOCK) 
                WHERE 
                    [f_order_uid] = @OrderUid";

            var exists = await _connection.ExecuteScalarAsync<int?>(sql, new { OrderUid = orderUid }, transaction: _transaction, commandTimeout: _commandTimeout);
            return exists.HasValue;
        }

        public async Task<ShippingModel> Add(ShippingModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_create_date = DateTime.UtcNow;
            model.f_create_by = _userUid;

            var sql = $@"
                INSERT INTO [{_schema}].[t_shipping_manifest] 
                (
                    [f_uid], 
                    [f_order_uid], 
                    [f_carrier_name], 
                    [f_tracking_number], 
                    [f_shipping_status], 
                    [f_estimated_delivery], 
                    [f_create_date], 
                    [f_create_by]
                )
                VALUES 
                (
                    @f_uid, 
                    @f_order_uid, 
                    @f_carrier_name, 
                    @f_tracking_number, 
                    @f_shipping_status, 
                    @f_estimated_delivery, 
                    @f_create_date, 
                    @f_create_by
                )";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<ShippingModel> Update(ShippingModel model)
        {
            model.f_update_date = DateTime.UtcNow;
            model.f_update_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_shipping_manifest]
                SET 
                    [f_carrier_name] = @f_carrier_name,
                    [f_tracking_number] = @f_tracking_number,
                    [f_shipping_status] = @f_shipping_status,
                    [f_estimated_delivery] = @f_estimated_delivery,
                    [f_update_date] = @f_update_date, 
                    [f_update_by] = @f_update_by
                WHERE 
                    [f_uid] = @f_uid AND 
                    [f_delete_date] IS NULL";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<int> Remove(ShippingModel model)
        {
            model.f_delete_date = DateTime.UtcNow;
            model.f_delete_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_shipping_manifest]
                SET 
                    [f_delete_date] = @f_delete_date, 
                    [f_delete_by] = @f_delete_by
                WHERE 
                    [f_uid] = @f_uid AND 
                    [f_delete_date] IS NULL";

            return await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<IEnumerable<ShippingHistoryModel>> GetHistory(Guid shippingManifestUid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_shipping_manifest_uid],
                    [f_old_status],
                    [f_new_status],
                    [f_status_location],
                    [f_comment],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_shipping_history] WITH (NOLOCK) 
                WHERE 
                    [f_shipping_manifest_uid] = @ShippingManifestUid AND
                    [f_delete_date] IS NULL";

            return await _connection.QueryAsync<ShippingHistoryModel>(sql, new { ShippingManifestUid = shippingManifestUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<ShippingHistoryModel> AddHistory(ShippingHistoryModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_create_date = DateTime.UtcNow;
            model.f_create_by = _userUid;

            var sql = $@"
                INSERT INTO [{_schema}].[t_shipping_history] 
                (
                    [f_uid], 
                    [f_shipping_manifest_uid], 
                    [f_old_status], 
                    [f_new_status], 
                    [f_status_location], 
                    [f_comment], 
                    [f_create_date], 
                    [f_create_by]
                )
                VALUES 
                (
                    @f_uid, 
                    @f_shipping_manifest_uid, 
                    @f_old_status, 
                    @f_new_status, 
                    @f_status_location, 
                    @f_comment, 
                    @f_create_date, 
                    @f_create_by
                )";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }
    }
}