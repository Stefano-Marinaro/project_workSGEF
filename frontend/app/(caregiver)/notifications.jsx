import { useCallback, useState } from 'react'
import { SectionList, View, Text, Pressable, StyleSheet } from 'react-native'
import { useFocusEffect, useRouter } from 'expo-router'

import api from '../../config/httpClient.js'
import { C, formatFullDate, formatTime, toIsoDay, apiErrorMessage } from '../../constants/caregiver'
import { Page, PageTitle, Loading, ErrorText } from '../../components/caregiver/ui'

// Notifiche che riguardano un trasporto: toccandole si apre il dettaglio
const TRIP_TYPES = [
    'RequestAccepted',
    'RequestNotCovered',
    'TripStatusChanged',
    'ModificationRequested',
    'ModificationApproved',
    'ModificationRejected',
    'CompanionUpdated',
    'TripCancelledByUser',
    'TripCancelledByAssociation',
    'TripReminder',
]

const dayTitle = (iso) => {
    const day = toIsoDay(new Date(iso))
    const today = new Date()
    const yesterday = new Date()
    yesterday.setDate(today.getDate() - 1)
    if (day === toIsoDay(today)) return 'Oggi'
    if (day === toIsoDay(yesterday)) return 'Ieri'
    return formatFullDate(iso)
}

const Notifications = () => {
    const router = useRouter()
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState('')
    const [items, setItems] = useState([])

    const load = useCallback(async () => {
        try {
            const { data } = await api.get('/notifications')
            setItems(data)
            setError('')
        } catch (e) {
            setError(apiErrorMessage(e, 'Impossibile caricare le notifiche.'))
        } finally {
            setLoading(false)
        }
    }, [])

    useFocusEffect(
        useCallback(() => {
            load()
        }, [load]),
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

    const open = async (n) => {
        if (!n.readAt) {
            api.post(`/notifications/${n.id}/read`).catch(() => {})
            setItems((list) => list.map((x) => (x.id === n.id ? { ...x, readAt: new Date().toISOString() } : x)))
        }
        if (n.relatedEntityId && TRIP_TYPES.includes(n.type))
            router.push({ pathname: '/transport/[id]', params: { id: n.relatedEntityId } })
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
    const hasUnread = items.some((n) => !n.readAt)

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
                        {hasUnread ? (
                            <Pressable onPress={markAll} style={styles.markAll} hitSlop={6}>
                                <Text style={styles.markAllText}>Segna tutte come lette</Text>
                            </Pressable>
                        ) : null}
                    </View>
                }
                renderSectionHeader={({ section }) => <Text style={styles.day}>{section.title}</Text>}
                ItemSeparatorComponent={() => <View style={styles.gap} />}
                renderItem={({ item }) => {
                    const unread = !item.readAt
                    return (
                        <Pressable onPress={() => open(item)} style={({ pressed }) => [styles.card, pressed && styles.pressed]}>
                            <View style={styles.titleRow}>
                                {unread ? <View style={styles.dot} /> : null}
                                <Text style={[styles.title, unread && styles.titleUnread]}>{item.title}</Text>
                            </View>
                            <Text style={styles.body}>{item.body}</Text>
                            <Text style={styles.time}>{formatTime(item.createdAt)}</Text>
                        </Pressable>
                    )
                }}
                ListEmptyComponent={
                    error ? null : (
                        <View style={styles.empty}>
                            <Text style={styles.emptyTitle}>Nessuna notifica</Text>
                            <Text style={styles.emptyText}>Qui troverai gli aggiornamenti sulle tue richieste di trasporto.</Text>
                        </View>
                    )
                }
            />
        </Page>
    )
}

export default Notifications

const styles = StyleSheet.create({
    list: { paddingHorizontal: 20, paddingBottom: 28 },
    error: { marginBottom: 10 },
    markAll: { alignSelf: 'flex-end', marginBottom: 8 },
    markAllText: { fontSize: 13, color: C.primary },
    day: { fontSize: 14, fontWeight: '600', color: C.muted, marginTop: 16, marginBottom: 12 },
    gap: { height: 14 },
    card: { backgroundColor: C.card, borderRadius: 14, padding: 18 },
    pressed: { opacity: 0.85 },
    titleRow: { flexDirection: 'row', alignItems: 'center', gap: 10 },
    dot: { width: 7, height: 7, borderRadius: 4, backgroundColor: C.primary },
    title: { flex: 1, fontSize: 16, color: C.text, fontWeight: '400' },
    titleUnread: { fontWeight: '700' },
    body: { fontSize: 15, lineHeight: 24, color: C.muted, marginTop: 6 },
    time: { fontSize: 12, color: C.muted, marginTop: 10 },
    empty: { alignItems: 'center', paddingTop: 20 },
    emptyTitle: { fontSize: 20, fontWeight: '700', color: C.text, marginBottom: 10 },
    emptyText: { fontSize: 15, lineHeight: 24, color: C.muted, textAlign: 'center' },
})
