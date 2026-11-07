import { useCallback, useState } from 'react'
import { SectionList, View, Text, Pressable, StyleSheet } from 'react-native'
import { useFocusEffect, useRouter } from 'expo-router'

import api from '../../config/httpClient.js'
import { C, formatTime, toIsoDay, apiErrorMessage } from '../../constants/caregiver'
import { Page, PageTitle, Loading, ErrorText, EmptyState } from '../../components/caregiver/ui'

const dayTitle = (iso) => {
	const d = new Date(iso)
	if (toIsoDay(d) === toIsoDay(new Date())) return 'Oggi'
	return d.toLocaleDateString('it-IT', { day: 'numeric', month: 'long' })
}

// Mockup "Notifiche" dell'associazione: nuove richieste e aggiornamenti sui trasporti
const AssocNotifications = () => {
	const router = useRouter()
	const [loading, setLoading] = useState(true)
	const [error, setError] = useState('')
	const [items, setItems] = useState([])

	useFocusEffect(
		useCallback(() => {
			const load = async () => {
				try {
					const { data } = await api.get('/notifications')
					setItems(data)
					setError('')
				} catch (e) {
					setError(apiErrorMessage(e, 'Impossibile caricare le notifiche.'))
				} finally {
					setLoading(false)
				}
			}
			load()
		}, []),
	)

	const markAll = async () => {
		try {
			await api.post('/notifications/read-all')
			const now = new Date().toISOString()
			setItems((list) => list.map((n) => ({ ...n, readAt: n.readAt ?? now })))
		} catch (e) {
			setError(apiErrorMessage(e))
		}
	}

	const open = (n) => {
		if (!n.readAt) {
			api.post(`/notifications/${n.id}/read`).catch(() => {})
			setItems((list) => list.map((x) => (x.id === n.id ? { ...x, readAt: new Date().toISOString() } : x)))
		}
		if (n.relatedEntityId)
			router.push({
				pathname: '/association/detail/[id]',
				params: { id: n.relatedEntityId, ...(n.type === 'NewRequest' ? { kind: 'request' } : {}) },
			})
	}

	if (loading) return <Loading />

	const sorted = [...items].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
	const sections = []
	for (const n of sorted) {
		const title = dayTitle(n.createdAt)
		const last = sections[sections.length - 1]
		if (last?.title === title) last.data.push(n)
		else sections.push({ title, data: [n] })
	}
	const unread = items.filter((n) => !n.readAt).length

	return (
		<Page>
			<SectionList
				sections={sections}
				keyExtractor={(n) => n.id}
				contentContainerStyle={styles.list}
				stickySectionHeadersEnabled={false}
				ListHeaderComponent={
					<View>
						<PageTitle>Notifiche</PageTitle>
						<ErrorText style={styles.error}>{error}</ErrorText>
						{items.length ? (
							<View style={styles.row}>
								<Text style={styles.count}>{unread} da leggere</Text>
								<Pressable onPress={markAll} hitSlop={6}>
									<Text style={styles.markAll}>Segna tutte come lette</Text>
								</Pressable>
							</View>
						) : null}
					</View>
				}
				renderSectionHeader={({ section }) => <Text style={styles.day}>{section.title}</Text>}
				ItemSeparatorComponent={() => <View style={styles.gap} />}
				renderItem={({ item }) => {
					const isUnread = !item.readAt
					return (
						<Pressable
							onPress={() => open(item)}
							style={({ pressed }) => [styles.card, isUnread && styles.cardUnread, pressed && styles.pressed]}
						>
							<Text style={[styles.title, isUnread && styles.titleUnread]}>{item.title}</Text>
							<Text style={styles.body}>{item.body}</Text>
							<Text style={styles.time}>{formatTime(item.createdAt)}</Text>
						</Pressable>
					)
				}}
				ListEmptyComponent={
					error ? null : (
						<EmptyState title="Nessuna notifica" text="Qui troverai nuove richieste e aggiornamenti sui trasporti." />
					)
				}
			/>
		</Page>
	)
}

export default AssocNotifications

const styles = StyleSheet.create({
	list: { paddingHorizontal: 20, paddingBottom: 28 },
	error: { marginBottom: 8 },
	row: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 4 },
	count: { fontSize: 13, color: C.muted },
	markAll: { fontSize: 13, fontWeight: '600', color: C.primary },
	day: { fontSize: 13, fontWeight: '600', color: C.muted, marginTop: 16, marginBottom: 10 },
	gap: { height: 12 },
	card: { backgroundColor: C.card, borderRadius: 14, padding: 16 },
	cardUnread: { borderLeftWidth: 3, borderLeftColor: C.primary },
	pressed: { opacity: 0.85 },
	title: { fontSize: 15, color: C.text },
	titleUnread: { fontWeight: '700' },
	body: { fontSize: 14, lineHeight: 21, color: C.muted, marginTop: 6 },
	time: { fontSize: 12, color: C.muted, marginTop: 8 },
})
