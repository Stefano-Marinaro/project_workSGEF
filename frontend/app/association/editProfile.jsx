import { useCallback, useEffect, useState } from 'react'
import { View, Text, Pressable, StyleSheet } from 'react-native'
import { useFocusEffect, useLocalSearchParams, useRouter } from 'expo-router'

import api from '../../config/httpClient.js'
import { C, apiErrorMessage } from '../../constants/caregiver'
import { Page, Scroll, Loading, PageTitle, Field, Row, Input, Btn, Footer, ErrorText, Dialog, Icon, Muted } from '../../components/caregiver/ui'

const EMPTY = { name: '', email: '', phone: '', hours: '', street: '', number: '', city: '', cap: '', province: '' }

// Mockup "Modifica profilo" / "Completa il profilo" dell'associazione.
// register=1: primo completamento dopo la registrazione (PATCH /association/profile).
const EditAssociationProfile = () => {
	const router = useRouter()
	const { register } = useLocalSearchParams()
	const isRegister = register === '1'

	const [loading, setLoading] = useState(true)
	const [form, setForm] = useState(EMPTY)
	const [otherPhones, setOtherPhones] = useState([])
	const [comuni, setComuni] = useState([]) // [{ codiceIstat, nome, provincia }]
	const [query, setQuery] = useState('')
	const [suggestions, setSuggestions] = useState([])
	const [errors, setErrors] = useState({})
	const [apiError, setApiError] = useState('')
	const [confirmOpen, setConfirmOpen] = useState(false)
	const [saving, setSaving] = useState(false)

	useFocusEffect(
		useCallback(() => {
			const load = async () => {
				try {
					const { data } = await api.get('/association/profile')
					const hq = data.headquarter
					setForm({
						name: data.name ?? '',
						email: data.email ?? '',
						phone: data.phones?.[0] ?? '',
						hours: data.availabilityHours ?? '',
						street: hq?.street ?? '',
						number: hq?.number ?? '',
						city: hq?.city ?? '',
						cap: hq?.postalCode ?? '',
						province: hq?.province ?? '',
					})
					setOtherPhones(data.phones?.slice(1) ?? [])
					setComuni(data.coveredComuni ?? [])
					setErrors({})
					setApiError('')
				} catch (e) {
					setApiError(apiErrorMessage(e, 'Impossibile caricare il profilo.'))
				} finally {
					setLoading(false)
				}
			}
			load()
		}, []),
	)

	// Suggerimenti dei comuni (anagrafica ISTAT del backend) mentre si scrive
	useEffect(() => {
		const q = query.trim()
		if (q.length < 2) {
			setSuggestions([])
			return
		}
		const timer = setTimeout(async () => {
			try {
				const { data } = await api.get('/comuni/search', { params: { q } })
				setSuggestions(data.filter((s) => !comuni.some((c) => c.codiceIstat === s.codiceIstat)).slice(0, 6))
			} catch (e) {
				setSuggestions([])
			}
		}, 250)
		return () => clearTimeout(timer)
	}, [query, comuni])

	const set = (key) => (value) => setForm((f) => ({ ...f, [key]: value }))

	const addComune = (s) => {
		if (!s) {
			setErrors((e) => ({ ...e, comuni: 'Scegli il comune dall’elenco, per esempio Perugia.' }))
			return
		}
		setComuni((list) => [...list, { codiceIstat: s.codiceIstat, nome: s.nomeComune, provincia: s.provincia }])
		setQuery('')
		setSuggestions([])
		setErrors((e) => ({ ...e, comuni: undefined }))
	}

	const validate = () => {
		const found = {}
		if (!form.name.trim()) found.name = 'Inserisci la denominazione.'
		if (!form.phone.trim()) found.phone = 'Inserisci il telefono.'
		if (!form.street.trim()) found.street = 'Inserisci la via.'
		if (!form.number.trim()) found.number = 'Inserisci il civico.'
		if (!form.city.trim()) found.city = 'Inserisci il comune.'
		if (!/^\d{5}$/.test(form.cap.trim())) found.cap = 'Il CAP deve essere di 5 cifre.'
		if (!/^[A-Za-z]{2}$/.test(form.province.trim())) found.province = 'Inserisci una provincia della sede valida.'
		if (!comuni.length) found.comuni = 'Aggiungi almeno un comune servito.'
		setErrors(found)
		return Object.keys(found).length === 0
	}

	const save = async () => {
		setSaving(true)
		setApiError('')
		const headquarter = {
			street: form.street.trim(),
			number: form.number.trim(),
			postalCode: form.cap.trim(),
			city: form.city.trim(),
			province: form.province.trim().toUpperCase(),
		}
		const phones = [form.phone.trim(), ...otherPhones]
		const coveredComuni = comuni.map((c) => c.codiceIstat)
		try {
			if (isRegister) {
				await api.patch('/association/profile', { name: form.name.trim(), headquarter, phones, coveredComuni })
				// Gli orari di reperibilità non fanno parte del completamento: si salvano subito dopo
				if (form.hours.trim())
					await api.put('/association/profile', {
						name: form.name.trim(),
						headquarter,
						phones,
						coveredComuni,
						availabilityHours: form.hours.trim(),
					})
			} else {
				await api.put('/association/profile', {
					name: form.name.trim(),
					headquarter,
					phones,
					coveredComuni,
					availabilityHours: form.hours.trim() || null,
				})
			}
			setConfirmOpen(false)
			router.replace({ pathname: '/association/profile', params: { saved: '1' } })
		} catch (e) {
			setConfirmOpen(false)
			setApiError(apiErrorMessage(e, 'Salvataggio non riuscito.'))
		} finally {
			setSaving(false)
		}
	}

	if (loading) return <Loading />

	return (
		<Page>
			<Scroll>
				<PageTitle>{isRegister ? 'Completa il profilo' : 'Modifica profilo'}</PageTitle>

				<Text style={styles.section}>Informazioni dell’associazione</Text>
				<Field label="Denominazione" value={form.name} onChangeText={set('name')} error={errors.name} />
				<Field label="Email" value={form.email} editable={false} />
				<Field label="Telefono" value={form.phone} onChangeText={set('phone')} keyboardType="phone-pad" error={errors.phone} />
				<Field label="Orari di reperibilità" value={form.hours} onChangeText={set('hours')} maxLength={500} />

				<Text style={styles.section}>Sede</Text>
				<Row>
					<Field label="Via" style={styles.flex} value={form.street} onChangeText={set('street')} error={errors.street} />
					<Field label="Civico" style={styles.civic} value={form.number} onChangeText={set('number')} error={errors.number} />
				</Row>
				<Field label="Comune" value={form.city} onChangeText={set('city')} error={errors.city} />
				<Row>
					<Field label="CAP" style={styles.flex} value={form.cap} onChangeText={set('cap')} keyboardType="number-pad" maxLength={5} error={errors.cap} />
					<Field
						label="Provincia"
						style={styles.flex}
						value={form.province}
						onChangeText={(v) => set('province')(v.toUpperCase())}
						autoCapitalize="characters"
						maxLength={2}
						error={errors.province}
					/>
				</Row>

				<Text style={styles.section}>Comuni serviti</Text>
				<View style={styles.pills}>
					{comuni.length ? (
						comuni.map((c) => (
							<View key={c.codiceIstat} style={styles.pill}>
								<Text style={styles.pillText}>{c.nome}</Text>
								<Pressable
									onPress={() => setComuni((list) => list.filter((x) => x.codiceIstat !== c.codiceIstat))}
									accessibilityLabel={`Rimuovi comune ${c.nome}`}
									hitSlop={8}
								>
									<Text style={styles.pillX}>×</Text>
								</Pressable>
							</View>
						))
					) : (
						<Muted style={styles.small}>Aggiungi almeno un comune.</Muted>
					)}
				</View>
				<View style={styles.addRow}>
					<Input
						value={query}
						onChangeText={setQuery}
						placeholder="Es. Perugia"
						style={styles.addInput}
						accessibilityLabel="Comune da aggiungere"
						maxLength={100}
					/>
					<Btn label="Aggiungi comune" icon="add" onPress={() => addComune(suggestions[0])} style={styles.addBtn} textStyle={styles.addText} />
				</View>
				{suggestions.length ? (
					<View style={styles.suggestions}>
						{suggestions.map((s) => (
							<Pressable key={s.codiceIstat} onPress={() => addComune(s)} style={styles.suggestion}>
								<Icon name="location-outline" size={15} color={C.primary} />
								<Text style={styles.suggestionText}>
									{s.nomeComune} ({s.provincia})
								</Text>
							</Pressable>
						))}
					</View>
				) : null}
				<ErrorText>{errors.comuni}</ErrorText>
				<Muted style={styles.hint}>Indica i comuni in cui la tua associazione offre il servizio.</Muted>
				<ErrorText>{apiError}</ErrorText>
			</Scroll>
			<Footer>
				<Btn
					label="Annulla"
					variant="secondary"
					onPress={() => (isRegister ? router.replace('/logout') : router.replace('/association/profile'))}
					style={styles.flex}
				/>
				<Btn
					label={isRegister ? 'Invia registrazione' : 'Salva modifiche'}
					onPress={() => validate() && setConfirmOpen(true)}
					style={styles.flex}
				/>
			</Footer>

			<Dialog visible={confirmOpen} onClose={() => setConfirmOpen(false)}>
				<Text style={styles.dialogTitle}>{isRegister ? 'Invia registrazione' : 'Salva modifiche'}</Text>
				<Text style={styles.dialogText}>
					{isRegister
						? 'Invierai i dati della tua associazione per l’accreditamento.'
						: 'Vuoi salvare i dati aggiornati dell’associazione?'}
				</Text>
				<View style={styles.dialogActions}>
					<Btn label="Annulla" variant="secondary" onPress={() => setConfirmOpen(false)} style={styles.flex} />
					<Btn label={isRegister ? 'Invia' : 'Salva'} onPress={save} loading={saving} style={styles.flex} />
				</View>
			</Dialog>
		</Page>
	)
}

export default EditAssociationProfile

const styles = StyleSheet.create({
	flex: { flex: 1 },
	civic: { width: 78 },
	section: { fontSize: 14, fontWeight: '600', color: C.muted, marginBottom: 12, marginTop: 4 },
	pills: { flexDirection: 'row', flexWrap: 'wrap', gap: 8, marginBottom: 12 },
	pill: { flexDirection: 'row', alignItems: 'center', gap: 8, backgroundColor: C.card, borderRadius: 8, paddingHorizontal: 12, paddingVertical: 7 },
	pillText: { fontSize: 13, color: C.muted },
	pillX: { fontSize: 16, color: C.muted },
	small: { fontSize: 13 },
	addRow: { flexDirection: 'row', gap: 10, alignItems: 'center' },
	addInput: { flex: 1, minWidth: 0 },
	addBtn: { paddingHorizontal: 12, minHeight: 48, flexShrink: 0 },
	addText: { fontSize: 13 },
	suggestions: { backgroundColor: C.card, borderRadius: 10, marginTop: 8, overflow: 'hidden' },
	suggestion: { flexDirection: 'row', alignItems: 'center', gap: 8, paddingHorizontal: 14, paddingVertical: 11, borderBottomWidth: 1, borderBottomColor: C.page },
	suggestionText: { fontSize: 14, color: C.text },
	hint: { fontSize: 13, marginTop: 8 },
	dialogTitle: { fontSize: 20, fontWeight: '600', color: C.text, marginBottom: 12 },
	dialogText: { fontSize: 15, lineHeight: 23, color: C.muted },
	dialogActions: { flexDirection: 'row', gap: 12, marginTop: 20 },
})
