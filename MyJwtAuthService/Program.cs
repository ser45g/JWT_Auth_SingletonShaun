using EFCore.PostgresExtensions.Extensions;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyJwtAuthService.Data;
using MyJwtAuthService.Endpoints;
using MyJwtAuthService.Extensions;
using MyJwtAuthService.Jobs;
using MyJwtAuthService.Models;
using MyJwtAuthService.Options;
using MyJwtAuthService.Outbox;
using MyJwtAuthService.Services.Authenticators;
using MyJwtAuthService.Services.EmailSenders;
using MyJwtAuthService.Services.RefreshTokenRepositories;
using MyJwtAuthService.Services.TokenGenerators;
using MyJwtAuthService.Services.TokenValidators;
using MyJwtAuthService.Webhooks;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Quartz;
using Scalar.AspNetCore;
using System.Text;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidationOptions(builder.Configuration);

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

var authenticationConfiguration = builder.Configuration.GetSection("Authentication").Get<AuthenticationOptions>();
ArgumentNullException.ThrowIfNull(authenticationConfiguration, nameof(authenticationConfiguration));

var outboxBacgroundServiceConfiguration = builder.Configuration.GetSection("OutboxBackgroundService").Get<OutboxBackgroundServiceOptions>();
ArgumentNullException.ThrowIfNull(outboxBacgroundServiceConfiguration, nameof(outboxBacgroundServiceConfiguration));

var rateLimitingOptions = builder.Configuration.GetSection("RateLimitingOptions").Get<RateLimitingOptions>();
ArgumentNullException.ThrowIfNull(rateLimitingOptions, nameof(rateLimitingOptions));

var rabbitMqOptions = builder.Configuration.GetRequiredSection(RabbitMqOptions.ConfigurationSection).Get<RabbitMqOptions>();
ArgumentNullException.ThrowIfNull(rabbitMqOptions);

var identityDbConnectionString = builder.Configuration.GetConnectionString(nameof(AppIdentityDbContext));
var quartzDbConnectionString = builder.Configuration.GetConnectionString("Quartz");

builder.Services.AddDbContext<AppIdentityDbContext>(o => {
    o.UseNpgsql(identityDbConnectionString).UseQueryLocks();
});

builder.Services.AddMassTransit(configure =>
{

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

builder.Services.AddScoped<WebhookDispatcher>();

builder.Services.AddMediatR(o =>
{
    o.RegisterServicesFromAssemblyContaining<Program>();
});

builder.Services.AddQuartz(options =>
{
    var jobKey = "outbox-job";

    options.AddJob<OutboxBackgroundJob>(options =>
    {
        options.WithIdentity(new JobKey(jobKey));
    });

    options.AddTrigger(opts => opts
      .ForJob(jobKey)
      .StartNow()
      .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(outboxBacgroundServiceConfiguration.IntervalSeconds)).RepeatForever())
    );

    options.UsePersistentStore(c =>
    {
        c.UseNewtonsoftJsonSerializer();
        c.ProvisionSchema();
        c.UsePostgres(postgres =>
        {
            postgres.ConnectionString = quartzDbConnectionString;
        });
    });
});

builder.Services.AddQuartzHostedService(options =>
{
    // When shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = true;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddAppRateLimiting(rateLimitingOptions);

builder.Services.AddIdentityCore<ApplicationUser>().AddRoles<Role>().AddSignInManager<SignInManager<ApplicationUser>>().AddDefaultTokenProviders().AddEntityFrameworkStores<AppIdentityDbContext>();

builder.Services.AddScoped<AccessTokenGenerator>();
builder.Services.AddScoped<RefreshTokenGenerator>();
builder.Services.AddScoped<RefreshTokenValidator>();
builder.Services.AddScoped<Authenticator>();
builder.Services.AddScoped<TokenGenerator>();
builder.Services.AddScoped<IRefreshTokenRepository, DatabaseRefreshTokenRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailSender<ApplicationUser>, EmailSender>();
builder.Services.AddScoped<IApplicationLinkGenerator, ApplicationLinkGenerator>();

builder.Services.AddScoped<OutboxProcessor>();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
        ctx.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        ctx.ProblemDetails.Instance = $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";
    };
});

builder.Services.AddSingleton<ProblemDetailsFactory, DefaultProblemDetailsFactory>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters()
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationConfiguration.AccessTokenSecret)),
        ValidIssuer = authenticationConfiguration.Issuer,
        ValidAudience = authenticationConfiguration.Audience,
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options => {});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseRateLimiter();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();

app.AddAuthenticationEndpoints();
app.AddWebhookEndpoints();

using (var scope = app.Services.CreateScope())
{
    await using var identityContext = scope.ServiceProvider.GetService<AppIdentityDbContext>();
    ArgumentNullException.ThrowIfNull(identityContext, nameof(identityContext));
    await identityContext.Database.MigrateAsync();
}

app.Run();

public partial class Program { }