using System.Text;
using System.Text.Json.Serialization;
using Application.Assemblers;
using Application.Interfaces;
using Application.Interfaces.Auth;
using Domain.Entities.Identity;
using Domain.Stripe;
using Infrastructure.Auth;
using Infrastructure.Options;
using Infrastructure.Persistence;
using Infrastructure.Seed;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Stripe;
using InfrastructureProductService = Infrastructure.Services.ProductService;
using InfrastructureTokenService = Infrastructure.Services.TokenService;

var builder = WebApplication.CreateBuilder(args);


//Cors Configuration.
builder.Services.AddCors(o =>
    o.AddPolicy("AllowFrontend", p => p
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    )
);
//Add Controllers & OpenApi

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

//Configure DbContext(SQLServer)
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PallshoppenDbContext>(options =>
    options.UseSqlServer(conn, x =>
    {
        x.MigrationsAssembly("Infrastructure");
        x.MigrationsHistoryTable("__EFMigrationsHistory", "core");
        x.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
    }));
builder.Services.AddDbContext<AuthDbContext>(options =>
        options.UseSqlServer(conn, x =>
        {
            x.MigrationsAssembly("Infrastructure");
            x.MigrationsHistoryTable("__EFMigrationsHistory", "auth");
            x.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
        })); 

//Stripe configuration
var stripeSection = builder.Configuration.GetSection("Stripe");
builder.Services.Configure<StripeOptions>(stripeSection);

var secretKey = stripeSection["SecretKey"];
if (string.IsNullOrWhiteSpace(secretKey))
    throw new InvalidOperationException("Stripe:SecretKey saknas i konfig.");


//PostNord Configuration
builder.Services.Configure<PostNordOptions>(builder.Configuration.GetSection("PostNord"));

builder.Services.AddHttpClient<IPostNordClient, PostNordClient>((sp, http) =>
{
    var opt = sp.GetRequiredService <IOptions<PostNordOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl.TrimEnd('/') + "/");
    http.Timeout = TimeSpan.FromSeconds(10);
});

//Identity Configuration
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
//Speciall IAppDbContext registration
builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<PallshoppenDbContext>());

//Services
builder.Services.AddScoped<PaymentsService>();
builder.Services.AddScoped<IProductService, InfrastructureProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();
builder.Services.AddScoped<IAdminProductService, AdminProductService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddSingleton(_ => new Stripe.StripeClient(secretKey));

//Register HostedService(Background seeder)
builder.Services.AddHostedService<DatabaseInitializerHostedService>();
builder.Services.AddHostedService<PendingCleanupService>();
builder.Services.AddHostedService<StockReservationDeleteService>();

//Token Service
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<ITokenRefreshStore, TokenRefreshStore>();
builder.Services.AddScoped<ITokenService, InfrastructureTokenService>();

//In-Memory cache for refresh tokens
builder.Services.AddMemoryCache();

//Assemblers
builder.Services.AddScoped<OrderAssembler>();
builder.Services.AddScoped<ProductAssembler>();

//Authentication & Authorization
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

    o.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = key,
        ClockSkew = TimeSpan.FromSeconds(30)
    };

    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            var access = ctx.HttpContext.Request.Cookies["access_token"];
            if (!string.IsNullOrEmpty(access)) ctx.Token = access;
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = ctx =>
        {
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


app.Logger.LogInformation("Stripe configured? Secret present: {HasKey}", !string.IsNullOrWhiteSpace(secretKey));

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

app.UseStaticFiles();

app.UseRouting();
app.UseCors("AllowFrontend");


app.UseAuthentication();
app.UseAuthorization();
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/auth/login"))
    {
        Console.WriteLine($"CT: {ctx.Request.ContentType ?? "<null>"}  Len: {ctx.Request.ContentLength?.ToString() ?? "<null>"}  Method: {ctx.Request.Method}");
    }
    await next();
});
app.MapControllers();


app.Run();
public partial class Program { }
