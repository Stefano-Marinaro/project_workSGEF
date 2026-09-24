import { StyleSheet, Text } from 'react-native'
import { useState, useEffect } from 'react' // fa partire la chiamata al beckend da sola, appena la schermata di apre, senza che l'utente clicchi niente

// hook di Expo Router che legge tutti i parametri presenti nell'URL della rotta corrente e li restituisce come oggetto
// (lo stesso hook che usiamo in login.jsx per leggere role)
import { useLocalSearchParams } from 'expo-router'

// client axios
import api from '../../config/httpClient.js'

const VerifyEmail = () => {

    const params = useLocalSearchParams()

    // può avere 3 valori 'verifying', 'success', 'error'
    const [status, setStatus] = useState('verifying')
    const [errorMessage, setErrorMessage] = useState('')

    // la funzione parte automaticamente una sola volta, appena il componente compare a schermo
    // l'array vuoto dice a React "fallo partire una volta sola e basta, non ripeterlo mai"
    
    // dettaglio tecnico: la funzione passata a useEffect non può essere async direttamente (regola di React)
    // perciò viene definita una funzione async dentro l'effetto e poi viene chiamata nella riga subito sotto
    // perché async? perché voglio aspettare il risultato della chiamata e mi serve await che esiste solo dentro funzioni async

    useEffect(() => {
        const verify = async () => {
            try {
                await api.post('/auth/verify-email', { token: params.token})
                setStatus('success')
            } catch(error) {
                setStatus('error')
                if(error.response) {
                    setErrorMessage(error.response.data.details || 'Link non valido o scaduto')
                } else {
                    setErrorMessage('Unable to reach the server')
                }
            }
        }

        verify()
        
    }, [])

    return (
        <Text>
            Token ricevuto: {params.token} - Stato: {status} {errorMessage}
        </Text> 
        // {params.token} prende il valore associato alla chiave token nell'URL
    )
}

export default VerifyEmail

const styles = StyleSheet.create({})