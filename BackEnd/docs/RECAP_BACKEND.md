# Recap backend GoCare — passaggio di consegne

> Stato del codice al commit `ce6ca9e` (merge PR #25, `PUT /association/profile`), 19/10/2026.
> Per chi prende in mano il backend (Stefano e Giorgia). Dove questo documento e `PIANO_BACKEND_GoCare.md` non coincidono, vale questo: il piano non è stato aggiornato dopo l'avvio dei controller di dominio (lì risultano ancora "da fare" endpoint che esistono già).

---

## 1. Riassunto delle cose fatte

- **Fondamenta**: un solo progetto `.NET 10` (`BackEnd/src/GoCare`), un solo database PostgreSQL `gocare`, un solo `GoCareDbContext`. Le migrazioni vengono applicate **automaticamente all'avvio** (`db.Database.Migrate()` in `Program.cs`).
- **`docker-compose.yml`** (nella root del repo): Postgres 16 + backend (porta `5000` → `8080`).
- **Auth completa**: registrazione utente/associazione, verifica email (con reinvio), login, refresh con rotazione, logout, password dimenticata/reset, cambio email con conferma.
- **Account admin** creato da migrazione (`SeedAdminAccount`, email `admin@gocare.it`) + endpoint di accreditamento/rifiuto associazioni.
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
- **Creazione** (`CreateTransportService`): controlla che il caregiver sia collegato all'assistito, salva richiesta + accompagnatori, calcola i **candidati** = associazioni `Accredited` la cui lista `CoveredProvinces` contiene la provincia di partenza (`TransportRequestCandidate`).
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
| Auth | `POST /auth/register/association` | Pubblico |
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
| Admin | `POST /admin/associations/{id}/accredit` · `POST /admin/associations/{id}/reject` | Admin |

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
| 10 | `GET /admin/associations?status=Pending` | Admin | Lista delle associazioni da accreditare (oggi l'admin deve conoscere l'id). Non era nel piano, ma serve per usare davvero accredit/reject. | `ListAssociationsForAdminService` | 🟡 Bassa |

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

---

## 4. Problemi trovati nel codice attuale

1. **`DeleteTransportController` non ha `[Authorize]`** (`Controllers/TransportRequest/DeleteTransportController.cs`): una chiamata senza token arriva al controller, `User.FindFirstValue("sub")!` è `null` e `Guid.Parse` lancia un'eccezione → **500 invece di 401**. Va aggiunto `[Authorize(Roles = nameof(EAccountRole.Person))]` come negli altri controller dei trasporti (e corretto il refuso `DeleteTranasport`).
2. **Doppio endpoint per il profilo**: esistono sia `PATCH` (completamento, `ProfilesController`) sia `PUT` (aggiornamento) su `/me/profile` e `/association/profile`. Conviene decidere quale usa il frontend e togliere l'altro.
3. **Annullamento caregiver**: `DeleteTransportService` usa `ForbiddenException` (403) quando lo stato non permette l'annullamento; semanticamente sarebbe un `ConflictException` (409).
4. **Gate di accreditamento**: un'associazione non accreditata può fare login e usare gli endpoint `/association/*`. In pratica non può accettare nulla (non diventa mai candidata), ma va deciso se bloccarla esplicitamente (PA-05).
