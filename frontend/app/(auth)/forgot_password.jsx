import { useState } from 'react'
import { Keyboard, Modal, Pressable, Platform, StyleSheet, Text, TouchableWithoutFeedback, View, useColorScheme,
} from 'react-native'
import api from '../../config/httpClient.js'

import ThemedButton from '../../components/ThemedButton.jsx'
import ThemedText from '../../components/ThemedText.jsx'
import ThemedTextInput from '../../components/ThemedTextInput.jsx'
import ThemedView from '../../components/ThemedView.jsx'
import { Colors } from '../../constants/Colors.js'


const ForgotPassword = () => {
	const [email, setEmail] = useState('')
	const [showConfirmation, setShowConfirmation] = useState(false)
	const [errorMessage, setErrorMessage] = useState('')
	const colorScheme = useColorScheme()
	const theme = Colors[colorScheme] ?? Colors.light

	// sul web TouchableWithoutFeedback blocca i click sugli input: lì basta una View
	const Wrapper = Platform.OS === 'web' ? View : TouchableWithoutFeedback
	const wrapperProps = Platform.OS === 'web' ? { style: { flex: 1 }} : { onPress: Keyboard.dismiss }

	const handleSubmit = async () => {
    Keyboard.dismiss()
    setErrorMessage('')
    try {
        // il backend risponde sempre 204, anche se l'email non è registrata
        await api.post('/auth/forgot-password', { email })
        setShowConfirmation(true)
    } catch (error) {
        // con error.response il server ha risposto (400 = email non valida), senza non è raggiungibile
        setErrorMessage(error.response ? 'Enter a valid email address' : 'Unable to reach the server')
    }
}

	return (
		<Wrapper {...wrapperProps}>
			<ThemedView style={styles.container}>
				<View style={styles.form}>
					<ThemedText title style={styles.title}>
						Recupera la password
					</ThemedText>

					<ThemedText style={styles.description}>
						Inserisci la tua email e ti invieremo le istruzioni per
						ripristinare la password.
					</ThemedText>

					<ThemedTextInput
						style={styles.input}
						placeholder="Inserisci email"
						placeholderTextColor={theme.iconColor}
						keyboardType="email-address"
						autoCapitalize="none"
						autoCorrect={false}
						onChangeText={setEmail}
						value={email}
					/>

					{errorMessage !== '' && <Text style={styles.error}>{errorMessage}</Text>}

					<ThemedButton style={styles.submitButton} onPress={handleSubmit}>
						<Text style={styles.buttonText}>Invia richiesta</Text>
					</ThemedButton>
				</View>

				<Modal
					visible={showConfirmation}
					transparent
					animationType="fade"
					onRequestClose={() => setShowConfirmation(false)}
				>
					<View style={styles.modalBackdrop}>
						<View style={styles.confirmationCard}>
							<Text style={styles.confirmationTitle}>Richiesta inviata</Text>
							<Text style={styles.confirmationMessage}>
								If the address is registered, you will receive an email with instructions to reset your password.
							</Text>
							<Pressable
								accessibilityRole="button"
								onPress={() => setShowConfirmation(false)}
								style={({ pressed }) => [
									styles.closeButton,
									pressed && styles.pressed,
								]}
							>
								<Text style={styles.closeButtonText}>Chiudi</Text>
							</Pressable>
						</View>
					</View>
				</Modal>
			</ThemedView>
		</Wrapper>
	)
}

export default ForgotPassword

const styles = StyleSheet.create({
	container: {
		flex: 1,
		alignItems: 'center',
		justifyContent: 'center',
		paddingHorizontal: 24,
	},
	form: {
		width: '100%',
		maxWidth: 420,
		alignItems: 'center',
	},
	title: {
		fontSize: 22,
		fontWeight: 'bold',
		marginBottom: 12,
		textAlign: 'center',
	},
	description: {
		lineHeight: 21,
		marginBottom: 28,
		maxWidth: 340,
		textAlign: 'center',
	},
	input: {
		width: '100%',
		marginBottom: 16,
	},
	submitButton: {
		minWidth: 180,
		alignItems: 'center',
	},
	buttonText: {
		color: '#f2f2f2',
		fontWeight: '600',
	},
	modalBackdrop: {
		flex: 1,
		alignItems: 'center',
		justifyContent: 'center',
		backgroundColor: 'rgba(0, 0, 0, 0.45)',
		padding: 24,
	},
	confirmationCard: {
		width: '100%',
		maxWidth: 360,
		alignItems: 'center',
		backgroundColor: '#2e8b57',
		borderRadius: 12,
		padding: 24,
		elevation: 5,
		shadowColor: '#000',
		shadowOffset: { width: 0, height: 3 },
		shadowOpacity: 0.25,
		shadowRadius: 6,
	},
	confirmationTitle: {
		color: '#fff',
		fontSize: 18,
		fontWeight: 'bold',
		marginBottom: 10,
	},
	confirmationMessage: {
		color: '#fff',
		fontSize: 15,
		lineHeight: 21,
		textAlign: 'center',
	},
	closeButton: {
		backgroundColor: '#fff',
		borderRadius: 6,
		marginTop: 20,
		paddingHorizontal: 22,
		paddingVertical: 10,
	},
	closeButtonText: {
		color: '#2e8b57',
		fontWeight: '600',
	},
	pressed: {
		opacity: 0.8,
	},
	error: {
		color: '#c0392b', 
		marginBottom: 12, 
		textAlign: 'center'
	},
})
