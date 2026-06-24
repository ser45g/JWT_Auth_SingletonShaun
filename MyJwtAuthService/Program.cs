using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyJwtAuthService.Data;
using MyJwtAuthService.Endpoints;
using MyJwtAuthService.Models;
using MyJwtAuthService.Services.Authenticators;
using MyJwtAuthService.Services.EmailSenders;
using MyJwtAuthService.Services.RefreshTokenRepositories;
using MyJwtAuthService.Services.TokenGenerators;
using MyJwtAuthService.Services.TokenValidators;
using Scalar.AspNetCore;
using System.Text;
using Microsoft.AspNetCore.Identity;
using MyJwtAuthService.Outbox;
using MyJwtAuthService.Options;
using MyJwtAuthService.Extensions;
using EFCore.PostgresExtensions.Extensions;
using Hangfire;
using Hangfire.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidationOptions(builder.Configuration);

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

var authenticationConfiguration = builder.Configuration.GetSection("Authentication").Get<AuthenticationOptions>();
ArgumentNullException.ThrowIfNull(authenticationConfiguration, nameof(authenticationConfiguration));

var outboxBacgroundServiceConfiguration = builder.Configuration.GetSection("OutboxBackgroundService").Get<OutboxBackgroundServiceOptions>();
ArgumentNullException.ThrowIfNull(outboxBacgroundServiceConfiguration, nameof(outboxBacgroundServiceConfiguration));

var identityDbConnectionString = builder.Configuration.GetConnectionString(nameof(AppIdentityDbContext));
var hangfireDbConnectionString = builder.Configuration.GetConnectionString("Hangfire");

builder.Services.AddDbContext<AppIdentityDbContext>(o => {
    o.UseNpgsql(identityDbConnectionString).UseQueryLocks();
});

builder.Services.AddDbContext<HangfireDbContext>(o => {
    o.UseNpgsql(hangfireDbConnectionString);
});

builder.Services.AddOpenApi();

builder.Services.AddMediatR(o =>
{
    o.RegisterServicesFromAssemblyContaining<Program>();
});

builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180).UseSimpleAssemblyNameTypeSerializer().UseRecommendedSerializerSettings().UsePostgreSqlStorage(o =>
          {
              o.UseNpgsqlConnection(hangfireDbConnectionString);
          }, new PostgreSqlStorageOptions
          {
              PrepareSchemaIfNecessary = true,
              SchemaName = "Schema"
          });
});
builder.Services.AddHangfireServer(o =>
{
    o.SchedulePollingInterval = TimeSpan.FromSeconds(outboxBacgroundServiceConfiguration.IntervalSeconds);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

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
        // Always include useful metadata
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
        ctx.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        ctx.ProblemDetails.Instance = $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";
    };
});

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
    app.UseHangfireDashboard();
    app.MapHangfireDashboard("/hangfire");
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();

app.AddAuthenticationEndpoints();

using (var scope = app.Services.CreateScope())
{
    await using var identityContext = scope.ServiceProvider.GetService<AppIdentityDbContext>();
    ArgumentNullException.ThrowIfNull(identityContext, nameof(identityContext));
    await identityContext.Database.MigrateAsync();

    await using var hangfireContext = scope.ServiceProvider.GetService<HangfireDbContext>();
    ArgumentNullException.ThrowIfNull(hangfireContext, nameof(hangfireContext));
    await hangfireContext.Database.MigrateAsync();

    var recurringJobManager = scope.ServiceProvider.GetService<IRecurringJobManager>();
    
    recurringJobManager.AddOrUpdate<OutboxProcessor>("outbox-job", x => x.ProcessOutboxMessagesAsync(CancellationToken.None), $"*/{outboxBacgroundServiceConfiguration.IntervalSeconds} * * * * *");
}

app.Run();


public partial class Program { }