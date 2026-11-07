// Card di una richiesta / trasporto nelle liste dell'associazione.
// GET /association/... non restituisce nome ed età dell'assistito: al loro posto si mostrano
// il codice della richiesta e il tipo di trasporto.
import { View, Text, Pressable, StyleSheet } from "react-native"
import { C, tripTypeLabel, directionLabel, formatFullDate, formatTime, monthShort, addressLines, toDate } from "../../constants/caregiver"
import { assocStatus, shortId } from "../../constants/association"
import { Icon, RouteStops, Divider } from "../caregiver/ui"

export const AssocStatus = ({ status, style }) => {
    const s = assocStatus(status)
    return (
        <View style={[styles.status, style]}>
            <View style={[styles.dot, { backgroundColor: s.dot }]} />
            <Text style={[styles.statusText, { color: s.color }]}>{s.label}</Text>
        </View>
    )
}

export default function AssocTripCard({ trip, onPress }) {
    const departure = toDate(trip.departureDateHour)
    const pending = trip.requestStatus === "Pending"
    return (
        <Pressable onPress={onPress} style={({ pressed }) => [styles.card, pressed && styles.pressed]}>
            <Text style={styles.id}>{shortId(trip.id)}</Text>
            <Text style={styles.heading}>{tripTypeLabel(trip.tripType)}</Text>
            <AssocStatus status={trip.requestStatus} style={styles.statusSpace} />

            <View style={styles.dateRow}>
                <View style={styles.dateBox}>
                    <Text style={styles.dateDay}>{departure.getDate()}</Text>
                    <Text style={styles.dateMonth}>{monthShort(departure)}</Text>
                </View>
                <View style={styles.flex}>
                    <Text style={styles.dateText}>{formatFullDate(departure)}</Text>
                    <View style={styles.timeRow}>
                        <Icon name="time-outline" size={14} color={C.primary} />
                        <Text style={styles.timeText}>
                            Partenza alle <Text style={styles.timeStrong}>{formatTime(departure)}</Text>
                        </Text>
                    </View>
                </View>
            </View>

            <Divider />
            <View style={styles.routeWrap}>
                <View style={styles.flex}>
                    <RouteStops
                        labelColor={C.muted}
                        stops={[
                            { label: "Partenza", ...addressLines(trip.startAddress) },
                            { label: "Destinazione", ...addressLines(trip.endAddress) },
                        ]}
                    />
                </View>
                <Icon name="chevron-forward" size={20} color={C.primary} />
            </View>
            <Divider style={styles.ruleTight} />

            <View style={styles.service}>
                <Icon name="car-outline" size={17} color={C.primary} />
                <Text style={styles.serviceText}>{directionLabel(trip.tripDirection)}</Text>
            </View>
            <View style={styles.openRow}>
                <Text style={styles.open}>Visualizza {pending ? "richiesta" : "trasporto"}</Text>
                <Icon name="arrow-forward" size={16} color={C.primary} />
            </View>
        </Pressable>
    )
}

const styles = StyleSheet.create({
    flex: { flex: 1 },
    card: { backgroundColor: C.card, borderRadius: 14, borderWidth: 1, borderColor: C.primary, padding: 18 },
    pressed: { opacity: 0.9 },
    id: { fontSize: 12, color: C.muted, marginBottom: 10 },
    heading: { fontSize: 17, fontWeight: "600", color: C.text },
    status: { flexDirection: "row", alignItems: "center", gap: 7 },
    statusSpace: { marginTop: 10, marginBottom: 16 },
    dot: { width: 8, height: 8, borderRadius: 4 },
    statusText: { fontSize: 13, fontWeight: "500" },
    dateRow: { flexDirection: "row", alignItems: "center", gap: 12 },
    dateBox: { width: 52, height: 56, borderRadius: 10, backgroundColor: C.initialsBg, alignItems: "center", justifyContent: "center" },
    dateDay: { fontSize: 22, fontWeight: "600", color: C.primary },
    dateMonth: { fontSize: 11, color: C.primary, letterSpacing: 0.5 },
    dateText: { fontSize: 16, fontWeight: "600", color: C.text },
    timeRow: { flexDirection: "row", alignItems: "center", gap: 5, marginTop: 4 },
    timeText: { fontSize: 13, color: C.muted },
    timeStrong: { color: C.text, fontWeight: "600" },
    routeWrap: { flexDirection: "row", alignItems: "center" },
    ruleTight: { marginTop: 0, marginBottom: 14 },
    service: { flexDirection: "row", alignItems: "center", gap: 9 },
    serviceText: { fontSize: 13, color: C.muted },
    openRow: { flexDirection: "row", alignItems: "center", gap: 10, paddingTop: 12 },
    open: { fontSize: 14, fontWeight: "600", color: C.primary },
})
