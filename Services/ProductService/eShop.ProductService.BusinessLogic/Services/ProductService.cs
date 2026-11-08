using eShop.ProductService.BusinessLogic.Interfaces;
using eShop.ProductService.Models;

namespace eShop.ProductService.BusinessLogic.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public Task<IEnumerable<ProductModel>> GetAllAsync()
        {
            return _productRepository.Get();
        }

        public Task<ProductModel?> GetByUidAsync(Guid uid)
        {
            return _productRepository.Find(uid);
        }

        public Task<ProductModel?> GetBySkuAsync(string sku)
        {
            return _productRepository.FindBySku(sku);
        }

        public Task<IEnumerable<ProductModel>> GetByCategoryAsync(Guid categoryUid)
        {
            return _productRepository.GetByCategory(categoryUid);
        }

        public async Task<ProductModel> CreateAsync(ProductModel product)
        {
            var existing = await _productRepository.FindBySku(product.f_sku);
            if (existing is not null)
            {
                throw new InvalidOperationException($"A product with SKU '{product.f_sku}' already exists.");
            }

            return await _productRepository.Add(product);
        }

        public async Task<ProductModel?> UpdateAsync(Guid uid, ProductModel product)
        {
            var existing = await _productRepository.Find(uid);
            if (existing is null)
            {
                return null;
            }

            existing.f_category_uid = product.f_category_uid;
            existing.f_name = product.f_name;
            existing.f_price = product.f_price;
            existing.f_attributes = product.f_attributes;

            return await _productRepository.Update(existing);
        }

        public async Task<int> DeleteAsync(Guid uid)
        {
            var existing = await _productRepository.Find(uid);
            if (existing is null)
            {
                return 0;
            }

            return await _productRepository.Remove(existing);
        }
    }
}