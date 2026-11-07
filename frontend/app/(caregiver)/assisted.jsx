import { useCallback, useState } from 'react'
import { FlatList, View, Text, Pressable, StyleSheet } from 'react-native'
import { useFocusEffect, useRouter } from 'expo-router'

import api from '../../config/httpClient.js'
import {
    C,
    ageFrom,
    cityLabel,
    tripState,
    tripTypeShort,
    formatDayMonth,
    formatTime,
    apiErrorMessage,
} from '../../constants/caregiver'
import { Page, PageTitle, Loading, Icon, Initials, StatusLine, EmptyState, ErrorText, Btn } from '../../components/caregiver/ui'

const PersonCard = ({ person, onPress }) => {
    const trip = person.nextTrip
    return (
        <Pressable onPress={onPress} style={({ pressed }) => [styles.card, pressed && styles.pressed]}>
            <View style={styles.identity}>
                <Initials name={person.name} surname={person.surname} size={46} style={styles.avatar} />
                <View style={styles.flex}>
                    <View style={styles.nameRow}>
                        <Text style={styles.name}>
                            {person.name} {person.surname}
                        </Text>
                        <Text style={styles.age}>{ageFrom(person.birthDate)}</Text>
                    </View>
                    {person.homeAddress ? <Text style={styles.city}>{cityLabel(person.homeAddress)}</Text> : null}
                </View>
            </View>
            <View style={styles.next}>
                {trip ? (
                    <>
                        <Text style={styles.nextLabel}>Prossimo trasporto · {tripTypeShort(trip.tripType)}</Text>
                        <View style={styles.nextDate}>
                            <Icon name="calendar-outline" size={16} color={C.primary} />
                            <Text style={styles.nextDateText}>
                                {formatDayMonth(trip.departureDateHour)} · {formatTime(trip.departureDateHour)}
                            </Text>
                        </View>
                        <StatusLine state={tripState(trip.status)} size={12} style={styles.nextState} />
                    </>
                ) : (
                    <View style={styles.nextDate}>
                        <Icon name="calendar-outline" size={16} color={C.muted} />
                        <Text style={styles.none}>Nessun trasporto programmato</Text>
                    </View>
                )}
            </View>
            <Icon name="chevron-forward" size={20} color={C.primary} style={styles.chevron} />
        </Pressable>
    )
}

const Assisted = () => {
    const router = useRouter()
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState('')
    const [people, setPeople] = useState([])

    useFocusEffect(
        useCallback(() => {
            const load = async () => {
                try {
                    const { data } = await api.get('/assisted')
                    setPeople(data)
                    setError('')
                } catch (e) {
                    setError(apiErrorMessage(e, 'Impossibile caricare gli assistiti.'))
                } finally {
                    setLoading(false)
                }
            }
            load()
        }, []),
    )

    if (loading) return <Loading />

    return (
        <Page>
            <FlatList
                data={people}
                keyExtractor={(p) => p.id}
                contentContainerStyle={styles.list}
                ItemSeparatorComponent={() => <View style={styles.gap} />}
                ListHeaderComponent={
                    <View>
                        <View style={styles.header}>
                            <PageTitle style={styles.title}>Assistiti</PageTitle>
                            {people.length > 0 ? (
                                <Btn
                                    label="Registra Assistito"
                                    icon="person-add-outline"
                                    onPress={() => router.push('/assisted/new')}
                                    style={styles.addBtn}
                                    textStyle={styles.addText}
                                />
                            ) : null}
                        </View>
                        <ErrorText style={styles.error}>{error}</ErrorText>
                    </View>
                }
                ListEmptyComponent={
                    error ? null : (
                        <EmptyState
                            title="Aggiungi il tuo primo assistito"
                            text="Registra la persona da accompagnare per iniziare a usare GoCare."
                            action="Registra assistito"
                            onAction={() => router.push('/assisted/new')}
                        />
                    )
                }
                renderItem={({ item }) => (
                    <PersonCard person={item} onPress={() => router.push({ pathname: '/assisted/[id]', params: { id: item.id } })} />
                )}
            />
        </Page>
    )
}

export default Assisted

const styles = StyleSheet.create({
    flex: { flex: 1 },
    list: { paddingHorizontal: 20, paddingBottom: 28 },
    gap: { height: 12 },
    header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', gap: 12, marginTop: 12, marginBottom: 16 },
    title: { marginTop: 0, marginBottom: 0 },
    addBtn: { minHeight: 44, paddingHorizontal: 14 },
    addText: { fontSize: 14, fontWeight: '500' },
    error: { marginBottom: 12 },
    card: {
        backgroundColor: C.card,
        borderRadius: 14,
        borderWidth: 1,
        borderColor: C.primary,
        paddingVertical: 18,
        paddingLeft: 16,
        paddingRight: 36,
    },
    pressed: { opacity: 0.9 },
    identity: { flexDirection: 'row', alignItems: 'center', gap: 12 },
    avatar: { backgroundColor: C.avatarBg },
    nameRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'baseline', gap: 6, flexWrap: 'wrap' },
    name: { fontSize: 15, fontWeight: '600', color: C.text, flexShrink: 1 },
    age: { fontSize: 12, color: C.muted },
    city: { fontSize: 13, color: C.muted, marginTop: 3 },
    next: { marginTop: 16, paddingTop: 13, borderTopWidth: 1, borderTopColor: '#bcb6cb' },
    nextLabel: { fontSize: 12, color: C.muted },
    nextDate: { flexDirection: 'row', alignItems: 'center', gap: 7, marginTop: 7 },
    nextDateText: { fontSize: 14, fontWeight: '500', color: C.primary },
    nextState: { marginTop: 8 },
    none: { fontSize: 13, color: C.muted },
    chevron: { position: 'absolute', right: 12, top: '50%', marginTop: -10 },
})
