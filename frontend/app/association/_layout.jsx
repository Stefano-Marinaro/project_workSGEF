import { Tabs } from 'expo-router'
import { Ionicons } from '@expo/vector-icons'
import { useColorScheme } from 'react-native'
import { Colors } from '../../constants/Colors'

const AssociationLayout = () => {
	const colorScheme = useColorScheme()
	const theme = Colors[colorScheme] ?? Colors.light

	return (
		<Tabs screenOptions={{
			headerShown: false,
			tabBarStyle: { backgroundColor: theme.navBackground, paddingTop: 10, height: 90 },
			tabBarActiveTintColor: theme.iconColorFocused,
			tabBarInactiveTintColor: theme.iconColor,
		}}>
			<Tabs.Screen
				name="request"
				options={{ title: 'Richieste', tabBarIcon: ({ focused }) => (
					<Ionicons size={24} name={focused ? 'mail' : 'mail-outline'} color={focused ? theme.iconColorFocused : theme.iconColor} />
				) }}
			/>
			<Tabs.Screen
				name="trasport"
				options={{ title: 'Trasporti', tabBarIcon: ({ focused }) => (
					<Ionicons size={24} name={focused ? 'car' : 'car-outline'} color={focused ? theme.iconColorFocused : theme.iconColor} />
				) }}
			/>
			<Tabs.Screen
				name="profile"
				options={{ title: 'Profilo', tabBarIcon: ({ focused }) => (
					<Ionicons size={24} name={focused ? 'person' : 'person-outline'} color={focused ? theme.iconColorFocused : theme.iconColor} />
				) }}
			/>
		</Tabs>
	)
}

export default AssociationLayout
