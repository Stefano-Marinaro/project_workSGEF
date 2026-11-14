import { useState } from 'react'
import { View, Text, Pressable, StyleSheet, Linking } from 'react-native'
import { useLocalSearchParams, useRouter } from 'expo-router'
import { C } from '../../constants/caregiver'
import { FAQ, HELP_TOPICS, HELP_CONTACTS } from '../../constants/help'
import { Page, Scroll, PageTitle, Muted, BackLink, Card, Icon, Divider, Btn, ErrorText } from '../../components/caregiver/ui'

const MENU = [
    { topic: 'states', icon: 'git-compare-outline', title: 'Stato delle richieste', subtitle: 'Segui ogni passaggio' },
    { topic: 'payments', icon: 'wallet-outline', title: 'Contributi e pagamenti', subtitle: 'Importi, pagamenti e annullamenti' },
    { topic: 'support', icon: 'chatbubble-ellipses-outline', title: 'Assistenza e segnalazioni', subtitle: 'Non hai trovato la risposta?' },
]

export default function Help() {
    const router = useRouter()
    const { topic } = useLocalSearchParams()
    const current = typeof topic === 'string' ? topic : ''
    const detail = HELP_TOPICS[current]
    const [expanded, setExpanded] = useState('confirmation')
    const [linkError, setLinkError] = useState('')

    const openTopic = (value) => {
        setLinkError('')
        router.push({ pathname: '/(caregiver)/help', params: { topic: value } })
    }
    const openLink = async (url) => {
        setLinkError('')
        try { await Linking.openURL(url) }
        catch { setLinkError('Impossibile aprire il collegamento. Riprova dal browser o dalla tua app di posta.') }
    }
    const isSupport = current === 'support'
    const isLegal = current === 'privacy' || current === 'terms'
    const hasDetail = !!detail || isSupport || isLegal
    const legalUrl = current === 'privacy' ? HELP_CONTACTS.privacyUrl : HELP_CONTACTS.termsUrl
    const supportMail = `mailto:${HELP_CONTACTS.email}?subject=${encodeURIComponent('Richiesta di assistenza GoCare')}`

    return (
        <Page>
            <Scroll key={current}>
                <BackLink label={hasDetail ? 'Aiuto e informazioni' : 'Profilo'}
                    onPress={() => {
                        if (!hasDetail) router.navigate('/(caregiver)/profile')
                        else if (router.canGoBack()) router.back()
                        else router.replace({ pathname: '/(caregiver)/help', params: { topic: '' } })
                    }} style={styles.back} />
                <PageTitle>{detail?.title || (isSupport ? 'Assistenza e segnalazioni' : isLegal ? current === 'privacy' ? 'Privacy' : 'Condizioni d’uso' : 'Aiuto e informazioni')}</PageTitle>

                {detail ? (
                    <>
                        <Muted style={styles.intro}>{detail.intro}</Muted>
                        {detail.sections.map((section) => (
                            <Card key={section.title} style={styles.sectionCard}>
                                <Text style={styles.sectionTitle}>{section.title}</Text>
                                <Muted style={styles.body}>{section.text}</Muted>
                            </Card>
                        ))}
                    </>
                ) : isSupport ? (
                    <>
                        <Muted style={styles.intro}>Una mano per usare GoCare o segnalare un problema.</Muted>
                        <Card style={styles.sectionCard}>
                            <Text style={styles.sectionTitle}>Per il tuo trasporto</Text>
                            <Muted style={styles.body}>Se il viaggio è stato confermato, consulta il suo dettaglio per trovare le informazioni e i recapiti disponibili dell’associazione.</Muted>
                            <Btn label="Vai ai trasporti" variant="outline" onPress={() => router.navigate('/(caregiver)/transport')} style={styles.action} />
                        </Card>
                        <Card style={styles.sectionCard}>
                            <Text style={styles.sectionTitle}>Segnala un problema nell’app</Text>
                            <Muted style={styles.body}>Indica la schermata interessata, cosa stavi facendo e il messaggio di errore. Se alleghi un’immagine, oscura i dati personali non necessari. Non inviare password o dati sanitari.</Muted>
                            {HELP_CONTACTS.email ? (
                                <>
                                    <Text selectable style={styles.contact}>{HELP_CONTACTS.email}</Text>
                                    <Btn label="Scrivi all’assistenza" icon="mail-outline" onPress={() => openLink(supportMail)} style={styles.action} />
                                </>
                            ) : <Muted style={styles.unavailable}>I recapiti di assistenza non sono ancora disponibili. Per assistenza sull’account, rivolgiti al gestore di GoCare tramite il tuo referente.</Muted>}
                        </Card>
                    </>
                ) : isLegal ? (
                    <Card style={styles.sectionCard}>
                        {/^https?:\/\//i.test(legalUrl) ? (
                            <>
                                <Muted style={styles.body}>Consulta il documento completo fornito dal gestore di GoCare.</Muted>
                                <Btn label={current === 'privacy' ? 'Apri l’informativa privacy' : 'Apri le condizioni d’uso'} icon="open-outline" onPress={() => openLink(legalUrl)} style={styles.action} />
                            </>
                        ) : <Muted style={styles.body}>Il documento non è ancora disponibile in questa versione. Richiedilo al gestore di GoCare prima di procedere se hai dubbi sull’utilizzo del servizio o sul trattamento dei dati.</Muted>}
                    </Card>
                ) : (
                    <>
                        <Muted style={styles.intro}>Una mano, quando ti serve.</Muted>
                        <Card onPress={() => openTopic('guide')} style={styles.guide}>
                            <Icon name="book-outline" size={25} />
                            <View style={styles.flex}>
                                <Text style={styles.sectionTitle}>Il tuo primo trasporto?</Text>
                                <Muted style={styles.subtitle}>Scopri come funziona GoCare</Muted>
                            </View>
                            <Icon name="chevron-forward" size={18} />
                        </Card>

                        <Text style={styles.heading}>Domande frequenti</Text>
                        <Card style={styles.faqCard}>
                            {FAQ.map((item, index) => {
                                const open = expanded === item.id
                                return (
                                    <View key={item.id}>
                                        {index > 0 ? <Divider style={styles.rule} /> : null}
                                        <Pressable accessibilityRole="button" accessibilityState={{ expanded: open }} accessibilityLabel={item.question}
                                            onPress={() => setExpanded(open ? null : item.id)} style={({ pressed }) => [styles.question, pressed && styles.pressed]}>
                                            <Text style={styles.questionText}>{item.question}</Text>
                                            <Icon name={open ? 'chevron-up' : 'chevron-down'} size={18} />
                                        </Pressable>
                                        {open ? <Muted style={styles.answer}>{item.answer}</Muted> : null}
                                    </View>
                                )
                            })}
                        </Card>

                        <Text style={styles.heading}>Ti serve altro?</Text>
                        {MENU.map((item) => (
                            <Card key={item.topic} onPress={() => openTopic(item.topic)} style={styles.menuRow}>
                                <Icon name={item.icon} size={22} />
                                <View style={styles.flex}>
                                    <Text style={styles.menuTitle}>{item.title}</Text>
                                    <Muted style={styles.subtitle}>{item.subtitle}</Muted>
                                </View>
                                <Icon name="chevron-forward" size={18} color={C.muted} />
                            </Card>
                        ))}
                        <View style={styles.notice}>
                            <Icon name="information-circle-outline" size={19} color={C.muted} />
                            <Muted style={styles.noticeText}>GoCare organizza accompagnamenti programmati. Non è un servizio di emergenza.</Muted>
                        </View>
                        <View style={styles.legal}>
                            <Pressable accessibilityRole="button" onPress={() => openTopic('privacy')} style={styles.legalButton}><Text style={styles.legalText}>Privacy</Text></Pressable>
                            <Muted>·</Muted>
                            <Pressable accessibilityRole="button" onPress={() => openTopic('terms')} style={styles.legalButton}><Text style={styles.legalText}>Condizioni d’uso</Text></Pressable>
                        </View>
                    </>
                )}
                <ErrorText>{linkError}</ErrorText>
            </Scroll>
        </Page>
    )
}

const styles = StyleSheet.create({
    flex: { flex: 1 },
    pressed: { opacity: 0.85 },
    back: { marginTop: 12 },
    intro: { fontSize: 15, lineHeight: 24, marginBottom: 20 },
    guide: { flexDirection: 'row', alignItems: 'center', gap: 14, marginBottom: 22 },
    heading: { fontSize: 16, fontWeight: '600', color: C.text, marginBottom: 12 },
    sectionTitle: { fontSize: 16, fontWeight: '600', color: C.text },
    subtitle: { fontSize: 13, lineHeight: 20, marginTop: 4 },
    faqCard: { paddingVertical: 0, marginBottom: 22 },
    question: { flexDirection: 'row', alignItems: 'center', gap: 12, minHeight: 54, paddingVertical: 14 },
    questionText: { flex: 1, fontSize: 14, fontWeight: '600', lineHeight: 21, color: C.text },
    rule: { marginVertical: 0 },
    answer: { fontSize: 14, lineHeight: 22, paddingBottom: 16 },
    menuRow: { flexDirection: 'row', alignItems: 'center', gap: 14, marginBottom: 12 },
    menuTitle: { fontSize: 15, fontWeight: '600', color: C.text },
    notice: { flexDirection: 'row', gap: 10, marginTop: 10 },
    noticeText: { flex: 1, fontSize: 13, lineHeight: 20 },
    legal: { flexDirection: 'row', flexWrap: 'wrap', alignItems: 'center', justifyContent: 'center', gap: 12, marginTop: 12 },
    legalButton: { minHeight: 44, justifyContent: 'center', paddingHorizontal: 4 },
    legalText: { fontSize: 13, color: C.primary, textDecorationLine: 'underline' },
    sectionCard: { marginBottom: 14 },
    body: { fontSize: 14, lineHeight: 23, marginTop: 10 },
    action: { marginTop: 16 },
    contact: { fontSize: 15, color: C.primary, marginTop: 14 },
    unavailable: { fontSize: 14, lineHeight: 23, marginTop: 16 },
})
