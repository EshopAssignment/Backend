namespace Infrastructure.Messaging.Email;



//copy-pasted from chatgpt 5.2.
internal static class OrderTemplates
{
    private const string Fg = "#1f2937";
    private const string Muted = "#6b7280";
    private const string Border = "#d7d2c7";
    private const string Primary = "#2f8f5b";
    private const string PrimaryFg = "#ffffff";
    private const string Card = "#fbfaf6";

    public static string OrderConfirmation(
        string orderNumber,
        string customerName,
        string currency,
        decimal total,
        IEnumerable<(string Name, int Qty, decimal Price)> items)
    {
        var rows = string.Join("", items.Select(i => $"""
<tr>
  <td style="padding:10px 0;border-bottom:1px solid {Border};color:{Fg};">{Html(i.Name)}</td>
  <td align="center" style="padding:10px 0;border-bottom:1px solid {Border};color:{Muted};">{i.Qty}</td>
  <td align="right" style="padding:10px 0;border-bottom:1px solid {Border};color:{Fg};white-space:nowrap;">{i.Price:0.00} {Html(currency)}</td>
</tr>
"""));

        var body = $"""
<p style="margin:0 0 12px 0;color:{Fg};font-size:14px;line-height:1.6;">
  Hej {Html(customerName)},
</p>

<p style="margin:0 0 16px 0;color:{Fg};font-size:14px;line-height:1.6;">
  Tack för din beställning! Din order har mottagits och behandlas nu.
</p>

<div style="margin:0 0 18px 0;padding:12px 14px;border:1px solid {Border};border-radius:10px;background:{Card};">
  <div style="font-size:13px;color:{Muted};">Ordernummer</div>
  <div style="font-size:16px;font-weight:700;color:{Fg};margin-top:2px;">{Html(orderNumber)}</div>
</div>

<table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="border-collapse:collapse;">
  <thead>
    <tr>
      <th align="left" style="padding:10px 0;border-bottom:2px solid {Border};color:{Muted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;">Produkt</th>
      <th align="center" style="padding:10px 0;border-bottom:2px solid {Border};color:{Muted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;">Antal</th>
      <th align="right" style="padding:10px 0;border-bottom:2px solid {Border};color:{Muted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;">Pris</th>
    </tr>
  </thead>
  <tbody>
    {rows}
  </tbody>
</table>

<div style="margin-top:16px;padding-top:12px;border-top:1px solid {Border};display:block;">
  <div style="font-size:14px;color:{Muted};">Total</div>
  <div style="font-size:18px;font-weight:800;color:{Fg};margin-top:2px;">{total:0.00} {Html(currency)}</div>
</div>

<p style="margin:18px 0 0 0;color:{Muted};font-size:13px;line-height:1.6;">
  Vi återkommer när din order skickas.
</p>
""";

        return EmailLayout.Wrap(
            title: "Orderbekräftelse",
            preheader: $"Tack! Vi har mottagit din order {orderNumber}.",
            bodyHtml: body
        );
    }

    public static string ShippingNotification(string orderNumber, string trackingUrl)
    {
        var body = $"""
<p style="margin:0 0 12px 0;color:{Fg};font-size:14px;line-height:1.6;">
  Din order <strong>{Html(orderNumber)}</strong> har skickats.
</p>

<p style="margin:0 0 16px 0;color:{Muted};font-size:13px;line-height:1.6;">
  Du kan spåra paketet via knappen nedan.
</p>

<table role="presentation" cellpadding="0" cellspacing="0" style="border-collapse:collapse;margin:18px 0;">
  <tr>
    <td align="center" bgcolor="{Primary}" style="border-radius:10px;">
      <a href="{Attr(trackingUrl)}"
         style="display:inline-block;padding:12px 16px;color:{PrimaryFg};text-decoration:none;font-weight:700;font-size:14px;">
        Spåra paket
      </a>
    </td>
  </tr>
</table>

<p style="margin:0;color:{Muted};font-size:12px;line-height:1.6;">
  Om knappen inte funkar: {Link(trackingUrl)}
</p>
""";

        return EmailLayout.Wrap(
            title: "Din order har skickats",
            preheader: $"Order {orderNumber} är på väg.",
            bodyHtml: body
        );
    }

    private static string Html(string? s)
        => (s ?? string.Empty)
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");

    private static string Attr(string? s)
        => Html(s);

    private static string Link(string url)
        => $"""<a href="{Attr(url)}" style="color:{Primary};text-decoration:underline;">{Html(url)}</a>""";
}