using Infrastructure.ACS;

namespace Infrastructure.Email;
//chatCpt copy-paste.
internal static class AuthTemplates
{
    private const string Fg = "#1f2937";
    private const string Muted = "#6b7280";
    private const string Border = "#d7d2c7";
    private const string Primary = "#2f8f5b";
    private const string PrimaryFg = "#ffffff";
    private const string Card = "#fbfaf6";

    public static string Welcome(string customerName)
    {
        var safeName = string.IsNullOrWhiteSpace(customerName) ? "!" : Html(customerName);

        var body = $"""
<p style="margin:0 0 12px 0;color:{Fg};font-size:14px;line-height:1.6;">
  Hej {safeName}
</p>

<p style="margin:0 0 16px 0;color:{Fg};font-size:14px;line-height:1.6;">
  Välkommen till Pallshoppen. Ditt konto är skapat.
</p>

<div style="margin:0;padding:12px 14px;border:1px solid {Border};border-radius:10px;background:{Card};">
  <div style="font-size:13px;color:{Muted};">
    Tips: Bekräfta din e-postadress för att slippa onödigt strul senare.
  </div>
</div>
""";

        return EmailLayout.Wrap(
            title: "Välkommen",
            preheader: "Ditt konto är skapat.",
            bodyHtml: body
        );
    }

    public static string EmailVerification(string verifyUrl)
    {
        var body = $"""
<p style="margin:0 0 12px 0;color:{Fg};font-size:14px;line-height:1.6;">
  Bekräfta din e-postadress för att aktivera kontot.
</p>

<table role="presentation" cellpadding="0" cellspacing="0" style="border-collapse:collapse;margin:18px 0;">
  <tr>
    <td align="center" bgcolor="{Primary}" style="border-radius:10px;">
      <a href="{Attr(verifyUrl)}"
         style="display:inline-block;padding:12px 16px;color:{PrimaryFg};text-decoration:none;font-weight:700;font-size:14px;">
        Bekräfta e-post
      </a>
    </td>
  </tr>
</table>

<p style="margin:10px 0 0 0;color:{Muted};font-size:12px;line-height:1.6;">
  Om knappen inte funkar: begär en ny länk..
</p>
""";

        return EmailLayout.Wrap(
            title: "Bekräfta din e-post",
            preheader: "Klicka för att aktivera kontot.",
            bodyHtml: body
        );
    }

    public static string PasswordReset(string resetUrl)
    {
        var body = $"""
<p style="margin:0 0 12px 0;color:{Fg};font-size:14px;line-height:1.6;">
  Någon (förmodligen du) bad om att återställa lösenordet.
</p>

<p style="margin:0 0 16px 0;color:{Muted};font-size:13px;line-height:1.6;">
  Klicka på knappen nedan för att välja ett nytt lösenord. Länken fungerar bara en begränsad tid.
</p>

<table role="presentation" cellpadding="0" cellspacing="0" style="border-collapse:collapse;margin:18px 0;">
  <tr>
    <td align="center" bgcolor="{Primary}" style="border-radius:10px;">
      <a href="{Attr(resetUrl)}"
         style="display:inline-block;padding:12px 16px;color:{PrimaryFg};text-decoration:none;font-weight:700;font-size:14px;">
        Återställ lösenord
      </a>
    </td>
  </tr>
</table>

<p style="margin:0;color:{Muted};font-size:12px;line-height:1.6;">
  Om du inte begärde detta kan du ignorera mailet.
</p>

<p style="margin:10px 0 0 0;color:{Muted};font-size:12px;line-height:1.6;">
  Om knappen inte funkar: gå till Inloggningssidan och begär en ny återställningslänk.
</p>
""";

        return EmailLayout.Wrap(
            title: "Återställ lösenord",
            preheader: "Länk för återställning av lösenord.",
            bodyHtml: body
        );
    }

    private static string Html(string? s)
        => (s ?? string.Empty)
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");

    private static string Attr(string? s) => Html(s);

    private static string Link(string url)
        => $"""<a href="{Attr(url)}" style="color:{Primary};text-decoration:underline;">{Html(url)}</a>""";
}