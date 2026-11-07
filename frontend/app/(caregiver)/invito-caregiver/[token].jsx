import { useCallback, useState } from 'react'
import { View, Text, StyleSheet } from 'react-native'
import { useLocalSearchParams, useRouter, useFocusEffect } from 'expo-router'

import api from '../../../config/httpClient.js'
import { getItem } from '../../../config/storage.js'
import { C, apiErrorMessage } from '../../../constants/caregiver'
import { Page, Scroll, Loading, PageTitle, Card, Btn, SuccessCheck, ErrorText } from '../../../components/caregiver/ui'

// Pagina aperta dal link di invito (Frontend:CaregiverInviteUrl = /invito-caregiver/{token}).
// Il caregiver invitato deve aver fatto l'accesso: altrimenti passa dal login e torna qui.
const CaregiverInvite = () => {
    const { token } = useLocalSearchParams()
    const router = useRouter()

    const [loading, setLoading] = useState(true)
    const [preview, setPreview] = useState(null)
    const [error, setError] = useState('')
    const [accepting, setAccepting] = useState(false)
    const [acceptError, setAcceptError] = useState('')
    const [accepted, setAccepted] = useState(false)

    useFocusEffect(
        useCallback(() => {
            const load = async () => {
                const accessToken = await getItem('accessToken')
                if (!accessToken) {
                    router.replace({ pathname: '/login', params: { redirect: `/invito-caregiver/${token}` } })
                    return
                }
                try {
                    const { data } = await api.get(`/caregiver-invites/${token}`)
                    setPreview(data)
                    setError('')
                } catch (e) {
                    setError(apiErrorMessage(e, 'Il link non è valido.'))
                } finally {
                    setLoading(false)
                }
            }
            load()
        }, [token]),
    )

    const accept = async () => {
        setAccepting(true)
        setAcceptError('')
        try {
            await api.post(`/caregiver-invites/${token}/accept`)
            setAccepted(true)
        } catch (e) {
            setAcceptError(apiErrorMessage(e, 'Impossibile accettare l’invito.'))
        } finally {
            setAccepting(false)
        }
    }

    if (loading) return <Loading />

    const personName = preview ? `${preview.assistedPersonName} ${preview.assistedPersonSurname}` : ''
    const inviter = preview ? [preview.invitedByName, preview.invitedBySurname].filter(Boolean).join(' ') : ''
    const goAssisted = () => router.replace('/assisted')

    let body
    if (error) {
        body = (
            <View style={styles.empty}>
                <Text style={styles.emptyTitle}>Invito non disponibile</Text>
                <Text style={styles.emptyText}>{error}</Text>
                <Btn label="Vai agli assistiti" onPress={goAssisted} />
            </View>
        )
    } else if (accepted || preview.alreadyLinked) {
        body = (
            <View style={styles.empty}>
                <SuccessCheck />
                <Text style={[styles.emptyTitle, styles.spaced]}>Invito accettato</Text>
                <Text style={styles.emptyText}>Sei collegato a {personName}.</Text>
                <Btn label="Vai agli assistiti" onPress={goAssisted} />
            </View>
        )
    } else {
        body = (
            <Card style={styles.panel}>
                {inviter ? <Text style={styles.from}>Invito da {inviter}</Text> : null}
                <Text style={styles.title}>Segui {personName}</Text>
                <Text style={styles.text}>Accettando l’invito comparirai tra i caregiver collegati all’assistito.</Text>
                <ErrorText>{acceptError}</ErrorText>
                <Btn label="Accetta invito" onPress={accept} loading={accepting} style={styles.accept} />
            </Card>
        )
    }

    return (
        <Page>
            <Scroll>
                <PageTitle>Invito caregiver</PageTitle>
                {body}
            </Scroll>
        </Page>
    )
}

export default CaregiverInvite

const styles = StyleSheet.create({
    empty: { alignItems: 'center', paddingVertical: 40, paddingHorizontal: 12 },
    emptyTitle: { fontSize: 22, lineHeight: 30, fontWeight: '600', color: C.text, marginBottom: 14, textAlign: 'center' },
    spaced: { marginTop: 18 },
    emptyText: { fontSize: 14, lineHeight: 23, color: C.muted, textAlign: 'center', marginBottom: 22 },
    panel: { padding: 16 },
    from: { fontSize: 12, color: C.muted, marginBottom: 6 },
    title: { fontSize: 16, lineHeight: 24, fontWeight: '600', color: C.text, marginBottom: 7 },
    text: { fontSize: 13, lineHeight: 22, color: C.muted },
    accept: { marginTop: 20, borderRadius: 11 },
})
