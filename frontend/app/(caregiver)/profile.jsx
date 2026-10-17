import { StyleSheet, Image, ScrollView, useColorScheme, ActivityIndicator } from 'react-native'
import { useState, useEffect } from 'react'

import ThemedView from '../../components/ThemedView'
import Spacer from '../../components/Spacer'
import ThemedText from '../../components/ThemedText'
import ThemedButton from '../../components/ThemedButton'
import { Colors } from '../../constants/Colors'
import { router } from 'expo-router'
import api from '../../config/httpClient.js'

const Profile = () => {
    const colorScheme = useColorScheme()
    const theme = Colors[colorScheme] ?? Colors.light
    const [profile, setProfile] = useState(null)
    const [errorMessage, setErrorMessage] = useState('')

    useEffect(() => {
        const loadProfile = async () => {
            try {
                const response = await api.get('/me/profile')
                setProfile(response.data)
            } catch (error) {
                setErrorMessage(error.response ? error.response.data.detail || 'Impossibile caricare il profilo' : 'Unable to reach the server')
            }
        }
        loadProfile()
    }, [])

    const handleLogout = () => {router.replace('/logout')}
    const handleModify = () => {router.push('/editProfile')} //poi verrà rimpiazzata con .replace, cosi che l'utente non possa piu tornare indietro, come sopra
    const handleModifyPassword = () => {router.push('/editPassword')}

    if (errorMessage) {
        return (
            <ThemedView style={styles.container}>
                <ThemedText style={{ color: 'red' }}>{errorMessage}</ThemedText>
            </ThemedView>
        )
    }

    if (!profile) {
        return (
            <ThemedView style={styles.container}>
                <ActivityIndicator size="large" />
            </ThemedView>
        )
    }

    return (
        <ThemedView style={styles.container}>
            <ScrollView
                contentContainerStyle={styles.scrollContent}
            >
                <Spacer height={60}/>

                {/* Avatar */}
                <Image
                    source={require('../../assets/img/fotoProfilo.jpg')}
                    style={[styles.avatar, { borderColor: Colors.primary, backgroundColor: 'red'}]}
                />

                <Spacer height={12}/>

                {/* Nome utente */}

                <ThemedText title={true} style={styles.nome}>
                    {profile.name} {profile.surname}
                </ThemedText>

                <Spacer height={30}/>

                {/* Card Informazioni Personali */}
                <ThemedView style={[styles.card, {backgroundColor: theme.uiBackground }]}>
                    <ThemedText style={styles.label}>Email</ThemedText>
                    <ThemedText style={styles.value}>{profile.email}</ThemedText>

                    <Spacer height={16} />

                    <ThemedText style={styles.label}>Phone</ThemedText>
                    <ThemedText style={styles.value}>{profile.phone}</ThemedText>

                </ThemedView>

                <Spacer height={30}/>

                {/* Card Informazioni Utente */}
                <ThemedView style={[styles.card, {backgroundColor: theme.uiBackground }]}>
                    <ThemedText style={styles.label}>Date of Birth</ThemedText>
                    <ThemedText style={styles.value}>{profile.birthDate}</ThemedText>

                </ThemedView>

                <Spacer height={30} />

                <ThemedButton
                    style={styles.input}
                    onPress={handleModify}
                >
                    <ThemedText style={styles.btnText}>Modify Profile</ThemedText>
                </ThemedButton>

                <Spacer height={15} />

                <ThemedButton
                    style={styles.input}
                    onPress={handleModifyPassword}
                >
                    <ThemedText style={styles.btnText}>Modify Password</ThemedText>
                </ThemedButton>

                <Spacer height={15} />

                <ThemedButton style={[styles.input, { backgroundColor: Colors.warning }]} onPress={handleLogout}>
                    <ThemedText style={styles.btnText}>Logout</ThemedText>
                </ThemedButton>

                <Spacer height={50}/>
            </ScrollView>
        </ThemedView>
    )
}

export default Profile

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
    nome: {
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
    value: {
        fontSize: 16,
        fontWeight: '500',
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
