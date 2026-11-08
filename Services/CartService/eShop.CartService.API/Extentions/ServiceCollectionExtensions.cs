using eShop.Shared.Models;
using eShop.CartService.BusinessLogic.Infrastructure;
using eShop.CartService.BusinessLogic.Interfaces;
using eShop.CartService.BusinessLogic.Repositories;
using CartServiceImpl = eShop.CartService.BusinessLogic.Services.CartService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace eShop.CartService.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCartInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<InvokeDataModel>(sp => new InvokeDataModel
            {
                userUid = Guid.Empty,
                schema = "dbo",
                commandTimeout = 30,
                configuration = sp.GetRequiredService<IConfiguration>()
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartService, CartServiceImpl>();

            return services;
        }
    }
}