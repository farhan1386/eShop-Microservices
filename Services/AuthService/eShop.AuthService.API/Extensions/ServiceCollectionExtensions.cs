using eShop.Shared.Models;
using eShop.AuthService.BusinessLogic.Infrastructure;
using eShop.AuthService.BusinessLogic.Interfaces;
using eShop.AuthService.BusinessLogic.Repositories;
using AuthServiceImpl = eShop.AuthService.BusinessLogic.Services.AuthService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace eShop.AuthService.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<InvokeDataModel>(sp => new InvokeDataModel
            {
                userUid = Guid.Empty,
                schema = "dbo",
                commandTimeout = 30,
                configuration = sp.GetRequiredService<IConfiguration>()
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IAuthService, AuthServiceImpl>();

            return services;
        }
    }
}