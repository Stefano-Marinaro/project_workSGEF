import { StyleSheet } from 'react-native'
import ThemedView from '../../components/ThemedView'
import ThemedText from '../../components/ThemedText'

const Group = () => (
	<ThemedView style={styles.container} safe={true}>
		<ThemedText title={true}>Gruppo di cura</ThemedText>
		<ThemedText>La gestione dei gruppi sarà disponibile prossimamente.</ThemedText>
	</ThemedView>
)

export default Group

const styles = StyleSheet.create({
	container: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24 },
})
