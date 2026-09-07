import { StyleSheet } from 'react-native'
import ThemedView from '../../components/ThemedView'
import ThemedText from '../../components/ThemedText'

const AssociationProfile = () => (
	<ThemedView style={styles.container} safe={true}>
		<ThemedText title={true}>Profilo associazione</ThemedText>
		<ThemedText style={styles.text}>Croce Rossa Perugia</ThemedText>
		<ThemedText>contatti@crocerossa.example</ThemedText>
	</ThemedView>
)

export default AssociationProfile

const styles = StyleSheet.create({
	container: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24 },
	text: { marginTop: 16, fontSize: 18 },
})
