using eShop.PaymentService.BusinessLogic.Infrastructure;
using eShop.PaymentService.BusinessLogic.Interfaces;
using eShop.PaymentService.BusinessLogic.Repositories;
using eShop.Shared.Models;
using PaymentServiceImpl = eShop.PaymentService.BusinessLogic.Services.PaymentService;

namespace eShop.PaymentService.API.Extentions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPaymentInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<InvokeDataModel>(sp => new InvokeDataModel
            {
                userUid = Guid.Empty,
                schema = "dbo",
                commandTimeout = 30,
                configuration = sp.GetRequiredService<IConfiguration>()
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IPaymentService, PaymentServiceImpl>();

            return services;
        }
    }
}