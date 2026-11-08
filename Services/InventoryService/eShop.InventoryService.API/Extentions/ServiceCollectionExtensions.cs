using eShop.InventoryService.BusinessLogic.Infrastructure;
using eShop.InventoryService.BusinessLogic.Interfaces;
using eShop.InventoryService.BusinessLogic.Repositories;
using eShop.Shared.Models;
using InventoryServiceImpl = eShop.InventoryService.BusinessLogic.Services.InventoryService;

namespace eShop.InventoryService.API.Extentions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<InvokeDataModel>(sp => new InvokeDataModel
            {
                userUid = Guid.Empty,
                schema = "dbo",
                commandTimeout = 30,
                configuration = sp.GetRequiredService<IConfiguration>()
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IInventoryService, InventoryServiceImpl>();

            return services;
        }
    }
}