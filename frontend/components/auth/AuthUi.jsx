// Elementi grafici delle schermate di accesso (mockup "Bentornato in GoCare" / "Crea il tuo account").
import { forwardRef, useState } from "react"
import { View, Text, TextInput, Pressable, Image, StyleSheet } from "react-native"
import { useSafeAreaInsets } from "react-native-safe-area-context"
import { C } from "../../constants/caregiver"
import { Icon } from "../caregiver/ui"

// Logo + "GoCare" in alto al centro. logoRef serve all'animazione di apertura del login.
export const AuthHeader = forwardRef(({ hideLogo, onLogoLayout }, logoRef) => {
    const insets = useSafeAreaInsets()
    return (
        <View style={[styles.header, { paddingTop: insets.top + 30 }]}>
            <View style={styles.brand}>
                <Image
                    ref={logoRef}
                    onLayout={onLogoLayout}
                    source={require("../../assets/img/gocareLogo.png")}
                    style={[styles.logo, hideLogo && styles.hidden]}
                />
                <Text style={styles.brandText}>GoCare</Text>
            </View>
        </View>
    )
})

export const AuthIntro = ({ title, children }) => (
    <View style={styles.intro}>
        <Text style={styles.introTitle}>{title}</Text>
        <Text style={styles.introText}>{children}</Text>
    </View>
)

// Campo con icona a sinistra e linea sotto; con secure mostra il pulsante "Mostra / Nascondi"
export const AuthField = ({ icon, secure, error, style, ...props }) => {
    const [focused, setFocused] = useState(false)
    const [visible, setVisible] = useState(false)
    return (
        <View style={[styles.fieldWrap, style]}>
            <View style={[styles.field, focused && styles.fieldFocused, error && styles.fieldError]}>
                <Icon name={icon} size={18} color={C.muted} />
                <TextInput
                    style={styles.input}
                    placeholderTextColor="#777184"
                    secureTextEntry={secure && !visible}
                    autoCapitalize={secure ? "none" : props.autoCapitalize}
                    onFocus={() => setFocused(true)}
                    onBlur={() => setFocused(false)}
                    accessibilityLabel={props.placeholder}
                    {...props}
                />
                {secure ? (
                    <Pressable
                        onPress={() => setVisible((v) => !v)}
                        hitSlop={8}
                        accessibilityLabel={visible ? "Nascondi password" : "Mostra password"}
                    >
                        <Text style={styles.toggle}>{visible ? "Nascondi" : "Mostra"}</Text>
                    </Pressable>
                ) : null}
            </View>
            {error ? <Text style={styles.error}>{error}</Text> : null}
        </View>
    )
}

export const AuthButton = ({ label, onPress, loading }) => (
    <Pressable
        onPress={onPress}
        disabled={loading}
        style={({ pressed }) => [styles.button, (pressed || loading) && styles.pressed]}
    >
        <Text style={styles.buttonText}>{loading ? "Attendi…" : label}</Text>
    </Pressable>
)

// "Non hai un account? Registrati"
export const AuthSwitch = ({ text, action, onPress }) => (
    <View style={styles.switch}>
        <Text style={styles.switchText}>{text} </Text>
        <Pressable onPress={onPress} hitSlop={10}>
            <Text style={styles.switchAction}>{action}</Text>
        </Pressable>
    </View>
)

export const authStyles = StyleSheet.create({
    page: { flex: 1, backgroundColor: C.page },
    scroll: { flexGrow: 1 },
    section: { width: "100%", maxWidth: 440, alignSelf: "center", paddingHorizontal: 28, paddingTop: 24, paddingBottom: 24 },
    formError: { color: "#ad293b", fontSize: 13, lineHeight: 20, textAlign: "center", marginTop: 14 },
})

const styles = StyleSheet.create({
    header: { alignItems: "center", paddingBottom: 18, minHeight: 90 },
    brand: { flexDirection: "row", alignItems: "center", gap: 6 },
    logo: { width: 60, height: 60, resizeMode: "contain" },
    hidden: { opacity: 0 },
    brandText: { fontSize: 28, fontWeight: "700", letterSpacing: -1, color: C.primary },
    intro: { alignItems: "center", marginBottom: 24 },
    introTitle: { fontSize: 24, fontWeight: "600", letterSpacing: -0.5, color: C.text, marginBottom: 12, textAlign: "center" },
    introText: { fontSize: 15, lineHeight: 24, color: C.muted, textAlign: "center" },
    fieldWrap: { marginBottom: 14 },
    field: {
        flexDirection: "row",
        alignItems: "center",
        gap: 12,
        minHeight: 56,
        borderBottomWidth: 1,
        borderBottomColor: "#b8b5c7",
        paddingLeft: 2,
    },
    fieldFocused: { borderBottomColor: C.primary },
    fieldError: { borderBottomColor: "#c03d49" },
    input: { flex: 1, minWidth: 0, fontSize: 16, color: C.text, paddingVertical: 15, outlineStyle: "none" },
    toggle: { fontSize: 13, fontWeight: "600", color: C.primary, paddingHorizontal: 8, paddingVertical: 10 },
    error: { color: "#ad293b", fontSize: 13, marginTop: 6 },
    button: {
        minHeight: 52,
        borderRadius: 12,
        backgroundColor: C.primary,
        alignItems: "center",
        justifyContent: "center",
        paddingHorizontal: 20,
    },
    pressed: { opacity: 0.85 },
    buttonText: { color: C.white, fontSize: 16, fontWeight: "600" },
    switch: { flexDirection: "row", justifyContent: "center", alignItems: "center", marginTop: 18, flexWrap: "wrap" },
    switchText: { fontSize: 14, color: C.muted },
    switchAction: { fontSize: 14, fontWeight: "600", color: C.primary },
})
