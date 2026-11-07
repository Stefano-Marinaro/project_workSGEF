import { useState } from 'react'
import { View, ScrollView, Text, StyleSheet, KeyboardAvoidingView, Platform } from 'react-native'
import { useRouter } from 'expo-router'
import api from '../../config/httpClient.js'
import { AuthHeader, AuthIntro, AuthField, AuthButton, AuthSwitch, authStyles } from '../../components/auth/AuthUi'

// Mockup "Password dimenticata?" e messaggio "Richiesta ricevuta"
const ForgotPassword = () => {
	const router = useRouter()
	const [email, setEmail] = useState('')
	const [error, setError] = useState('')
	const [errorMessage, setErrorMessage] = useState('')
	const [loading, setLoading] = useState(false)
	const [sent, setSent] = useState(false)

	const handleSubmit = async () => {
		setErrorMessage('')
		if (!email.trim()) return setError('Inserisci la tua email.')
		if (!/^\S+@\S+\.\S+$/.test(email.trim())) return setError('L’email non è valida.')
		setError('')
		setLoading(true)
		try {
			// il backend risponde sempre 204, anche se l'email non è registrata
			await api.post('/auth/forgot-password', { email: email.trim() })
			setSent(true)
		} catch (e) {
			// con e.response il server ha risposto (400 = email non valida), senza non è raggiungibile
			setErrorMessage(e.response ? 'Inserisci un indirizzo email valido.' : 'Impossibile raggiungere il server.')
		} finally {
			setLoading(false)
		}
	}

	return (
		<View style={authStyles.page}>
			<KeyboardAvoidingView style={styles.flex} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
				<AuthHeader />
				<ScrollView contentContainerStyle={authStyles.scroll} keyboardShouldPersistTaps="handled">
					<View style={authStyles.section}>
						{sent ? (
							<>
								<AuthIntro title="Richiesta ricevuta">
									Se l’indirizzo è associato a un account, riceverai le istruzioni per recuperare l’accesso.
								</AuthIntro>
								<AuthButton label="Torna al login" onPress={() => router.replace('/login')} />
							</>
						) : (
							<>
								<AuthIntro title="Password dimenticata?">
									Inserisci l’email del tuo account{'\n'}per recuperare l’accesso.
								</AuthIntro>
								<AuthField
									icon="mail-outline"
									placeholder="Email"
									keyboardType="email-address"
									autoCapitalize="none"
									autoComplete="email"
									value={email}
									onChangeText={setEmail}
									onSubmitEditing={handleSubmit}
									error={error}
									style={styles.lastField}
								/>
								<View style={styles.submit}>
									<AuthButton label="Recupera password" onPress={handleSubmit} loading={loading} />
								</View>
								{errorMessage ? <Text style={authStyles.formError}>{errorMessage}</Text> : null}
								<AuthSwitch text="" action="Torna al login" onPress={() => router.replace('/login')} />
							</>
						)}
					</View>
				</ScrollView>
			</KeyboardAvoidingView>
		</View>
	)
}

export default ForgotPassword

const styles = StyleSheet.create({
	flex: { flex: 1 },
	lastField: { marginBottom: 0 },
	submit: { marginTop: 28 },
})
