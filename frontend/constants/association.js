// Etichette e helper delle schermate associazione (mockup "Mockup_associazione.html").
import { C } from "./caregiver"

// Stato della richiesta lato associazione: label + colore come nel mockup
export const assocStatus = (status) =>
    ({
        Pending: { label: "Da valutare", color: C.waiting, dot: C.waitingDot },
        Confirmed: { label: "Confermato", color: C.success, dot: C.success },
        InProgress: { label: "In esecuzione", color: C.primary, dot: C.primary },
        Completed: { label: "Concluso", color: C.success, dot: C.success },
        Cancelled: { label: "Annullato", color: C.cancelled, dot: C.cancelled },
        NotCovered: { label: "Non coperto", color: C.cancelled, dot: C.cancelled },
    })[status] ?? { label: status, color: C.muted, dot: C.muted }

export const ACCREDITATION = {
    Accredited: { label: "Associazione accreditata", color: C.success },
    Pending: { label: "In attesa di accreditamento", color: C.waiting },
    Rejected: { label: "Accreditamento rifiutato", color: C.cancelled },
}

// Etichette degli stati registrati dall'operatore (ETripTransitionStatus)
export const EXECUTION_LABELS = {
    InCharge: "Preso in carico",
    Arriving: "In arrivo",
    OnSite: "In visita",
    Returning: "In ritorno",
    Suspended: "Sospeso",
    Completed: "Concluso",
}

// Codice breve mostrato sulle card al posto del codice "GC-1024" del mockup
export const shortId = (id) => `#${String(id).slice(0, 8).toUpperCase()}`

// Raggruppa per giorno con le etichette "Oggi · 7 novembre" / "Domani · 8 novembre"
export const dayHeading = (iso) => {
    const d = new Date(iso)
    const key = (x) => `${x.getFullYear()}-${x.getMonth()}-${x.getDate()}`
    const today = new Date()
    const tomorrow = new Date()
    tomorrow.setDate(today.getDate() + 1)
    const label = d.toLocaleDateString("it-IT", { day: "numeric", month: "long" })
    if (key(d) === key(today)) return `Oggi · ${label}`
    if (key(d) === key(tomorrow)) return `Domani · ${label}`
    return label
}

// I filtri restano impostati finché l'app è aperta, come nel mockup
export const listFilters = { richieste: {}, trasporti: {}, storico: {} }

// Link operatore generati in questa sessione (il backend non ha un elenco dei link)
export const operatorLinks = {}
