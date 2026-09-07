import { FlatList, StyleSheet } from 'react-native'
import ThemedView from '../../components/ThemedView'
import ThemedText from '../../components/ThemedText'
import ThemedCard from '../../components/ThemedCard'
import Spacer from '../../components/Spacer'

const ACCEPTED = [
	{ id: '1', person: 'Lucia Bianchi', date: '12/09/2026 · 08:15', route: 'Via della Pallotta → Ospedale' },
	{ id: '2', person: 'Paolo Neri', date: '16/09/2026 · 14:00', route: 'Stazione → Clinica San Sisto' },
]

const AcceptedTransport = () => (
	<ThemedView style={styles.container} safe={true}>
		<ThemedText title={true} style={styles.heading}>Trasporti accettati</ThemedText>
		<Spacer height={20} />
		<FlatList
			data={ACCEPTED}
			keyExtractor={(item) => item.id}
			contentContainerStyle={styles.list}
			ItemSeparatorComponent={() => <Spacer height={12} />}
			renderItem={({ item }) => (
				<ThemedCard style={styles.card}>
					<ThemedText title={true}>{item.person}</ThemedText>
					<ThemedText>{item.date}</ThemedText>
					<ThemedText>{item.route}</ThemedText>
					<ThemedText style={styles.status}>Accettato</ThemedText>
				</ThemedCard>
			)}
		/>
	</ThemedView>
)

export default AcceptedTransport

const styles = StyleSheet.create({
	container: { flex: 1 },
	heading: { textAlign: 'center', fontSize: 20 },
	list: { paddingHorizontal: 20, paddingBottom: 30 },
	card: { width: '100%' },
	status: { color: '#4caf50', fontWeight: '600', marginTop: 8 },
})
