
using Domain.Entities;

namespace Infrastructure.ACS;

public static class OrderEmailMappingExtensions
{
    public static string ToCustomerName(this Order o)
    {
        var name = $"{o.CustomerFirstName} {o.CustomerLastName}".Trim();
        return string.IsNullOrWhiteSpace(name) ? "Kund" : name;
    }

    public static IEnumerable<(string Name, int Qty, decimal Price)> ToEmailItems(this Order o) =>
            o.OrderItems.OrderBy(i => i.Id).Select(i => (i.ProductName, i.Quantity, i.LineTotalIncVat));
}
