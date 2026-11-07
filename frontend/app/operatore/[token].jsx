import { useCallback, useState } from 'react'
import { View, Text, StyleSheet } from 'react-native'
import { useLocalSearchParams, useFocusEffect } from 'expo-router'

import api from '../../config/httpClient.js'
import { getItem, setItem } from '../../config/storage.js'
import { C, apiErrorMessage } from '../../constants/caregiver'
import { EXECUTION_LABELS } from '../../constants/association'
import Header from '../../components/Header'
import { Page, Scroll, Loading, PageTitle, Card, Btn, Field, Dialog, ErrorText, SuccessCheck, Muted } from '../../components/caregiver/ui'

// Pagina aperta dal link operatore (Frontend:OperatorUrl = /operatore/{token}): non richiede l'accesso.
// L'endpoint pubblico restituisce solo lo stato del viaggio, non i dati del trasporto.
const OperatorPage = () => {
	const { token } = useLocalSearchParams()
	const storageKey = `operatore-${token}`

	const [loading, setLoading] = useState(true)
	const [status, setStatus] = useState(null)
	const [error, setError] = useState('')
	const [operator, setOperator] = useState('')
	const [nameInput, setNameInput] = useState('')
	const [nameError, setNameError] = useState('')
	const [busy, setBusy] = useState(false)
	const [actionError, setActionError] = useState('')
	const [confirm, setConfirm] = useState(null) // 'complete' | 'suspend'

	useFocusEffect(
		useCallback(() => {
			const load = async () => {
				try {
					setOperator((await getItem(storageKey)) ?? '')
					const { data } = await api.get(`/operator/${token}/status`)
					setStatus(data)
					setError('')
				} catch (e) {
					setError(apiErrorMessage(e, 'Link non disponibile.'))
				} finally {
					setLoading(false)
				}
			}
			load()
		}, [token]),
	)

	// Il nome dell'operatore viene chiesto la prima volta e ricordato su questo dispositivo
	const operatorName = async () => {
		if (operator) return operator
		const value = nameInput.trim()
		if (!value) {
			setNameError('Inserisci il tuo nome.')
			return null
		}
		await setItem(storageKey, value)
		setOperator(value)
		return value
	}

	const send = async (action) => {
		const name = await operatorName()
		if (!name) return
		setBusy(true)
		setActionError('')
		try {
			const { data } = await api.post(`/operator/${token}/status`, { action, operatorLabel: name })
			setStatus(data)
			setConfirm(null)
		} catch (e) {
			setActionError(apiErrorMessage(e, 'Aggiornamento non disponibile.'))
		} finally {
			setBusy(false)
		}
	}

	if (loading) return <Loading />

	let body
	if (error) {
		body = (
			<View style={styles.empty}>
				<Text style={styles.emptyTitle}>Link non disponibile</Text>
				<Text style={styles.emptyText}>
					Questo link è scaduto oppure il trasporto non è più attivo. Contatta la tua associazione.
				</Text>
			</View>
		)
	} else if (status.isCompleted) {
		body = (
			<View style={styles.empty}>
				<SuccessCheck />
				<Text style={[styles.emptyTitle, styles.spaced]}>Trasporto concluso</Text>
				<Text style={styles.emptyText}>Gli aggiornamenti sono stati registrati. Grazie per l’accompagnamento.</Text>
			</View>
		)
	} else {
		const next = status.nextStatus
		body = (
			<>
				<PageTitle>Il tuo trasporto</PageTitle>
				<Card style={styles.state}>
					<Text style={styles.stateLabel}>Stato attuale</Text>
					<Text style={styles.stateTitle}>
						{status.currentStatus ? EXECUTION_LABELS[status.currentStatus] : 'Pronto per iniziare'}
					</Text>
					<Text style={styles.stateText}>
						{status.isSuspended
							? 'Riprendi il trasporto quando puoi proseguire.'
							: 'Conferma il passaggio solo quando è avvenuto.'}
					</Text>
				</Card>

				{operator ? (
					<Muted style={styles.operator}>Operatore: {operator}</Muted>
				) : (
					<Field
						label="Il tuo nome"
						required
						value={nameInput}
						onChangeText={(v) => {
							setNameInput(v)
							setNameError('')
						}}
						autoComplete="given-name"
						error={nameError}
					/>
				)}

				{status.canResume ? (
					<Btn label="Riprendi trasporto" onPress={() => send('Resume')} loading={busy} style={styles.btn} />
				) : next ? (
					<Btn
						label={next === 'Completed' ? 'Concludi trasporto' : EXECUTION_LABELS[next]}
						icon={next === 'Completed' ? 'checkmark' : 'arrow-forward'}
						onPress={() => (next === 'Completed' ? setConfirm('complete') : send('Advance'))}
						loading={busy && !confirm}
						style={styles.btn}
					/>
				) : null}
				{status.canSuspend ? (
					<Btn label="Segnala sospensione" icon="pause" variant="secondary" onPress={() => setConfirm('suspend')} style={styles.btn} />
				) : null}
				<ErrorText>{actionError}</ErrorText>
			</>
		)
	}

	return (
		<Page>
			<Header brandOnly />
			<Scroll>{body}</Scroll>
			<Dialog visible={!!confirm} onClose={() => setConfirm(null)}>
				<Text style={styles.dialogTitle}>{confirm === 'complete' ? 'Concludi trasporto' : 'Sospendi trasporto'}</Text>
				<Text style={styles.dialogText}>
					{confirm === 'complete'
						? 'Confermi che l’accompagnamento è terminato? Dopo la conferma il trasporto verrà chiuso.'
						: 'Segnala un guasto o un imprevisto durante il trasporto.'}
				</Text>
				<View style={styles.dialogActions}>
					<Btn label="Annulla" variant="secondary" onPress={() => setConfirm(null)} style={styles.flex} />
					<Btn
						label={confirm === 'complete' ? 'Concludi' : 'Segnala sospensione'}
						onPress={() => send(confirm === 'complete' ? 'Advance' : 'Suspend')}
						loading={busy}
						style={styles.flex}
					/>
				</View>
			</Dialog>
		</Page>
	)
}

export default OperatorPage

const styles = StyleSheet.create({
	flex: { flex: 1 },
	state: { alignItems: 'center', paddingVertical: 20, marginBottom: 16 },
	stateLabel: { fontSize: 12, color: C.muted },
	stateTitle: { fontSize: 24, fontWeight: '600', color: C.text, marginVertical: 8 },
	stateText: { fontSize: 13, color: C.muted, textAlign: 'center' },
	operator: { fontSize: 13, marginBottom: 14 },
	btn: { marginBottom: 12 },
	empty: { alignItems: 'center', paddingVertical: 40, paddingHorizontal: 12 },
	emptyTitle: { fontSize: 22, fontWeight: '600', color: C.text, marginBottom: 12, textAlign: 'center' },
	spaced: { marginTop: 18 },
	emptyText: { fontSize: 14, lineHeight: 23, color: C.muted, textAlign: 'center' },
	dialogTitle: { fontSize: 20, fontWeight: '600', color: C.text, marginBottom: 12 },
	dialogText: { fontSize: 15, lineHeight: 23, color: C.muted },
	dialogActions: { flexDirection: 'row', gap: 12, marginTop: 20 },
})
