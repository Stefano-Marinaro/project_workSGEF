
import { useState } from 'react'
import { StyleSheet, FlatList, View } from 'react-native'

import ThemedView from '../../components/ThemedView'
import ThemedText from '../../components/ThemedText'
import ThemedCard from '../../components/ThemedCard'
import Spacer from '../../components/Spacer'
import { Colors } from '../../constants/Colors'

// --- MOCK DATA ---
// Struttura pensata per essere sostituita 1:1 dalla futura chiamata API
// (es. GET /api/trasporti/utente/:id) senza toccare la UI sotto.
const MOCK_TRANSPORTS = [
    {
        id: '1',
        data: '10/09/2026',
        orario: '09:30',
        tipologia: 'Visita medica',
        destinazione: 'Ospedale Santa Maria della Misericordia',
        stato: 'Confermata',
        associazione: { nome: 'Croce Rossa Perugia', telefono: '075 123456' },
    },
    {
        id: '2',
        data: '14/09/2026',
        orario: '11:00',
        tipologia: 'Dimissione',
        destinazione: 'Domicilio - Via dei Filosofi 12',
        stato: 'In attesa',
        associazione: null,
    },
    {
        id: '3',
        data: '02/09/2026',
        orario: '15:00',
        tipologia: 'Visita medica',
        destinazione: 'Clinica San Sisto',
        stato: 'Conclusa',
        associazione: { nome: 'Misericordia Perugia', telefono: '075 654321' },
    },
]

const statusColor = (stato) => {
    switch (stato) {
        case 'Confermata': return '#4caf50'
        case 'In attesa': return Colors.warning
        case 'Conclusa': return '#8884a0'
        default: return Colors.primary
    }
}

const TransportCard = ({ item }) => (
    <ThemedCard style={styles.card}>
        <View style={styles.cardHeader}>
            <ThemedText title={true} style={styles.tipologia}>
                {item.tipologia}
            </ThemedText>
            <ThemedView style={[styles.badge, { backgroundColor: statusColor(item.stato) }]}>
                <ThemedText style={styles.badgeText}>{item.stato}</ThemedText>
            </ThemedView>
        </View>

        <Spacer height={8} />

        <ThemedText>{item.data} · {item.orario}</ThemedText>
        <ThemedText style={styles.destinazione}>{item.destinazione}</ThemedText>

        {item.associazione && (
            <>
                <Spacer height={10} />
                <ThemedText style={styles.assocLabel}>Presa in carico da</ThemedText>
                <ThemedText>{item.associazione.nome} · {item.associazione.telefono}</ThemedText>
            </>
        )}
    </ThemedCard>
)

const ListOfTransports = () => {
    // In futuro: sostituire con uno stato popolato da una fetch/useEffect
    // verso il backend C#/ASP.NET Core (Francesco/Elio).
    const [transports] = useState(MOCK_TRANSPORTS)

    return (
        <ThemedView style={styles.container} safe={true}>
            <Spacer />
            <ThemedText title={true} style={styles.heading}>
                I Miei Trasporti
            </ThemedText>
            <Spacer height={25} />

            <FlatList
                data={transports}
                keyExtractor={(item) => item.id}
                renderItem={({ item }) => <TransportCard item={item} />}
                contentContainerStyle={styles.list}
                ItemSeparatorComponent={() => <Spacer height={12} />}
                ListEmptyComponent={
                    <ThemedText style={styles.empty}>
                        Nessun trasporto registrato.
                    </ThemedText>
                }
            />
        </ThemedView>
    )
}

export default ListOfTransports

const styles = StyleSheet.create({
    container: { flex: 1 },
    heading: {
        fontWeight: 'bold',
        fontSize: 18,
        textAlign: 'center',
    },
    list: { paddingHorizontal: 20, paddingBottom: 40 },
    card: { width: '100%' },
    cardHeader: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
    },
    tipologia: { fontSize: 16, fontWeight: '600' },
    destinazione: { marginTop: 2 },
    assocLabel: { fontSize: 12, opacity: 0.7 },
    badge: {
        paddingHorizontal: 10,
        paddingVertical: 4,
        borderRadius: 20,
    },
    badgeText: { color: '#fff', fontSize: 12, fontWeight: '600' },
    empty: { textAlign: 'center', marginTop: 40, opacity: 0.6 },
})