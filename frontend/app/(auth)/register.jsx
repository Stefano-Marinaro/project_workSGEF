import { StyleSheet, Text } from 'react-native'
import { Link } from 'expo-router'

// themed components
import ThemedView from  '../../components/ThemedView.jsx'
import Spacer from '../../components/Spacer.jsx'
import ThemedText from '../../components/ThemedText.jsx'
import ThemedButton from '../../components/ThemedButton.jsx'
import ThemedTextInput from '../../components/ThemedTextInput.jsx'
import { useState } from 'react'
import { TouchableWithoutFeedback } from 'react-native'

const Register = () => {

    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')

    const handleSubmit = () => {
        console.log('Register form submitted', email, password)
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
    }
})

