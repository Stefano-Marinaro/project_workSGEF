# Guida Stripe — configurazione locale (PA-07)

Questa guida copre cosa serve, dove si prende e dove va messo per far
funzionare il pagamento del trasporto in locale. Riguarda il branch
`feature/stripe` (e i servizi `CreateCheckoutSessionService`,
`StripeWebhookService`, `IRoutingService`/`OpenRouteServiceClient`).

## 1. Cosa serve

Due account esterni, entrambi gratuiti per lo sviluppo:

1. **Stripe** — un account sandbox. Serve per `SecretKey` e `WebhookSecret`.
2. **OpenRouteService** — solo email, nessuna carta. Serve per `ApiKey`
   (calcolo dei km stradali, usato per il prezzo). *Al momento il servizio
   può risultare non raggiungibile per via delle patch di emergenza PHP di
   fine ottobre 2026 (CVE-2026-109618/109619, "PHP2Shell") — se succede,
   riprovare più tardi, non è un problema del nostro codice.*

Tutte le chiavi vanno in **`src/GoCare/appsettings.Local.json`**, mai in
`appsettings.json` — quel file non è tracciato da git, ogni sviluppatore
ha il proprio.

## 2. Stripe — Secret Key

Dashboard Stripe (sandbox) → **Developers** → **API keys**. Copiare quella
che inizia con `sk_test_...` (non la "Publishable key", quella pubblica
non serve qui).

```json
{
  "Stripe": {
    "SecretKey": "sk_test_...",
    "WebhookSecret": ""
  }
}
```

La `SecretKey` può essere la stessa per tutto il team, è quella
dell'account sandbox condiviso.

## 3. Stripe — Webhook Secret (diverso per ogni sviluppatore)

Il webhook secret **non si prende dalla pagina Webhooks del dashboard**
(quella serve per un server pubblico, non per `localhost`) — si usa la
Stripe CLI, ed è diverso per ciascuno perché ognuno inoltra gli eventi
alla propria macchina.

**Prerequisito**: bisogna essere stati invitati nell'account Stripe di
GoCare (Dashboard → **Settings → Team** → Invite), altrimenti `stripe
login` collega a un account Stripe personale diverso, non al sandbox
condiviso del progetto.

```
# 1. Installare la CLI (Windows)
winget install --id Stripe.StripeCli -e

# IMPORTANTE: chiudere e riaprire il terminale dopo l'installazione,
# il PATH non si aggiorna nella sessione già aperta.

# 2. Login (apre il browser, autorizzare con l'account invitato)
stripe login

# 3. Avviare l'ascolto - lasciarlo acceso in un terminale dedicato
# mentre si sviluppa/testa (la porta deve corrispondere a quella
# del backend in locale, vedi Properties/launchSettings.json)
stripe listen --forward-to localhost:5161/payments/stripe/webhook --events checkout.session.completed
```

All'avvio stampa una riga tipo:

```
Ready! ... Your webhook signing secret is whsec_xxxxxxxxxxxxx
```

Quel valore va in `WebhookSecret`:

```json
{
  "Stripe": {
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  }
}
```

Resta stabile tra un riavvio e l'altro dello stesso comando sulla stessa
macchina, ma è diverso da persona a persona — normale, nessun problema.

**Quando l'app sarà online da qualche parte** (non più solo in locale), il
webhook si configura diversamente: si registra **un** endpoint vero nel
dashboard Stripe con l'URL pubblico del server, e quello ha un solo
secret fisso condiviso — la CLI serve solo per lo sviluppo locale.

## 4. OpenRouteService — API Key

openrouteservice.org → registrazione con email → chiave gratuita.

```json
{
  "OpenRouteService": {
    "ApiKey": "..."
  }
}
```

## 5. `appsettings.Local.json` completo (esempio)

```json
{
  "Frontend": {
    "VerifyEmailUrl": "http://<il-tuo-ip-lan>:8081/verify_email",
    "ResetPasswordUrl": "http://<il-tuo-ip-lan>:8081/reset_password",
    "ChangeEmailUrl": "http://<il-tuo-ip-lan>:8081/conferma-cambio-email",
    "OperatorUrl": "http://<il-tuo-ip-lan>:8081/operatore",
    "CaregiverInviteUrl": "http://<il-tuo-ip-lan>:8081/invito-caregiver"
  },
  "Stripe": {
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  },
  "OpenRouteService": {
    "ApiKey": "..."
  }
}
```

## 6. Provare il flusso end-to-end (da Swagger)

Il frontend reale non manda ancora le coordinate degli indirizzi (vedi
`docs/MODIFICHE_MOCKUP_TRASPORTI.md`), quindi per ora si testa da
`/swagger`:

1. `POST /transports` con indirizzi che includono `Latitude`/`Longitude`
   veri (presi a mano da una mappa).
2. Far accettare il trasporto da un'associazione, perché passi a
   "Confirmed".
3. `POST /transports/{id}/payment-session` → prendere l'URL restituito.
4. Aprire l'URL nel browser, pagare con una carta di test Stripe:
   `4242 4242 4242 4242`, data futura qualsiasi, CVC qualsiasi.
5. Il terminale con `stripe listen` acceso dovrebbe mostrare l'evento
   `checkout.session.completed` inoltrato al backend.
6. `GET /transports/{id}` → dovrebbe mostrare `IsPaid: true` e i contatti
   veri dell'associazione.
