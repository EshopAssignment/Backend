namespace Infrastructure.Messaging.Email;

internal static class CustomRequestTemplates
{
    private const string Fg = "#1f2937";
    private const string Muted = "#6b7280";
    private const string Border = "#d7d2c7";
    private const string Card = "#fbfaf6";

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
  Vårt team kommer att gå igenom uppgifterna och återkomma så snart vi kan.
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
        var fileRow = string.IsNullOrWhiteSpace(fileName)
            ? """
<p style="margin:0;color:#6b7280;font-size:13px;line-height:1.6;">
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
    <strong>Telefon:</strong> {Html(phone)}
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

    private static string Html(string? s)
        => (s ?? string.Empty)
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
}