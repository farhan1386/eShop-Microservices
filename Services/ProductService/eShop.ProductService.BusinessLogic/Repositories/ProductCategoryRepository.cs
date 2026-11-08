using Dapper;
using eShop.ProductService.BusinessLogic.Infrastructure;
using eShop.ProductService.BusinessLogic.Interfaces;
using eShop.ProductService.Models;

namespace eShop.ProductService.BusinessLogic.Repositories
{
    public class ProductCategoryRepository : BaseRepository, IProductCategoryRepository
    {
        public ProductCategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<ProductCategory>> Get()
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_name],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by],
                    COUNT(*) OVER() AS [RowCount]
                FROM 
                    [{_schema}].[t_product_category] WITH (NOLOCK)
                WHERE 
                    [f_delete_date] IS NULL";

            return await _connection.QueryAsync<ProductCategory>(sql, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<ProductCategory?> Find(Guid uid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_name],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by],
                    1 AS [RowCount]
                FROM 
                    [{_schema}].[t_product_category] WITH (NOLOCK)
                WHERE 
                    [f_uid] = @Uid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryFirstOrDefaultAsync<ProductCategory>(sql, new { Uid = uid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<ProductCategory> Add(ProductCategory model)
        {
            if (model.f_uid == Guid.Empty)
            {
                model.f_uid = Guid.NewGuid();
            }
            model.f_create_date = DateTime.UtcNow;
            model.f_create_by = _userUid;

            var sql = $@"
                INSERT INTO [{_schema}].[t_product_category] 
                (
                    [f_uid], 
                    [f_name], 
                    [f_create_date], 
                    [f_create_by]
                )
                VALUES 
                (
                    @f_uid, 
                    @f_name, 
                    @f_create_date, 
                    @f_create_by
                )";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<ProductCategory> Update(ProductCategory model)
        {
            model.f_update_date = DateTime.UtcNow;
            model.f_update_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_product_category]
                SET 
                    [f_name] = @f_name,
                    [f_update_date] = @f_update_date,
                    [f_update_by] = @f_update_by
                WHERE 
                    [f_uid] = @f_uid AND 
                    [f_delete_date] IS NULL";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<int> Remove(ProductCategory model)
        {
            model.f_delete_date = DateTime.UtcNow;
            model.f_delete_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_product_category]
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