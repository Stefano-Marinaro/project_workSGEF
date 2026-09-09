// Stesso schema di profile.jsx, 
// ma con campi editabili tramite ThemedTextInput 
// invece di ThemedText, e uno state iniziale precompilato coi dati mock

import { StyleSheet, Image, ScrollView, useColorScheme } from 'react-native'
import { useState } from 'react'
import { useRouter } from 'expo-router'

import ThemedView from '../../components/ThemedView'
import Spacer from '../../components/Spacer'
import ThemedText from '../../components/ThemedText'
import ThemedTextInput from '../../components/ThemedTextInput'
import ThemedButton from '../../components/ThemedButton'
import { Colors } from '../../constants/Colors'

const MOCK_USER = {
    name: 'Mario Rossi',
    ruolo: 'Son',
    email: 'mariorossi@gmail.com',
    dataIscrizione: '12/06/2026',
    password: 'mariorossi12',
    avatar: require('../../assets/img/fotoProfilo.jpg')
}

const EditProfile = () => {
    const colorScheme = useColorScheme()
    const theme = Colors[colorScheme] ?? Colors.light
    const router = useRouter()

    // stato locale precompilato coi dati mock (per ora, poi arriverà dal backend)
    const [name, setName] = useState(MOCK_USER.name)
    const [email, setEmail] = useState(MOCK_USER.email)
    const [password, setPassword] = useState(MOCK_USER.password)

    const handleSave = () => {
        const payload = { name, email, password }
        console.log('profile update submitted', payload)
        router.back() // torna alla pagina profile
    }

    return (
        <ThemedView style={styles.container}>
            <ScrollView contentContainerStyle={styles.scrollContent}>
                <Spacer height={60} />

                {/* Avatar (per ora non modificabile, solo visualizzato) */}
                <Image
                    source={MOCK_USER.avatar}
                    style={[styles.avatar, { borderColor: Colors.primary }]}
                />

                <Spacer height={12} />

                <ThemedText title={true} style={styles.title}>
                    Edit Profile
                </ThemedText>

                <Spacer height={30} />

                {/* Card campi modificabili */}
                <ThemedView style={[styles.card, { backgroundColor: theme.uiBackground }]}>
                    <ThemedText style={styles.label}>Name</ThemedText>
                    <ThemedTextInput
                        style={styles.textInput}
                        value={name}
                        onChangeText={setName}
                    />

                    <Spacer height={16} />

                    <ThemedText style={styles.label}>Email</ThemedText>
                    <ThemedTextInput
                        style={styles.textInput}
                        value={email}
                        onChangeText={setEmail}
                        keyboardType="email-address"
                    />

                    <Spacer height={16} />

                    <ThemedText style={styles.label}>Password</ThemedText>
                    <ThemedTextInput
                        style={styles.textInput}
                        value={password}
                        onChangeText={setPassword}
                        secureTextEntry
                    />
                </ThemedView>

                <Spacer height={30} />

                <ThemedButton style={styles.input} onPress={handleSave}>
                    <ThemedText style={styles.btnText}>Save Changes</ThemedText>
                </ThemedButton>

                <Spacer height={15} />

                <ThemedButton
                    style={[styles.input, { backgroundColor: Colors.warning }]}
                    onPress={() => router.back()}
                >
                    <ThemedText style={styles.btnText}>Cancel</ThemedText>
                </ThemedButton>

                <Spacer height={50} />
            </ScrollView>
        </ThemedView>
    )
}

export default EditProfile

const styles = StyleSheet.create({
    container: {
        flex: 1,
    },
    scrollContent: {
        alignItems: 'center',
        paddingBottom: 40,
    },
    avatar: {
        width: 110,
        height: 110,
        borderRadius: 55,
        borderWidth: 3,
        alignSelf: 'center',
    },
    title: {
        fontSize: 20,
        fontWeight: 'bold',
        textAlign: 'center',
    },
    card: {
        width: '85%',
        borderRadius: 12,
        padding: 20,
    },
    label: {
        fontSize: 13,
        opacity: 0.6,
        marginBottom: 4,
    },
    textInput: {
        width: '100%',
    },
    input: {
        width: '80%',
        marginBottom: 0,
    },
    btnText: {
        color: '#f2f2f2',
        textAlign: 'center',
    },
})