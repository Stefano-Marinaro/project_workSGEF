import { StyleSheet, Text, View } from 'react-native'
import ThemedView from '../components/ThemedView'
import ThemedText from '../components/ThemedText'
import ThemedCard from '../components/ThemedCard'
import Spacer from '../components/Spacer'


const Logout = () => {
  return (
    <ThemedView>
        <Spacer height={80} />
        <ThemedCard style={{backgroundColor: "#cc475a"}}>
            <ThemedText style={styles.btnText}>Attenzione! Logout effettuato, arrivederci.</ThemedText>
        </ThemedCard>
    </ThemedView>
  )
}

export default Logout

const styles = StyleSheet.create({
    card: { backgroundColor: "#cc475a" },
    btnText: { color: '#f2f2f2', textAlign: 'center' },
})