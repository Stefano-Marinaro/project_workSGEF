using System.Net;

namespace GoCare.Infrastructure;

// Modelli HTML delle e-mail inviate da GoCare.
// I client di posta (Gmail, Outlook, app mobili) supportano poco CSS: per questo il layout
// è fatto a tabelle con stili inline, senza fogli di stile esterni né immagini remote.
public static class EmailTemplates
{
    private const string Primary = "#6849a7";
    private const string Page = "#e5e4ec";
    private const string Card = "#ffffff";
    private const string Text = "#201e2b";
    private const string Muted = "#625f72";
    private const string FontStack = "-apple-system,'Segoe UI',Roboto,Helvetica,Arial,sans-serif";

    // E-mail di conferma dell'indirizzo dopo la registrazione (o su richiesta di un nuovo link).
    public static (string Subject, string Html) VerifyAccount(string verifyUrl, TimeSpan validity)
    {
        var url = WebUtility.HtmlEncode(verifyUrl);
        var body =
            Paragraph("Ciao,") +
            Paragraph("grazie per esserti registrato a <strong>GoCare</strong>. Per attivare il tuo account " +
                      "conferma il tuo indirizzo email premendo il pulsante qui sotto.") +
            Button("Conferma il mio indirizzo email", url) +
            Paragraph($"Il link è valido per <strong>{FormatValidity(validity)}</strong>.", small: true) +
            Paragraph("Se il pulsante non funziona, copia e incolla questo indirizzo nel browser:", small: true) +
            $"<p style=\"margin:0 0 20px;font-family:{FontStack};font-size:13px;line-height:20px;word-break:break-all;\">" +
            $"<a href=\"{url}\" style=\"color:{Primary};text-decoration:underline;\">{url}</a></p>" +
            Paragraph("Se non hai creato tu un account GoCare, puoi ignorare questa email: " +
                      "senza conferma l'account non verrà attivato.", small: true);

        return ("Conferma il tuo account GoCare",
            Layout("Conferma il tuo indirizzo email per iniziare a usare GoCare.", "Conferma il tuo indirizzo email", body));
    }

    // Struttura comune: sfondo, intestazione con il nome GoCare, scheda bianca, piè di pagina
    private static string Layout(string preheader, string title, string bodyHtml) =>
        $"""
        <!DOCTYPE html>
        <html lang="it">
        <head>
        <meta charset="utf-8">
        <meta name="viewport" content="width=device-width,initial-scale=1">
        <meta name="color-scheme" content="light">
        <title>{WebUtility.HtmlEncode(title)}</title>
        </head>
        <body style="margin:0;padding:0;background-color:{Page};">
        <div style="display:none;max-height:0;overflow:hidden;opacity:0;">{WebUtility.HtmlEncode(preheader)}</div>
        <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:{Page};">
          <tr>
            <td align="center" style="padding:32px 16px;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width:520px;">
                <tr>
                  <td align="center" style="padding:0 0 24px;font-family:{FontStack};font-size:28px;font-weight:700;letter-spacing:-1px;color:{Primary};">
                    GoCare
                  </td>
                </tr>
                <tr>
                  <td style="background-color:{Card};border-radius:16px;padding:36px 32px;">
                    <h1 style="margin:0 0 20px;font-family:{FontStack};font-size:22px;line-height:30px;font-weight:600;color:{Text};">{WebUtility.HtmlEncode(title)}</h1>
                    {bodyHtml}
                  </td>
                </tr>
                <tr>
                  <td align="center" style="padding:24px 16px 0;font-family:{FontStack};font-size:12px;line-height:18px;color:{Muted};">
                    GoCare · I tuoi trasporti, in un unico posto.<br>
                    Questa è un'email automatica: ti chiediamo di non rispondere a questo messaggio.
                  </td>
                </tr>
              </table>
            </td>
          </tr>
        </table>
        </body>
        </html>
        """;

    private static string Paragraph(string html, bool small = false) =>
        $"<p style=\"margin:0 0 16px;font-family:{FontStack};font-size:{(small ? 13 : 15)}px;line-height:{(small ? 20 : 24)}px;color:{(small ? Muted : Text)};\">{html}</p>";

    // Pulsante "a prova di client": tabella con sfondo, funziona anche in Outlook
    private static string Button(string label, string encodedUrl) =>
        $"""
        <table role="presentation" cellpadding="0" cellspacing="0" border="0" style="margin:8px 0 24px;">
          <tr>
            <td align="center" bgcolor="{Primary}" style="border-radius:12px;">
              <a href="{encodedUrl}" target="_blank" style="display:inline-block;padding:14px 28px;font-family:{FontStack};font-size:16px;font-weight:600;color:#ffffff;text-decoration:none;border-radius:12px;">{WebUtility.HtmlEncode(label)}</a>
            </td>
          </tr>
        </table>
        """;

    private static string FormatValidity(TimeSpan validity) =>
        validity.TotalHours >= 1
            ? (validity.TotalHours == 1 ? "1 ora" : $"{(int)validity.TotalHours} ore")
            : $"{(int)validity.TotalMinutes} minuti";
}
