import { useCallback, useState } from 'react'
import { View, Text, Pressable, StyleSheet } from 'react-native'
import { useLocalSearchParams, useRouter, useFocusEffect } from 'expo-router'

import api from '../../../config/httpClient.js'
import {
    C,
    ageFrom,
    shortStatus,
    tripTypeShort,
    formatNumericDate,
    formatTime,
    apiErrorMessage,
} from '../../../constants/caregiver'
import AssistedForm, { assistedToForm, validateAssisted } from '../../../components/caregiver/AssistedForm'
import InviteCaregiverModal from '../../../components/caregiver/InviteCaregiverModal'
import {
    Page,
    Scroll,
    Loading,
    PageTitle,
    Card,
    Icon,
    Initials,
    CircleButton,
    SectionLabel,
    Btn,
    Footer,
    ErrorText,
    ConfirmDialog,
    EmptyState,
} from '../../../components/caregiver/ui'

const InfoCard = ({ icon, label, children }) => (
    <Card style={styles.infoCard}>
        <Icon name={icon} size={20} color={C.muted} />
        <View style={styles.flex}>
            <Text style={styles.infoLabel}>{label}</Text>
            {children}
        </View>
    </Card>
)

const AssistedDetail = () => {
    const { id } = useLocalSearchParams()
    const router = useRouter()

    const [loading, setLoading] = useState(true)
    const [error, setError] = useState('')
    const [person, setPerson] = useState(null)
    const [myEmail, setMyEmail] = useState('')

    const [editing, setEditing] = useState(false)
    const [form, setForm] = useState(null)
    const [formErrors, setFormErrors] = useState({})
    const [saving, setSaving] = useState(false)
    const [saveError, setSaveError] = useState('')

    const [inviteOpen, setInviteOpen] = useState(false)
    // { kind: 'caregiver' | 'self', caregiver }
    const [confirm, setConfirm] = useState(null)
    const [confirmLoading, setConfirmLoading] = useState(false)
    const [confirmError, setConfirmError] = useState('')

    const load = useCallback(async () => {
        try {
            const [detail, me] = await Promise.all([api.get(`/assisted/${id}`), api.get('/me/profile')])
            setPerson(detail.data)
            setMyEmail((me.data.email ?? '').toLowerCase())
            setError('')
        } catch (e) {
            setError(apiErrorMessage(e, 'Impossibile caricare l’assistito.'))
        } finally {
            setLoading(false)
        }
    }, [id])

    useFocusEffect(
        useCallback(() => {
            setEditing(false)
            load()
        }, [load]),
    )

    if (loading) return <Loading />

    if (error || !person) {
        return (
            <Page>
                <Scroll>
                    <EmptyState title="Assistito non disponibile" text={error} action="Tutti gli assistiti" onAction={() => router.replace('/assisted')} />
                </Scroll>
            </Page>
        )
    }

    const fullName = `${person.name} ${person.surname}`
    const me = person.caregivers.find((c) => c.email.toLowerCase() === myEmail)

    const startEdit = () => {
        setForm(assistedToForm(person))
        setFormErrors({})
        setSaveError('')
        setEditing(true)
    }

    const save = async () => {
        const { errors, payload } = validateAssisted(form)
        setFormErrors(errors)
        if (Object.keys(errors).length) return
        setSaving(true)
        setSaveError('')
        try {
            await api.put(`/assisted/${id}`, payload)
            setEditing(false)
            await load()
        } catch (e) {
            setSaveError(apiErrorMessage(e, 'Salvataggio non riuscito.'))
        } finally {
            setSaving(false)
        }
    }

    // Eliminare dalla propria lista è distinto dal rimuovere un altro caregiver.
    const runConfirm = async () => {
        const target = confirm.kind === 'self' ? me : confirm.caregiver
        if (confirm.kind !== 'self' && !target) {
            setConfirmError('Impossibile identificare il caregiver.')
            return
        }
        setConfirmLoading(true)
        setConfirmError('')
        try {
            if (confirm.kind === 'self') await api.delete(`/assisted/${id}`)
            else await api.delete(`/assisted/${id}/caregivers/${target.id}`)
            setConfirm(null)
            if (confirm.kind === 'self') router.replace('/assisted')
            else await load()
        } catch (e) {
            setConfirmError(apiErrorMessage(e, 'Operazione non riuscita.'))
        } finally {
            setConfirmLoading(false)
        }
    }

    if (editing) {
        return (
            <Page>
                <Scroll>
                    <PageTitle>Modifica assistito</PageTitle>
                    <AssistedForm form={form} setForm={setForm} errors={formErrors} variant="edit" />
                    <ErrorText>{saveError}</ErrorText>
                </Scroll>
                <Footer>
                    <Btn label="Annulla" variant="secondary" onPress={() => setEditing(false)} />
                    <Btn label="Salva modifiche" onPress={save} loading={saving} style={styles.save} />
                </Footer>
            </Page>
        )
    }

    const a = person.homeAddress

    return (
        <Page>
            <Scroll>
                <View style={styles.titleRow}>
                    <PageTitle style={styles.title}>Assistito</PageTitle>
                    <Pressable onPress={() => router.push('/assisted')} hitSlop={8}>
                        <Text style={styles.allLink}>Tutti gli assistiti</Text>
                    </Pressable>
                </View>

                <View style={styles.identity}>
                    <Initials name={person.name} surname={person.surname} size={52} solid />
                    <View style={styles.flex}>
                        <Text style={styles.name}>{fullName}</Text>
                        <Text style={styles.age}>{ageFrom(person.birthDate)}</Text>
                    </View>
                    <CircleButton icon="pencil" label="Modifica assistito" onPress={startEdit} />
                </View>

                <InfoCard icon="call-outline" label="Telefono">
                    <Text style={styles.infoValue}>{person.phone}</Text>
                </InfoCard>
                <InfoCard icon="home-outline" label="Residenza">
                    {a ? (
                        <>
                            <Text style={styles.infoValue}>
                                {a.street} {a.number}
                            </Text>
                            <Text style={styles.infoValue}>
                                {a.city} ({a.province}) {a.postalCode}
                            </Text>
                        </>
                    ) : (
                        <Text style={styles.infoValue}>Non indicata</Text>
                    )}
                </InfoCard>

                <SectionLabel style={styles.section}>Note</SectionLabel>
                <Card style={styles.noteCard}>
                    <Text style={styles.noteText}>{person.note || 'Non sono state aggiunte note.'}</Text>
                </Card>

                <View style={styles.caregiversHead}>
                    <SectionLabel style={styles.caregiversLabel}>Caregiver collegati</SectionLabel>
                    <Btn
                        label="Aggiungi caregiver"
                        icon="person-add-outline"
                        onPress={() => setInviteOpen(true)}
                        style={styles.addCaregiver}
                        textStyle={styles.addCaregiverText}
                    />
                </View>
                {person.caregivers.map((c) => {
                    const isMe = me && c.id === me.id
                    const name = [c.name, c.surname].filter(Boolean).join(' ') || c.email
                    return (
                        <Card key={c.id} style={styles.caregiver}>
                            <View style={styles.flex}>
                                <Text style={styles.caregiverName}>{name}</Text>
                                {isMe ? <Text style={styles.you}>Tu</Text> : null}
                                <Text style={styles.caregiverSub}>{c.email}</Text>
                            </View>
                            {!isMe ? (
                                <Pressable
                                    onPress={() => {
                                        setConfirmError('')
                                        setConfirm({ kind: 'caregiver', caregiver: c })
                                    }}
                                    accessibilityLabel={`Rimuovi ${name}`}
                                    hitSlop={8}
                                >
                                    <Icon name="trash-outline" size={20} color={C.danger} />
                                </Pressable>
                            ) : null}
                        </Card>
                    )
                })}

                <SectionLabel style={styles.section}>Richieste e trasporti per questa persona</SectionLabel>
                {person.trips.length === 0 ? (
                    <Text style={styles.noTrips}>Nessun trasporto registrato per questo assistito.</Text>
                ) : (
                    person.trips.map((t) => (
                        <Card
                            key={t.id}
                            style={styles.trip}
                            onPress={() =>
                                router.push({
                                    pathname: '/transport/[id]',
                                    params: { id: t.id, assistedId: person.id, assistedName: fullName },
                                })
                            }
                        >
                            <View style={styles.flex}>
                                <View style={styles.tripTop}>
                                    <Text style={styles.tripType}>{tripTypeShort(t.tripType)}</Text>
                                    <Text style={styles.tripStatus}>{shortStatus(t.status)}</Text>
                                </View>
                                <Text style={styles.tripDate}>
                                    {formatNumericDate(t.departureDateHour)} · {formatTime(t.departureDateHour)}
                                </Text>
                            </View>
                            <Icon name="chevron-forward" size={18} color={C.muted} />
                        </Card>
                    ))
                )}

                <Btn
                    label="Elimina assistito"
                    variant="danger"
                    icon="close-circle-outline"
                    onPress={() => {
                        setConfirmError('')
                        setConfirm({ kind: 'self' })
                    }}
                    style={styles.delete}
                />
            </Scroll>

            <InviteCaregiverModal visible={inviteOpen} onClose={() => setInviteOpen(false)} person={person} />

            <ConfirmDialog
                visible={!!confirm}
                title={confirm?.kind === 'self' ? 'Elimina assistito' : 'Rimuovi caregiver'}
                message={
                    confirm?.kind === 'self'
                        ? `Vuoi eliminare ${fullName} dalla tua lista? Se sei l’unico caregiver, l’assistito verrà archiviato conservando lo storico. Prima dovrai annullare eventuali trasporti ancora aperti.`
                        : `Vuoi rimuovere ${[confirm?.caregiver?.name, confirm?.caregiver?.surname].filter(Boolean).join(' ') || confirm?.caregiver?.email} dai caregiver collegati?`
                }
                onCancel={() => setConfirm(null)}
                onConfirm={runConfirm}
                loading={confirmLoading}
                error={confirmError}
            />
        </Page>
    )
}

export default AssistedDetail

const styles = StyleSheet.create({
    flex: { flex: 1 },
    titleRow: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between' },
    title: { marginBottom: 16 },
    allLink: { fontSize: 13, color: C.muted },
    identity: { flexDirection: 'row', alignItems: 'center', gap: 14, marginBottom: 20 },
    name: { fontSize: 19, fontWeight: '600', color: C.text },
    age: { fontSize: 13, color: C.muted, marginTop: 2 },
    infoCard: { flexDirection: 'row', gap: 14, alignItems: 'flex-start', marginBottom: 12, paddingVertical: 16 },
    infoLabel: { fontSize: 12, color: C.muted, marginBottom: 4 },
    infoValue: { fontSize: 16, lineHeight: 24, color: C.text },
    section: { marginTop: 14 },
    noteCard: { paddingVertical: 15, marginBottom: 10 },
    noteText: { fontSize: 15, color: C.text },
    caregiversHead: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', marginTop: 14, marginBottom: 8 },
    caregiversLabel: { marginBottom: 0 },
    addCaregiver: { minHeight: 40, paddingHorizontal: 12, borderRadius: 10 },
    addCaregiverText: { fontSize: 14 },
    caregiver: { flexDirection: 'row', alignItems: 'center', gap: 10, paddingVertical: 14, marginBottom: 8 },
    caregiverName: { fontSize: 15, fontWeight: '600', color: C.text },
    you: { fontSize: 12, fontWeight: '600', color: C.primary, marginTop: 6 },
    caregiverSub: { fontSize: 13, color: C.muted, marginTop: 6 },
    noTrips: { fontSize: 14, color: C.muted, marginBottom: 8 },
    trip: { flexDirection: 'row', alignItems: 'center', gap: 10, paddingVertical: 14, marginBottom: 10 },
    tripTop: { flexDirection: 'row', justifyContent: 'space-between' },
    tripType: { fontSize: 15, fontWeight: '600', color: C.text },
    tripStatus: { fontSize: 12, color: C.muted },
    tripDate: { fontSize: 13, color: C.muted, marginTop: 6 },
    delete: { marginTop: 16 },
    save: { flex: 1, maxWidth: 200 },
})
