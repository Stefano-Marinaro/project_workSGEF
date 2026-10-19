import { StyleSheet } from 'react-native'
import { useLocalSearchParams } from 'expo-router'
import ThemedView from '../../../components/ThemedView'
import ThemedText from '../../../components/ThemedText'

const TransportDetail = () => {
	const { id } = useLocalSearchParams()

	return (
		<ThemedView style={styles.container} safe={true}>
			<ThemedText title={true}>Dettaglio trasporto</ThemedText>
			<ThemedText>Trasporto #{id} — dettaglio disponibile prossimamente.</ThemedText>
		</ThemedView>
	)
}

export default TransportDetail

const styles = StyleSheet.create({
	container: { flex: 1, alignItems: 'center', justifyContent: 'center', padding: 24 },
})
