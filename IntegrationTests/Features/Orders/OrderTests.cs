

using IntegrationTests.Contracts.Common;
using IntegrationTests.Contracts.Orders;
using IntegrationTests.Contracts.Products;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;

namespace IntegrationTests.Features.Orders;

[Collection("db")]
public sealed class OrderTests(CustomWebApplicationFactory factory, DbFixture db) :IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly DbFixture _db = db;

    private const string OrderUrl = "/api/order";
    private const string ProductUrl = "/api/products";

    private async Task<int> GetAnyProductIdAsync()
    {
        var products = await (await _client.GetAsync($"{ProductUrl}?page=1&pageSize=1"))
            .ReadJsonAsync<PagedResult<ProductDto>>();


        products.Items.Should().NotBeEmpty("seed ska ge produkter");
        return products.Items[0].Id;
    }

    [Fact]
    public async Task Create_returns_201_and_location_header()
    {
        await _db.ResetAsync();

        var productId = await GetAnyProductIdAsync();

        var req = new CreateOrderRequestDto
        {
            CartId = Guid.NewGuid().ToString("N"),
            Items = new()
            {
                new CreateOrderItemRequestDto{ ProductId = productId, Quantity = 1  }
            },
            Currency = "SEK",
            ReservationTtlMinutes = 60
        };

        var res = await _client.PostAsJsonAsync(OrderUrl, req);

        res.StatusCode.Should().Be(HttpStatusCode.Created);

        res.Headers.Location.Should().NotBeNull();

        var created = await res.ReadJsonAsync<OrderCreatedDto>();
        created.Id.Should().BeGreaterThan(0);
        created.OrderNumber.Should().StartWith("ORD-");
    }

    [Fact]
    public async Task Create_with_empty_items_return_400()
    {
        await _db.ResetAsync();

        var res = await _client.PostJsonAsync(OrderUrl, new CreateOrderRequestDto
        {
            CartId = Guid.NewGuid().ToString("N"),
            Items = new()
        });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_with_quantity_null_returns_400()
    {
        await _db.ResetAsync();

        var productId = await GetAnyProductIdAsync();

        var res = await _client.PostJsonAsync(OrderUrl, new CreateOrderRequestDto
        {
            CartId = new Guid().ToString("N"),
            Items = new()
            {
                new CreateOrderItemRequestDto
                {
                    ProductId = productId,
                    Quantity = 0
                }
            }
        });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_without_cartId_returns_400()
    {
        await _db.ResetAsync();

        var productId = await GetAnyProductIdAsync();

        var res = await _client.PostJsonAsync(OrderUrl, new CreateOrderRequestDto
        {
            CartId = "    ",
            Items = new()
            {
                new CreateOrderItemRequestDto
                {
                    ProductId = productId,
                    Quantity = 1
                }
            }
        });

       res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_missing_returns_404()
    {
        await _db.ResetAsync();

        var res = await _client.GetAsync($"{OrderUrl}/{int.MaxValue}");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_then_GetById_retuns_200()
    {
        await _db.ResetAsync();

        var productId = await GetAnyProductIdAsync();

        var createRes = await _client.PostJsonAsync(OrderUrl, new CreateOrderRequestDto
        {
            CartId = Guid.NewGuid().ToString("N"),
            Items = new()
            {
                new CreateOrderItemRequestDto
                {
                    ProductId = productId,
                    Quantity = 1
                }
            }
        });

        createRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createRes.ReadJsonAsync<OrderCreatedDto>();

        var getRes = await _client.GetAsync($"{OrderUrl}/{created.Id}");
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await getRes.ReadJsonAsync<OrderCreatedDto>();
        fetched.Id.Should().Be(created.Id);
        fetched.OrderNumber.Should().Be(created.OrderNumber);
    }

    [Fact]
    public async Task Create_then_getByNumber_returns_200()
    {
        await _db.ResetAsync();
        var productId = await GetAnyProductIdAsync();
        var createRes = await _client.PostJsonAsync(OrderUrl, new CreateOrderRequestDto
        {
            CartId = Guid.NewGuid().ToString("N"),
            Items = new()
            {
                new CreateOrderItemRequestDto
                {
                    ProductId = productId,
                    Quantity = 1
                }
            }
        });
        createRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createRes.ReadJsonAsync<OrderCreatedDto>();

        var getRes = await _client.GetAsync($"{OrderUrl}/by-number/{Uri.EscapeDataString(created.OrderNumber)}");
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await getRes.ReadJsonAsync<OrderCreatedDto>();
        fetched.OrderNumber.Should().Be(created.OrderNumber);
    }

    [Fact]
    public async Task GetByNumebr_blank_returns_404()
    {
        await _db.ResetAsync();

        var res = await _client.GetAsync($"{OrderUrl}/by-number/   ");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

    }
}
