// Popup "Filtra richieste / trasporti / storico" del mockup associazione.
import { useEffect, useState } from "react"
import { View, Text, StyleSheet } from "react-native"
import { Picker } from "@react-native-picker/picker"
import { C, TRIP_TYPES } from "../../constants/caregiver"
import { assocStatus } from "../../constants/association"
import { Dialog, FieldLabel, Input, Row, Btn, ErrorText } from "../caregiver/ui"
import DateTimeField from "../caregiver/DateTimeField"

const TITLES = { richieste: "Filtra richieste", trasporti: "Filtra trasporti", storico: "Filtra storico" }
const STATUS_OPTIONS = { trasporti: ["Confirmed", "InProgress"], storico: ["Completed", "Cancelled"] }

const Select = ({ value, onChange, options }) => (
    <View style={styles.select}>
        <Picker selectedValue={value ?? ""} onValueChange={onChange} style={styles.picker}>
            <Picker.Item label="Tutti" value="" />
            {options.map((o) => (
                <Picker.Item key={o.value} label={o.label} value={o.value} />
            ))}
        </Picker>
    </View>
)

const Labeled = ({ label, children, style }) => (
    <View style={[styles.field, style]}>
        <FieldLabel label={label} />
        {children}
    </View>
)

export default function FiltersModal({ visible, name, filters, onClose, onApply }) {
    const [f, setF] = useState(filters)
    const [error, setError] = useState("")

    useEffect(() => {
        if (visible) {
            setF(filters)
            setError("")
        }
    }, [visible])

    const set = (key) => (value) => setF((prev) => ({ ...prev, [key]: value }))

    const apply = () => {
        if ((f.fromDate && f.toDate && f.fromDate > f.toDate) || (f.timeFrom && f.timeTo && f.timeFrom > f.timeTo)) {
            setError("L’intervallo selezionato non è valido.")
            return
        }
        onApply(f)
    }

    return (
        <Dialog visible={visible} onClose={onClose}>
            <Text style={styles.title}>{TITLES[name]}</Text>

            {name === "storico" ? (
                <Row>
                    <Labeled label="Dal" style={styles.flex}>
                        <DateTimeField mode="date" value={f.fromDate} onChange={set("fromDate")} placeholder="gg/mm/aaaa" />
                    </Labeled>
                    <Labeled label="Al" style={styles.flex}>
                        <DateTimeField mode="date" value={f.toDate} onChange={set("toDate")} placeholder="gg/mm/aaaa" />
                    </Labeled>
                </Row>
            ) : (
                <Labeled label="Data">
                    <DateTimeField mode="date" value={f.date} onChange={set("date")} placeholder="gg/mm/aaaa" />
                </Labeled>
            )}

            {name === "richieste" ? (
                <>
                    <Row>
                        <Labeled label="Dalle" style={styles.flex}>
                            <DateTimeField mode="time" value={f.timeFrom} onChange={set("timeFrom")} placeholder="--:--" />
                        </Labeled>
                        <Labeled label="Alle" style={styles.flex}>
                            <DateTimeField mode="time" value={f.timeTo} onChange={set("timeTo")} placeholder="--:--" />
                        </Labeled>
                    </Row>
                    <Labeled label="Destinazione">
                        <Input value={f.destination ?? ""} onChangeText={set("destination")} />
                    </Labeled>
                    <Labeled label="Tipo di trasporto">
                        <Select
                            value={f.type}
                            onChange={set("type")}
                            options={TRIP_TYPES.map((t) => ({ value: t.value, label: t.short }))}
                        />
                    </Labeled>
                </>
            ) : (
                <Labeled label={name === "storico" ? "Esito" : "Stato"}>
                    <Select
                        value={f.status}
                        onChange={set("status")}
                        options={STATUS_OPTIONS[name].map((s) => ({ value: s, label: assocStatus(s).label }))}
                    />
                </Labeled>
            )}

            <ErrorText>{error}</ErrorText>
            <View style={styles.actions}>
                <Btn label="Annulla" variant="secondary" onPress={onClose} style={styles.flex} />
                <Btn label="Applica filtri" onPress={apply} style={styles.flex} />
            </View>
        </Dialog>
    )
}

// Applica i filtri del mockup (filterList) alla lista ricevuta dal backend
export const applyFilters = (trips, f) =>
    trips.filter((t) => {
        const d = new Date(t.departureDateHour)
        const pad = (n) => String(n).padStart(2, "0")
        const day = `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
        const time = `${pad(d.getHours())}:${pad(d.getMinutes())}`
        const destination = `${t.endAddress?.street ?? ""} ${t.endAddress?.city ?? ""}`.toLocaleLowerCase("it")
        return (
            (!f.date || day === f.date) &&
            (!f.fromDate || day >= f.fromDate) &&
            (!f.toDate || day <= f.toDate) &&
            (!f.timeFrom || time >= f.timeFrom) &&
            (!f.timeTo || time <= f.timeTo) &&
            (!f.destination || destination.includes(f.destination.toLocaleLowerCase("it"))) &&
            (!f.type || t.tripType === f.type) &&
            (!f.status || t.requestStatus === f.status)
        )
    })

const styles = StyleSheet.create({
    flex: { flex: 1 },
    title: { fontSize: 20, fontWeight: "600", color: C.text, marginBottom: 16 },
    field: { marginBottom: 14 },
    select: { backgroundColor: C.card, borderRadius: 10, overflow: "hidden" },
    picker: { color: C.text, backgroundColor: C.card, borderWidth: 0, minHeight: 48, paddingHorizontal: 10, fontSize: 16 },
    actions: { flexDirection: "row", gap: 12, marginTop: 8 },
})
