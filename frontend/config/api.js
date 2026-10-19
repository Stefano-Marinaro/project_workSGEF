import Constants from 'expo-constants'
import { Platform } from 'react-native'

const BACKEND_PORT = 5161

// Deriva l'host dal server di sviluppo Metro (lo stesso IP a cui il telefono
// si è già collegato per scaricare il bundle JS): niente IP scritto a mano,
// funziona per chiunque avvii "expo start" sulla propria rete.
function resolveApiBaseUrl() {
  // Sul web la pagina può essere aperta da qualunque dispositivo (es. il link
  // di verifica email apribile dal telefono): usiamo l'host con cui la pagina
  // stessa è stata caricata, non un "localhost" che varrebbe solo sul PC di sviluppo.
  if (Platform.OS === 'web') {
    const host = typeof window !== 'undefined' ? window.location.hostname : 'localhost'
    return `http://${host}:${BACKEND_PORT}`
  }

  const debuggerHost = Constants.expoConfig?.hostUri?.split(':')[0]
  if (debuggerHost) return `http://${debuggerHost}:${BACKEND_PORT}`

  return `http://localhost:${BACKEND_PORT}` // fallback (es. build di produzione)
}

export const API_BASE_URL = resolveApiBaseUrl()
