namespace Infrastructure.Messaging.Email;

internal static class CustomRequestTemplates
{
    private const string Fg = "#1f2937";
    private const string Muted = "#6b7280";
    private const string Border = "#d7d2c7";
    private const string Card = "#fbfaf6";
    private const string Accent = "#2f8f5b";
    private const string AccentFg = "#ffffff";

    public static string CustomerConfirmation(string customerName)
    {
        var body = $"""
<p style="margin:0 0 12px 0;color:{Fg};font-size:14px;line-height:1.6;">
  Hej {Html(customerName)},
</p>

<p style="margin:0 0 12px 0;color:{Fg};font-size:14px;line-height:1.6;">
  Vi har tagit emot din specialförfrågan.
</p>

<p style="margin:0 0 12px 0;color:{Fg};font-size:14px;line-height:1.6;">
  Vårt team går nu igenom uppgifterna och återkommer så snart vi kan.
</p>

<p style="margin:0;color:{Muted};font-size:13px;line-height:1.6;">
  Tack för att du kontaktar Pallshoppen.
</p>
""";

        return EmailLayout.Wrap(
            title: "Vi har tagit emot din förfrågan",
            preheader: "Din specialförfrågan har tagits emot.",
            bodyHtml: body
        );
    }

    public static string InternalNotification(
        string name,
        string email,
        string phone,
        string message,
        string? fileName)
    {
        var phoneValue = string.IsNullOrWhiteSpace(phone) ? "Ej angivet" : Html(phone);

        var fileRow = string.IsNullOrWhiteSpace(fileName)
            ? $"""
<p style="margin:0;color:{Muted};font-size:13px;line-height:1.6;">
  Ingen fil bifogades i formuläret.
</p>
"""
            : $"""
<p style="margin:0;color:{Fg};font-size:13px;line-height:1.6;">
  <strong>Uppladdad fil:</strong> {Html(fileName)}
</p>
""";

        var body = $"""
<p style="margin:0 0 16px 0;color:{Fg};font-size:14px;line-height:1.6;">
  En ny specialförfrågan har skickats in via formuläret på webbplatsen.
</p>

<div style="margin:0 0 18px 0;padding:14px;border:1px solid {Border};border-radius:10px;background:{Card};">
  <p style="margin:0 0 8px 0;color:{Fg};font-size:13px;line-height:1.6;">
    <strong>Namn:</strong> {Html(name)}
  </p>
  <p style="margin:0 0 8px 0;color:{Fg};font-size:13px;line-height:1.6;">
    <strong>E-post:</strong> {Html(email)}
  </p>
  <p style="margin:0 0 8px 0;color:{Fg};font-size:13px;line-height:1.6;">
    <strong>Telefon:</strong> {phoneValue}
  </p>
  {fileRow}
</div>

<div style="margin-top:16px;">
  <div style="margin-bottom:8px;color:{Muted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;">
    Meddelande
  </div>
  <div style="padding:14px;border:1px solid {Border};border-radius:10px;background:{Card};color:{Fg};font-size:14px;line-height:1.7;white-space:pre-line;">
    {Html(message)}
  </div>
</div>
""";

        return EmailLayout.Wrap(
            title: "Ny specialförfrågan",
            preheader: $"Ny förfrågan från {name}.",
            bodyHtml: body
        );
    }

    public static string CustomerQuote(
        string customerName,
        string quoteTitle,
        string currency,
        decimal totalIncVat,
        DateTime? expiresAtUtc,
        string? customerMessage,
        IEnumerable<(string Description, int Qty, decimal UnitPrice, decimal LineTotal)> items)
    {
        var safeCustomerName = string.IsNullOrWhiteSpace(customerName) ? "kund" : Html(customerName);
        var safeQuoteTitle = string.IsNullOrWhiteSpace(quoteTitle) ? "Specialoffert" : Html(quoteTitle);
        var safeCurrency = string.IsNullOrWhiteSpace(currency) ? "SEK" : Html(currency.Trim().ToUpperInvariant());

        var expiresBlock = expiresAtUtc.HasValue
            ? $"""
<p style="margin:0 0 12px 0;color:{Fg};font-size:14px;line-height:1.6;">
  Offerten gäller till och med <strong>{expiresAtUtc.Value:yyyy-MM-dd}</strong>.
</p>
"""
            : string.Empty;

        var messageBlock = string.IsNullOrWhiteSpace(customerMessage)
            ? string.Empty
            : $"""
<div style="margin:0 0 18px 0;">
  <div style="margin-bottom:8px;color:{Muted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;">
    Meddelande från oss
  </div>
  <div style="padding:14px;border:1px solid {Border};border-radius:10px;background:{Card};color:{Fg};font-size:14px;line-height:1.7;white-space:pre-line;">
    {Html(customerMessage)}
  </div>
</div>
""";

        var rows = string.Join("", items.Select(i => $"""
<tr>
  <td style="padding:10px 12px;border-bottom:1px solid {Border};color:{Fg};font-size:13px;vertical-align:top;">
    {Html(i.Description)}
  </td>
  <td style="padding:10px 12px;border-bottom:1px solid {Border};color:{Fg};font-size:13px;text-align:center;white-space:nowrap;">
    {i.Qty}
  </td>
  <td style="padding:10px 12px;border-bottom:1px solid {Border};color:{Fg};font-size:13px;text-align:right;white-space:nowrap;">
    {Money(i.UnitPrice)} {safeCurrency}
  </td>
  <td style="padding:10px 12px;border-bottom:1px solid {Border};color:{Fg};font-size:13px;text-align:right;white-space:nowrap;font-weight:600;">
    {Money(i.LineTotal)} {safeCurrency}
  </td>
</tr>
"""));

        var body = $"""
<p style="margin:0 0 12px 0;color:{Fg};font-size:14px;line-height:1.6;">
  Hej {safeCustomerName},
</p>

<p style="margin:0 0 12px 0;color:{Fg};font-size:14px;line-height:1.6;">
  Vi har tagit fram en offert för din förfrågan.
</p>

<div style="margin:0 0 18px 0;padding:16px;border:1px solid {Border};border-radius:12px;background:{Card};">
  <p style="margin:0 0 8px 0;color:{Muted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;">
    Offert
  </p>
  <p style="margin:0;color:{Fg};font-size:18px;line-height:1.4;font-weight:700;">
    {safeQuoteTitle}
  </p>
</div>

{messageBlock}

<div style="margin:0 0 18px 0;overflow:hidden;border:1px solid {Border};border-radius:12px;background:{Card};">
  <table role="presentation" style="width:100%;border-collapse:collapse;">
    <thead>
      <tr style="background:#f3f0e8;">
        <th style="padding:10px 12px;text-align:left;color:{Muted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;border-bottom:1px solid {Border};">
          Beskrivning
        </th>
        <th style="padding:10px 12px;text-align:center;color:{Muted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;border-bottom:1px solid {Border};white-space:nowrap;">
          Antal
        </th>
        <th style="padding:10px 12px;text-align:right;color:{Muted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;border-bottom:1px solid {Border};white-space:nowrap;">
          Pris/st
        </th>
        <th style="padding:10px 12px;text-align:right;color:{Muted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;border-bottom:1px solid {Border};white-space:nowrap;">
          Radtotal
        </th>
      </tr>
    </thead>
    <tbody>
      {rows}
    </tbody>
  </table>
</div>

<div style="margin:0 0 18px 0;padding:16px;border-radius:12px;background:linear-gradient(135deg, rgba(47,143,91,0.10), rgba(47,143,91,0.03));border:1px solid rgba(47,143,91,0.18);">
  <p style="margin:0 0 6px 0;color:{Muted};font-size:12px;text-transform:uppercase;letter-spacing:0.5px;">
    Totalpris
  </p>
  <p style="margin:0;color:{Fg};font-size:24px;line-height:1.2;font-weight:800;">
    {Money(totalIncVat)} {safeCurrency}
  </p>
</div>

{expiresBlock}

<p style="margin:0 0 12px 0;color:{Fg};font-size:14px;line-height:1.6;">
  Om du vill gå vidare svarar du enklast på detta mail, så hjälper vi dig vidare med nästa steg.
</p>

<p style="margin:0;color:{Muted};font-size:13px;line-height:1.6;">
  Tack för att du väljer Pallshoppen.
</p>
""";

        return EmailLayout.Wrap(
            title: "Din offert från Pallshoppen",
            preheader: $"Vi har tagit fram en offert: {quoteTitle}",
            bodyHtml: body
        );
    }

    private static string Money(decimal value)
        => value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

    private static string Html(string? s)
        => (s ?? string.Empty)
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
}