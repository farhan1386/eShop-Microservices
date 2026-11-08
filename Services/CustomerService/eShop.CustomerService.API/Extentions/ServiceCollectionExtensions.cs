using eShop.CustomerService.BusinessLogic.Infrastructure;
using eShop.CustomerService.BusinessLogic.Interfaces;
using eShop.CustomerService.BusinessLogic.Repositories;
using eShop.Shared.Models;
using CustomerServiceImpl = eShop.CustomerService.BusinessLogic.Services.CustomerService;

namespace eShop.CustomerService.API.Extentions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomerInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<InvokeDataModel>(sp => new InvokeDataModel
            {
                userUid = Guid.Empty,
                schema = "dbo",
                commandTimeout = 30,
                configuration = sp.GetRequiredService<IConfiguration>()
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICustomerAddressRepository, CustomerAddressRepository>();
            services.AddScoped<ICustomerService, CustomerServiceImpl>();

            return services;
        }
    }
}
