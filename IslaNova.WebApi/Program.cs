using IslaNova.Core.Application.IOC;
using IslaNova.Infrastructure.Identity.IOC;
using IslaNova.Infrastructure.Persistence.IOC;
using IslaNova.Infrastructure.Shared.IOC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Layers
builder.Services.AddApplicationLayerIOC();
builder.Services.AddPersistenceLayerIoc(builder.Configuration);
builder.Services.AddIdentityLayerIocForWebApi(builder.Configuration);
builder.Services.AddSharedLayerIoc(builder.Configuration);

var app = builder.Build();

// Seed
await app.Services.RunIdentitySeedAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
