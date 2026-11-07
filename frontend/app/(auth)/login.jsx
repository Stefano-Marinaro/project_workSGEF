import { useEffect, useRef, useState } from 'react'
import { View, ScrollView, Animated, Easing, StyleSheet, AccessibilityInfo, KeyboardAvoidingView, Platform, Text, Pressable } from 'react-native'
import { useLocalSearchParams, useRouter } from 'expo-router'
import { setItem } from '../../config/storage.js'
import api from '../../config/httpClient.js'
import { C } from '../../constants/caregiver'
import { AuthHeader, AuthIntro, AuthField, AuthButton, AuthSwitch, authStyles } from '../../components/auth/AuthUi'

// L'animazione di apertura (logo che dal centro si sposta nell'header) parte solo
// alla prima apertura del login in questa sessione dell'app
let splashPlayed = false

const SPLASH_SIZE = 200

// Il ruolo è nel claim "role" dell'access token (TokenService): non serve chiederlo all'utente
const roleFromToken = (token) => {
    try {
        const part = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
        const padded = part + '='.repeat((4 - (part.length % 4)) % 4)
        const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'
        let bits = 0
        let value = 0
        let json = ''
        for (const ch of padded.replace(/=+$/, '')) {
            value = (value << 6) | chars.indexOf(ch)
            bits += 6
            if (bits >= 8) {
                bits -= 8
                json += String.fromCharCode((value >> bits) & 0xff)
            }
        }
        // i byte UTF-8 restano validi come caratteri JSON: a noi serve solo "role" (ASCII)
        return JSON.parse(json).role
    } catch (e) {
        return null
    }
}

const Login = () => {
    const params = useLocalSearchParams()
    const router = useRouter()

    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')
    const [errors, setErrors] = useState({})
    const [errorMessage, setErrorMessage] = useState('')
    const [loading, setLoading] = useState(false)

    // ---- animazione di apertura ----
    const [splash, setSplash] = useState(!splashPlayed)
    const progress = useRef(new Animated.Value(0)).current
    const [flight, setFlight] = useState(null) // { dx, dy, scale }
    const headerLogo = useRef(null)
    const splashLogo = useRef(null)
    const flying = useRef(false)

    useEffect(() => {
        if (!splash) return
        splashPlayed = true
        let cancelled = false
        AccessibilityInfo.isReduceMotionEnabled()
            .then((reduce) => {
                if (reduce && !cancelled) setSplash(false)
            })
            .catch(() => {})
        // Rete di sicurezza: se le misure non arrivano, la schermata compare comunque
        const timer = setTimeout(() => !cancelled && setSplash(false), 2500)
        return () => {
            cancelled = true
            clearTimeout(timer)
        }
    }, [])

    const startFlight = () => {
        if (!splash || flying.current || !headerLogo.current || !splashLogo.current) return
        flying.current = true
        headerLogo.current.measureInWindow((hx, hy, hw, hh) => {
            splashLogo.current.measureInWindow((sx, sy, sw, sh) => {
                if (!hw || !sw) {
                    flying.current = false
                    return
                }
                const move = {
                    dx: hx + hw / 2 - (sx + sw / 2),
                    dy: hy + hh / 2 - (sy + sh / 2),
                    scale: hw / sw,
                }
                setFlight(move)
                Animated.timing(progress, {
                    toValue: 1,
                    duration: 1000,
                    easing: Easing.bezier(0.4, 0, 0.2, 1),
                    useNativeDriver: Platform.OS !== 'web',
                }).start(() => setSplash(false))
            })
        })
    }

    const contentOpacity = splash
        ? progress.interpolate({ inputRange: [0, 0.58, 1], outputRange: [0, 0, 1] })
        : 1

    // ---- accesso ----
    const handleSubmit = async () => {
        const found = {}
        if (!email.trim()) found.email = 'Inserisci la tua email.'
        if (!password) found.password = 'Inserisci la password.'
        setErrors(found)
        setErrorMessage('')
        if (Object.keys(found).length) return

        setLoading(true)
        try {
            const response = await api.post('/auth/login', { email: email.trim(), password })
            await setItem('accessToken', response.data.accessToken)
            await setItem('refreshToken', response.data.refreshToken)
            setPassword('')

            const role = roleFromToken(response.data.accessToken)
            // redirect: pagina da riaprire dopo l'accesso (es. link di invito caregiver)
            if (params.redirect) router.replace(params.redirect)
            else if (role === 'Association') router.replace('/association/request')
            else if (role === 'Person') router.replace('/(caregiver)/transport')
            else setErrorMessage('Questo account non può accedere all’app.')
        } catch (error) {
            setErrorMessage(
                error.response ? error.response.data?.detail || 'Accesso non riuscito.' : 'Impossibile raggiungere il server.',
            )
        } finally {
            setLoading(false)
        }
    }

    return (
        <View style={authStyles.page}>
            <KeyboardAvoidingView style={styles.flex} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
                <AuthHeader ref={headerLogo} hideLogo={splash} onLogoLayout={startFlight} />
                <Animated.View style={[styles.flex, { opacity: contentOpacity }]}>
                    <ScrollView contentContainerStyle={[authStyles.scroll, styles.center]} keyboardShouldPersistTaps="handled">
                        <View style={authStyles.section}>
                            <AuthIntro title="Bentornato in GoCare">
                                Accedi per organizzare i tuoi trasporti{'\n'}e seguire le tue richieste.
                            </AuthIntro>

                            <AuthField
                                icon="mail-outline"
                                placeholder="Email"
                                keyboardType="email-address"
                                autoCapitalize="none"
                                autoComplete="email"
                                value={email}
                                onChangeText={setEmail}
                                error={errors.email}
                            />
                            <AuthField
                                icon="lock-closed-outline"
                                placeholder="Password"
                                secure
                                autoComplete="current-password"
                                value={password}
                                onChangeText={setPassword}
                                onSubmitEditing={handleSubmit}
                                error={errors.password}
                                style={styles.passwordField}
                            />

                            <Pressable onPress={() => router.push('/forgot_password')} style={styles.forgot} hitSlop={8}>
                                <Text style={styles.forgotText}>Password dimenticata?</Text>
                            </Pressable>

                            <AuthButton label="Accedi" onPress={handleSubmit} loading={loading} />
                            {errorMessage ? <Text style={authStyles.formError}>{errorMessage}</Text> : null}

                            <AuthSwitch text="Non hai un account?" action="Registrati" onPress={() => router.push('/register')} />
                        </View>
                    </ScrollView>
                    <Text style={styles.footer}>I tuoi trasporti, in un unico posto.</Text>
                </Animated.View>
            </KeyboardAvoidingView>

            {splash ? (
                <View style={styles.splash} pointerEvents="none">
                    <Animated.Image
                        ref={splashLogo}
                        onLayout={startFlight}
                        source={require('../../assets/img/gocareLogo.png')}
                        style={[
                            styles.splashLogo,
                            flight && {
                                transform: [
                                    { translateX: progress.interpolate({ inputRange: [0, 1], outputRange: [0, flight.dx] }) },
                                    { translateY: progress.interpolate({ inputRange: [0, 1], outputRange: [0, flight.dy] }) },
                                    { scale: progress.interpolate({ inputRange: [0, 1], outputRange: [1, flight.scale] }) },
                                ],
                            },
                        ]}
                    />
                </View>
            ) : null}
        </View>
    )
}

export default Login

const styles = StyleSheet.create({
    flex: { flex: 1 },
    center: { justifyContent: 'center' },
    passwordField: { marginBottom: 8 },
    forgot: { alignSelf: 'flex-end', paddingVertical: 8, marginBottom: 24 },
    forgotText: { fontSize: 14, color: C.primary },
    footer: { textAlign: 'center', fontSize: 13, color: C.muted, paddingHorizontal: 24, paddingTop: 16, paddingBottom: 24 },
    splash: { ...StyleSheet.absoluteFillObject, alignItems: 'center', justifyContent: 'center' },
    splashLogo: { width: SPLASH_SIZE, height: SPLASH_SIZE, resizeMode: 'contain' },
})
