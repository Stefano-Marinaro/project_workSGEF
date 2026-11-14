import { useCallback, useRef, useState } from 'react'
import { View, Text, Pressable, StyleSheet } from 'react-native'
import { useFocusEffect, useLocalSearchParams, useRouter } from 'expo-router'

import api from '../../../config/httpClient.js'
import { donationEstimate, donationDigitCount, MAX_DONATION_DIGITS } from '../../../utils/donation'
import Banner from '../../../components/Banner'
import { geocodeAddress } from '../../../config/caregiverApi'
import {
    C,
    TRIP_TYPES,
    ONE_WAY_ONLY,
    tripTypeLabel,
    directionLabel,
    ageFrom,
    formatFullDate,
    addressLines,
    toIsoDay,
    apiErrorMessage,
} from '../../../constants/caregiver'
import DateTimeField from '../../../components/caregiver/DateTimeField'
import AddressModal from '../../../components/caregiver/AddressModal'
import InviteCaregiverModal from '../../../components/caregiver/InviteCaregiverModal'
import {
    Page,
    Scroll,
    Loading,
    PageTitle,
    Card,
    Icon,
    Initials,
    Radio,
    OptionRow,
    FieldLabel,
    Input,
    Row,
    Divider,
    Btn,
    Footer,
    ErrorText,
    EmptyState,
    SuccessCheck,
    RouteStops,
} from '../../../components/caregiver/ui'

const STEPS = 5

const MOBILITY = [
    { value: 'walking', label: 'Cammina senza ausili' },
    { value: 'aid', label: 'Usa bastone o deambulatore' },
    { value: 'wheelchair', label: 'Usa una sedia a rotelle' },
]
const HELP = [
    { value: 'boarding', label: 'Aiuto per salire e scendere dal mezzo' },
    { value: 'walking_help', label: 'Aiuto nel tragitto a piedi fino al mezzo' },
]

// Tariffa e minimo come in appsettings.json (sezione Pricing) del backend
const PRICE_PER_KM = 0.8
const MINIMUM_AMOUNT = 5

const initialState = (assistedId = '') => ({
    step: 1,
    published: false,
    tripType: 'Visit',
    direction: 'RoundTrip',
    date: '',
    time: '',
    added: [],
    coords: {},
    fromKey: '',
    toKey: '',
    returnKey: '',
    sameReturn: true,
    personId: assistedId,
    mobility: '',
    help: [],
    donate: false,
    extra: '',
})

// Distanza in linea d'aria (km): serve solo per la stima del contributo
const distanceKm = (a, b) => {
    if (!a?.latitude || !b?.latitude) return 0
    const rad = (d) => (d * Math.PI) / 180
    const dLat = rad(b.latitude - a.latitude)
    const dLon = rad(b.longitude - a.longitude)
    const h = Math.sin(dLat / 2) ** 2 + Math.cos(rad(a.latitude)) * Math.cos(rad(b.latitude)) * Math.sin(dLon / 2) ** 2
    return 6371 * 2 * Math.atan2(Math.sqrt(h), Math.sqrt(1 - h))
}

const euros = (value) => value.toLocaleString('it-IT', { style: 'currency', currency: 'EUR' })

// ---------- Pezzi di interfaccia del wizard ----------

const Progress = ({ step }) => (
    <View style={styles.progress}>
        <Text style={styles.progressText}>
            Passaggio {step} di {STEPS}
        </Text>
        <View style={styles.bars}>
            {Array.from({ length: STEPS }, (_, i) => (
                <View key={i} style={[styles.bar, i < step && styles.barOn]} />
            ))}
        </View>
    </View>
)

const Required = () => (
    <Text style={styles.requiredNote}>
        I campi contrassegnati con <Text style={styles.star}>*</Text> sono obbligatori.
    </Text>
)

const StepTitle = ({ title, required, subtitle }) => (
    <View style={styles.stepHead}>
        <Text style={styles.stepTitle}>
            {title}
            {required ? <Text style={styles.star}> *</Text> : null}
        </Text>
        {subtitle ? <Text style={styles.stepSubtitle}>{subtitle}</Text> : null}
    </View>
)

const AddressOption = ({ option, selected, onPress }) => {
    const lines = addressLines(option)
    return (
        <Pressable onPress={onPress} style={[styles.addressOption, selected && styles.addressOptionOn]}>
            <View style={styles.addressRadio}>
                <Radio checked={selected} />
            </View>
            <View style={styles.flex}>
                {option.label ? <Text style={styles.addressTag}>{option.label}</Text> : null}
                <Text style={styles.addressLine1}>{lines.line1}</Text>
                <Text style={styles.addressLine2}>{lines.line2}</Text>
            </View>
        </Pressable>
    )
}

// Blocco "Partenza" / "Destinazione" / "Destinazione del ritorno" dello step 2
const AddressPicker = ({ icon, title, emptyHint, options, value, onSelect, onAdd, error }) => (
    <View>
        <View style={styles.pickerHead}>
            <Icon name={icon} size={19} color={C.primary} />
            <Text style={styles.pickerTitle}>
                {title}
                <Text style={styles.star}> *</Text>
            </Text>
        </View>
        <Text style={[styles.pickerHint, !options.length && styles.pickerHintIndented]}>
            {options.length ? 'Scegli tra gli indirizzi già usati' : emptyHint}
        </Text>
        {options.map((o) => (
            <AddressOption key={o.key} option={o} selected={value === o.key} onPress={() => onSelect(o.key)} />
        ))}
        <Btn
            label={options.length ? 'Inserisci un altro indirizzo' : 'Inserisci indirizzo'}
            icon="add"
            onPress={onAdd}
            style={styles.addAddress}
            textStyle={styles.addAddressText}
        />
        <ErrorText>{error}</ErrorText>
    </View>
)

// ---------- Schermata ----------

const Create = () => {
    const { assistedId, saved } = useLocalSearchParams()
    const router = useRouter()
    const scrollRef = useRef(null)
    const consumedAssisted = useRef(null)

    const [loading, setLoading] = useState(true)
    const [loadError, setLoadError] = useState('')
    const [profile, setProfile] = useState(null)
    const [people, setPeople] = useState([])
    const [s, setS] = useState(() => initialState(assistedId))
    const [errors, setErrors] = useState({})
    const [busy, setBusy] = useState(false)
    const [submitError, setSubmitError] = useState('')
    const [addressTarget, setAddressTarget] = useState(null)
    const [inviteOpen, setInviteOpen] = useState(false)

    const update = (patch) => {
        setS((prev) => ({ ...prev, ...patch }))
        setErrors({})
    }

    useFocusEffect(
        useCallback(() => {
            let active = true
            // Dopo un invio, o arrivando con un nuovo assistito già scelto, si riparte da capo
            const freshAssisted = assistedId && consumedAssisted.current !== assistedId
            if (freshAssisted) consumedAssisted.current = assistedId
            setS((prev) => prev.published ? initialState(assistedId ?? '')
                : freshAssisted ? { ...prev, personId: assistedId, mobility: '', help: [] } : prev)
            setErrors({})
            setSubmitError('')
            const load = async () => {
                try {
                    const [me, list] = await Promise.all([api.get('/me/profile'), api.get('/assisted')])
                    if (!active) return
                    setProfile(me.data)
                    setPeople(list.data)
                    setLoadError('')
                } catch (e) {
                    if (active) setLoadError(apiErrorMessage(e, 'Impossibile preparare la richiesta.'))
                } finally {
                    if (active) setLoading(false)
                }
            }
            load()
            return () => { active = false }
        }, [assistedId]),
    )

    if (loading) return <Loading />

    if (loadError) {
        return (
            <Page>
                <Scroll>
                    <EmptyState title="Nuovo trasporto" text={loadError} />
                </Scroll>
            </Page>
        )
    }

    if (people.length === 0) {
        return (
            <Page>
                <Scroll>
                    <EmptyState
                        title="Aggiungi il tuo primo assistito"
                        text="Registra la persona da accompagnare per iniziare a usare GoCare."
                        action="Registra assistito"
                        onAction={() => router.push({ pathname: '/assisted/new', params: { from: 'wizard' } })}
                    />
                </Scroll>
            </Page>
        )
    }

    // ---- dati derivati ----
    const person = people.find((p) => p.id === s.personId) ?? null
    const orderedPeople = person ? [person, ...people.filter((p) => p !== person)] : people
    const residences = orderedPeople
        .filter((p) => p.homeAddress)
        .map((p) => ({ key: `res-${p.id}`, label: `Residenza di ${p.name} ${p.surname}`, ...p.homeAddress }))
    const allOptions = [...residences, ...s.added]
    // Le coordinate delle residenze vengono calcolate allo step 2 e salvate a parte
    const find = (key) => {
        const option = allOptions.find((o) => o.key === key)
        return option ? { ...option, ...s.coords[key] } : undefined
    }
    const roundTrip = s.direction === 'RoundTrip'
    const from = find(s.fromKey)
    const to = find(s.toKey)
    const returnTo = roundTrip ? (s.sameReturn ? from : find(s.returnKey)) : null
    const me = { name: profile.name, surname: profile.surname, phone: profile.phone }
    const profileComplete = !!(profile.name && profile.surname && profile.phone && profile.birthDate)

    const km = distanceKm(from, to) + (roundTrip ? distanceKm(to, returnTo) : 0)
    const roundedKm = Math.round(km * 10) / 10
    const minimum = Math.max(roundedKm * PRICE_PER_KM, MINIMUM_AMOUNT)
    const donation = donationEstimate(s.extra, s.donate, minimum)

    const goTo = (step) => {
        update({ step })
        setErrors({})
        scrollRef.current?.scrollTo?.({ y: 0, animated: false })
    }

    // ---- validazione per step ----
    const validateStep2 = async () => {
        const found = {}
        if (!s.date) found.date = 'Scegli la data.'
        if (!s.time) found.time = 'Scegli l’ora di partenza.'
        if (s.date && s.time && new Date(`${s.date}T${s.time}`) <= new Date()) found.time = 'La partenza deve essere nel futuro.'
        if (!from) found.from = 'Scegli l’indirizzo di partenza.'
        if (!to) found.to = 'Scegli la destinazione.'
        if (roundTrip && !s.sameReturn && !returnTo) found.returnTo = 'Scegli dove accompagnare la persona al ritorno.'
        if (Object.keys(found).length) return found

        // Le residenze non hanno coordinate: si geocodificano qui, prima di proseguire
        const missing = [from, to, returnTo].filter((o) => o && o.latitude == null)
        for (const option of missing) {
            const located = await geocodeAddress(option)
            if (!located) {
                const field = option === from ? 'from' : option === to ? 'to' : 'returnTo'
                found[field] = 'Indirizzo non trovato: inseriscilo di nuovo controllando via, civico e comune.'
                continue
            }
            setS((prev) => ({
                ...prev,
                coords: { ...prev.coords, [option.key]: { latitude: located.latitude, longitude: located.longitude } },
            }))
        }
        return found
    }

    const next = async () => {
        if (s.step === 3 && !profileComplete) {
            setErrors({ profile: 'Completa i dati del referente prima di proseguire.' })
            return
        }
        if (s.step === 2) {
            setBusy(true)
            try {
                const found = await validateStep2()
                setErrors(found)
                if (Object.keys(found).length) return
            } catch (e) {
                setErrors({ from: apiErrorMessage(e, 'Impossibile verificare gli indirizzi.') })
                return
            } finally {
                setBusy(false)
            }
        }
        if (s.step === 4) {
            const found = {}
            if (!person) found.person = 'Scegli la persona da accompagnare.'
            else if (!s.mobility) found.mobility = 'Seleziona come si sposta la persona.'
            setErrors(found)
            if (Object.keys(found).length) return
        }
        goTo(s.step + 1)
    }

    const back = () => {
        if (s.step === 1) router.replace('/transport')
        else goTo(s.step - 1)
    }

    const tripAddress = (o) => ({
        street: o.street,
        number: o.number,
        postalCode: o.postalCode,
        city: o.city,
        province: o.province,
        latitude: o.latitude,
        longitude: o.longitude,
    })

    const submit = async () => {
        if (donation.error) {
            setErrors({ extra: donation.error })
            return
        }
        setBusy(true)
        setSubmitError('')
        try {
            // Mobilità, aiuti e donazione non hanno ancora un campo nel backend (NewTransportRequest)
            const sameAsStart = s.sameReturn || returnTo?.key === from.key
            await api.post('/transports', {
                beneficiaryId: person.id,
                tripType: s.tripType,
                tripDirection: s.direction,
                departureDateHour: new Date(`${s.date}T${s.time}`).toISOString(),
                returnDateHour: null,
                startAddress: tripAddress(from),
                endAddress: tripAddress(to),
                returnEndAddress: roundTrip && !sameAsStart ? tripAddress(returnTo) : null,
                referencePhone: profile.phone,
                referenceEmail: profile.email,
                companions: [],
            })
            update({ published: true })
        } catch (e) {
            setSubmitError(apiErrorMessage(e, 'Invio della richiesta non riuscito.'))
        } finally {
            setBusy(false)
        }
    }

    // ---- schermata di conferma ----
    if (s.published) {
        return (
            <Page style={styles.sent}>
                <SuccessCheck />
                <Text style={styles.sentTitle}>Richiesta inviata</Text>
                <Text style={styles.sentState}>In attesa di approvazione</Text>
                <Text style={styles.sentText}>
                    Un’associazione di volontariato deve ancora confermare la disponibilità. Ti avviseremo quando
                    l’accompagnamento sarà approvato.
                </Text>
                <Btn label="Visualizza le mie richieste" onPress={() => router.replace('/transport')} style={styles.sentBtn} />
            </Page>
        )
    }

    // ---- step ----
    const renderStep1 = () => (
        <>
            <Required />
            <StepTitle title="Di cosa hai bisogno?" required subtitle="Scegli il servizio di accompagnamento." />
            <View style={styles.services}>
                {TRIP_TYPES.map((t) => {
                    const selected = s.tripType === t.value
                    return (
                        <Pressable
                            key={t.value}
                            onPress={() =>
                                update({ tripType: t.value, ...(ONE_WAY_ONLY.includes(t.value) ? { direction: 'OneWay' } : {}) })
                            }
                            style={[styles.service, selected && styles.serviceOn]}
                        >
                            <View style={styles.serviceSymbol}>
                                <Icon name={`mc:${t.icon}`} size={20} color={C.primary} />
                            </View>
                            {selected ? <Icon name="checkmark-circle" size={20} color={C.primary} style={styles.serviceCheck} /> : null}
                            <Text style={styles.serviceTitle}>{t.label}</Text>
                            <Text style={styles.serviceText}>{t.description}</Text>
                        </Pressable>
                    )
                })}
            </View>
            <FieldLabel label="Di quale percorso hai bisogno?" required style={styles.directionLabel} />
            <Row>
                <OptionRow label="Solo andata" selected={s.direction === 'OneWay'} onPress={() => update({ direction: 'OneWay' })} style={styles.directionOption} />
                {!ONE_WAY_ONLY.includes(s.tripType) ? <OptionRow
                    label="Andata e ritorno"
                    selected={s.direction === 'RoundTrip'}
                    onPress={() => update({ direction: 'RoundTrip' })}
                    style={styles.directionOption}
                /> : null}
            </Row>
            {ONE_WAY_ONLY.includes(s.tripType) ? (
                <Text style={styles.hint}>Per {tripTypeLabel(s.tripType).toLowerCase()} è previsto solo il viaggio di andata.</Text>
            ) : null}
        </>
    )

    const renderStep2 = () => (
        <>
            <Required />
            <StepTitle title="Quando e dove?" subtitle="Indica quando partire e il percorso." />
            <Row>
                <View style={styles.flex}>
                    <FieldLabel label="Data del trasporto" required />
                    <DateTimeField mode="date" value={s.date} onChange={(date) => update({ date })} placeholder="gg/mm/aaaa" minimumDate={new Date()} />
                    <ErrorText>{errors.date}</ErrorText>
                </View>
                <View style={styles.flex}>
                    <FieldLabel label="Ora di partenza" required />
                    <DateTimeField mode="time" value={s.time} onChange={(time) => update({ time })} placeholder="--:--" />
                    <ErrorText>{errors.time}</ErrorText>
                </View>
            </Row>

            <Card style={styles.routePanel}>
                <AddressPicker
                    icon="radio-button-on-outline"
                    title="Partenza"
                    emptyHint="Da dove parte la persona?"
                    options={allOptions}
                    value={s.fromKey}
                    onSelect={(fromKey) => update({ fromKey })}
                    onAdd={() => setAddressTarget('fromKey')}
                    error={errors.from}
                />
                <Divider style={styles.panelDivider} />
                <AddressPicker
                    icon="location-outline"
                    title="Destinazione"
                    emptyHint="Dove deve essere accompagnata?"
                    options={s.added}
                    value={s.toKey}
                    onSelect={(toKey) => update({ toKey })}
                    onAdd={() => setAddressTarget('toKey')}
                    error={errors.to}
                />
                {roundTrip ? (
                    <>
                        <Divider style={styles.panelDivider} />
                        <Text style={styles.pickerTitle}>Ritorno al termine della visita</Text>
                        <Text style={styles.hint}>Non serve indicare un orario: la persona sarà riaccompagnata appena finita la visita.</Text>
                        <OptionRow
                            selected={s.sameReturn}
                            onPress={() => update({ sameReturn: true })}
                            label="Ritorno all’indirizzo di partenza"
                            style={styles.sameReturn}
                        />
                        <OptionRow selected={!s.sameReturn} onPress={() => update({ sameReturn: false })}
                            label="Ritorno a un altro indirizzo" style={styles.sameReturn} />
                        {!s.sameReturn ? (
                            <AddressPicker
                                icon="location-outline"
                                title="Destinazione del ritorno"
                                emptyHint="Dove accompagnare la persona al ritorno?"
                                options={allOptions}
                                value={s.returnKey}
                                onSelect={(returnKey) => update({ returnKey })}
                                onAdd={() => setAddressTarget('returnKey')}
                                error={errors.returnTo}
                            />
                        ) : null}
                    </>
                ) : null}
            </Card>
        </>
    )

    const renderStep3 = () => (
        <>
            <StepTitle title="Chi possiamo contattare?" subtitle="L’associazione userà questi recapiti per organizzare l’accompagnamento." />
            <Card style={styles.contactPanel}>
                <View style={styles.pickerHead}>
                    <Icon name="call-outline" size={19} color={C.primary} />
                    <Text style={styles.pickerTitle}>Referente principale</Text>
                </View>
                <View style={[styles.addressOption, styles.addressOptionOn, styles.referent]}>
                    <Radio checked />
                    <Initials name={me.name} surname={me.surname} size={33} />
                    <View style={styles.flex}>
                        <Text style={styles.referentName}>
                            {me.name} {me.surname} · Tu
                        </Text>
                        <Text style={styles.referentPhone}>{me.phone}</Text>
                    </View>
                </View>
                {!profileComplete ? (
                    <View>
                        <Text style={styles.hint}>Completa nome, cognome, telefono e data di nascita del referente per proseguire. La richiesta rimane salvata mentre aggiorni il profilo.</Text>
                        <Btn label="Completa i dati del referente" onPress={() => router.push({ pathname: '/editProfile', params: { next: 'wizard' } })} style={styles.sameReturn} />
                        <ErrorText>{errors.profile}</ErrorText>
                    </View>
                ) : <Text style={styles.hint}>L’associazione contatterà te per organizzare il trasporto.</Text>}
            </Card>
            <Btn label="Aggiungi caregiver" icon="person-add-outline" onPress={() => setInviteOpen(true)} />
        </>
    )

    const renderStep4 = () => (
        <>
            <Required />
            <StepTitle title="Chi deve essere accompagnato?" required subtitle="Scegli la persona per cui inviare la richiesta." />
            {people.map((p) => {
                const selected = s.personId === p.id
                const meta = [ageFrom(p.birthDate), p.homeAddress?.city].filter(Boolean).join(' · ')
                return (
                    <Pressable
                        key={p.id}
                        onPress={() => update({ personId: p.id, mobility: selected ? s.mobility : '', help: selected ? s.help : [] })}
                        style={[styles.personOption, selected && styles.personOptionOn]}
                    >
                        <Radio checked={selected} />
                        <Initials name={p.name} surname={p.surname} size={34} />
                        <View style={styles.flex}>
                            <Text style={styles.personName}>
                                {p.name} {p.surname}
                            </Text>
                            {meta ? <Text style={styles.personMeta}>{meta}</Text> : null}
                        </View>
                    </Pressable>
                )
            })}
            <ErrorText>{errors.person}</ErrorText>

            {person ? (
                <View style={styles.support}>
                    <Text style={styles.supportTitle}>Supporto durante il trasporto</Text>
                    <Text style={styles.stepSubtitle}>Aiuta l’associazione a organizzare l’accompagnamento.</Text>
                    <FieldLabel label="Come si sposta la persona?" required style={styles.supportLabel} />
                    {MOBILITY.map((m) => (
                        <OptionRow
                            key={m.value}
                            label={m.label}
                            selected={s.mobility === m.value}
                            onPress={() => update({ mobility: m.value })}
                            style={styles.supportOption}
                        />
                    ))}
                    <ErrorText style={styles.supportError}>{errors.mobility}</ErrorText>
                    <FieldLabel label="Di quale aiuto ha bisogno?" optional style={styles.supportLabel} />
                    {[
                        { value: 'none', label: 'Nessun aiuto aggiuntivo' },
                        ...HELP,
                        { value: 'both', label: 'Entrambi gli aiuti' },
                    ].map((h) => {
                        const on = h.value === 'none' ? s.help.length === 0
                            : h.value === 'both' ? s.help.length === HELP.length
                            : s.help.length === 1 && s.help[0] === h.value
                        return (
                            <OptionRow
                                key={h.value}
                                label={h.label}
                                selected={on}
                                onPress={() => update({ help: h.value === 'none' ? []
                                    : h.value === 'both' ? HELP.map((item) => item.value) : [h.value] })}
                                style={styles.supportOption}
                            />
                        )
                    })}
                </View>
            ) : null}
        </>
    )

    const renderStep5 = () => {
        const fromLines = addressLines(from)
        const toLines = addressLines(to)
        const returnLines = addressLines(returnTo)
        const tags = [MOBILITY.find((m) => m.value === s.mobility)?.label, ...HELP.filter((h) => s.help.includes(h.value)).map((h) => h.label)].filter(Boolean)
        return (
            <>
                <StepTitle title="Controlla la richiesta" subtitle="Verifica le informazioni prima di inviare." />

                <Card style={styles.reviewCard}>
                    <Row>
                        <View style={styles.flex}>
                            <Icon name="calendar-outline" size={18} color={C.primary} />
                            <Text style={styles.reviewLabel}>Data del trasporto</Text>
                            <Text style={styles.reviewStrong}>{formatFullDate(`${s.date}T12:00:00`)}</Text>
                        </View>
                        <View style={styles.flex}>
                            <Icon name="time-outline" size={18} color={C.primary} />
                            <Text style={styles.reviewLabel}>Ora di partenza</Text>
                            <Text style={styles.reviewStrong}>{s.time}</Text>
                        </View>
                    </Row>
                    <Divider />
                    <RouteStops
                        labelColor={C.muted}
                        stops={[
                            { label: 'Partenza', ...fromLines },
                            { label: 'Destinazione', ...toLines },
                            ...(roundTrip ? [{ label: 'Ritorno al termine della visita', ...returnLines }] : []),
                        ]}
                    />
                </Card>

                <Card style={styles.reviewCard}>
                    <View style={styles.reviewPerson}>
                        <Initials name={person.name} surname={person.surname} size={40} />
                        <View>
                            <Text style={styles.reviewLabelTight}>Assistito da accompagnare</Text>
                            <Text style={styles.reviewName}>
                                {person.name} {person.surname}
                            </Text>
                        </View>
                    </View>
                    <Divider />
                    <Text style={styles.reviewLabelTight}>Supporto durante il trasporto</Text>
                    <View style={styles.tags}>
                        {tags.map((t) => (
                            <Text key={t} style={styles.tag}>
                                {t}
                            </Text>
                        ))}
                    </View>
                    <Divider />
                    <View style={styles.reviewService}>
                        <Icon name="mc:stethoscope" size={18} color={C.primary} />
                        <View>
                            <Text style={styles.reviewServiceTitle}>{tripTypeLabel(s.tripType)}</Text>
                            <Text style={styles.reviewMuted}>{directionLabel(s.direction)}</Text>
                        </View>
                    </View>
                </Card>

                <Card style={styles.reviewCard}>
                    <Text style={styles.reviewLabelTight}>Referente principale</Text>
                    <View style={styles.reviewService}>
                        <Icon name="call-outline" size={18} color={C.primary} />
                        <View>
                            <Text style={styles.reviewServiceTitle}>
                                {me.name} {me.surname}
                            </Text>
                            <Text style={styles.reviewPhone}>{me.phone}</Text>
                        </View>
                    </View>
                </Card>

                <Text style={styles.contributionTitle}>Contributo e pagamento</Text>
                <Card style={styles.reviewCard}>
                    <Text style={styles.reviewMuted}>Contributo minimo stimato</Text>
                    <Text style={styles.amount}>{euros(minimum)}</Text>
                    <Text style={styles.reviewMuted}>Per {roundedKm.toLocaleString('it-IT')} km di percorso dell’assistito</Text>
                    <Divider />
                    <View style={styles.payRow}>
                        <Icon name="shield-checkmark-outline" size={18} color={C.primary} />
                        <Text style={styles.payFree}>Ora non devi pagare nulla</Text>
                    </View>
                    <View style={styles.payRow}>
                        <Icon name="time-outline" size={18} color={C.primary} />
                        <View style={styles.flex}>
                            <Text style={styles.payTitle}>Quando paghi?</Text>
                            <Text style={styles.payText}>Al momento del servizio.</Text>
                        </View>
                    </View>
                    <View style={styles.payRow}>
                        <Icon name="mc:hand-coin-outline" size={18} color={C.primary} />
                        <View style={styles.flex}>
                            <Text style={styles.payTitle}>A chi versi il contributo?</Text>
                            <Text style={styles.payText}>All’associazione che confermerà il trasporto.</Text>
                        </View>
                    </View>
                    <View style={styles.payRow}>
                        <Icon name="information-circle-outline" size={18} color={C.primary} />
                        <View style={styles.flex}>
                            <Text style={styles.payTitle}>La cifra è definitiva?</Text>
                            <Text style={styles.payText}>
                                È una stima. L’associazione ti confermerà l’importo e come pagare prima del trasporto. Eventuali
                                variazioni ti saranno comunicate prima del servizio.
                            </Text>
                        </View>
                    </View>
                    <Divider />
                    <FieldLabel label="Vuoi aggiungere una donazione?" />
                    <OptionRow selected={!s.donate} onPress={() => update({ donate: false })} label="No, solo il contributo minimo" style={styles.sameReturn} />
                    <OptionRow selected={s.donate} onPress={() => update({ donate: true })} label="Sì, voglio donare di più" />
                    {s.donate ? (
                        <View style={styles.extra}>
                            <FieldLabel label="Donazione aggiuntiva (€)" />
                            <Input style={styles.donationInput} value={String(s.extra)} onChangeText={(v) => {
                                if (donationDigitCount(v) > MAX_DONATION_DIGITS) {
                                    // Keep the previous value, including when pasting an oversized amount.
                                    setS((prev) => ({ ...prev }))
                                    setErrors({ extra: 'La donazione può contenere al massimo 10 cifre complessive.' })
                                    return
                                }
                                update({ extra: v })
                            }}
                                keyboardType="decimal-pad" inputMode="decimal" maxLength={24} placeholder="0,00" accessibilityLabel="Donazione aggiuntiva in euro" />
                            <Text style={styles.hint}>Massimo 10 cifre complessive, con non più di due decimali.</Text>
                            <ErrorText>{errors.extra || (String(s.extra).trim() ? donation.error : '')}</ErrorText>
                        </View>
                    ) : null}
                    <Divider />
                    <View style={styles.totalRow}>
                        <Text style={styles.totalLabel}>Totale stimato</Text>
                        <Text style={styles.total}>{euros(donation.total)}</Text>
                    </View>
                </Card>

                <Text style={styles.afterSend}>Dopo l’invio, il trasporto dovrà essere approvato da un’associazione.</Text>
                <ErrorText>{submitError}</ErrorText>
            </>
        )
    }

    const steps = { 1: renderStep1, 2: renderStep2, 3: renderStep3, 4: renderStep4, 5: renderStep5 }

    return (
        <Page>
            <Scroll ref={scrollRef}>
                {saved === '1' ? <Banner variant="success" message="Dati del profilo salvati. Puoi continuare la richiesta." onDismiss={() => router.setParams({ saved: '' })} /> : null}
                <PageTitle style={styles.title}>Nuovo trasporto</PageTitle>
                <Progress step={s.step} />
                {steps[s.step]()}
            </Scroll>
            <Footer>
                <Btn label="Indietro" variant="secondary" onPress={back} style={styles.footerBtn} />
                {s.step < STEPS ? (
                    <Btn label="Continua" onPress={next} loading={busy} style={styles.footerBtn} />
                ) : (
                    <Btn label="Invia richiesta" onPress={submit} loading={busy} style={styles.footerBtn} />
                )}
            </Footer>

            <AddressModal
                visible={!!addressTarget}
                onClose={() => setAddressTarget(null)}
                onSave={(address) => {
                    const key = `add-${Date.now()}`
                    setS((prev) => ({ ...prev, added: [...prev.added, { key, label: '', ...address }], [addressTarget]: key }))
                    setAddressTarget(null)
                }}
            />
            <InviteCaregiverModal visible={inviteOpen} onClose={() => setInviteOpen(false)} person={person} people={people} />
        </Page>
    )
}

export default Create

const styles = StyleSheet.create({
    flex: { flex: 1 },
    title: { marginBottom: 12 },
    star: { color: C.primary, fontWeight: '700' },
    hint: { fontSize: 13, lineHeight: 20, color: C.muted, marginTop: 8 },

    progress: { marginBottom: 18 },
    progressText: { fontSize: 12, color: C.muted, marginBottom: 10 },
    bars: { flexDirection: 'row', gap: 6 },
    bar: { flex: 1, height: 4, borderRadius: 2, backgroundColor: '#c9c6d4' },
    barOn: { backgroundColor: C.primary },
    requiredNote: { fontSize: 12, color: C.muted, marginBottom: 16 },
    stepHead: { marginBottom: 16 },
    stepTitle: { fontSize: 20, fontWeight: '600', color: C.text },
    stepSubtitle: { fontSize: 14, lineHeight: 21, color: C.muted, marginTop: 8 },

    services: { flexDirection: 'row', flexWrap: 'wrap', gap: 12, marginBottom: 20 },
    service: {
        width: '47.5%',
        flexGrow: 1,
        minHeight: 120,
        backgroundColor: C.card,
        borderRadius: 14,
        borderWidth: 1.5,
        borderColor: 'transparent',
        padding: 12,
    },
    serviceOn: { borderColor: C.primary, backgroundColor: C.cardSelected },
    serviceSymbol: { width: 32, height: 32, borderRadius: 9, backgroundColor: C.initialsBg, alignItems: 'center', justifyContent: 'center', marginBottom: 8 },
    serviceCheck: { position: 'absolute', top: 10, right: 10 },
    serviceTitle: { fontSize: 14, fontWeight: '600', color: C.text, marginBottom: 3 },
    serviceText: { fontSize: 12, lineHeight: 17, color: C.muted },
    directionLabel: { marginBottom: 10 },
    directionOption: { flex: 1, paddingVertical: 11 },

    routePanel: { marginTop: 8, padding: 16 },
    panelDivider: { marginVertical: 16 },
    pickerHead: { flexDirection: 'row', alignItems: 'center', gap: 10, marginBottom: 8 },
    pickerTitle: { fontSize: 16, fontWeight: '600', color: C.text },
    pickerHint: { fontSize: 12, color: C.muted, marginBottom: 10 },
    pickerHintIndented: { fontSize: 13, marginLeft: 29 },
    addressOption: {
        flexDirection: 'row',
        alignItems: 'center',
        gap: 12,
        backgroundColor: C.page,
        borderRadius: 12,
        borderWidth: 1,
        borderColor: '#b8b5c7',
        paddingVertical: 12,
        paddingHorizontal: 12,
        marginBottom: 10,
    },
    addressOptionOn: { borderColor: C.primary, backgroundColor: C.cardSelected },
    addressRadio: { paddingTop: 2 },
    addressTag: { fontSize: 12, fontWeight: '600', color: C.primary, marginBottom: 3 },
    addressLine1: { fontSize: 15, color: C.text },
    addressLine2: { fontSize: 12, color: C.muted, marginTop: 3 },
    addAddress: { minHeight: 44, borderRadius: 10, marginTop: 2 },
    addAddressText: { fontSize: 14 },
    sameReturn: { marginTop: 4, marginBottom: 10 },

    contactPanel: { marginBottom: 20, padding: 18 },
    referent: { marginTop: 8, marginBottom: 0 },
    referentName: { fontSize: 14, fontWeight: '600', color: C.text },
    referentPhone: { fontSize: 13, color: C.muted, marginTop: 4 },

    personOption: {
        flexDirection: 'row',
        alignItems: 'center',
        gap: 10,
        backgroundColor: C.card,
        borderRadius: 14,
        borderWidth: 1.5,
        borderColor: 'transparent',
        padding: 12,
        marginBottom: 12,
    },
    personOptionOn: { borderColor: C.primary, backgroundColor: C.cardSelected },
    personName: { fontSize: 15, fontWeight: '600', color: C.text },
    personMeta: { fontSize: 12, color: C.muted, marginTop: 3 },
    support: { marginTop: 10 },
    supportTitle: { fontSize: 17, fontWeight: '600', color: C.text },
    supportLabel: { marginTop: 16, marginBottom: 10 },
    supportOption: { marginBottom: 10 },
    supportError: { marginTop: -4 },

    reviewCard: { padding: 16, marginBottom: 14 },
    reviewLabel: { fontSize: 12, color: C.muted, marginTop: 8, marginBottom: 4 },
    reviewLabelTight: { fontSize: 12, color: C.muted, marginBottom: 4 },
    reviewStrong: { fontSize: 14, fontWeight: '700', color: C.text },
    reviewPerson: { flexDirection: 'row', alignItems: 'center', gap: 12 },
    reviewName: { fontSize: 16, fontWeight: '600', color: C.text },
    tags: { flexDirection: 'row', flexWrap: 'wrap', gap: 8, marginTop: 6 },
    tag: { fontSize: 12, color: C.primary, backgroundColor: C.initialsBg, borderRadius: 6, paddingHorizontal: 8, paddingVertical: 5, overflow: 'hidden' },
    reviewService: { flexDirection: 'row', alignItems: 'flex-start', gap: 12, marginTop: 4 },
    reviewServiceTitle: { fontSize: 15, fontWeight: '600', color: C.text },
    reviewMuted: { fontSize: 13, color: C.muted, marginTop: 2 },
    reviewPhone: { fontSize: 15, color: C.primary, marginTop: 4 },
    contributionTitle: { fontSize: 18, fontWeight: '600', color: C.text, marginTop: 10, marginBottom: 12 },
    amount: { fontSize: 30, fontWeight: '700', color: C.primary, marginVertical: 6 },
    payRow: { flexDirection: 'row', alignItems: 'flex-start', gap: 12, marginBottom: 14 },
    payFree: { fontSize: 14, fontWeight: '600', color: C.primary },
    payTitle: { fontSize: 14, fontWeight: '600', color: C.text },
    payText: { fontSize: 13, lineHeight: 20, color: C.muted, marginTop: 3 },
    extra: { marginTop: 10 },
    donationInput: { backgroundColor: C.white },
    totalRow: { flexDirection: 'row', flexWrap: 'wrap', gap: 8, justifyContent: 'space-between', alignItems: 'center' },
    totalLabel: { fontSize: 14, color: C.text },
    total: { fontSize: 22, fontWeight: '700', color: C.primary, flexShrink: 1, minWidth: 0 },
    afterSend: { fontSize: 13, lineHeight: 20, color: C.muted, marginTop: 6 },

    footerBtn: { paddingHorizontal: 26 },

    sent: { alignItems: 'center', justifyContent: 'center', paddingHorizontal: 28 },
    sentTitle: { fontSize: 24, fontWeight: '600', color: C.text, marginTop: 20, marginBottom: 12 },
    sentState: { fontSize: 18, fontWeight: '600', color: C.waiting, marginBottom: 12 },
    sentText: { fontSize: 16, lineHeight: 25, color: C.muted, textAlign: 'center', marginBottom: 24 },
    sentBtn: { paddingHorizontal: 18 },
})
