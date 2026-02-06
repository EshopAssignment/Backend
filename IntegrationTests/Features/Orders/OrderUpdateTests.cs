
using IntegrationTests.Contracts.Common;
using IntegrationTests.Contracts.Orders;
using IntegrationTests.Contracts.Products;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;

namespace IntegrationTests.Features.Orders;

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
}
