using System.Text;
using Application.Interfaces;
using Application.Interfaces.Auth;
using Domain.Entities.Identity;
using Infrastructure.Auth;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

//Cors Configuration.
builder.Services.AddCors(o =>
    o.AddPolicy("AllowFrontend", p => p
        .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    )
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

//Configure DbContext(SQLServer)
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PallshoppenDbContext>(options =>
    options.UseSqlServer(conn, x =>
    {
        x.MigrationsAssembly("Infrastructure");
        x.MigrationsHistoryTable("__EFMigrationsHistory", "core");
    }));
builder.Services.AddDbContext<AuthDbContext>(options =>
        options.UseSqlServer(conn, x =>
        {
            x.MigrationsAssembly("Infrastructure");
            x.MigrationsHistoryTable("__EFMigrationsHistory", "auth");
        })); //jaja byt sen jag är inte rik. 

builder.Services.AddIdentity<User, AppRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

//Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();
builder.Services.AddScoped<IAdminProductService, AdminProductService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

//Register HostedService(Background seeder)
builder.Services.AddHostedService<DatabaseInitializerHostedService>();
//Token Service

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ITokenRefreshStore, TokenRefreshStore>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddAuthentication()
    .AddJwtBearer(o =>
    {
        var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
        o.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
        o.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var access = ctx.HttpContext.Request.Cookies["access_token"];
                if (!string.IsNullOrEmpty(access)) ctx.Token = access;
                return Task.CompletedTask;
            }
        };
    });


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


app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();
