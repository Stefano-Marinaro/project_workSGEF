import { useEffect, useState } from "react"
import { Tabs, useRouter } from "expo-router"
import { useColorScheme } from "react-native"
import {Colors } from "../../constants/Colors"
import { Ionicons } from "@expo/vector-icons"
import Header from "../../components/Header"
import api from "../../config/httpClient.js"

const DashboardLayout = () => {
    const colorScheme = useColorScheme()
    const theme = Colors[colorScheme] ?? Colors.light
    const router = useRouter()

    const [name, setName] = useState("")
    const [hasUnreadNotifications, setHasUnreadNotifications] = useState(false)

    useEffect(() => {
        const loadHeaderData = async () => {
            try {
                const profileResponse = await api.get("/me/profile")
                setName(`${profileResponse.data.name} ${profileResponse.data.surname}`)
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
    }, [])

    return (
        <>
        <Header
            name={name}
            hasUnreadNotifications={hasUnreadNotifications}
            onPressAvatar={() => router.push('/profile')}
            onPressNotifications={() => router.push('/notifications')}
        />
        <Tabs
            screenOptions={{ headerShown: false, tabBarStyle: {
                backgroundColor: theme.navBackground,
                paddingTop: 10,
                height: 90
            },
            tabBarActiveTintColor: theme.iconColorFocused,
            tabBarInactiveTintColor: theme.iconColor
        }}

        >
            {/* Si raggiunge dall'avatar nell'Header, non più come tab: href:null la tiene navigabile senza icona in basso */}
            <Tabs.Screen
                name="profile"
                options={{
                    href: null,
                    title: "Profile"
                }}
            />
            {/* Questa Tabs dice editProfile esiste ed è raggiungibile tramite router.push, ma non mostrarla nella tab bar */}
            <Tabs.Screen 
                name="editProfile" 
                options={{
                    href: null,
                    title: "Edit Profile"
                }}
            />

            <Tabs.Screen
                name="editPassword"
                options={{
                    href: null,
                    title: "Edit Password"
                }}
            />
            {/* Raggiunta dal campanello nell'Header, non è una tab */}
            <Tabs.Screen
                name="notifications"
                options={{
                    href: null,
                    title: "Notifications"
                }}
            />

            <Tabs.Screen
                name="transport"
                options={{title:"My Transports", tabBarIcon: ({ focused }) => (
                    <Ionicons 
                        size={24} 
                        name={ focused ? 'car' : 'car-outline'} 
                        color={ focused ?  theme.iconColorFocused :  theme.iconColor }
                    />
                )}}
            />
            <Tabs.Screen
                name="transport/new"
                options={{title:"New Transport", tabBarIcon: ({ focused }) => (
                    <Ionicons
                        size={24}
                        name={ focused ? 'add-circle' : 'add-circle-outline'}
                        color={ focused ?  theme.iconColorFocused :  theme.iconColor }
                    />
                )}}
            />
            {/* TODO: assisted.jsx è ancora un placeholder - va sostituito con la vera lista assistiti (mockup 3.1) */}
            <Tabs.Screen
                name="assisted"
                options={{title:"Assistiti", tabBarIcon: ({ focused }) => (
                    <Ionicons
                        size={24}
                        name={ focused ? 'people' : 'people-outline'}
                        color={ focused ?  theme.iconColorFocused :  theme.iconColor }
                    />
                )}}
            />
            {/* Dettaglio raggiunto dalla lista assistiti, non è una tab */}
            <Tabs.Screen
                name="assisted/[id]"
                options={{
                    href: null,
                    title: "Assisted Detail"
                }}
            />
            {/* Dettaglio raggiunto dalla lista trasporti, non è una tab */}
            <Tabs.Screen
                name="transport/[id]"
                options={{
                    href: null,
                    title: "Transport Detail"
                }}
            />
        </Tabs>
        </>
    )
}

export default DashboardLayout
