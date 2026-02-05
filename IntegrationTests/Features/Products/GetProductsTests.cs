

using System.Web;
using IntegrationTests.Contracts.Common;
using IntegrationTests.Contracts.Products;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;


namespace IntegrationTests.Features.Products;

[Collection("db")]
public sealed class GetProductsTests(CustomWebApplicationFactory factory, DbFixture db) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly DbFixture _db = db;

    private const string Url = "/api/products";

    [Fact]
    public async Task GetAll_Returns_200_paged()
    {
        //
        await _db.ResetAsync();

        //
        var res = await _client.GetAsync(Url);
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        //
        var body = await res.ReadJsonAsync<PagedResult<ProductDto>>();
        body.Items.Should().NotBeNull();
        body.Page.Should().BeGreaterThanOrEqualTo(1);
        body.PageSize.Should().BeGreaterThan(0);
        body.TotalPages.Should().BeGreaterThanOrEqualTo(1);
        body.TotalItems.Should().BeGreaterThanOrEqualTo(body.Items.Count);
    }

    [Fact]
    public async Task GetAll_pageSize_limits_Items()
    {
        await _db.ResetAsync();

        var body = await (await _client.GetAsync($"{Url}?page=1&pageSize=5"))
            .ReadJsonAsync<PagedResult<ProductDto>>();

        body.Items.Count.Should().BeLessThanOrEqualTo(5);
        body.Page.Should().Be(1);
        body.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetAll_page_2_has_different_ids_than_page_1()
    {
        await _db.ResetAsync();

        var p1 = await (await _client.GetAsync($"{Url}?page=1&pageSize=10"))
            .ReadJsonAsync<PagedResult<ProductDto>>();

        if (p1.TotalPages < 2)
            return;

        var p2 = await (await _client.GetAsync($"{Url}?page=2&pageSize=10"))
            .ReadJsonAsync<PagedResult<ProductDto>>();

        p1.Items.Select(x => x.Id).Should().NotIntersectWith(p2.Items.Select(x => x.Id));
    }

    [Fact]
    public async Task GetAll_query_filters_by_name_or_description()
    {
        await _db.ResetAsync();

        var first = await (await _client.GetAsync($"{Url}?page=1&pageSize=1"))
            .ReadJsonAsync<PagedResult<ProductDto>>();

        first.Items.Should().NotBeEmpty();
        var name = first.Items[0].Name;
        name.Should().NotBeNullOrWhiteSpace();

        var token = name.Length >= 4 ? name[..4] : name;
        var q = HttpUtility.UrlEncode(token);

        var filtered = await (await _client.GetAsync($"{Url}?query={q}&page=1&pageSize=50"))
             .ReadJsonAsync<PagedResult<ProductDto>>();

        filtered.TotalItems.Should().BeGreaterThan(0);
        filtered.TotalItems.Should().BeLessThanOrEqualTo(first.TotalItems);

        filtered.Items.Should().OnlyContain(p =>
            p.Name.Contains(token, StringComparison.OrdinalIgnoreCase) ||
            p.Description.Contains(token, StringComparison.OrdinalIgnoreCase) ||
            (p.Sku != null && p.Sku.Contains(token, StringComparison.OrdinalIgnoreCase)) ||
            (p.Slug != null && p.Slug.Contains(token, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task GetAll_sort_name_desc_orders_by_name()
    {
        await _db.ResetAsync();

        var body = await (await _client.GetAsync($"{Url}?sort=name_desc&page=1&pageSize=50"))
            .ReadJsonAsync<PagedResult<ProductDto>>();

        body.Items.Select(p => p.Name).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetAll_sort_price_asc_orders_by_price()
    {
        await _db.ResetAsync();

        var body = await (await _client.GetAsync($"{Url}?sort=price_asc&page=1&pageSize=50"))
            .ReadJsonAsync<PagedResult<ProductDto>>();

        body.Items.Select(p => p.PriceExVat).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetAll_minPrice_maxPrice_filters_PriceExVat()
    {
        await _db.ResetAsync();

        var body = await (await _client.GetAsync($"{Url}?minPrice=10&maxPrice=999999&page=1&pageSize=50"))
            .ReadJsonAsync<PagedResult<ProductDto>>();

        body.Items.Should().OnlyContain(p => p.PriceExVat >= 10m && p.PriceExVat <= 999999m);
    }

    [Fact]
    public async Task GetAll_inStock_true_only_returns_Available()
    {
        await _db.ResetAsync();

        var res = await _client.GetAsync($"{Url}?inStock=true&page=1&pageSize=50");

        if (res.StatusCode == HttpStatusCode.BadRequest)
            return;

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.ReadJsonAsync<PagedResult<ProductDto>>();

        body.Items.Should().OnlyContain(p => p.Available > 0);
    }

    [Fact]
    public async Task GetAll_type_filter_only_return_filter()
    {
        await _db.ResetAsync();

        var first = await (await _client.GetAsync($"{Url}?page=1&pageSize=1"))
            .ReadJsonAsync<PagedResult<ProductDto>>();

        first.Items.Should().NotBeEmpty();
        var t = first.Items[0].PalletType; 
        t.Should().NotBeNullOrWhiteSpace();

        var res = await _client.GetAsync($"{Url}?type={HttpUtility.UrlEncode(t)}&page=1&pageSize=50");
        if (res.StatusCode == HttpStatusCode.BadRequest)
            return;

        var body = await res.ReadJsonAsync<PagedResult<ProductDto>>();
        body.Items.Should().OnlyContain(p => p.PalletType.Equals(t, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetAll_condition_filter_only_return_filter()
    {
        await _db.ResetAsync();

        var first = await (await _client.GetAsync($"{Url}?page=1&pageSize=1"))
            .ReadJsonAsync<PagedResult<ProductDto>>();

        first.Items.Should().NotBeEmpty();
        var c = first.Items[0].Condition;
        c.Should().NotBeNullOrWhiteSpace();

        var res = await _client.GetAsync($"{Url}?condition={HttpUtility.UrlEncode(c)}&page=1&pageSize=50");
        if (res.StatusCode == HttpStatusCode.BadRequest)
            return;

        var body = await res.ReadJsonAsync<PagedResult<ProductDto>>();
        body.Items.Should().OnlyContain(p => p.Condition.Equals(c, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetAll_respone_has_sane_values()
    {
        await _db.ResetAsync();
        
        var body = await (await _client.GetAsync($"{Url}?page=1&pageSize=20"))
            .ReadJsonAsync<PagedResult<ProductDto>>();

        body.Items.Should().NotBeEmpty();

        body.Items.Should().OnlyContain(p =>
            p.Id > 0 &&
            !string.IsNullOrWhiteSpace(p.Name) &&
            p.PriceExVat >= 0m &&
            p.VatRatePercent >= 0 && p.VatRatePercent <= 100 &&
            p.OnHand >= 0 &&
            p.Reserved >= 0 &&
            p.Available >= 0);
    }

    [Fact]
    public async Task GetAll_Available_is_consistent_with_OnHand_and_Reserved()
    {
        await _db.ResetAsync();
        var body = await (await _client.GetAsync($"{Url}?page=1&pageSize=20"))
            .ReadJsonAsync<PagedResult<ProductDto>>();

        body.Items.Should().OnlyContain(p => p.Available == Math.Max(0, p.OnHand - p.Reserved));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetAll_invalid_page_returns_400_or_clamps(int page)
    {
        await _db.ResetAsync();

        var res = await _client.GetAsync($"{Url}?page={page}&pageSize=10");

        if (res.StatusCode == HttpStatusCode.OK)
        {
            var body = await res.ReadJsonAsync<PagedResult<ProductDto>>();
            body.Page.Should().BeGreaterThanOrEqualTo(1);
        }
        else
        {
            res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task GetAll_invalid_pageSize_returns_400_or_clamps(int pageSize)
    {
        await _db.ResetAsync();

        var res = await _client.GetAsync($"{Url}?page=1&pageSize={pageSize}");

        if(res.StatusCode == HttpStatusCode.OK)
        {
            var body = await res.ReadJsonAsync<PagedResult<ProductDto>>();
            body.PageSize.Should().BeGreaterThan(0);
        }
        else
        {
            res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task GetAll_minPrice_greater_than_maxPrice_returns_400_or_empty()
    {
        await _db.ResetAsync();

        var res = await _client.GetAsync($"{Url}?minPrice=100&maxPrice=10&page=1&pageSize=20");

        if(res.StatusCode == HttpStatusCode.OK)
        {
            var body = await res.ReadJsonAsync<PagedResult<ProductDto>>();
            body.Items.Should().BeEmpty();
        }
        else
        {
            res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task GetAll_invalid_type_returns_400()
    {
        await _db.ResetAsync();

        var res = await _client.GetAsync($"{Url}?type=definitely_not_a_real_type&page=1&pageSize=10");
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAll_invalid_condition_returns_400()
    {
        await _db.ResetAsync();

        var res = await _client.GetAsync($"{Url}?condition=trashbag&page=1&pageSize=10");
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAll_when_query_is_gibberish_returns_empty()
    {
        await _db.ResetAsync();
        
        var q = HttpUtility.UrlEncode("asdlkjasdfljalksdfjalksdfjalksdf");
        var body = await (await _client.GetAsync($"{Url}?query={q}&page=1&pageSize=20"))
            .ReadJsonAsync<PagedResult<ProductDto>>();

        body.Items.Should().BeEmpty();
    }
}
