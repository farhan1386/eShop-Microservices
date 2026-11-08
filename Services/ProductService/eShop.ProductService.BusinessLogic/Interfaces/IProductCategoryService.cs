using eShop.ProductService.Models;

namespace eShop.ProductService.BusinessLogic.Interfaces
{
    public interface IProductCategoryService
    {
        Task<IEnumerable<ProductCategory>> GetAllAsync();
        Task<ProductCategory?> GetByUidAsync(Guid uid);
        Task<ProductCategory> CreateAsync(ProductCategory category);
        Task<ProductCategory?> UpdateAsync(Guid uid, ProductCategory category);
        Task<int> DeleteAsync(Guid uid);
    }
}