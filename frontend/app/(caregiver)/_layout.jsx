import { useEffect, useState } from 'react'
import { Stack, useRouter, usePathname } from 'expo-router'
import { View, Text, Pressable, StyleSheet } from 'react-native'
import { useSafeAreaInsets } from 'react-native-safe-area-context'
import { Ionicons } from '@expo/vector-icons'
import Header from '../../components/Header'
import api from '../../config/httpClient.js'
import { getItem } from '../../config/storage.js'
import { C, initialsFor } from '../../constants/caregiver'

const SECTIONS = [
    { path: '/transport', label: 'Trasporti', icon: 'car-outline' },
    { path: '/transport/new', label: 'Nuovo trasporto', icon: 'add-circle-outline', create: true },
    { path: '/assisted', label: 'Assistiti', icon: 'people-outline' },
]

export default function DashboardLayout() {
    const router = useRouter()
    const pathname = usePathname()
    const insets = useSafeAreaInsets()
    const [initials, setInitials] = useState('')
    const [hasUnreadNotifications, setHasUnreadNotifications] = useState(false)

    useEffect(() => {
        let active = true
        const load = async () => {
            if (!(await getItem('accessToken'))) return
            try {
                const { data } = await api.get('/me/profile')
                // Compatibilità con gli account creati prima del salvataggio dei nomi sul server.
                let pending = null
                try { pending = JSON.parse((await getItem('pendingProfile')) ?? 'null') } catch {}
                const local = pending?.email === data.email?.toLowerCase() ? pending : null
                if (active) setInitials(initialsFor(data.name || local?.name || '', data.surname || local?.surname || ''))
            } catch {}
            try {
                const { data } = await api.get('/notifications/counters')
                if (active) setHasUnreadNotifications(data.unread > 0)
            } catch {}
        }
        load()
        return () => { active = false }
    }, [pathname])

    return (
        <View style={styles.container}>
            <Header initials={initials} hasUnreadNotifications={hasUnreadNotifications}
                onPressAvatar={() => router.navigate('/(caregiver)/profile')}
                onPressNotifications={() => router.navigate('/(caregiver)/notifications')} />
            <View style={styles.content}>
                <Stack screenOptions={{ headerShown: false, contentStyle: { backgroundColor: C.page }, animation: 'none' }} />
            </View>
            <View style={[styles.nav, { paddingBottom: Math.max(insets.bottom, 16) }]} accessibilityRole="tablist">
                {SECTIONS.map((section) => {
                    const selected = section.create ? pathname === section.path
                        : section.path === '/transport' ? pathname.startsWith('/transport') && pathname !== '/transport/new'
                        : pathname.startsWith('/assisted')
                    return (
                        <Pressable key={section.path} accessibilityRole="tab" accessibilityState={{ selected }}
                            onPress={() => router.navigate(`/(caregiver)${section.path}`)} style={styles.tab}>
                            <View style={section.create ? styles.newIcon : styles.icon}>
                                <Ionicons size={section.create ? 24 : 22} name={section.icon}
                                    color={section.create ? C.white : selected ? C.primary : C.muted} />
                            </View>
                            <Text style={[styles.label, (selected || section.create) && styles.labelSelected]}>{section.label}</Text>
                        </Pressable>
                    )
                })}
            </View>
        </View>
    )
}

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: C.page },
    content: { flex: 1, minHeight: 0 },
    nav: { flexDirection: 'row', backgroundColor: C.nav, borderTopWidth: 1, borderTopColor: C.card, paddingTop: 8 },
    tab: { flex: 1, alignItems: 'center', justifyContent: 'center', minHeight: 62 },
    icon: { width: 36, height: 36, alignItems: 'center', justifyContent: 'center' },
    newIcon: { width: 36, height: 36, borderRadius: 18, backgroundColor: C.primary, alignItems: 'center', justifyContent: 'center' },
    label: { fontSize: 12, color: C.muted, marginTop: 4 },
    labelSelected: { color: C.primary, fontWeight: '600' },
})
