using eShop.ProductService.BusinessLogic.Interfaces;
using eShop.ProductService.Models;

namespace eShop.ProductService.BusinessLogic.Services
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IProductCategoryRepository _categoryRepository;

        public ProductCategoryService(IProductCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public Task<IEnumerable<ProductCategory>> GetAllAsync()
        {
            return _categoryRepository.Get();
        }

        public Task<ProductCategory?> GetByUidAsync(Guid uid)
        {
            return _categoryRepository.Find(uid);
        }

        public async Task<ProductCategory> CreateAsync(ProductCategory category)
        {
            return await _categoryRepository.Add(category);
        }

        public async Task<ProductCategory?> UpdateAsync(Guid uid, ProductCategory category)
        {
            var existing = await _categoryRepository.Find(uid);
            if (existing is null)
            {
                return null;
            }

            existing.f_name = category.f_name;

            return await _categoryRepository.Update(existing);
        }

        public async Task<int> DeleteAsync(Guid uid)
        {
            var existing = await _categoryRepository.Find(uid);
            if (existing is null)
            {
                return 0;
            }

            return await _categoryRepository.Remove(existing);
        }
    }
}