// Componenti grafici condivisi dalle schermate caregiver, ricavati dal mockup.
import { forwardRef } from "react"
import {
    View,
    Text,
    Pressable,
    TextInput,
    StyleSheet,
    Modal,
    ScrollView,
    ActivityIndicator,
    KeyboardAvoidingView,
    Platform,
} from "react-native"
import { Ionicons, MaterialCommunityIcons } from "@expo/vector-icons"
import { C, stateColor, stateDot, initialsFor } from "../../constants/caregiver"

// ---------- Icone ----------
// Le icone "mc:" arrivano da MaterialCommunityIcons, le altre da Ionicons
export const Icon = ({ name, size = 20, color = C.primary, style }) =>
    name.startsWith("mc:") ? (
        <MaterialCommunityIcons name={name.slice(3)} size={size} color={color} style={style} />
    ) : (
        <Ionicons name={name} size={size} color={color} style={style} />
    )

// ---------- Layout ----------
export const Page = ({ children, style }) => <View style={[styles.page, style]}>{children}</View>

export const Scroll = forwardRef(({ children, contentStyle, ...props }, ref) => (
    <ScrollView
        ref={ref}
        style={styles.flex}
        contentContainerStyle={[styles.scrollContent, contentStyle]}
        keyboardShouldPersistTaps="handled"
        {...props}
    >
        {children}
    </ScrollView>
))

export const Loading = () => (
    <Page style={styles.center}>
        <ActivityIndicator size="large" color={C.primary} />
    </Page>
)

export const PageTitle = ({ children, style }) => <Text style={[styles.pageTitle, style]}>{children}</Text>

export const Subtitle = ({ children, style }) => <Text style={[styles.subtitle, style]}>{children}</Text>

export const BackLink = ({ label, onPress, style }) => (
    <Pressable onPress={onPress} style={[styles.backLink, style]} hitSlop={8}>
        <Icon name="chevron-back" size={16} color={C.primary} />
        <Text style={styles.backLinkText}>{label}</Text>
    </Pressable>
)

export const Card = ({ children, style, onPress, selected }) => {
    const content = [styles.card, selected && styles.cardSelected, style]
    if (!onPress) return <View style={content}>{children}</View>
    return (
        <Pressable onPress={onPress} style={({ pressed }) => [content, pressed && styles.pressed]}>
            {children}
        </Pressable>
    )
}

export const Divider = ({ style }) => <View style={[styles.divider, style]} />

export const SectionLabel = ({ children, style }) => <Text style={[styles.sectionLabel, style]}>{children}</Text>

export const Muted = ({ children, style }) => <Text style={[styles.muted, style]}>{children}</Text>

export const ErrorText = ({ children, style }) =>
    children ? <Text style={[styles.error, style]}>{children}</Text> : null

// ---------- Bottoni ----------
export const Btn = ({ label, onPress, variant = "primary", icon, disabled, loading, style, textStyle }) => {
    const v = BTN[variant] ?? BTN.primary
    return (
        <Pressable
            onPress={onPress}
            disabled={disabled || loading}
            style={({ pressed }) => [
                styles.btn,
                { backgroundColor: v.bg, borderColor: v.border ?? v.bg },
                (disabled || loading) && styles.disabled,
                pressed && styles.pressed,
                style,
            ]}
        >
            {loading ? (
                <ActivityIndicator color={v.fg} />
            ) : (
                <>
                    {icon ? <Icon name={icon} size={18} color={v.fg} /> : null}
                    <Text style={[styles.btnText, { color: v.fg }, textStyle]}>{label}</Text>
                </>
            )}
        </Pressable>
    )
}

const BTN = {
    primary: { bg: C.primary, fg: C.white },
    secondary: { bg: C.card, fg: C.muted },
    outline: { bg: "transparent", fg: C.primary, border: C.primary },
    danger: { bg: C.danger, fg: C.white },
    ghost: { bg: "transparent", fg: C.primary, border: "transparent" },
}

export const Footer = ({ children }) => <View style={styles.footer}>{children}</View>

export const CircleButton = ({ icon, onPress, label }) => (
    <Pressable onPress={onPress} accessibilityLabel={label} style={({ pressed }) => [styles.circleBtn, pressed && styles.pressed]}>
        <Icon name={icon} size={18} color={C.white} />
    </Pressable>
)

// ---------- Form ----------
export const FieldLabel = ({ label, required, optional, style }) => (
    <Text style={[styles.fieldLabel, style]}>
        {label}
        {required ? <Text style={styles.star}> *</Text> : null}
        {optional ? <Text style={styles.optional}> · Facoltativo</Text> : null}
    </Text>
)

export const Input = ({ style, multiline, editable = true, ...props }) => (
    <TextInput
        placeholderTextColor={C.faint}
        multiline={multiline}
        editable={editable}
        style={[styles.input, multiline && styles.textarea, !editable && styles.inputDisabled, style]}
        {...props}
    />
)

export const Field = ({ label, required, optional, error, style, children, ...inputProps }) => (
    <View style={[styles.field, style]}>
        <FieldLabel label={label} required={required} optional={optional} />
        {children ?? <Input {...inputProps} />}
        <ErrorText>{error}</ErrorText>
    </View>
)

export const Row = ({ children, style }) => <View style={[styles.row, style]}>{children}</View>

export const Radio = ({ checked }) => (
    <View style={[styles.radio, checked && styles.radioOn]}>{checked ? <View style={styles.radioDot} /> : null}</View>
)

export const Checkbox = ({ checked }) => (
    <View style={[styles.checkbox, checked && styles.checkboxOn]}>
        {checked ? <Icon name="checkmark" size={14} color={C.white} /> : null}
    </View>
)

export const CheckRow = ({ checked, onPress, label, style }) => (
    <Pressable onPress={onPress} style={[styles.checkRow, style]}>
        <Checkbox checked={checked} />
        <Text style={styles.checkRowText}>{label}</Text>
    </Pressable>
)

// Opzione "pillola" con radio, usata per mobilità e percorso
export const OptionRow = ({ selected, onPress, label, disabled, multi, style }) => (
    <Pressable
        onPress={onPress}
        disabled={disabled}
        style={[styles.option, selected && styles.optionSelected, disabled && styles.disabled, style]}
    >
        {multi ? <Checkbox checked={selected} /> : <Radio checked={selected} />}
        <Text style={styles.optionText}>{label}</Text>
    </Pressable>
)

// ---------- Persone e stati ----------
export const Initials = ({ name, surname, size = 38, solid, style }) => (
    <View
        style={[
            styles.initials,
            { width: size, height: size, borderRadius: size / 2, backgroundColor: solid ? C.primary : C.initialsBg },
            style,
        ]}
    >
        <Text style={[styles.initialsText, { fontSize: size * 0.36, color: solid ? C.white : C.primary }]}>
            {initialsFor(name, surname)}
        </Text>
    </View>
)

// color: forza il colore (nel dettaglio lo stato "in corso" è viola come nel mockup)
export const StatusLine = ({ state, size = 13, style, color }) => (
    <View style={[styles.statusLine, style]}>
        <View style={[styles.statusDot, { backgroundColor: color ?? stateDot(state.kind) }]} />
        <Text style={[styles.statusText, { color: color ?? stateColor(state.kind), fontSize: size }]}>{state.label}</Text>
    </View>
)

// Percorso verticale con icone partenza/arrivo collegate da una linea
export const RouteStops = ({ stops, labelColor = C.primary }) => (
    <View>
        {stops.map((stop, i) => (
            <View key={i} style={styles.stop}>
                <View style={styles.stopRail}>
                    <Icon name={i === 0 ? "radio-button-on-outline" : "location-outline"} size={18} color={C.primary} />
                    {i < stops.length - 1 ? <View style={styles.stopLine} /> : null}
                </View>
                <View style={styles.stopBody}>
                    <Text style={[styles.stopLabel, { color: labelColor }]}>
                        {stop.label}
                        {stop.time ? <Text style={styles.stopTime}> · {stop.time}</Text> : null}
                    </Text>
                    <Text style={styles.stopLine1}>{stop.line1}</Text>
                    {stop.line2 ? <Text style={styles.stopLine2}>{stop.line2}</Text> : null}
                </View>
            </View>
        ))}
    </View>
)

export const SuccessCheck = () => (
    <View style={styles.successCheck}>
        <Icon name="checkmark" size={30} color={C.success} />
    </View>
)

export const EmptyState = ({ title, text, action, onAction }) => (
    <View style={styles.empty}>
        <Text style={styles.emptyTitle}>{title}</Text>
        {text ? <Text style={styles.emptyText}>{text}</Text> : null}
        {action ? <Btn label={action} onPress={onAction} style={styles.emptyBtn} /> : null}
    </View>
)

// ---------- Modali ----------
export const Dialog = ({ visible, onClose, children, style }) => (
    <Modal visible={visible} transparent animationType="fade" onRequestClose={onClose}>
        <KeyboardAvoidingView behavior={Platform.OS === "ios" ? "padding" : undefined} style={styles.backdrop}>
            <Pressable style={StyleSheet.absoluteFill} onPress={onClose} />
            <View style={[styles.dialog, style]}>
                <ScrollView keyboardShouldPersistTaps="handled" bounces={false}>
                    {children}
                </ScrollView>
            </View>
        </KeyboardAvoidingView>
    </Modal>
)

export const BottomSheet = ({ visible, onClose, title, children }) => (
    <Modal visible={visible} transparent animationType="slide" onRequestClose={onClose}>
        <KeyboardAvoidingView behavior={Platform.OS === "ios" ? "padding" : undefined} style={styles.sheetBackdrop}>
            <Pressable style={StyleSheet.absoluteFill} onPress={onClose} />
            <View style={styles.sheet}>
                <View style={styles.sheetHeader}>
                    <Text style={styles.sheetTitle}>{title}</Text>
                    <Pressable onPress={onClose} style={styles.sheetClose} accessibilityLabel="Chiudi">
                        <Icon name="close" size={16} color={C.text} />
                    </Pressable>
                </View>
                <ScrollView keyboardShouldPersistTaps="handled">{children}</ScrollView>
            </View>
        </KeyboardAvoidingView>
    </Modal>
)

export const ConfirmDialog = ({ visible, title, message, onCancel, onConfirm, loading, error }) => (
    <Dialog visible={visible} onClose={onCancel}>
        <Text style={styles.confirmTitle}>{title}</Text>
        <Text style={styles.confirmText}>{message}</Text>
        <ErrorText>{error}</ErrorText>
        <View style={styles.confirmActions}>
            <Btn label="Annulla" onPress={onCancel} style={styles.confirmBtn} />
            <Btn label="Conferma" onPress={onConfirm} loading={loading} style={styles.confirmBtn} />
        </View>
    </Dialog>
)

export const styles = StyleSheet.create({
    flex: { flex: 1 },
    page: { flex: 1, backgroundColor: C.page },
    center: { alignItems: "center", justifyContent: "center" },
    scrollContent: { paddingHorizontal: 20, paddingBottom: 28 },
    pressed: { opacity: 0.85 },
    disabled: { opacity: 0.5 },

    pageTitle: { fontSize: 22, fontWeight: "600", color: C.text, letterSpacing: -0.3, marginTop: 12, marginBottom: 16 },
    subtitle: { fontSize: 15, lineHeight: 22, color: C.muted, marginBottom: 14 },
    backLink: { flexDirection: "row", alignItems: "center", gap: 8, alignSelf: "flex-start", paddingVertical: 6, marginBottom: 14 },
    backLinkText: { fontSize: 14, fontWeight: "600", color: C.primary },

    card: { backgroundColor: C.card, borderRadius: 14, padding: 18 },
    cardSelected: { backgroundColor: C.cardSelected, borderWidth: 1.5, borderColor: C.primary },
    divider: { height: 1, backgroundColor: C.rule, marginVertical: 18 },
    sectionLabel: { fontSize: 14, fontWeight: "600", color: C.muted, marginBottom: 10 },
    muted: { fontSize: 14, lineHeight: 22, color: C.muted },
    error: { color: "#ad293b", fontSize: 13, lineHeight: 20, marginTop: 6 },

    btn: {
        minHeight: 48,
        borderRadius: 12,
        borderWidth: 1,
        paddingHorizontal: 18,
        paddingVertical: 11,
        flexDirection: "row",
        alignItems: "center",
        justifyContent: "center",
        gap: 9,
    },
    btnText: { fontSize: 15, fontWeight: "600", textAlign: "center" },
    footer: {
        flexDirection: "row",
        justifyContent: "space-between",
        gap: 12,
        paddingHorizontal: 20,
        paddingVertical: 14,
        borderTopWidth: 1,
        borderTopColor: C.card,
        backgroundColor: C.page,
    },
    circleBtn: { width: 44, height: 44, borderRadius: 22, backgroundColor: C.primary, alignItems: "center", justifyContent: "center" },

    field: { marginBottom: 16 },
    fieldLabel: { fontSize: 14, fontWeight: "600", color: C.textStrong, marginBottom: 7 },
    star: { color: C.primary, fontWeight: "700" },
    optional: { fontWeight: "600", color: C.textStrong },
    input: {
        backgroundColor: C.card,
        borderRadius: 10,
        paddingHorizontal: 14,
        paddingVertical: 12,
        minHeight: 48,
        fontSize: 16,
        color: C.text,
    },
    textarea: { minHeight: 110, textAlignVertical: "top" },
    inputDisabled: { color: C.faint },
    row: { flexDirection: "row", gap: 12 },

    radio: { width: 18, height: 18, borderRadius: 9, borderWidth: 1.5, borderColor: C.muted, backgroundColor: C.white, alignItems: "center", justifyContent: "center" },
    radioOn: { borderColor: C.primary },
    radioDot: { width: 10, height: 10, borderRadius: 5, backgroundColor: C.primary },
    checkbox: { width: 18, height: 18, borderRadius: 4, borderWidth: 1.5, borderColor: C.muted, backgroundColor: C.white, alignItems: "center", justifyContent: "center" },
    checkboxOn: { backgroundColor: C.primary, borderColor: C.primary },
    checkRow: { flexDirection: "row", alignItems: "center", gap: 10, paddingVertical: 6 },
    checkRowText: { flex: 1, fontSize: 14, color: C.textStrong },
    option: {
        flexDirection: "row",
        alignItems: "center",
        gap: 12,
        backgroundColor: C.card,
        borderRadius: 12,
        borderWidth: 1.5,
        borderColor: "transparent",
        paddingVertical: 13,
        paddingHorizontal: 14,
    },
    optionSelected: { borderColor: C.primary, backgroundColor: C.cardSelected },
    optionText: { flex: 1, fontSize: 14, color: C.text },

    initials: { alignItems: "center", justifyContent: "center" },
    initialsText: { fontWeight: "600" },
    statusLine: { flexDirection: "row", alignItems: "center", gap: 7 },
    statusDot: { width: 8, height: 8, borderRadius: 4 },
    statusText: { fontWeight: "500" },

    stop: { flexDirection: "row", gap: 12 },
    stopRail: { width: 20, alignItems: "center" },
    stopLine: { flex: 1, width: 2, backgroundColor: C.primary, marginVertical: 2 },
    stopBody: { flex: 1, paddingBottom: 18 },
    stopLabel: { fontSize: 13, fontWeight: "500", marginBottom: 4 },
    stopTime: { fontWeight: "700", color: C.textStrong },
    stopLine1: { fontSize: 16, color: C.text },
    stopLine2: { fontSize: 14, color: C.muted, marginTop: 4 },

    successCheck: { width: 64, height: 64, borderRadius: 32, backgroundColor: "#dcefe3", alignItems: "center", justifyContent: "center", alignSelf: "center" },
    empty: { alignItems: "center", paddingHorizontal: 6, paddingVertical: 32 },
    emptyTitle: { fontSize: 22, lineHeight: 30, fontWeight: "500", color: C.text, textAlign: "center", marginBottom: 12 },
    emptyText: { fontSize: 15, lineHeight: 24, color: C.muted, textAlign: "center", marginBottom: 24 },
    emptyBtn: { alignSelf: "stretch" },

    backdrop: { flex: 1, backgroundColor: "rgba(32,30,43,0.53)", justifyContent: "center", padding: 16 },
    dialog: { backgroundColor: C.page, borderRadius: 18, padding: 22, maxHeight: "90%", width: "100%", maxWidth: 380, alignSelf: "center" },
    sheetBackdrop: { flex: 1, backgroundColor: "rgba(32,30,43,0.45)", justifyContent: "flex-end" },
    sheet: { backgroundColor: C.page, borderTopLeftRadius: 16, borderTopRightRadius: 16, padding: 20, maxHeight: "85%" },
    sheetHeader: { flexDirection: "row", alignItems: "center", justifyContent: "space-between", marginBottom: 14 },
    sheetTitle: { fontSize: 17, fontWeight: "700", color: C.text },
    sheetClose: { width: 28, height: 28, borderRadius: 14, backgroundColor: C.card, alignItems: "center", justifyContent: "center" },

    confirmTitle: { fontSize: 20, fontWeight: "600", color: C.text, marginBottom: 14 },
    confirmText: { fontSize: 16, lineHeight: 24, color: C.text },
    confirmActions: { flexDirection: "row", justifyContent: "flex-end", gap: 12, marginTop: 20 },
    confirmBtn: { minHeight: 40, borderRadius: 8, paddingHorizontal: 12 },
})
