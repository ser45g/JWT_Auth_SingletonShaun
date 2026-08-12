using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyJwtAuthService.Data;
using MyJwtAuthService.Endpoints;
using MyJwtAuthService.Models;
using MyJwtAuthService.Requests;
using MyJwtAuthService.Validators;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppIdentityDbContext>(o => {
    o.UseNpgsql(builder.Configuration.GetConnectionString(nameof(AppIdentityDbContext)));
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddOptions<CorsConfiguration>().Bind(builder.Configuration.GetSection("Cors")).ValidateDataAnnotations().ValidateOnStart();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {

        string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();

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

//To set up cookie auth
builder.Services.AddIdentity<ApplicationUser, Role>(o =>
{
    o.User.RequireUniqueEmail = true;

    o.Password.RequireDigit = true;
    o.Password.RequireNonAlphanumeric = true;
    o.Password.RequireUppercase = true;
    o.Password.RequiredLength = 8;

    o.Lockout.MaxFailedAccessAttempts = 5;
    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

}).AddRoles<Role>().AddSignInManager<SignInManager<ApplicationUser>>().AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultTokenProviders();

// Configure the Identity cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Domain = "localhost";
    options.Cookie.Path = "/";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(14); // This should be set!
    options.SlidingExpiration = true;
    options.LoginPath = "/auth/login"; // Optional
    options.LogoutPath = "/auth/logout"; // Optional
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