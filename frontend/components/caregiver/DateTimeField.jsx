// Campo data / ora: su web usa l'<input> nativo del browser (come il mockup),
// su iOS/Android apre il DateTimePicker di sistema.
import { createElement, useState } from "react"
import { Platform, Pressable, Text, View, StyleSheet } from "react-native"
import DateTimePicker from "@react-native-community/datetimepicker"
import { C } from "../../constants/caregiver"
import { Icon } from "./ui"

const pad = (n) => String(n).padStart(2, "0")

// value: "YYYY-MM-DD" (mode date) oppure "HH:MM" (mode time); stringa vuota se non scelto
export default function DateTimeField({ mode = "date", value, onChange, placeholder, maximumDate, minimumDate, style }) {
    const [open, setOpen] = useState(false)

    if (Platform.OS === "web") {
        return createElement("input", {
            type: mode,
            value: value ?? "",
            max: maximumDate ? toIso(maximumDate) : undefined,
            min: minimumDate ? toIso(minimumDate) : undefined,
            onChange: (e) => onChange(e.target.value),
            style: StyleSheet.flatten([webStyle, style]),
        })
    }

    const asDate = () => {
        if (!value) return new Date()
        if (mode === "date") {
            const [y, m, d] = value.split("-").map(Number)
            return new Date(y, m - 1, d)
        }
        const [h, min] = value.split(":").map(Number)
        const d = new Date()
        d.setHours(h, min, 0, 0)
        return d
    }

    const handle = (event, selected) => {
        if (Platform.OS === "android") setOpen(false)
        if (event?.type === "dismissed" || !selected) return
        onChange(
            mode === "date"
                ? `${selected.getFullYear()}-${pad(selected.getMonth() + 1)}-${pad(selected.getDate())}`
                : `${pad(selected.getHours())}:${pad(selected.getMinutes())}`,
        )
    }

    const display = value ? (mode === "date" ? value.split("-").reverse().join("/") : value) : placeholder

    return (
        <View>
            <Pressable onPress={() => setOpen((o) => !o)} style={[styles.box, style]}>
                <Text style={[styles.text, !value && styles.placeholder]}>{display}</Text>
                <Icon name={mode === "date" ? "calendar-outline" : "time-outline"} size={18} color={C.primary} />
            </Pressable>
            {open ? (
                <DateTimePicker
                    value={asDate()}
                    mode={mode}
                    is24Hour
                    locale="it-IT"
                    display={Platform.OS === "ios" ? "spinner" : "default"}
                    maximumDate={maximumDate}
                    minimumDate={minimumDate}
                    onChange={handle}
                />
            ) : null}
        </View>
    )
}

const toIso = (d) => `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`

const webStyle = {
    boxSizing: "border-box",
    width: "100%",
    minHeight: 48,
    border: "none",
    borderRadius: 10,
    padding: "12px 14px",
    backgroundColor: C.card,
    color: C.text,
    fontSize: 16,
    fontFamily: "-apple-system, \"Segoe UI\", Roboto, system-ui, sans-serif",
    outline: "none",
}

const styles = StyleSheet.create({
    box: {
        minHeight: 48,
        borderRadius: 10,
        backgroundColor: C.card,
        paddingHorizontal: 14,
        flexDirection: "row",
        alignItems: "center",
        justifyContent: "space-between",
    },
    text: { fontSize: 16, color: C.text },
    placeholder: { color: C.faint },
})
