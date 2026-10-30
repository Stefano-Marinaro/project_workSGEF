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

Elenco dei macro Use Case e delle relative sotto-voci. L'identificativo UC 2 non è usato: dopo la creazione l'unica azione su un viaggio è l'annullamento (UC 3), non esiste modifica del viaggio. La sotto-voce 1.4 (accompagnatori) è stata rimossa e il suo numero non viene riassegnato.

## UC 1 – RICHIEDI TRASPORTO

- 1.1 Richiedere un trasporto (data, orario, luogo di partenza, destinazione).
- 1.2 Scegliere la direzione del viaggio (andata e ritorno, solo andata, solo ritorno).
- 1.3 Scegliere destinazioni diverse dal domicilio abituale.
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
- 5.3 Visualizzazione del nome dell'associazione che ha preso in carico il viaggio, con i relativi contatti, visibili solo dopo l'avvenuto pagamento (Stripe).
- 5.4 Visualizzazione dello storico dei viaggi conclusi.
- 5.5 Visualizzazione dello stato di avanzamento del viaggio in corso.

## UC 6 – VISUALIZZAZIONE E GESTIONE RICHIESTE PENDENTI (associazione)

- 6.1 Dashboard con le richieste di trasporto dei caregiver destinate all'associazione, con filtri (data, orario, destinazione, tipo di viaggio).
- 6.2 Visualizzazione del dettaglio della richiesta (informazioni personali, data, ora, destinazioni).
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

- 8.5 Visualizzazione delle informazioni dell'associazione, compresi i comuni coperti.
- 8.6 Aggiunta e modifica delle informazioni di contatto dell'associazione.

## UC 9 – GESTIONE ASSISTITI

L'assistito non è un account: è un'anagrafica gestita dal caregiver, senza login e senza possibilità di interazione propria. Un assistito può essere collegato a più caregiver; ogni caregiver con un collegamento attivo vede tutti i dati e tutti i viaggi dell'assistito, indipendentemente da chi lo ha originariamente aggiunto. **In v0 il collegamento è sempre diretto e immediato**, anche quando riguarda un secondo caregiver: non è richiesta nessuna accettazione. Un flusso di invito con consenso esplicito (link condivisibile via e-mail/WhatsApp) è previsto per la v1 — vedi §13 "Roadmap v1".

- 9.1 Aggiunta di un assistito da parte del caregiver.
- 9.2 Visualizzazione dell'elenco dei propri assistiti.
- 9.3 Visualizzazione del dettaglio di un assistito (dati anagrafici e viaggi).
- 9.4 Modifica dei dati di un assistito.
- 9.5 Collegamento diretto di un ulteriore caregiver a un assistito già esistente.
- 9.6 Rimozione di un collegamento caregiver↔assistito (il proprio o quello di un altro caregiver collegato).

## UC 10 – ACCOUNT

- 10.1 Creazione dell'account caregiver (le associazioni non si registrano autonomamente: le registra l'amministratore, UC 11.1).
- 10.2 Login.
- 10.3 Recupero delle credenziali in caso di smarrimento.
- 10.4 Eliminazione dell'account.

## UC 11 – AMMINISTRAZIONE GOCARE

- 11.1 Registrazione di un'associazione partner da parte dell'amministratore.
- 11.2 Modifica dei dati di un'associazione partner.
- 11.3 Monitoraggio delle richieste di trasporto destinate a un'associazione partner.
- 11.4 Eliminazione di un'associazione partner al termine della collaborazione.
- 11.5 Gestione degli account caregiver: ripristino dell'account.
- 11.6 Visualizzazione delle associazioni partner e delle rispettive informazioni (dati, stato account, trasporti).

---

# 3. User Stories e mappatura sugli Use Case

Le user story sono riportate con il riferimento allo Use Case (e alla sotto-voce) di appartenenza.

## 3.1 Stories caregiver

| ID    | User story                                                                                                                                                                                                               | UC                 |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------ |
| CG-01 | Come caregiver voglio potermi registrare in GoCare.                                                                                                                                                                      | 10.1               |
| CG-02 | Come caregiver voglio poter effettuare il login.                                                                                                                                                                         | 10.2               |
| CG-03 | Come caregiver voglio poter modificare i miei dati personali e di contatto.                                                                                                                                              | 8.2, 8.3           |
| CG-04 | Come caregiver voglio poter recuperare la password in caso di smarrimento.                                                                                                                                               | 10.3               |
| CG-05 | Come caregiver voglio poter eliminare il mio account.                                                                                                                                                                    | 10.4               |
| CG-14 | Come caregiver voglio poter registrare il prossimo trasporto del mio assistito, indicando la direzione (andata e ritorno, solo andata, solo ritorno) e il tipo di viaggio (visita, ricovero, dimissione, trasferimento). | 1.1, 1.2, 1.5, 1.6 |
| CG-17 | Come caregiver voglio poter scegliere una destinazione diversa dall'indirizzo abituale dell'assistito.                                                                                                                   | 1.3                |
| CG-18 | Come caregiver voglio poter salvare, modificare ed eliminare le mie destinazioni ricorrenti.                                                                                                                             | 8.4                |
| CG-23 | Come caregiver voglio poter annullare il viaggio, indicando il motivo dell'annullamento.                                                                                                                                 | 3.1, 3.3           |
| CG-25 | Come caregiver voglio sapere se il viaggio non può essere coperto.                                                                                                                                                       | 4.4                |
| CG-26 | Come caregiver voglio ricevere notifica quando il viaggio è preso in carico dall'associazione.                                                                                                                           | 4.4                |
| CG-27 | Come caregiver voglio essere avvisato tempestivamente se un viaggio già preso in carico viene annullato dall'associazione, così da poter cercare un'alternativa.                                                         | 4.6                |
| CG-28 | Come caregiver voglio poter ricevere notifica dei prossimi trasporti programmati.                                                                                                                                        | 4.3                |
| CG-29 | Come caregiver voglio poter vedere lo stato in tempo reale del viaggio dell'assistito preso in carico (posizione GPS esclusa, vedi Roadmap v1).                                                                          | 4.1, 4.2, 5.5      |
| CG-30 | Come caregiver voglio poter vedere lo stato della mia richiesta (in attesa, confermata, in esecuzione, conclusa, non coperta, annullata).                                                                                | 5.2                |
| CG-31 | Come caregiver voglio poter vedere i contatti dell'associazione che ha preso in carico il mio viaggio, per poterla contattare in caso di necessità, solamente dopo l'avvenuto pagamento.                                 | 5.3                |
| CG-32 | Come caregiver voglio poter avere uno storico delle visite fatte.                                                                                                                                                        | 5.4                |
| CG-33 | Come caregiver voglio poter registrare un trasporto ricorrente (es. dialisi ogni martedì), così da non dover ripetere la richiesta ogni volta. Rinviata alla v1.                                                         | 1.1                |
| CG-38 | Come caregiver voglio poter indicare per il ritorno una destinazione diversa da quella da cui sono partito.                                                                                                              | 1.7                |
| CG-39 | Come caregiver voglio poter aggiungere un assistito di cui mi prendo cura.                                                                                                                                               | 9.1                |
| CG-40 | Come caregiver voglio poter visualizzare l'elenco degli assistiti di cui mi occupo.                                                                                                                                      | 9.2                |
| CG-41 | Come caregiver voglio poter visualizzare i dati e i viaggi di un assistito che seguo.                                                                                                                                    | 9.3                |
| CG-42 | Come caregiver voglio poter modificare i dati di un assistito che seguo, dato che non può farlo da solo.                                                                                                                 | 9.4                |
| CG-43 | Come caregiver voglio poter collegare direttamente un altro caregiver a un assistito che già seguo, così che possa vederne dati e viaggi.                                                                                | 9.5                |
| CG-44 | Come caregiver voglio poter rimuovere un collegamento caregiver↔assistito, il mio o quello di un altro caregiver collegato.                                                                                              | 9.6                |

## 3.2 Stories associazione

| ID    | User story                                                                                                                                                                   | UC       |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------- |
| AS-02 | Come associazione voglio poter effettuare il login.                                                                                                                          | 10.2     |
| AS-03 | Come associazione voglio poter recuperare la password in caso di smarrimento.                                                                                                | 10.3     |
| AS-04 | Come associazione voglio poter eliminare il mio account.                                                                                                                     | 10.4     |
| AS-05 | Come associazione voglio poter visualizzare e modificare i dati, i contatti e i comuni coperti dalla mia associazione.                                                       | 8.5, 8.6 |
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

| ID    | User story                                                                                                                                  | UC   |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------- | ---- |
| AD-01 | Come amministratore GoCare voglio poter registrare l'associazione partner.                                                                  | 11.1 |
| AD-02 | Come amministratore GoCare voglio poter modificare i dati dell'associazione partner.                                                        | 11.2 |
| AD-03 | Come amministratore GoCare voglio poter monitorare le richieste all'associazione partner.                                                   | 11.3 |
| AD-04 | Come amministratore GoCare voglio poter eliminare l'associazione partner al termine della collaborazione.                                   | 11.4 |
| AD-05 | Come amministratore GoCare voglio poter gestire gli account CareGiver: ripristino account.                                                  | 11.5 |
| AD-06 | Come amministratore GoCare voglio poter visualizzare le associazioni partner e le rispettive informazioni (dati, stato account, trasporti). | 11.6 |

## 3.4 Matrice di copertura Use Case → User Stories

Verifica che ogni sotto-voce di Use Case abbia almeno una user story associata.

| Use Case        | User stories | Esito   |
| --------------- | ------------ | ------- |
| 1.1             | CG-14, CG-33 | Coperto |
| 1.2             | CG-14        | Coperto |
| 1.3             | CG-17        | Coperto |
| 1.5             | CG-14        | Coperto |
| 1.6             | CG-14        | Coperto |
| 1.7             | CG-38        | Coperto |
| 3.1             | CG-23        | Coperto |
| 3.2             | AS-17        | Coperto |
| 3.3             | CG-23, AS-17 | Coperto |
| 4.1             | CG-29        | Coperto |
| 4.2             | CG-29        | Coperto |
| 4.3             | CG-28        | Coperto |
| 4.4             | CG-25, CG-26 | Coperto |
| 4.6             | CG-27        | Coperto |
| 4.7             | AS-06        | Coperto |
| 4.9             | AS-18        | Coperto |
| 4.10            | AS-19        | Coperto |
| 5.1             | CG-32        | Coperto |
| 5.2             | CG-30        | Coperto |
| 5.3             | CG-31        | Coperto |
| 5.4             | CG-32        | Coperto |
| 5.5             | CG-29        | Coperto |
| 6.1             | AS-07        | Coperto |
| 6.2             | AS-07        | Coperto |
| 6.3             | AS-08        | Coperto |
| 6.4             | AS-09        | Coperto |
| 6.5             | AS-10        | Coperto |
| 7.1             | AS-11        | Coperto |
| 7.2             | AS-12        | Coperto |
| 8.1 / 8.2 / 8.3 | CG-03        | Coperto |
| 8.4             | CG-18        | Coperto |
| 8.5 / 8.6       | AS-05        | Coperto |
| 9.1             | CG-39        | Coperto |
| 9.2             | CG-40        | Coperto |
| 9.3             | CG-41        | Coperto |
| 9.4             | CG-42        | Coperto |
| 9.5             | CG-43        | Coperto |
| 9.6             | CG-44        | Coperto |
| 10.1            | CG-01        | Coperto |
| 10.2            | CG-02, AS-02 | Coperto |
| 10.3            | CG-04, AS-03 | Coperto |
| 10.4            | CG-05, AS-04 | Coperto |
| 11.1            | AD-01        | Coperto |
| 11.2            | AD-02        | Coperto |
| 11.3            | AD-03        | Coperto |
| 11.4            | AD-04        | Coperto |
| 11.5            | AD-05        | Coperto |
| 11.6            | AD-06        | Coperto |

---

# 4. Attori

| Attore                      | Descrizione                                           | Ruolo nel sistema                                                                                                                                                                                                                                                                         |
| --------------------------- | ----------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Assistito                   | Persona fragile destinataria del trasporto.           | Non ha un account e non interagisce mai con l'app: è un'anagrafica creata e gestita da uno o più caregiver. Può essere collegato a più caregiver contemporaneamente (relazione diretta, non mediata da un gruppo); ogni caregiver collegato vede tutti i suoi dati e tutti i suoi viaggi. |
| Caregiver                   | Familiare o assistente che gestisce l'assistito.      | Si registra come account autonomo, aggiunge e gestisce uno o più assistiti, richiede trasporti per loro conto, segue lo stato dei viaggi. Può essere collegato a più assistiti; più caregiver possono essere collegati allo stesso assistito.                                             |
| Associazione                | Ente di volontariato/soccorso che eroga il trasporto. | Riceve le richieste a sé destinate, accetta o rifiuta, gestisce i trasporti presi in carico e il loro stato. Non si registra autonomamente: è un'associazione partner registrata dall'amministratore GoCare.                                                                              |
| Operatore dell'associazione | Volontario che esegue materialmente il trasporto.     | Aggiorna lo stato del viaggio durante l'esecuzione tramite un link scoped al singolo trasporto, generato e inoltrato dall'associazione: non ha un account proprio e non fa login. Viene registrato come etichetta testuale sulla transizione di stato.                                    |
| Amministratore GoCare       | Chi gestisce la piattaforma.                          | Registra, modifica, monitora ed elimina le associazioni partner; gestisce il ripristino degli account caregiver (UC 11).                                                                                                                                                                  |
| GoCare (sistema)            | La piattaforma.                                       | Instrada le richieste alle associazioni competenti per comune, genera notifiche push ed e-mail, conserva lo storico.                                                                                                                                                                      |

---

# 5. Tipi di viaggio

| Tipo di viaggio                        | Direzione tipica                    | Scope            |
| -------------------------------------- | ----------------------------------- | ---------------- |
| Visita medica                          | Andata e ritorno                    | In scope v0      |
| Ricovero                               | Solo andata                         | In scope v0      |
| Dimissione                             | Solo ritorno                        | In scope v0      |
| Trasferimento                          | Solo andata (struttura → struttura) | In scope v0      |
| Trasporto sociale (spesa, posta, ecc.) | Variabile                           | Rinviato alla v1 |

Il tipo di viaggio determina il valore di default della direzione nel form dell'UC 1 (1.2 e 1.5), che resta comunque modificabile dall'utente. Il trasporto sociale è rinviato alla v1 (vedi Punti aperti).

---

# 6. Decisioni consolidate

- Dashboard interna per le associazioni presente e obbligatoria in V.0.
- Le disponibilità delle associazioni non sono gestite dalla piattaforma: le associazioni accettano le richieste secondo la propria disponibilità.
- Flusso API bidirezionale in V.1: tutte le chiamate sono in scrittura e ogni chiamata corrisponde a un cambio di stato.
- Colonne Trello: To do, Ready for development, In progress (una sola card alla volta), Ready for testing, In testing, Tested, Released.
- Solo i caregiver si registrano come account autonomi. L'assistito non ha un account: è un'anagrafica creata da un caregiver, senza login e senza possibilità di interazione propria. Il collegamento caregiver↔assistito è diretto e molti-a-molti (un caregiver può seguire più assistiti, un assistito può avere più caregiver), senza nessun concetto di "gruppo cura" a mediarlo. Aggiungere un assistito crea il collegamento subito attivo, senza consenso da richiedere (l'assistito non può darlo); collegare un **secondo caregiver** a un assistito già esistente richiede invece l'accettazione del caregiver invitato, perché a differenza dell'assistito è un utente con un proprio account. Un caregiver con un collegamento attivo verso un assistito vede tutti i suoi dati e tutti i suoi viaggi; se più caregiver sono collegati allo stesso assistito, vedono tutti tutto, senza distinzione fra chi lo ha aggiunto e chi può solo vedere. La registrazione pubblica è riservata ai caregiver; gli account associazione sono creati dall'amministratore.
- Il viaggio non è modificabile dopo la creazione (niente più UC "Modifica trasporto"): l'unica azione disponibile su un viaggio già registrato è l'annullamento. Se le condizioni del viaggio cambiano, il caregiver annulla e invia una nuova richiesta.
- Il rifiuto di una singola associazione non è uno stato della richiesta: la richiesta resta "in attesa" e visibile alle altre associazioni destinatarie. L'esito negativo aggregato è sempre "non coperta".
- Ogni evento genera una sola notifica, con i canali (push, e-mail) combinati sulla stessa voce: il centro notifiche non mostra righe duplicate per lo stesso evento.
- Gli indirizzi sono strutturati (via, civico, CAP, città, provincia) e vengono copiati sulla richiesta al momento della creazione: modificare o eliminare una destinazione salvata non altera i viaggi già registrati. Gli indirizzi di partenza, destinazione ed eventuale ritorno di un trasporto (`TripAddress`) portano in più latitudine e longitudine obbligatorie, necessarie al calcolo dei km (vedi sotto); gli altri indirizzi del dominio (domicilio, destinazioni salvate, sede dell'associazione) restano senza coordinate.
- Il calcolo dei km previsti è in v0: la richiesta porta un campo `DistanceKm`, calcolato tramite un servizio esterno di instradamento (GraphHopper, dietro l'interfaccia `IRoutingService`) a partire dalle coordinate di partenza e destinazione, con la tratta di rientro aggiunta per i viaggi "andata e ritorno". Resta rinviato alla v1 solo un filtro/colonna dedicato ai km nelle dashboard dell'associazione (vedi Roadmap v1).
- L'importo del pagamento del trasporto (UC 5.2) è calcolato automaticamente come `DistanceKm × 0,80 €/km`, con un minimo di 5 €: non è stabilito dall'associazione né negoziabile. L'incasso avviene sull'unico account Stripe di GoCare (Checkout standard), non su un account per associazione (Stripe Connect): la ridistribuzione dei fondi alle singole associazioni resta fuori dalla piattaforma ed è rinviata alla v1. Se il caregiver annulla un viaggio già pagato, il sistema rimborsa automaticamente l'intero importo tramite Stripe.
- L'instradamento delle richieste avviene per comune, non per provincia: ogni associazione dichiara l'elenco esplicito dei comuni che copre (il proprio e quelli limitrofi in cui è disposta a operare) e riceve solo le richieste con partenza in uno di essi. I comuni si scelgono dall'elenco ufficiale ISTAT e sono identificati dal codice ISTAT; il comune di partenza si ricava dall'indirizzo di partenza (città e provincia) della richiesta (vedi PA-03).
- Il pagamento del trasporto avviene tramite Stripe Checkout (pagina di pagamento ospitata da Stripe, esito confermato al backend solo via webhook, mai dal semplice ritorno dal browser). I contatti dell'associazione assegnataria diventano visibili al caregiver solo dopo l'avvenuto pagamento (CG-31).
- Le associazioni non si registrano autonomamente: sono associazioni partner registrate, modificate ed eliminate dall'amministratore GoCare (UC 11). La verifica dell'ente avviene fuori dalla piattaforma, prima della registrazione.
- Autenticazione e dominio vivono nello stesso database; l'account e il profilo caregiver sono due entità distinte che **condividono lo stesso identificatore**, con una foreign key reale fra loro (`Person.Id`/`Association.Id` → `Account.Id`, `OnDelete(Cascade)`). L'assistito non ha invece un account e non ha identificatore condiviso, perché non fa mai login. In v0 ogni caregiver di dominio ha esattamente un account: non si opera con caregiver non registrati.

---

# 7. Stack tecnologico

- Backend: C# / .NET 10, ASP.NET Core Web API con controller MVC
- Frontend: React Native
- Database: PostgreSQL, un unico database (`gocare`), accesso via EF Core + Npgsql
- Struttura della soluzione: un unico progetto (`GoCare`), non più tre progetti separati. Organizzato per cartelle a livello tecnico anziché per feature: `Controllers/`, `Dtos/`, `Models/`, `Services/`, `Data/`, più `Abstractions/`, `Errors/`, `Pagination/`, `Validation/` per il kernel trasversale. Dentro `Models/`, `Services/` e `Dtos/` resta la separazione fra area autenticazione e area dominio, come sottocartelle (`Auth/`, `Domain/`). Le interfacce di servizio sono state mantenute solo dove esiste più di un'implementazione o è pianificato uno swap concreto (es. l'invio e-mail); dove c'era un'unica implementazione senza swap previsto (servizi applicativi, orologio di sistema, utente corrente) sono state rimosse a favore della classe concreta.
- Notifiche: push applicative + e-mail di conferma
- Pagamenti: Stripe (Checkout + webhook)
- Instradamento/calcolo km: servizio esterno dietro interfaccia (`IRoutingService`), implementazione attuale GraphHopper

---

# 8. Perimetro versione BETA e suddivisione del lavoro

Versione BETA:

- Dashboard con login per i ruoli con account (caregiver, associazioni, amministratore GoCare). L'assistito non ha login: le sue schermate sono gestite dal caregiver che lo segue.
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

# 9. Tabella delle Route

## 9.1 Pagine dell'applicazione

Elenco completo delle pagine dell'applicazione, con path proposto, ruoli abilitati e presenza dell'header. Le pagine di autenticazione sono le uniche prive di header.

| Pagina                                                                                 | Route                             | Ruoli                    | Header | UC                 |
| -------------------------------------------------------------------------------------- | --------------------------------- | ------------------------ | ------ | ------------------ |
| Registrazione                                                                          | /register                         | Pubblico                 | NO     | 10.1               |
| Login                                                                                  | /login                            | Pubblico                 | NO     | 10.2               |
| Recupero password                                                                      | /forgot-password                  | Pubblico                 | NO     | 10.3               |
| Reimposta password                                                                     | /reset-password/:token            | Pubblico                 | NO     | 10.3               |
| Verifica e-mail                                                                        | /verify-email/:token              | Pubblico                 | NO     | 10.1               |
| Pagina principale associazione (dashboard nuove richieste + filtri)                    | /associazione/richieste           | Associazione             | SÌ     | 6.1, 6.2           |
| Dettaglio richiesta pendente                                                           | /associazione/richieste/:id       | Associazione             | SÌ     | 6.2, 6.3, 6.4, 6.5 |
| Pagina privata associazione (richieste accettate, info associazione)                   | /associazione/trasporti           | Associazione             | SÌ     | 7.1                |
| Dettaglio trasporto accettato                                                          | /associazione/trasporti/:id       | Associazione             | SÌ     | 7.1                |
| Generazione/rigenerazione link operatore                                               | /associazione/trasporti/:id/stato | Associazione             | SÌ     | 4.11               |
| Schermata operatore (avanzamento stato, via link, nessun login)                        | /operatore/:token                 | Pubblico (solo con link) | NO     | 4.11               |
| Storico trasporti associazione                                                         | /associazione/storico             | Associazione             | SÌ     | 7.3                |
| Pagina di modifica informazioni associazione                                           | /associazione/profilo/modifica    | Associazione             | SÌ     | 8.5, 8.6           |
| Profilo associazione (sola lettura)                                                    | /associazione/profilo             | Associazione             | SÌ     | 8.5                |
| Pagina principale caregiver (card degli assistiti)                                     | /assistiti                        | Caregiver                | SÌ     | 9.2                |
| Form nuovo assistito                                                                   | /assistiti/nuovo                  | Caregiver                | SÌ     | 9.1                |
| Dettaglio assistito (dati, caregiver collegati, viaggi)                                | /assistiti/:id                    | Caregiver                | SÌ     | 9.3, 9.4, 9.5, 9.6 |
| Form nuovo trasporto (per quale assistito, data, destinazione, tipo viaggio, contatti) | /viaggi/nuovo                     | Caregiver                | SÌ     | 1.1 – 1.8          |
| Dettaglio viaggio                                                                      | /viaggi/:id                       | Caregiver                | SÌ     | 3.1, 5.4           |
| Pagina "I miei viaggi" (prossimi viaggi + storico)                                     | /viaggi                           | Caregiver                | SÌ     | 5.1, 5.3           |
| Storico viaggi                                                                         | /viaggi/storico                   | Caregiver                | SÌ     | 5.3                |
| Pagina gestione dati personali account                                                 | /profilo                          | Caregiver                | SÌ     | 8.1, 8.2, 10.4     |
| Gestione destinazioni salvate                                                          | /profilo/destinazioni             | Caregiver                | SÌ     | 8.3                |
| Pagina pubblica associazione (contatti)                                                | /associazioni/:id                 | Caregiver                | SÌ     | 5.2                |
| Esito pagamento (ritorno da Stripe)                                                    | /viaggi/:id/pagamento             | Caregiver                | SÌ     | 5.3                |
| Centro notifiche                                                                       | /notifiche                        | Caregiver / Associazione | SÌ     | UC 4               |
| Pagina principale amministratore (elenco associazioni partner)                         | /admin/associazioni               | Amministratore           | SÌ     | 11.6               |
| Nuova associazione partner                                                             | /admin/associazioni/nuova         | Amministratore           | SÌ     | 11.1               |
| Dettaglio associazione partner (dati, stato account, trasporti, richieste)             | /admin/associazioni/:id           | Amministratore           | SÌ     | 11.3, 11.4, 11.6   |
| Modifica associazione partner                                                          | /admin/associazioni/:id/modifica  | Amministratore           | SÌ     | 11.2               |
| Gestione account caregiver                                                             | /admin/caregiver                  | Amministratore           | SÌ     | 11.5               |
| Impostazione password associazione (link di invito)                                    | /set-password/:token              | Pubblico                 | NO     | 11.1               |

Note sulle scelte: (a) "Reimposta password" e "Verifica e-mail" sono pagine pubbliche raggiunte da link e-mail, necessarie perché i flussi di registrazione e recupero credenziali si concludano; (b) la pagina "gestione stato viaggio" è dove l'**associazione** genera/rigenera il link per l'operatore, non dove l'operatore stesso lavora: l'operatore ha una schermata separata (`/operatore/:token`), pubblica ma utilizzabile solo con il link ricevuto, senza login e senza accesso al resto dell'app; (c) "Storico viaggi" e "Storico trasporti" possono essere realizzate come tab della pagina principale corrispondente anziché come route autonome: la scelta è indifferente sul piano funzionale; (d) il centro notifiche raccoglie lo storico delle notifiche dell'UC 4, che altrimenti sarebbero consultabili solo al momento della ricezione; (e) non esistono più pagine di modifica del viaggio né di gestione di un gruppo cura: la gestione degli assistiti si concentra nella pagina di dettaglio (UC 9), con collegamento diretto in v0 (l'invito con accettazione è previsto per la v1, §13).

## 9.2 Endpoint di back-end

Elenco degli endpoint che servono le pagine della tabella precedente, con il ruolo abilitato e lo Use Case di riferimento.

| Area    | Metodo e path                                                   | Ruoli                                 | UC        |
| ------- | --------------------------------------------------------------- | ------------------------------------- | --------- |
| Auth    | `POST /auth/register/user`                                      | Pubblico                              | 10.1      |
| Auth    | `POST /auth/verify-email`                                       | Pubblico                              | 10.1      |
| Auth    | `POST /auth/verify-email/resend`                                | Pubblico                              | 10.1      |
| Auth    | `POST /auth/login`                                              | Pubblico                              | 10.2      |
| Auth    | `POST /auth/refresh`                                            | Pubblico                              | 10.2      |
| Auth    | `POST /auth/logout`                                             | Pubblico (possesso del refresh token) | 10.2      |
| Auth    | `POST /auth/forgot-password`                                    | Pubblico                              | 10.3      |
| Auth    | `POST /auth/reset-password`                                     | Pubblico                              | 10.3      |
| Auth    | `POST /auth/change-email`                                       | Autenticato                           | 8.2       |
| Auth    | `DELETE /auth/account`                                          | Autenticato                           | 10.4      |
| Dominio | `POST /transports`                                              | Caregiver                             | 1.1 – 1.8 |
| Dominio | `GET /transports`                                               | Caregiver                             | 5.1, 5.3  |
| Dominio | `GET /transports/:id`                                           | Caregiver                             | 3.1, 5.4  |
| Dominio | `POST /transports/:id/cancel`                                   | Caregiver                             | 3.1       |
| Dominio | `GET /transports/:id/status-timeline`                           | Caregiver                             | 5.4       |
| Dominio | `GET /association/requests`                                     | Associazione                          | 6.1, 6.2  |
| Dominio | `GET /association/requests/:id`                                 | Associazione                          | 6.2       |
| Dominio | `POST /association/requests/:id/accept`                         | Associazione                          | 6.3       |
| Dominio | `POST /association/requests/:id/decline`                        | Associazione                          | 6.4       |
| Dominio | `GET /association/requests/:id/contacts`                        | Associazione                          | 6.5       |
| Dominio | `GET /association/transports`                                   | Associazione                          | 7.1       |
| Dominio | `GET /association/transports/:id`                               | Associazione                          | 7.1       |
| Dominio | `POST /association/transports/:id/operator-link`                | Associazione                          | 4.11      |
| Dominio | `GET /operatore/:token`                                         | Pubblico (solo con link)              | 4.11      |
| Dominio | `POST /operatore/:token/status`                                 | Pubblico (solo con link)              | 4.11      |
| Dominio | `POST /association/transports/:id/cancel`                       | Associazione                          | 3.2       |
| Dominio | `GET /association/history`                                      | Associazione                          | 7.3       |
| Dominio | `GET /association/profile` · `PUT /association/profile`         | Associazione                          | 8.5, 8.6  |
| Dominio | `GET /me/profile` · `PUT /me/profile`                           | Caregiver                             | 8.1, 8.2  |
| Dominio | `GET · POST · PUT · DELETE /me/destinations[/:id]`              | Caregiver                             | 8.3       |
| Dominio | `GET /associations/:id/contacts`                                | Caregiver (solo dopo il pagamento)    | 5.3       |
| Dominio | `POST /transports/:id/payment`                                  | Caregiver                             | 5.3       |
| Dominio | `POST /payments/stripe/webhook`                                 | Stripe (firma verificata)             | 5.3       |
| Dominio | `POST /assisted`                                                | Caregiver                             | 9.1       |
| Dominio | `GET /assisted`                                                 | Caregiver                             | 9.2       |
| Dominio | `GET /assisted/:id`                                             | Caregiver                             | 9.3       |
| Dominio | `PUT /assisted/:id`                                             | Caregiver                             | 9.4       |
| Dominio | `POST /assisted/:id/caregivers`                                 | Caregiver                             | 9.5       |
| Dominio | `DELETE /assisted/:id/caregivers/:caregiverId`                  | Caregiver                             | 9.6       |
| Dominio | `GET /notifications`                                            | Tutti                                 | UC 4      |
| Dominio | `POST /notifications/:id/read` · `POST /notifications/read-all` | Tutti                                 | UC 4      |
| Dominio | `GET /notifications/counters`                                   | Tutti                                 | §10.5     |
| Dominio | `POST /devices` · `DELETE /devices/:id`                         | Tutti                                 | UC 4      |
| Admin   | `POST /admin/associations`                                      | Amministratore GoCare                 | 11.1      |
| Auth    | `POST /auth/set-password`                                       | Pubblico (token di invito)            | 11.1      |
| Admin   | `PUT /admin/associations/:id`                                   | Amministratore GoCare                 | 11.2      |
| Admin   | `GET /admin/associations/:id/requests`                          | Amministratore GoCare                 | 11.3      |
| Admin   | `DELETE /admin/associations/:id`                                | Amministratore GoCare                 | 11.4      |
| Admin   | `GET /admin/caregivers` · `GET /admin/caregivers/:id`           | Amministratore GoCare                 | 11.5      |
| Admin   | `POST /admin/caregivers/:id/restore`                            | Amministratore GoCare                 | 11.5      |
| Admin   | `POST /admin/caregivers/:id/resend-verification`                | Amministratore GoCare                 | 11.5      |
| Admin   | `POST /admin/caregivers/:id/password-reset`                     | Amministratore GoCare                 | 11.5      |
| Admin   | `GET /admin/associations` · `GET /admin/associations/:id`       | Amministratore GoCare                 | 11.6      |

---

# 10. Definizione dell'Header

L'header è presente su tutte le pagine successive all'autenticazione. La struttura è la stessa per tutti i ruoli; cambiano le voci di navigazione centrali.

## 10.1 Struttura comune

| Posizione | Elemento                           | Comportamento                                                           |
| --------- | ---------------------------------- | ----------------------------------------------------------------------- |
| Sinistra  | Logo "GoCare"                      | Riporta alla pagina principale del ruolo.                               |
| Centro    | Voci di navigazione                | Variabili per ruolo (vedi 10.2 e 10.3).                                 |
| Destra    | Icona notifiche con badge numerico | Apre il centro notifiche. Il badge conta le notifiche non lette (UC 4). |
| Destra    | Avatar / menu utente               | Menu a tendina: Profilo, Impostazioni, Logout.                          |

## 10.2 Voci di navigazione – Caregiver

| Voce             | Route         | UC che la giustifica | Motivazione                                                           |
| ---------------- | ------------- | -------------------- | --------------------------------------------------------------------- |
| I miei assistiti | /assistiti    | 9.2                  | Pagina principale del ruolo: le card degli assistiti collegati.       |
| I miei viaggi    | /viaggi       | 5.1, 5.3             | Prossimi viaggi e storico.                                            |
| Nuovo trasporto  | /viaggi/nuovo | 1.1                  | Azione principale del prodotto: resa raggiungibile da ogni pagina.    |
| Profilo          | /profilo      | 8.1 – 8.3            | Dati personali, contatti, destinazioni salvate, eliminazione account. |

"Nuovo trasporto" può essere realizzata come pulsante di azione in evidenza anziché come voce di navigazione: è la funzione più frequente e sarebbe penalizzata se raggiungibile solo dalla pagina "I miei viaggi".

## 10.3 Voci di navigazione – Associazione

| Voce                 | Route                   | UC che la giustifica | Motivazione                                                                                                                                                                                            |
| -------------------- | ----------------------- | -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Richieste            | /associazione/richieste | 6.1, 6.2             | Dashboard delle richieste pendenti con filtri. È la pagina operativa principale dell'associazione; porta un badge con il numero di richieste non ancora valutate (UC 4.9).                             |
| Trasporti accettati  | /associazione/trasporti | 7.1                  | Vista dei trasporti presi in carico; da qui si accede alla gestione dello stato del viaggio (UC 4.11).                                                                                                 |
| Storico              | /associazione/storico   | 7.3                  | Viaggi conclusi, annullati e rifiutati. Separato dai trasporti attivi per non appesantire la vista operativa.                                                                                          |
| Profilo associazione | /associazione/profilo   | 8.5, 8.6             | Informazioni, contatti e comuni coperti dell'associazione. I contatti sono esposti agli utenti (UC 5.2) e i comuni determinano quali richieste arrivano, quindi devono essere aggiornabili facilmente. |

Le quattro voci del ruolo Associazione sono derivate dagli Use Case 6, 7 e 8: senza "Richieste" e "Trasporti accettati" le due dashboard previste dagli UC 6 e 7 non sarebbero raggiungibili.

## 10.4 Voci di navigazione – Amministratore

| Voce                 | Route               | UC che la giustifica | Motivazione                                                                       |
| -------------------- | ------------------- | -------------------- | --------------------------------------------------------------------------------- |
| Associazioni partner | /admin/associazioni | 11.1 – 11.4, 11.6    | Pagina principale del ruolo: elenco, registrazione, dettaglio e gestione partner. |
| Caregiver            | /admin/caregiver    | 11.5                 | Ricerca degli account caregiver e azioni di ripristino.                           |

## 10.5 Badge e contatori

| Elemento             | Ruolo        | Cosa conta                                                 | UC        |
| -------------------- | ------------ | ---------------------------------------------------------- | --------- |
| Icona notifiche      | Tutti        | Notifiche non lette                                        | 4.2 – 4.9 |
| Voce "Richieste"     | Associazione | Richieste pendenti non ancora valutate                     | 4.9, 6.1  |
| Voce "I miei viaggi" | Caregiver    | Viaggi con esito o cambio di stato non ancora visualizzato | 4.4       |

_Rimossa la riga sul contatore delle richieste di modifica pendenti su "Trasporti accettati": la modifica del viaggio non esiste più._

---

# 11. Flussi

Sequenza logica dei processi principali. I flussi descrivono solo la successione dei passi e i cambi di stato, senza indicazioni grafiche.

## 11.1 Registrazione

Registrazione del caregiver:

1. L'utente apre /register (la registrazione pubblica è riservata ai caregiver).
2. Compila il form e accetta l'informativa privacy.
3. Il sistema valida i dati e verifica che l'e-mail non sia già registrata.
4. L'account viene creato in stato "non verificato" e, contestualmente, viene creato il profilo caregiver con lo stesso identificatore dell'account.
5. Viene inviata l'e-mail di verifica con token a scadenza.
6. L'utente apre il link ricevuto: l'account passa in stato "attivo".
7. L'utente viene reindirizzato al login.

Registrazione di un'associazione partner (UC 11.1):

1. L'amministratore apre /admin/associazioni/nuova, compila i dati dell'associazione e conferma.
2. Il sistema crea account e profilo associazione e invia all'associazione l'e-mail di invito con il link /set-password/:token.
3. L'associazione imposta la password: l'account è attivo e l'associazione è subito operativa sulle richieste dei propri comuni.

Casi alternativi:

- E-mail già registrata → messaggio di errore sul campo, nessun account creato.
- Link di verifica scaduto → pagina con possibilità di richiedere un nuovo invio.
- Login con account non verificato → accesso negato con invito a verificare l'e-mail.
- Creazione dell'account riuscita ma creazione del profilo fallita → l'incoerenza (stesso database, due scritture separate non nella stessa transazione) viene recuperata da una riconciliazione, non lasciata all'utente.

## 11.2 Login

1. L'utente apre /login e inserisce e-mail e password.
2. Il sistema verifica le credenziali e lo stato dell'account.
3. In caso di esito positivo viene emesso il token di sessione e restituito il ruolo.
4. L'utente viene reindirizzato alla pagina principale del proprio ruolo: /associazione/richieste per l'associazione, /assistiti per il caregiver.
5. Vengono mostrati gli eventuali pop-up di notifica pendenti (conferma viaggio, disdetta).

Casi alternativi:

- Credenziali errate → messaggio generico, senza indicare quale campo è sbagliato.
- Superamento del numero di tentativi → blocco temporaneo dell'accesso.
- Associazione eliminata dall'amministratore → accesso negato.

## 11.3 Recupero password

1. Dalla pagina di login l'utente apre /forgot-password e inserisce la propria e-mail.
2. Il sistema mostra sempre lo stesso messaggio neutro, indipendentemente dall'esistenza dell'account.
3. Se l'account esiste, viene generato un token monouso a scadenza e inviata l'e-mail con il link di reset.
4. L'utente apre /reset-password/:token e imposta la nuova password.
5. Il sistema aggiorna la password, invalida il token e tutte le sessioni attive.
6. L'utente viene reindirizzato al login.

## 11.4 Richiesta di un nuovo trasporto

1. Il caregiver apre /viaggi/nuovo.
2. Seleziona l'assistito beneficiario del trasporto tra quelli a lui collegati.
3. Indica tipo di viaggio, direzione, data e orario; con "andata e ritorno" indica anche l'orario di ritorno.
4. Seleziona la destinazione dalla lista delle destinazioni salvate oppure ne inserisce una nuova; in questo secondo caso il sistema chiede se salvarla per il futuro. Se il ritorno è verso un indirizzo diverso da quello di partenza, lo indica come terzo indirizzo.
5. Indica i contatti di riferimento.
6. Il sistema valida i dati e la coerenza fra direzione, orari e indirizzi, copia gli indirizzi e i contatti sulla richiesta e la crea in stato "in attesa".
7. Il sistema ricava il comune di partenza dall'indirizzo di partenza, calcola le associazioni destinatarie, cioè le associazioni partner attive che includono quel comune fra i propri comuni coperti, e congela l'elenco sulla richiesta.
8. Il sistema invia la notifica di nuova richiesta alle associazioni destinatarie (UC 4.9).
9. La richiesta compare nella dashboard /associazione/richieste di ciascuna destinataria, con i relativi filtri.
10. Un'associazione apre il dettaglio e accetta: la richiesta passa in stato "confermata" e le viene assegnata l'associazione.
11. Il sistema notifica l'utente della presa in carico (UC 4.2) e abilita il pagamento.
12. Il caregiver paga il trasporto tramite Stripe; alla conferma del pagamento (webhook) il sistema rende visibili i contatti dell'associazione (UC 5.2).
13. La richiesta esce dalla dashboard delle pendenti di tutte le associazioni e compare in /associazione/trasporti dell'associazione assegnataria.

Casi alternativi:

- Un'associazione rifiuta: il rifiuto viene registrato per quella sola associazione, la richiesta resta "in attesa" e continua a essere visibile alle altre destinatarie.
- Il pagamento non va a buon fine o viene annullato: il viaggio resta confermato ma i contatti dell'associazione restano nascosti; il caregiver può riprovare.
- Due associazioni accettano contemporaneamente: vince la prima, la seconda riceve un messaggio di errore gestito dal controllo di concorrenza sulla richiesta.
- Nessuna associazione accetta entro la soglia definita _[DA VALIDARE]_: la richiesta passa in stato "non coperta" e l'utente riceve la notifica di esito negativo (UC 4.1).
- Nessuna associazione partner copre il comune di partenza: la richiesta nasce senza destinatari e viene dichiarata "non coperta".

Il viaggio, una volta creato, non è più modificabile: se qualcosa cambia, il caregiver lo annulla (UC 3) e invia una nuova richiesta.

## 11.6 Annullamento del trasporto

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

## 11.7 Ciclo di stato del viaggio

Stati della richiesta (dalla creazione alla presa in carico):

in attesa → confermata → in esecuzione → conclusa

Stati terminali alternativi: non coperta (nessuna associazione destinataria ha accettato), annullata (dall'utente o dall'associazione).

Il rifiuto di una singola associazione non è uno stato della richiesta: viene registrato come rifiuto di quella associazione e la richiesta resta "in attesa" per le altre destinatarie. L'unico esito negativo aggregato è "non coperta".

Stati di esecuzione del viaggio (UC 4.1), aggiornati dall'operatore tramite il link generato dall'associazione (UC 4.11):

| Stato               | Chi lo imposta | Significato                                                                                                              | Notifiche                                                                                      |
| ------------------- | -------------- | ------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------- |
| Non preso in carico | —              | Il viaggio è confermato ma l'operatore non ha ancora iniziato.                                                           | Nessuna notifica (stato iniziale).                                                             |
| Preso in carico     | Operatore      | L'operatore ha preso in carico l'assistito.                                                                              | Notifica push ai caregiver collegati: "L'assistito è stato preso in carico".                   |
| In arrivo           | Operatore      | Il mezzo è in viaggio verso la destinazione.                                                                             | Notifica push ai caregiver collegati.                                                          |
| In visita           | Operatore      | L'assistito è alla destinazione (visita, ricovero, esame).                                                               | Notifica push ai caregiver collegati.                                                          |
| In ritorno          | Operatore      | Viaggio di rientro in corso. Stato non attraversato dai viaggi di sola andata.                                           | Notifica push ai caregiver collegati.                                                          |
| Sospeso             | Operatore      | Guasto o imprevisto durante il viaggio. Il viaggio resta in carico all'associazione, che resta responsabile del rientro. | Notifica push immediata ai caregiver collegati, con e-mail (evento di esito).                  |
| Concluso            | Operatore      | Il viaggio è terminato. Stato terminale.                                                                                 | Notifica push ai caregiver collegati; il viaggio passa nello storico di utente e associazione. |

Regole di transizione:

- Gli stati si attraversano in sequenza; non sono ammessi salti in avanti né ritorni indietro, salvo "sospeso" (vedi sotto).
- Un viaggio di sola andata (ricovero, trasferimento) salta lo stato "in ritorno".
- Un viaggio di solo ritorno (dimissione) non attraversa lo stato "in visita".
- "Sospeso" è raggiungibile da qualunque stato di esecuzione attivo (preso in carico, in arrivo, in visita, in ritorno) e da esso si riprende dallo stesso stato in cui ci si era fermati: non è uno stato terminale, è una pausa registrata nel percorso.
- Ogni transizione registra stato, timestamp, associazione e operatore che l'ha effettuata, come nuova riga di un registro append-only: lo stato corrente è la transizione più recente e nessuna riga viene modificata.
- I due assi di stato avanzano insieme: alla prima transizione di esecuzione la richiesta passa da "confermata" a "in esecuzione"; con "concluso" passa a "conclusa".
- Il passaggio a "concluso" chiude il viaggio e lo rende non più modificabile (non che lo fosse comunque: il viaggio non è mai modificabile dopo la creazione, in nessuno stato).
- Le notifiche di cambio stato sono solo push, salvo "sospeso" (push + e-mail, trattato come evento di esito data la sua criticità): le altre e-mail restano riservate a presa in carico, disdetta, mancata copertura.

## 11.8 Gestione degli assistiti

Aggiunta di un assistito:

1. Il caregiver apre /assistiti e preme "aggiungi assistito".
2. Inserisce nome, cognome, data di nascita, telefono e conferma.
3. Il sistema crea l'anagrafica dell'assistito e il collegamento con il caregiver nello stesso passaggio: nessun invito, nessuna accettazione.
4. L'assistito compare come nuova card nella pagina principale.

Collegamento di un ulteriore caregiver (v0, diretto — vedi §13 per il flusso a invito previsto in v1):

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

# 12. Cosa manca / Punti aperti

Per ciascun punto è indicata un'opzione consigliata, che resta da validare con il team prima dello sviluppo. Dove l'opzione consigliata è già stata recepita dal modello dati della v0 il punto lo dichiara, ma la decisione non è per questo chiusa: dove incide sui flussi resta segnalata con l'etichetta [DA VALIDARE].

Riepilogo:

| ID    | Punto aperto                                                     | Impatto | Stato              |
| ----- | ---------------------------------------------------------------- | ------- | ------------------ |
| PA-01 | Modello di prenotazione: richiesta utente o slot offerti         | Alto    | Aperto             |
| PA-02 | Soglia oltre la quale una richiesta è "non coperta"              | Alto    | Aperto             |
| PA-03 | Visibilità delle richieste: tutte le associazioni o per area     | Alto    | Deciso: per comune |
| PA-04 | Esposizione dei dati di contatto dell'assistito all'associazione | Alto    | Aperto             |
| PA-05 | Accreditamento delle associazioni                                | Medio   | Superato (UC 11)   |
| PA-06 | Politica di conservazione dei dati dopo l'eliminazione account   | Basso   | Aperto             |
| PA-07 | Pagamento con Stripe: importo, destinatario fondi, rimborsi      | Alto    | Deciso (UC 5.2)    |

## PA-01 Modello di prenotazione: richiesta dell'utente o slot offerti dalle associazioni

Problema: È aperta la domanda "l'utente richiede il trasporto, o le associazioni offrono disponibilità e l'utente seleziona lo slot?". È la decisione più strutturante del prodotto: cambia il modello dati, il flusso principale (11.4), il contenuto della dashboard associazione e il significato stesso degli stati della richiesta.

Opzione consigliata: Mantenere il modello a richiesta, coerente con l'intero documento e con la decisione che GoCare non gestisce le disponibilità delle associazioni. Il modello a slot richiederebbe alle associazioni di pubblicare in anticipo i turni disponibili, un onere organizzativo che la maggior parte delle associazioni di volontariato non è in grado di sostenere. Tutto il documento è scritto assumendo il modello a richiesta: se il team scegliesse il modello a slot, gli UC 1, 4, 5 e 6 andrebbero riscritti.

Stato in v0: Il modello dati realizzato è quello a richiesta.

## PA-02 Soglia oltre la quale una richiesta è considerata "non coperta"

Problema: La story CG-25 ("voglio sapere se il viaggio non può essere coperto") e la notifica dell'UC 4.4 presuppongono un momento in cui il sistema dichiara fallita la ricerca di copertura, ma nessuna regola lo definisce. Senza questa regola l'utente resta indefinitamente in attesa e non può cercare alternative.

Opzione consigliata: Dichiarare la richiesta "non coperta" al verificarsi della prima delle due condizioni: tutte le associazioni destinatarie hanno rifiutato, oppure mancano meno di 24 ore alla data del viaggio e nessuna l'ha accettata. La seconda condizione è quella che conta davvero: lascia all'utente il tempo di organizzarsi diversamente.

Stato in v0: Aperto. Il modello dati registra i rifiuti per associazione e la lista congelata delle destinatarie, quindi entrambe le condizioni sono calcolabili; manca la scelta della soglia.

## PA-03 Visibilità delle richieste: tutte le associazioni o per area geografica

Problema: La dashboard dell'UC 6 mostra "tutte le richieste di trasporto dei caregiver", ma non è specificato se ogni associazione veda tutte le richieste della piattaforma o solo quelle del proprio territorio.

Opzione consigliata: Introdurre un'area di operatività nel profilo dell'associazione (UC 8.5) e mostrare a ciascuna associazione solo le richieste con partenza entro la propria area. In fase di test, con poche associazioni pilota, la regola può essere disattivata; senza di essa, però, il prodotto non è scalabile oltre una singola città.

Decisione (rivista con il docente): l'area di operatività è definita per **comune**, non per provincia. La provincia si è rivelata troppo ampia: un'associazione con sede ad Assisi non opera su tutta la provincia di Perugia, ma sul proprio comune e su quelli limitrofi.

Regole:

- L'associazione dichiara un **elenco esplicito** di comuni coperti: il proprio e quelli limitrofi in cui è disposta a operare. Esempio: un'associazione di Assisi può coprire Assisi, Bastia Umbra, Perugia, Foligno e Valfabbrica; una richiesta con partenza da uno di questi comuni le arriva, una con partenza da Spoleto no.
- Non c'è un calcolo automatico per distanza o per confini: la scelta resta all'associazione (o all'amministratore, UC 11.1 e 11.2), che conosce il proprio territorio e i propri mezzi.
- I comuni si scelgono dall'elenco ufficiale ISTAT già presente nella piattaforma e sono identificati dal **codice ISTAT** (ad esempio Assisi `054001`), non dal nome: il nome non è univoco (esistono comuni omonimi in province diverse) e può essere scritto in modi diversi.
- Il comune di partenza della richiesta si ricava dall'indirizzo di partenza (città e sigla della provincia), già validato contro lo stesso elenco ufficiale.
- L'elenco delle destinatarie viene congelato sulla richiesta alla creazione: modificare i comuni coperti vale solo per le richieste future.
- Nessuna associazione copre il comune di partenza → la richiesta nasce senza destinatarie ed è "non coperta" (11.4).

Stato in v0: decisione presa; il modello dati usa ancora le province (`CoveredProvinces`) ed è in corso di adeguamento. Il dettaglio tecnico (tabella dei comuni coperti, contratto delle API, migrazione dei dati esistenti) è in `RECAP_BACKEND.md`, §5.

## PA-04 Esposizione dei dati di contatto dell'assistito all'associazione

Problema: L'associazione deve poter contattare chi ha prenotato, ma non è definito a quali dati acceda, da quale momento e con quale base giuridica. Il tema riguarda persone fragili e dati sanitari indiretti (il tipo di viaggio rivela la natura della prestazione).

Opzione consigliata: Mostrare nella fase di valutazione della richiesta (UC 6.1, 6.2) solo i dati necessari a decidere: iniziali o nome, fascia oraria, indirizzo di partenza a livello di via, destinazione, esigenze di mobilità. Rendere visibili i recapiti completi solo dopo l'accettazione (UC 6.5), tracciando gli accessi. Va inoltre redatta un'informativa privacy, richiamata in fase di registrazione (UC 10.1).

Stato in v0: Il modello dati recepisce la parte di tracciamento — ogni accesso ai contatti registra richiesta, associazione, tipo di dato (richiedente o beneficiario) e istante — mentre la soglia di visibilità dei dati minimi resta da definire.

## PA-05 Accreditamento delle associazioni

Problema: Un account associazione accede ai dati personali di persone fragili e può prendere in carico trasporti sanitari, quindi non può essere aperto liberamente da chiunque.

Decisione: Le associazioni non si registrano autonomamente. Sono associazioni partner, verificate da GoCare fuori dalla piattaforma e registrate dall'amministratore (UC 11.1), che le gestisce per tutta la durata della collaborazione (UC 11.2 – 11.4, 11.6).

Stato in v0: Superato. Lo stato di accreditamento (in attesa / accreditata / rifiutata) e gli endpoint di accredito/rifiuto presenti nel codice non servono più e vanno rimossi o riconvertiti.

## PA-06 Conservazione dei dati dopo l'eliminazione dell'account

Problema: L'UC 10 prevede l'eliminazione dell'account ma non dice cosa accada ai viaggi conclusi presenti nello storico dell'associazione, che potrebbe averne bisogno per rendicontazione, né a un assistito che resta senza nessun caregiver collegato.

Opzione consigliata: Anonimizzare i dati personali del caregiver mantenendo il viaggio nello storico dell'associazione con i soli dati di servizio (data, tratta, esito). Per l'assistito senza più caregiver collegati: mantenerne l'anagrafica (non è mai stata un account, non richiede anonimizzazione ai fini dell'accesso) ma segnalarla come irraggiungibile, in attesa che un altro caregiver la ricolleghi o che venga eliminata con un'azione esplicita. Da verificare con i requisiti di rendicontazione delle associazioni coinvolte.

Stato in v0: Il modello dati recepisce parzialmente l'opzione consigliata — il caregiver porta una data di cancellazione e una data di anonimizzazione distinte, e i viaggi conservano indirizzi e contatti copiati al momento della richiesta — ma la politica di retention, e il comportamento per l'assistito rimasto senza caregiver, restano da confermare.

## PA-07 Pagamento del trasporto con Stripe

Problema: La story CG-31 rende visibili i contatti dell'associazione solo dopo l'avvenuto pagamento; andavano definiti l'importo, chi lo stabilisce, chi riceve i fondi e cosa succede in caso di annullamento.

Decisione: Pagamento tramite Stripe Checkout dopo la presa in carico, con conferma solo via webhook. Importo automatico, calcolato come `DistanceKm × 0,80 €/km` con un minimo di 5 €: non è stabilito dall'associazione né negoziabile. I fondi affluiscono sull'unico account Stripe di GoCare (Checkout standard): la ridistribuzione diretta alle associazioni (Stripe Connect) è rinviata alla v1. Se il caregiver annulla un viaggio già pagato, il sistema rimborsa automaticamente l'intero importo; il percorso di rimborso per la disdetta dell'associazione dopo il pagamento resta da realizzare con la stessa logica.

Stato in v0: Deciso e implementato. Il modello dati include l'entità `Payment` (importo, identificativo Stripe, stato, data), collegata al trasporto; il calcolo della distanza avviene tramite un servizio esterno di instradamento (GraphHopper) dietro un'interfaccia (`IRoutingService`) che permette di cambiare fornitore senza impatti sul resto del codice.

---

# Priorità

## 1 FEAUTURE V0 ESSENZIALI

- Autenticazione
-
- Crea richiesta trasporto
- Dettaglio trasporto
- Elimina trasporto
- Profilo e gestione informazioni
- Dashboard visualizza viaggi caregiver

## 2 FEAUTURE POSSIBILE IMPLEMENTAZIONE

- modifica trasporto
-

# 13. Roadmap v1

Punti valutati e deliberatamente rinviati oltre la v0, raccolti qui in un solo posto invece che sparsi fra i singoli Use Case. Nessuno di questi introduce debito tecnico nella v0: sono funzionalità assenti, non abbozzate a metà. Non in questa lista perché **decisi per la v0** (non rinviati): lo stato "sospeso" per guasti/imprevisti e il link scoped al trasporto per l'operatore — entrambi fanno parte del modello v0 (UC 4.11, §11.7).

## Invito con consenso per il collegamento di un secondo caregiver

Il modello v0 collega un secondo caregiver a un assistito in modo diretto e immediato, senza che possa accettare o rifiutare — a differenza dell'assistito (che non ha un account e non può comunque esprimere consenso), il caregiver invitato è un utente con un proprio account e potrebbe non voler essere associato alla cura di quell'assistito. In v1: sostituire il collegamento diretto (UC 9.5) con un invito da accettare/rifiutare, condivisibile anche fuori dall'app (e-mail, WhatsApp). Il link userà l'`Id` di `AssistedPerson` (già un Guid, non indovinabile) invece di un token dedicato, perché la prova di identità è il login del caregiver invitato, non il possesso del link — il server verifica che l'utente autenticato corrisponda al collegamento in attesa.

## Modifica della richiesta di viaggio

L'UC "Modifica trasporto" non è presente in v0: dopo la creazione, l'unica azione su un viaggio è l'annullamento. Semplificazione deliberata per lanciare prima, non un giudizio definitivo sulla necessità della funzione. In v1 il disegno riparte da zero: una possibile base di partenza è una richiesta di modifica a data/orario/destinazione, con approvazione dell'associazione se il viaggio è già confermato — ma va valutata alla luce dell'uso reale della v0: potrebbe bastare annullamento+nuova richiesta, o servire una versione più snella senza approvazione per i cambi minori. Se questo punto viene ripreso, va affrontata anche la domanda su cosa succede quando l'associazione rifiuta una modifica proposta (esito, opzioni per l'utente).

## Filtri per chilometraggio nelle dashboard associazione

Le dashboard dell'associazione (UC 6, UC 7) non hanno ancora un filtro o una colonna dedicata ai km del viaggio, pur essendo il campo (`DistanceKm`) già presente nel modello dati dalla v0 (vedi §6 "Decisioni consolidate" e PA-07).

## Trasporto sociale

Nuovo tipo di viaggio (spesa, posta, commissioni varie), oltre ai quattro sanitari della v0 (visita, ricovero, dimissione, trasferimento). Non introduce complessità tecnica di per sé, ma cambia il posizionamento del prodotto e il tipo di associazioni da coinvolgere — da riproporre dopo la validazione sul trasporto sanitario.

## Trasporti ricorrenti

Possibilità di registrare un trasporto che si ripete (es. dialisi ogni martedì) senza rifare la richiesta ogni volta. Escluso dalla v0 perché la ricorrenza implica generazione automatica delle occorrenze, gestione delle eccezioni sulla singola data e annullamento in blocco: è un sottosistema, non un campo del form.

## Posizione GPS del mezzo

Tracciamento in tempo reale della posizione, oltre allo stato testuale con timeline già presente in v0. Richiede tracciamento continuo del mezzo, consenso esplicito del volontario e trattamento di dati di localizzazione; lo stato testuale copre già l'esigenza di ridurre le chiamate all'associazione durante il viaggio.

## Flusso API bidirezionale

Tutte le chiamate in scrittura, ogni chiamata corrisponde a un cambio di stato — alternativa al modello REST misto lettura/scrittura della v0. Impatto architetturale ampio, non solo funzionale.
