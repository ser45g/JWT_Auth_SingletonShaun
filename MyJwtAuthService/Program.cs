using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5174").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddIdentityCore<ApplicationUser>(o =>
{
    o.User.RequireUniqueEmail = true;
    o.Password.RequireDigit = false;
    o.Password.RequireNonAlphanumeric = false;
    o.Password.RequireUppercase = false;
    o.Password.RequiredLength = 0;
}).AddRoles<Role>().AddEntityFrameworkStores<AppIdentityDbContext>();

var authenticationConfiguration = builder.Configuration.GetSection("Authentication")
            .Get<AuthenticationConfiguration>();

ArgumentNullException.ThrowIfNull(authenticationConfiguration, nameof(authenticationConfiguration));

builder.Services.AddSingleton(authenticationConfiguration);

builder.Services.AddScoped<AccessTokenGenerator>();
builder.Services.AddScoped<RefreshTokenGenerator>();
builder.Services.AddScoped<RefreshTokenValidator>();
builder.Services.AddScoped<Authenticator>();
builder.Services.AddScoped<TokenGenerator>();
builder.Services.AddScoped<IRefreshTokenRepository, DatabaseRefreshTokenRepository>();

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseStatusCodePages();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetService<AppIdentityDbContext>();
    ArgumentNullException.ThrowIfNull(context, nameof(context));
    using (context) {
        await context.Database.MigrateAsync();
    }
}
   
app.Run();
