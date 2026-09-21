import { StyleSheet, Text, Platform, Keyboard, TouchableWithoutFeedback, Pressable, View, Modal, TouchableOpacity } from 'react-native'
import { Link, useRouter } from 'expo-router'
import { useState } from 'react'
import api from '../../config/httpClient.js'


// themed components
import ThemedView from '../../components/ThemedView.jsx'
import Spacer from '../../components/Spacer.jsx'
import ThemedText from '../../components/ThemedText.jsx'
import ThemedButton from '../../components/ThemedButton.jsx'
import ThemedTextInput from '../../components/ThemedTextInput.jsx'

const Register = () => {

    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')
    const [role, setRole] = useState('caregiver')
    const router = useRouter()
    

    //Su Android il selettore di data si apre e chiude da solo appena scegli un giorno,
    //per questo devi dire a React Native che non è piu aperto con setShowDatePicker(false)

    const [errorMessage, setErrorMessage] = useState('')   // manca anche questo state, aggiungilo

const handleSubmit = async () => {
    setErrorMessage('')
    console.log('REGISTER BUTTON PRESSED');
    try {
        const endpoint = role === 'association' ? 'auth/register/association' : 'auth/register/user'  
        const response = await api.post(endpoint, { email, password })        

        console.log('Account created:', response.data.accountId)          

        router.replace({ pathname: '/login', params: { role } })
    } catch (error) {
        if (error.response) {
            setErrorMessage(error.response.data.detail || 'Registration failed')
            console.log('REGISTER ERROR:', error.response?.status, error.response?.data)
        } else {
            setErrorMessage('Unable to reach the server')
        }
    }
}

  return (
    <TouchableWithoutFeedback onPress={() => Keyboard.dismiss()}>
        <ThemedView style={styles.container}>

            <Spacer/>
            <ThemedText title={true} style={styles.title}>
                Register to your account
            </ThemedText>

            <ThemedTextInput 
                style={{ width: '80%', marginBottom: 20}}
                placeholder="Email"
                keyboardType="email-address"
                onChangeText={setEmail}
                value={email}
            />

            <ThemedTextInput 
                style={{ width: '80%', marginBottom: 20}}
                placeholder="Password"
                secureTextEntry
                onChangeText={setPassword}
                value={password}
            />


            <Spacer height={20}/>

            <ThemedText style={styles.roleLabel}>I am registering as</ThemedText>
            <View style={styles.roleOptions}>
                <Pressable
                    onPress={() => setRole('caregiver')}
                    style={[styles.roleOption, role === 'caregiver' && styles.selectedRole]}
                >
                    <ThemedText>Caregiver / Persona</ThemedText>
                </Pressable>
                <Pressable
                    onPress={() => setRole('association')}
                    style={[styles.roleOption, role === 'association' && styles.selectedRole]}
                >
                    <ThemedText>Associazione</ThemedText>
                </Pressable>
            </View>

            <Spacer height={20}/>

            <ThemedButton onPress={handleSubmit}>
                <Text style={{ color: '#f2f2f2'}}>Register</Text>
            </ThemedButton>

            {errorMessage ? <ThemedText style={{ color: 'red' }}>{errorMessage}</ThemedText> : null}

            <Spacer height={50}/>

            <Link href='/login' style={styles.link}>
                <ThemedText style={{ textAlign: 'center' }}>
                    Login instead
                </ThemedText>
            </Link>
        </ThemedView>
    </TouchableWithoutFeedback>
  )
}

export default Register

const styles = StyleSheet.create({
    container: {
        flex: 1,
        alignItems: "center", 
        justifyContent: "center"
    },
    title: {
        fontWeight: "bold",
        textAlign: "center",
        fontSize: 18,
        marginBottom: 30,
    },
    input: {
        width: '80%',
        marginBottom: 15,
    },
    btnText: {
        color: '#f2f2f2',
    },
    link: {
        marginVertical: 10,
        borderBottomWidth: 1
    },
    roleLabel: {
        alignSelf: 'flex-start',
        marginLeft: '10%',
        marginBottom: 8,
    },
    roleOptions: {
        width: '80%',
        flexDirection: 'row',
        gap: 8,
        marginBottom: 10,
    },
    roleOption: {
        flex: 1,
        padding: 12,
        borderWidth: 1,
        borderColor: '#aaa',
        borderRadius: 6,
        alignItems: 'center',
    },
    selectedRole: {
        borderColor: '#2f80ed',
        backgroundColor: '#dbeafe',
    },
    modalOverlay: {
        flex: 1,
        justifyContent: 'flex-end',
        backgroundColor: 'rgba(0,0,0,0.5)',
    },
    modalContent: {
        backgroundColor: '#ffffff',
        borderTopLeftRadius: 16,
        borderTopRightRadius: 16,
        paddingBottom: 30,
    },
    modalHeader: {
        alignItems: 'flex-end',
        padding: 15,
        borderBottomWidth: 1,
        borderBottomColor: '#eee',
        backgroundColor: '#f8f8f8',
        borderTopLeftRadius: 16,
        borderTopRightRadius: 16,
    },
    doneText: {
        color: '#007AFF',
        fontWeight: 'bold',
        fontSize: 16,
    },
})