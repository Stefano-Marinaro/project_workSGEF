import { useCallback, useState } from 'react'
import { View, Text, StyleSheet, Linking } from 'react-native'
import { useLocalSearchParams, useRouter, useFocusEffect } from 'expo-router'

import api from '../../../config/httpClient.js'
import {
	C,
	tripTypeShort,
	directionLabel,
	formatFullDate,
	formatTime,
	addressLines,
	apiErrorMessage,
} from '../../../constants/caregiver'
import { shortId } from '../../../constants/association'
import { AssocStatus } from '../../../components/association/AssocTripCard'
import {
	Page,
	Scroll,
	Loading,
	PageTitle,
	BackLink,
	Card,
	Icon,
	Initials,
	Divider,
	RouteStops,
	Btn,
	Footer,
	Dialog,
	FieldLabel,
	Input,
	ErrorText,
	EmptyState,
} from '../../../components/caregiver/ui'
import Banner from '../../../components/Banner'

const PanelTitle = ({ children }) => <Text style={styles.panelTitle}>{children}</Text>

// Popup di conferma usato per accetta / rifiuta / disdici (modal del mockup)
const ActionDialog = ({ action, onClose, onConfirm, loading, error, trip }) => {
	const [reason, setReason] = useState('')
	if (!action) return null
	const release = action === 'release'
	const decline = action === 'decline'
	const title = release ? 'Disdici trasporto' : decline ? 'Rifiuta richiesta' : 'Accetta richiesta'
	const text = release
		? 'Il trasporto uscirà da quelli in carico. La richiesta rimarrà disponibile per le altre associazioni destinatarie.'
		: decline
			? 'La richiesta scomparirà dal tuo elenco e resterà disponibile per le altre associazioni.'
			: `Confermi di poter effettuare il trasporto del ${formatFullDate(trip.departureDateHour)} alle ${formatTime(trip.departureDateHour)}?`
	return (
		<Dialog visible onClose={onClose}>
			<Text style={styles.dialogTitle}>{title}</Text>
			<Text style={styles.dialogText}>{text}</Text>
			{release || decline ? (
				<>
					<FieldLabel label={release ? 'Motivo' : 'Motivo (facoltativo)'} />
					<Input multiline value={reason} onChangeText={setReason} style={styles.reason} maxLength={300} />
				</>
			) : null}
			<ErrorText>{error}</ErrorText>
			<View style={styles.dialogActions}>
				<Btn label="Annulla" variant="secondary" onPress={onClose} style={styles.flex} />
				<Btn
					label={release ? 'Disdici' : decline ? 'Rifiuta' : 'Accetta'}
					variant={release || decline ? 'danger' : 'primary'}
					disabled={release && !reason.trim()}
					loading={loading}
					onPress={() => onConfirm(reason.trim())}
					style={styles.flex}
				/>
			</View>
		</Dialog>
	)
}

const AssocDetail = () => {
	const { id, kind } = useLocalSearchParams()
	const router = useRouter()

	const [loading, setLoading] = useState(true)
	const [error, setError] = useState('')
	const [trip, setTrip] = useState(null)
	const [contacts, setContacts] = useState(null)
	const [notice, setNotice] = useState('')

	const [action, setAction] = useState(null)
	const [acting, setActing] = useState(false)
	const [actionError, setActionError] = useState('')

	const load = useCallback(async (as = kind) => {
		setLoading(true)
		try {
			// Le richieste in attesa e i trasporti assegnati hanno due endpoint diversi;
			// senza "kind" (es. da una notifica) si provano entrambi.
			let data
			if (as === 'request') data = (await api.get(`/association/requests/${id}`)).data
			else {
				try {
					data = (await api.get(`/association/transports/${id}`)).data
				} catch (e) {
					if (as === 'transport' || e.response?.status !== 404) throw e
					data = (await api.get(`/association/requests/${id}`)).data
				}
			}
			setTrip(data)
			// I recapiti si vedono solo dopo l'accettazione e finché il trasporto è attivo
			if (data.requestStatus === 'Confirmed' || data.requestStatus === 'InProgress') {
				const c = await api.get(`/association/requests/${id}/contacts`).catch(() => ({ data: null }))
				setContacts(c.data)
			} else setContacts(null)
			setError('')
		} catch (e) {
			setError(apiErrorMessage(e, 'Richiesta non trovata o non più disponibile.'))
		} finally {
			setLoading(false)
		}
	}, [id, kind])

	useFocusEffect(
		useCallback(() => {
			setNotice('')
			load()
		}, [load]),
	)

	if (loading) return <Loading />

	if (error || !trip) {
		return (
			<Page>
				<Scroll>
					<EmptyState
						title="Contenuto non disponibile"
						text={error}
						action="Apri Richieste"
						onAction={() => router.replace('/association/request')}
					/>
				</Scroll>
			</Page>
		)
	}

	const status = trip.requestStatus
	const isPending = status === 'Pending'
	const active = status === 'Confirmed' || status === 'InProgress'
	const list = isPending ? 'richieste' : active ? 'trasporti' : 'storico'
	const listRoute = { richieste: '/association/request', trasporti: '/association/trasport', storico: '/association/history' }[list]
	const roundTrip = trip.tripDirection === 'RoundTrip'
	const start = addressLines(trip.startAddress)
	const end = addressLines(trip.endAddress)
	const beneficiary = contacts ? `${contacts.beneficiaryName} ${contacts.beneficiarySurname}` : ''
	const referent = contacts ? [contacts.requesterName, contacts.requesterSurname].filter(Boolean).join(' ') : ''

	const confirm = async (reason) => {
		setActing(true)
		setActionError('')
		try {
			if (action === 'accept') {
				await api.post(`/association/requests/${id}/accept`)
				setAction(null)
				await load('transport')
				setNotice('Richiesta accettata. Il trasporto è ora in carico alla tua associazione.')
			} else if (action === 'decline') {
				await api.post(`/association/requests/${id}/decline`, { reason: reason || null })
				setAction(null)
				router.replace('/association/request')
			} else {
				await api.post(`/association/transports/${id}/cancel`, { reason })
				setAction(null)
				router.replace('/association/trasport')
			}
		} catch (e) {
			setActionError(apiErrorMessage(e, 'L’azione non è più disponibile.'))
		} finally {
			setActing(false)
		}
	}

	return (
		<Page>
			<Scroll>
				<PageTitle>{isPending ? 'Dettaglio richiesta' : 'Dettaglio trasporto'}</PageTitle>
				<BackLink
					label={list === 'richieste' ? 'Torna alle richieste' : list === 'trasporti' ? 'Torna ai trasporti' : 'Torna allo storico'}
					onPress={() => router.replace(listRoute)}
				/>
				{notice ? <Banner variant="success" message={notice} onDismiss={() => setNotice('')} style={styles.notice} /> : null}

				<View style={styles.statusRow}>
					<Text style={styles.id}>{shortId(trip.id)}</Text>
					<AssocStatus status={status} />
				</View>

				<Card style={styles.panel}>
					{beneficiary ? (
						<>
							<View style={styles.identity}>
								<Initials name={contacts.beneficiaryName} surname={contacts.beneficiarySurname} size={44} solid />
								<View>
									<Text style={styles.label}>Assistito da accompagnare</Text>
									<Text style={styles.h3}>{beneficiary}</Text>
									<Text style={styles.sub}>{contacts.beneficiaryPhone}</Text>
								</View>
							</View>
							<Divider />
						</>
					) : null}
					<View style={styles.service}>
						<Icon name="car-outline" size={20} color={C.primary} />
						<View>
							<Text style={styles.serviceTitle}>{tripTypeShort(trip.tripType)}</Text>
							<Text style={styles.sub}>{directionLabel(trip.tripDirection)}</Text>
						</View>
					</View>
					<Divider />
					<Text style={styles.label}>Supporto durante il trasporto</Text>
					<Text style={styles.sub}>Esigenze non specificate nella richiesta</Text>
				</Card>

				<Card style={styles.panel}>
					<View style={styles.two}>
						<View style={styles.flex}>
							<Icon name="calendar-outline" size={18} color={C.primary} />
							<Text style={styles.labelTop}>Data del trasporto</Text>
							<Text style={styles.value}>{formatFullDate(trip.departureDateHour)}</Text>
						</View>
						<View style={styles.flex}>
							<Icon name="time-outline" size={18} color={C.primary} />
							<Text style={styles.labelTop}>Ora di partenza</Text>
							<Text style={styles.value}>{formatTime(trip.departureDateHour)}</Text>
						</View>
					</View>
					<Divider />
					{roundTrip ? <Text style={styles.leg}>Andata</Text> : null}
					<RouteStops labelColor={C.muted} stops={[{ label: 'Partenza', ...start }, { label: 'Destinazione', ...end }]} />
					{roundTrip ? (
						<>
							<Divider style={styles.legDivider} />
							<Text style={styles.leg}>Ritorno</Text>
							<RouteStops
								labelColor={C.muted}
								stops={[
									{ label: 'Partenza', ...end, time: trip.returnDateHour ? formatTime(trip.returnDateHour) : undefined },
									{ label: 'Destinazione', ...start },
								]}
							/>
						</>
					) : null}
				</Card>

				<Card style={styles.panel}>
					<PanelTitle>Referente principale</PanelTitle>
					<View style={styles.contact}>
						<Icon name="call-outline" size={20} color={C.muted} />
						<View style={styles.flex}>
							{contacts ? (
								<>
									<Text style={styles.contactName}>{referent || 'Referente'}</Text>
									<Text
										style={styles.phone}
										onPress={() => Linking.openURL(`tel:${contacts.referencePhone.replace(/\s/g, '')}`)}
									>
										{contacts.referencePhone}
									</Text>
									<Text style={styles.sub}>{contacts.referenceEmail || contacts.requesterEmail}</Text>
								</>
							) : (
								<Text style={styles.sub}>
									{isPending
										? 'I recapiti saranno disponibili dopo l’accettazione.'
										: 'I recapiti non sono più disponibili per questo trasporto.'}
								</Text>
							)}
						</View>
					</View>
				</Card>

				{active ? (
					<>
						<Text style={styles.sectionTitle}>Stato del trasporto</Text>
						<Card style={styles.panel}>
							<Text style={styles.h3}>{status === 'InProgress' ? 'In corso' : 'In attesa dell’operatore'}</Text>
							<Text style={styles.sub}>L’operatore aggiorna lo stato durante il trasporto.</Text>
							<Btn
								label="Gestisci link operatore"
								icon="link-outline"
								onPress={() => router.push({ pathname: '/association/link/[id]', params: { id } })}
								style={styles.linkBtn}
							/>
						</Card>
					</>
				) : null}

				{status === 'Confirmed' ? (
					<Btn
						label="Disdici trasporto"
						variant="danger"
						icon="close"
						onPress={() => {
							setActionError('')
							setAction('release')
						}}
					/>
				) : null}
			</Scroll>

			{isPending ? (
				<Footer>
					<Btn
						label="Rifiuta richiesta"
						variant="danger"
						icon="close"
						onPress={() => {
							setActionError('')
							setAction('decline')
						}}
						style={styles.flex}
					/>
					<Btn
						label="Accetta richiesta"
						icon="checkmark"
						onPress={() => {
							setActionError('')
							setAction('accept')
						}}
						style={styles.flex}
					/>
				</Footer>
			) : null}

			<ActionDialog
				key={action ?? 'none'}
				action={action}
				trip={trip}
				onClose={() => setAction(null)}
				onConfirm={confirm}
				loading={acting}
				error={actionError}
			/>
		</Page>
	)
}

export default AssocDetail

const styles = StyleSheet.create({
	flex: { flex: 1 },
	notice: { marginBottom: 14 },
	statusRow: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', marginBottom: 14 },
	id: { fontSize: 13, color: C.muted },
	panel: { marginBottom: 16, padding: 16 },
	panelTitle: { fontSize: 14, fontWeight: '600', color: C.muted, marginBottom: 14 },
	identity: { flexDirection: 'row', alignItems: 'center', gap: 12 },
	label: { fontSize: 12, color: C.muted, marginBottom: 4 },
	labelTop: { fontSize: 12, color: C.muted, marginTop: 10, marginBottom: 4 },
	h3: { fontSize: 18, fontWeight: '600', color: C.text },
	sub: { fontSize: 13, lineHeight: 20, color: C.muted, marginTop: 2 },
	service: { flexDirection: 'row', alignItems: 'flex-start', gap: 12 },
	serviceTitle: { fontSize: 15, fontWeight: '600', color: C.text },
	two: { flexDirection: 'row', gap: 12 },
	value: { fontSize: 15, fontWeight: '600', color: C.text },
	leg: { fontSize: 14, color: C.muted, marginBottom: 12 },
	legDivider: { marginTop: 0 },
	contact: { flexDirection: 'row', gap: 14, alignItems: 'flex-start' },
	contactName: { fontSize: 16, color: C.text },
	phone: { fontSize: 15, color: C.primary, marginTop: 6 },
	sectionTitle: { fontSize: 14, fontWeight: '600', color: C.muted, marginTop: 6, marginBottom: 10 },
	linkBtn: { marginTop: 16 },
	dialogTitle: { fontSize: 20, fontWeight: '600', color: C.text, marginBottom: 12 },
	dialogText: { fontSize: 15, lineHeight: 23, color: C.muted, marginBottom: 14 },
	reason: { minHeight: 90 },
	dialogActions: { flexDirection: 'row', gap: 12, marginTop: 16 },
})
