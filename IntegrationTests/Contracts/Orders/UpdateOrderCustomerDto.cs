

namespace IntegrationTests.Contracts.Orders;

public sealed class UpdateOrderCustomerDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; } 
    public string? Phone { get; set; }
}
