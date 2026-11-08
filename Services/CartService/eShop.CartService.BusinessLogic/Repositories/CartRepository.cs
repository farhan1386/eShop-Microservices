using Dapper;
using eShop.CartService.BusinessLogic.Infrastructure;
using eShop.CartService.BusinessLogic.Interfaces;
using eShop.CartService.Models;

namespace eShop.CartService.BusinessLogic.Repositories
{
    public class CartRepository : BaseRepository, ICartRepository
    {
        public CartRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<CartModel?> FindByCustomerUid(Guid customerUid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_customer_uid],
                    [f_create_date],
                    [f_update_date],
                    [f_create_by],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_cart] WITH (NOLOCK) 
                WHERE 
                    [f_customer_uid] = @CustomerUid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryFirstOrDefaultAsync<CartModel>(sql, new { CustomerUid = customerUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<CartModel> AddCart(CartModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_create_date = DateTime.UtcNow;
            model.f_create_by = _userUid;

            var sql = $@"
                INSERT INTO [{_schema}].[t_cart] 
                (
                    [f_uid], 
                    [f_customer_uid], 
                    [f_create_date], 
                    [f_create_by]
                )
                VALUES 
                (
                    @f_uid, 
                    @f_customer_uid, 
                    @f_create_date, 
                    @f_create_by
                )";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<IEnumerable<CartItemModel>> GetItems(Guid cartUid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_cart_uid],
                    [f_product_uid],
                    [f_quantity],
                    [f_price_snapshot],
                    [f_create_date],
                    [f_update_date],
                    [f_create_by],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_cart_item] WITH (NOLOCK) 
                WHERE 
                    [f_cart_uid] = @CartUid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryAsync<CartItemModel>(sql, new { CartUid = cartUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<CartItemModel?> FindItem(Guid uid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_cart_uid],
                    [f_product_uid],
                    [f_quantity],
                    [f_price_snapshot],
                    [f_create_date],
                    [f_update_date],
                    [f_create_by],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_cart_item] WITH (NOLOCK) 
                WHERE 
                    [f_uid] = @Uid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryFirstOrDefaultAsync<CartItemModel>(sql, new { Uid = uid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<CartItemModel?> FindItemByProduct(Guid cartUid, Guid productUid)
        {
            var sql = $@"
                SELECT 
                    [f_uid],
                    [f_iid],
                    [f_cart_uid],
                    [f_product_uid],
                    [f_quantity],
                    [f_price_snapshot],
                    [f_create_date],
                    [f_update_date],
                    [f_create_by],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_cart_item] WITH (NOLOCK) 
                WHERE 
                    [f_cart_uid] = @CartUid AND 
                    [f_product_uid] = @ProductUid AND 
                    [f_delete_date] IS NULL";

            return await _connection.QueryFirstOrDefaultAsync<CartItemModel>(sql, new { CartUid = cartUid, ProductUid = productUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<CartItemModel> AddItem(CartItemModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_create_date = DateTime.UtcNow;
            model.f_create_by = _userUid;

            var sql = $@"
                INSERT INTO [{_schema}].[t_cart_item] 
                (
                    [f_uid], 
                    [f_cart_uid], 
                    [f_product_uid], 
                    [f_quantity], 
                    [f_price_snapshot], 
                    [f_create_date], 
                    [f_create_by]
                )
                VALUES 
                (
                    @f_uid, 
                    @f_cart_uid, 
                    @f_product_uid, 
                    @f_quantity, 
                    @f_price_snapshot, 
                    @f_create_date, 
                    @f_create_by
                )";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<CartItemModel> UpdateItem(CartItemModel model)
        {
            model.f_update_date = DateTime.UtcNow;
            model.f_update_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_cart_item]
                SET 
                    [f_quantity] = @f_quantity, 
                    [f_price_snapshot] = @f_price_snapshot, 
                    [f_update_date] = @f_update_date, 
                    [f_update_by] = @f_update_by
                WHERE 
                    [f_uid] = @f_uid AND 
                    [f_delete_date] IS NULL";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<int> RemoveItem(CartItemModel model)
        {
            model.f_delete_date = DateTime.UtcNow;
            model.f_delete_by = _userUid;

            var sql = $@"
                UPDATE 
                    [{_schema}].[t_cart_item]
                SET 
                    [f_delete_date] = @f_delete_date, 
                    [f_delete_by] = @f_delete_by
                WHERE 
                    [f_uid] = @f_uid AND 
                    [f_delete_date] IS NULL";

            return await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<int> ClearItems(Guid cartUid)
        {
            var sql = $@"
                UPDATE 
                    [{_schema}].[t_cart_item]
                SET 
                    [f_delete_date] = @DeleteDate, 
                    [f_delete_by] = @DeleteBy
                WHERE 
                    [f_cart_uid] = @CartUid AND 
                    [f_delete_date] IS NULL";

            return await _connection.ExecuteAsync(sql, new { CartUid = cartUid, DeleteDate = DateTime.UtcNow, DeleteBy = _userUid }, transaction: _transaction, commandTimeout: _commandTimeout);
        }
    }
}