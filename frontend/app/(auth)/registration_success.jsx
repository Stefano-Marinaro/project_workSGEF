import { View, Text, StyleSheet } from 'react-native'
import { useLocalSearchParams, useRouter } from 'expo-router'
import { AuthHeader, AuthButton, authStyles } from '../../components/auth/AuthUi'
import { SuccessCheck } from '../../components/caregiver/ui'
import { C } from '../../constants/caregiver'

export default function RegistrationSuccess() {
    const { email } = useLocalSearchParams()
    const router = useRouter()
    return (
        <View style={authStyles.page}>
            <AuthHeader />
            <View style={styles.content}>
                <SuccessCheck />
                <Text style={styles.title}>Account creato</Text>
                <Text style={styles.text}>Controlla la tua email{email ? ` (${email})` : ''} e apri il link di verifica per attivare l’account.</Text>
                <Text style={styles.hint}>Dopo la conferma potrai accedere a GoCare.</Text>
                <View style={styles.button}><AuthButton label="Vai all’accesso" onPress={() => router.replace('/login')} /></View>
            </View>
        </View>
    )
}

const styles = StyleSheet.create({
    content: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 24 },
    title: { fontSize: 26, fontWeight: '700', color: C.text, marginTop: 20, marginBottom: 12 },
    text: { fontSize: 16, lineHeight: 24, color: C.text, textAlign: 'center', maxWidth: 420 },
    hint: { fontSize: 14, lineHeight: 22, color: C.muted, textAlign: 'center', marginTop: 12 },
    button: { width: '100%', maxWidth: 420, marginTop: 28 },
})
