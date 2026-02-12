namespace IntegrationTests.Contracts.Orders;

public sealed class OrderCreatedDto
{
    public string OrderNumber { get; set; } = "";
    public int OrderId { get; set; }
    }
