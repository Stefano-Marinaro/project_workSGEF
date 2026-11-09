using System.Net;

namespace GoCare.Infrastructure;

// Modelli HTML delle e-mail inviate da GoCare.
// I client di posta (Gmail, Outlook, app mobili) supportano poco CSS: per questo il layout
// è fatto a tabelle con stili inline, senza fogli di stile esterni né immagini remote.
// Ogni metodo restituisce oggetto e corpo HTML, da passare a IEmailSender.SendAsync.
public static class EmailTemplates
{
    private const string Primary = "#6849a7";
    private const string Page = "#e5e4ec";
    private const string Card = "#ffffff";
    private const string Text = "#201e2b";
    private const string Muted = "#625f72";
    private const string Panel = "#f1effa";
    private const string FontStack = "-apple-system,'Segoe UI',Roboto,Helvetica,Arial,sans-serif";

    // ---------------------------------------------------------------- e-mail con link

    // Conferma dell'indirizzo dopo la registrazione (o su richiesta di un nuovo link)
    public static (string Subject, string Html) VerifyAccount(string verifyUrl, TimeSpan validity) =>
        ("Conferma il tuo account GoCare",
         ActionEmail(
             preheader: "Conferma il tuo indirizzo email per iniziare a usare GoCare.",
             title: "Conferma il tuo indirizzo email",
             paragraphs:
             [
                 "Ciao,",
                 "grazie per esserti registrato a <strong>GoCare</strong>. Per attivare il tuo account " +
                 "conferma il tuo indirizzo email premendo il pulsante qui sotto.",
             ],
             buttonLabel: "Conferma il mio indirizzo email",
             url: verifyUrl,
             validity: validity,
             ignoreNote: "Se non hai creato tu un account GoCare, puoi ignorare questa email: " +
                         "senza conferma l'account non verrà attivato."));

    // Recupero password ("Password dimenticata?")
    public static (string Subject, string Html) ResetPassword(string resetUrl, TimeSpan validity) =>
        ("Reimposta la password di GoCare",
         ActionEmail(
             preheader: "Hai chiesto di reimpostare la password del tuo account GoCare.",
             title: "Reimposta la tua password",
             paragraphs:
             [
                 "Ciao,",
                 "abbiamo ricevuto una richiesta di reimpostare la password del tuo account <strong>GoCare</strong>. " +
                 "Premi il pulsante qui sotto per sceglierne una nuova.",
             ],
             buttonLabel: "Reimposta la password",
             url: resetUrl,
             validity: validity,
             ignoreNote: "Se non hai chiesto tu di reimpostare la password, puoi ignorare questa email: " +
                         "la tua password attuale resta valida."));

    // Conferma del nuovo indirizzo e-mail (dal profilo, o indicato dall'amministratore per un'associazione)
    public static (string Subject, string Html) ConfirmEmailChange(string confirmUrl, TimeSpan validity, bool requestedByAdmin = false) =>
        ("Conferma la nuova email GoCare",
         ActionEmail(
             preheader: "Conferma il nuovo indirizzo email del tuo account GoCare.",
             title: "Conferma il nuovo indirizzo email",
             paragraphs: requestedByAdmin
                 ?
                 [
                     "Buongiorno,",
                     "l'amministratore di <strong>GoCare</strong> ha indicato questo indirizzo come nuova email " +
                     "della vostra associazione. Per renderlo effettivo confermatelo con il pulsante qui sotto.",
                 ]
                 :
                 [
                     "Ciao,",
                     "hai chiesto di usare questo indirizzo come nuova email del tuo account <strong>GoCare</strong>. " +
                     "Per completare il cambio confermalo con il pulsante qui sotto.",
                 ],
             buttonLabel: "Conferma il nuovo indirizzo",
             url: confirmUrl,
             validity: validity,
             ignoreNote: "Se non riconosci questa richiesta, ignora questa email: l'indirizzo dell'account non verrà modificato."));

    // Invito di un'associazione partner registrata dall'amministratore
    public static (string Subject, string Html) AssociationInvite(string associationName, string setPasswordUrl, TimeSpan validity) =>
        ("Benvenuti in GoCare",
         ActionEmail(
             preheader: "La vostra associazione è stata registrata su GoCare: impostate la password per accedere.",
             title: "Benvenuti in GoCare",
             paragraphs:
             [
                 "Buongiorno,",
                 $"<strong>{WebUtility.HtmlEncode(associationName)}</strong> è stata registrata su GoCare come associazione partner. " +
                 "Da GoCare riceverete le richieste di trasporto dei comuni che servite e potrete gestirle in un unico posto.",
                 "Per iniziare impostate la password del vostro account con il pulsante qui sotto.",
             ],
             buttonLabel: "Imposta la password",
             url: setPasswordUrl,
             validity: validity,
             ignoreNote: "Se pensate di aver ricevuto questa email per errore, potete ignorarla."));

    // ---------------------------------------------------------------- notifiche

    // Notifiche via email (presa in carico, disdetta, mancata copertura, ...): titolo e testo
    // sono gli stessi salvati nel centro notifiche e inviati come push.
    public static (string Subject, string Html) Notification(string title, string body) =>
        (title,
         Layout(
             preheader: body,
             title: title,
             bodyHtml:
                 $"<div style=\"margin:0 0 20px;padding:16px 18px;background-color:{Panel};border-left:4px solid {Primary};border-radius:10px;" +
                 $"font-family:{FontStack};font-size:15px;line-height:24px;color:{Text};\">{WebUtility.HtmlEncode(body)}</div>" +
                 Paragraph("Apri l'app GoCare per vedere tutti i dettagli e gli aggiornamenti del trasporto.") +
                 Paragraph("Ricevi questa email perché è un aggiornamento importante su un trasporto che ti riguarda.", small: true)));

    // ---------------------------------------------------------------- struttura comune

    // Testo introduttivo + pulsante + validità + link di riserva + nota finale
    private static string ActionEmail(
        string preheader, string title, string[] paragraphs, string buttonLabel, string url, TimeSpan validity, string ignoreNote)
    {
        var encodedUrl = WebUtility.HtmlEncode(url);
        var body =
            string.Concat(paragraphs.Select(p => Paragraph(p))) +
            Button(buttonLabel, encodedUrl) +
            Paragraph($"Il link è valido per <strong>{FormatValidity(validity)}</strong> e può essere usato una sola volta.", small: true) +
            Paragraph("Se il pulsante non funziona, copia e incolla questo indirizzo nel browser:", small: true) +
            $"<p style=\"margin:0 0 20px;font-family:{FontStack};font-size:13px;line-height:20px;word-break:break-all;\">" +
            $"<a href=\"{encodedUrl}\" style=\"color:{Primary};text-decoration:underline;\">{encodedUrl}</a></p>" +
            Paragraph(ignoreNote, small: true);
        return Layout(preheader, title, body);
    }

    // Sfondo, intestazione con il nome GoCare, scheda bianca, piè di pagina
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

    private static string FormatValidity(TimeSpan validity)
    {
        if (validity.TotalDays >= 2)
        {
            var days = (int)Math.Round(validity.TotalDays);
            return days == 1 ? "1 giorno" : $"{days} giorni";
        }
        if (validity.TotalHours >= 1)
        {
            var hours = (int)Math.Round(validity.TotalHours);
            return hours == 1 ? "1 ora" : $"{hours} ore";
        }
        return $"{(int)Math.Round(validity.TotalMinutes)} minuti";
    }
}
