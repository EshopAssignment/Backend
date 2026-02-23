using Infrastructure.ACS;

namespace Infrastructure.Email;
//chatgpt copy-pasted.
internal static class AuthTemplates
{
    private const string Fg = "#1f2937";
    private const string Muted = "#6b7280";
    private const string Border = "#d7d2c7";
    private const string Primary = "#2f8f5b";
    private const string PrimaryFg = "#ffffff";

    public static string VerifyEmail(string verifyUrl)
    {
        var body = $"""
<p style="margin:0 0 12px 0;color:{Fg};font-size:14px;line-height:1.6;">
  Bekräfta din e-postadress för att aktivera ditt konto.
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

<p style="margin:0;color:{Muted};font-size:12px;line-height:1.6;">
  Om knappen inte funkar: {Link(verifyUrl, Primary)}
</p>
""";

        return EmailLayout.Wrap(
            title: "Bekräfta din e-post",
            preheader: "Klicka för att aktivera kontot.",
            bodyHtml: body
        );
    }

    public static string Welcome(string name)
    {
        var safeName = Html(string.IsNullOrWhiteSpace(name) ? "!" : name);
        var body = $"""
<p style="margin:0 0 12px 0;color:{Fg};font-size:14px;line-height:1.6;">
  Hej {safeName}
</p>
<p style="margin:0;color:{Muted};font-size:13px;line-height:1.6;">
  Välkommen till Pallshoppen. Tyvärr.
</p>
""";

        return EmailLayout.Wrap(
            title: "Välkommen",
            preheader: "Ditt konto är skapat.",
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

    private static string Link(string url, string color)
        => $"""<a href="{Attr(url)}" style="color:{color};text-decoration:underline;">{Html(url)}</a>""";
}
