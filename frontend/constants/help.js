// Contenuti della guida caregiver, allineati ai flussi disponibili nell'app.
export const FAQ = [
    { id: 'confirmation', question: 'La richiesta è già confermata?', answer: 'Dopo l’invio, la richiesta è in attesa di approvazione. Il trasporto è confermato solo quando un’associazione lo accetta. Puoi seguire gli aggiornamenti in Trasporti e nelle notifiche.' },
    { id: 'assisted', question: 'Come aggiungo un assistito?', answer: 'Apri Assistiti e scegli “Registra assistito”. Inserisci i dati della persona da accompagnare e conferma. Puoi aggiungere un assistito anche durante una nuova richiesta di trasporto.' },
    { id: 'cancel', question: 'Posso annullare un trasporto?', answer: 'Puoi annullare una richiesta in attesa o confermata dal suo dettaglio. Se il trasporto è già in corso, contatta l’associazione che lo sta effettuando.' },
    { id: 'verification', question: 'Perché il mio account non è attivo?', answer: 'Dopo la registrazione devi confermare l’indirizzo email tramite il link di verifica. Controlla anche la posta indesiderata. Se il link non funziona o non arriva, contatta il gestore di GoCare.' },
    { id: 'profile', question: 'Perché devo completare il profilo?', answer: 'Nome, cognome, telefono e data di nascita servono per identificare il referente e organizzare il trasporto. Puoi aggiornarli dal profilo o completare i dati durante la richiesta.' },
    { id: 'direction', question: 'Quando posso scegliere andata e ritorno?', answer: 'L’andata e ritorno è disponibile per le visite mediche. Il ritorno avviene appena finita la visita, senza indicare un orario. Ricovero, dimissione e trasferimento prevedono solo andata. La destinazione del ritorno è inizialmente l’indirizzo di partenza; puoi sceglierne un altro.' },
    { id: 'caregiver', question: 'Posso condividere un assistito con un altro caregiver?', answer: 'Apri il dettaglio dell’assistito e scegli “Aggiungi caregiver”. Segui le indicazioni per invitarlo. I caregiver collegati possono consultare l’assistito e organizzare i suoi trasporti.' },
    { id: 'remove', question: 'Cosa succede se elimino un assistito?', answer: 'L’assistito viene rimosso dalla tua lista. Se sei l’unico caregiver, viene archiviato conservando lo storico: prima devi annullare eventuali trasporti in attesa o confermati e attendere la conclusione di quelli in corso. Se ci sono altri caregiver, resta disponibile nelle loro liste.' },
]

export const HELP_TOPICS = {
    guide: {
        title: 'Come funziona GoCare',
        intro: 'Dalla prima registrazione alla conferma del trasporto.',
        sections: [
            { title: '1. Prepara il tuo profilo', text: 'Conferma la tua email e completa i dati personali e il numero di telefono del referente.' },
            { title: '2. Registra l’assistito', text: 'Nella sezione Assistiti aggiungi la persona da accompagnare, i suoi recapiti e le eventuali note utili.' },
            { title: '3. Invia la richiesta', text: 'Apri Nuovo trasporto. Scegli il servizio, data e ora, indirizzi e assistito. Controlla il riepilogo prima di inviare.' },
            { title: '4. Attendi la conferma', text: 'L’invio non garantisce la disponibilità del servizio. Un’associazione deve accettare la richiesta. Consulta Trasporti e le notifiche per gli aggiornamenti.' },
        ],
    },
    states: {
        title: 'Stato delle richieste', intro: 'Cosa significa lo stato che vedi nei tuoi trasporti.',
        sections: [
            { title: 'In attesa di approvazione', text: 'La richiesta è stata inviata alle associazioni. Nessuna ha ancora confermato il servizio.' },
            { title: 'Trasporto confermato', text: 'Un’associazione ha accettato la richiesta. Apri il dettaglio per consultare le informazioni disponibili.' },
            { title: 'In corso', text: 'Il servizio è iniziato. Puoi seguirne gli aggiornamenti nel dettaglio del trasporto.' },
            { title: 'Trasporto concluso', text: 'L’accompagnamento è terminato. Il viaggio rimane nello storico.' },
            { title: 'Richiesta annullata', text: 'La richiesta è stata annullata e il servizio non verrà effettuato.' },
            { title: 'Trasporto non disponibile', text: 'Nessuna associazione ha confermato la disponibilità per questa richiesta.' },
        ],
    },
    payments: {
        title: 'Contributi e pagamenti', intro: 'Le informazioni da controllare prima di pagare.',
        sections: [
            { title: 'Come viene calcolato il contributo?', text: 'Il contributo viene calcolato sulla distanza stradale del percorso, con una tariffa per chilometro e un importo minimo. Per l’andata e ritorno vengono considerate entrambe le tratte. La stima del modulo può differire dal percorso stradale calcolato dal servizio.' },
            { title: 'Quando posso pagare?', text: 'L’invio della richiesta non richiede un pagamento. Per un trasporto confermato puoi accedere al pagamento online dal dettaglio, quando disponibile. Controlla l’importo nella pagina di pagamento prima di confermare.' },
            { title: 'Come so se il pagamento è riuscito?', text: 'Il pagamento è confermato nell’app dopo che il servizio di pagamento ne comunica l’esito. Se lo stato non si aggiorna, evita di pagare nuovamente e contatta l’assistenza.' },
            { title: 'Se annullo un viaggio già pagato?', text: 'Per un viaggio annullabile già pagato, l’annullamento prevede il rimborso integrale tramite il servizio di pagamento. In caso di errore, verifica lo stato del viaggio e contatta l’assistenza prima di ripetere il pagamento.' },
        ],
    },
}

// Solo recapiti e documenti reali: nessun indirizzo di assistenza o testo legale inventato.
export const HELP_CONTACTS = {
    email: (process.env.EXPO_PUBLIC_SUPPORT_EMAIL || '').trim(),
    privacyUrl: (process.env.EXPO_PUBLIC_PRIVACY_URL || '').trim(),
    termsUrl: (process.env.EXPO_PUBLIC_TERMS_URL || '').trim(),
}
