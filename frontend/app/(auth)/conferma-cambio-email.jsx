import { useEffect, useRef, useState } from 'react'
import { ActivityIndicator, View, Text, StyleSheet } from 'react-native'
import { useLocalSearchParams, useRouter } from 'expo-router'
import api from '../../config/httpClient'
import { deleteItem } from '../../config/storage'
import { C, apiErrorMessage } from '../../constants/caregiver'
import { AuthHeader, AuthIntro, AuthButton, authStyles } from '../../components/auth/AuthUi'

export default function ConfirmEmailChange() {
    const { token } = useLocalSearchParams()
    const router = useRouter()
    const started = useRef(false)
    const [status, setStatus] = useState('loading')
    const [error, setError] = useState('')
    useEffect(() => {
        if (started.current) return
        started.current = true
        const confirm = async () => {
            if (typeof token !== 'string' || !token) {
                setError('Il link di conferma non è completo.')
                setStatus('error')
                return
            }
            try {
                await api.post('/auth/change-email/confirm', { token })
                await deleteItem('accessToken')
                await deleteItem('refreshToken')
                setStatus('success')
            } catch (failure) {
                setError(apiErrorMessage(failure, 'Il link non è valido oppure è scaduto.'))
                setStatus('error')
            }
        }
        confirm()
    }, [token])
    return (
        <View style={authStyles.page}>
            <AuthHeader />
            <View style={styles.content}>
                {status === 'loading' ? <ActivityIndicator size="large" color={C.primary} /> : (
                    <>
                        <AuthIntro title={status === 'success' ? 'Email aggiornata' : 'Conferma non riuscita'}>
                            {status === 'success' ? 'Accedi di nuovo con il nuovo indirizzo email.' : error}
                        </AuthIntro>
                        <AuthButton label="Vai all’accesso" onPress={() => router.replace('/login')} />
                    </>
                )}
            </View>
        </View>
    )
}

const styles = StyleSheet.create({ content: { flex: 1, padding: 24, justifyContent: 'center', maxWidth: 480, width: '100%', alignSelf: 'center' } })
