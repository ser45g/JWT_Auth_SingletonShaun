using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyJwtAuthService.Data;
using MyJwtAuthService.Models;
using MyJwtAuthService.Services.Authenticators;
using MyJwtAuthService.Services.RefreshTokenRepositories;
using MyJwtAuthService.Services.TokenGenerators;
using MyJwtAuthService.Services.TokenValidators;
using Scalar.AspNetCore;
using System.Text;

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

builder.Services.AddIdentityCore<ApplicationUser>(o =>
{
    o.User.RequireUniqueEmail = true;

    o.Password.RequireDigit = true;
    o.Password.RequireNonAlphanumeric = true;
    o.Password.RequireUppercase = true;
    o.Password.RequiredLength = 8;

    o.Lockout.MaxFailedAccessAttempts = 5;
    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    
}).AddRoles<Role>().AddSignInManager<SignInManager<ApplicationUser>>().AddEntityFrameworkStores<AppIdentityDbContext>();

builder.Services.AddOptions<AuthenticationConfiguration>().Bind(builder.Configuration.GetSection("Authentication")).ValidateDataAnnotations().ValidateOnStart();

builder.Services.AddScoped<AuthenticationConfiguration>(s=>s.GetRequiredService<IOptions<AuthenticationConfiguration>>().Value);

var authenticationConfiguration = builder.Configuration.GetSection("Authentication").Get<AuthenticationConfiguration>();

builder.Services.AddScoped<AccessTokenGenerator>();
builder.Services.AddScoped<RefreshTokenGenerator>();
builder.Services.AddScoped<RefreshTokenValidator>();
builder.Services.AddScoped<Authenticator>();
builder.Services.AddScoped<TokenGenerator>();
builder.Services.AddScoped<IRefreshTokenRepository, DatabaseRefreshTokenRepository>();

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

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetService<AppIdentityDbContext>();
    ArgumentNullException.ThrowIfNull(context, nameof(context));
    using (context) {
        await context.Database.MigrateAsync();
    }
}
   
app.Run();
