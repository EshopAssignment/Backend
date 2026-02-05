

using IntegrationTests.Infrastructure;

namespace IntegrationTests.Features.Products;

[Collection("db")]
public sealed class GetProductsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly DbFixture _db;

    public GetProductsTests(CustomWebApplicationFactory factory, DbFixture db)
    {
        _client = factory.CreateClient();
        _db = db;
    }

}
