import { StyleSheet, Text } from 'react-native'
import { Link } from 'expo-router'

// themed components 
import ThemedView from '../../components/ThemedView'
import Spacer from '../../components/Spacer'
import ThemedText from '../../components/ThemedText'
import ThemedButton from '../../components/ThemedButton'

const Register = () => {

    const handleSubmit = () => {
        console.log('register form submitted')
    }

    return(
        <ThemedView style={styles.container}>
        <Spacer/>
        <ThemedText title={true} style={styles.title}> 
            Register For an Account
        </ThemedText>

        <ThemedButton onPress={handleSubmit}>
            <Text style={{ color: '#f2f2f2'}}>Register</Text>
        </ThemedButton>

        <Spacer height={100}/>
        <Link href='/register'>
            <ThemedText style={{ textAlign: 'center' }}>
                Login instead 
            </ThemedText>
        </Link>

        </ThemedView>
    )
    
}

export default Register

const styles = StyleSheet.create({
    container: {
        flex: 1,
        justifyContent: 'center',
        alignItems: 'center',
    },
    title: {
        textAlign: 'center',
        fontSize: 18,
        marginBottom: 30, 
    }
})