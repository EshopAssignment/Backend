
namespace IntegrationTests.Contracts.Orders;

public sealed class CreateOrderItemRequestDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }  
}
