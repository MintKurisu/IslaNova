using IslaNova.Core.Application.IOC;
using IslaNova.Infrastructure.AI.IOC;
using IslaNova.Infrastructure.Identity.IOC;
using IslaNova.Infrastructure.Persistence.IOC;
using IslaNova.Infrastructure.Shared.IOC;
using IslaNova.WebApi.Extensions;
using IslaNova.WebApi.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(opt =>
{
    opt.Filters.Add(new ProducesAttribute("application/json"));
}).ConfigureApiBehaviorOptions(opt =>
{
    opt.SuppressInferBindingSourcesForParameters = true;
    opt.SuppressMapClientErrors = true;
}).AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Layers
builder.Services.AddApplicationLayerIOC();
builder.Services.AddPersistenceLayerIoc(builder.Configuration);
builder.Services.AddIdentityLayerIocForWebApi(builder.Configuration);
await builder.Services.AddSharedLayerIocAsync(builder.Configuration);
builder.Services.AddAILayerIoc(builder.Configuration);

// Documentation
builder.Services.AddSwaggerExtension();
builder.Services.AddApiVersioningExtension();

var app = builder.Build();

await app.Services.RunIdentitySeedAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerExtensions(app);
}

app.UseRouting();
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseHealthChecks("/health");
app.MapControllers();

await app.RunAsync();