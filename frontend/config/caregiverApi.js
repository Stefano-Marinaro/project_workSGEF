// Chiamate API usate da più schermate caregiver.
import api from "./httpClient.js"

// GET /transports non restituisce il nome dell'assistito: per mostrarlo nelle card
// si chiede la lista per ogni assistito (?assistedId=...) e si aggiunge il nome.
export const loadTripsWithPeople = async () => {
    const { data: people } = await api.get("/assisted")
    const perPerson = await Promise.all(
        people.map(async (person) => {
            const { data: trips } = await api.get("/transports", { params: { assistedId: person.id } })
            return trips.map((trip) => ({ ...trip, person }))
        }),
    )
    return { people, trips: perPerson.flat() }
}

// Cerca il trasporto tra gli assistiti collegati (serve quando il dettaglio viene
// aperto senza parametri, es. da una notifica).
export const findTripOwner = async (tripId) => {
    const { people, trips } = await loadTripsWithPeople()
    const trip = trips.find((t) => t.id === tripId)
    return trip ? { person: trip.person, associationName: trip.associationName } : { person: null, people }
}

// Geocodifica un indirizzo inserito a mano: il backend vuole latitudine e longitudine
// (TripAddressRequest) e le ricava solo tramite /addresses/search.
export const geocodeAddress = async (address) => {
    const query = `${address.street} ${address.number}, ${address.postalCode} ${address.city}`
    const { data } = await api.get("/addresses/search", { params: { q: query } })
    const match = data?.[0]
    if (!match) return null
    return {
        street: address.street,
        number: address.number,
        postalCode: address.postalCode,
        city: address.city,
        province: address.province,
        latitude: match.latitude,
        longitude: match.longitude,
    }
}
