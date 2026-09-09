import { StyleSheet, Image, ScrollView, useColorScheme } from 'react-native'
import { useRouter } from 'expo-router'

import ThemedView from '../../components/ThemedView'
import Spacer from '../../components/Spacer'
import ThemedText from '../../components/ThemedText'
import ThemedButton from '../../components/ThemedButton'
import { Colors } from '../../constants/Colors'


const MOCK_USER = {
    name: 'Mario Rossi',
    ruolo: 'Son',
    email: 'mariorossi@gmail.com',
    dataIscrizione: '12/06/2026',
    password: 'mariorossi12', // in mock va in chiaro, poi l'API pensa a gestire la sicurezza
    avatar: require('../../assets/img/fotoProfilo.jpg')
}

const Profile = () => {
    const colorScheme = useColorScheme()
    const theme = Colors[colorScheme] ?? Colors.light
    const handleLogout = () => {router.push('/logout')} //poi verrà rimpiazzata con .replace, cosi che l'utente non possa piu tornare indietro
    return (
        <ThemedView style={styles.container}>
            <ScrollView
                contentContainerStyle={styles.scrollContent}
            >
                <Spacer height={60}/>

                {/* Avatar */}
                <Image
                    source={MOCK_USER.avatar }
                    style={[styles.avatar, { borderColor: Colors.primary, backgroundColor: 'red'}]}
                />
 
                <Spacer height={12}/>
 
                {/* Nome utente */}
 
                <ThemedText title={true} style={styles.nome}>
                    {MOCK_USER.name}
                </ThemedText>
 
                {/* Badge Ruolo */}
                <ThemedView style={[styles.badge, {backgroudColor: Colors.primary}]}>
                    <ThemedText style={styles.badgeText}>
                        {MOCK_USER.ruolo}
                    </ThemedText>
                </ThemedView>
                
 
                <Spacer height={30} />

                {/* Card Informazioni */}
                <ThemedView style={[styles.card, {backgroundColor: theme.uiBackground }]}>
                    <ThemedText style={styles.label}>Email</ThemedText>
                    <ThemedText style={styles.value}>{MOCK_USER.email}</ThemedText>

                    <Spacer height={16} />

                    <ThemedText style={styles.label}>Member since</ThemedText>
                    <ThemedText style={styles.value}>{MOCK_USER.dataIscrizione}</ThemedText>

                </ThemedView>

                <Spacer height={30}/>

                <ThemedButton 
                    style={styles.input}
                    onPress={() => router.push('/editProfile')}
                >
                    <ThemedText style={styles.btnText}>Modify Profile</ThemedText>
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
    badge: {
        marginTop: 8,
        paddingVertical: 4,
        paddingHorizontal: 14,
        borderRadius: 20,
    },
    badgeText: {
        color: '#fff',
        fontSize: 13,
        fontWeight: '600',
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

