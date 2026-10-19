import { StyleSheet } from 'react-native'
import ThemedView from '../../components/ThemedView'
import ThemedText from '../../components/ThemedText'

const EditPassword = () => (
	<ThemedView style={styles.container} safe={true}>
		<ThemedText title={true}>Modifica Password</ThemedText>
		<ThemedText>La modifica della password sarà disponibile prossimamente.</ThemedText>
	</ThemedView>
)

export default EditPassword

const styles = StyleSheet.create({
	container: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24 },
})
