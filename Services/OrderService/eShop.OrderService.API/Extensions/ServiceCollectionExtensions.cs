using eShop.OrderService.BusinessLogic.Infrastructure;
using eShop.OrderService.BusinessLogic.Interfaces;
using eShop.OrderService.BusinessLogic.Repositories;
using eShop.OrderService.BusinessLogic.Services;
using eShop.Shared.Models;
using OrderServiceImpl = eShop.OrderService.BusinessLogic.Services.OrderService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace eShop.OrderService.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrderInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<InvokeDataModel>(sp => new InvokeDataModel
            {
                userUid = Guid.Empty,
                schema = "dbo",
                commandTimeout = 30,
                configuration = sp.GetRequiredService<IConfiguration>()
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderHistoryRepository, OrderHistoryRepository>();
            services.AddScoped<IOutboxEventRepository, OutboxEventRepository>();

            services.AddScoped<IOrderService, OrderServiceImpl>();

            services.AddHostedService<OutboxPublisherWorker>();

            return services;
        }
    }
}