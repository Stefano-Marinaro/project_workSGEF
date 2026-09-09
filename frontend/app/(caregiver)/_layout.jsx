import { Tabs } from "expo-router"
import { useColorScheme } from "react-native"
import {Colors } from "../../constants/Colors"
import { Ionicons } from "@expo/vector-icons"

const DashboardLayout = () => {
    const colorScheme = useColorScheme()
    const theme = Colors[colorScheme] ?? Colors.light

    return (
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
            <Tabs.Screen 
                name="profile" 
                options={{title:"Profile", tabBarIcon: ({ focused }) => (
                    <Ionicons 
                        size={24} 
                        name={ focused ? 'person' : 'person-outline'} 
                        color={ focused ?  theme.iconColorFocused :  theme.iconColor }
                    />
                )}}
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
                        name={ focused ? 'create' : 'create-outline'} 
                        color={ focused ?  theme.iconColorFocused :  theme.iconColor }
                    />
                )}}
            />
            <Tabs.Screen 
                name="group" 
                options={{title:"Groups", tabBarIcon: ({ focused }) => (
                    <Ionicons 
                        size={24} 
                        name={ focused ? 'people' : 'people-outline'} 
                        color={ focused ?  theme.iconColorFocused :  theme.iconColor }
                    />
                )}}
            />
        </Tabs>
    )
}

export default DashboardLayout
