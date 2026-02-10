
using IntegrationTests.Contracts.Common;
using IntegrationTests.Contracts.Orders;
using IntegrationTests.Contracts.Products;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;

namespace IntegrationTests.Features.Orders;

[Collection("db")]
public sealed class OrderUpdateTests(CustomWebApplicationFactory factory, DbFixture db) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly DbFixture _db = db;

    private const string OrdersUrl = "/api/order";
    private const string ProductsUrl = "/api/products";

    private async Task<OrderCreatedDto> CreateOrderAsync()
    {
        var products = await (await _client.GetAsync($"{ProductsUrl}?page=1&pageSize=1"))
            .ReadJsonAsync<PagedResult<ProductDto>>();

        var productId = products.Items[0].Id;

        var res = await _client.PostJsonAsync(OrdersUrl, new CreateOrderRequestDto
        {
            CartId = Guid.NewGuid().ToString("N"),
            Items = new()
            {
                new CreateOrderItemRequestDto{ProductId = productId, Quantity = 1 }
            }
        });

        res.StatusCode.Should().Be(HttpStatusCode.Created);
        return await res.ReadJsonAsync<OrderCreatedDto>();
    }


    [Fact]
    public async Task UpdateCutomer_valid_returns_204()
    {
        await _db.ResetAsync();
        
        var order = await CreateOrderAsync();

        var res = await _client.PatchJsonAsync(
            $"{OrdersUrl}/by-number/{Uri.EscapeDataString(order.OrderNumber)}/customer",
            new UpdateOrderCustomerDto
            {
                FirstName = "Test",
                LastName = "Testsson",
                Email = "test@test.com",
                Phone = "0700000000"
            });
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Theory]
    [InlineData("", "User", "test@test.com")]
    [InlineData("Test",  "", "test@test.com")]
    [InlineData("test", "user", "")]
    public async Task UpdateCustomer_missing_required_returns_400(string first, string last, string email)
    {
        await _db.ResetAsync();

        var order = await CreateOrderAsync();

        var res = await _client.PatchJsonAsync(
            $"{OrdersUrl}/by-number/{Uri.EscapeDataString(order.OrderNumber)}/customer",
            new UpdateOrderCustomerDto
            {
                FirstName = first,
                LastName = last,
                Email = email
            });
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCuastomer_unknown_order_returns_404()
    {
        await _db.ResetAsync();

        var res = await _client.PatchJsonAsync(
            $"{OrdersUrl}/by-number/ORD-does-not-exist/customer",
            new UpdateOrderCustomerDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@test.com"
            });

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateShippingAddress_valid_returns_204()
    {
        await _db.ResetAsync();

        var order = await CreateOrderAsync();

        var res = await _client.PatchJsonAsync(
            $"{OrdersUrl}/by-number/{Uri.EscapeDataString(order.OrderNumber)}/shipping-address",
            new UpdateOrderShippingAddressDto
            {
                Street = "Pallgatan 1",
                City = "Pallkenberg",
                PostalCode = "31135",
                Country = "SE"
            });

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Theory]
    [InlineData("", "Halmstad", "30250")]
    [InlineData("Pallgatan 1", "", "30250")]
    [InlineData("Pallgatan 1", "Halmstad", "")]
    public async Task UpdateShippingAddres_missing_required_returns_400(string street, string city, string postalCode)
    {
        await _db.ResetAsync();

        var order = await CreateOrderAsync();

        var res = await _client.PatchJsonAsync(
            $"{OrdersUrl}/by-number/{Uri.EscapeDataString(order.OrderNumber)}/shipping-address",
            new UpdateOrderShippingAddressDto
            {
                Street = street,
                City = city,
                PostalCode = postalCode
            });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task UpdateShippingAddress_unknown_order_returns_404()
    {
        await _db.ResetAsync();

        var res = await _client.PatchJsonAsync(
            $"{OrdersUrl}/by-number/ORD-does-not-exist/shipping-address",
            new UpdateOrderShippingAddressDto
            {
                Street = "pallgatan",
                City = "pallkenberg",
                PostalCode = "31135"
            });

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

    }
}
