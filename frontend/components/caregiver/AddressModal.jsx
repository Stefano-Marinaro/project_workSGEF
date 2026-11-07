// Popup "Aggiungi indirizzo" del wizard. Al salvataggio l'indirizzo viene geocodificato
// tramite /addresses/search perché il backend richiede le coordinate del viaggio.
import { useEffect, useState } from "react"
import { Text, View, StyleSheet } from "react-native"
import { geocodeAddress } from "../../config/caregiverApi"
import { C, apiErrorMessage } from "../../constants/caregiver"
import { Dialog, Field, Btn, ErrorText } from "./ui"

const EMPTY = { street: "", number: "", city: "", postalCode: "", province: "" }

export default function AddressModal({ visible, onClose, onSave }) {
    const [form, setForm] = useState(EMPTY)
    const [errors, setErrors] = useState({})
    const [error, setError] = useState("")
    const [saving, setSaving] = useState(false)

    useEffect(() => {
        if (visible) {
            setForm(EMPTY)
            setErrors({})
            setError("")
        }
    }, [visible])

    const set = (key) => (value) => setForm((f) => ({ ...f, [key]: value }))

    const save = async () => {
        const found = {}
        if (!form.street.trim()) found.street = "Inserisci la via."
        if (!form.number.trim()) found.number = "Inserisci il civico."
        if (!form.city.trim()) found.city = "Inserisci il comune."
        if (!/^\d{5}$/.test(form.postalCode.trim())) found.postalCode = "Il CAP deve essere di 5 cifre."
        if (!/^[A-Za-z]{2}$/.test(form.province.trim())) found.province = "Inserisci la sigla della provincia."
        setErrors(found)
        if (Object.keys(found).length) return

        setSaving(true)
        setError("")
        try {
            const address = {
                street: form.street.trim(),
                number: form.number.trim(),
                city: form.city.trim(),
                postalCode: form.postalCode.trim(),
                province: form.province.trim().toUpperCase(),
            }
            const located = await geocodeAddress(address)
            if (!located) {
                setError("Indirizzo non trovato. Controlla via, civico e comune.")
                return
            }
            onSave(located)
        } catch (e) {
            setError(apiErrorMessage(e, "Impossibile verificare l’indirizzo."))
        } finally {
            setSaving(false)
        }
    }

    return (
        <Dialog visible={visible} onClose={onClose}>
            <Text style={styles.title}>Aggiungi indirizzo</Text>
            <Field label="Via" required value={form.street} onChangeText={set("street")} error={errors.street} />
            <Field label="Civico" required value={form.number} onChangeText={set("number")} error={errors.number} />
            <Field label="Comune" required value={form.city} onChangeText={set("city")} error={errors.city} />
            <Field
                label="CAP"
                required
                value={form.postalCode}
                onChangeText={set("postalCode")}
                keyboardType="number-pad"
                maxLength={5}
                error={errors.postalCode}
            />
            <Field
                label="Provincia"
                required
                value={form.province}
                onChangeText={(v) => set("province")(v.toUpperCase())}
                autoCapitalize="characters"
                maxLength={2}
                error={errors.province}
            />
            <ErrorText>{error}</ErrorText>
            <View style={styles.actions}>
                <Btn label="Annulla" variant="secondary" onPress={onClose} style={styles.btn} />
                <Btn label="Salva" onPress={save} loading={saving} style={styles.btn} />
            </View>
        </Dialog>
    )
}

const styles = StyleSheet.create({
    title: { fontSize: 22, fontWeight: "600", color: C.text, marginBottom: 18 },
    actions: { flexDirection: "row", justifyContent: "space-between", marginTop: 12 },
    btn: { minHeight: 46, borderRadius: 10, paddingHorizontal: 16 },
})
