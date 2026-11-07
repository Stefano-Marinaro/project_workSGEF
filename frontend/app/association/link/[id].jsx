import { useCallback, useState } from 'react'
import { View, Text, StyleSheet } from 'react-native'
import { useLocalSearchParams, useRouter, useFocusEffect } from 'expo-router'
import * as Clipboard from 'expo-clipboard'

import api from '../../../config/httpClient.js'
import { C, tripTypeShort, formatDayMonth, formatTime, formatFullDate, apiErrorMessage } from '../../../constants/caregiver'
import { operatorLinks } from '../../../constants/association'
import { Page, Scroll, Loading, PageTitle, Card, Btn, ErrorText, EmptyState, Muted } from '../../../components/caregiver/ui'

// Mockup "Link operatore": genera il link con cui l'operatore aggiorna lo stato del viaggio
const OperatorLink = () => {
	const { id } = useLocalSearchParams()
	const router = useRouter()
	const [loading, setLoading] = useState(true)
	const [trip, setTrip] = useState(null)
	const [error, setError] = useState('')
	const [link, setLink] = useState(null)
	const [generating, setGenerating] = useState(false)
	const [copied, setCopied] = useState(false)

	useFocusEffect(
		useCallback(() => {
			const load = async () => {
				try {
					const { data } = await api.get(`/association/transports/${id}`)
					setTrip(data)
					// Il backend non elenca i link già creati: si mostra l'ultimo generato in questa sessione
					const latest = operatorLinks[id]
					setLink(latest && new Date(latest.expiresAt) > new Date() ? latest : null)
					setError('')
				} catch (e) {
					setError(apiErrorMessage(e, 'Trasporto non trovato.'))
				} finally {
					setLoading(false)
				}
			}
			setCopied(false)
			load()
		}, [id]),
	)

	const generate = async () => {
		setGenerating(true)
		setError('')
		try {
			const { data } = await api.post(`/association/transports/${id}/operator-link`)
			operatorLinks[id] = data
			setLink(data)
			setCopied(false)
		} catch (e) {
			setError(apiErrorMessage(e, 'Impossibile generare il link.'))
		} finally {
			setGenerating(false)
		}
	}

	if (loading) return <Loading />

	const eligible = trip && (trip.requestStatus === 'Confirmed' || trip.requestStatus === 'InProgress')
	if (!eligible) {
		return (
			<Page>
				<Scroll>
					<PageTitle>Link operatore</PageTitle>
					<EmptyState
						title="Trasporto non attivo"
						text={error || 'Puoi generare il link solo per un trasporto accettato e non ancora concluso.'}
					/>
				</Scroll>
			</Page>
		)
	}

	const token = link ? link.url.split('/').pop() : ''

	return (
		<Page>
			<Scroll>
				<PageTitle>Link operatore</PageTitle>
				<Muted style={styles.description}>
					Invia il link alla persona che effettuerà il trasporto. Potrà aggiornare lo stato senza accedere all’app.
				</Muted>
				<Card style={styles.panel}>
					<Text style={styles.h3}>{tripTypeShort(trip.tripType)}</Text>
					<Text style={styles.sub}>
						{formatDayMonth(trip.departureDateHour)} · {formatTime(trip.departureDateHour)}
					</Text>
				</Card>

				{link ? (
					<>
						<Text style={styles.section}>Link da condividere</Text>
						<Card style={styles.panel}>
							<Text style={styles.label}>Valido fino al</Text>
							<Text style={styles.value}>
								{formatFullDate(link.expiresAt)} · {formatTime(link.expiresAt)}
							</Text>
							<View style={styles.linkBox}>
								<Text style={styles.linkText} selectable>
									{link.url}
								</Text>
							</View>
							<View style={styles.actions}>
								<Btn
									label={copied ? 'Link copiato' : 'Copia link'}
									icon={copied ? 'checkmark' : 'copy-outline'}
									onPress={async () => {
										await Clipboard.setStringAsync(link.url)
										setCopied(true)
									}}
									style={styles.flex}
								/>
								<Btn
									label="Apri schermata"
									variant="ghost"
									onPress={() => router.push({ pathname: '/operatore/[token]', params: { token } })}
									style={styles.flex}
									textStyle={styles.openText}
								/>
							</View>
						</Card>
						<Btn label="Genera un altro link" variant="secondary" onPress={generate} loading={generating} />
						<Text style={styles.note}>
							I link già generati rimangono validi fino alla scadenza o alla conclusione del trasporto.
						</Text>
					</>
				) : (
					<Btn label="Genera link operatore" icon="link-outline" onPress={generate} loading={generating} />
				)}
				<ErrorText>{error}</ErrorText>
			</Scroll>
		</Page>
	)
}

export default OperatorLink

const styles = StyleSheet.create({
	flex: { flex: 1 },
	description: { fontSize: 15, lineHeight: 24, marginBottom: 16 },
	panel: { padding: 16, marginBottom: 16 },
	h3: { fontSize: 18, fontWeight: '600', color: C.text },
	sub: { fontSize: 13, color: C.muted, marginTop: 6 },
	section: { fontSize: 14, fontWeight: '600', color: C.muted, marginBottom: 10 },
	label: { fontSize: 12, color: C.muted },
	value: { fontSize: 15, fontWeight: '500', color: C.text, marginTop: 6 },
	linkBox: { borderWidth: 1, borderColor: '#b8b5c7', borderRadius: 10, padding: 12, marginTop: 14, backgroundColor: C.page },
	linkText: { fontSize: 12, lineHeight: 19, color: C.text },
	actions: { flexDirection: 'row', gap: 10, marginTop: 14 },
	openText: { color: C.muted },
	note: { fontSize: 13, lineHeight: 20, color: C.muted, marginTop: 12 },
})
