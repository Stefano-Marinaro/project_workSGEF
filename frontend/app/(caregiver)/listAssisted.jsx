import { StyleSheet } from 'react-native'
import ThemedView from '../../components/ThemedView'
import ThemedText from '../../components/ThemedText'

const listAssisted = () => (
	<ThemedView style={styles.container} safe={true}>
		<ThemedText title={true}>List Assisted</ThemedText>
		<ThemedText>La gestione degli assistiti sarà disponibile prossimamente.</ThemedText>
	</ThemedView>
)

export default listAssisted

const styles = StyleSheet.create({
	container: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24 },
})
