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
using MyJwtAuthService.BackgroundServices;
using MyJwtAuthService.Outbox;
using MyJwtAuthService.Options;
using MyJwtAuthService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidationOptions(builder.Configuration);

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

var authenticationConfiguration = builder.Configuration.GetSection("Authentication").Get<AuthenticationOptions>();
ArgumentNullException.ThrowIfNull(authenticationConfiguration, nameof(authenticationConfiguration));

builder.Services.AddDbContext<AppIdentityDbContext>(o => {
    o.UseNpgsql(builder.Configuration.GetConnectionString(nameof(AppIdentityDbContext)));
});

builder.Services.AddOpenApi();

builder.Services.AddMediatR(o =>
{
    o.RegisterServicesFromAssemblyContaining<Program>();
});

builder.Services.AddHostedService<OutboxBackgroundService>();

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
    var context = scope.ServiceProvider.GetService<AppIdentityDbContext>();
    ArgumentNullException.ThrowIfNull(context, nameof(context));
    using (context) {
        await context.Database.MigrateAsync();
    }
}

app.Run();


public partial class Program { }