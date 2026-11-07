// Liste dell'associazione: "Richieste" (da valutare), "Trasporti accettati", "Storico dei trasporti".
import { useCallback, useState } from "react"
import { FlatList, View, Text, Pressable, StyleSheet, RefreshControl } from "react-native"
import { useFocusEffect, useRouter } from "expo-router"
import api from "../../config/httpClient.js"
import { C, apiErrorMessage } from "../../constants/caregiver"
import { dayHeading, listFilters } from "../../constants/association"
import AssocTripCard from "./AssocTripCard"
import FiltersModal, { applyFilters } from "./FiltersModal"
import { Page, Loading, Icon, EmptyState, ErrorText } from "../caregiver/ui"

const CONFIG = {
    richieste: {
        title: "Richieste",
        url: "/association/requests",
        emptyTitle: "Nessuna richiesta da valutare",
        emptyText: "Qui troverai le nuove richieste destinate alla tua associazione.",
    },
    trasporti: {
        title: "Trasporti accettati",
        url: "/association/transports",
        emptyTitle: "Nessun trasporto in carico",
        emptyText: "I trasporti che accetti compariranno qui.",
    },
    storico: {
        title: "Storico dei trasporti",
        url: "/association/history",
        emptyTitle: "Lo storico è ancora vuoto",
        emptyText: "Qui ritroverai i trasporti conclusi e annullati.",
    },
}

const isToday = (iso) => new Date(iso).toDateString() === new Date().toDateString()

export default function AssocListScreen({ name }) {
    const cfg = CONFIG[name]
    const router = useRouter()
    const [loading, setLoading] = useState(true)
    const [refreshing, setRefreshing] = useState(false)
    const [error, setError] = useState("")
    const [trips, setTrips] = useState([])
    const [profile, setProfile] = useState(null)
    const [filters, setFilters] = useState(listFilters[name])
    const [filtersOpen, setFiltersOpen] = useState(false)

    const load = useCallback(async () => {
        try {
            const [list, me] = await Promise.all([api.get(cfg.url), api.get("/association/profile")])
            setTrips(list.data)
            setProfile(me.data)
            setError("")
        } catch (e) {
            setError(apiErrorMessage(e, "Impossibile caricare l’elenco."))
        } finally {
            setLoading(false)
            setRefreshing(false)
        }
    }, [cfg.url])

    useFocusEffect(
        useCallback(() => {
            setFilters(listFilters[name])
            load()
        }, [load, name]),
    )

    const updateFilters = (next) => {
        listFilters[name] = next
        setFilters(next)
    }

    if (loading) return <Loading />

    const time = (t) => new Date(t.departureDateHour).getTime()
    let source = trips
    if (name === "trasporti" && filters.scope === "today") source = source.filter((t) => isToday(t.departureDateHour))
    if (name === "trasporti" && filters.scope === "inprogress") source = source.filter((t) => t.requestStatus === "InProgress")
    const visible = applyFilters(source, filters).sort((a, b) => (name === "storico" ? time(b) - time(a) : time(a) - time(b)))

    const activeFilters = Object.entries(filters).filter(([k, v]) => k !== "scope" && v).length
    const accredited = profile?.status === "Accredited"

    // Elenco piatto con le intestazioni di giorno ("Oggi · 7 novembre")
    const rows = []
    let last = ""
    for (const t of visible) {
        const heading = dayHeading(t.departureDateHour)
        if (heading !== last) {
            rows.push({ key: `h-${heading}`, heading })
            last = heading
        }
        rows.push({ key: t.id, trip: t })
    }

    const open = (t) =>
        router.push({
            pathname: "/association/detail/[id]",
            params: { id: t.id, kind: t.requestStatus === "Pending" ? "request" : "transport" },
        })

    const intro = () => {
        if (name === "richieste")
            return (
                <View style={styles.countStrip}>
                    <Text style={styles.count}>
                        <Text style={styles.countNumber}>{trips.length}</Text> da valutare
                    </Text>
                    <View style={styles.territory}>
                        <Icon name="location-outline" size={14} color={C.muted} />
                        <Text style={styles.territoryText} numberOfLines={1}>
                            {profile?.coveredComuni?.map((c) => c.nome).join(" · ") || "Comuni da indicare"}
                        </Text>
                    </View>
                </View>
            )
        if (name === "trasporti")
            return (
                <View style={styles.chips}>
                    {[
                        ["", "Tutti"],
                        ["today", "Oggi"],
                        ["inprogress", "In corso"],
                    ].map(([value, label]) => {
                        const selected = (filters.scope ?? "") === value
                        return (
                            <Pressable
                                key={label}
                                onPress={() => updateFilters({ ...filters, scope: value })}
                                style={[styles.chip, selected && styles.chipOn]}
                            >
                                <Text style={[styles.chipText, selected && styles.chipTextOn]}>{label}</Text>
                            </Pressable>
                        )
                    })}
                </View>
            )
        return (
            <View style={styles.history}>
                <Text style={styles.historyNumber}>{visible.filter((t) => t.requestStatus === "Completed").length}</Text>
                <Text style={styles.historyText}>
                    trasporti conclusi{"\n"}nel periodo visualizzato
                </Text>
            </View>
        )
    }

    const header = (
        <View>
            <View style={styles.titleRow}>
                <Text style={styles.title}>{cfg.title}</Text>
                <Pressable onPress={() => setFiltersOpen(true)} style={styles.filterBtn} accessibilityLabel={`Filtra ${name}`}>
                    <Icon name="filter" size={20} color={C.primary} />
                    {activeFilters ? <View style={styles.filterBadge} /> : null}
                </Pressable>
            </View>
            <ErrorText style={styles.error}>{error}</ErrorText>
            {accredited || name === "storico" ? intro() : null}
            {activeFilters ? (
                <View style={styles.filterSummary}>
                    <Text style={styles.filterSummaryText}>
                        {activeFilters} {activeFilters === 1 ? "filtro attivo" : "filtri attivi"}
                    </Text>
                    <Pressable onPress={() => updateFilters(name === "trasporti" ? { scope: filters.scope } : {})}>
                        <Text style={styles.clear}>Rimuovi filtri</Text>
                    </Pressable>
                </View>
            ) : null}
        </View>
    )

    const empty = () => {
        if (error) return null
        if (!accredited && name !== "storico")
            return (
                <EmptyState
                    title="Accreditamento in corso"
                    text="Potrai gestire le richieste quando GoCare avrà verificato la tua associazione."
                    action="Vai al profilo"
                    onAction={() => router.push("/association/profile")}
                />
            )
        const filtered = activeFilters > 0 || !!filters.scope
        return (
            <EmptyState
                title={filtered ? "Nessun risultato" : cfg.emptyTitle}
                text={filtered ? "Prova a cambiare o rimuovere i filtri." : cfg.emptyText}
                action={name === "trasporti" && !filtered ? "Visualizza richieste" : undefined}
                onAction={() => router.push("/association/request")}
            />
        )
    }

    return (
        <Page>
            <FlatList
                data={accredited || name === "storico" ? rows : []}
                keyExtractor={(r) => r.key}
                contentContainerStyle={styles.list}
                ListHeaderComponent={header}
                ListEmptyComponent={empty()}
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
                renderItem={({ item }) =>
                    item.heading ? (
                        <Text style={styles.day}>{item.heading}</Text>
                    ) : (
                        <View style={styles.cardGap}>
                            <AssocTripCard trip={item.trip} onPress={() => open(item.trip)} />
                        </View>
                    )
                }
            />
            <FiltersModal
                visible={filtersOpen}
                name={name}
                filters={filters}
                onClose={() => setFiltersOpen(false)}
                onApply={(f) => {
                    updateFilters(f)
                    setFiltersOpen(false)
                }}
            />
        </Page>
    )
}

const styles = StyleSheet.create({
    list: { paddingHorizontal: 20, paddingBottom: 28 },
    titleRow: { flexDirection: "row", alignItems: "center", justifyContent: "space-between", marginTop: 12, marginBottom: 14 },
    title: { fontSize: 22, fontWeight: "600", color: C.text, letterSpacing: -0.3 },
    filterBtn: { width: 38, height: 34, borderRadius: 8, backgroundColor: C.card, alignItems: "center", justifyContent: "center" },
    filterBadge: { position: "absolute", top: 4, right: 5, width: 8, height: 8, borderRadius: 4, backgroundColor: C.primary },
    error: { marginBottom: 8 },
    countStrip: { flexDirection: "row", alignItems: "center", justifyContent: "space-between", gap: 12, marginBottom: 10 },
    count: { fontSize: 13, color: C.muted },
    countNumber: { fontSize: 20, fontWeight: "600", color: C.primary },
    territory: { flexDirection: "row", alignItems: "center", gap: 5, flexShrink: 1 },
    territoryText: { fontSize: 12, color: C.muted, flexShrink: 1 },
    chips: { flexDirection: "row", gap: 8, marginBottom: 6 },
    chip: { paddingHorizontal: 14, paddingVertical: 8, borderRadius: 8, backgroundColor: C.card },
    chipOn: { backgroundColor: C.primary },
    chipText: { fontSize: 14, color: C.muted },
    chipTextOn: { color: C.white, fontWeight: "600" },
    history: { flexDirection: "row", alignItems: "center", gap: 14, marginBottom: 6 },
    historyNumber: { fontSize: 26, fontWeight: "600", color: C.primary },
    historyText: { fontSize: 13, lineHeight: 20, color: C.muted },
    filterSummary: { flexDirection: "row", justifyContent: "space-between", alignItems: "center", marginTop: 8 },
    filterSummaryText: { fontSize: 13, color: C.muted },
    clear: { fontSize: 13, fontWeight: "600", color: C.primary },
    day: { fontSize: 13, fontWeight: "600", color: C.muted, marginTop: 16, marginBottom: 10 },
    cardGap: { marginBottom: 14 },
})
