using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using PatchServer.Data;
using PatchServer.Repositories;
using SharedLibrary.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Repository services
builder.Services.AddScoped<IPatchRepository, PatchRepository>();
builder.Services.AddScoped<ICustomerAgentRepository, CustomerAgentRepository>();
builder.Services.AddScoped<IAgentMonitoredProductsRepository, AgentMonitoredProductsRepository>();
builder.Services.AddScoped<IPatchStatusRepository, PatchStatusRepository>();

// Database context
builder.Services.AddDbContext<PatchDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// Swagger/OpenAPI configuration with explicit OpenAPI version
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Patch Server API",
        Version = "v1",
        Description = "API for managing patches and customer agents"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();



// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Patch Server API V1");
        //c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// ✅ ADD STATIC FILE SERVING FOR PATCHES
var patchFilesPath = @"C:\PatchServer\patches";
Directory.CreateDirectory(patchFilesPath); // Ensure directory exists

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(patchFilesPath),
    RequestPath = "/patches"
});

//app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();