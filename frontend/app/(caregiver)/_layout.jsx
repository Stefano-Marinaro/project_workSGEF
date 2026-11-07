import { useEffect, useState } from "react"
import { Tabs, useRouter, usePathname } from "expo-router"
import { View, Text, StyleSheet } from "react-native"
import { Ionicons } from "@expo/vector-icons"
import Header from "../../components/Header"
import api from "../../config/httpClient.js"
import { getItem } from "../../config/storage.js"
import { C, initialsFor } from "../../constants/caregiver"

// Schermate raggiungibili con router.push ma non mostrate nella tab bar
const HIDDEN = [
    "profile",
    "editProfile",
    "editPassword",
    "notifications",
    "transport/[id]",
    "transport/history",
    "assisted/[id]",
    "assisted/new",
    "invito-caregiver/[token]",
]

const TabLabel = ({ focused, label }) => (
    <Text style={[styles.label, focused && styles.labelFocused]}>{label}</Text>
)

const DashboardLayout = () => {
    const router = useRouter()
    const pathname = usePathname()

    const [initials, setInitials] = useState("")
    const [hasUnreadNotifications, setHasUnreadNotifications] = useState(false)

    // Ricarica iniziali e contatore notifiche a ogni cambio di schermata,
    // così il pallino si aggiorna dopo aver letto le notifiche o modificato il profilo.
    useEffect(() => {
        const loadHeaderData = async () => {
            // Senza sessione (es. link di invito aperto da chi non ha fatto l'accesso)
            // non si chiama il backend: ci pensa la schermata a mandare al login.
            if (!(await getItem("accessToken"))) return
            try {
                const profileResponse = await api.get("/me/profile")
                setInitials(initialsFor(profileResponse.data.name ?? "", profileResponse.data.surname ?? ""))
            } catch (error) {
                // L'Header non deve bloccare l'app se questa chiamata fallisce
            }

            try {
                const countersResponse = await api.get("/notifications/counters")
                setHasUnreadNotifications(countersResponse.data.unread > 0)
            } catch (error) {
                // Idem
            }
        }
        loadHeaderData()
    }, [pathname])

    return (
        <>
        <Header
            initials={initials}
            hasUnreadNotifications={hasUnreadNotifications}
            onPressAvatar={() => router.push('/profile')}
            onPressNotifications={() => router.push('/notifications')}
        />
        <Tabs
            backBehavior="history"
            screenOptions={{
                headerShown: false,
                sceneStyle: { backgroundColor: C.page },
                tabBarStyle: {
                    backgroundColor: C.nav,
                    borderTopColor: C.card,
                    paddingTop: 8,
                    height: 94,
                },
                tabBarActiveTintColor: C.text,
                tabBarInactiveTintColor: "#686477",
            }}
        >
            <Tabs.Screen
                name="transport"
                options={{
                    title: "Trasporti",
                    tabBarLabel: ({ focused }) => <TabLabel focused={focused} label="Trasporti" />,
                    tabBarIcon: ({ color }) => <Ionicons size={22} name="car-outline" color={color} />,
                }}
            />
            <Tabs.Screen
                name="transport/new"
                options={{
                    title: "Nuovo trasporto",
                    tabBarLabel: () => <Text style={styles.newLabel}>Nuovo trasporto</Text>,
                    tabBarIcon: () => (
                        <View style={styles.newIcon}>
                            <Ionicons size={24} name="add-circle-outline" color={C.white} />
                        </View>
                    ),
                }}
            />
            <Tabs.Screen
                name="assisted"
                options={{
                    title: "Assistiti",
                    tabBarLabel: ({ focused }) => <TabLabel focused={focused} label="Assistiti" />,
                    tabBarIcon: ({ color }) => <Ionicons size={22} name="people-outline" color={color} />,
                }}
            />
            {HIDDEN.map((name) => (
                <Tabs.Screen key={name} name={name} options={{ href: null }} />
            ))}
        </Tabs>
        </>
    )
}

export default DashboardLayout

const styles = StyleSheet.create({
    label: { fontSize: 12, color: "#686477", marginTop: 4 },
    labelFocused: { color: C.text, fontWeight: "700" },
    newIcon: {
        width: 36,
        height: 36,
        borderRadius: 18,
        backgroundColor: C.primary,
        alignItems: "center",
        justifyContent: "center",
    },
    newLabel: { fontSize: 13, fontWeight: "600", color: C.primary, marginTop: 6 },
})
