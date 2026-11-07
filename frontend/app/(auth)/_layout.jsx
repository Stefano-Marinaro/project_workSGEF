import { Stack } from "expo-router"
import { StatusBar } from "react-native"

export default function AuthLayout(){
    return (
        <>
            <StatusBar style="auto" />
            <Stack 
                screenOptions={{ animation: "none" }}
            >
                {/* Login e registrazione hanno il proprio header con il logo (mockup) */}
                <Stack.Screen name="login" options={{ headerShown: false }} />
                <Stack.Screen name="register" options={{ headerShown: false }} />
            </Stack>
        </>
    )
}
