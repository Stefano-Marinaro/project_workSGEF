import { StyleSheet } from 'react-native'
import { useLocalSearchParams } from 'expo-router'
import ThemedView from '../../../components/ThemedView'
import ThemedText from '../../../components/ThemedText'

const AssistedDetail = () => {
	const { id } = useLocalSearchParams()

	return (
		<ThemedView style={styles.container} safe={true}>
			<ThemedText title={true}>Dettaglio assistito</ThemedText>
			<ThemedText>Assistito #{id} — dettaglio disponibile prossimamente.</ThemedText>
		</ThemedView>
	)
}

export default AssistedDetail

const styles = StyleSheet.create({
	container: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24 },
})
