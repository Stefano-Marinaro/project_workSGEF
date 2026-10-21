# Modifiche al mockup "Nuovo Trasporto" e conseguenze lato backend

**Ambito:** Artifact Design "GoCare - Mockup Nuovo Trasporto"
(`https://claude.ai/artifact/1WfEQEUtwtcNHuAFAFi9sA`) — pagine Trasporti
(Prossimi), Trasporti (Passati), Wizard "Nuovo trasporto", Header.

## 1. Modifiche fatte al mockup (solo UI, nessun codice reale)

### Header (componente condiviso da tutte le pagine)
- Provato un azzurro chiaro, poi scartato su richiesta.
- Colore finale: viola pieno del brand (`#6849a7`), nome e icone in bianco,
  cerchi avatar/notifica in bianco trasparente, pallino notifica corallo
  (`#ff7a90`) per restare leggibile sul viola.
- Provato un logo ritagliato in alto a sinistra, poi rimosso su richiesta.
- Spaziatura sotto l'header aumentata: il titolo di ogni pagina ora ha
  `margin-top: 20px` invece di 0–4px.

### Pagina "Trasporti" (Prossimi)
- Lista dei trasporti raggruppata in sezioni per data: **Oggi**, **Domani**,
  poi la data per esteso (es. "24 ottobre") per i giorni successivi.
- Card dei trasporti arricchite con percorso (`Casa → destinazione`) e orario
  di partenza.

### Pagina "Passati" (nuova)
- Nuova pagina gemella di "Prossimi", stessa struttura (header, tab
  Prossimi/Passati, bottom nav), con trasporti già conclusi: stato
  "Concluso", riferimento temporale relativo ("Ieri", "2 giorni fa", ...).
- Le due tab "Prossimi"/"Passati" sono ora collegate a vicenda.
- Rimossa la vecchia sezione "Storico" e i tre bottoni statistici
  (Approvati/Non coperti/In attesa) che linkavano a pagine ormai eliminate.

### Dimensioni dei testi
- Aumentate di ~1–2px su tutte le pagine (titoli, nomi, etichette, orari),
  per restare sopra i minimi di leggibilità consigliati su mobile (iOS HIG:
  default 17pt/minimo 11pt; pratica comune Material: minimo 16px per il testo
  "body").

### Wizard "Nuovo trasporto"
- **Step 1 — Assistito:** aggiunto un bottone "+" circolare viola nella barra
  di ricerca, per aggiungere un nuovo assistito se non esiste (apre un
  modale "Aggiungi assistito": Nome, Cognome, Data di nascita, Telefono).
- **Step 1 — Tipo di viaggio:** "Trasferimento" ora forza "Solo andata/ritorno"
  (OneWay) come già succede per Ricovero e Dimissione — *nel mockup*; la
  stessa regola va applicata lato backend (vedi §2, punto 1).
- **Step 2 — Dati:** i campi Data/Ora sono diventati input nativi `date`/`time`:
  al tocco aprono calendario e selettore orario del dispositivo invece di
  mostrare testo statico.
- **Step 2 — Indirizzo:** l'etichetta "Città" è diventata "Comune" nel modale
  di aggiunta indirizzo (e nel form indirizzo di Dettaglio assistito).
- **Step 3 — titolo:** "Accompagnatori e contatti" → "Caregiver e contatti".
- **Step 3 — rimozione Accompagnatore:** tolta del tutto la ricerca "Cerca
  accompagnatore" e il modale "Aggiungi accompagnatore". Non esisterà un
  concetto di "accompagnatore" nel dominio — era un errore di
  pianificazione iniziale (vedi §2, punto 2 per le conseguenze reali sul
  backend, dove il concetto esisteva già).
- **Step 3 — Contatto preferenziale:** nuova sezione che mostra **solo** il
  caregiver scelto come contatto principale per questa richiesta (di
  default chi crea la richiesta, "Nome Cognome (tu)"), seguita da una linea
  separatrice sottile e grigia e dalla sezione **"Altri contatti"** con gli
  altri caregiver collegati all'assistito. Ogni riga ha una maniglia (icona
  a pallini) che, al click, sposta quel caregiver tra le due sezioni (al
  posto del drag-and-drop reale, non realizzabile in un mockup statico). Se
  "Contatto preferenziale" resta vuota, compare una card con bordo
  tratteggiato grigio e scritta centrale "Trascina un caregiver".
- **Step 3 — Contatti alternativi:** nuova sezione facoltativa, separata dai
  caregiver: permette di aggiungere (via modale dedicato) altri numeri di
  telefono liberi, con nota opzionale, rimovibili singolarmente dalla lista.

## 2. Conseguenze lato backend

| # | Modifica mockup | Cosa serve nel backend | Stato |
|---|---|---|---|
| 1 | Trasferimento → OneWay forzato | Esteso `TransportRequest.EnsureScheduleConsistency()` e `NewTransportValidator` includendo `ETripType.Transfer` nella regola già esistente per Ricovero/Dimissione | **Fatto** — su richiesta esplicita dell'utente ("aggiusta tutto il backend tu"), applicato direttamente |
| 2 | Rimozione "Accompagnatore" | `Companion` era un'entità di dominio reale, già cablata in `CreateTransportService`. Rimossi: entità (`Models/Domain/Companion.cs`), DTO (`CompanionRequest`), validator (`CompanionValidator`), il campo `Companions` da `NewTransportRequest`, la regola da `NewTransportValidator`, l'estrazione/parametro in `CreateTransportController` e `CreateTransportService`, il `DbSet<Companion>` e il mapping in `GoCareDbContext`, il valore `ENotificationType.CompanionUpdated` (non referenziato altrove). Generata la migration EF Core `20261021232147_RemoveCompanionEntity` che droppa la tabella `companions` | **Fatto** — build verificata (0 errori), migration generata e pronta ad applicarsi automaticamente al prossimo avvio (`db.Database.Migrate()` in `Program.cs`) |
| 3 | "Comune" al posto di "Città" | Nessuna: il backend valida già con messaggi "Comune" (solo la proprietà C# interna si chiama `City`) | Nessuna azione necessaria |
| 4 | Sezioni Oggi/Domani/data + pagina Passati separata | Aggiunto un parametro di query `when` (`ETripTimeframe`: `Upcoming`/`Past`) a `GET /transports`. `Upcoming` = stato Pending/Confirmed/InProgress, ordinato per partenza crescente; `Past` = stato Completed/Cancelled/NotCovered, ordinato per partenza decrescente. Senza il parametro il comportamento resta quello di prima (tutti i trasporti, decrescente) — nessuna rottura per chi già chiama l'endpoint | **Fatto** |
| 5 | "Caregiver collegati a [assistito]" | In realtà **già esisteva**: `GET /assisted/{id}` (`AssistedPersonService.GetDetailAsync`) restituisce già `Caregivers: IReadOnlyList<CaregiverResponse>`. Mancava solo il `Phone` nella risposta, usato dal mockup — aggiunto a `CaregiverResponse` e alla query | **Fatto** |
| 6 | "Contatto preferenziale" + "Altri contatti" + "Contatti alternativi" | Aggiunto `TransportRequest.PreferredContactPersonId` (FK a `Person`, validato: deve essere il richiedente o un caregiver collegato all'assistito) e una nuova entità `AlternativeContact` (Phone + Note opzionale, 0..N per richiesta) con tabella propria | **Fatto** |
| 7 | Date/ore con picker nativi | Nessuna: solo UX frontend, formato dati invariato | Nessuna azione necessaria |
| 8 | Font più grandi | Nessuna: solo styling frontend | Nessuna azione necessaria |

## 3. File backend modificati per il punto 2

- `src/GoCare/Models/Domain/TransportRequest.cs` — regola OneWay estesa a Transfer
- `src/GoCare/Models/Domain/Companion.cs` — **eliminato**
- `src/GoCare/Dtos/Transport/Requests/NewTransportRequest.cs` — tolto il campo `Companions`
- `src/GoCare/Dtos/Transport/Requests/CompanionRequest.cs` — **eliminato**
- `src/GoCare/Dtos/Transport/Validators/NewTransportValidator.cs` — regola OneWay estesa, tolta la validazione dei companion
- `src/GoCare/Dtos/Transport/Validators/CompanionValidator.cs` — **eliminato**
- `src/GoCare/Controllers/TransportRequest/CreateTransportController.cs` — tolta l'estrazione/il passaggio dei companion
- `src/GoCare/Services/Transport/CreateTransportService.cs` — tolto il parametro e la persistenza dei companion
- `src/GoCare/Data/GoCareDbContext.cs` — tolto `DbSet<Companion>` e il relativo mapping
- `src/GoCare/Models/Enums/ENotificationType.cs` — tolto `CompanionUpdated`
- `src/GoCare/Data/Migrations/20261021232147_RemoveCompanionEntity.cs` — nuova migration, droppa la tabella `companions`

## 4. File backend modificati per i punti 5 e 6

- `src/GoCare/Dtos/Domain/Responses/AssistedPersonResponses.cs` — aggiunto `Phone` a `CaregiverResponse`
- `src/GoCare/Services/Domain/AssistedPersonService.cs` — la query di `GetDetailAsync` ora proietta anche `person.Phone`
- `src/GoCare/Models/Domain/TransportRequest.cs` — aggiunta proprietà `PreferredContactPersonId` (Guid, FK a `Person`)
- `src/GoCare/Models/Domain/AlternativeContact.cs` — **nuova entità**: `Id`, `TransportRequestId`, `Phone`, `Note?`
- `src/GoCare/Dtos/Transport/Requests/AlternativeContactRequest.cs` — **nuovo DTO**
- `src/GoCare/Dtos/Transport/Validators/AlternativeContactValidator.cs` — **nuovo validator**
- `src/GoCare/Dtos/Transport/Requests/NewTransportRequest.cs` — aggiunti `PreferredContactPersonId` (obbligatorio) e `AlternativeContacts` (lista opzionale)
- `src/GoCare/Dtos/Transport/Validators/NewTransportValidator.cs` — validazione dei due nuovi campi
- `src/GoCare/Controllers/TransportRequest/CreateTransportController.cs` — passa i nuovi campi al service
- `src/GoCare/Services/Transport/CreateTransportService.cs` — valida che il contatto preferenziale sia il richiedente o un caregiver collegato all'assistito (altrimenti 403), persiste i contatti alternativi
- `src/GoCare/Data/GoCareDbContext.cs` — `DbSet<AlternativeContact>`, mapping della nuova entità, FK `PreferredContactPersonId → Person`
- `src/GoCare/Data/Migrations/20261021232616_AddPreferredContactAndAlternativeContacts.cs` — nuova migration: aggiunge la colonna e crea la tabella `alternative_contacts`

Tutte le modifiche di questo file sono state applicate direttamente
dall'assistente su richiesta esplicita e ribadita dell'utente ("aggiusta
tutto il backend tu" → "ti ho detto di aggiustare tutto"), in deroga alla
modalità didattica di default di `AGENTS.md` (§1–2). Ogni gruppo di modifiche
è stato verificato con una build pulita (0 errori) prima di generare la
migration corrispondente; la tabella `transport_requests` nel DB di sviluppo
era vuota al momento della migration del punto 6, quindi il valore
placeholder (`Guid.Empty`) usato da EF per la colonna NOT NULL aggiunta non
ha creato righe orfane.

## 5. File backend modificati per il punto 4 (filtro Prossimi/Passati)

- `src/GoCare/Models/Enums/ETripTimeframe.cs` — **nuovo enum**: `Upcoming`, `Past`
- `src/GoCare/Services/Transport/ListTransportsService.cs` — `ListMineAsync` accetta ora un `ETripTimeframe? timeframe` opzionale e filtra/ordina di conseguenza
- `src/GoCare/Controllers/TransportRequest/ListTransportsController.cs` — nuovo parametro di query `when` su `GET /transports`

Nessuna migration necessaria: è solo un filtro su una colonna (`RequestStatus`)
già esistente, non un cambio di schema. Build verificata (0 errori).
