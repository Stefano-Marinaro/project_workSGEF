import { StyleSheet } from 'react-native'
import ThemedView from '../../components/ThemedView'
import ThemedText from '../../components/ThemedText'
import ThemedButton from '../../components/ThemedButton'
import Spacer from '../../components/Spacer'
import { Colors } from '../../constants/Colors'
import { router } from 'expo-router'

const handleLogout = () => {router.push('/logout')} //poi verrà rimpiazzata con .replace, cosi che l'utente non possa piu tornare indietro


const AssociationProfile = () => (
	<ThemedView style={styles.container} safe={true}>
		<ThemedText title={true}>Profilo associazione</ThemedText>
		<ThemedText style={styles.text}>Croce Rossa Perugia</ThemedText>
		<ThemedText>contatti@crocerossa.example</ThemedText>

		<Spacer height={15} />

		<ThemedButton style={[styles.input, { backgroundColor: Colors.warning }]} onPress={handleLogout}>
			<ThemedText style={styles.btnText}>Logout</ThemedText>
		</ThemedButton>

	</ThemedView>
)

export default AssociationProfile

const styles = StyleSheet.create({
	container: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24 },
	text: { marginTop: 16, fontSize: 18 },
	btnText: { color: '#f2f2f2', textAlign: 'center' },
})
