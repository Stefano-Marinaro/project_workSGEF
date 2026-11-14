import { Text, StyleSheet } from 'react-native'
import { useRouter } from 'expo-router'
import { C } from '../../constants/caregiver'
import { Page, Scroll, PageTitle, Muted, Card, Icon, Btn } from './ui'

// Returning from Stripe never marks a payment successful: only the webhook can do so.
export default function PaymentReturn({ cancelled = false }) {
    const router = useRouter()
    return (
        <Page>
            <Scroll>
                <PageTitle>{cancelled ? 'Pagamento interrotto' : 'Verifica del pagamento'}</PageTitle>
                <Card>
                    <Icon name={cancelled ? 'information-circle-outline' : 'time-outline'} size={28} />
                    <Text style={styles.title}>{cancelled ? 'Controlla lo stato prima di riprovare' : 'Attendi la conferma del pagamento'}</Text>
                    <Muted style={styles.body}>{cancelled
                        ? 'Se hai chiuso o annullato la procedura di pagamento, consulta il dettaglio del trasporto prima di effettuare un nuovo tentativo.'
                        : 'Lo stato del trasporto verrà aggiornato quando il servizio di pagamento comunicherà l’esito a GoCare. Il ritorno a questa pagina, da solo, non conferma il pagamento. Evita un secondo pagamento mentre attendi la conferma.'}</Muted>
                </Card>
                <Btn label="Vai ai miei trasporti" onPress={() => router.replace('/(caregiver)/transport')} style={styles.button} />
            </Scroll>
        </Page>
    )
}

const styles = StyleSheet.create({ title: { fontSize: 17, fontWeight: '600', color: C.text, marginTop: 14 }, body: { fontSize: 14, lineHeight: 23, marginTop: 10 }, button: { marginTop: 20 } })
