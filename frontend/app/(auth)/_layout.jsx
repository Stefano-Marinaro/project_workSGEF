import { Stack } from "expo-router"
import { StatusBar } from "react-native"

export default function AuthLayout(){
    return (
        <>
            <StatusBar style="auto" />
            <Stack 
                screenOptions={{ animation: "none" }}
            >
                {/* Login, registrazione e recupero password hanno il proprio header con il logo (mockup) */}
                <Stack.Screen name="login" options={{ headerShown: false }} />
                <Stack.Screen name="register" options={{ headerShown: false }} />
                <Stack.Screen name="forgot_password" options={{ headerShown: false }} />
                <Stack.Screen name="reset_password" options={{ headerShown: false }} />
                <Stack.Screen name="verify_email" options={{ headerShown: false }} />
            </Stack>
        </>
    )
}
