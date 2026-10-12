import { useState } from 'react'
import { Keyboard, Platform, StyleSheet, Text, TouchableWithoutFeedback, View } from 'react-native'
import { Link, useLocalSearchParams } from 'expo-router'

import api from '../../config/httpClient.js'
import ThemedView from '../../components/ThemedView.jsx'
import ThemedText from '../../components/ThemedText.jsx'
import ThemedTextInput from '../../components/ThemedTextInput.jsx'
import ThemedButton from '../../components/ThemedButton.jsx'

const ResetPassword = () => {
    const { token } = useLocalSearchParams()
    const [password, setPassword] = useState('')
    const [confirmPassword, setConfirmPassword] = useState('')
    const [errorMessage, setErrorMessage] = useState('')
    const [done, setDone] = useState(false)

    // stesso accorgimento di forgot_password per il web
    const Wrapper = Platform.OS === 'web' ? View : TouchableWithoutFeedback
    const wrapperProps = Platform.OS === 'web' ? { style: { flex: 1 } } : { onPress: Keyboard.dismiss }

    const handleSubmit = async () => {
        Keyboard.dismiss()
        setErrorMessage('')

        // controlli lato client: stessa regola del validator del backend
        if (password.length < 8) { setErrorMessage('Password must be at least 8 characters'); return }
        if (password !== confirmPassword) { setErrorMessage('Passwords do not match'); return }

        try {
            await api.post('/auth/reset-password', { token, newPassword: password })
            setDone(true)
        } catch (error) {
            // 404 = token inesistente, 403 = token già usato o scaduto
            setErrorMessage(error.response ? error.response.data.detail || 'Invalid or expired link' : 'Unable to reach the server')
        }
    }

    // password cambiata: mostriamo solo il messaggio di successo
    if (done) {
        return (
            <ThemedView style={styles.container}>
                <ThemedText title style={styles.title}>Password updated</ThemedText>
                <ThemedText style={styles.message}>You can now log in with your new password.</ThemedText>
                <Link href="/login" asChild>
                    <ThemedButton><Text style={styles.buttonText}>Go to login</Text></ThemedButton>
                </Link>
            </ThemedView>
        )
    }

    return (
        <Wrapper {...wrapperProps}>
            <ThemedView style={styles.container}>
                <ThemedText title style={styles.title}>Set a new password</ThemedText>

                <ThemedTextInput style={styles.input} placeholder="New password"
                    secureTextEntry value={password} onChangeText={setPassword} />
                <ThemedTextInput style={styles.input} placeholder="Confirm password"
                    secureTextEntry value={confirmPassword} onChangeText={setConfirmPassword} />

                {errorMessage !== '' && <Text style={styles.error}>{errorMessage}</Text>}

                <ThemedButton onPress={handleSubmit}>
                    <Text style={styles.buttonText}>Save password</Text>
                </ThemedButton>
            </ThemedView>
        </Wrapper>
    )
}

export default ResetPassword

const styles = StyleSheet.create({
    container: { 
        flex: 1, 
        alignItems: 'center', 
        justifyContent: 'center', 
        paddingHorizontal: 24 
    },
    title: { 
        fontSize: 22, 
        fontWeight: 'bold', 
        marginBottom: 20, 
        textAlign: 'center' 
    },
    message: { 
        lineHeight: 22, 
        marginBottom: 24, 
        textAlign: 'center' 
    },
    input: { 
        width: '100%', 
        maxWidth: 420, 
        marginBottom: 16 
    },
    error: { 
        color: '#c0392b', 
        marginBottom: 12, 
        textAlign: 'center' 
    },
    buttonText: { 
        color: '#f2f2f2', 
        fontWeight: '600' 
    },
})