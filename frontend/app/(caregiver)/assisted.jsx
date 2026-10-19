import { StyleSheet } from 'react-native'
import ThemedView from '../../components/ThemedView'
import ThemedText from '../../components/ThemedText'

const Assisted = () => (
	<ThemedView style={styles.container} safe={true}>
		<ThemedText title={true}>Assistiti</ThemedText>
		<ThemedText>La lista degli assistiti sarà disponibile prossimamente.</ThemedText>
	</ThemedView>
)

export default Assisted

const styles = StyleSheet.create({
	container: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24 },
})
