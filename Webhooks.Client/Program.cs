using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseCors();

app.MapHealthChecks("/health");

app.MapPost("webhooks/{eventType}", Ok ([FromRoute] string eventType, object request, ILogger<IRouteBuilder> logger) =>
{
    logger.LogInformation($"{eventType}");
    return TypedResults.Ok();
});



app.UseHttpsRedirection();

app.Run();
