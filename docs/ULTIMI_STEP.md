# Ultimi step richiesti

Documento di lavoro da aggiornare man mano. Ogni step ha tre parti:

- **COSA IMPLEMENTARE**: l'obiettivo in una riga;
- **DETTAGLIO**: com'è oggi il codice e cosa manca;
- **SOLUZIONE**: un prompt già pronto da incollare in una sessione di Claude Code.

Quando uno step è finito, segnarlo ✅ nella tabella e aggiungere la data (o il link alla PR).

Legenda: ⬜ da fare · 🔄 in corso · ✅ fatto

---

## Riepilogo

| # | Step | Requisito | Area | Stato |
|---|------|-----------|------|-------|
| 1 | Push al cambio di stato del viaggio | UC 4.2 | Backend | ⬜ |
| 2 | Notifica di nuova richiesta alle associazioni | UC 4.7 | Backend | ⬜ |
| 3 | Notifica di annullamento da parte dell'utente | UC 4.9 | Backend | ⬜ |
| 4 | Notifica del singolo rifiuto (opzionale) | UC 4.4 | Backend | ⬜ |
| 5 | Promemoria del giorno prima | UC 4.3 | Backend | ⬜ |
| 6 | Invio push reale tramite Expo | UC 4.2–4.9 | Backend | ⬜ |
| 7 | Registrazione del dispositivo per le push | UC 4.2–4.9 | Frontend | ⬜ |
| 8 | Lista delle notifiche (caregiver) | UC 4 | Frontend | ⬜ |
| 9 | Dettaglio del trasporto con stato e storico | UC 4.1 | Frontend | ⬜ |
| 10 | Pagina dell'operatore per aggiornare lo stato | UC 4.10 | Frontend | ⬜ |
| 11 | Pagine dell'associazione collegate alle API | UC 4.7 / 4.10 | Frontend | ⬜ |

Già fatti (verificato il 2026-11-04):

- ✅ **UC 4.4**: notifiche di presa in carico (`AcceptTransportRequestService`) e di mancata copertura (`DeclineTransportRequestService`, `DeletePartnerAssociationService`);
- ✅ **UC 4.6**: notifica di disdetta da parte dell'associazione (`CancelAcceptedTransportService`);
- ✅ **UC 4.10 (backend)**: link per l'operatore e avanzamento, sospensione o ripresa dello stato (`AdvanceTripStatusService`, `POST /operator/{token}/status`);
- ✅ Infrastruttura: `NotificationDispatcher`, tabella `Notifications`, `POST/DELETE /devices`, API per lista, contatori e lettura delle notifiche.

---

## 1. Push al cambio di stato del viaggio (UC 4.2) ⬜

### COSA IMPLEMENTARE
Avvisare il caregiver a ogni cambio di stato del viaggio.

### DETTAGLIO
`BackEnd/src/GoCare/Services/Transport/AdvanceTripStatusService.cs` salva una nuova `TripStatusTransition` (InCharge, Arriving, OnSite, Returning, Suspended, Completed e la ripresa dopo Suspended) ma non invia nessuna notifica. Il tipo `ENotificationType.TripStatusChanged` esiste ma non è mai usato. Il destinatario è `transport.RequestedById` (Person).

### SOLUZIONE
```text
Nel backend (BackEnd/src/GoCare), modifica AdvanceTripStatusService
(Services/Transport/AdvanceTripStatusService.cs) perché, dopo il SaveChangesAsync
che registra la nuova TripStatusTransition, invii una notifica al caregiver con
NotificationDispatcher (Services/Transport/NotificationDispatcher.cs).

- Inietta NotificationDispatcher nel costruttore, come fanno
  AcceptTransportRequestService e CancelAcceptedTransportService.
- Destinatario: ENotificationSubject.Person, subjectId = trip.RequestedById.
- Tipo: ENotificationType.TripStatusChanged; relatedEntityId = trip.Id.
- Canale: solo ENotificationChannel.Push (niente email a ogni passaggio).
  Fa eccezione Completed, che va anche via Email.
- Testi in italiano, uno per stato: "Il trasporto è stato preso in carico",
  "Il mezzo è in arrivo", "Arrivati a destinazione / in visita",
  "Viaggio di ritorno iniziato", "Trasporto sospeso", "Trasporto ripreso"
  (per l'azione Resume), "Trasporto concluso".
- Non cambiare la logica delle transizioni né la risposta dell'endpoint.

Aggiungi o aggiorna i test esistenti del servizio (se c'è un progetto di test)
per verificare che a ogni transizione valida corrisponda una riga in
Notifications. Esegui build e test, poi fai commit.
```

---

## 2. Notifica di nuova richiesta alle associazioni (UC 4.7) ⬜

### COSA IMPLEMENTARE
Avvisare le associazioni della zona quando arriva una nuova richiesta di trasporto.

### DETTAGLIO
`CreateTransportService` crea una `TransportRequestCandidate` per ogni associazione accreditata della zona, ma nessuna di loro riceve un avviso. Il tipo `ENotificationType.NewRequest` non è usato. Da decidere: notificare alla creazione oppure solo a pagamento riuscito (`StripeWebhookService`, evento `checkout.session.completed`). Se la richiesta è visibile alle associazioni solo dopo il pagamento, la notifica va nel webhook.

### SOLUZIONE
```text
Nel backend (BackEnd/src/GoCare), implementa la notifica di nuova richiesta
alle associazioni candidate (UC 4.7).

1. Leggi CreateTransportService e StripeWebhookService e capisci da quando la
   richiesta è visibile alle associazioni (dopo la creazione o solo dopo il
   pagamento, vedi ListPendingRequestsService). Notifica in quel momento:
   - se è visibile subito: alla fine di CreateTransportService, dopo il SaveChangesAsync;
   - se serve il pagamento: in StripeWebhookService, dopo MarkSucceeded e il
     SaveChangesAsync, solo nel ramo in cui il pagamento passa da Pending a
     Succeeded (così resta idempotente).
2. Per ogni TransportRequestCandidate della richiesta chiama
   NotificationDispatcher.DispatchAsync con:
   - ENotificationSubject.Association, subjectId = candidate.AssociationId;
   - ENotificationType.NewRequest; relatedEntityId = transport.Id;
   - canali Push | Email;
   - titolo "Nuova richiesta di trasporto"; corpo con data e ora di partenza
     (DepartureDateHour, ora italiana) e comune di partenza.
3. Non notificare le associazioni che hanno già rifiutato la richiesta.

Esegui build e test, poi fai commit.
```

---

## 3. Notifica di annullamento da parte dell'utente (UC 4.9) ⬜

### COSA IMPLEMENTARE
Avvisare l'associazione quando il caregiver annulla un trasporto.

### DETTAGLIO
`BackEnd/src/GoCare/Services/Transport/DeleteTransportService.cs` annulla la richiesta (`transport.Cancel`) e rimborsa il pagamento, ma non avvisa nessuno. Il tipo `ENotificationType.TripCancelledByUser` non è usato. I destinatari sono:

- se la richiesta era **Confirmed**: l'associazione assegnata (`AssignedAssociationId`);
- se era **Pending**: le associazioni candidate che non l'hanno rifiutata.

Inoltre vanno revocati i link dell'operatore ancora attivi, come fa già `CancelAcceptedTransportService`.

### SOLUZIONE
```text
Nel backend (BackEnd/src/GoCare), modifica DeleteTransportService
(Services/Transport/DeleteTransportService.cs):

1. Prima di transport.Cancel(...) salva lo stato precedente
   (Pending o Confirmed) e l'AssignedAssociationId.
2. Revoca i TripOperatorLinks ancora attivi della richiesta (RevokedAt == null),
   come in CancelAcceptedTransportService.
3. Dopo il SaveChangesAsync, invia con NotificationDispatcher:
   - se era Confirmed: una notifica all'associazione assegnata;
   - se era Pending: una notifica a ogni TransportRequestCandidate che non ha
     una TransportRequestRejection.
   Parametri: ENotificationSubject.Association, ENotificationType.TripCancelledByUser,
   canali Push | Email, relatedEntityId = transport.Id, titolo
   "Trasporto annullato dall'utente", corpo con data e ora del viaggio.
4. Inietta NotificationDispatcher nel costruttore.

Esegui build e test, poi fai commit.
```

---

## 4. Notifica del singolo rifiuto (UC 4.4, opzionale) ⬜

### COSA IMPLEMENTARE
Decidere se il caregiver va avvisato anche quando una singola associazione rifiuta.

### DETTAGLIO
Oggi `DeclineTransportRequestService` notifica solo quando **nessuna** associazione resta disponibile (`RequestNotCovered`). Il rifiuto di una singola associazione non viene comunicato. Può bastare così: per il caregiver conta solo l'esito finale. Il requisito 4.4 dice "presa in carico, rifiuto o mancata copertura". Se "rifiuto" va inteso come evento separato, serve un nuovo tipo di notifica.

### SOLUZIONE
```text
Prima di scrivere codice, chiedimi conferma su una scelta: per il requisito
UC 4.4 il "rifiuto" va notificato al caregiver a ogni singola associazione che
rifiuta, oppure basta la notifica finale di mancata copertura già presente?

Se va notificato ogni rifiuto:
- aggiungi ENotificationType.RequestDeclined in fondo all'enum
  (Models/Enums/ENotificationType.cs), così i valori esistenti non cambiano.
  Controlla se l'enum è salvato come intero o come stringa e, se serve, crea
  la migration EF;
- in DeclineTransportRequestService, quando la richiesta NON diventa
  NotCovered, invia al caregiver (transport.RequestedById) una notifica solo
  Push: "Un'associazione non può effettuare il trasporto, la richiesta resta
  visibile alle altre associazioni della zona".

Esegui build e test, poi fai commit.
```

---

## 5. Promemoria del giorno prima (UC 4.3) ⬜

### COSA IMPLEMENTARE
Inviare al caregiver, il giorno prima, un promemoria del viaggio.

### DETTAGLIO
Il tipo `ENotificationType.TripReminder` esiste, ma nel progetto non c'è nessun servizio in background (`BackgroundService` o `IHostedService`). Bisogna aggiungerne uno che giri periodicamente, trovi i trasporti `Confirmed` con partenza il giorno dopo e non li notifichi due volte. Per evitare i doppioni basta controllare se in `Notifications` esiste già un `TripReminder` con lo stesso `RelatedEntityId`.

### SOLUZIONE
```text
Nel backend (BackEnd/src/GoCare), crea un servizio in background per il
promemoria del giorno prima (UC 4.3).

- Nuovo file Services/Notifications/TripReminderBackgroundService.cs che
  estende BackgroundService. Registralo in Program.cs con AddHostedService.
- Ogni 15 minuti (usa PeriodicTimer e rendi l'intervallo configurabile in
  appsettings) apri uno scope con IServiceScopeFactory e cerca i
  TransportRequest con RequestStatus == Confirmed e DepartureDateHour nel
  giorno di calendario successivo, ora Europe/Rome.
- Per ognuno, se in Notifications non esiste già una notifica con
  Type == TripReminder e RelatedEntityId == transport.Id, chiama
  NotificationDispatcher.DispatchAsync verso il caregiver
  (ENotificationSubject.Person, transport.RequestedById), canali Push | Email,
  titolo "Promemoria: trasporto domani", corpo con ora di partenza e indirizzo
  di partenza.
- Un errore su un trasporto va loggato e non deve fermare né il ciclo né il
  servizio.
- Tieni la logica di selezione in un metodo separato e testabile, e scrivi un
  test che controlli la finestra "domani" e l'assenza di doppioni.

Esegui build e test, poi fai commit.
```

---

## 6. Invio push reale tramite Expo (UC 4.2–4.9) ⬜

### COSA IMPLEMENTARE
Sostituire lo stub che scrive le push nel log con un invio vero.

### DETTAGLIO
`Infrastructure/ConsolePushSender.cs` implementa `IPushSender` ma si limita a scrivere nel log. È registrato in `Program.cs` con `AddScoped<IPushSender, ConsolePushSender>()`. Il frontend è un'app Expo, quindi la scelta più semplice è l'Expo Push API (`https://exp.host/--/api/v2/push/send`), che accetta i token `ExponentPushToken[...]`. Se Expo risponde `DeviceNotRegistered`, il `DeviceToken` va disattivato.

### SOLUZIONE
```text
Nel backend (BackEnd/src/GoCare), crea Infrastructure/ExpoPushSender.cs, una
nuova implementazione di IPushSender (Abstractions/IPushSender.cs) che usa la
Expo Push API.

- Usa un HttpClient tipizzato (AddHttpClient<IPushSender, ExpoPushSender>) con
  POST https://exp.host/--/api/v2/push/send e body JSON
  { to, title, body, sound: "default" }.
- Se in configurazione c'è un access token Expo (Push:ExpoAccessToken), mandalo
  come header Authorization: Bearer. Non scrivere mai il token nel codice.
- Leggi la risposta: se il ticket ha status "error" con details.error ==
  "DeviceNotRegistered", disattiva il DeviceToken corrispondente (guarda il
  modello DeviceToken in Models/Domain/DeviceToken.cs e come lo disattiva
  UnregisterDeviceService).
- Un errore di rete o una risposta diversa da 2xx va loggato e non deve far
  fallire la richiesta HTTP che ha prodotto la notifica.
- In Program.cs scegli l'implementazione da configurazione (Push:Provider =
  "Console" | "Expo"), tenendo Console come default per lo sviluppo, come si fa
  per l'invio email.
- Aggiorna docs/RECAP_BACKEND.md con la nuova configurazione.

Esegui build e test, poi fai commit.
```

---

## 7. Registrazione del dispositivo per le push (frontend) ⬜

### COSA IMPLEMENTARE
Far registrare all'app il token push del telefono, così il backend può inviargli le notifiche.

### DETTAGLIO
`expo-notifications` non è in `frontend/package.json` e l'app non chiama mai `POST /devices`. Il backend accetta `POST /devices` e `DELETE /devices/{id}` per Person e Association: il corpo è definito in `RegisterDeviceRequest.cs` e le regole in `RegisterDeviceValidator.cs`. Al logout il dispositivo va cancellato (`frontend/app/logout.jsx`).

### SOLUZIONE
```text
Nel frontend Expo (frontend/), aggiungi la registrazione per le push.

1. Installa expo-notifications ed expo-device con `npx expo install`.
   Configura il plugin in app.json se serve.
2. Crea config/push.js con:
   - registerForPush(): chiede il permesso, ottiene l'Expo push token
     (Notifications.getExpoPushTokenAsync con il projectId di app.json),
     chiama POST /devices tramite config/httpClient.js con il corpo previsto da
     BackEnd/src/GoCare/Dtos/Devices/Requests/RegisterDeviceRequest.cs
     (leggi anche RegisterDeviceValidator.cs per piattaforma e campi
     obbligatori) e salva l'id restituito con config/storage.js;
   - unregisterPush(): chiama DELETE /devices/{id} e cancella l'id salvato.
3. Chiama registerForPush() dopo il login, sia per il caregiver
   (app/(caregiver)/_layout.jsx) sia per l'associazione
   (app/association/_layout.jsx), e unregisterPush() in app/logout.jsx.
4. Sul web e sull'emulatore senza permessi il codice non deve andare in
   errore: in quei casi salta la registrazione.
5. Quando l'utente tocca una notifica con relatedEntityId, apri il dettaglio
   del trasporto (/transport/[id]).

Segui le convenzioni di frontend/CLAUDE.md. Fai commit.
```

---

## 8. Lista delle notifiche (caregiver) ⬜

### COSA IMPLEMENTARE
Mostrare al caregiver l'elenco delle notifiche ricevute.

### DETTAGLIO
`frontend/app/(caregiver)/notifications.jsx` è un segnaposto ("disponibile prossimamente"). Il backend espone già la lista paginata (`ListNotificationsController`), i contatori (`/notifications/counters`, già usato per il badge in `_layout.jsx`), "segna come letta" (`MarkNotificationReadController`) e "segna tutte come lette" (`MarkAllNotificationsReadController`).

### SOLUZIONE
```text
Nel frontend (frontend/), implementa la pagina app/(caregiver)/notifications.jsx.

- Leggi i controller in BackEnd/src/GoCare/Controllers/Notifications/ e
  NotificationResponse.cs per route, paginazione (PageQuery/PagedResult) e campi.
- Mostra una FlatList con titolo, testo e data (formato italiano). Le non lette
  vanno evidenziate. Aggiungi pull-to-refresh e caricamento della pagina
  successiva a fine lista.
- Toccando una notifica la segni come letta e, se ha relatedEntityId, apri
  /transport/[id].
- Aggiungi in alto il pulsante "Segna tutte come lette".
- Dopo ogni azione aggiorna il badge del contatore in app/(caregiver)/_layout.jsx.
- Usa i componenti Themed* e StatusBadge esistenti e gestisci lo stato vuoto.

Segui frontend/CLAUDE.md. Fai commit.
```

---

## 9. Dettaglio del trasporto con stato e storico (UC 4.1) ⬜

### COSA IMPLEMENTARE
Mostrare al caregiver lo stato attuale del trasporto e lo storico dei passaggi, aggiornati quasi in tempo reale.

### DETTAGLIO
`frontend/app/(caregiver)/transport/[id].jsx` è un segnaposto. Il backend ha già:

- il dettaglio del viaggio (`TripDetailController`);
- lo storico degli stati: `GET /transports/{id}/status-timeline` (`TimelineController`).

Gli stati da mostrare sono: non preso in carico, preso in carico, in arrivo, in visita, in ritorno, sospeso, concluso. Per il "tempo reale" basta un polling ogni 20–30 secondi mentre la pagina è aperta e il viaggio non è concluso. Un hub SignalR è possibile più avanti.

### SOLUZIONE
```text
Nel frontend (frontend/), implementa app/(caregiver)/transport/[id].jsx.

- Carica il dettaglio da TripDetailController e lo storico da
  GET /transports/{id}/status-timeline (leggi i controller e i DTO in
  BackEnd/src/GoCare per route e campi).
- In alto, uno StatusBadge con lo stato attuale. Etichette in italiano:
  Pending = "Non preso in carico", InCharge = "Preso in carico",
  Arriving = "In arrivo", OnSite = "In visita", Returning = "In ritorno",
  Suspended = "Sospeso", Completed = "Concluso". Gestisci anche i
  RequestStatus NotCovered = "Non coperto" e Cancelled = "Annullato".
- Sotto, la timeline verticale dei passaggi con ora e nome dell'operatore.
- Polling ogni 20 secondi solo con la schermata a fuoco (useFocusEffect) e
  solo se il viaggio non è Completed, Cancelled o NotCovered. Interrompilo
  quando si esce dalla pagina.
- Dati del viaggio: data e ora, indirizzi, assistito, associazione assegnata.
- Pulsante "Annulla trasporto" se lo stato è Pending o Confirmed: chiama
  l'endpoint di DeleteTransportController dopo una conferma.

Usa i componenti esistenti e segui frontend/CLAUDE.md. Fai commit.
```

---

## 10. Pagina dell'operatore per aggiornare lo stato (UC 4.10) ⬜

### COSA IMPLEMENTARE
Una pagina pubblica, aperta dal link ricevuto, da cui l'operatore aggiorna lo stato del viaggio.

### DETTAGLIO
Il backend è pronto:

- `POST /association/transports/{id}/operator-link` genera il link (solo associazione);
- `GET /operator/{token}/status` restituisce `currentStatus`, `nextStatus`, `canSuspend`, `canResume`, `isCompleted` (`OperatorTripStatusResponse`);
- `POST /operator/{token}/status` esegue l'azione (`ETripOperatorAction`: Advance, Suspend, Resume, più `operatorLabel`).

Nel frontend non esiste la pagina. Va verificato quale URL genera il backend per il link, così la route coincide.

### SOLUZIONE
```text
Nel frontend (frontend/), crea la pagina pubblica dell'operatore per UC 4.10.

1. Leggi GenerateOperatorLinkService in BackEnd/src/GoCare/Services/Transport/
   per sapere quale URL viene generato (base URL e path) e crea una route
   expo-router che corrisponda, ad es. app/operator/[token].jsx, accessibile
   senza login (controlla le redirect in app/_layout.jsx e app/index.jsx).
2. La pagina chiama GET /operator/{token}/status e mostra lo stato attuale in
   grande, poi:
   - pulsante principale "Avanza a: <nextStatus>" (Advance);
   - "Sospendi" se canSuspend, "Riprendi" se canResume;
   - un campo "Nome operatore" (operatorLabel), da ricordare in storage locale.
3. Ogni azione chiama POST /operator/{token}/status (corpo come
   AdvanceTripStatusRequest.cs e AdvanceTripStatusValidator.cs) e aggiorna la
   UI con la risposta.
4. Gestisci gli errori: 404 "Link non valido", 403 "Link scaduto o revocato",
   409 "Azione non valida".
5. Layout pensato per il telefono: pulsanti grandi, utilizzabili con una mano.

Nella pagina dei trasporti accettati dell'associazione aggiungi il pulsante
"Genera link operatore", che chiama POST
/association/transports/{id}/operator-link e permette di copiare o condividere
il link. Fai commit.
```

---

## 11. Pagine dell'associazione collegate alle API ⬜

### COSA IMPLEMENTARE
Sostituire i dati di prova delle pagine dell'associazione con le chiamate reali.

### DETTAGLIO
`frontend/app/association/request.jsx` e `trasport.jsx` usano array scritti nel codice e i pulsanti Accetta/Rifiuta non fanno nulla. Le API ci sono già: `ListPendingRequestsController`, `PendingRequestDetailController`, `AcceptTransportRequestController`, `DeclineTransportRequestController`, `ListAcceptedTransportsController`, `GetAcceptedTransportController`, `CancelAcceptedTransportController`, `AssociationHistoryController`.

### SOLUZIONE
```text
Nel frontend (frontend/), collega le pagine dell'associazione alle API reali.

- app/association/request.jsx: lista delle richieste pendenti
  (ListPendingRequestsController) con pull-to-refresh. Il dettaglio usa
  PendingRequestDetailController. "Accetta" chiama
  AcceptTransportRequestController; "Rifiuta" apre un campo motivo facoltativo
  (max 300 caratteri) e chiama DeclineTransportRequestController. Gestisci il
  409 ("richiesta non più in attesa") ricaricando la lista.
- app/association/trasport.jsx: lista dei trasporti accettati
  (ListAcceptedTransportsController), con stato attuale e i pulsanti
  "Genera link operatore" (vedi step 10) e "Disdici" (motivo obbligatorio,
  CancelAcceptedTransportController).
- Aggiungi, se manca, una sezione storico (AssociationHistoryController).
- Rimuovi gli array di prova REQUESTS e simili.

Leggi controller e DTO in BackEnd/src/GoCare per route e campi. Segui
frontend/CLAUDE.md. Fai commit.
```
