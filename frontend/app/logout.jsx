import { StyleSheet, Text, View } from 'react-native'
import { Link } from 'expo-router'
import { useEffect, useState } from 'react'
import { ActivityIndicator } from 'react-native'
import api from '../config/httpClient.js'
import { getItem, deleteItem } from '../config/storage.js'


import ThemedView from '../components/ThemedView'
import ThemedText from '../components/ThemedText'
import ThemedCard from '../components/ThemedCard'
import ThemedButton from '../components/ThemedButton'
import { Colors } from '../constants/Colors'
import { router } from 'expo-router'

const Logout = () => {

  const [done, setDone] = useState(false)

  useEffect(() => {
    const logout = async () => {
        try {
            // revoca la sessione sul backend
            const refreshToken = await getItem('refreshToken')
            if (refreshToken) await api.post('/auth/logout', { refreshToken })
        } catch (error) {
            console.log('LOGOUT ERROR:', error.response?.status)
        } finally {
            // i token locali si cancellano sempre, anche se il server non risponde
            await deleteItem('accessToken')
            await deleteItem('refreshToken')
            setDone(true)
        }
    }
    logout()
  }, [])

  // finché il logout non è concluso mostriamo solo la rotellina
  if (!done) {
    return (
        <ThemedView style={styles.container}>
            <ActivityIndicator size="large" />
        </ThemedView>
    )
  }

  return (
    <ThemedView style={styles.container}>
      <ThemedCard style={styles.card}>
        <View style={styles.statusIcon}>
          <Text style={styles.statusIconText}>✓</Text>
        </View>

        <ThemedText title style={styles.title}>
          Logout effettuato
        </ThemedText>

        <ThemedText style={styles.message}>
          Hai effettuato il logout dal tuo account.
          {'\n'}A presto!
        </ThemedText>

        <Link href="/login" asChild>
          <ThemedButton style={styles.loginButton}>
            <Text style={styles.loginButtonText}>Torna al login</Text>
          </ThemedButton>
        </Link>
      </ThemedCard>
    </ThemedView>
  )
}

export default Logout

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 24,
  },
  card: {
    width: '100%',
    maxWidth: 380,
    alignItems: 'center',
    paddingHorizontal: 28,
    paddingVertical: 32,
    borderRadius: 16,
  },
  statusIcon: {
    width: 64,
    height: 64,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: Colors.warning,
    borderRadius: 32,
    marginBottom: 20,
  },
  statusIconText: {
    color: '#fff',
    fontSize: 34,
    fontWeight: 'bold',
  },
  title: {
    fontSize: 22,
    fontWeight: 'bold',
    marginBottom: 12,
    textAlign: 'center',
  },
  message: {
    lineHeight: 22,
    marginBottom: 24,
    textAlign: 'center',
  },
  loginButton: {
    minWidth: 170,
    alignItems: 'center',
  },
  loginButtonText: {
    color: '#f2f2f2',
    fontWeight: '600',
  },
})