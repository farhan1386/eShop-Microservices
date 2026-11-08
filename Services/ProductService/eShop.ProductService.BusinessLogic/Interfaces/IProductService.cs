using eShop.ProductService.Models;

namespace eShop.ProductService.BusinessLogic.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductModel>> GetAllAsync();
        Task<ProductModel?> GetByUidAsync(Guid uid);
        Task<ProductModel?> GetBySkuAsync(string sku);
        Task<IEnumerable<ProductModel>> GetByCategoryAsync(Guid categoryUid);
        Task<ProductModel> CreateAsync(ProductModel product);
        Task<ProductModel?> UpdateAsync(Guid uid, ProductModel product);
        Task<int> DeleteAsync(Guid uid);
    }
}
