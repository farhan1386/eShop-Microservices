using eShop.Shared.Models;
using eShop.ShippingService.BusinessLogic.Infrastructure;
using eShop.ShippingService.BusinessLogic.Interfaces;
using eShop.ShippingService.BusinessLogic.Repositories;
using ShippingServiceImpl = eShop.ShippingService.BusinessLogic.Services.ShippingService;

namespace eShop.ShippingService.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddShippingInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<InvokeDataModel>(sp => new InvokeDataModel
            {
                userUid = Guid.Empty,
                schema = "dbo",
                commandTimeout = 30,
                configuration = sp.GetRequiredService<IConfiguration>()
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IShippingRepository, ShippingRepository>();
            services.AddScoped<IShippingService, ShippingServiceImpl>();

            return services;
        }
    }
}