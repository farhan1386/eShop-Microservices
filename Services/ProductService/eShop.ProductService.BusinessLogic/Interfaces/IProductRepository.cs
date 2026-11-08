using eShop.ProductService.Models;

namespace eShop.ProductService.BusinessLogic.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductModel>> Get();
        Task<ProductModel?> Find(Guid uid);
        Task<ProductModel?> FindBySku(string sku);
        Task<IEnumerable<ProductModel>> GetByCategory(Guid categoryUid);
        Task<ProductModel> Add(ProductModel model);
        Task<ProductModel> Update(ProductModel model);
        Task<int> Remove(ProductModel model);
    }
}