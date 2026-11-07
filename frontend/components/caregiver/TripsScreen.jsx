// Lista trasporti: "Trasporti" (richieste aperte) e "Storico dei trasporti" (concluse/annullate).
import { useCallback, useState } from "react"
import { FlatList, View, StyleSheet, RefreshControl } from "react-native"
import { useFocusEffect, useRouter } from "expo-router"
import { loadTripsWithPeople } from "../../config/caregiverApi"
import { C, isTerminal, apiErrorMessage } from "../../constants/caregiver"
import TripCard from "./TripCard"
import { Page, PageTitle, Loading, EmptyState, ErrorText, BackLink } from "./ui"

export default function TripsScreen({ closed = false }) {
    const router = useRouter()
    const [loading, setLoading] = useState(true)
    const [refreshing, setRefreshing] = useState(false)
    const [error, setError] = useState("")
    const [people, setPeople] = useState([])
    const [trips, setTrips] = useState([])

    const load = useCallback(async () => {
        try {
            const data = await loadTripsWithPeople()
            const time = (t) => new Date(t.departureDateHour).getTime()
            const filtered = data.trips
                .filter((t) => isTerminal(t.requestStatus) === closed)
                .sort((a, b) => (closed ? time(b) - time(a) : time(a) - time(b)))
            setPeople(data.people)
            setTrips(filtered)
            setError("")
        } catch (e) {
            setError(apiErrorMessage(e, "Impossibile caricare i trasporti."))
        } finally {
            setLoading(false)
            setRefreshing(false)
        }
    }, [closed])

    useFocusEffect(
        useCallback(() => {
            load()
        }, [load]),
    )

    if (loading) return <Loading />

    const openTrip = (trip) =>
        router.push({
            pathname: "/transport/[id]",
            params: {
                id: trip.id,
                assistedId: trip.person.id,
                assistedName: `${trip.person.name} ${trip.person.surname}`,
                associationName: trip.associationName ?? "",
                from: closed ? "history" : "list",
            },
        })

    const empty = () => {
        if (closed) return <ErrorText style={styles.closedEmpty}>Nessun trasporto nello storico.</ErrorText>
        if (people.length === 0)
            return (
                <EmptyState
                    title="Aggiungi il tuo primo assistito"
                    text="Registra la persona da accompagnare per iniziare a usare GoCare."
                    action="Registra assistito"
                    onAction={() => router.push("/assisted/new")}
                />
            )
        return (
            <EmptyState
                title="Hai bisogno di un accompagnamento?"
                text="Invia una richiesta per la persona che segui. Un’associazione di volontariato dovrà confermare la disponibilità."
                action="Richiedi un trasporto"
                onAction={() => router.push("/transport/new")}
            />
        )
    }

    return (
        <Page>
            <FlatList
                data={trips}
                keyExtractor={(item) => item.id}
                renderItem={({ item }) => <TripCard trip={item} onPress={() => openTrip(item)} />}
                ItemSeparatorComponent={() => <View style={styles.gap} />}
                contentContainerStyle={styles.list}
                refreshControl={
                    <RefreshControl
                        refreshing={refreshing}
                        onRefresh={() => {
                            setRefreshing(true)
                            load()
                        }}
                        tintColor={C.primary}
                    />
                }
                ListHeaderComponent={
                    <View>
                        <PageTitle>{closed ? "Storico dei trasporti" : "Trasporti"}</PageTitle>
                        {closed ? <BackLink label="Torna al profilo" onPress={() => router.push("/profile")} /> : null}
                        <ErrorText style={styles.error}>{error}</ErrorText>
                    </View>
                }
                ListEmptyComponent={error ? null : empty()}
            />
        </Page>
    )
}

const styles = StyleSheet.create({
    list: { paddingHorizontal: 20, paddingBottom: 28 },
    gap: { height: 14 },
    error: { marginBottom: 12 },
    closedEmpty: { color: C.muted, fontSize: 15 },
})
