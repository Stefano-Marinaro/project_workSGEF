import { useCallback, useState } from 'react'
import { View, Text, StyleSheet, Linking } from 'react-native'
import { useLocalSearchParams, useRouter, useFocusEffect } from 'expo-router'
import { Picker } from '@react-native-picker/picker'

import api from '../../../config/httpClient.js'
import { findTripOwner } from '../../../config/caregiverApi'
import {
    C,
    tripState,
    tripTypeShort,
    directionLabel,
    formatFullDate,
    formatTime,
    addressLines,
    isTerminal,
    isCancellable,
    apiErrorMessage,
} from '../../../constants/caregiver'
import {
    Page,
    Scroll,
    Loading,
    PageTitle,
    Subtitle,
    BackLink,
    Card,
    Icon,
    StatusLine,
    RouteStops,
    Btn,
    BottomSheet,
    FieldLabel,
    Input,
    ErrorText,
    EmptyState,
} from '../../../components/caregiver/ui'

const CANCEL_REASONS = [
    { value: 'non_necessario', label: 'Non è più necessario' },
    { value: 'cambio_programma', label: 'Cambio di programma dell’assistito' },
    { value: 'altro_mezzo', label: 'Trovato un altro mezzo di trasporto' },
    { value: 'altro', label: 'Altro' },
]

// Pannello con titolo e icona (rqPanel del mockup)
const Panel = ({ title, icon, children }) => (
    <Card style={styles.panel}>
        <View style={styles.panelHead}>
            {icon ? <Icon name={icon} size={19} color={C.primary} /> : null}
            <Text style={styles.panelTitle}>{title}</Text>
        </View>
        {children}
    </Card>
)

const Timeline = ({ status, departure }) => {
    const phase = status === 'Pending' ? 1 : status === 'Confirmed' ? 2 : status === 'Completed' ? 4 : 3
    const steps = [
        ['Richiesta inviata', 'Ricevuta dalle associazioni.'],
        ['Approvazione dell’associazione', phase === 1 ? 'In attesa di una disponibilità.' : 'Disponibilità confermata.'],
        ['Accompagnamento in corso', phase === 2 ? `Previsto per il ${formatFullDate(departure)}.` : ''],
        ['Trasporto concluso', ''],
    ]
    return (
        <Card style={styles.panel}>
            <Text style={styles.timelineTitle}>A che punto siamo</Text>
            {steps.map(([title, help], i) => {
                const done = i === 0 || phase === 4 || (phase >= 2 && i === 1)
                const current = (phase === 1 && i === 1) || (phase === 3 && i === 2)
                const color = done ? C.primary : current ? C.waiting : '#827b91'
                return (
                    <View key={title} style={styles.step}>
                        <View style={styles.stepRail}>
                            <View
                                style={[
                                    styles.stepSymbol,
                                    done && { backgroundColor: C.primary },
                                    current && { backgroundColor: C.waitingSoft },
                                ]}
                            >
                                {done ? (
                                    <Icon name="checkmark" size={14} color={C.white} />
                                ) : (
                                    <Text style={[styles.stepNumber, { color }]}>{i + 1}</Text>
                                )}
                            </View>
                            {i < steps.length - 1 ? <View style={styles.stepLine} /> : null}
                        </View>
                        <View style={styles.stepBody}>
                            <Text style={[styles.stepTitle, { color }]}>{title}</Text>
                            {help ? <Text style={styles.stepHelp}>{help}</Text> : null}
                        </View>
                    </View>
                )
            })}
        </Card>
    )
}

const TransportDetail = () => {
    const params = useLocalSearchParams()
    const router = useRouter()
    const { id } = params

    const [loading, setLoading] = useState(true)
    const [error, setError] = useState('')
    const [trip, setTrip] = useState(null)
    const [timeline, setTimeline] = useState([])
    const [profile, setProfile] = useState(null)
    const [owner, setOwner] = useState({ name: params.assistedName, associationName: params.associationName })

    const [sheetOpen, setSheetOpen] = useState(false)
    const [reason, setReason] = useState('')
    const [note, setNote] = useState('')
    const [cancelling, setCancelling] = useState(false)
    const [cancelError, setCancelError] = useState('')

    const load = useCallback(async () => {
        setLoading(true)
        try {
            const [detail, steps, me] = await Promise.all([
                api.get(`/transports/${id}`),
                api.get(`/transports/${id}/status-timeline`).catch(() => ({ data: [] })),
                api.get('/me/profile').catch(() => ({ data: null })),
            ])
            setTrip(detail.data)
            setTimeline(steps.data)
            setProfile(me.data)
            if (!params.assistedName) {
                const found = await findTripOwner(id)
                if (found.person)
                    setOwner({ name: `${found.person.name} ${found.person.surname}`, associationName: found.associationName })
            }
            setError('')
        } catch (e) {
            setError(apiErrorMessage(e, 'Impossibile caricare il trasporto.'))
        } finally {
            setLoading(false)
        }
    }, [id, params.assistedName])

    useFocusEffect(
        useCallback(() => {
            load()
        }, [load]),
    )

    if (loading) return <Loading />

    if (error || !trip) {
        return (
            <Page>
                <Scroll>
                    <EmptyState title="Trasporto non disponibile" text={error} action="Torna ai trasporti" onAction={() => router.replace('/transport')} />
                </Scroll>
            </Page>
        )
    }

    const closed = isTerminal(trip.requestStatus)
    const lastTransition = timeline.length ? timeline[timeline.length - 1].status : null
    const state = tripState(trip.requestStatus, lastTransition)
    const start = addressLines(trip.startAddress)
    const end = addressLines(trip.endAddress)
    const returnEnd = addressLines(trip.returnEndAddress ?? trip.startAddress)
    const associationName = trip.associationName || owner.associationName
    const referentName = [profile?.name, profile?.surname].filter(Boolean).join(' ')
    const borderColor =
        state.kind === 'waiting' ? C.waitingBorder : state.kind === 'confirmed' ? C.success : state.kind === 'cancelled' ? C.cancelled : C.primary

    const associationTitle =
        associationName ||
        (trip.requestStatus === 'Pending'
            ? 'In attesa di una disponibilità'
            : trip.requestStatus === 'Cancelled'
              ? 'Richiesta annullata'
              : 'Associazione non indicata')
    const associationText =
        trip.requestStatus === 'Pending'
            ? 'Qui compariranno i dati dell’associazione che accetterà la richiesta.'
            : trip.associationPhones?.length
              ? trip.associationPhones.join(' · ')
              : state.text

    const confirmCancel = async () => {
        if (!reason) return
        setCancelling(true)
        setCancelError('')
        try {
            // Il backend non registra motivo e note: l'annullamento è un POST senza corpo
            await api.post(`/transports/${id}/cancel`)
            setSheetOpen(false)
            router.replace('/transport')
        } catch (e) {
            setCancelError(apiErrorMessage(e, 'Annullamento non riuscito.'))
        } finally {
            setCancelling(false)
        }
    }

    return (
        <Page>
            <Scroll>
                <PageTitle style={styles.title}>Dettaglio della richiesta</PageTitle>
                {owner.name ? <Subtitle>Accompagnamento per {owner.name}</Subtitle> : null}
                <BackLink
                    label={closed ? 'Torna allo storico' : 'Torna ai trasporti'}
                    onPress={() => router.replace(closed ? '/transport/history' : '/transport')}
                />

                <View style={[styles.statePanel, { borderLeftColor: borderColor }]}>
                    <View style={styles.flex}>
                        <StatusLine state={state} color={state.kind === 'active' ? C.primary : undefined} />
                        <Text style={styles.stateText}>{state.text}</Text>
                    </View>
                    <Icon name={trip.requestStatus === 'Confirmed' ? 'checkmark-circle-outline' : 'information-circle-outline'} size={25} color={C.primary} />
                </View>

                {state.kind !== 'cancelled' ? <Timeline status={trip.requestStatus} departure={trip.departureDateHour} /> : null}

                {owner.name ? (
                    <Panel title="Chi viene accompagnato" icon="person-outline">
                        <Text style={styles.h3}>{owner.name}</Text>
                        <Text style={styles.p}>Persona per cui è stato richiesto il servizio.</Text>
                    </Panel>
                ) : null}

                <Panel title="Quando è richiesto il trasporto" icon="calendar-outline">
                    <View style={styles.dateLine}>
                        <Icon name="calendar-outline" size={20} color={C.primary} />
                        <Text style={styles.dateText}>{formatFullDate(trip.departureDateHour)}</Text>
                    </View>
                    <Text style={styles.p}>
                        {tripTypeShort(trip.tripType)} · {directionLabel(trip.tripDirection)}
                    </Text>
                </Panel>

                <Panel title="Andata" icon="location-outline">
                    <RouteStops
                        labelColor={C.muted}
                        stops={[
                            { label: 'Partenza', time: formatTime(trip.departureDateHour), ...start },
                            { label: 'Dove accompagnare la persona', ...end },
                        ]}
                    />
                </Panel>

                {trip.tripDirection === 'RoundTrip' ? (
                    <Panel title="Ritorno" icon="location-outline">
                        <RouteStops
                            labelColor={C.muted}
                            stops={[
                                { label: 'Partenza per il ritorno', ...end },
                                { label: 'Dove accompagnare la persona al ritorno', ...returnEnd },
                            ]}
                        />
                    </Panel>
                ) : null}

                <Panel title="Associazione" icon="information-circle-outline">
                    <Text style={styles.h3}>{associationTitle}</Text>
                    <Text style={styles.p}>{associationText}</Text>
                </Panel>

                <Panel title="Chi verrà contattato">
                    <Text style={styles.h3}>{referentName || 'Referente non indicato'}</Text>
                    {profile?.phone ? (
                        <Text
                            style={styles.phone}
                            onPress={() => Linking.openURL(`tel:${profile.phone.replace(/\s/g, '')}`)}
                        >
                            <Icon name="call-outline" size={18} color={C.primary} />  {profile.phone}
                        </Text>
                    ) : (
                        <Text style={styles.p}>Recapito non indicato</Text>
                    )}
                </Panel>

                {isCancellable(trip.requestStatus) ? (
                    <Btn label="Annulla trasporto" variant="danger" icon="close-circle-outline" onPress={() => setSheetOpen(true)} />
                ) : null}
            </Scroll>

            <BottomSheet visible={sheetOpen} onClose={() => setSheetOpen(false)} title="Annulla trasporto">
                <Text style={styles.sheetText}>
                    Sei sicuro di voler eliminare questo trasporto? Una volta eliminato si dovrà effettuare una nuova
                    richiesta per richiedere un nuovo trasporto.
                </Text>
                <FieldLabel label="Motivo" />
                <View style={styles.select}>
                    <Picker selectedValue={reason} onValueChange={setReason} style={styles.picker} dropdownIconColor={C.text}>
                        <Picker.Item label="Seleziona un motivo" value="" />
                        {CANCEL_REASONS.map((r) => (
                            <Picker.Item key={r.value} label={r.label} value={r.value} />
                        ))}
                    </Picker>
                </View>
                <FieldLabel label="Note aggiuntive (opzionale)" style={styles.noteLabel} />
                <Input multiline value={note} onChangeText={setNote} placeholder="Aggiungi dettagli se vuoi" style={styles.note} />
                <ErrorText>{cancelError}</ErrorText>
                <View style={styles.sheetActions}>
                    <Btn label="Annulla" variant="outline" onPress={() => setSheetOpen(false)} style={styles.sheetBtnGrey} textStyle={styles.sheetBtnGreyText} />
                    <Btn
                        label="Conferma annullamento"
                        variant="danger"
                        onPress={confirmCancel}
                        disabled={!reason}
                        loading={cancelling}
                        style={styles.flex}
                    />
                </View>
            </BottomSheet>
        </Page>
    )
}

export default TransportDetail

const styles = StyleSheet.create({
    flex: { flex: 1 },
    title: { marginBottom: 6 },
    statePanel: {
        flexDirection: 'row',
        alignItems: 'flex-start',
        gap: 16,
        backgroundColor: C.card,
        borderWidth: 1,
        borderColor: '#bcb6cc',
        borderLeftWidth: 4,
        borderRadius: 14,
        padding: 18,
        marginBottom: 24,
    },
    stateText: { fontSize: 14, lineHeight: 24, color: C.textStrong, marginTop: 10 },
    panel: { padding: 20, marginBottom: 24 },
    panelHead: { flexDirection: 'row', alignItems: 'center', gap: 10, marginBottom: 17 },
    panelTitle: { fontSize: 15, fontWeight: '600', color: C.textStrong },
    h3: { fontSize: 19, lineHeight: 27, fontWeight: '600', color: C.text },
    p: { fontSize: 15, lineHeight: 25, color: C.muted, marginTop: 8 },
    dateLine: { flexDirection: 'row', alignItems: 'center', gap: 12 },
    dateText: { fontSize: 20, fontWeight: '600', color: C.primary },
    phone: { fontSize: 17, color: C.primary, paddingVertical: 10 },
    timelineTitle: { fontSize: 15, fontWeight: '600', color: C.text, marginBottom: 20 },
    step: { flexDirection: 'row', gap: 12 },
    stepRail: { alignItems: 'center', width: 25 },
    stepSymbol: { width: 25, height: 25, borderRadius: 13, backgroundColor: C.stepIdle, alignItems: 'center', justifyContent: 'center' },
    stepNumber: { fontSize: 12, fontWeight: '600' },
    stepLine: { flex: 1, width: 1, backgroundColor: '#bab3ca', minHeight: 18 },
    stepBody: { flex: 1, paddingBottom: 20, minHeight: 45 },
    stepTitle: { fontSize: 13, lineHeight: 22, fontWeight: '500' },
    stepHelp: { fontSize: 12, lineHeight: 20, color: C.muted, marginTop: 2 },
    sheetText: { fontSize: 14, lineHeight: 21, color: C.muted, marginBottom: 14 },
    select: { backgroundColor: C.card, borderRadius: 10, overflow: 'hidden', marginBottom: 14 },
    picker: { color: C.text, backgroundColor: C.card, borderWidth: 0, minHeight: 48, paddingHorizontal: 10, fontSize: 15 },
    noteLabel: { marginTop: 2 },
    note: { minHeight: 80 },
    sheetActions: { flexDirection: 'row', gap: 10, marginTop: 16 },
    sheetBtnGrey: { flex: 1, borderColor: '#9a97a8' },
    sheetBtnGreyText: { color: C.muted },
})
