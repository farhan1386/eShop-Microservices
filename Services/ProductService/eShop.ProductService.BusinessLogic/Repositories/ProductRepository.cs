using Dapper;
using eShop.ProductService.BusinessLogic.Infrastructure;
using eShop.ProductService.BusinessLogic.Interfaces;
using eShop.ProductService.Models;

namespace eShop.ProductService.BusinessLogic.Repositories
{
    public class ProductRepository : BaseRepository, IProductRepository
    {
        public ProductRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<ProductModel>> Get()
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_category_uid],
                    [f_name],
                    [f_sku],
                    [f_price],
                    [f_attributes],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_product] WITH (NOLOCK) 
                WHERE 
                    [f_delete_date] IS NULL";

            return await _connection.QueryAsync<ProductModel>(sql, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<ProductModel?> Find(Guid uid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_category_uid],
                    [f_name],
                    [f_sku],
                    [f_price],
                    [f_attributes],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_product] WITH (NOLOCK) 
                WHERE 
                    [f_uid] = @Uid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryFirstOrDefaultAsync<ProductModel>(sql, new { Uid = uid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<ProductModel?> FindBySku(string sku)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_category_uid],
                    [f_name],
                    [f_sku],
                    [f_price],
                    [f_attributes],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_product] WITH (NOLOCK) 
                WHERE 
                    [f_sku] = @Sku AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryFirstOrDefaultAsync<ProductModel>(sql, new { Sku = sku }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<IEnumerable<ProductModel>> GetByCategory(Guid categoryUid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_category_uid],
                    [f_name],
                    [f_sku],
                    [f_price],
                    [f_attributes],
                    [f_create_date],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_product] WITH (NOLOCK) 
                WHERE 
                    [f_category_uid] = @CategoryUid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryAsync<ProductModel>(sql, new { CategoryUid = categoryUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<ProductModel> Add(ProductModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_create_date = DateTime.UtcNow;
            model.f_create_by = _userUid;

            var sql = $@"
                INSERT INTO [{_schema}].[t_product] 
                (
                    [f_uid], 
                    [f_category_uid], 
                    [f_name], 
                    [f_sku], 
                    [f_price], 
                    [f_attributes], 
                    [f_create_date], 
                    [f_create_by]
                )
                VALUES 
                (
                    @f_uid, 
                    @f_category_uid, 
                    @f_name, 
                    @f_sku, 
                    @f_price, 
                    @f_attributes, 
                    @f_create_date, 
                    @f_create_by
                )";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<ProductModel> Update(ProductModel model)
        {
            model.f_update_date = DateTime.UtcNow;
            model.f_update_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_product]
                SET 
                    [f_category_uid] = @f_category_uid, 
                    [f_name] = @f_name, 
                    [f_price] = @f_price, 
                    [f_attributes] = @f_attributes, 
                    [f_update_date] = @f_update_date, 
                    [f_update_by] = @f_update_by
                WHERE 
                    [f_uid] = @f_uid AND 
                    [f_delete_date] IS NULL";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<int> Remove(ProductModel model)
        {
            model.f_delete_date = DateTime.UtcNow;
            model.f_delete_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_product]
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