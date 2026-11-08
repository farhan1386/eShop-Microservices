using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eShop.EventBus.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? consumerAssembly = null)
    {
        var settings = new RabbitMqSettings();
        configuration.GetSection(RabbitMqSettings.SectionName).Bind(settings);

        services.AddMassTransit(x =>
        {
            if (consumerAssembly is not null)
            {
                x.AddConsumers(consumerAssembly);
            }

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(settings.Host, settings.VirtualHost, h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });

                cfg.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)));

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventBus, MassTransitEventBus>();

        return services;
    }
}