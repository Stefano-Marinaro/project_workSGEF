import { useState } from 'react'
import { View, ScrollView, Text, StyleSheet, KeyboardAvoidingView, Platform } from 'react-native'
import { useRouter } from 'expo-router'
import api from '../../config/httpClient.js'
import { apiErrorMessage } from '../../constants/caregiver'
import { AuthHeader, AuthIntro, AuthField, AuthButton, AuthSwitch, authStyles } from '../../components/auth/AuthUi'

// Registrazione pubblica riservata ai caregiver.
// Nome e cognome vengono salvati sul server già durante la registrazione.
const Register = () => {
    const router = useRouter()
    const [name, setName] = useState('')
    const [surname, setSurname] = useState('')
    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')
    const [confirm, setConfirm] = useState('')
    const [errors, setErrors] = useState({})
    const [errorMessage, setErrorMessage] = useState('')
    const [loading, setLoading] = useState(false)

    const handleSubmit = async () => {
        const found = {}
        if (!name.trim()) found.name = 'Inserisci il nome.'
        if (!surname.trim()) found.surname = 'Inserisci il cognome.'
        if (!email.trim()) found.email = 'Inserisci la tua email.'
        else if (!/^\S+@\S+\.\S+$/.test(email.trim())) found.email = 'L’email non è valida.'
        if (password.length < 8) found.password = 'La password deve avere almeno 8 caratteri.'
        if (!confirm) found.confirm = 'Conferma la password.'
        else if (confirm !== password) found.confirm = 'Le password non coincidono.'
        setErrors(found)
        setErrorMessage('')
        if (Object.keys(found).length) return

        setLoading(true)
        try {
            await api.post('/auth/register/user', {
                email: email.trim().toLowerCase(), password,
                name: name.trim(), surname: surname.trim(),
            })
            router.replace({ pathname: '/registration_success', params: { email: email.trim() } })
        } catch (error) {
            setErrorMessage(apiErrorMessage(error, 'Registrazione non riuscita.'))
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
                        <AuthIntro title="Crea il tuo account">
                            Registrati per organizzare{'\n'}i tuoi trasporti con GoCare.
                        </AuthIntro>

                        <View style={styles.pair}>
                            <AuthField
                                icon="person-outline"
                                placeholder="Nome"
                                autoComplete="given-name"
                                value={name}
                                onChangeText={setName}
                                error={errors.name}
                                style={styles.flex}
                            />
                            <AuthField
                                icon="person-outline"
                                placeholder="Cognome"
                                autoComplete="family-name"
                                value={surname}
                                onChangeText={setSurname}
                                error={errors.surname}
                                style={styles.flex}
                            />
                        </View>
                        <AuthField
                            icon="mail-outline"
                            placeholder="Email"
                            keyboardType="email-address"
                            autoCapitalize="none"
                            autoComplete="email"
                            value={email}
                            onChangeText={setEmail}
                            error={errors.email}
                        />
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
                            <AuthButton label="Registrati" onPress={handleSubmit} loading={loading} />
                        </View>
                        {errorMessage ? <Text style={authStyles.formError}>{errorMessage}</Text> : null}

                        <AuthSwitch text="Hai già un account?" action="Accedi" onPress={() => router.replace('/login')} />
                    </View>
                </ScrollView>
            </KeyboardAvoidingView>
        </View>
    )
}

export default Register

const styles = StyleSheet.create({
    flex: { flex: 1 },
    pair: { flexDirection: 'row', gap: 16 },
    lastField: { marginBottom: 0 },
    submit: { marginTop: 28 },
})
