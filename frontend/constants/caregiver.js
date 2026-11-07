// Palette e helper condivisi dalle schermate caregiver (mockup "Mockup_Caregiver.html").
import { Colors } from "./Colors"

export const C = {
    primary: Colors.primary,
    danger: "#c43c4c",
    page: "#e5e4ec",
    card: "#d6d5e1",
    cardSelected: "#d2ccdf",
    nav: "#e8e7ef",
    text: "#201e2b",
    textStrong: "#454152",
    muted: "#625f72",
    faint: "#9a97a8",
    rule: "#bfb9ce",
    initialsBg: "#c7bfd9",
    avatarBg: "#c4bbd8",
    stepIdle: "#c8c4d5",
    waiting: "#805b0c",
    waitingDot: "#b58009",
    waitingSoft: "#cfc0a1",
    waitingBorder: "#ab7b15",
    success: "#28734d",
    successSoft: "#e3f6e8",
    cancelled: "#b53e4c",
    white: "#ffffff",
}

// ---- Enum backend (serializzati come stringhe) -> etichette italiane ----

export const TRIP_TYPES = [
    { value: "Visit", label: "Visita medica", short: "Visita", description: "Per una visita o un esame.", icon: "stethoscope" },
    { value: "Hospitalization", label: "Ricovero", short: "Ricovero", description: "Per entrare in ospedale.", icon: "hospital-box-outline" },
    { value: "Discharge", label: "Dimissione", short: "Dimissione", description: "Per lasciare l’ospedale.", icon: "home-outline" },
    { value: "Transfer", label: "Trasferimento", short: "Trasferimento", description: "Da una struttura a un’altra.", icon: "swap-horizontal" },
]

export const tripTypeShort = (value) => TRIP_TYPES.find((t) => t.value === value)?.short ?? "Trasporto"
export const tripTypeLabel = (value) => TRIP_TYPES.find((t) => t.value === value)?.label ?? "Trasporto"

// Ricovero e Dimissione ammettono solo un viaggio OneWay (vedi NewTransportValidator)
export const ONE_WAY_ONLY = ["Hospitalization", "Discharge"]

export const directionLabel = (value) => (value === "RoundTrip" ? "Andata e ritorno" : "Solo andata")

const TERMINAL = ["Completed", "Cancelled", "NotCovered"]
export const isTerminal = (status) => TERMINAL.includes(status)
export const isCancellable = (status) => status === "Pending" || status === "Confirmed"

const TRANSITION_LABELS = {
    InCharge: "In carico",
    Arriving: "In arrivo",
    OnSite: "In visita",
    Returning: "In ritorno",
    Suspended: "Sospeso",
    Completed: "Concluso",
}

// Stato di una richiesta come lo mostra il mockup (rqState):
// kind = waiting | confirmed | active | cancelled
export const tripState = (status, lastTransition) => {
    switch (status) {
        case "Pending":
            return {
                kind: "waiting",
                label: "In attesa di approvazione",
                text: "La richiesta è stata inviata alle associazioni. Nessuna l’ha ancora accettata: il trasporto non è ancora confermato.",
                cardText: "Un’associazione deve ancora approvare la richiesta.",
            }
        case "Confirmed":
            return {
                kind: "confirmed",
                label: "Trasporto confermato",
                text: "Un’associazione ha accettato la richiesta e ha confermato l’accompagnamento per la data e l’ora indicate.",
            }
        case "Completed":
            return { kind: "confirmed", label: "Trasporto concluso", text: "L’accompagnamento è terminato." }
        case "Cancelled":
            return { kind: "cancelled", label: "Richiesta annullata", text: "Questa richiesta è stata annullata." }
        case "NotCovered":
            return {
                kind: "cancelled",
                label: "Trasporto non disponibile",
                text: "Nessuna associazione ha confermato la disponibilità per questa richiesta.",
            }
        default:
            return {
                kind: "active",
                label: TRANSITION_LABELS[lastTransition] ?? "In corso",
                text: "Il trasporto è stato accettato. Qui puoi seguire gli aggiornamenti dell’associazione.",
            }
    }
}

// Etichetta breve usata nelle liste (dettaglio assistito)
export const shortStatus = (status) =>
    ({
        Pending: "In attesa",
        Confirmed: "Confermato",
        InProgress: "In corso",
        Completed: "Concluso",
        Cancelled: "Annullato",
        NotCovered: "Non coperto",
    })[status] ?? status

export const stateColor = (kind) =>
    ({ waiting: C.waiting, confirmed: C.success, active: C.success, cancelled: C.cancelled })[kind] ?? C.primary

export const stateDot = (kind) => (kind === "waiting" ? C.waitingDot : stateColor(kind))

// ---- Date ----

const pad = (n) => String(n).padStart(2, "0")

export const toDate = (value) => (value instanceof Date ? value : new Date(value))

export const formatTime = (value) => {
    const d = toDate(value)
    return `${pad(d.getHours())}:${pad(d.getMinutes())}`
}

export const formatDayMonth = (value) => toDate(value).toLocaleDateString("it-IT", { day: "numeric", month: "long" })

export const formatFullDate = (value) =>
    toDate(value).toLocaleDateString("it-IT", { day: "numeric", month: "long", year: "numeric" })

export const formatWeekday = (value) => {
    const d = toDate(value)
    const s = d.toLocaleDateString("it-IT", { weekday: "long", day: "numeric", month: "long" })
    return s.charAt(0).toUpperCase() + s.slice(1)
}

export const formatNumericDate = (value) => {
    const d = toDate(value)
    return `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()}`
}

// "1978-05-14" -> "14/05/1978"
export const formatIsoDate = (iso) => (iso ? String(iso).split("-").reverse().join("/") : "")

export const monthShort = (value) =>
    toDate(value).toLocaleDateString("it-IT", { month: "short" }).replace(".", "").toUpperCase()

export const ageFrom = (birthDate) => {
    if (!birthDate) return ""
    const [y, m, d] = String(birthDate).split("-").map(Number)
    const now = new Date()
    let age = now.getFullYear() - y
    if (now.getMonth() + 1 < m || (now.getMonth() + 1 === m && now.getDate() < d)) age--
    return age >= 0 && age <= 120 ? `${age} anni` : ""
}

export const toIsoDay = (date) => `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`

// ---- Persone e indirizzi ----

export const initialsFor = (name = "", surname = "") => {
    const parts = `${name} ${surname}`.trim().split(/\s+/).filter(Boolean)
    if (parts.length === 0) return ""
    return ((parts[0][0] ?? "") + (parts.length > 1 ? parts[parts.length - 1][0] : "")).toUpperCase()
}

export const fullName = (p) => [p?.name, p?.surname].filter(Boolean).join(" ")

// Riga principale e secondaria di un indirizzo, come nel mockup
export const addressLines = (a) => {
    if (!a) return { line1: "Non indicato", line2: "" }
    return {
        line1: [a.street, a.number].filter(Boolean).join(" "),
        line2: `${a.city ?? ""}${a.province ? ` (${a.province})` : ""}`,
    }
}

export const cityLabel = (a) => (a ? `${a.city}${a.province ? ` (${a.province})` : ""}` : "")

// ---- Errori API ----

export const apiErrorMessage = (error, fallback = "Si è verificato un errore. Riprova.") => {
    if (!error?.response) return "Impossibile raggiungere il server."
    const data = error.response.data
    if (data?.errors && typeof data.errors === "object") {
        const first = Object.values(data.errors).flat()[0]
        if (first) return String(first)
    }
    return data?.detail || data?.title || fallback
}
