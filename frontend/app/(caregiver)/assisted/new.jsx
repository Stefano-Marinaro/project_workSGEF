import { useCallback, useState } from 'react'
import { View, Text, StyleSheet } from 'react-native'
import { useLocalSearchParams, useRouter, useFocusEffect } from 'expo-router'

import api from '../../../config/httpClient.js'
import { C, apiErrorMessage } from '../../../constants/caregiver'
import AssistedForm, { emptyAssisted, validateAssisted } from '../../../components/caregiver/AssistedForm'
import { Page, Scroll, PageTitle, Muted, Footer, Btn, ErrorText, SuccessCheck, Loading, EmptyState } from '../../../components/caregiver/ui'

// Mockup "Registra la persona da accompagnare" + schermata "Assistito registrato"
const NewAssisted = () => {
    const { from } = useLocalSearchParams()
    const router = useRouter()
    const [checkingProfile, setCheckingProfile] = useState(true)
    const [profileReady, setProfileReady] = useState(false)
    const [form, setForm] = useState(emptyAssisted)
    const [errors, setErrors] = useState({})
    const [saving, setSaving] = useState(false)
    const [apiError, setApiError] = useState('')
    const [created, setCreated] = useState(null)

    // Ogni volta che si arriva qui si parte da un modulo vuoto
    useFocusEffect(
        useCallback(() => {
            let active = true
            setCheckingProfile(true)
            setProfileReady(false)
            const checkProfile = async () => {
                try {
                    const { data } = await api.get('/me/profile')
                    if (!active) return
                    if (!(data.name && data.surname && data.birthDate && data.phone)) {
                        router.replace({ pathname: '/editProfile', params: { next: 'assisted', from } })
                        return
                    }
                    setProfileReady(true)
                } catch (e) {
                    if (active) setApiError(apiErrorMessage(e, 'Impossibile verificare il profilo.'))
                } finally {
                    if (active) setCheckingProfile(false)
                }
            }
            checkProfile()
            setForm(emptyAssisted)
            setErrors({})
            setApiError('')
            setCreated(null)
            return () => { active = false }
        }, [from, router]),
    )

    const submit = async () => {
        if (!profileReady) return
        const { errors: found, payload } = validateAssisted(form)
        setErrors(found)
        if (Object.keys(found).length) return
        setSaving(true)
        setApiError('')
        try {
            const { data } = await api.post('/assisted', payload)
            setCreated({ id: data.id, name: `${payload.name} ${payload.surname}` })
        } catch (e) {
            setApiError(apiErrorMessage(e, 'Registrazione non riuscita.'))
        } finally {
            setSaving(false)
        }
    }

    if (checkingProfile) return <Loading />
    if (!profileReady) {
        return (
            <Page>
                <Scroll>
                    <EmptyState
                        title="Completa il tuo profilo"
                        text={apiError || 'Completa i dati personali prima di registrare un assistito.'}
                        action="Vai al profilo"
                        onAction={() => router.replace({ pathname: '/editProfile', params: { next: 'assisted', from } })}
                    />
                </Scroll>
            </Page>
        )
    }

    if (created) {
        return (
            <Page style={styles.center}>
                <SuccessCheck />
                <Text style={styles.createdTitle}>Assistito registrato</Text>
                <Text style={styles.createdText}>
                    <Text style={styles.strong}>{created.name}</Text> è stato aggiunto ai tuoi assistiti.
                </Text>
                <Btn
                    label="Continua con il trasporto"
                    onPress={() => router.dismissTo({ pathname: '/transport/new', params: { assistedId: created.id } })}
                    style={styles.createdBtn}
                />
                <Btn label="Vai agli assistiti" variant="ghost" onPress={() => router.replace('/assisted')} style={styles.createdBtn} />
            </Page>
        )
    }

    return (
        <Page>
            <Scroll>
                <PageTitle>Registra la persona da accompagnare</PageTitle>
                <Muted style={styles.intro}>
                    {from === 'wizard'
                        ? 'Inserisci i dati della persona da accompagnare. Poi tornerai alla richiesta per completarla.'
                        : 'Inserisci i dati della persona da accompagnare. Poi potrai creare la richiesta di trasporto.'}
                </Muted>
                <Text style={styles.required}>
                    I campi contrassegnati con <Text style={styles.star}>*</Text> sono obbligatori.
                </Text>
                <AssistedForm form={form} setForm={setForm} errors={errors} />
                <ErrorText>{apiError}</ErrorText>
            </Scroll>
            <Footer>
                <Btn label="Indietro" variant="secondary" onPress={() => router.back()} style={styles.back} />
                <Btn label="Registra assistito" onPress={submit} loading={saving} style={styles.submit} />
            </Footer>
        </Page>
    )
}

export default NewAssisted

const styles = StyleSheet.create({
    center: { justifyContent: 'center', alignItems: 'center', paddingHorizontal: 28 },
    intro: { fontSize: 15, lineHeight: 24, marginBottom: 14 },
    required: { fontSize: 12, color: C.muted, marginBottom: 16 },
    star: { color: C.primary, fontWeight: '700' },
    back: { paddingHorizontal: 28 },
    submit: { flex: 1, maxWidth: 200 },
    createdTitle: { fontSize: 24, fontWeight: '600', color: C.text, marginTop: 20, marginBottom: 12, textAlign: 'center' },
    createdText: { fontSize: 15, lineHeight: 24, color: C.muted, textAlign: 'center', marginBottom: 24 },
    strong: { fontWeight: '600', color: C.textStrong },
    createdBtn: { alignSelf: 'stretch', marginBottom: 8 },
})
