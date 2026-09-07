import { StyleSheet, Text } from 'react-native'
import { Link, useRouter } from 'expo-router'

// themed components
import ThemedView from  '../../components/ThemedView.jsx'
import Spacer from '../../components/Spacer.jsx'
import ThemedText from '../../components/ThemedText.jsx'
import ThemedButton from '../../components/ThemedButton.jsx'
import ThemedTextInput from '../../components/ThemedTextInput.jsx'
import { useState } from 'react'
import { TouchableWithoutFeedback, Pressable, View } from 'react-native'

const Register = () => {

    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')
    const [role, setRole] = useState('caregiver')
    const router = useRouter()

    const handleSubmit = () => {
        console.log('Register form submitted', email, password)
        router.replace({ pathname: '/login', params: { role } })
    }

  return (
    <TouchableWithoutFeedback /*onPress={() => Keyboard.dismiss()}*/>
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


            <ThemedButton onPress={handleSubmit}>
                <Text style={{ color: '#f2f2f2'}}>Register</Text>
            </ThemedButton>

            <Spacer height={100}/>

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
})

