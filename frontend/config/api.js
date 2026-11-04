// Indirizzo del backend.
// - In Docker viene passato a build-time tramite la variabile EXPO_PUBLIC_API_URL
//   (Expo "incolla" nel bundle le variabili che iniziano con EXPO_PUBLIC_).
// - Senza variabile si usa l'IP di LAN dello sviluppatore, come prima.
export const API_BASE_URL = process.env.EXPO_PUBLIC_API_URL || 'http://10.10.52.36:5161'
