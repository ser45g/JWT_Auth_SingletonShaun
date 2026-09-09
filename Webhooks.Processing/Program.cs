using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyJwtAuthServer.Contracts.Events.Webhooks;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using System.Text;
using Webhooks.Processing.Consumers;
using Webhooks.Processing.Data;
using Webhooks.Processing.Models;
using Webhooks.Processing.OpenTelemetry;
using Webhooks.Processing.Options;
using Webhooks.Processing.Webhooks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<RabbitMqOptions>().Bind(builder.Configuration.GetRequiredSection(RabbitMqOptions.ConfigurationSection)).ValidateDataAnnotations().ValidateOnStart();

builder.Services.AddOptions<AuthenticationOptions>().Bind(builder.Configuration.GetRequiredSection(AuthenticationOptions.ConfigurationSection)).ValidateDataAnnotations().ValidateOnStart();

var rabbitMqOptions = builder.Configuration.GetRequiredSection(RabbitMqOptions.ConfigurationSection).Get<RabbitMqOptions>();
ArgumentNullException.ThrowIfNull(rabbitMqOptions, nameof(rabbitMqOptions));

var authenticationOptions = builder.Configuration.GetRequiredSection(AuthenticationOptions.ConfigurationSection).Get<AuthenticationOptions>();

ArgumentNullException.ThrowIfNull(authenticationOptions, nameof(authenticationOptions));

var dbConnectionString = builder.Configuration.GetConnectionString(nameof(WebhooksDbContext));

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddDbContext<WebhooksDbContext>(o => {
    o.UseNpgsql(dbConnectionString);
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters()
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationOptions.AccessTokenSecret)),
        ValidIssuer = authenticationOptions.Issuer,
        ValidAudience = authenticationOptions.Audience,
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("admin", policy => policy.RequireRole("Admin"));
    //.AddPolicy("webhook-subscriber", policy => policy.RequireRole("webhook-subscriber"));

builder.Services.AddOpenApi();

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<WebhookTriggeredConsumer>();
    configure.AddConsumer<WebhookDispatchedConsumer>();
    configure.AddConsumer<WebhookDispatchablesConsumer>();

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
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

var webhooksGroup = app.MapGroup("webhooks");

webhooksGroup.MapPost("subscriptions", async Task<Results<Created, BadRequest<string>>> (CreateWebhookRequest req,  WebhooksDbContext dbContext, CancellationToken cancellationToken) =>
{
    var subscription = new WebhookSubscription(Guid.NewGuid(), req.EventType, req.WebhookUrl, DateTime.UtcNow);

    try
    {
        dbContext.WebhookSubscriptions.Add(subscription);

        await dbContext.SaveChangesAsync(cancellationToken);
    }catch(Exception ex)
    {
        return TypedResults.BadRequest(ex.Message);
    }

    return TypedResults.Created();
}).RequireAuthorization().WithName("add-subscription").WithDescription("Allows users to add a webhook subscription");

if (builder.Environment.IsDevelopment())
{
    webhooksGroup.MapGet("test", async Task<Ok> (IPublishEndpoint publishEndpoint, CancellationToken cancellationToken) =>
    {
        await publishEndpoint.Publish(new WebhookDispatchedEvent("test.event", new { Message = $"This is a test event. {DateTime.UtcNow}" }, null), cancellationToken);

        return TypedResults.Ok();
    });

    webhooksGroup.MapGet("test-connection", async Task<Ok> (IHttpClientFactory clientFactory, CancellationToken cancellationToken) =>
    {
        using var client = clientFactory.CreateClient();

        await client.GetAsync("https://webhooks.client:443/health", cancellationToken);
        return TypedResults.Ok();
    });
}

using (var scope = app.Services.CreateScope())
{
    await using var webhooksDbContext = scope.ServiceProvider.GetService<WebhooksDbContext>();
    ArgumentNullException.ThrowIfNull(webhooksDbContext, nameof(webhooksDbContext));
    await webhooksDbContext.Database.MigrateAsync();
}

app.Run();
