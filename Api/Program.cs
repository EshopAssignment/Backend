using Application.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

//Cors Configuration.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173",
                "http://127.0.0.1:5173",
                "https://localhost:5173",
                "https://127.0.0.1:5173"
            )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();


//Configure DbContext(SQLServer)
builder.Services.AddDbContext<PallshoppenDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();
builder.Services.AddScoped<IAdminProductService, AdminProductService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

//Register HostedService(Background seeder)
builder.Services.AddHostedService<DatabaseInitializerHostedService>();



var app = builder.Build();

app.MapOpenApi("/openapi.json");

//Scalar API Reference
app.MapScalarApiReference(options =>
{
    options.Title = "Pallshop API";
    options.Theme = ScalarTheme.BluePlanet;
    options.WithOpenApiRoutePattern("/openapi.json");
}).ExcludeFromDescription();

app.MapGet("/", () => Results.Redirect("/scalar"))
    .ExcludeFromDescription();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseCors("AllowFrontend");


app.UseAuthorization();

app.MapControllers();

app.Run();
