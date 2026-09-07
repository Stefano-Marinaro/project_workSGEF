import { FlatList, StyleSheet } from 'react-native'
import ThemedView from '../../components/ThemedView'
import ThemedText from '../../components/ThemedText'
import ThemedCard from '../../components/ThemedCard'
import ThemedButton from '../../components/ThemedButton'
import Spacer from '../../components/Spacer'

const REQUESTS = [
	{ id: '1', person: 'Mario Rossi', date: '10/09/2026 · 09:30', route: 'Via Roma 10 → Ospedale Santa Maria' },
	{ id: '2', person: 'Anna Verdi', date: '14/09/2026 · 11:00', route: 'Via dei Filosofi 12 → Clinica San Sisto' },
]

const Requests = () => (
	<ThemedView style={styles.container} safe={true}>
		<ThemedText title={true} style={styles.heading}>Richieste pendenti</ThemedText>
		<Spacer height={20} />
		<FlatList
			data={REQUESTS}
			keyExtractor={(item) => item.id}
			contentContainerStyle={styles.list}
			ItemSeparatorComponent={() => <Spacer height={12} />}
			renderItem={({ item }) => (
				<ThemedCard style={styles.card}>
					<ThemedText title={true}>{item.person}</ThemedText>
					<ThemedText>{item.date}</ThemedText>
					<ThemedText>{item.route}</ThemedText>
					<ThemedView style={styles.actions}>
						<ThemedButton style={styles.action}><ThemedText style={styles.buttonText}>Accetta</ThemedText></ThemedButton>
						<ThemedButton style={[styles.action, styles.reject]}><ThemedText style={styles.buttonText}>Rifiuta</ThemedText></ThemedButton>
					</ThemedView>
				</ThemedCard>
			)}
		/>
	</ThemedView>
)

export default Requests

const styles = StyleSheet.create({
	container: { flex: 1 },
	heading: { textAlign: 'center', fontSize: 20 },
	list: { paddingHorizontal: 20, paddingBottom: 30 },
	card: { width: '100%' },
	actions: { flexDirection: 'row', gap: 10, backgroundColor: 'transparent' },
	action: { flex: 1 },
	reject: { backgroundColor: '#8884a0' },
	buttonText: { color: '#fff', textAlign: 'center' },
})
