

namespace IntegrationTests.Contracts.Orders;

public sealed class CreateOrderRequestDto
{
    public List<CreateOrderItemRequestDto> Items { get; set; } = new();
    public string CartId { get; set; } = "";
    public string Currency { get; set; } = "SEK";
    public int ReservationTtlMinutes { get; set; } = 60;
}
