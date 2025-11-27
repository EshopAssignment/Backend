using Application.Interfaces;
using Infrastructure.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

//Cors Configuration.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") 
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

//Register HostedService(Background seeder)
builder.Services.AddHostedService<DatabaseInitializerHostedService>();



var app = builder.Build();

app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    options.Title = "Pallshop API";
    options.Theme = ScalarTheme.Kepler;
}).ExcludeFromDescription();

app.MapGet("/", () => Results.Redirect("/scalar"))
    .ExcludeFromDescription();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowFrontend");


app.UseAuthorization();

app.MapControllers();

app.Run();
