using IntegrationTests.Contracts.Common;
using IntegrationTests.Contracts.Products;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;

namespace IntegrationTests.Features.Products;



[Collection("db")]
public sealed class GetProductByIdTests(CustomWebApplicationFactory factory, DbFixture db) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly DbFixture _db = db;

    [Fact]
    public async Task GetById_existing_returns_200()
    {
        await _db.ResetAsync();

        var first = await (await _client.GetAsync("/api/products?page=1&pageSize=1"))
            .ReadJsonAsync<PagedResult<ProductDto>>();

        first.Items.Should().NotBeEmpty();
        var id = first.Items[0].Id;

        var res = await _client.GetAsync($"/api/products/{id}");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var p = await res.ReadJsonAsync<ProductDto>();
        p.Id.Should().Be(id);
        p.IsActive.Should().BeTrue();


    }

    [Fact]
    public async Task GetById_missing_returns404()
    {
        await _db.ResetAsync();

        var rs = await _client.GetAsync($"/api/products/{int.MaxValue}");
        rs.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
