// Popup "Invita caregiver": genera un link personale (con QR code) per l'assistito.
// L'invito via email del mockup è stato tolto: si usa solo il link.
import { useEffect, useMemo, useState } from "react"
import { View, Text, Pressable, StyleSheet } from "react-native"
import { Picker } from "@react-native-picker/picker"
import * as Clipboard from "expo-clipboard"
import QRCodeCore from "qrcode/lib/core/qrcode"
import api from "../../config/httpClient.js"
import { C, apiErrorMessage } from "../../constants/caregiver"
import { Dialog, Btn, Icon, FieldLabel, ErrorText } from "./ui"

// QR disegnato con delle View (nessuna dipendenza nativa)
const QrCode = ({ value, size = 170 }) => {
    const matrix = useMemo(() => {
        const qr = QRCodeCore.create(value, { errorCorrectionLevel: "M" })
        const n = qr.modules.size
        const rows = []
        for (let r = 0; r < n; r++) {
            const row = []
            for (let c = 0; c < n; c++) row.push(qr.modules.get(r, c))
            rows.push(row)
        }
        return rows
    }, [value])
    const quiet = 2
    const cell = Math.floor(size / (matrix.length + quiet * 2))
    return (
        <View style={[styles.qrBox, { padding: cell * quiet }]}>
            {matrix.map((row, r) => (
                <View key={r} style={styles.qrRow}>
                    {row.map((on, c) => (
                        <View key={c} style={{ width: cell, height: cell, backgroundColor: on ? C.text : C.white }} />
                    ))}
                </View>
            ))}
        </View>
    )
}

// person: { id, name, surname } oppure null → si sceglie l'assistito dalla lista people
export default function InviteCaregiverModal({ visible, onClose, person, people = [] }) {
    const [selectedId, setSelectedId] = useState("")
    const [link, setLink] = useState(null)
    const [showQr, setShowQr] = useState(true)
    const [loading, setLoading] = useState(false)
    const [error, setError] = useState("")
    const [copied, setCopied] = useState(false)

    useEffect(() => {
        if (visible) {
            setSelectedId(person?.id ?? (people.length === 1 ? people[0].id : ""))
            setLink(null)
            setShowQr(true)
            setError("")
            setCopied(false)
        }
    }, [visible, person?.id])

    const current = person ?? people.find((p) => p.id === selectedId)

    const generate = async () => {
        if (!current) {
            setError("Scegli l’assistito da far seguire.")
            return
        }
        setLoading(true)
        setError("")
        try {
            const { data } = await api.post(`/assisted/${current.id}/caregiver-invite-link`)
            setLink(data)
        } catch (e) {
            setError(apiErrorMessage(e, "Impossibile generare il link."))
        } finally {
            setLoading(false)
        }
    }

    const copy = async () => {
        await Clipboard.setStringAsync(link.url)
        setCopied(true)
    }

    return (
        <Dialog visible={visible} onClose={onClose}>
            <Text style={styles.title}>Invita caregiver</Text>
            <Text style={styles.intro}>
                Invita un caregiver a seguire{" "}
                <Text style={styles.strong}>{current ? `${current.name} ${current.surname}` : "un assistito"}</Text>.
                Potrà accedere o registrarsi e accettare il tuo invito.
            </Text>

            {!person ? (
                <View>
                    <FieldLabel label="Assistito" required />
                    <View style={styles.select}>
                        <Picker
                            selectedValue={selectedId}
                            onValueChange={(v) => {
                                setSelectedId(v)
                                setLink(null)
                            }}
                            style={styles.picker}
                        >
                            <Picker.Item label="Scegli assistito" value="" />
                            {people.map((p) => (
                                <Picker.Item key={p.id} label={`${p.name} ${p.surname}`} value={p.id} />
                            ))}
                        </Picker>
                    </View>
                </View>
            ) : null}

            <Text style={styles.help}>Condividi un link personale per questo assistito.</Text>

            {link ? (
                <View>
                    <View style={styles.linkBox}>
                        <Text style={styles.linkText} numberOfLines={1} selectable>
                            {link.url}
                        </Text>
                        <Pressable onPress={copy} accessibilityLabel="Copia link" style={styles.copy}>
                            <Icon name={copied ? "checkmark" : "copy-outline"} size={20} color={C.primary} />
                        </Pressable>
                    </View>
                    <Text style={styles.local}>
                        {copied ? "Link copiato." : `Valido fino al ${new Date(link.expiresAt).toLocaleDateString("it-IT")}`}
                    </Text>
                    <Pressable onPress={() => setShowQr((s) => !s)} style={styles.qrToggle}>
                        <Icon name="qr-code-outline" size={19} color={C.primary} />
                        <Text style={styles.qrToggleText}>{showQr ? "Nascondi QR code" : "Mostra QR code"}</Text>
                    </Pressable>
                    {showQr ? (
                        <View style={styles.qrWrap}>
                            <QrCode value={link.url} />
                            <Text style={styles.qrHelp}>Scansiona per aprire l’invito.</Text>
                        </View>
                    ) : null}
                </View>
            ) : (
                <Btn label="Genera link di invito" icon="link-outline" variant="outline" onPress={generate} loading={loading} style={styles.generate} />
            )}

            <ErrorText>{error}</ErrorText>

            <Btn label="Chiudi" variant="secondary" onPress={onClose} style={styles.close} textStyle={styles.closeText} />
        </Dialog>
    )
}

const styles = StyleSheet.create({
    title: { fontSize: 22, lineHeight: 29, fontWeight: "600", color: C.text, marginBottom: 12 },
    intro: { fontSize: 13, lineHeight: 21, color: C.muted, marginBottom: 18 },
    strong: { fontWeight: "600", color: C.textStrong },
    select: { backgroundColor: C.card, borderRadius: 10, overflow: "hidden", marginBottom: 16 },
    picker: { color: C.text, backgroundColor: C.card, borderWidth: 0, minHeight: 48, paddingHorizontal: 10, fontSize: 16 },
    help: { fontSize: 13, lineHeight: 21, color: C.muted, marginBottom: 12 },
    generate: { minHeight: 44, borderRadius: 10 },
    linkBox: { flexDirection: "row", alignItems: "center", gap: 5, backgroundColor: C.card, borderRadius: 10, paddingVertical: 6, paddingLeft: 11, paddingRight: 8 },
    linkText: { flex: 1, fontSize: 12, color: C.text, paddingVertical: 8 },
    copy: { width: 34, height: 36, alignItems: "center", justifyContent: "center" },
    local: { fontSize: 11, color: C.muted, marginTop: 6 },
    qrToggle: { flexDirection: "row", alignItems: "center", justifyContent: "center", gap: 8, minHeight: 44, marginTop: 8 },
    qrToggleText: { fontSize: 14, fontWeight: "600", color: C.primary },
    qrWrap: { alignItems: "center", gap: 8, marginTop: 8 },
    qrBox: { backgroundColor: C.white, borderRadius: 9 },
    qrRow: { flexDirection: "row" },
    qrHelp: { fontSize: 12, color: C.muted },
    close: { marginTop: 16, minHeight: 44, borderRadius: 10 },
    closeText: { color: C.textStrong },
})
