import { StyleSheet, Text, Keyboard } from 'react-native'
import { Link, useLocalSearchParams, useRouter } from 'expo-router'
import * as SecureStore from 'expo-secure-store'
import api from '../../config/httpClient.js'

// themed components
import ThemedView from  '../../components/ThemedView.jsx'
import Spacer from '../../components/Spacer.jsx'
import ThemedText from '../../components/ThemedText.jsx'
import ThemedTextInput from '../../components/ThemedTextInput.jsx'
import ThemedButton from '../../components/ThemedButton.jsx'
import { useState } from 'react'
import { TouchableWithoutFeedback, Pressable, View } from 'react-native'


const Login = () => {

    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')
    const params = useLocalSearchParams()
    const router = useRouter()
    const [role, setRole] = useState(params.role === 'association' ? 'association' : 'caregiver')

    const [errorMessage, setErrorMessage] = useState('')

    const handleSubmit = async () => {
        console.log('BUTTON PRESSED')
        setErrorMessage('')
        try {
            const response = await api.post('/auth/login', { email, password });
 
     
            console.log('Access token received:', response.data.accessToken)

            await SecureStore.setItemAsync('accessToken', response.data.accessToken)
            await SecureStore.setItemAsync('refreshToken', response.data.refreshToken)
 
            router.replace(role === 'association' ? '/association/request' : '/(caregiver)/transport')
        } catch (error) {
            if (error.response) {
       
                setErrorMessage(error.response.data.detail || 'Login failed')  
                console.log('LOGIN ERROR:', error.response?.status, error.response?.data)
           
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
                Login to your account
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

            <ThemedText style={styles.roleLabel}>Continue as</ThemedText>
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

            <ThemedButton onPress={handleSubmit}>
                <Text style={{ color: '#f2f2f2'}}>Login</Text>
            </ThemedButton>

            {errorMessage ? <ThemedText style={{ color: 'red' }}>{errorMessage}</ThemedText> : null}

            <Spacer height={20}/>

            <Link href='/forgot_password' style={styles.forgotLink}>
                <ThemedText style={styles.forgotLinkText}>
                    Forgot Password?
                </ThemedText>
            </Link>

            <Spacer height={100}/>

            <Link href='/register' style={styles.link}>
                <ThemedText style={{ textAlign: 'center' }}>
                    Register instead
                </ThemedText>
            </Link>
            
        </ThemedView>
    </TouchableWithoutFeedback>
  )
}

export default Login

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
    link: {
        marginVertical: 10,
        borderBottomWidth: 1,
        textAlign: 'center',
    },
    forgotLink: {
        marginTop: 2,
        borderBottomWidth: 1,
        borderBottomColor: '#6849a7',
    },
    forgotLinkText: {
        color: '#6849a7',
        fontSize: 14,
        textAlign: 'center',
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
  })

