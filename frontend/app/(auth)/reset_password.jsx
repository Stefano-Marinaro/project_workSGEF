import { useState } from 'react'
import { View, ScrollView, Text, StyleSheet, KeyboardAvoidingView, Platform } from 'react-native'
import { useLocalSearchParams, useRouter } from 'expo-router'
import api from '../../config/httpClient.js'
import { AuthHeader, AuthIntro, AuthField, AuthButton, AuthSwitch, authStyles } from '../../components/auth/AuthUi'

// Pagina aperta dal link nell'email di recupero (Frontend:ResetPasswordUrl), nello stile del login
const ResetPassword = () => {
	const { token } = useLocalSearchParams()
	const router = useRouter()
	const [password, setPassword] = useState('')
	const [confirm, setConfirm] = useState('')
	const [errors, setErrors] = useState({})
	const [errorMessage, setErrorMessage] = useState('')
	const [loading, setLoading] = useState(false)
	const [done, setDone] = useState(false)

	const handleSubmit = async () => {
		// stessa regola del validator del backend (almeno 8 caratteri)
		const found = {}
		if (password.length < 8) found.password = 'La password deve avere almeno 8 caratteri.'
		if (!confirm) found.confirm = 'Conferma la password.'
		else if (confirm !== password) found.confirm = 'Le password non coincidono.'
		setErrors(found)
		setErrorMessage('')
		if (Object.keys(found).length) return

		setLoading(true)
		try {
			await api.post('/auth/reset-password', { token, newPassword: password })
			setDone(true)
		} catch (e) {
			// 404 = token inesistente, 403 = token già usato o scaduto
			setErrorMessage(e.response ? e.response.data?.detail || 'Link non valido o scaduto.' : 'Impossibile raggiungere il server.')
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
						{done ? (
							<>
								<AuthIntro title="Password aggiornata">Ora puoi accedere con la nuova password.</AuthIntro>
								<AuthButton label="Vai al login" onPress={() => router.replace('/login')} />
							</>
						) : (
							<>
								<AuthIntro title="Imposta una nuova password">Scegli una password di almeno 8 caratteri.</AuthIntro>
								<AuthField
									icon="lock-closed-outline"
									placeholder="Almeno 8 caratteri"
									secure
									autoComplete="new-password"
									value={password}
									onChangeText={setPassword}
									error={errors.password}
								/>
								<AuthField
									icon="lock-closed-outline"
									placeholder="Conferma password"
									secure
									autoComplete="new-password"
									value={confirm}
									onChangeText={setConfirm}
									onSubmitEditing={handleSubmit}
									error={errors.confirm}
									style={styles.lastField}
								/>
								<View style={styles.submit}>
									<AuthButton label="Salva password" onPress={handleSubmit} loading={loading} />
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

export default ResetPassword

const styles = StyleSheet.create({
	flex: { flex: 1 },
	lastField: { marginBottom: 0 },
	submit: { marginTop: 28 },
})
