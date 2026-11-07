// Card di un trasporto nella lista "Trasporti" / "Storico dei trasporti".
import { View, Text, Pressable, StyleSheet } from "react-native"
import {
    C,
    tripState,
    tripTypeShort,
    directionLabel,
    formatWeekday,
    formatTime,
    monthShort,
    addressLines,
    toDate,
} from "../../constants/caregiver"
import { Icon, Initials, StatusLine, RouteStops, Divider } from "./ui"

export default function TripCard({ trip, onPress }) {
    const state = tripState(trip.requestStatus)
    const start = addressLines(trip.startAddress)
    const end = addressLines(trip.endAddress)
    const departure = toDate(trip.departureDateHour)

    return (
        <Pressable onPress={onPress} style={({ pressed }) => [styles.card, pressed && styles.pressed]}>
            <View style={styles.person}>
                <Initials name={trip.person?.name} surname={trip.person?.surname} />
                <View style={styles.flex}>
                    <Text style={styles.eyebrow}>Persona da accompagnare</Text>
                    <Text style={styles.name}>{[trip.person?.name, trip.person?.surname].join(" ")}</Text>
                </View>
            </View>

            <StatusLine state={state} size={12} style={styles.status} />
            <Text style={styles.note}>{state.cardText ?? state.text}</Text>

            <View style={styles.dateRow}>
                <View style={styles.dateBox}>
                    <Text style={styles.dateDay}>{String(departure.getDate()).padStart(2, "0")}</Text>
                    <Text style={styles.dateMonth}>{monthShort(departure)}</Text>
                </View>
                <View style={styles.flex}>
                    <Text style={styles.weekday}>{formatWeekday(departure)}</Text>
                    <View style={styles.timeRow}>
                        <Icon name="time-outline" size={14} color={C.muted} />
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
                        stops={[
                            { label: "Partenza", ...start },
                            { label: "Destinazione", ...end },
                        ]}
                    />
                </View>
                <Icon name="chevron-forward" size={20} color={C.primary} style={styles.chevron} />
            </View>
            <Divider style={styles.ruleTight} />

            <View style={styles.service}>
                <Icon name="mc:stethoscope" size={17} color={C.primary} />
                <Text style={styles.serviceText}>
                    {tripTypeShort(trip.tripType)} · {directionLabel(trip.tripDirection)}
                </Text>
            </View>
            <View style={styles.openRow}>
                <Text style={styles.open}>
                    Visualizza {trip.requestStatus === "Pending" ? "richiesta" : "trasporto"}
                </Text>
                <Icon name="arrow-forward" size={17} color={C.primary} />
            </View>
        </Pressable>
    )
}

const styles = StyleSheet.create({
    flex: { flex: 1 },
    card: { backgroundColor: C.card, borderRadius: 14, borderWidth: 1, borderColor: C.primary, padding: 20 },
    pressed: { opacity: 0.9 },
    person: { flexDirection: "row", alignItems: "center", gap: 11 },
    eyebrow: { fontSize: 12, color: C.muted, marginBottom: 2 },
    name: { fontSize: 17, fontWeight: "600", color: C.text },
    status: { marginTop: 16, marginBottom: 6 },
    note: { fontSize: 13, lineHeight: 22, color: C.muted, marginBottom: 20 },
    dateRow: { flexDirection: "row", alignItems: "center", gap: 14 },
    dateBox: { width: 58, height: 64, borderRadius: 10, backgroundColor: C.initialsBg, alignItems: "center", justifyContent: "center" },
    dateDay: { fontSize: 26, fontWeight: "600", color: C.primary, lineHeight: 30 },
    dateMonth: { fontSize: 11, fontWeight: "600", color: C.primary, letterSpacing: 1 },
    weekday: { fontSize: 16, fontWeight: "600", color: C.text },
    timeRow: { flexDirection: "row", alignItems: "center", gap: 5, marginTop: 4 },
    timeText: { fontSize: 13, color: C.muted },
    timeStrong: { color: C.text, fontWeight: "600" },
    routeWrap: { flexDirection: "row", alignItems: "flex-start" },
    chevron: { marginTop: 0, marginLeft: 6 },
    ruleTight: { marginTop: 0, marginBottom: 14 },
    service: { flexDirection: "row", alignItems: "center", gap: 9 },
    serviceText: { fontSize: 13, color: C.muted },
    openRow: { flexDirection: "row", alignItems: "center", gap: 10, paddingTop: 10 },
    open: { fontSize: 14, fontWeight: "600", color: C.primary },
})
