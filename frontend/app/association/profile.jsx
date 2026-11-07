import { useCallback, useState } from 'react'
import { View, Text, Pressable, StyleSheet, Linking } from 'react-native'
import { useFocusEffect, useLocalSearchParams, useRouter } from 'expo-router'

import api from '../../config/httpClient.js'
import Banner from '../../components/Banner'
import { C, apiErrorMessage } from '../../constants/caregiver'
import { ACCREDITATION } from '../../constants/association'
import { Page, Scroll, Loading, Card, Icon, Initials, Divider, Btn, ErrorText, Dialog } from '../../components/caregiver/ui'

const ContactRow = ({ icon, label, children, last }) => (
	<>
		<View style={styles.contactRow}>
			<Icon name={icon} size={19} color={C.muted} />
			<View style={styles.flex}>
				<Text style={styles.label}>{label}</Text>
				{children}
			</View>
		</View>
		{!last ? <Divider style={styles.divider} /> : null}
	</>
)

// Mockup "Profilo associazione"
const AssociationProfile = () => {
	const router = useRouter()
	const { saved } = useLocalSearchParams()
	const [loading, setLoading] = useState(true)
	const [error, setError] = useState('')
	const [profile, setProfile] = useState(null)
	const [showSaved, setShowSaved] = useState(false)
	const [dialog, setDialog] = useState(null) // 'preview' | 'logout'

	useFocusEffect(
		useCallback(() => {
			setShowSaved(saved === '1')
			const load = async () => {
				try {
					const { data } = await api.get('/association/profile')
					setProfile(data)
					setError('')
				} catch (e) {
					setError(apiErrorMessage(e, 'Impossibile caricare il profilo.'))
				} finally {
					setLoading(false)
				}
			}
			load()
		}, [saved]),
	)

	if (loading) return <Loading />

	const p = profile ?? {}
	const words = (p.name ?? '').split(' ').filter(Boolean)
	const accreditation = ACCREDITATION[p.status] ?? ACCREDITATION.Pending
	const phones = p.phones ?? []
	const hq = p.headquarter

	return (
		<Page>
			<Scroll>
				{showSaved ? (
					<Banner
						variant="success"
						message="Modifiche salvate."
						onDismiss={() => {
							setShowSaved(false)
							router.setParams({ saved: '' })
						}}
						style={styles.banner}
					/>
				) : null}

				<View style={styles.titleRow}>
					<Text style={styles.title}>Profilo associazione</Text>
					<Pressable
						onPress={() => router.push('/association/editProfile')}
						accessibilityLabel="Modifica profilo associazione"
						style={styles.editBtn}
					>
						<Icon name="create-outline" size={20} color={C.primary} />
					</Pressable>
				</View>
				<ErrorText>{error}</ErrorText>

				<View style={styles.identity}>
					<Initials name={words[0] ?? ''} surname={words[1] ?? ''} size={56} solid />
					<View style={styles.flex}>
						<Text style={styles.name}>{p.name}</Text>
						<View style={styles.accreditation}>
							<View style={[styles.dot, { backgroundColor: accreditation.color }]} />
							<Text style={[styles.accreditationText, { color: accreditation.color }]}>{accreditation.label}</Text>
						</View>
					</View>
				</View>

				<Text style={styles.section}>Contatti</Text>
				<Card style={styles.panel}>
					<ContactRow icon="call-outline" label="Telefono">
						{phones.length ? (
							phones.map((ph) => (
								<Text key={ph} style={styles.phone} onPress={() => Linking.openURL(`tel:${ph.replace(/\s/g, '')}`)}>
									{ph}
								</Text>
							))
						) : (
							<Text style={styles.value}>Non indicato</Text>
						)}
					</ContactRow>
					<ContactRow icon="mail-outline" label="Email" last={!p.availabilityHours}>
						<Text style={styles.value}>{p.email}</Text>
					</ContactRow>
					{p.availabilityHours ? (
						<ContactRow icon="time-outline" label="Reperibilità" last>
							<Text style={styles.value}>{p.availabilityHours}</Text>
						</ContactRow>
					) : null}
				</Card>

				<Text style={styles.section}>Sede</Text>
				<Card style={styles.panel}>
					{hq ? (
						<>
							<Text style={styles.value}>
								{hq.street} {hq.number}
							</Text>
							<Text style={styles.sub}>
								{hq.postalCode} {hq.city} ({hq.province})
							</Text>
						</>
					) : (
						<Text style={styles.sub}>Sede non indicata</Text>
					)}
				</Card>

				<Text style={styles.section}>Comuni serviti</Text>
				<View style={styles.pills}>
					{p.coveredComuni?.length ? (
						p.coveredComuni.map((c) => (
							<Text key={c.codiceIstat} style={styles.pill}>
								{c.nome}
							</Text>
						))
					) : (
						<Text style={styles.sub}>Indica i comuni serviti modificando il profilo.</Text>
					)}
				</View>

				<Btn label="Anteprima contatti per gli utenti" variant="secondary" onPress={() => setDialog('preview')} style={styles.btn} />
				<Btn label="Esci" variant="danger" icon="log-out-outline" onPress={() => setDialog('logout')} style={styles.btn} />
			</Scroll>

			<Dialog visible={!!dialog} onClose={() => setDialog(null)}>
				{dialog === 'preview' ? (
					<>
						<Text style={styles.dialogTitle}>Contatti visibili agli utenti</Text>
						<Card style={styles.previewCard}>
							<Text style={styles.previewName}>{p.name}</Text>
							<Text style={styles.sub}>
								{[phones.join(' · '), p.email, p.availabilityHours].filter(Boolean).join('\n')}
							</Text>
						</Card>
						<Btn label="Chiudi" onPress={() => setDialog(null)} />
					</>
				) : (
					<>
						<Text style={styles.dialogTitle}>Esci da GoCare</Text>
						<Text style={styles.dialogText}>Vuoi tornare alla pagina di accesso?</Text>
						<View style={styles.dialogActions}>
							<Btn label="Annulla" variant="secondary" onPress={() => setDialog(null)} style={styles.flex} />
							<Btn
								label="Esci"
								variant="danger"
								onPress={() => {
									setDialog(null)
									router.replace('/logout')
								}}
								style={styles.flex}
							/>
						</View>
					</>
				)}
			</Dialog>
		</Page>
	)
}

export default AssociationProfile

const styles = StyleSheet.create({
	flex: { flex: 1 },
	banner: { marginTop: 12 },
	titleRow: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', marginTop: 12, marginBottom: 14 },
	title: { fontSize: 22, fontWeight: '600', color: C.text },
	editBtn: { width: 44, height: 44, borderRadius: 22, backgroundColor: C.card, alignItems: 'center', justifyContent: 'center' },
	identity: { flexDirection: 'row', alignItems: 'center', gap: 14, marginBottom: 18 },
	name: { fontSize: 20, fontWeight: '600', color: C.text },
	accreditation: { flexDirection: 'row', alignItems: 'center', gap: 6, marginTop: 6 },
	dot: { width: 7, height: 7, borderRadius: 4 },
	accreditationText: { fontSize: 12, fontWeight: '600' },
	section: { fontSize: 14, fontWeight: '600', color: C.muted, marginBottom: 10 },
	panel: { padding: 16, marginBottom: 18 },
	contactRow: { flexDirection: 'row', gap: 14, alignItems: 'flex-start' },
	divider: { marginVertical: 14 },
	label: { fontSize: 12, color: C.muted, marginBottom: 4 },
	value: { fontSize: 16, color: C.text },
	phone: { fontSize: 16, color: C.primary },
	sub: { fontSize: 13, lineHeight: 20, color: C.muted, marginTop: 4 },
	pills: { flexDirection: 'row', flexWrap: 'wrap', gap: 8, marginBottom: 18 },
	pill: { fontSize: 13, color: C.muted, backgroundColor: C.card, borderRadius: 8, paddingHorizontal: 12, paddingVertical: 8, overflow: 'hidden' },
	btn: { marginBottom: 12 },
	dialogTitle: { fontSize: 20, fontWeight: '600', color: C.text, marginBottom: 14 },
	dialogText: { fontSize: 15, color: C.muted },
	dialogActions: { flexDirection: 'row', gap: 12, marginTop: 20 },
	previewCard: { marginBottom: 16 },
	previewName: { fontSize: 17, fontWeight: '600', color: C.text },
})
