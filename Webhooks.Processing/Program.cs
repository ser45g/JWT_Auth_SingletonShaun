using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Webhooks.Processing.Consumers;
using Webhooks.Processing.Data;
using Webhooks.Processing.OpenTelemetry;
using Webhooks.Processing.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<RabbitMqOptions>().Bind(builder.Configuration.GetRequiredSection(RabbitMqOptions.ConfigurationSection)).ValidateDataAnnotations().ValidateOnStart();

var rabbitMqOptions = builder.Configuration.GetRequiredSection(RabbitMqOptions.ConfigurationSection).Get<RabbitMqOptions>();
ArgumentNullException.ThrowIfNull(rabbitMqOptions, nameof(rabbitMqOptions));

var dbConnectionString = builder.Configuration.GetConnectionString(nameof(WebhooksDbContext));

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddDbContext<WebhooksDbContext>(o => {
    o.UseNpgsql(dbConnectionString);
});

builder.Services.AddOpenApi();

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<WebhookTriggeredConsumer>();
    configure.AddConsumer<WebhookDispatchedConsumer>();
    configure.AddConsumer<WebhookSubscriptionAddedConsumer>();

    configure.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqOptions.Url, h =>
        {
            h.Username(rabbitMqOptions.Username);
            h.Password(rabbitMqOptions.Password);
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddOpenApi();

builder.Services.AddHttpClient();

builder.Services.AddOpenTelemetry().ConfigureResource(config =>
{
    config.AddService("MyJwtAuthService");
}).WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource(DiagnosticConfig.ActivitySource.Name)
        .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName);
    tracing.AddOtlpExporter();

}).WithMetrics(metrics =>
{
    metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();

    metrics.AddOtlpExporter();
}).WithLogging(logging =>
{
    logging.AddOtlpExporter();
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    await using var webhooksDbContext = scope.ServiceProvider.GetService<WebhooksDbContext>();
    ArgumentNullException.ThrowIfNull(webhooksDbContext, nameof(webhooksDbContext));
    await webhooksDbContext.Database.MigrateAsync();
}

app.Run();
