using Dapper;
using eShop.InventoryService.BusinessLogic.Infrastructure;
using eShop.InventoryService.BusinessLogic.Interfaces;
using eShop.InventoryService.Models;

namespace eShop.InventoryService.BusinessLogic.Repositories
{
    public class InventoryRepository : BaseRepository, IInventoryRepository
    {
        public InventoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<InventoryStockModel>> Get()
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_product_uid],
                    [f_quantity],
                    [f_reserved],
                    [f_row_version],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_inventory_stock] WITH (NOLOCK) 
                WHERE 
                    [f_delete_date] IS NULL";

            return await _connection.QueryAsync<InventoryStockModel>(sql, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<InventoryStockModel?> Find(Guid uid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_product_uid],
                    [f_quantity],
                    [f_reserved],
                    [f_row_version],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_inventory_stock] WITH (NOLOCK) 
                WHERE 
                    [f_uid] = @Uid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryFirstOrDefaultAsync<InventoryStockModel>(sql, new { Uid = uid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<InventoryStockModel?> FindByProduct(Guid productUid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_product_uid],
                    [f_quantity],
                    [f_reserved],
                    [f_row_version],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_inventory_stock] WITH (NOLOCK) 
                WHERE 
                    [f_product_uid] = @ProductUid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryFirstOrDefaultAsync<InventoryStockModel>(sql, new { ProductUid = productUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<InventoryStockModel> Add(InventoryStockModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_create_date = DateTime.UtcNow;
            model.f_create_by = _userUid;

            var sql = $@"
                INSERT INTO [{_schema}].[t_inventory_stock] 
                (
                    [f_uid], 
                    [f_product_uid], 
                    [f_quantity], 
                    [f_reserved], 
                    [f_create_date], 
                    [f_create_by]
                )
                OUTPUT INSERTED.[f_row_version]
                VALUES 
                (
                    @f_uid, 
                    @f_product_uid, 
                    @f_quantity, 
                    @f_reserved, 
                    @f_create_date, 
                    @f_create_by
                )";

            model.f_row_version = await _connection.QuerySingleAsync<byte[]>(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<InventoryStockModel?> Update(InventoryStockModel model)
        {
            model.f_update_date = DateTime.UtcNow;
            model.f_update_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_inventory_stock]
                SET 
                    [f_quantity] = @f_quantity, 
                    [f_reserved] = @f_reserved, 
                    [f_update_date] = @f_update_date, 
                    [f_update_by] = @f_update_by
                OUTPUT INSERTED.[f_row_version]
                WHERE 
                    [f_uid] = @f_uid AND 
                    [f_row_version] = @f_row_version AND 
                    [f_delete_date] IS NULL";

            var newRowVersion = await _connection.QueryFirstOrDefaultAsync<byte[]>(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            if (newRowVersion is null)
            {
                return null;
            }

            model.f_row_version = newRowVersion;
            return model;
        }

        public async Task<int> Remove(InventoryStockModel model)
        {
            model.f_delete_date = DateTime.UtcNow;
            model.f_delete_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_inventory_stock]
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