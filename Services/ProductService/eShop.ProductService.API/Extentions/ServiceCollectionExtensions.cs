using eShop.ProductService.BusinessLogic.Infrastructure;
using eShop.ProductService.BusinessLogic.Interfaces;
using eShop.ProductService.BusinessLogic.Repositories;
using eShop.ProductService.BusinessLogic.Services;
using eShop.Shared.Models;
using ProductServiceImpl = eShop.ProductService.BusinessLogic.Services.ProductService;

namespace eShop.ProductService.API.Extentions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProductInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<InvokeDataModel>(sp => new InvokeDataModel
            {
                userUid = Guid.Empty,
                schema = "dbo",
                commandTimeout = 30,
                configuration = sp.GetRequiredService<IConfiguration>()
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();

            services.AddScoped<IProductService, ProductServiceImpl>();
            services.AddScoped<IProductCategoryService, ProductCategoryService>();

            return services;
        }
    }
}