import { useEffect, useRef, useState } from 'react'
import { View, Text, ScrollView, ActivityIndicator, StyleSheet } from 'react-native'
import { useLocalSearchParams, useRouter } from 'expo-router'

import api from '../../config/httpClient.js'
import { C, apiErrorMessage } from '../../constants/caregiver'
import { Icon } from '../../components/caregiver/ui'
import { AuthHeader, AuthIntro, AuthButton, authStyles } from '../../components/auth/AuthUi'

// Pagina aperta dal link nell'email di conferma (Frontend:VerifyEmailUrl = /verify_email?token=...).
// Verifica il token appena si apre e mostra "Email confermata" con il pulsante per accedere.
const VerifyEmail = () => {
    const { token } = useLocalSearchParams()
    const router = useRouter()

    // 'verifying' | 'success' | 'error'
    const [status, setStatus] = useState('verifying')
    const [errorMessage, setErrorMessage] = useState('')
    // Il token è monouso: evita una seconda chiamata se l'effetto viene rieseguito
    const started = useRef(false)

    useEffect(() => {
        if (started.current) return
        started.current = true

        const verify = async () => {
            if (!token) {
                setErrorMessage('Il link di conferma non è completo.')
                setStatus('error')
                return
            }
            try {
                await api.post('/auth/verify-email', { token })
                setStatus('success')
            } catch (error) {
                setErrorMessage(apiErrorMessage(error, 'Il link non è valido oppure è scaduto.'))
                setStatus('error')
            }
        }
        verify()
    }, [token])

    const goToLogin = () => router.replace('/login')

    return (
        <View style={authStyles.page}>
            <AuthHeader />
            <ScrollView contentContainerStyle={[authStyles.scroll, styles.center]}>
                <View style={authStyles.section}>
                    {status === 'verifying' ? (
                        <View style={styles.loading}>
                            <ActivityIndicator size="large" color={C.primary} />
                            <Text style={styles.loadingText}>Stiamo confermando la tua email…</Text>
                        </View>
                    ) : status === 'success' ? (
                        <>
                            <View style={[styles.symbol, styles.symbolOk]}>
                                <Icon name="checkmark" size={26} color={C.success} />
                            </View>
                            <AuthIntro title="Email confermata">
                                La tua email è stata verificata.{'\n'}Ora puoi accedere a GoCare e organizzare i tuoi trasporti.
                            </AuthIntro>
                            <AuthButton label="Accedi a GoCare" onPress={goToLogin} />
                        </>
                    ) : (
                        <>
                            <View style={[styles.symbol, styles.symbolError]}>
                                <Icon name="close" size={26} color={C.cancelled} />
                            </View>
                            <AuthIntro title="Conferma non riuscita">{errorMessage}</AuthIntro>
                            <AuthButton label="Vai al login" onPress={goToLogin} />
                        </>
                    )}
                </View>
            </ScrollView>
            <Text style={styles.footer}>I tuoi trasporti, in un unico posto.</Text>
        </View>
    )
}

export default VerifyEmail

const styles = StyleSheet.create({
    center: { justifyContent: 'center' },
    loading: { alignItems: 'center', gap: 16, paddingVertical: 24 },
    loadingText: { fontSize: 15, color: C.muted },
    symbol: { width: 72, height: 72, borderRadius: 36, alignItems: 'center', justifyContent: 'center', alignSelf: 'center', marginBottom: 24 },
    symbolOk: { backgroundColor: '#dcefe3' },
    symbolError: { backgroundColor: '#f6dde1' },
    footer: { textAlign: 'center', fontSize: 13, color: C.muted, paddingHorizontal: 24, paddingTop: 16, paddingBottom: 24 },
})
