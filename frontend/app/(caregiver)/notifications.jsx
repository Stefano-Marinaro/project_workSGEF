import { StyleSheet } from 'react-native'
import ThemedView from '../../components/ThemedView'
import ThemedText from '../../components/ThemedText'

const Notifications = () => (
	<ThemedView style={styles.container} safe={true}>
		<ThemedText title={true}>Notifiche</ThemedText>
		<ThemedText>L'elenco delle notifiche sarà disponibile prossimamente.</ThemedText>
	</ThemedView>
)

export default Notifications

const styles = StyleSheet.create({
	container: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24 },
})
