import { useEffect, useState } from 'react'
import { Tabs, useRouter, usePathname } from 'expo-router'
import { View, Text, StyleSheet } from 'react-native'
import { Ionicons } from '@expo/vector-icons'
import Header from '../../components/Header'
import api from '../../config/httpClient.js'
import { getItem } from '../../config/storage.js'
import { C, initialsFor } from '../../constants/caregiver'

// Schermate raggiungibili con router.push ma non mostrate nella tab bar
const HIDDEN = ['detail/[id]', 'link/[id]', 'notifications', 'profile', 'editProfile']

// Icona della tab: quella attiva sta in un cerchio viola (mockup)
const TabIcon = ({ name, focused, badge }) => (
	<View style={[styles.icon, focused && styles.iconOn]}>
		<Ionicons size={20} name={name} color={focused ? C.white : '#686477'} />
		{badge > 0 ? (
			<View style={styles.badge}>
				<Text style={styles.badgeText}>{badge}</Text>
			</View>
		) : null}
	</View>
)

const TabLabel = ({ focused, label }) => (
	<Text style={[styles.label, focused && styles.labelOn]}>{label}</Text>
)

const AssociationLayout = () => {
	const router = useRouter()
	const pathname = usePathname()
	const [initials, setInitials] = useState('')
	const [unread, setUnread] = useState(0)
	const [pending, setPending] = useState(0)

	// Ricarica a ogni cambio di schermata: iniziali, notifiche non lette e richieste da valutare
	useEffect(() => {
		const load = async () => {
			if (!(await getItem('accessToken'))) return
			try {
				const { data } = await api.get('/association/profile')
				// Profilo non ancora completato dopo la registrazione: si va a "Completa il profilo"
				if (!data.name) {
					if (!pathname.startsWith('/association/editProfile'))
						router.replace({ pathname: '/association/editProfile', params: { register: '1' } })
					return
				}
				const words = data.name.split(' ').filter(Boolean)
				setInitials(initialsFor(words[0] ?? '', words[1] ?? ''))
			} catch (e) {
				// L'header non deve bloccare l'app
			}
			try {
				const { data } = await api.get('/notifications/counters')
				setUnread(data.unread)
			} catch (e) {}
			try {
				const { data } = await api.get('/association/requests')
				setPending(data.length)
			} catch (e) {}
		}
		load()
	}, [pathname])

	return (
		<>
			<Header
				initials={initials}
				badgeCount={unread}
				onPressAvatar={() => router.push('/association/profile')}
				onPressNotifications={() => router.push('/association/notifications')}
			/>
			<Tabs
				backBehavior="history"
				screenOptions={{
					headerShown: false,
					sceneStyle: { backgroundColor: C.page },
					tabBarStyle: { backgroundColor: C.nav, borderTopColor: C.card, paddingTop: 8, height: 94 },
				}}
			>
				<Tabs.Screen
					name="request"
					options={{
						title: 'Richieste',
						tabBarLabel: ({ focused }) => <TabLabel focused={focused} label="Richieste" />,
						tabBarIcon: ({ focused }) => <TabIcon name="clipboard-outline" focused={focused} badge={pending} />,
					}}
				/>
				<Tabs.Screen
					name="trasport"
					options={{
						title: 'Trasporti',
						tabBarLabel: ({ focused }) => <TabLabel focused={focused} label="Trasporti" />,
						tabBarIcon: ({ focused }) => <TabIcon name="car-outline" focused={focused} />,
					}}
				/>
				<Tabs.Screen
					name="history"
					options={{
						title: 'Storico',
						tabBarLabel: ({ focused }) => <TabLabel focused={focused} label="Storico" />,
						tabBarIcon: ({ focused }) => <TabIcon name="time-outline" focused={focused} />,
					}}
				/>
				{HIDDEN.map((name) => (
					<Tabs.Screen key={name} name={name} options={{ href: null }} />
				))}
			</Tabs>
		</>
	)
}

export default AssociationLayout

const styles = StyleSheet.create({
	icon: { width: 36, height: 36, borderRadius: 18, alignItems: 'center', justifyContent: 'center' },
	iconOn: { backgroundColor: C.primary },
	badge: {
		position: 'absolute',
		top: -4,
		right: -6,
		minWidth: 17,
		height: 17,
		borderRadius: 9,
		paddingHorizontal: 4,
		backgroundColor: C.text,
		borderWidth: 1.5,
		borderColor: C.nav,
		alignItems: 'center',
		justifyContent: 'center',
	},
	badgeText: { color: C.white, fontSize: 10, fontWeight: '700' },
	label: { fontSize: 12, color: '#686477', marginTop: 4 },
	labelOn: { color: C.primary, fontWeight: '700' },
})
