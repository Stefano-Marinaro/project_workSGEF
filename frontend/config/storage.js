import * as SecureStore from 'expo-secure-store'
import { Platform } from 'react-native'

export const setItem = async (key, value) => {
    if (Platform.OS === 'web') {                 // ??? 1: quale valore di Platform.OS identifica il web?
        localStorage.setItem(key, value)              // ??? 2: quale metodo di localStorage salva un valore?
    } else {
        await SecureStore.setItemAsync(key, value)          // ??? 3: quale metodo di SecureStore usavi già in login.jsx?
    }
}