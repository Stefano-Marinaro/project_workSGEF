import { useState } from 'react'
import { View, ScrollView, Text, Pressable, StyleSheet, KeyboardAvoidingView, Platform } from 'react-native'
import { useRouter } from 'expo-router'
import api from '../../config/httpClient.js'
import { setItem } from '../../config/storage.js'
import { C, apiErrorMessage } from '../../constants/caregiver'
import { AuthHeader, AuthIntro, AuthField, AuthButton, AuthSwitch, authStyles } from '../../components/auth/AuthUi'

// Mockup "Crea il tuo account".
// La stessa app serve caregiver e associazioni, quindi si sceglie il tipo di account.
// Il backend registra solo email e password: nome e cognome del caregiver vengono
// ricordati sul dispositivo e proposti nel "Modifica profilo" dopo il primo accesso.
const Register = () => {
    const router = useRouter()
    const [role, setRole] = useState('caregiver')
    const [name, setName] = useState('')
    const [surname, setSurname] = useState('')
    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')
    const [confirm, setConfirm] = useState('')
    const [errors, setErrors] = useState({})
    const [errorMessage, setErrorMessage] = useState('')
    const [loading, setLoading] = useState(false)

    const isCaregiver = role === 'caregiver'

    const handleSubmit = async () => {
        const found = {}
        if (isCaregiver && !name.trim()) found.name = 'Inserisci il nome.'
        if (isCaregiver && !surname.trim()) found.surname = 'Inserisci il cognome.'
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
            const endpoint = isCaregiver ? '/auth/register/user' : '/auth/register/association'
            await api.post(endpoint, { email: email.trim(), password })
            if (isCaregiver)
                await setItem(
                    'pendingProfile',
                    JSON.stringify({ email: email.trim().toLowerCase(), name: name.trim(), surname: surname.trim() }),
                )
            router.replace('/login')
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

                        <View style={styles.roles} accessibilityRole="radiogroup">
                            {[
                                ['caregiver', 'Caregiver'],
                                ['association', 'Associazione'],
                            ].map(([value, label]) => (
                                <Pressable
                                    key={value}
                                    onPress={() => setRole(value)}
                                    accessibilityRole="radio"
                                    accessibilityState={{ selected: role === value }}
                                    style={[styles.role, role === value && styles.roleOn]}
                                >
                                    <Text style={[styles.roleText, role === value && styles.roleTextOn]}>{label}</Text>
                                </Pressable>
                            ))}
                        </View>

                        {isCaregiver ? (
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
                        ) : null}
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
    roles: { flexDirection: 'row', gap: 4, padding: 4, borderRadius: 12, backgroundColor: C.card, marginBottom: 14 },
    role: { flex: 1, alignItems: 'center', borderRadius: 9, paddingVertical: 10 },
    roleOn: { backgroundColor: C.primary },
    roleText: { fontSize: 14, fontWeight: '600', color: C.muted },
    roleTextOn: { color: C.white },
    pair: { flexDirection: 'row', gap: 16 },
    lastField: { marginBottom: 0 },
    submit: { marginTop: 28 },
})
