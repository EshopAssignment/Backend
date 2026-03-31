namespace Application.Options;

public sealed class FulfillmentOptions
{
    public const string SectionName = "Fulfillment";
    public int OverdueAfterDays { get; set; } = 5;
}
