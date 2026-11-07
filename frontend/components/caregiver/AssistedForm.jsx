// Campi dell'assistito usati da "Registra la persona da accompagnare" e "Modifica assistito".
import { View, Text, StyleSheet } from "react-native"
import { C, toIsoDay } from "../../constants/caregiver"
import { Field, Row, FieldLabel, ErrorText } from "./ui"
import DateTimeField from "./DateTimeField"

export const emptyAssisted = {
    name: "",
    surname: "",
    birthDate: "",
    phone: "",
    street: "",
    number: "",
    city: "",
    postalCode: "",
    province: "",
    note: "",
}

export const assistedToForm = (a) => ({
    name: a.name ?? "",
    surname: a.surname ?? "",
    birthDate: a.birthDate ?? "",
    phone: a.phone ?? "",
    street: a.homeAddress?.street ?? "",
    number: a.homeAddress?.number ?? "",
    city: a.homeAddress?.city ?? "",
    postalCode: a.homeAddress?.postalCode ?? "",
    province: a.homeAddress?.province ?? "",
    note: a.note ?? "",
})

const ADDRESS_KEYS = ["street", "number", "city", "postalCode", "province"]

// Controlli lato client prima della chiamata; ritorna { errors, payload }
export const validateAssisted = (form) => {
    const errors = {}
    if (!form.name.trim()) errors.name = "Inserisci il nome."
    if (!form.surname.trim()) errors.surname = "Inserisci il cognome."
    if (!form.birthDate) errors.birthDate = "Inserisci la data di nascita."
    else if (form.birthDate >= toIsoDay(new Date())) errors.birthDate = "La data di nascita deve essere nel passato."
    if (!form.phone.trim()) errors.phone = "Inserisci il numero di telefono."
    else if (!/^\+?[0-9][0-9 ()\-]{5,}$/.test(form.phone.trim())) errors.phone = "Il numero di telefono non è valido."

    const filled = ADDRESS_KEYS.filter((k) => form[k].trim())
    if (filled.length > 0 && filled.length < ADDRESS_KEYS.length)
        errors.address = "Per salvare la residenza compila via, civico, comune, CAP e provincia."

    const payload = {
        name: form.name.trim(),
        surname: form.surname.trim(),
        birthDate: form.birthDate,
        phone: form.phone.trim(),
        homeAddress:
            filled.length === ADDRESS_KEYS.length
                ? {
                      street: form.street.trim(),
                      number: form.number.trim(),
                      postalCode: form.postalCode.trim(),
                      city: form.city.trim(),
                      province: form.province.trim().toUpperCase(),
                  }
                : null,
        note: form.note.trim() || null,
    }
    return { errors, payload }
}

// variant "new": etichette con * e "Facoltativo"; variant "edit": sezioni come nel mockup Modifica assistito
export default function AssistedForm({ form, setForm, errors = {}, variant = "new" }) {
    const set = (key) => (value) => setForm((f) => ({ ...f, [key]: value }))
    const isNew = variant === "new"

    return (
        <View>
            {!isNew ? <Text style={styles.section}>Dati personali</Text> : null}
            {isNew ? (
                <>
                    <Field label="Nome" required value={form.name} onChangeText={set("name")} placeholder="Nome" error={errors.name} />
                    <Field label="Cognome" required value={form.surname} onChangeText={set("surname")} placeholder="Cognome" error={errors.surname} />
                </>
            ) : (
                <Row>
                    <Field label="Nome" style={styles.flex} value={form.name} onChangeText={set("name")} error={errors.name} />
                    <Field label="Cognome" style={styles.flex} value={form.surname} onChangeText={set("surname")} error={errors.surname} />
                </Row>
            )}
            <Field label="Data di nascita" required={isNew} error={errors.birthDate}>
                <DateTimeField mode="date" value={form.birthDate} onChange={set("birthDate")} placeholder="gg/mm/aaaa" maximumDate={new Date()} />
            </Field>
            <Field
                label="Telefono"
                required={isNew}
                value={form.phone}
                onChangeText={set("phone")}
                placeholder={isNew ? "Numero di telefono" : undefined}
                keyboardType="phone-pad"
                error={errors.phone}
            />

            {isNew ? <FieldLabel label="Residenza" optional style={styles.subsection} /> : <Text style={styles.section}>Residenza</Text>}
            <Row>
                <Field label="Via" style={styles.flex} value={form.street} onChangeText={set("street")} />
                <Field label="Civico" style={styles.civic} value={form.number} onChangeText={set("number")} />
            </Row>
            <Field label="Comune" value={form.city} onChangeText={set("city")} />
            <Row>
                <Field label="CAP" style={styles.flex} value={form.postalCode} onChangeText={set("postalCode")} keyboardType="number-pad" maxLength={5} />
                <Field
                    label="Provincia"
                    style={styles.flex}
                    value={form.province}
                    onChangeText={(v) => set("province")(v.toUpperCase())}
                    autoCapitalize="characters"
                    maxLength={2}
                />
            </Row>
            <ErrorText style={styles.addressError}>{errors.address}</ErrorText>

            <Field
                label="Note"
                optional={isNew}
                multiline
                value={form.note}
                onChangeText={set("note")}
                placeholder={isNew ? "Indicazioni utili per accompagnare l’assistito" : undefined}
                maxLength={255}
            />
        </View>
    )
}

const styles = StyleSheet.create({
    flex: { flex: 1 },
    civic: { width: 78 },
    section: { fontSize: 14, fontWeight: "600", color: C.muted, marginBottom: 12 },
    subsection: { marginTop: 8, marginBottom: 16 },
    addressError: { marginTop: -8, marginBottom: 12 },
})
