# Project GoCare

_Convenzioni di lettura: i punti ancora da decidere sono segnalati in corsivo con l'etichetta [DA VALIDARE] e ripresi tutti nella sezione "Cosa manca / Punti aperti". Alcuni identificativi (Use Case, sotto-voci, user story) non sono consecutivi: gli ID non vengono riassegnati, per non invalidare i riferimenti incrociati in uso nel resto del documento._

---

# 1. Target, problema e soluzione

## 1.1 Target

Persone che hanno bisogno del supporto di associazioni di volontariato per il trasporto, i loro caregiver (familiari o assistenti che gestiscono la prenotazione per conto dell'assistito) e le associazioni di volontariato e soccorso che erogano il servizio di trasporto.

## 1.2 Problema

Spesso le persone non hanno chi possa accompagnarle a fare visite mediche, perché non automunite, non autosufficienti o sole.

Problemi collegati:

- Le persone dimenticano gli appuntamenti e le visite programmate.
- Una volta che il paziente è preso in carico, le famiglie devono fare ripetute chiamate all'associazione per sapere lo stato del trasporto.
- In caso di annullamento del viaggio deve essere trovata una soluzione alternativa, per non lasciare l'assistito senza copertura.
- Le associazioni ricevono le richieste su canali non strutturati (telefono, messaggi) e non hanno una vista unica delle richieste pendenti e dei trasporti già presi in carico.

## 1.3 Soluzione

- Offrire un luogo digitale di prenotazione viaggi in contatto diretto con le associazioni di volontariato e soccorso.
- Offrire funzionalità di registrazione e notifica dei prossimi trasporti.
- Offrire la visualizzazione in tempo reale dello stato del viaggio per familiari e caregiver a distanza (non preso in carico, preso in carico, in arrivo, in visita, in ritorno, concluso).
- Offrire una soluzione alternativa in caso di mancata copertura, anche coinvolgendo un'altra associazione.
- Offrire alle associazioni una dashboard operativa unica per richieste pendenti, trasporti accettati e storico.

---

# 2. Use Cases

Elenco dei macro Use Case e delle relative sotto-voci. L'identificativo UC 2 non è usato: dopo la creazione l'unica azione su un viaggio è l'annullamento (UC 3), non esiste modifica del viaggio.

## UC 1 – RICHIEDI TRASPORTO

- 1.1 Richiedere un trasporto (data, orario, luogo di partenza, destinazione).
- 1.2 Scegliere la direzione del viaggio (andata e ritorno, solo andata, solo ritorno).
- 1.3 Scegliere destinazioni diverse dal domicilio abituale.
- 1.4 Indicare eventuali accompagnatori.
- 1.5 Indicare il tipo/obiettivo del viaggio (visita medica, ricovero, dimissione, trasferimento).
- 1.6 Richiedere un trasporto per un assistito collegato al proprio account (prenotazione per conto terzi).
- 1.7 Indicare per il ritorno una destinazione diversa da quella di partenza dell'andata.

## UC 3 – ANNULLA TRASPORTO

- 3.1 Annullamento del trasporto da parte del caregiver.
- 3.2 Annullamento del trasporto da parte dell'associazione.
- 3.3 Specifica della causale di annullamento.

## UC 4 – STATO E NOTIFICHE

Notifiche e stato lato caregiver:

- 4.1 Stato del trasporto in tempo reale (non preso in carico, preso in carico, in arrivo, in visita, in ritorno, sospeso, concluso).
- 4.2 Notifica push al cambio di stato del viaggio.
- 4.3 Notifica push per viaggio imminente (giorno precedente).
- 4.4 Notifica di esito della richiesta: presa in carico, rifiuto o mancata copertura.
- 4.6 Notifica di annullamento del trasporto da parte dell'associazione.

Notifiche lato associazione:

- 4.7 Notifica di nuova richiesta di trasporto.
- 4.9 Notifica di annullamento di un trasporto da parte dell'utente.

Gestione operativa dello stato:

- 4.10 Aggiornamento dello stato del viaggio da parte dell'associazione/operatore.

## UC 5 – VISUALIZZAZIONE "I MIEI VIAGGI"

- 5.1 Visualizzazione di tutte le richieste di trasporto effettuate dal caregiver, con data, orario e destinazioni.
- 5.2 Visualizzazione dello stato della richiesta (in attesa, confermata, in esecuzione, conclusa, non coperta, annullata).
- 5.3 Visualizzazione del nome dell'associazione che ha preso in carico il viaggio, con i relativi contatti.
- 5.4 Visualizzazione dello storico dei viaggi conclusi.
- 5.5 Visualizzazione dello stato di avanzamento del viaggio in corso.

## UC 6 – VISUALIZZAZIONE E GESTIONE RICHIESTE PENDENTI (associazione)

- 6.1 Dashboard con le richieste di trasporto dei caregiver destinate all'associazione, con filtri (data, orario, destinazione, tipo di viaggio).
- 6.2 Visualizzazione del dettaglio della richiesta (informazioni personali, data, ora, destinazioni, accompagnatori).
- 6.3 Accettazione di una richiesta di trasporto pendente.
- 6.4 Rifiuto / declino di una richiesta di trasporto pendente.
- 6.5 Visualizzazione dei contatti della persona che ha prenotato il trasporto.

## UC 7 – VISUALIZZAZIONE TRASPORTI ACCETTATI (associazione)

- 7.1 Dashboard con tutti i trasporti accettati e le relative informazioni complete.
- 7.2 Storico dei viaggi conclusi dall'associazione.

## UC 8 – VISUALIZZAZIONE E GESTIONE INFORMAZIONI PROFILO

Profilo caregiver:

- 8.1 Visualizzazione delle informazioni personali.
- 8.2 Modifica delle informazioni personali.
- 8.3 Visualizzazione e modifica dei dati di contatto.
- 8.4 Gestione delle destinazioni salvate (elenco, aggiunta, modifica, eliminazione).

Profilo associazione:

- 8.5 Visualizzazione delle informazioni dell'associazione, comprese le province coperte.
- 8.6 Aggiunta e modifica delle informazioni di contatto dell'associazione.

## UC 9 – GESTIONE ASSISTITI

L'assistito non è un account: è un'anagrafica gestita dal caregiver, senza login e senza possibilità di interazione propria. Un assistito può essere collegato a più caregiver; ogni caregiver con un collegamento attivo vede tutti i dati e tutti i viaggi dell'assistito, indipendentemente da chi lo ha originariamente aggiunto. **In v0 il collegamento è sempre diretto e immediato**, anche quando riguarda un secondo caregiver: non è richiesta nessuna accettazione. Un flusso di invito con consenso esplicito (link condivisibile via e-mail/WhatsApp) è previsto per la v1 — vedi §14 "Roadmap v1".

- 9.1 Aggiunta di un assistito da parte del caregiver.
- 9.2 Visualizzazione dell'elenco dei propri assistiti.
- 9.3 Visualizzazione del dettaglio di un assistito (dati anagrafici e viaggi).
- 9.4 Modifica dei dati di un assistito.
- 9.5 Collegamento diretto di un ulteriore caregiver a un assistito già esistente.
- 9.6 Rimozione di un collegamento caregiver↔assistito (il proprio o quello di un altro caregiver collegato).

## UC 10 – ACCOUNT

- 10.1 Creazione dell'account (caregiver o associazione).
- 10.2 Login.
- 10.3 Recupero delle credenziali in caso di smarrimento.
- 10.4 Eliminazione dell'account.
- 10.5 Accreditamento dell'associazione da parte di GoCare.

---

# 3. User Stories e mappatura sugli Use Case

Le user story sono riportate con il riferimento allo Use Case (e alla sotto-voce) di appartenenza.

## 3.1 Stories caregiver

| ID    | User story                                                                                                                                                                 | UC            |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------- |
| CG-01 | Come caregiver voglio potermi registrare in GoCare.                                                                                                                        | 10.1          |
| CG-02 | Come caregiver voglio poter effettuare il login.                                                                                                                           | 10.2          |
| CG-03 | Come caregiver voglio poter modificare i miei dati personali e di contatto.                                                                                                | 8.2, 8.3      |
| CG-04 | Come caregiver voglio poter recuperare la password in caso di smarrimento.                                                                                                 | 10.3          |
| CG-05 | Come caregiver voglio poter eliminare il mio account.                                                                                                                      | 10.4          |
| CG-14 | Come caregiver voglio poter registrare il prossimo trasporto del mio assistito.                                                                                            | 1.1, 1.6      |
| CG-15 | Come caregiver voglio poter scegliere la direzione del viaggio (andata e ritorno, solo andata, solo ritorno).                                                              | 1.2           |
| CG-16 | Come caregiver voglio poter indicare il tipo di viaggio (visita, ricovero, dimissione, trasferimento).                                                                     | 1.5           |
| CG-17 | Come caregiver voglio poter scegliere una destinazione diversa dal domicilio abituale dell'assistito.                                                                      | 1.3           |
| CG-18 | Come caregiver voglio poter salvare, modificare ed eliminare le mie destinazioni ricorrenti.                                                                               | 8.4           |
| CG-19 | Come caregiver voglio poter indicare un accompagnatore per il viaggio.                                                                                                     | 1.4           |
| CG-23 | Come caregiver voglio poter annullare il viaggio.                                                                                                                          | 3.1           |
| CG-24 | Come caregiver voglio poter indicare il motivo dell'annullamento.                                                                                                          | 3.3           |
| CG-25 | Come caregiver voglio sapere se il viaggio non può essere coperto.                                                                                                         | 4.4           |
| CG-26 | Come caregiver voglio ricevere notifica quando il viaggio è preso in carico dall'associazione.                                                                             | 4.4           |
| CG-27 | Come caregiver voglio essere avvisato tempestivamente se un viaggio già preso in carico viene annullato dall'associazione, così da poter cercare un'alternativa.           | 4.6           |
| CG-28 | Come caregiver voglio poter ricevere notifica dei prossimi trasporti programmati.                                                                                          | 4.3           |
| CG-29 | Come caregiver voglio poter vedere lo stato in tempo reale del viaggio dell'assistito preso in carico (posizione GPS esclusa, vedi Roadmap v1).                            | 4.1, 4.2, 5.5 |
| CG-30 | Come caregiver voglio poter vedere lo stato della mia richiesta (in attesa, confermata, in esecuzione, conclusa, non coperta, annullata).                                  | 5.2           |
| CG-31 | Come caregiver voglio poter vedere i contatti dell'associazione che ha preso in carico il mio viaggio, per poterla contattare in caso di necessità.                        | 5.3           |
| CG-32 | Come caregiver voglio poter avere uno storico delle visite fatte.                                                                                                          | 5.4           |
| CG-33 | Come caregiver voglio poter registrare un trasporto ricorrente (es. dialisi ogni martedì), così da non dover ripetere la richiesta ogni volta. Rinviata alla v1.           | 1.1           |
| CG-38 | Come caregiver voglio poter indicare per il ritorno una destinazione diversa da quella da cui sono partito.                                                               | 1.7           |
| CG-39 | Come caregiver voglio poter aggiungere un assistito di cui mi prendo cura.                                                                                                 | 9.1           |
| CG-40 | Come caregiver voglio poter visualizzare l'elenco degli assistiti di cui mi occupo.                                                                                        | 9.2           |
| CG-41 | Come caregiver voglio poter visualizzare i dati e i viaggi di un assistito che seguo.                                                                                      | 9.3           |
| CG-42 | Come caregiver voglio poter modificare i dati di un assistito che seguo, dato che non può farlo da solo.                                                                  | 9.4           |
| CG-43 | Come caregiver voglio poter collegare direttamente un altro caregiver a un assistito che già seguo, così che possa vederne dati e viaggi.                                  | 9.5           |
| CG-44 | Come caregiver voglio poter rimuovere un collegamento caregiver↔assistito, il mio o quello di un altro caregiver collegato.                                               | 9.6           |

## 3.2 Stories associazione

| ID    | User story                                                                                                                                                                   | UC       |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- |
| AS-01 | Come associazione voglio potermi registrare in GoCare.                                                                                                                       | 10.1     |
| AS-02 | Come associazione voglio poter effettuare il login.                                                                                                                          | 10.2     |
| AS-03 | Come associazione voglio poter recuperare la password in caso di smarrimento.                                                                                                | 10.3     |
| AS-04 | Come associazione voglio poter eliminare il mio account.                                                                                                                     | 10.4     |
| AS-05 | Come associazione voglio poter visualizzare e modificare i dati, i contatti e le province coperte dalla mia associazione.                                                    | 8.5, 8.6 |
| AS-06 | Come associazione voglio ricevere notifica quando un utente invia una richiesta di trasporto.                                                                                | 4.7      |
| AS-07 | Come associazione voglio poter vedere tutte le richieste di trasporto pendenti a me destinate nella mia dashboard, con filtri (data, orario, destinazione, tipo di viaggio). | 6.1, 6.2 |
| AS-08 | Come associazione voglio poter accettare una richiesta di trasporto pendente.                                                                                                | 6.3      |
| AS-09 | Come associazione voglio poter rifiutare/declinare una richiesta di trasporto pendente.                                                                                      | 6.4      |
| AS-10 | Come associazione voglio poter contattare la persona che ha prenotato un trasporto (contatto visibile). Trattamento di dati sensibili, vedi Punti aperti (PA-04).            | 6.5      |
| AS-11 | Come associazione voglio poter vedere tutti i trasporti in carico nella mia dashboard.                                                                                       | 7.1      |
| AS-12 | Come associazione voglio poter vedere uno storico dei trasporti effettuati.                                                                                                  | 7.2      |
| AS-17 | Come associazione voglio poter disdire un trasporto specificando la causa.                                                                                                   | 3.2, 3.3 |
| AS-18 | Come associazione voglio ricevere notifica quando un trasporto viene annullato dall'utente.                                                                                  | 4.9      |
| AS-19 | Come operatore dell'associazione voglio poter aggiornare lo stato del viaggio (preso in carico, in arrivo, in visita, in ritorno, concluso).                                 | 4.10     |

## 3.3 Stories amministratore GoCare

| ID    | User story                                                                                                                                                     | UC   |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---- |
| AD-01 | Come amministratore GoCare voglio poter accreditare o rifiutare un'associazione registrata, così che solo gli enti verificati possano operare sulle richieste. | 10.5 |

## 3.4 Matrice di copertura Use Case → User Stories

Verifica che ogni sotto-voce di Use Case abbia almeno una user story associata.

| Use Case  | User stories        | Esito    |
| --------- | -------------------- | -------- |
| 1.1       | CG-14, CG-33         | Coperto |
| 1.2       | CG-15                | Coperto |
| 1.3       | CG-17                | Coperto |
| 1.4       | CG-19                | Coperto |
| 1.5       | CG-16                | Coperto |
| 1.6       | CG-14                | Coperto |
| 1.7       | CG-38                | Coperto |
| 3.1       | CG-23                | Coperto |
| 3.2       | AS-17                | Coperto |
| 3.3       | CG-24, AS-17         | Coperto |
| 4.1       | CG-29                | Coperto |
| 4.2       | CG-29                | Coperto |
| 4.3       | CG-28                | Coperto |
| 4.4       | CG-25, CG-26         | Coperto |
| 4.6       | CG-27                | Coperto |
| 4.7       | AS-06                | Coperto |
| 4.9       | AS-18                | Coperto |
| 4.10      | AS-19                | Coperto |
| 5.1       | CG-32                | Coperto |
| 5.2       | CG-30                | Coperto |
| 5.3       | CG-31                | Coperto |
| 5.4       | CG-32                | Coperto |
| 5.5       | CG-29                | Coperto |
| 6.1       | AS-07                | Coperto |
| 6.2       | AS-07                | Coperto |
| 6.3       | AS-08                | Coperto |
| 6.4       | AS-09                | Coperto |
| 6.5       | AS-10                | Coperto |
| 7.1       | AS-11                | Coperto |
| 7.2       | AS-12                | Coperto |
| 8.1 / 8.2 / 8.3 | CG-03          | Coperto |
| 8.4       | CG-18                | Coperto |
| 8.5 / 8.6 | AS-05                | Coperto |
| 9.1       | CG-39                | Coperto |
| 9.2       | CG-40                | Coperto |
| 9.3       | CG-41                | Coperto |
| 9.4       | CG-42                | Coperto |
| 9.5       | CG-43                | Coperto |
| 9.6       | CG-44                | Coperto |
| 10.1      | CG-01, AS-01         | Coperto |
| 10.2      | CG-02, AS-02         | Coperto |
| 10.3      | CG-04, AS-03         | Coperto |
| 10.4      | CG-05, AS-04         | Coperto |
| 10.5      | AD-01                | Coperto |

---

# 4. Attori

| Attore                      | Descrizione                                           | Ruolo nel sistema                                                                                                                                                                                                                                     |
| ---------------------------- | ----------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Assistito                   | Persona fragile destinataria del trasporto.           | Non ha un account e non interagisce mai con l'app: è un'anagrafica creata e gestita da uno o più caregiver. Può essere collegato a più caregiver contemporaneamente (relazione diretta, non mediata da un gruppo); ogni caregiver collegato vede tutti i suoi dati e tutti i suoi viaggi.                          |
| Caregiver                   | Familiare o assistente che gestisce l'assistito.      | Si registra come account autonomo, aggiunge e gestisce uno o più assistiti, richiede trasporti per loro conto, segue lo stato dei viaggi. Può essere collegato a più assistiti; più caregiver possono essere collegati allo stesso assistito.                                                                     |
| Associazione                | Ente di volontariato/soccorso che eroga il trasporto. | Riceve le richieste a sé destinate, accetta o rifiuta, gestisce i trasporti presi in carico e il loro stato. Opera solo se accreditata.                                                                                                             |
| Operatore dell'associazione | Volontario che esegue materialmente il trasporto.     | Aggiorna lo stato del viaggio durante l'esecuzione tramite un link scoped al singolo trasporto, generato e inoltrato dall'associazione: non ha un account proprio e non fa login. Viene registrato come etichetta testuale sulla transizione di stato. |
| Amministratore GoCare       | Chi gestisce la piattaforma.                          | Accredita o rifiuta le associazioni registrate prima che possano operare sulle richieste (PA-05).                                                                                                                                                   |
| GoCare (sistema)            | La piattaforma.                                       | Instrada le richieste alle associazioni competenti per provincia, genera notifiche push ed e-mail, conserva lo storico.                                                                                                                             |

---

# 5. Tipi di viaggio

| Tipo di viaggio                        | Direzione tipica                    | Scope            |
| --------------------------------------- | ------------------------------------ | ----------------- |
| Visita medica                          | Andata e ritorno                    | In scope v0       |
| Ricovero                               | Solo andata                         | In scope v0       |
| Dimissione                             | Solo ritorno                        | In scope v0       |
| Trasferimento                          | Solo andata (struttura → struttura) | In scope v0       |
| Trasporto sociale (spesa, posta, ecc.) | Variabile                           | Rinviato alla v1  |

Il tipo di viaggio determina il valore di default della direzione nel form dell'UC 1 (1.2 e 1.5), che resta comunque modificabile dall'utente. Il trasporto sociale è rinviato alla v1 (vedi Punti aperti).

---

# 6. Decisioni consolidate

- Dashboard interna per le associazioni presente e obbligatoria in V.0.
- Le disponibilità delle associazioni non sono gestite dalla piattaforma: le associazioni accettano le richieste secondo la propria disponibilità.
- Flusso API bidirezionale in V.1: tutte le chiamate sono in scrittura e ogni chiamata corrisponde a un cambio di stato.
- Colonne Trello: To do, Ready for development, In progress (una sola card alla volta), Ready for testing, In testing, Tested, Released.
- Il calcolo dei km previsti è rinviato alla v1: la richiesta non porta un campo km e le dashboard dell'associazione non hanno filtri né colonne per i km.
- Solo i caregiver si registrano come account autonomi. L'assistito non ha un account: è un'anagrafica creata da un caregiver, senza login e senza possibilità di interazione propria. Il collegamento caregiver↔assistito è diretto e molti-a-molti (un caregiver può seguire più assistiti, un assistito può avere più caregiver), senza nessun concetto di "gruppo cura" a mediarlo. Aggiungere un assistito crea il collegamento subito attivo, senza consenso da richiedere (l'assistito non può darlo); collegare un **secondo caregiver** a un assistito già esistente richiede invece l'accettazione del caregiver invitato, perché a differenza dell'assistito è un utente con un proprio account. Un caregiver con un collegamento attivo verso un assistito vede tutti i suoi dati e tutti i suoi viaggi; se più caregiver sono collegati allo stesso assistito, vedono tutti tutto, senza distinzione fra chi lo ha aggiunto e chi può solo vedere. La registrazione distingue solo fra account caregiver e account associazione.
- Il viaggio non è modificabile dopo la creazione (niente più UC "Modifica trasporto"): l'unica azione disponibile su un viaggio già registrato è l'annullamento. Se le condizioni del viaggio cambiano, il caregiver annulla e invia una nuova richiesta.
- Il rifiuto di una singola associazione non è uno stato della richiesta: la richiesta resta "in attesa" e visibile alle altre associazioni destinatarie. L'esito negativo aggregato è sempre "non coperta".
- Ogni evento genera una sola notifica, con i canali (push, e-mail) combinati sulla stessa voce: il centro notifiche non mostra righe duplicate per lo stesso evento.
- Gli indirizzi sono strutturati (via, civico, CAP, città, provincia) e vengono copiati sulla richiesta al momento della creazione: modificare o eliminare una destinazione salvata non altera i viaggi già registrati.
- L'instradamento delle richieste avviene per provincia: ogni associazione dichiara le province coperte e riceve solo le richieste con partenza in una di esse.
- Autenticazione e dominio vivono nello stesso database; l'account e il profilo caregiver sono due entità distinte che **condividono lo stesso identificatore**, con una foreign key reale fra loro (`Person.Id`/`Association.Id` → `Account.Id`, `OnDelete(Cascade)`). L'assistito non ha invece un account e non ha identificatore condiviso, perché non fa mai login. In v0 ogni caregiver di dominio ha esattamente un account: non si opera con caregiver non registrati.

---

# 7. Stack tecnologico

- Backend: C# / .NET 10, ASP.NET Core Web API con controller MVC
- Frontend: React Native
- Database: PostgreSQL, un unico database (`gocare`), accesso via EF Core + Npgsql
- Struttura della soluzione: un unico progetto (`GoCare`), non più tre progetti separati. Organizzato per cartelle a livello tecnico anziché per feature: `Controllers/`, `Dtos/`, `Models/`, `Services/`, `Data/`, più `Abstractions/`, `Errors/`, `Pagination/`, `Validation/` per il kernel trasversale. Dentro `Models/`, `Services/` e `Dtos/` resta la separazione fra area autenticazione e area dominio, come sottocartelle (`Auth/`, `Domain/`). Le interfacce di servizio sono state mantenute solo dove esiste più di un'implementazione o è pianificato uno swap concreto (es. l'invio e-mail); dove c'era un'unica implementazione senza swap previsto (servizi applicativi, orologio di sistema, utente corrente) sono state rimosse a favore della classe concreta.
- Notifiche: push applicative + e-mail di conferma

---

# 8. Perimetro versione BETA e suddivisione del lavoro

Versione BETA:

- Dashboard con login per i due ruoli con account (caregiver, associazioni). L'assistito non ha login: le sue schermate sono gestite dal caregiver che lo segue.
- Calendario e interfaccia di prenotazione.

Suddivisione del lavoro (ad alto livello):

- Stefano: grafica React Native
- Giorgia: grafica React Native
- Francesco: logica C# – area di dominio (richieste di trasporto, assistiti, notifiche)
- Elio: logica C# – area di autenticazione (account, token, sessioni)
- Tutti: database (Francesco ed Elio potranno modificarlo per la logica)

Note dalla riunione Discord del 03/08/26:

- Gestione dei guasti da parte dell'associazione (aperto – vedi Punti aperti).
- Slot viaggi e conferma post-prenotazione tramite e-mail (chiuso – vedi Punti aperti).
- UX/UI: facilità per l'utente di capire quali giorni sono disponibili e quali no.
- Definiti i prossimi due giorni di lavoro (martedì 17:00 / mercoledì 13:00).

---

# 9. SPRINT – dettaglio Frontend / Backend per Use Case

Ogni voce riporta la user story di riferimento (ID della sezione 3) e il breakdown FRONTEND/BACKEND. L'identificativo UC 2 (Modifica trasporto) non è usato in questo elenco, coerentemente con l'assenza della funzione in v0.

## UC 1 – RICHIEDI TRASPORTO

**1.1 Come caregiver voglio poter registrare il trasporto di un assistito. (CG-14)**

FRONTEND:

- Pagina con form di aggiunta di un nuovo trasporto.
- Campi: per quale assistito, data, orario, indirizzo di partenza, destinazione, tipo di viaggio, direzione, accompagnatori, contatti.
- Selezione con tendina delle destinazioni già salvate, in alternativa bottone di aggiunta con comparsa del form di aggiunta destinazione.
- Validazione lato client dei campi obbligatori e della data (non nel passato).
- Pop-up di conferma invio: "La tua richiesta è stata inviata alle associazioni. Riceverai una notifica appena verrà presa in carico."

BACKEND:

- Validazione dati e form.
- Creazione della richiesta in stato "in attesa", con doppio riferimento a richiedente (il caregiver) e beneficiario (l'assistito).
- Copia sulla richiesta degli indirizzi (via, civico, CAP, città, provincia) e dei contatti di riferimento: sono uno scatto al momento della richiesta e non seguono le modifiche successive dell'anagrafica.
- Congelamento della lista delle associazioni destinatarie: al momento della creazione vengono selezionate le associazioni accreditate che coprono la provincia di partenza, e l'elenco resta immutabile per tutta la vita della richiesta.
- Invio della notifica di nuova richiesta alle associazioni destinatarie (UC 4.7).

**1.2 Come caregiver voglio poter registrare il prossimo trasporto del mio assistito. (CG-14)**

FRONTEND:

- Selezione con tendina dell'assistito ricevente del trasporto, tra quelli collegati al caregiver.
- La tendina è visibile solo se il caregiver ha almeno un assistito collegato.
- Precompilazione dei dati di contatto e dell'indirizzo abituale dell'assistito selezionato.

BACKEND:

- Verifica che il richiedente abbia un collegamento attivo con l'assistito indicato (autorizzazione).
- Salvataggio della richiesta con doppio riferimento: richiedente (caregiver) e beneficiario (assistito) — alimenta l'UC 9.3.

**1.3 Come caregiver voglio poter scegliere la direzione del viaggio. (CG-15)**

FRONTEND:

- Selettore a tre opzioni: andata e ritorno / solo andata / solo ritorno.
- In caso di "andata e ritorno" compare il campo orario di ritorno.

BACKEND:

- Validazione della coerenza fra direzione, orari e indirizzi: un "andata e ritorno" richiede obbligatoriamente l'orario di ritorno; le altre due direzioni non possono avere né orario né indirizzo di ritorno; l'orario di ritorno deve essere successivo a quello di partenza. La regola vale alla creazione (non ci sono più modifiche successive: il viaggio non è modificabile una volta registrato).
- Persistenza della direzione sulla richiesta: determina gli stati attraversati dal viaggio (UC 4.1).

**1.4 Come caregiver voglio poter indicare il tipo di viaggio. (CG-16)**

FRONTEND:

- Tendina con i tipi di viaggio previsti (visita medica, ricovero, dimissione, trasferimento).
- La selezione imposta il valore di default della direzione, che resta modificabile in fase di creazione.

BACKEND:

- Validazione del valore rispetto alla lista dei tipi ammessi.
- Salvataggio del tipo sulla richiesta (dato visibile all'associazione nell'UC 6.2).

**1.5 Come caregiver voglio poter indicare un accompagnatore per il viaggio. (CG-19)**

FRONTEND:

- Form di inserimento dei dati generali dell'accompagnatore (nome, cognome, parentela, contatto).
- Possibilità di indicare più di un accompagnatore o nessuno.

BACKEND:

- Validazione dati e form.
- Salvataggio degli accompagnatori collegati alla richiesta, al momento della creazione. L'accompagnatore è un dato immutabile dopo l'invio della richiesta: non esiste più un flusso di modifica successivo (era l'ex UC 2.2).

**1.6 Come caregiver voglio poter scegliere una destinazione diversa dal domicilio abituale dell'assistito. (CG-17)**

FRONTEND:

- Form di aggiunta nuova destinazione, con bottone "aggiungi destinazione".
- Comparsa del pop-up "Vuoi aggiungere questa destinazione alla tua lista destinazioni? SI/NO".

BACKEND:

- Validazione dati e form.
- Se l'utente conferma, salvataggio della destinazione nella lista personale (UC 8.4).

**1.7 Come caregiver voglio poter registrare un trasporto ricorrente. (CG-33)** _[RINVIATA a v1 – vedi Punti aperti]_

La raccomandazione è escluderla dalla v0: la ricorrenza implica generazione automatica di richieste, gestione delle eccezioni sulle singole occorrenze e annullamento in blocco, con un impatto sul modello dati sproporzionato rispetto al valore in fase di validazione del prodotto.

**1.8 Come caregiver voglio poter indicare per il ritorno una destinazione diversa da quella da cui sono partito. (CG-38)**

FRONTEND:

- Nel form di nuovo trasporto, solo con direzione "andata e ritorno", checkbox "il ritorno è verso un indirizzo diverso".
- Se selezionata, compare un terzo campo indirizzo, con la stessa tendina delle destinazioni salvate.
- Se non selezionata, il ritorno è verso l'indirizzo di partenza dell'andata.

BACKEND:

- Terzo indirizzo facoltativo sulla richiesta, valorizzabile solo con direzione "andata e ritorno"; se assente, il ritorno si intende verso l'indirizzo di partenza.
- Stessa validazione di coerenza direzione/indirizzi della 1.3.
- L'indirizzo di ritorno è visibile all'associazione nel dettaglio della richiesta (UC 6.2). Non è più un campo modificabile dopo l'invio: la richiesta di modifica (ex UC 2.1) non esiste più.

## UC 3 – ANNULLA TRASPORTO

**3.1 Come caregiver voglio poter annullare il viaggio, indicando il motivo. (CG-23, CG-24)**

FRONTEND:

- Bottone di annullamento del viaggio.
- Pop-up di conferma dell'annullamento: "Sei sicuro di voler eliminare questo trasporto? Una volta eliminato si dovrà effettuare una nuova richiesta per richiedere un nuovo trasporto."
- Tendina con le causali più frequenti + campo di testo libero opzionale.

BACKEND:

- Soft delete dal database (il viaggio resta nello storico con stato "annullato"), con registrazione di chi ha annullato e quando.
- Salvataggio della causale.
- Notifica all'associazione (UC 4.10), se il viaggio era già stato preso in carico.

**3.2 Come associazione voglio poter disdire un trasporto specificando la causa. (AS-17)**

FRONTEND:

- Apertura del form con area di testo per scrivere il motivo, o tendina a scomparsa per sceglierlo.
- Pop-up di conferma.

BACKEND:

- Validazione del form (causale obbligatoria).
- Registrazione della disdetta come rifiuto post-accettazione di quella associazione, distinto dal semplice declino di una richiesta pendente: per la disdetta la causale è obbligatoria, per il declino è facoltativa.
- Il viaggio torna in stato "in attesa" e rientra nella dashboard delle richieste pendenti delle altre associazioni destinatarie, così da consentire una copertura alternativa.
- Invio della notifica all'utente con il testo della causale (UC 4.6).

_[DA VALIDARE] Il rientro automatico del viaggio fra le richieste pendenti è la risposta al problema n.3 dichiarato in apertura del documento ("deve essere trovata una soluzione per assicurare un'alternativa"). Va confermato che sia il comportamento desiderato e con quale anticipo minimo rispetto alla data del viaggio._

## UC 4 – STATO E NOTIFICHE

**4.1 Come caregiver voglio sapere se il viaggio non può essere coperto. (CG-25)**

FRONTEND:

- Pop-up con messaggio di esito negativo e mancanza di soluzioni.
- Badge "non coperto" sulla card del viaggio in "I miei viaggi".

BACKEND:

- Notifica ed e-mail con esito negativo.
- Regola di scatto dell'esito negativo: nessuna associazione destinataria ha accettato entro la soglia definita (vedi Punti aperti), valutata da un job schedulato che confronta i rifiuti registrati con la lista congelata delle associazioni destinatarie.
- Il passaggio a "non coperta" registra la data dell'esito.

**4.2 Come caregiver voglio ricevere notifica quando il viaggio è preso in carico. (CG-26)**

FRONTEND:

- Apertura del pop-up con avviso di conferma del viaggio al login, con link alla scheda del viaggio.
- Aggiornamento del badge di stato sulla card.

BACKEND:

- Notifica ed e-mail di conferma, con nome e contatti dell'associazione.

**4.3 Come caregiver voglio essere avvisato se un viaggio già preso in carico viene annullato dall'associazione. (CG-27)**

FRONTEND:

- Pop-up al login con avviso e testo del motivo della disdetta.
- Bottone "cerca un'alternativa" che riporta il viaggio in stato di richiesta.

BACKEND:

- Notifica ed e-mail.
- Registrazione della causale nello storico del viaggio.

**4.4 Come caregiver voglio sapere l'esito della mia richiesta. (CG-25, CG-26)**

FRONTEND:

- Badge sulla card del viaggio: "in attesa / confermata / non coperta".
- Pop-up di notifica all'esito.

BACKEND:

- Notifica ed e-mail all'esito della valutazione delle associazioni destinatarie.

**4.6 Come caregiver voglio poter vedere lo stato in tempo reale del viaggio dell'assistito. (CG-29)**

FRONTEND:

- Stato mostrato nella card del viaggio, nella pagina dell'assistito e nella pagina "I miei viaggi".
- Timeline degli stati attraversati con l'orario di ciascun passaggio.
- Pagina/schermata in cui l'operatore gestisce lo stato dell'assistito.

BACKEND:

- Persistenza dei cambi di stato in un registro append-only: ogni riga porta stato, timestamp, associazione che l'ha effettuato ed etichetta testuale dell'operatore. Lo stato corrente è la riga più recente.
- Cambio di stato gestito dall'operatore dall'account dell'associazione (modello di accesso da confermare: vedi Punti aperti).
- Ogni cambio di stato genera la notifica dell'UC 4.2.

_[DA VALIDARE] La visualizzazione della posizione GPS del mezzo è esclusa dalla v0: richiede tracciamento continuo, consenso esplicito dell'operatore e trattamento di dati di geolocalizzazione. Lo stato testuale copre l'esigenza dichiarata nel problema n.2 (evitare le ripetute chiamate all'associazione)._

**4.7 Come caregiver voglio ricevere notifica a ogni cambio di stato del viaggio. (CG-29)**

FRONTEND:

- Notifica push con il nuovo stato, es. "Il tuo assistito è stato preso in carico", "Viaggio concluso".

BACKEND:

- Invio della notifica push a tutti i caregiver collegati all'assistito interessato dal viaggio.
- Nessuna e-mail per i cambi di stato intermedi, per non generare rumore.
- L'invio push richiede che il dispositivo sia registrato: ogni caregiver e ogni associazione registra i propri token push, con la piattaforma di provenienza (iOS, Android, Web); i token possono essere disattivati al logout o alla disinstallazione.

**4.8 Come caregiver voglio ricevere notifica dei prossimi trasporti programmati. (CG-28)**

FRONTEND:

- Pop-up: "Ricordati il trasporto di domani alle 17:25 da X a Y".

BACKEND:

- Notifica ed e-mail il giorno precedente il viaggio.
- Job schedulato giornaliero che seleziona i viaggi confermati con data uguale a domani.

**4.9 Come associazione voglio ricevere notifica se un utente invia una richiesta di trasporto. (AS-06)**

FRONTEND:

- Notifica "Ci sono nuove richieste, entra nell'app per visualizzarle".
- Badge con contatore sulla voce di header "Richieste".

BACKEND:

- Notifica ed e-mail a tutte le associazioni destinatarie della richiesta, cioè quelle congelate nella lista dei candidati alla creazione.

**4.10 Come associazione voglio ricevere notifica se un trasporto viene annullato dall'utente. (AS-18)**

FRONTEND:

- Pop-up al login: "Il viaggio del [data e ora] è stato disdetto."
- Bottone "cerca un nuovo viaggio" che porta alla pagina delle richieste di trasporto.

BACKEND:

- Notifica ed e-mail.
- Rimozione del viaggio dalla lista dei trasporti accettati e spostamento nello storico con stato "annullato".

**4.11 Come operatore dell'associazione voglio poter aggiornare lo stato del viaggio. (AS-19)**

FRONTEND (associazione):

- Nella pagina del trasporto accettato, bottone "genera link operatore".
- Il link generato è mostrato con un bottone "copia" per inoltrarlo all'operatore con il canale che preferisce (SMS, WhatsApp, ecc.) — GoCare non lo spedisce automaticamente.
- Possibilità di rigenerare il link in qualunque momento (es. se il primo va perso); il precedente resta comunque valido finché non scade.

FRONTEND (operatore, nessun login):

- Aprendo il link, schermata minima e isolata dal resto dell'app: solo i pulsanti di avanzamento di stato, uno per volta e in sequenza: preso in carico → in arrivo → in visita → in ritorno → concluso, più "sospeso" disponibile durante l'esecuzione (guasto/imprevisto).
- Il pulsante mostra sempre e solo lo stato successivo (o "sospeso"), per ridurre gli errori di inserimento.
- Campo con il nome dell'operatore, da compilare al primo utilizzo del link.
- Pop-up di conferma sul passaggio a "concluso", che chiude il viaggio e rende il link non più utilizzabile.

BACKEND:

- Il link (`TripOperatorLink`) è scoped a un solo trasporto, con scadenza calcolata automaticamente su `DepartureDateHour + 24 ore`; nessun account individuale per l'operatore (sostituisce sia l'ipotesi "dall'account associazione" sia quella "account per operatore").
- Ogni richiesta di cambio stato è autenticata tramite il token del link, non tramite login.
- Validazione della transizione rispetto alla macchina a stati (UC 4.1) e alla direzione del viaggio: un viaggio "solo andata" non attraversa lo stato "in ritorno", un viaggio "solo ritorno" non attraversa lo stato "in visita"; "sospeso" è raggiungibile da qualunque stato di esecuzione attivo e da esso si riprende dallo stato in cui ci si era fermati.
- Registrazione di stato, timestamp, associazione ed etichetta dell'operatore in una nuova riga del registro delle transizioni; nessuna riga viene mai modificata o cancellata.
- Allineamento dello stato della richiesta: alla prima transizione la richiesta passa in "in esecuzione", con il passaggio a "concluso" passa in "conclusa".
- "Sospeso" genera notifica push immediata ai caregiver collegati più e-mail (trattato come evento di esito, data la criticità); gli altri cambi di stato solo push (UC 4.7).

## UC 5 – VISUALIZZAZIONE "I MIEI VIAGGI"

**5.1 Come caregiver voglio vedere l'elenco dei miei viaggi. (CG-30, CG-32)**

FRONTEND:

- Bottone "aggiungi" per aprire il form di aggiunta viaggio (rimanda all'UC 1).
- Lista di tutti i viaggi in forma di card.
- Contenuto della card: data e ora, associazione, contatto dell'associazione, indirizzo di partenza, indirizzo di destinazione, eventuale indirizzo di ritorno, accompagnatore, badge di stato.
- Separazione fra "prossimi viaggi" e "storico".

BACKEND:

- Lettura dei viaggi richiesti dal caregiver, relativi a tutti gli assistiti a lui collegati.
- Ordinamento per data crescente sui prossimi viaggi, decrescente sullo storico.
- Paginazione dello storico.

_[DA VALIDARE] Va deciso se esporre il contatto generico dell'associazione e, durante il viaggio, quello dell'operatore, oppure solo quello generico. Ripreso nella sezione Punti aperti._

**5.2 Come caregiver voglio vedere i contatti dell'associazione che ha preso in carico il mio viaggio. (CG-31)**

FRONTEND:

- Contatto riportato nella card del viaggio.
- Contatti completi nella pagina dell'associazione, raggiungibile dalla card.
- I contatti compaiono solo dopo la presa in carico.

BACKEND:

- Esposizione dei dati di contatto dell'associazione assegnataria: telefoni, e-mail e orari di reperibilità.
- Nessun contatto esposto per i viaggi in stato "in attesa".

**5.3 Come caregiver voglio poter avere uno storico delle visite fatte. (CG-32)**

FRONTEND:

- Pagina/sezione "Storico viaggi".
- Lista di card in sola lettura, con stato finale (concluso / annullato / non coperto).
- Filtro per assistito e per periodo.

BACKEND:

- Query sui viaggi in stato terminale, con paginazione.
- I viaggi soft-deleted restano visibili nello storico con stato "annullato".

**5.4 Come caregiver voglio vedere lo stato di avanzamento del viaggio in corso. (CG-29)**

FRONTEND:

- Card del viaggio in corso in evidenza in cima alla lista, con la timeline degli stati.
- Aggiornamento della card alla ricezione della notifica di cambio stato.

BACKEND:

- Esposizione dello stato corrente e della cronologia dei passaggi, letta dal registro delle transizioni ordinato per timestamp.
- Lettura in polling alla riapertura della schermata (il push copre l'aggiornamento immediato).

## UC 6 – RICHIESTE PENDENTI (associazione)

**6.1 Come associazione voglio vedere tutte le richieste pendenti nella mia dashboard, con filtri. (AS-07)**

FRONTEND:

- Dashboard con lista delle richieste in stato "in attesa", in forma di card o righe di tabella.
- Barra filtri: data, fascia oraria, destinazione, tipo di viaggio.
- Ordinamento per data del viaggio.
- Stato vuoto: "Non ci sono richieste al momento".

BACKEND:

- Query delle richieste in stato "in attesa" per cui l'associazione compare fra i candidati congelati alla creazione e non ha già rifiutato.
- Applicazione dei filtri lato server e paginazione.
- Nessun dato esposto alle associazioni non destinatarie.

**6.2 Come associazione voglio vedere il dettaglio di una richiesta. (AS-07)**

FRONTEND:

- Pagina o pannello di dettaglio: dati dell'assistito, data e ora, indirizzo di partenza, destinazione, eventuale indirizzo di ritorno, tipo di viaggio, direzione, accompagnatori, note.
- Bottoni "accetta" e "rifiuta" in fondo al dettaglio.

BACKEND:

- Lettura completa della richiesta.
- Verifica che la richiesta sia ancora disponibile prima di mostrare le azioni.

**6.3 Come associazione voglio poter accettare una richiesta di trasporto pendente. (AS-08)**

FRONTEND:

- Bottone "accetta" con pop-up di conferma.
- Al termine, la richiesta esce dalla dashboard delle pendenti e compare fra i trasporti accettati.
- Messaggio di errore dedicato se la richiesta è stata nel frattempo presa da un'altra associazione.

BACKEND:

- Passaggio di stato della richiesta da "in attesa" a "confermata", con assegnazione all'associazione e registrazione della data di accettazione. Il passaggio è ammesso solo da "in attesa".
- Controllo di concorrenza ottimistico sulla richiesta: la prima accettazione vince, le successive ricevono un errore gestito.
- Notifica di presa in carico all'utente (UC 4.2).

_[DA VALIDARE] Il modello "prima accettazione vince" discende dalla decisione consolidata che le disponibilità non sono gestite dalla piattaforma. Va confermato il criterio con cui si compone l'area di competenza: in v0 la richiesta è visibile alle sole associazioni che coprono la provincia di partenza._

**6.4 Come associazione voglio poter rifiutare o declinare una richiesta pendente. (AS-09)**

FRONTEND:

- Bottone "rifiuta" con pop-up di conferma e causale facoltativa.
- La richiesta scompare dalla dashboard dell'associazione che ha rifiutato.

BACKEND:

- Registrazione del rifiuto per quella associazione, come declino pre-accettazione con causale facoltativa: la richiesta resta "in attesa" e visibile alle altre destinatarie.
- Se tutte le associazioni destinatarie hanno rifiutato, o è scaduta la soglia temporale, la richiesta passa in stato "non coperta" e scatta la notifica dell'UC 4.1.

**6.5 Come associazione voglio poter contattare la persona che ha prenotato un trasporto. (AS-10)**

FRONTEND:

- Contatto telefonico visibile nel dettaglio della richiesta, con azione "chiama".
- Contatto dell'accompagnatore, se indicato.

BACKEND:

- Esposizione dei dati di contatto solo alle associazioni destinatarie della richiesta.
- Tracciamento degli accessi ai dati di contatto, distinguendo i recapiti del richiedente da quelli del beneficiario: ogni accesso registra richiesta, associazione, tipo di dato e istante.

_[DA VALIDARE] Il recapito della persona che ha prenotato è un dato sensibile. Raccomandazione: mostrare il recapito completo solo dopo l'accettazione, e limitarsi al nome e alla fascia oraria nella fase di valutazione della richiesta. Punto ripreso in "Cosa manca / Punti aperti"._

## UC 7 – TRASPORTI ACCETTATI (associazione)

**7.1 Come associazione voglio vedere tutti i trasporti in carico nella mia dashboard. (AS-11)**

FRONTEND:

- Lista dei trasporti accettati e non ancora conclusi, in forma di card.
- Contenuto della card: data e ora, assistito, contatti, partenza, destinazione, eventuale indirizzo di ritorno, accompagnatori, stato corrente.
- Filtri per data e per stato; ordinamento per data crescente.
- Accesso alla schermata di aggiornamento stato (UC 4.11) dalla card.

BACKEND:

- Query dei viaggi assegnati all'associazione in stato "confermato" o "in esecuzione".
- Esposizione dello stato corrente.

**7.3 Come associazione voglio poter vedere uno storico dei trasporti effettuati. (AS-12)**

FRONTEND:

- Sezione "Storico" con lista in sola lettura dei viaggi conclusi, annullati e rifiutati.
- Filtri per periodo e per esito.
- Riepilogo numerico dei viaggi conclusi nel periodo selezionato.

BACKEND:

- Query sui viaggi in stato terminale assegnati all'associazione, con paginazione.
- Conservazione dei dati anche dopo il soft delete del viaggio.

## UC 8 – GESTIONE INFORMAZIONI PROFILO

**8.1 Come caregiver voglio visualizzare le mie informazioni personali. (CG-03)**

FRONTEND:

- Pagina "Profilo" con i dati personali in sola lettura: nome, cognome, data di nascita, indirizzo di domicilio, telefono, e-mail.
- Bottone "modifica" che attiva la modalità di editing.
- Sezione separata con i dati di accesso (e-mail di login) e l'elenco degli assistiti collegati.

BACKEND:

- Lettura dei dati dell'utente autenticato.
- L'indirizzo di domicilio è facoltativo e strutturato in via, civico, CAP, città e provincia.
- Nessun dato di altri utenti esposto su questo endpoint.

**8.2 Come caregiver voglio modificare le mie informazioni personali e di contatto. (CG-03)**

FRONTEND:

- Form di modifica precompilato, con i campi non modificabili disabilitati.
- Validazione lato client di telefono ed e-mail.
- Pop-up di conferma "Vuoi salvare le modifiche? SI/NO" e messaggio di esito.
- Se viene modificata l'e-mail, avviso che sarà necessaria una nuova verifica.

BACKEND:

- Validazione dei dati e del form.
- Aggiornamento dell'anagrafica utente.
- Se cambia l'e-mail di login: invio dell'e-mail di verifica al nuovo indirizzo e conferma del cambio solo dopo la validazione. L'e-mail di accesso vive nell'area autenticazione, l'e-mail di contatto nel profilo di dominio: le due modifiche sono distinte.
- I viaggi già registrati mantengono i contatti indicati al momento della richiesta.

_[DA VALIDARE] Se un contatto cambia mentre un viaggio è in corso, va deciso se l'associazione debba vedere il contatto aggiornato o quello registrato con la richiesta._

**8.3 Come caregiver voglio gestire le mie destinazioni salvate. (CG-18)**

FRONTEND:

- Sezione della pagina profilo (o pagina dedicata) "Le mie destinazioni".
- Lista di tutte le destinazioni salvate, con bottoni di modifica ed eliminazione accanto a ciascuna.
- Form di aggiunta/modifica: etichetta (es. "Ospedale Sant'Anna"), indirizzo completo, note.
- Pop-up di conferma per l'eliminazione.
- Le destinazioni salvate alimentano la tendina del form di nuovo trasporto (UC 1.6).

BACKEND:

- CRUD delle destinazioni collegate al caregiver.
- Validazione dell'indirizzo (via, civico, CAP, città, provincia) e dei campi obbligatori.
- L'eliminazione di una destinazione non modifica i viaggi già registrati che la utilizzano (l'indirizzo è copiato sulla richiesta).

**8.4 Come associazione voglio visualizzare e modificare le informazioni della mia associazione. (AS-05)**

FRONTEND:

- Pagina informazioni associazione in sola lettura: denominazione, sede, province coperte, recapiti telefonici, e-mail, orari di reperibilità, stato di accreditamento.
- Pagina di modifica delle informazioni, con form precompilato e pop-up di conferma.
- Le province coperte si gestiscono come elenco, con aggiunta e rimozione delle singole voci.
- Anteprima di come i contatti vengono visti dagli utenti (UC 5.2).

BACKEND:

- Lettura e aggiornamento dei dati dell'associazione autenticata: denominazione, sede (indirizzo strutturato, obbligatoria), elenco dei telefoni, una e-mail, orari di reperibilità facoltativi, elenco delle province coperte.
- Validazione dei recapiti (almeno un telefono e una e-mail obbligatori, perché esposti agli utenti) e di almeno una provincia coperta, perché da essa dipende l'instradamento delle richieste.
- La modifica dei contatti si riflette immediatamente sui viaggi in carico.
- La modifica delle province coperte vale per le richieste future: le liste di destinatari già congelate non vengono ricalcolate.

## UC 9 – GESTIONE ASSISTITI

**9.1 Come caregiver voglio poter aggiungere un assistito di cui mi prendo cura. (CG-39)**

FRONTEND:

- Bottone "aggiungi assistito" nella pagina principale caregiver.
- Form: nome, cognome, data di nascita, telefono, indirizzo abituale (facoltativo).
- Al salvataggio, l'assistito compare come nuova card nella pagina principale.

BACKEND:

- Creazione dell'anagrafica dell'assistito e del collegamento caregiver↔assistito, entrambi nello stesso passaggio: nessun invito da accettare, l'assistito non ha modo di dare o negare il proprio consenso.
- Validazione dei dati obbligatori (nome, cognome, data di nascita, telefono).

**9.2 Come caregiver voglio poter visualizzare l'elenco degli assistiti di cui mi occupo. (CG-40)**

FRONTEND:

- Pagina principale con una card per ogni assistito collegato.
- La card mostra: nome, prossimo viaggio in programma.
- Stato vuoto con invito ad aggiungere il primo assistito.

BACKEND:

- Lettura degli assistiti collegati al caregiver autenticato, tramite i collegamenti attivi.
- Aggregazione del prossimo viaggio per ciascun assistito.

**9.3 Come caregiver voglio poter visualizzare i dati e i viaggi di un assistito che seguo. (CG-41)**

FRONTEND:

- Pagina di dettaglio dell'assistito: dati anagrafici, elenco degli altri caregiver collegati, prossimi viaggi e storico.
- Card dei viaggi con evidenza di chi li ha prenotati, se collegato più di un caregiver.

BACKEND:

- Verifica che il richiedente abbia un collegamento attivo con l'assistito prima di esporre qualunque dato (autorizzazione).
- Se l'assistito ha più caregiver collegati, ciascuno vede tutti i dati e tutti i viaggi, senza distinzione fra chi lo ha aggiunto e gli altri collegati.

**9.4 Come caregiver voglio poter modificare i dati di un assistito che seguo. (CG-42)**

FRONTEND:

- Form di modifica precompilato con i dati attuali dell'assistito, raggiungibile dalla pagina di dettaglio.
- Pop-up di conferma sul salvataggio.

BACKEND:

- Verifica del collegamento attivo tra il richiedente e l'assistito.
- Validazione e aggiornamento dei dati anagrafici.

**9.5 Come caregiver voglio poter collegare direttamente un altro caregiver a un assistito che già seguo. (CG-43)**

FRONTEND:

- Bottone "aggiungi caregiver" nella pagina di dettaglio dell'assistito.
- Form con l'e-mail del caregiver da collegare.
- Il nuovo caregiver compare nella lista dei collegati non appena l'operazione va a buon fine.

BACKEND:

- Verifica che chi effettua l'operazione abbia già un collegamento attivo con l'assistito.
- Verifica che l'e-mail indicata corrisponda a un account caregiver esistente.
- Creazione diretta del nuovo collegamento: in v0 nessun invito da accettare, per lo stesso motivo della 9.1 — è una semplificazione deliberata, non una necessità (il caregiver invitato è un utente con un proprio account e potrebbe voler rifiutare). Il flusso di invito con consenso è rinviato alla v1 (§14).
- Un caregiver non può comparire due volte come collegato allo stesso assistito.

**9.6 Come caregiver voglio poter rimuovere un collegamento caregiver↔assistito. (CG-44)**

FRONTEND:

- Bottone di rimozione accanto a ciascun caregiver collegato, nella pagina di dettaglio dell'assistito.
- Pop-up di conferma: "Vuoi rimuovere [nome] come caregiver di [assistito]? SI/NO".
- Un caregiver può sempre rimuovere il proprio collegamento; la rimozione del collegamento di un altro caregiver richiede una conferma esplicita più marcata.

BACKEND:

- Il collegamento viene disattivato, non cancellato.
- Blocco della rimozione se è l'unico caregiver collegato all'assistito, o richiesta di conferma esplicita che l'assistito resterebbe senza nessun caregiver.
- I viaggi passati restano nello storico dell'assistito, visibili agli eventuali caregiver rimasti collegati.

## UC 10 – ACCOUNT

**10.1 Come caregiver o associazione voglio potermi registrare in GoCare. (CG-01, AS-01)**

FRONTEND:

- Pagina di registrazione senza header, con selezione iniziale del tipo di account: "Sono un caregiver" / "Sono un'associazione".
- Form caregiver: nome, cognome, data di nascita, indirizzo, telefono, e-mail, password, conferma password.
- Form associazione: denominazione, sede, province coperte, recapiti, e-mail, password.
- Indicatore di robustezza della password e checkbox di accettazione dell'informativa privacy.
- Messaggio finale: "Ti abbiamo inviato un'e-mail di verifica".

BACKEND:

- Validazione dei dati e del form; controllo di unicità dell'e-mail.
- Hashing della password.
- Creazione dell'account in stato "non verificato" e invio dell'e-mail di verifica con token a scadenza.
- Creazione contestuale del profilo di dominio corrispondente (caregiver o associazione) con lo **stesso identificatore** dell'account: i due dati vivono su database distinti, senza foreign key. L'assistito non ha un profilo di questo tipo: nasce solo con l'UC 9.1, senza account.
- Attivazione dell'account al click sul link di verifica.
- Per le associazioni, dopo l'attivazione l'account è a tutti gli effetti attivo; è il profilo associazione a restare in stato "in attesa di accreditamento" finché non interviene l'UC 10.5, e l'operatività sulle richieste è subordinata all'accreditamento del profilo.

_[DA VALIDARE] Va deciso se la registrazione di un'associazione richieda una validazione manuale da parte di GoCare prima dell'abilitazione operativa. Raccomandazione: sì, perché un'associazione accreditata accede a dati personali di persone fragili._

**10.2 Come utente voglio poter effettuare il login. (CG-02, AS-02)**

FRONTEND:

- Pagina di login senza header: e-mail, password, "ricordami", link "password dimenticata", link alla registrazione.
- Messaggio di errore generico in caso di credenziali errate.
- Dopo l'accesso, reindirizzamento alla pagina principale del ruolo: dashboard richieste per l'associazione, elenco assistiti per il caregiver.
- Al login vengono mostrati gli eventuali pop-up di notifica pendenti (UC 4).

BACKEND:

- Verifica delle credenziali ed emissione del token di sessione, con token di rinnovo revocabile.
- Blocco dell'accesso agli account non verificati, con messaggio dedicato.
- Limitazione dei tentativi di accesso falliti, con blocco temporaneo.
- Restituzione del ruolo, che determina la navigazione e le voci di header. Il ruolo distingue solo caregiver e associazione: l'assistito non è un ruolo di account.

**10.3 Come utente voglio poter recuperare la password in caso di smarrimento. (CG-04, AS-03)**

FRONTEND:

- Pagina "Password dimenticata" senza header, con il solo campo e-mail.
- Messaggio neutro: "Se l'indirizzo è registrato, riceverai un'e-mail con le istruzioni".
- Pagina di impostazione della nuova password, raggiunta dal link ricevuto via e-mail.
- Messaggio di esito e reindirizzamento al login.

BACKEND:

- Generazione di un token di reset a scadenza, monouso.
- Invio dell'e-mail con il link di reset.
- Nessuna informazione sull'esistenza dell'account nella risposta.
- Aggiornamento della password e invalidazione delle sessioni attive.

**10.4 Come utente voglio poter eliminare il mio account. (CG-05, AS-04)**

FRONTEND:

- Bottone "elimina account" in fondo alla pagina di gestione dati personali.
- Pop-up di conferma con reinserimento della password e avviso sulle conseguenze (perdita dell'accesso allo storico; gli assistiti collegati solo a questo caregiver restano senza nessun caregiver).
- Blocco con messaggio esplicativo se esistono viaggi futuri confermati.
- Per l'associazione: blocco se esistono trasporti accettati non ancora conclusi.

BACKEND:

- Soft delete dell'account e del profilo caregiver, e anonimizzazione dei dati personali con registrazione della data di anonimizzazione.
- Verifica dell'assenza di viaggi futuri attivi prima di procedere.
- Disattivazione dei collegamenti caregiver↔assistito di cui l'account era parte. Se un assistito resta senza nessun caregiver collegato, l'anagrafica resta nel database ma non è più raggiungibile da nessun account: da valutare se serva un avviso esplicito prima di procedere (vedi Punti aperti).
- Invalidazione di tutte le sessioni e invio dell'e-mail di conferma.

_[DA VALIDARE] Va definita la politica di conservazione dei dati dopo l'eliminazione dell'account: i viaggi conclusi restano nello storico dell'associazione anche dopo l'anonimizzazione dell'utente? E cosa succede a un assistito che resta senza nessun caregiver collegato?_

**10.5 Come amministratore GoCare voglio poter accreditare o rifiutare un'associazione. (AD-01)**

FRONTEND:

- Elenco delle associazioni in attesa di accreditamento, con i dati dichiarati in registrazione.
- Azioni "accredita" e "rifiuta", con pop-up di conferma.
- Nel profilo dell'associazione, badge con lo stato di accreditamento (in attesa / accreditata / rifiutata).

BACKEND:

- L'associazione nasce in stato "in attesa di accreditamento"; l'azione dell'amministratore la porta in "accreditata" o "rifiutata".
- Solo le associazioni accreditate entrano nelle liste di destinatari delle nuove richieste (UC 1.1) e possono accettare trasporti.
- In v0 la verifica dell'ente può essere una procedura esterna alla piattaforma: sulla piattaforma resta la sola registrazione dell'esito.

---

# 10. Tabella delle Route

## 10.1 Pagine dell'applicazione

Elenco completo delle pagine dell'applicazione, con path proposto, ruoli abilitati e presenza dell'header. Le pagine di autenticazione sono le uniche prive di header.

| Pagina                                                                       | Route                             | Ruoli                     | Header | UC                  |
| ------------------------------------------------------------------------------ | ---------------------------------- | -------------------------- | ------ | -------------------- |
| Registrazione                                                                | /register                         | Pubblico                  | NO     | 10.1                |
| Login                                                                        | /login                            | Pubblico                  | NO     | 10.2                |
| Recupero password                                                            | /forgot-password                  | Pubblico                  | NO     | 10.3                |
| Reimposta password                                                           | /reset-password/:token            | Pubblico                  | NO     | 10.3                |
| Verifica e-mail                                                              | /verify-email/:token              | Pubblico                  | NO     | 10.1                |
| Pagina principale associazione (dashboard nuove richieste + filtri)         | /associazione/richieste           | Associazione              | SÌ     | 6.1, 6.2            |
| Dettaglio richiesta pendente                                                 | /associazione/richieste/:id       | Associazione              | SÌ     | 6.2, 6.3, 6.4, 6.5  |
| Pagina privata associazione (richieste accettate, info associazione)        | /associazione/trasporti           | Associazione              | SÌ     | 7.1                 |
| Dettaglio trasporto accettato                                                | /associazione/trasporti/:id       | Associazione              | SÌ     | 7.1                 |
| Generazione/rigenerazione link operatore                                    | /associazione/trasporti/:id/stato | Associazione              | SÌ     | 4.11                |
| Schermata operatore (avanzamento stato, via link, nessun login)             | /operatore/:token                 | Pubblico (solo con link)  | NO     | 4.11                |
| Storico trasporti associazione                                               | /associazione/storico             | Associazione              | SÌ     | 7.3                 |
| Pagina di modifica informazioni associazione                                 | /associazione/profilo/modifica    | Associazione              | SÌ     | 8.5, 8.6            |
| Profilo associazione (sola lettura)                                          | /associazione/profilo             | Associazione              | SÌ     | 8.5                 |
| Pagina principale caregiver (card degli assistiti)                          | /assistiti                        | Caregiver                 | SÌ     | 9.2                 |
| Form nuovo assistito                                                        | /assistiti/nuovo                  | Caregiver                 | SÌ     | 9.1                 |
| Dettaglio assistito (dati, caregiver collegati, viaggi)                     | /assistiti/:id                    | Caregiver                 | SÌ     | 9.3, 9.4, 9.5, 9.6  |
| Form nuovo trasporto (per quale assistito, data, destinazione, tipo viaggio, accompagnatori, contatti) | /viaggi/nuovo | Caregiver | SÌ | 1.1 – 1.8 |
| Dettaglio viaggio                                                            | /viaggi/:id                       | Caregiver                 | SÌ     | 3.1, 5.4            |
| Pagina "I miei viaggi" (prossimi viaggi + storico)                          | /viaggi                           | Caregiver                 | SÌ     | 5.1, 5.3            |
| Storico viaggi                                                               | /viaggi/storico                   | Caregiver                 | SÌ     | 5.3                 |
| Pagina gestione dati personali account                                      | /profilo                          | Caregiver                 | SÌ     | 8.1, 8.2, 10.4      |
| Gestione destinazioni salvate                                                | /profilo/destinazioni             | Caregiver                 | SÌ     | 8.3                 |
| Pagina pubblica associazione (contatti)                                     | /associazioni/:id                 | Caregiver                 | SÌ     | 5.2                 |
| Centro notifiche                                                             | /notifiche                        | Caregiver / Associazione  | SÌ     | UC 4                |

Note sulle scelte: (a) "Reimposta password" e "Verifica e-mail" sono pagine pubbliche raggiunte da link e-mail, necessarie perché i flussi di registrazione e recupero credenziali si concludano; (b) la pagina "gestione stato viaggio" è dove l'**associazione** genera/rigenera il link per l'operatore, non dove l'operatore stesso lavora: l'operatore ha una schermata separata (`/operatore/:token`), pubblica ma utilizzabile solo con il link ricevuto, senza login e senza accesso al resto dell'app; (c) "Storico viaggi" e "Storico trasporti" possono essere realizzate come tab della pagina principale corrispondente anziché come route autonome: la scelta è indifferente sul piano funzionale; (d) il centro notifiche raccoglie lo storico delle notifiche dell'UC 4, che altrimenti sarebbero consultabili solo al momento della ricezione; (e) non esistono più pagine di modifica del viaggio né di gestione di un gruppo cura: la gestione degli assistiti si concentra nella pagina di dettaglio (UC 9), con collegamento diretto in v0 (l'invito con accettazione è previsto per la v1, §14).

## 10.2 Endpoint di back-end

Elenco degli endpoint che servono le pagine della tabella precedente, con il ruolo abilitato e lo Use Case di riferimento.

| Area    | Metodo e path                                                   | Ruoli                    | UC                 |
| ------- | ----------------------------------------------------------------- | ------------------------- | ------------------- |
| Auth    | `POST /auth/register/user`                                      | Pubblico                 | 10.1               |
| Auth    | `POST /auth/register/association`                               | Pubblico                 | 10.1               |
| Auth    | `POST /auth/verify-email`                                       | Pubblico                 | 10.1               |
| Auth    | `POST /auth/verify-email/resend`                                | Pubblico                 | 10.1               |
| Auth    | `POST /auth/login`                                              | Pubblico                 | 10.2               |
| Auth    | `POST /auth/refresh`                                            | Pubblico                 | 10.2               |
| Auth    | `POST /auth/logout`                                             | Pubblico (possesso del refresh token) | 10.2  |
| Auth    | `POST /auth/forgot-password`                                    | Pubblico                 | 10.3               |
| Auth    | `POST /auth/reset-password`                                     | Pubblico                 | 10.3               |
| Auth    | `POST /auth/change-email`                                       | Autenticato              | 8.2                |
| Auth    | `DELETE /auth/account`                                          | Autenticato              | 10.4               |
| Dominio | `POST /transports`                                              | Caregiver                | 1.1 – 1.8          |
| Dominio | `GET /transports`                                               | Caregiver                | 5.1, 5.3           |
| Dominio | `GET /transports/:id`                                           | Caregiver                | 3.1, 5.4           |
| Dominio | `POST /transports/:id/cancel`                                   | Caregiver                | 3.1                |
| Dominio | `GET /transports/:id/status-timeline`                           | Caregiver                | 5.4                |
| Dominio | `GET /association/requests`                                     | Associazione             | 6.1, 6.2           |
| Dominio | `GET /association/requests/:id`                                 | Associazione             | 6.2                |
| Dominio | `POST /association/requests/:id/accept`                         | Associazione             | 6.3                |
| Dominio | `POST /association/requests/:id/decline`                        | Associazione             | 6.4                |
| Dominio | `GET /association/requests/:id/contacts`                        | Associazione             | 6.5                |
| Dominio | `GET /association/transports`                                   | Associazione             | 7.1                |
| Dominio | `GET /association/transports/:id`                               | Associazione             | 7.1                |
| Dominio | `POST /association/transports/:id/operator-link`                | Associazione             | 4.11               |
| Dominio | `GET /operatore/:token`                                          | Pubblico (solo con link) | 4.11               |
| Dominio | `POST /operatore/:token/status`                                  | Pubblico (solo con link) | 4.11               |
| Dominio | `POST /association/transports/:id/cancel`                       | Associazione             | 3.2                |
| Dominio | `GET /association/history`                                      | Associazione             | 7.3                |
| Dominio | `GET /association/profile` · `PUT /association/profile`         | Associazione             | 8.5, 8.6           |
| Dominio | `GET /me/profile` · `PUT /me/profile`                           | Caregiver                | 8.1, 8.2           |
| Dominio | `GET · POST · PUT · DELETE /me/destinations[/:id]`              | Caregiver                | 8.3                |
| Dominio | `GET /associations/:id/contacts`                                | Caregiver                | 5.2                |
| Dominio | `POST /assisted`                                                 | Caregiver                | 9.1                |
| Dominio | `GET /assisted`                                                  | Caregiver                | 9.2                |
| Dominio | `GET /assisted/:id`                                              | Caregiver                | 9.3                |
| Dominio | `PUT /assisted/:id`                                              | Caregiver                | 9.4                |
| Dominio | `POST /assisted/:id/caregivers`                                  | Caregiver                | 9.5                |
| Dominio | `DELETE /assisted/:id/caregivers/:caregiverId`                   | Caregiver                | 9.6                |
| Dominio | `GET /notifications`                                            | Tutti                    | UC 4               |
| Dominio | `POST /notifications/:id/read` · `POST /notifications/read-all` | Tutti                    | UC 4               |
| Dominio | `GET /notifications/counters`                                   | Tutti                    | 11.4               |
| Dominio | `POST /devices` · `DELETE /devices/:id`                         | Tutti                    | UC 4               |
| Admin   | `POST /admin/associations/:id/accredit`                         | Amministratore GoCare    | 10.5               |
| Admin   | `POST /admin/associations/:id/reject`                           | Amministratore GoCare    | 10.5               |

---

# 11. Definizione dell'Header

L'header è presente su tutte le pagine successive all'autenticazione. La struttura è la stessa per tutti i ruoli; cambiano le voci di navigazione centrali.

## 11.1 Struttura comune

| Posizione | Elemento                           | Comportamento                                                           |
| --------- | ----------------------------------- | -------------------------------------------------------------------------- |
| Sinistra  | Logo "GoCare"                      | Riporta alla pagina principale del ruolo.                               |
| Centro    | Voci di navigazione                | Variabili per ruolo (vedi 11.2 e 11.3).                                 |
| Destra    | Icona notifiche con badge numerico | Apre il centro notifiche. Il badge conta le notifiche non lette (UC 4). |
| Destra    | Avatar / menu utente               | Menu a tendina: Profilo, Impostazioni, Logout.                          |

## 11.2 Voci di navigazione – Caregiver

| Voce            | Route         | UC che la giustifica | Motivazione                                                                                                          |
| ---------------- | ------------- | ---------------------- | -------------------------------------------------------------------------------------------------------------------- |
| I miei assistiti | /assistiti    | 9.2                   | Pagina principale del ruolo: le card degli assistiti collegati.                                                      |
| I miei viaggi    | /viaggi       | 5.1, 5.3              | Prossimi viaggi e storico.                                                                                           |
| Nuovo trasporto  | /viaggi/nuovo | 1.1                   | Azione principale del prodotto: resa raggiungibile da ogni pagina.                                                   |
| Profilo          | /profilo      | 8.1 – 8.3             | Dati personali, contatti, destinazioni salvate, eliminazione account.                                                |

"Nuovo trasporto" può essere realizzata come pulsante di azione in evidenza anziché come voce di navigazione: è la funzione più frequente e sarebbe penalizzata se raggiungibile solo dalla pagina "I miei viaggi".

## 11.3 Voci di navigazione – Associazione

| Voce                 | Route                   | UC che la giustifica | Motivazione                                                                                                                                                       |
| --------------------- | ------------------------ | ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Richieste            | /associazione/richieste | 6.1, 6.2              | Dashboard delle richieste pendenti con filtri. È la pagina operativa principale dell'associazione; porta un badge con il numero di richieste non ancora valutate (UC 4.9). |
| Trasporti accettati  | /associazione/trasporti | 7.1                   | Vista dei trasporti presi in carico; da qui si accede alla gestione dello stato del viaggio (UC 4.11).                                                             |
| Storico              | /associazione/storico   | 7.3                   | Viaggi conclusi, annullati e rifiutati. Separato dai trasporti attivi per non appesantire la vista operativa.                                                     |
| Profilo associazione | /associazione/profilo   | 8.5, 8.6              | Informazioni, contatti e province coperte dell'associazione. I contatti sono esposti agli utenti (UC 5.2) e le province determinano quali richieste arrivano, quindi devono essere aggiornabili facilmente. |

Le quattro voci del ruolo Associazione sono derivate dagli Use Case 6, 7 e 8: senza "Richieste" e "Trasporti accettati" le due dashboard previste dagli UC 6 e 7 non sarebbero raggiungibili.

## 11.4 Badge e contatori

| Elemento                   | Ruolo                 | Cosa conta                                                 | UC        |
| --------------------------- | ----------------------- | ------------------------------------------------------------- | ----------- |
| Icona notifiche            | Tutti                 | Notifiche non lette                                        | 4.2 – 4.9 |
| Voce "Richieste"           | Associazione          | Richieste pendenti non ancora valutate                     | 4.9, 6.1  |
| Voce "I miei viaggi"       | Caregiver             | Viaggi con esito o cambio di stato non ancora visualizzato | 4.4       |

_Rimossa la riga sul contatore delle richieste di modifica pendenti su "Trasporti accettati": la modifica del viaggio non esiste più._

---

# 12. Flussi

Sequenza logica dei processi principali. I flussi descrivono solo la successione dei passi e i cambi di stato, senza indicazioni grafiche.

## 12.1 Registrazione

1. L'utente apre /register e sceglie il tipo di account (caregiver oppure associazione).
2. Compila il form corrispondente e accetta l'informativa privacy.
3. Il sistema valida i dati e verifica che l'e-mail non sia già registrata.
4. L'account viene creato in stato "non verificato" e, contestualmente, viene creato il profilo di dominio corrispondente (caregiver o associazione) con lo stesso identificatore dell'account.
5. Viene inviata l'e-mail di verifica con token a scadenza.
6. L'utente apre il link ricevuto: l'account passa in stato "attivo".
7. Per le associazioni, l'account è attivo come per un caregiver; è il profilo associazione a restare in stato "in attesa di accreditamento" da parte di GoCare finché l'amministratore non interviene, e solo l'accreditamento del profilo abilita l'operatività sulle richieste (UC 10.5).
8. L'utente viene reindirizzato al login.

Casi alternativi:

- E-mail già registrata → messaggio di errore sul campo, nessun account creato.
- Link di verifica scaduto → pagina con possibilità di richiedere un nuovo invio.
- Login con account non verificato → accesso negato con invito a verificare l'e-mail.
- Creazione dell'account riuscita ma creazione del profilo fallita → l'incoerenza (stesso database, due scritture separate non nella stessa transazione) viene recuperata da una riconciliazione, non lasciata all'utente.

## 12.2 Login

1. L'utente apre /login e inserisce e-mail e password.
2. Il sistema verifica le credenziali e lo stato dell'account.
3. In caso di esito positivo viene emesso il token di sessione e restituito il ruolo.
4. L'utente viene reindirizzato alla pagina principale del proprio ruolo: /associazione/richieste per l'associazione, /assistiti per il caregiver.
5. Vengono mostrati gli eventuali pop-up di notifica pendenti (conferma viaggio, disdetta).

Casi alternativi:

- Credenziali errate → messaggio generico, senza indicare quale campo è sbagliato.
- Superamento del numero di tentativi → blocco temporaneo dell'accesso.
- Associazione non ancora accreditata → accesso consentito al solo profilo, nessuna richiesta visibile.

## 12.3 Recupero password

1. Dalla pagina di login l'utente apre /forgot-password e inserisce la propria e-mail.
2. Il sistema mostra sempre lo stesso messaggio neutro, indipendentemente dall'esistenza dell'account.
3. Se l'account esiste, viene generato un token monouso a scadenza e inviata l'e-mail con il link di reset.
4. L'utente apre /reset-password/:token e imposta la nuova password.
5. Il sistema aggiorna la password, invalida il token e tutte le sessioni attive.
6. L'utente viene reindirizzato al login.

## 12.4 Richiesta di un nuovo trasporto

1. Il caregiver apre /viaggi/nuovo.
2. Seleziona l'assistito beneficiario del trasporto tra quelli a lui collegati.
3. Indica tipo di viaggio, direzione, data e orario; con "andata e ritorno" indica anche l'orario di ritorno.
4. Seleziona la destinazione dalla lista delle destinazioni salvate oppure ne inserisce una nuova; in questo secondo caso il sistema chiede se salvarla per il futuro. Se il ritorno è verso un indirizzo diverso da quello di partenza, lo indica come terzo indirizzo.
5. Indica gli eventuali accompagnatori e i contatti di riferimento.
6. Il sistema valida i dati e la coerenza fra direzione, orari e indirizzi, copia gli indirizzi e i contatti sulla richiesta e la crea in stato "in attesa".
7. Il sistema calcola le associazioni destinatarie, cioè quelle accreditate che coprono la provincia di partenza, e congela l'elenco sulla richiesta.
8. Il sistema invia la notifica di nuova richiesta alle associazioni destinatarie (UC 4.9).
9. La richiesta compare nella dashboard /associazione/richieste di ciascuna destinataria, con i relativi filtri.
10. Un'associazione apre il dettaglio e accetta: la richiesta passa in stato "confermata" e le viene assegnata l'associazione.
11. Il sistema notifica l'utente della presa in carico e rende visibili i contatti dell'associazione (UC 4.2, 5.2).
12. La richiesta esce dalla dashboard delle pendenti di tutte le associazioni e compare in /associazione/trasporti dell'associazione assegnataria.

Casi alternativi:

- Un'associazione rifiuta: il rifiuto viene registrato per quella sola associazione, la richiesta resta "in attesa" e continua a essere visibile alle altre destinatarie.
- Due associazioni accettano contemporaneamente: vince la prima, la seconda riceve un messaggio di errore gestito dal controllo di concorrenza sulla richiesta.
- Nessuna associazione accetta entro la soglia definita _[DA VALIDARE]_: la richiesta passa in stato "non coperta" e l'utente riceve la notifica di esito negativo (UC 4.1).
- Nessuna associazione accreditata copre la provincia di partenza: la richiesta nasce senza destinatari e viene dichiarata "non coperta".

Il viaggio, una volta creato, non è più modificabile: se qualcosa cambia, il caregiver lo annulla (UC 3) e invia una nuova richiesta.

## 12.6 Annullamento del trasporto

Annullamento da parte dell'utente:

1. L'utente apre il dettaglio del viaggio e preme "annulla viaggio".
2. Compare il pop-up di conferma con l'avviso che sarà necessaria una nuova richiesta.
3. L'utente seleziona la causale dalla tendina o la scrive nel campo libero e conferma.
4. Il sistema esegue il soft delete: il viaggio passa in stato "annullato", registra chi ha annullato e quando, e resta nello storico.
5. Se il viaggio era già stato preso in carico, il sistema notifica l'associazione (UC 4.10) e lo rimuove dalla sua lista dei trasporti attivi.

Annullamento da parte dell'associazione (disdetta):

1. L'associazione apre il trasporto accettato e preme "disdici".
2. Compila la causale, obbligatoria, e conferma nel pop-up.
3. Il sistema registra la disdetta come rifiuto post-accettazione di quella associazione e il viaggio viene rimosso dai trasporti in carico.
4. Il viaggio torna in stato "in attesa" e rientra nella dashboard delle richieste pendenti delle altre associazioni destinatarie, per consentire una copertura alternativa _[DA VALIDARE]_.
5. Il sistema notifica l'utente con il testo della causale (UC 4.3) e gli propone l'azione "cerca un'alternativa".
6. Se nessun'altra associazione accetta entro la soglia, il viaggio passa in stato "non coperto" e l'utente riceve la notifica di esito negativo.

## 12.7 Ciclo di stato del viaggio

Stati della richiesta (dalla creazione alla presa in carico):

in attesa → confermata → in esecuzione → conclusa

Stati terminali alternativi: non coperta (nessuna associazione destinataria ha accettato), annullata (dall'utente o dall'associazione).

Il rifiuto di una singola associazione non è uno stato della richiesta: viene registrato come rifiuto di quella associazione e la richiesta resta "in attesa" per le altre destinatarie. L'unico esito negativo aggregato è "non coperta".

Stati di esecuzione del viaggio (UC 4.1), aggiornati dall'operatore tramite il link generato dall'associazione (UC 4.11):

| Stato               | Chi lo imposta | Significato                                                                    | Notifiche                                                                              |
| --------------------- | ---------------- | ---------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| Non preso in carico | —              | Il viaggio è confermato ma l'operatore non ha ancora iniziato.                 | Nessuna notifica (stato iniziale).                                                     |
| Preso in carico     | Operatore      | L'operatore ha preso in carico l'assistito.                                    | Notifica push ai caregiver collegati: "L'assistito è stato preso in carico".           |
| In arrivo           | Operatore      | Il mezzo è in viaggio verso la destinazione.                                   | Notifica push ai caregiver collegati.                                                  |
| In visita           | Operatore      | L'assistito è alla destinazione (visita, ricovero, esame).                     | Notifica push ai caregiver collegati.                                                  |
| In ritorno          | Operatore      | Viaggio di rientro in corso. Stato non attraversato dai viaggi di sola andata. | Notifica push ai caregiver collegati.                                                  |
| Sospeso             | Operatore      | Guasto o imprevisto durante il viaggio. Il viaggio resta in carico all'associazione, che resta responsabile del rientro. | Notifica push immediata ai caregiver collegati, con e-mail (evento di esito). |
| Concluso            | Operatore      | Il viaggio è terminato. Stato terminale.                                       | Notifica push ai caregiver collegati; il viaggio passa nello storico di utente e associazione. |

Regole di transizione:

- Gli stati si attraversano in sequenza; non sono ammessi salti in avanti né ritorni indietro, salvo "sospeso" (vedi sotto).
- Un viaggio di sola andata (ricovero, trasferimento) salta lo stato "in ritorno".
- Un viaggio di solo ritorno (dimissione) non attraversa lo stato "in visita".
- "Sospeso" è raggiungibile da qualunque stato di esecuzione attivo (preso in carico, in arrivo, in visita, in ritorno) e da esso si riprende dallo stesso stato in cui ci si era fermati: non è uno stato terminale, è una pausa registrata nel percorso.
- Ogni transizione registra stato, timestamp, associazione e operatore che l'ha effettuata, come nuova riga di un registro append-only: lo stato corrente è la transizione più recente e nessuna riga viene modificata.
- I due assi di stato avanzano insieme: alla prima transizione di esecuzione la richiesta passa da "confermata" a "in esecuzione"; con "concluso" passa a "conclusa".
- Il passaggio a "concluso" chiude il viaggio e lo rende non più modificabile (non che lo fosse comunque: il viaggio non è mai modificabile dopo la creazione, in nessuno stato).
- Le notifiche di cambio stato sono solo push, salvo "sospeso" (push + e-mail, trattato come evento di esito data la sua criticità): le altre e-mail restano riservate a presa in carico, disdetta, mancata copertura.

## 12.8 Gestione degli assistiti

Aggiunta di un assistito:

1. Il caregiver apre /assistiti e preme "aggiungi assistito".
2. Inserisce nome, cognome, data di nascita, telefono e conferma.
3. Il sistema crea l'anagrafica dell'assistito e il collegamento con il caregiver nello stesso passaggio: nessun invito, nessuna accettazione.
4. L'assistito compare come nuova card nella pagina principale.

Collegamento di un ulteriore caregiver (v0, diretto — vedi §14 per il flusso a invito previsto in v1):

1. Un caregiver già collegato apre il dettaglio dell'assistito e preme "aggiungi caregiver".
2. Indica l'e-mail dell'account caregiver da collegare e conferma.
3. Il sistema verifica che l'e-mail corrisponda a un account caregiver esistente e crea il collegamento direttamente, senza invito.
4. Da quel momento anche il nuovo caregiver vede tutti i dati e tutti i viaggi dell'assistito.

Modifica dei dati dell'assistito:

1. Un caregiver collegato apre il dettaglio dell'assistito e preme "modifica".
2. Aggiorna i campi nel form precompilato e conferma.
3. Il sistema applica la modifica; è visibile immediatamente a tutti i caregiver collegati.

Rimozione di un collegamento:

1. Un caregiver collegato preme il bottone di rimozione accanto a un caregiver nella lista (il proprio o quello di un altro collegato).
2. Conferma nel pop-up.
3. Il sistema verifica che non sia l'ultimo collegamento attivo dell'assistito; in tal caso richiede una conferma esplicita aggiuntiva.
4. Il collegamento viene disattivato; i viaggi passati restano nello storico dell'assistito, visibili agli eventuali caregiver rimasti collegati.

---

# 13. Cosa manca / Punti aperti

Per ciascun punto è indicata un'opzione consigliata, che resta da validare con il team prima dello sviluppo. Dove l'opzione consigliata è già stata recepita dal modello dati della v0 il punto lo dichiara, ma la decisione non è per questo chiusa: dove incide sui flussi resta segnalata con l'etichetta [DA VALIDARE].

Riepilogo:

| ID    | Punto aperto                                                     | Impatto | Stato   |
| ----- | ------------------------------------------------------------------ | ------- | ------- |
| PA-01 | Modello di prenotazione: richiesta utente o slot offerti         | Alto    | Aperto |
| PA-02 | Soglia oltre la quale una richiesta è "non coperta"              | Alto    | Aperto |
| PA-03 | Visibilità delle richieste: tutte le associazioni o per area     | Alto    | Aperto |
| PA-04 | Esposizione dei dati di contatto dell'assistito all'associazione | Alto    | Aperto |
| PA-05 | Accreditamento delle associazioni                                 | Medio   | Aperto |
| PA-06 | Politica di conservazione dei dati dopo l'eliminazione account    | Basso   | Aperto |

## PA-01 Modello di prenotazione: richiesta dell'utente o slot offerti dalle associazioni

Problema: È aperta la domanda "l'utente richiede il trasporto, o le associazioni offrono disponibilità e l'utente seleziona lo slot?". È la decisione più strutturante del prodotto: cambia il modello dati, il flusso principale (12.4), il contenuto della dashboard associazione e il significato stesso degli stati della richiesta.

Opzione consigliata: Mantenere il modello a richiesta, coerente con l'intero documento e con la decisione che GoCare non gestisce le disponibilità delle associazioni. Il modello a slot richiederebbe alle associazioni di pubblicare in anticipo i turni disponibili, un onere organizzativo che la maggior parte delle associazioni di volontariato non è in grado di sostenere. Tutto il documento è scritto assumendo il modello a richiesta: se il team scegliesse il modello a slot, gli UC 1, 4, 5 e 6 andrebbero riscritti.

Stato in v0: Il modello dati realizzato è quello a richiesta.

## PA-02 Soglia oltre la quale una richiesta è considerata "non coperta"

Problema: La story CG-25 ("voglio sapere se il viaggio non può essere coperto") e la notifica dell'UC 4.4 presuppongono un momento in cui il sistema dichiara fallita la ricerca di copertura, ma nessuna regola lo definisce. Senza questa regola l'utente resta indefinitamente in attesa e non può cercare alternative.

Opzione consigliata: Dichiarare la richiesta "non coperta" al verificarsi della prima delle due condizioni: tutte le associazioni destinatarie hanno rifiutato, oppure mancano meno di 24 ore alla data del viaggio e nessuna l'ha accettata. La seconda condizione è quella che conta davvero: lascia all'utente il tempo di organizzarsi diversamente.

Stato in v0: Aperto. Il modello dati registra i rifiuti per associazione e la lista congelata delle destinatarie, quindi entrambe le condizioni sono calcolabili; manca la scelta della soglia.

## PA-03 Visibilità delle richieste: tutte le associazioni o per area geografica

Problema: La dashboard dell'UC 6 mostra "tutte le richieste di trasporto dei caregiver", ma non è specificato se ogni associazione veda tutte le richieste della piattaforma o solo quelle del proprio territorio.

Opzione consigliata: Introdurre un'area di operatività nel profilo dell'associazione (UC 8.5) e mostrare a ciascuna associazione solo le richieste con partenza entro la propria area. In fase di test, con poche associazioni pilota, la regola può essere disattivata; senza di essa, però, il prodotto non è scalabile oltre una singola città.

Stato in v0: Il modello dati recepisce l'opzione consigliata — l'associazione dichiara un elenco di province coperte, confrontato con la provincia dell'indirizzo di partenza; l'elenco delle destinatarie viene congelato sulla richiesta alla creazione — ma il criterio (provincia anziché raggio in km o comune) va confermato.

## PA-04 Esposizione dei dati di contatto dell'assistito all'associazione

Problema: L'associazione deve poter contattare chi ha prenotato, ma non è definito a quali dati acceda, da quale momento e con quale base giuridica. Il tema riguarda persone fragili e dati sanitari indiretti (il tipo di viaggio rivela la natura della prestazione).

Opzione consigliata: Mostrare nella fase di valutazione della richiesta (UC 6.1, 6.2) solo i dati necessari a decidere: iniziali o nome, fascia oraria, indirizzo di partenza a livello di via, destinazione, esigenze di mobilità. Rendere visibili i recapiti completi solo dopo l'accettazione (UC 6.5), tracciando gli accessi. Va inoltre redatta un'informativa privacy, richiamata in fase di registrazione (UC 10.1).

Stato in v0: Il modello dati recepisce la parte di tracciamento — ogni accesso ai contatti registra richiesta, associazione, tipo di dato (richiedente o beneficiario) e istante — mentre la soglia di visibilità dei dati minimi resta da definire.

## PA-05 Accreditamento delle associazioni

Problema: L'UC 10 tratta la registrazione delle associazioni come quella di un caregiver qualsiasi. Un account associazione, però, accede ai dati personali di persone fragili e può prendere in carico trasporti sanitari.

Opzione consigliata: Prevedere una validazione manuale da parte di GoCare prima dell'abilitazione operativa dell'account associazione: registrazione libera, ma stato "in attesa di accreditamento" finché l'ente non viene verificato. In v0 la verifica può essere una procedura esterna alla piattaforma.

Stato in v0: Il modello dati recepisce l'opzione consigliata — l'associazione porta uno stato di accreditamento con tre valori (in attesa, accreditata, rifiutata) e nasce in attesa — ma restano da definire i criteri di verifica e chi li applica.

## PA-06 Conservazione dei dati dopo l'eliminazione dell'account

Problema: L'UC 10 prevede l'eliminazione dell'account ma non dice cosa accada ai viaggi conclusi presenti nello storico dell'associazione, che potrebbe averne bisogno per rendicontazione, né a un assistito che resta senza nessun caregiver collegato.

Opzione consigliata: Anonimizzare i dati personali del caregiver mantenendo il viaggio nello storico dell'associazione con i soli dati di servizio (data, tratta, esito). Per l'assistito senza più caregiver collegati: mantenerne l'anagrafica (non è mai stata un account, non richiede anonimizzazione ai fini dell'accesso) ma segnalarla come irraggiungibile, in attesa che un altro caregiver la ricolleghi o che venga eliminata con un'azione esplicita. Da verificare con i requisiti di rendicontazione delle associazioni coinvolte.

Stato in v0: Il modello dati recepisce parzialmente l'opzione consigliata — il caregiver porta una data di cancellazione e una data di anonimizzazione distinte, e i viaggi conservano indirizzi e contatti copiati al momento della richiesta — ma la politica di retention, e il comportamento per l'assistito rimasto senza caregiver, restano da confermare.

---

# 14. Roadmap v1

Punti valutati e deliberatamente rinviati oltre la v0, raccolti qui in un solo posto invece che sparsi fra i singoli Use Case. Nessuno di questi introduce debito tecnico nella v0: sono funzionalità assenti, non abbozzate a metà. Non in questa lista perché **decisi per la v0** (non rinviati): lo stato "sospeso" per guasti/imprevisti e il link scoped al trasporto per l'operatore — entrambi fanno parte del modello v0 (UC 4.11, §12.7).

## Invito con consenso per il collegamento di un secondo caregiver

Il modello v0 collega un secondo caregiver a un assistito in modo diretto e immediato, senza che possa accettare o rifiutare — a differenza dell'assistito (che non ha un account e non può comunque esprimere consenso), il caregiver invitato è un utente con un proprio account e potrebbe non voler essere associato alla cura di quell'assistito. In v1: sostituire il collegamento diretto (UC 9.5) con un invito da accettare/rifiutare, condivisibile anche fuori dall'app (e-mail, WhatsApp). Il link userà l'`Id` di `AssistedPerson` (già un Guid, non indovinabile) invece di un token dedicato, perché la prova di identità è il login del caregiver invitato, non il possesso del link — il server verifica che l'utente autenticato corrisponda al collegamento in attesa.

## Modifica della richiesta di viaggio

L'UC "Modifica trasporto" non è presente in v0: dopo la creazione, l'unica azione su un viaggio è l'annullamento. Semplificazione deliberata per lanciare prima, non un giudizio definitivo sulla necessità della funzione. In v1 il disegno riparte da zero: una possibile base di partenza è una richiesta di modifica a data/orario/destinazione/accompagnatori, con approvazione dell'associazione se il viaggio è già confermato — ma va valutata alla luce dell'uso reale della v0: potrebbe bastare annullamento+nuova richiesta, o servire una versione più snella senza approvazione per i cambi minori. Se questo punto viene ripreso, va affrontata anche la domanda su cosa succede quando l'associazione rifiuta una modifica proposta (esito, opzioni per l'utente).

## Calcolo dei km previsti

La richiesta guadagna un campo km; le dashboard dell'associazione un filtro/colonna dedicati. Rinviato perché il calcolo richiede un servizio esterno di geocoding/instradamento, non solo un campo aggiunto al form (§6 "Decisioni consolidate").

## Trasporto sociale

Nuovo tipo di viaggio (spesa, posta, commissioni varie), oltre ai quattro sanitari della v0 (visita, ricovero, dimissione, trasferimento). Non introduce complessità tecnica di per sé, ma cambia il posizionamento del prodotto e il tipo di associazioni da coinvolgere — da riproporre dopo la validazione sul trasporto sanitario.

## Trasporti ricorrenti

Possibilità di registrare un trasporto che si ripete (es. dialisi ogni martedì) senza rifare la richiesta ogni volta. Escluso dalla v0 perché la ricorrenza implica generazione automatica delle occorrenze, gestione delle eccezioni sulla singola data e annullamento in blocco: è un sottosistema, non un campo del form.

## Posizione GPS del mezzo

Tracciamento in tempo reale della posizione, oltre allo stato testuale con timeline già presente in v0. Richiede tracciamento continuo del mezzo, consenso esplicito del volontario e trattamento di dati di localizzazione; lo stato testuale copre già l'esigenza di ridurre le chiamate all'associazione durante il viaggio.

## Flusso API bidirezionale

Tutte le chiamate in scrittura, ogni chiamata corrisponde a un cambio di stato — alternativa al modello REST misto lettura/scrittura della v0. Impatto architetturale ampio, non solo funzionale.
