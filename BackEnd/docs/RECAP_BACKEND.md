# Recap backend GoCare — passaggio di consegne

> Stato del codice al commit `ce6ca9e` (merge PR #25, `PUT /association/profile`), 19/10/2026.
> Aggiornato dopo la revisione di `Project_GoCare.md` (UC 11 Amministrazione, pagamento Stripe, comuni coperti, registrazione associazioni solo da admin).
> Aggiornato il 25/10/2026: instradamento delle richieste per **comuni coperti** al posto delle province (§5); fix di `DeleteTransport` risolti (§4).
> Per chi prende in mano il backend (Stefano e Giorgia). Dove questo documento e `PIANO_BACKEND_GoCare.md` non coincidono, vale questo: il piano non è stato aggiornato dopo l'avvio dei controller di dominio (lì risultano ancora "da fare" endpoint che esistono già).

---

## 1. Riassunto delle cose fatte

- **Fondamenta**: un solo progetto `.NET 10` (`BackEnd/src/GoCare`), un solo database PostgreSQL `gocare`, un solo `GoCareDbContext`. Le migrazioni vengono applicate **automaticamente all'avvio** (`db.Database.Migrate()` in `Program.cs`).
- **`docker-compose.yml`** (nella root del repo): Postgres 16 + backend (porta `5000` → `8080`).
- **Auth completa**: registrazione utente/associazione, verifica email (con reinvio), login, refresh con rotazione, logout, password dimenticata/reset, cambio email con conferma.
- **Account admin** creato da migrazione (`SeedAdminAccount`, email `admin@gocare.it`) + endpoint di accreditamento/rifiuto associazioni (⚠️ ora obsoleti: le associazioni le registra l'admin, vedi §3.3).
- **Profili**: GET/PUT profilo caregiver e associazione (+ PATCH di completamento del profilo "scheletro").
- **Assistiti**: CRUD completo + aggiunta/rimozione di un caregiver collegato.
- **Trasporti lato caregiver**: crea, lista (filtrabile per assistito), dettaglio, annulla, timeline di stato.
- **Trasporti lato associazione**: accetta richiesta ("prima accettazione vince" con concorrenza ottimistica su `xmin`), lista trasporti accettati, dettaglio, storico, generazione del link operatore.
- **Notifiche**: lista, segna letta, segna tutte lette, contatori; `NotificationDispatcher` che salva la notifica e la invia via email/push.
- **Dispositivi**: registra/rimuovi push token.
- **Destinazioni salvate**: CRUD su `/me/destinations`.
- **Swagger** con pulsante *Authorize* (Bearer JWT), enum serializzati come stringa nel JSON, CORS aperto su `http://localhost:8081` (Expo web).

---

## 2. Come è fatto il codice

### 2.1 Struttura delle cartelle (`BackEnd/src/GoCare`)

| Cartella | Contenuto |
|---|---|
| `Controllers/<Area>/` | **Un controller per endpoint** (es. `LoginController`, `CreateTransportController`). Sottili: leggono l'id utente dal JWT, chiamano un Service, restituiscono il risultato. |
| `Services/<Area>/` | **Un service per caso d'uso** (es. `LoginService`, `AcceptTransportRequestService`). Classi concrete, niente interfacce `I…Service`, niente repository: usano direttamente `GoCareDbContext`. |
| `Dtos/<Area>/{Requests,Responses,Validators}/` | Contratti HTTP (record) + validator FluentValidation. Le entità di `Models/` non escono mai verso il client. |
| `Models/Auth`, `Models/Domain`, `Models/Enums` | Entità EF Core. Contengono anche la logica di dominio (es. `TransportRequest.Accept()`, `.Cancel()`, controllo coerenza orari nel costruttore). |
| `Data/` | `GoCareDbContext` (configurazione EF inline in `OnModelCreating`, divisa in `#region`) + `Migrations/`. |
| `Errors/` | Eccezioni tipizzate + `GlobalExceptionHandler`. |
| `Validation/` | `ValidationFilter`. |
| `Abstractions/` + `Infrastructure/` | `IEmailSender` / `IPushSender` e relative implementazioni (`ConsoleEmailSender`, `ConsolePushSender`, `SmtpEmailSender`). |
| `Pagination/` | `PageQuery`, `PagedResult<T>` — pronti ma **non ancora usati**. |

### 2.2 Ciclo di vita di una richiesta

```
HTTP → [Authentication JWT] → [Authorization: [Authorize(Roles = ...)]]
     → ValidationFilter (cerca il validator del DTO e, se fallisce, lancia ValidationException)
     → Controller (legge "sub" dal token, chiama il Service)
     → Service (controlli di permesso sulla risorsa, logica, SaveChangesAsync, notifiche)
     → risposta 200/201/204
   eccezioni ──► GlobalExceptionHandler ──► ProblemDetails (RFC 7807)
```

Mappatura errori (`Errors/GlobalExceptionHandler.cs`):

| Eccezione | Status |
|---|---|
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| `ForbiddenException` | 403 |
| `ValidationException` | 422 (con dizionario `campo → messaggi`) |
| qualsiasi altra | 500, senza stack trace |

**Regola pratica per scrivere un nuovo endpoint:**
1. DTO `…Request` in `Dtos/<Area>/Requests` + validator in `Dtos/<Area>/Validators` (viene registrato **in automatico** da `AddValidatorsFromAssemblyContaining<Program>()`).
2. Service in `Services/<Area>/` con parametri primitivi o di dominio (non il DTO HTTP); lancia le eccezioni di `Errors/` invece di restituire codici.
3. **Registrare il Service in `Program.cs`** (`builder.Services.AddScoped<MioService>()`) — questo invece **non** è automatico.
4. Controller in `Controllers/<Area>/` con `[ApiController]`, `[Route(...)]`, `[Tags(...)]` e **sempre** `[Authorize(Roles = ...)]` se l'endpoint non è pubblico.

### 2.3 Autenticazione

- **Access token JWT** (15 minuti) con claim `sub` = `Account.Id`, `role` (`Person` | `Association` | `Admin`), `email_verified`. In `Program.cs` `MapInboundClaims = false`, quindi nei controller si legge `User.FindFirstValue("sub")`.
- **Refresh token** opaco (64 byte random, Base64) salvato in tabella, 30 giorni. `/auth/refresh` lo **ruota** (revoca il vecchio, emette uno nuovo). `TokenIssuer` è condiviso da login e refresh.
- **Password** con `PasswordHasher<Account>` (PBKDF2) in `PasswordService`.
- **Registrazione in due tempi**: `/auth/register/*` crea `Account` (stato `Unverified`) + profilo "scheletro" (`Person`/`Association` con solo id ed email, stesso `Guid` dell'account, FK reale). Il resto del profilo si completa dopo (PATCH/PUT profilo).
- Verifica email, reset password e cambio email usano token monouso con scadenza (`IExpirable`). I link nelle email puntano al frontend (`Frontend:*Url` in `appsettings.json`), che poi chiama la POST del backend con il token nel body.
- Il reset password **revoca tutte le sessioni** dell'account. `forgot-password` risponde sempre uguale (niente enumerazione delle email).

### 2.4 Dominio dei trasporti

- `TransportRequest` ha due "assi" di stato:
  - **stato della richiesta** `ETripRequestStatus`: `Pending → Confirmed → InProgress → Completed`, più i terminali `NotCovered` e `Cancelled`;
  - **stato di esecuzione** `ETripTransitionStatus` (log append-only in `TripStatusTransition`): `Pending → InCharge → Arriving → OnSite → Returning → Completed`, con `Suspended` per gli imprevisti.
- **Creazione** (`CreateTransportService`): controlla che il caregiver sia collegato all'assistito, salva richiesta + accompagnatori, calcola i **candidati** = associazioni `Accredited` la cui lista `CoveredProvinces` contiene la provincia di partenza (`TransportRequestCandidate`). ⚠️ Decisione cambiata: il confronto va fatto sui **comuni coperti** (codice ISTAT del comune di partenza), vedi §5; gli accompagnatori (UC 1.4) sono stati tolti, vedi §3.3.
- **Accettazione** (`AcceptTransportRequestService`): l'associazione deve essere candidata e la richiesta `Pending`; se due associazioni accettano insieme, la seconda riceve `DbUpdateConcurrencyException` → 409 (colonna di sistema Postgres `xmin` come token di concorrenza). Poi notifica il caregiver (push + email).
- **Link operatore** (`GenerateOperatorLinkService`): l'associazione assegnataria genera un token che scade a `DepartureDateHour + 24h`; l'URL restituito è `Frontend:OperatorUrl/{token}`. L'operatore non ha account: userà il link per aggiornare lo stato (endpoint **ancora da scrivere**, vedi §3).
- **Notifiche** (`NotificationDispatcher`): salva sempre la `Notification`, poi invia sui canali richiesti; se manca l'email o non ci sono device registrati quel canale viene saltato.

### 2.5 Database

- Nomi di tabelle/colonne in `snake_case` (`EFCore.NamingConventions`); enum salvati come stringa (eccetto `Notification.Channels`, che è `[Flags]` e quindi intero).
- `Notification.SubjectId` e `DeviceToken.SubjectId` sono id **polimorfici senza FK** (possono puntare a `Person` o `Association`).
- Soft delete con `DeletedAt` ma **nessun query filter globale**: ogni query deve filtrare a mano `DeletedAt == null`.
- ⚠️ Bug di EF Core 10 già incontrato: proprietà enum/`DateTimeOffset` senza setter vengono ignorate in silenzio → vanno configurate con `entity.Property(x => x.Campo)` esplicito. Se una colonna "sparisce", guardare lì.

### 2.6 Avvio in locale

```bash
docker compose up -d db                     # solo Postgres
cd BackEnd && dotnet run --project src/GoCare
# Swagger: http(s)://localhost:<porta>/swagger  (solo ambiente Development)
```
Oppure tutto in Docker con `docker compose up --build` (API su `http://localhost:5000`). Le migrazioni partono da sole.
Nota: `BackEnd/README.md` è **obsoleto** (parla di `GoCare.Api` e di due database).

---

## 3. Tabella delle API

### 3.1 Già implementate

| Area | Metodo + route | Ruolo |
|---|---|---|
| Auth | `POST /auth/register/user` | Pubblico |
| Auth | `POST /auth/register/association` | Pubblico (⚠️ obsoleto: da rimuovere, vedi §3.3) |
| Auth | `POST /auth/verify-email` | Pubblico |
| Auth | `POST /auth/verify-email/resend` | Pubblico |
| Auth | `POST /auth/login` | Pubblico |
| Auth | `POST /auth/refresh` | Pubblico |
| Auth | `POST /auth/logout` | Pubblico (serve il refresh token) |
| Auth | `POST /auth/forgot-password` | Pubblico |
| Auth | `POST /auth/reset-password` | Pubblico |
| Auth | `POST /auth/change-email` | Autenticato |
| Auth | `POST /auth/change-email/confirm` | Pubblico (token) |
| Profilo | `GET` · `PUT` · `PATCH /me/profile` | Caregiver |
| Profilo | `GET` · `PUT` · `PATCH /association/profile` | Associazione |
| Assistiti | `POST /assisted` · `GET /assisted` · `GET /assisted/{id}` · `PUT /assisted/{id}` | Caregiver |
| Assistiti | `POST /assisted/{id}/caregivers` · `DELETE /assisted/{id}/caregivers/{caregiverId}` | Caregiver |
| Trasporti | `POST /transports` · `GET /transports?assistedId=` · `GET /transports/{id}` | Caregiver |
| Trasporti | `POST /transports/{id}/cancel` | Caregiver (⚠️ manca `[Authorize]`, vedi §4) |
| Trasporti | `GET /transports/{id}/status-timeline` | Caregiver |
| Associazione | `POST /association/requests/{id}/accept` | Associazione |
| Associazione | `GET /association/transports` · `GET /association/transports/{id}` | Associazione |
| Associazione | `POST /association/transports/{id}/operator-link` | Associazione |
| Associazione | `GET /association/history` | Associazione |
| Destinazioni | `GET` · `POST /me/destinations` · `PUT` · `DELETE /me/destinations/{id}` | Caregiver |
| Notifiche | `GET /notifications` · `GET /notifications/counters` | Caregiver / Associazione |
| Notifiche | `POST /notifications/{id}/read` · `POST /notifications/read-all` | Caregiver / Associazione |
| Dispositivi | `POST /devices` · `DELETE /devices/{id}` | Caregiver / Associazione |
| Admin | `POST /admin/associations/{id}/accredit` · `POST /admin/associations/{id}/reject` | Admin (⚠️ obsoleti: PA-05 superato, da rimuovere) |

### 3.2 API rimanenti

Ordinate per priorità (prima ciò che blocca il flusso principale richiesta → accettazione → viaggio).

| # | Metodo + route | Ruolo | Cosa deve fare | Service da creare / toccare | Priorità |
|---|---|---|---|---|---|
| 1 | `GET /association/requests` | Associazione | Richieste `Pending` per cui l'associazione è candidata, escluse quelle che ha già rifiutato. Dati minimi dell'assistito (PA-04). | `ListPendingRequestsService` | 🔴 Alta — senza questa l'associazione non vede cosa accettare |
| 2 | `GET /association/requests/{id}` | Associazione | Dettaglio di una richiesta pendente (solo se candidata). | `PendingRequestDetailService` | 🔴 Alta |
| 3 | `POST /association/requests/{id}/decline` | Associazione | Crea `TransportRequestRejection` (`Kind = Declined`, motivo opzionale); se tutte le candidate hanno rifiutato → `NotCovered` + notifica al caregiver. | `DeclineTransportRequestService` (+ `CoverageEvaluator`) | 🔴 Alta |
| 4 | `GET /operatore/{token}` | Pubblico (link) | L'operatore apre il link: verifica che il token sia valido/non scaduto e restituisce i dati del viaggio + stato attuale. | `OperatorScreenService` | 🔴 Alta |
| 5 | `POST /operatore/{token}/status` | Pubblico (link) | Avanza lo stato di esecuzione: valida la transizione (`TripStateMachine`), salva `TripStatusTransition` con `OperatorLabel`, porta la richiesta a `InProgress`/`Completed`, notifica push (+ email su `Completed`/`Suspended`). | `AdvanceTripStatusService` + `TripStateMachine` | 🔴 Alta |
| 6 | `POST /association/transports/{id}/cancel` | Associazione | Disdetta dopo l'accettazione: causale obbligatoria, `TransportRequestRejection` (`CancelledAfterAcceptance`), la richiesta torna `Pending` senza associazione assegnata, notifica al caregiver. | `CancelAcceptedTransportService` | 🟠 Media |
| 7 | `GET /association/requests/{id}/contacts` | Associazione | Recapiti completi del richiedente/assistito solo dopo l'accettazione; scrive `ContactAccessLog` (PA-04). | `RequesterContactsService` | 🟠 Media |
| 8 | `GET /associations/{id}/contacts` | Caregiver | Contatti pubblici dell'associazione che ha preso in carico il viaggio. | `AssociationContactsService` | 🟡 Bassa — nome e telefoni dell'associazione sono già restituiti da `GET /transports/{id}`: probabilmente si può saltare |
| 9 | `DELETE /auth/account` | Autenticato | Eliminazione account con anonimizzazione dei dati personali, mantenendo lo storico dei viaggi (PA-06). | `DeleteAccountService` / `AnonymizeForDeletedAccountAsync` | 🟡 Bassa |
| 10 | `POST /transports/{id}/payment` | Caregiver | Crea la sessione Stripe Checkout per un viaggio `Confirmed` (caregiver collegato all'assistito) e restituisce l'URL di pagamento (UC 5.3, CG-31). | `CreatePaymentSessionService` | 🟠 Media |
| 11 | `POST /payments/stripe/webhook` | Stripe (firma verificata) | Riceve l'esito da Stripe, verifica la firma e segna il viaggio come pagato (importo, id pagamento, istante). Solo il webhook conferma il pagamento. | `StripeWebhookService` | 🟠 Media |

#### API amministratore (UC 11 di `Project_GoCare.md`)

Tutte con `[Authorize(Roles = nameof(EAccountRole.Admin))]`, tranne `set-password` che è pubblica con token.

| # | Metodo + route | UC / story | Cosa deve fare | Service da creare | Priorità |
|---|---|---|---|---|---|
| 12 | `POST /admin/associations` | 11.1 · AD-01 | Crea `Account` (ruolo `Association`, senza password) + `Association` con lo stesso id: denominazione, sede, comuni coperti, telefoni, email, orari. Valida come `UpdateAssociationProfileValidator`, email univoca. Invia email di invito con token monouso a scadenza. | `CreatePartnerAssociationService` | 🔴 Alta — senza questa non esistono associazioni |
| 13 | `POST /auth/set-password` | 11.1 · AD-01 | Pubblico (token di invito): imposta la password, attiva l'account e segna l'email come verificata. Può riusare la logica di `ResetPasswordService`. | `SetPasswordService` | 🔴 Alta |
| 14 | `GET /admin/associations` | 11.6 · AD-06 | Elenco paginato (`PageQuery`/`PagedResult<T>`) con denominazione, comuni, stato account (invito in attesa / attivo / eliminato), conteggio trasporti in carico. Filtri per stato e comune, ricerca per nome. | `ListPartnerAssociationsService` | 🔴 Alta |
| 15 | `GET /admin/associations/{id}` | 11.6 · AD-06 | Dettaglio: dati, contatti, stato account, trasporti assegnati (in carico + storico, solo dati di servizio). | `PartnerAssociationDetailService` | 🟠 Media |
| 16 | `PUT /admin/associations/{id}` | 11.2 · AD-02 | Aggiorna gli stessi campi di `PUT /association/profile` con le stesse validazioni. I comuni valgono solo per le richieste future (candidati già congelati). Cambio email → conferma al nuovo indirizzo. | `UpdatePartnerAssociationService` (può riusare `UpdateAssociationProfileService`) | 🟠 Media |
| 17 | `GET /admin/associations/{id}/requests` | 11.3 · AD-03 | Richieste in cui l'associazione è candidata, con esito per lei (in attesa, accettata, rifiutata, disdetta, non coperta, annullata) + indicatori (ricevute, accettate, rifiutate, disdette, tempo medio di presa in carico). Filtri periodo/esito, paginata, nessun recapito personale. | `PartnerAssociationRequestsService` | 🟡 Bassa |
| 18 | `DELETE /admin/associations/{id}` | 11.4 · AD-04 | 409 se ha trasporti `Confirmed`/`InProgress`. Altrimenti soft delete di account e profilo, revoca refresh token e `TripOperatorLink` validi, esclusione da candidature future; rivalutazione copertura delle richieste pendenti di cui era candidata. | `DeletePartnerAssociationService` | 🟡 Bassa |
| 19 | `GET /admin/caregivers` · `GET /admin/caregivers/{id}` | 11.5 · AD-05 | Ricerca caregiver per email/nome con stato account (`Unverified`, `Active`, `Suspended`, `Deleted`). | `ListCaregiverAccountsService` | 🟡 Bassa |
| 20 | `POST /admin/caregivers/{id}/restore` | 11.5 · AD-05 | Riattiva un account `Suspended`/bloccato (azzera `FailedLoginAttempt`) o `Deleted` **solo se non ancora anonimizzato**. Registra chi/cosa/quando. Perimetro [DA VALIDARE]. | `RestoreCaregiverAccountService` | 🟡 Bassa |
| 21 | `POST /admin/caregivers/{id}/resend-verification` | 11.5 · AD-05 | Reinvia l'email di verifica a un account `Unverified` (riusa `ResendVerificationEmailService`). | — (riuso) | 🟡 Bassa |
| 22 | `POST /admin/caregivers/{id}/password-reset` | 11.5 · AD-05 | Invia l'email di reset password e revoca le sessioni (riusa `ForgotPasswordService`). | — (riuso) | 🟡 Bassa |

**Lavoro non-endpoint ancora da fare:**

| Cosa | Note |
|---|---|
| `TripStateMachine` | Funzione pura `CanTransition(attuale, nuovo, direzione)`: niente salti, `OnlyGo` salta `Returning`, `OnlyReturn` salta `OnSite`, `Suspended` da qualsiasi stato attivo e ritorno allo stato di provenienza. Serve per l'API #5. |
| `CoverageEvaluator` | Decide quando una richiesta diventa `NotCovered` (tutte le candidate hanno rifiutato, o soglia temporale PA-02). Serve per #3 e #6. |
| Notifiche mancanti | `POST /transports` non avvisa le associazioni candidate; `POST /transports/{id}/cancel` non avvisa l'associazione già assegnata. |
| Job schedulati | `TripReminderJob` (promemoria del giorno prima), `CoverageTimeoutJob` (timeout → `NotCovered`), `TokenCleanupJob`. Nessuno scritto. |
| Email reali | `SmtpEmailSender` esiste ma in `Program.cs` è registrato `ConsoleEmailSender` (le email finiscono nel log). |
| Push reali | Solo `ConsolePushSender`; manca un adapter FCM. |
| Sicurezza | Rate limiting su login/forgot-password, uso di `FailedLoginAttempt` (tabella pronta, mai usata), segreti fuori da `appsettings.Development.json`. |
| Paginazione | `PageQuery`/`PagedResult<T>` pronti ma nessuna lista li usa (trasporti, notifiche, storico). |
| Test | Nessun progetto di test presente. |
| Documentazione | Aggiornare `BackEnd/README.md` e la tabella §14 di `PIANO_BACKEND_GoCare.md`. |

### 3.3 Da rimuovere / adeguare per le nuove decisioni

| Cosa | Note |
|---|---|
| `POST /auth/register/association` | Le associazioni non si registrano più da sole (UC 10.1 solo caregiver). Rimuovere controller, service, request e validator. |
| `POST /admin/associations/{id}/accredit` · `reject` | PA-05 superato: un'associazione registrata dall'admin è subito operativa. Rimuovere i due controller e `AccreditAssociationAsync`/`RejectAssociationAsync`; valutare se eliminare `EAccreditationStatus` e il filtro `Status == Accredited` in `CreateTransportService` (sostituendolo con `DeletedAt == null`). |
| `CoveredProvinces` → comuni coperti | Instradamento per comune (PA-03 deciso): specifica completa in §5. |
| Accompagnatori (`Companion`) | UC 1.4 rimossa: togliere `Companions` da `NewTransportRequest`, `CreateTransportService` e dalle risposte; decidere se eliminare tabella ed entità con una migrazione. |
| Pagamento | Nuova entità/colonne (stato pagamento, importo, id Stripe, `PaidAt`) su `TransportRequest`; `TripDetailService` deve restituire i contatti dell'associazione solo se il viaggio è pagato. Configurazione `StripeOptions` (chiave segreta, secret del webhook) fuori dal repo. Importo e destinatario dei fondi ancora aperti (PA-07). |

---

## 4. Problemi trovati nel codice attuale

1. ✅ **Risolto** (commit `29192fa`). **`DeleteTransportController` non aveva `[Authorize]`** (`Controllers/TransportRequest/DeleteTransportController.cs`): una chiamata senza token arriva al controller, `User.FindFirstValue("sub")!` è `null` e `Guid.Parse` lancia un'eccezione → **500 invece di 401**. Va aggiunto `[Authorize(Roles = nameof(EAccountRole.Person))]` come negli altri controller dei trasporti (e corretto il refuso `DeleteTranasport`).
2. **Doppio endpoint per il profilo**: esistono sia `PATCH` (completamento, `ProfilesController`) sia `PUT` (aggiornamento) su `/me/profile` e `/association/profile`. Conviene decidere quale usa il frontend e togliere l'altro.
3. ✅ **Risolto** (commit `b127e88`). **Annullamento caregiver**: `DeleteTransportService` usava `ForbiddenException` (403) quando lo stato non permette l'annullamento; ora `ConflictException` (409).
5. ✅ **Risolto** (commit `8c8ca8a`). Le liste che proiettano indirizzi owned (`StartAddress`, `EndAddress`, `SavedAddress`) andavano in 500 (*tracking query … owned entity without owner*): aggiunto `AsNoTracking()`. Regola per le nuove query di sola lettura: sempre `AsNoTracking()`.
4. **Accreditamento**: superato dalla nuova decisione (le associazioni le registra l'admin, PA-05 superato). Il codice di accreditamento va rimosso, vedi §3.3.

---

## 5. Instradamento per comuni coperti (al posto delle province)

### 5.1 La decisione

Dopo la revisione con il docente l'area di operatività di un'associazione non è più la **provincia** ma un **elenco esplicito di comuni** (`Project_GoCare.md`, PA-03 e §6). La provincia è troppo ampia: un'associazione di Assisi non opera su tutta la provincia di Perugia, ma sul proprio comune e su quelli limitrofi.

Esempio: l'associazione di Assisi copre Assisi, Bastia Umbra, Perugia, Foligno e Valfabbrica.

| Richiesta con partenza da… | Arriva all'associazione di Assisi? |
|---|---|
| Bastia Umbra (`054002`) | ✅ sì |
| Perugia (`054039`) | ✅ sì |
| Spoleto (`054051`) | ❌ no: non è nel suo elenco |

Regole:

- l'elenco è **scelto dall'associazione** (o dall'admin): nessun calcolo automatico per distanza o per confini;
- i comuni sono identificati dal **codice ISTAT** (tabella `comuni`, già popolata da `ComuniSeeder` all'avvio), non dal nome: il nome non è univoco (comuni omonimi in province diverse) e si può scrivere in modi diversi (accenti, apostrofi, maiuscole);
- il comune di partenza si ricava dall'indirizzo di partenza (`City` + `Province`), che `AddressValidator` già verifica contro la tabella `comuni`;
- i candidati restano **congelati** alla creazione (`transport_request_candidates`): cambiare i comuni coperti vale solo per le richieste future;
- almeno un comune coperto è obbligatorio (UC 8.4, 11.1).

### 5.2 Modello dati

Si sostituisce la colonna `associations.covered_provinces` (`text[]`) con una **tabella ponte** associazione ↔ comune:

```text
association_covered_comuni
  association_id      uuid         PK (1/2), FK → associations.id   ON DELETE CASCADE
  comune_codice_istat varchar(10)  PK (2/2), FK → comuni.codice_istat
  index (comune_codice_istat)      -- per l'instradamento: "chi copre questo comune?"
```

Perché una tabella e non un array di codici:

- **integrità**: la FK verso `comuni` impedisce di salvare un codice inesistente;
- **instradamento e filtri**: la ricerca "associazioni che coprono il comune X" usa l'indice su `comune_codice_istat`, sia in `CreateTransportService` sia nel filtro della lista admin;
- **lettura**: per mostrare nome e provincia di ogni comune basta un `JOIN` con `comuni`.

Perché la FK punta a `codice_istat` e non a `comuni.id`: `ComuniSeeder` genera `comuni.id` con `Guid.NewGuid()`, quindi lo stesso comune ha un id diverso in ogni database (locale di ognuno, Docker, produzione). Il codice ISTAT invece è stabile ovunque ed è già **univoco** (`HasIndex(c => c.CodiceIstat).IsUnique()`), quindi può fare da chiave alternativa (`HasPrincipalKey(c => c.CodiceIstat)` in EF Core).

Nel dominio:

```csharp
// Models/Domain/AssociationCoveredComune.cs — entità della tabella ponte
public sealed class AssociationCoveredComune
{
    private AssociationCoveredComune() { }
    public AssociationCoveredComune(Guid associationId, string comuneCodiceIstat)
    {
        AssociationId = associationId;
        ComuneCodiceIstat = comuneCodiceIstat;
    }

    public Guid AssociationId { get; }
    public string ComuneCodiceIstat { get; } = null!;
}
```

- `Association`: togliere `CoveredProvinces`; `IsProfileComplete` controlla che esista almeno un comune coperto. I metodi `CompleteProfile`/`UpdateProfile` non ricevono più la lista: l'elenco si sostituisce nel service (cancella le righe dell'associazione, inserisce le nuove) nella stessa `SaveChangesAsync`.
- Come per le altre tabelle ponte del progetto (`caregiver_assisted_links`, `transport_request_candidates`) non servono proprietà di navigazione: la relazione si dichiara nel `DbContext` con `HasOne<Association>().WithMany()` e `HasOne<Comune>().WithMany().HasForeignKey(x => x.ComuneCodiceIstat).HasPrincipalKey(c => c.CodiceIstat)`.
- Togliere l'indice `ix_transport_requests_start_address_province`: non serve più all'instradamento.

### 5.3 Contratto delle API

Le API parlano **solo di codici ISTAT** in scrittura; in lettura restituiscono anche nome e provincia, per il frontend.

**Scrittura** — `PUT /association/profile`, `PATCH /association/profile`, in futuro `POST /admin/associations` (#12) e `PUT /admin/associations/{id}` (#16):

```json
{
  "name": "Croce Bianca Assisi",
  "headquarter": { "street": "Via Roma", "number": "1", "postalCode": "06081", "city": "Assisi", "province": "PG" },
  "phones": ["+39 075 1234567"],
  "coveredComuni": ["054001", "054002", "054039", "054018", "054057"],
  "availabilityHours": "Lun-Ven 8-18"
}
```

Validazione (`UpdateAssociationProfileValidator`, `CompleteAssociationProfileValidator`):

| Regola | Messaggio |
|---|---|
| `coveredComuni` non vuoto | "Inserire almeno un comune coperto." |
| ogni codice: 6 cifre | "Codice ISTAT non valido." |
| nessun duplicato | "Comune ripetuto." |
| ogni codice esiste in `comuni` (validator asincrono, come in `AddressValidator`) | "Comune non trovato." |

**Lettura** — `GET /association/profile` (`AssociationProfileResponse`) e `GET /admin/associations` (`PartnerAssociationListItemResponse`): `CoveredProvinces` diventa

```json
"coveredComuni": [
  { "codiceIstat": "054001", "nome": "Assisi", "provincia": "PG" },
  { "codiceIstat": "054002", "nome": "Bastia Umbra", "provincia": "PG" }
]
```

ordinati per nome.

**Filtro admin** — `GET /admin/associations`: il parametro `?province=` diventa `?comune=054001` (codice ISTAT): associazioni che coprono quel comune.

**Autocompletamento** — il frontend sceglie i comuni con `GET /comuni/search?q=...`, che quindi passa da "controller temporaneo" a endpoint vero:

- aggiungere `[Authorize]` (qualsiasi utente autenticato);
- aggiungere `codiceIstat` a `ComuneSuggestion`: senza, il frontend non ha il valore da inviare in `coveredComuni`.

### 5.4 Instradamento in `CreateTransportService`

Prima:

```csharp
.Where(a => a.Status == EAccreditationStatus.Accredited && a.DeletedAt == null
         && a.CoveredProvinces != null && a.CoveredProvinces.Contains(startAddress.Province))
```

Dopo, in due passi:

1. ricavare il codice ISTAT del comune di partenza da `comuni` con `NomeComune == startAddress.City && Provincia == startAddress.Province` (lo stesso confronto di `AddressValidator`, che garantisce già che il comune esista);
2. candidati = associazioni attive con una riga in `association_covered_comuni` per quel codice:

```csharp
.Where(a => a.Status == EAccreditationStatus.Accredited && a.DeletedAt == null
         && db.AssociationCoveredComuni.Any(c => c.AssociationId == a.Id && c.ComuneCodiceIstat == startCodiceIstat))
```

Il filtro su `Status == Accredited` resta finché non si chiude l'accreditamento (§3.3, PA-05).

### 5.5 Migrazione

Una migrazione unica (es. `CoveredComuniInsteadOfProvinces`):

1. crea `association_covered_comuni` con PK composta, FK e indice;
2. **converte i dati esistenti**, per non perdere la copertura delle associazioni già configurate: ogni provincia coperta diventa l'elenco di tutti i comuni di quella provincia. In `Up()`, prima di togliere la colonna:

   ```sql
   INSERT INTO association_covered_comuni (association_id, comune_codice_istat)
   SELECT a.id, c.codice_istat
   FROM associations a
   CROSS JOIN LATERAL unnest(a.covered_provinces) AS p(sigla)
   JOIN comuni c ON c.provincia = p.sigla
   ON CONFLICT DO NOTHING;
   ```

   È un punto di partenza "largo": ogni associazione poi restringe l'elenco ai comuni che copre davvero. Su un database nuovo la query non inserisce nulla (`comuni` viene popolata da `ComuniSeeder` *dopo* le migrazioni), ed è corretto così: non ci sono ancora associazioni;
3. elimina la colonna `covered_provinces` e l'indice su `start_address_province`;
4. `Down()`: ricrea `covered_provinces` (ricostruendola con le sigle distinte delle province dei comuni coperti) ed elimina la tabella.

Le righe di `transport_request_candidates` già create **non si toccano**: sono la lista congelata delle richieste esistenti.

### 5.6 File da toccare (checklist)

| Area | File |
|---|---|
| Dominio | `Models/Domain/Association.cs`, nuovo `Models/Domain/AssociationCoveredComune.cs` |
| Database | `Data/GoCareDbContext.cs` (nuovo `DbSet`, configurazione con `HasPrincipalKey`, rimozione indice provincia), nuova migrazione |
| DTO | `Dtos/Domain/Requests/CompleteAssociationProfileRequest.cs`, `UpdateAssociationProfileRequest.cs`; `Dtos/Domain/Responses/AssociationProfileResponse.cs`; `Dtos/Admin/Responses/PartnerAssociationListItemResponse.cs`; nuovo `CoveredComuneResponse(CodiceIstat, Nome, Provincia)` |
| Validator | `Dtos/Domain/Validators/CompleteAssociationProfileValidator.cs`, `UpdateAssociationProfileValidator.cs` |
| Service | `Services/Profile/UpdateAssociationProfileService.cs`, `GetAssociationProfileService.cs`; `Services/Provisioning/ProfileProvisioningService.cs` + `AssociationProfileData.cs`; `Services/Transport/CreateTransportService.cs`; `Services/Admin/ListPartnerAssociationService.cs`; `Services/Domain/SearchComuneService.cs` |
| Controller | `Controllers/Profile/UpdateAssociationProfileController.cs`, `Controllers/Domain/ProfilesController.cs`, `Controllers/Admin/ListPartnerAssociationController.cs` (`province` → `comune`), `Controllers/Domain/SearchComuneController.cs` (`[Authorize]`) |
| Frontend | form profilo associazione e form admin: selezione multipla con autocompletamento al posto delle sigle di provincia |

### 5.7 Test

Con due associazioni come nella guida dell'API #1, ma con comuni al posto delle province:

| Associazione | `coveredComuni` |
|---|---|
| A | Assisi `054001`, Bastia Umbra `054002`, Perugia `054039` |
| B | Spoleto `054051` |

| Caso | Atteso |
|---|---|
| Trasporto con partenza da Perugia | candidata solo **A** (`transport_request_candidates`, `GET /association/requests`) |
| Trasporto con partenza da Spoleto | candidata solo **B** |
| Trasporto con partenza da Foligno | nessuna candidata → la richiesta non arriva a nessuno |
| `PUT /association/profile` con `coveredComuni: []` | **422** "Inserire almeno un comune coperto." |
| `PUT /association/profile` con `"999999"` | **422** "Comune non trovato." |
| A aggiunge Foligno, poi nuovo trasporto da Foligno | candidata **A**; il trasporto precedente da Foligno resta senza candidate (lista congelata) |
| `GET /admin/associations?comune=054039` | solo **A** |
| `GET /comuni/search?q=assi` senza token | **401** |
