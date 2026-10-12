import * as SecureStore from 'expo-secure-store'
import { Platform } from 'react-native'

export const setItem = async (key, value) => {
    if (Platform.OS === 'web') {                 // ??? 1: quale valore di Platform.OS identifica il web?
        localStorage.setItem(key, value)              // ??? 2: quale metodo di localStorage salva un valore?
    } else {
        await SecureStore.setItemAsync(key, value)          // ??? 3: quale metodo di SecureStore usavi già in login.jsx?
    }
}

// per leggere
export const getItem = async (key) => {
    // sul web localStorage è sincrono, su mobile SecureStore è asincrono
    return Platform.OS === 'web' ? localStorage.getItem(key) : await SecureStore.getItemAsync(key)
}

// per cancellare
export const deleteItem = async (key) => {
    if (Platform.OS === 'web') localStorage.removeItem(key)
    else await SecureStore.deleteItemAsync(key)
}