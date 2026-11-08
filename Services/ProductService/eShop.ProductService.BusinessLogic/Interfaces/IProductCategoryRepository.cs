using eShop.ProductService.Models;

namespace eShop.ProductService.BusinessLogic.Interfaces
{
    public interface IProductCategoryRepository
    {
        Task<IEnumerable<ProductCategory>> Get();
        Task<ProductCategory?> Find(Guid uid);
        Task<ProductCategory> Add(ProductCategory model);
        Task<ProductCategory> Update(ProductCategory model);
        Task<int> Remove(ProductCategory model);
    }
}