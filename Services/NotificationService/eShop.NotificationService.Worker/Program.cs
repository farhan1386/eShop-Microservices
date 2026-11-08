using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitConfig = builder.Configuration.GetSection("RabbitMqSettings");

        cfg.Host(
            rabbitConfig["Host"] ?? "localhost",
            rabbitConfig["VirtualHost"] ?? "/",
            h =>
            {
                h.Username(rabbitConfig["Username"] ?? "guest");
                h.Password(rabbitConfig["Password"] ?? "guest");
            });

        cfg.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)));

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();