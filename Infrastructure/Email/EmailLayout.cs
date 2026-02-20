namespace Infrastructure.ACS;

internal static class EmailLayout
{
    private const string Bg = "#f7f5ef";              
    private const string Fg = "#1f2937";            
    private const string Card = "#fbfaf6";           
    private const string Border = "#d7d2c7";         
    private const string Muted = "#6b7280";          

    private const string FontStack = "Inter, Arial, Helvetica, sans-serif";
    private const string Radius = "10px"; 

    public static string Wrap(string title, string preheader, string bodyHtml)
    { //copy-pasted from chatgpt 5.2.
        var preheaderHtml = $"""
        <span style="display:none!important;visibility:hidden;mso-hide:all;font-size:1px;line-height:1px;color:{Bg};max-height:0;max-width:0;opacity:0;overflow:hidden;">
        {Escape(preheader)}
        </span>
        """;

                return $"""
        <!DOCTYPE html>
        <html lang="sv">
        <head>
        <meta charset="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <title>{Escape(title)}</title>
        </head>

        <body style="margin:0;padding:0;background-color:{Bg};font-family:{FontStack};color:{Fg};">
        {preheaderHtml}

        <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="border-collapse:collapse;background-color:{Bg};">
          <tr>
            <td align="center" style="padding:32px 12px;">
              <table role="presentation" width="600" cellpadding="0" cellspacing="0" style="border-collapse:collapse;width:600px;max-width:600px;background:{Card};border:1px solid {Border};border-radius:{Radius};overflow:hidden;">
        
                <!-- Header -->
                <tr>
                  <td style="padding:20px 24px;background:{Card};border-bottom:1px solid {Border};">
                    <div style="font-size:14px;letter-spacing:0.5px;color:{Muted};text-transform:uppercase;">
                      Pallshoppen
                    </div>
                    <div style="margin-top:6px;font-size:20px;line-height:1.25;font-weight:700;color:{Fg};">
                      {Escape(title)}
                    </div>
                  </td>
                </tr>

                <!-- Content -->
                <tr>
                  <td style="padding:24px;">
                    {bodyHtml}
                  </td>
                </tr>

                <!-- Footer -->
                <tr>
                  <td style="padding:16px 24px;border-top:1px solid {Border};background:{Card};">
                    <div style="font-size:12px;line-height:1.4;color:{Muted};">
                      Detta är ett automatiskt utskick. Om du inte förväntade dig detta mail kan du ignorera det och fortsätta leva ditt liv.
                    </div>
                  </td>
                </tr>

              </table>
            </td>
          </tr>
        </table>

        </body>
        </html>
        """;
            }

            private static string Escape(string? s)
                => (s ?? string.Empty)
                    .Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&quot;");
}
