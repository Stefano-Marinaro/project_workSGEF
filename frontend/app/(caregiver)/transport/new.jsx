import { Pressable, View, ScrollView, StyleSheet, Text, Platform, Keyboard, TouchableWithoutFeedback, Modal, TouchableOpacity } from 'react-native'
import { useState } from 'react'
import DateTimePicker from '@react-native-community/datetimepicker' //Gestisce la data/ora a secondo di IOS/Android
import { Picker } from '@react-native-picker/picker' //Picker, stile tendina (select)

import ThemedView from '../../../components/ThemedView'
import Spacer from '../../../components/Spacer'
import ThemedText from '../../../components/ThemedText'
import ThemedTextInput from '../../../components/ThemedTextInput'
import ThemedButton from '../../../components/ThemedButton'

// Gli accompagnatori
const COMPANIONS = [
    { label: 'Nobody', value: null },
    { label: 'Mario Rossi', value: 'mario_rossi' },
    { label: 'Luigi Bianchi', value: 'luigi_bianchi' },
    { label: 'Anna Verdi', value: 'anna_verdi' },
]

// Tipo di viaggio
const TYPE_OF_TRANSPORT = [
    { label: 'Nobody', value: null },
    { label: 'Visit', value: 'visit' },
    { label: 'Admission', value: 'admission' },
    { label: 'Discharge', value: 'discharge' },
    { label: 'Transfer', value: 'transfer' },
]

const DIRECTIONS = [
    { label: 'Round Trip', value: 'round_trip' },
    { label: 'Outbound Only', value: 'outbound' },
    { label: 'Return Only', value: 'return' },
]

// Gruppo cura (placeholder - UC9 non ancora implementato)
const CARE_GROUPS = [
    { label: 'Nobody', value: null },
    { label: 'Mamma Rossi', value: 'gruppo_1' },
    { label: 'Nonno Bianchi', value: 'gruppo_2' },
]

const Create = () => {
    //useState è una funzione React che dà memoria a un componente, cioè l'informazione salvata 
    //su questa variabile const rimane tale un render e un altro e 
    //può essere modificata solo da setPickupAddress
    const [pickupAddress, setPickupAddress] = useState('')
    const [destinationAddress, setDestinationAddress] = useState('')
    const [notes, setNotes] = useState('')
    const [direction, setDirection] = useState('round_trip')

    const [date, setDate] = useState(new Date())
    const [time, setTime] = useState(new Date())
    const [companion, setCompanion] = useState(null)
    const [typeOfTransport, setTypeOfTransport] = useState(null)
    const [careGroup, setCareGroup] = useState(null)

    const [showDatePicker, setShowDatePicker] = useState(false)
    const [showTimePicker, setShowTimePicker] = useState(false)

    //Su Android il selettore di data si apre e chiude da solo appena scegli un giorno,
    //per questo devi dire a React Native che non è piu aperto con setShowDatePicker(false)
    const onChangeDate = (event, selectedDate) => {
        if (Platform.OS === 'android') {
            setShowDatePicker(false)
        }
        if (selectedDate) setDate(selectedDate)
        // è un controllo di sicurezza in caso l'utente
        //"tocca fuori" prima di aver selezionato una data quindi aggiorni lo stato se è arrivato davvero un valore
    }

    const onChangeTime = (event, selectedTime) => {
        if (Platform.OS === 'android') {
            setShowTimePicker(false)
        }
        if (selectedTime) setTime(selectedTime)
    }

    // tutte le informazioni fornite, che verrà collegata con una funzione fetch per il backend C#
    //date.toISOString(),split('T')[0], converte la data in formato ISO e prende la parte prima di T([0], lo split li divide in 0 e 1) non compresa

    const handleSubmit = () => {
        const payload = {
            pickupAddress,
            destinationAddress,
            notes,
            date: date.toISOString().split('T')[0],
            time: time.toTimeString().split(' ')[0],
            companion,
            typeOfTransport,
            direction,
            careGroup,
        }
        console.log('transport form submitted', payload)
    }

    return (
        //TouchableWithoutFeedback è un wrapper invisibile che intercetta i tap, se l'utente tocca un punto qualsiasi dello schermo fuori dai campi di testo, keyboard.dismiss() chiude la tastiera
        //ScrollView rende il contenuto scrollabile 
        //contentContainerStyle definisce lo stile del contenuto dentro (dove metti alignItems, paddingBottom ecc)
        //keyboardShouldPersistTaps="handled": senza questa prop, se la tastiera è aperta e tocchi un bottone, il primo tap si "sprecherebbe" solo per chiudere la tastiera. Con "handled", il tap viene passato correttamente all'elemento anche se la tastiera è aperta.
        <TouchableWithoutFeedback onPress={() => Keyboard.dismiss()}>
            <ThemedView style={styles.container}>
                <ScrollView
                    contentContainerStyle={styles.scrollContent}
                    keyboardShouldPersistTaps="handled"
                >
                    <Spacer />
                    <ThemedText title={true} style={styles.title}>
                        Create New Transport 
                    </ThemedText>

                    {/* Indirizzo di Partenza */}
                    <ThemedTextInput
                        style={styles.input}
                        placeholder="Starting Address"
                        onChangeText={setPickupAddress}
                        value={pickupAddress}
                    />

                    {/* Indirizzo di Arrivo */}
                    <ThemedTextInput
                        style={styles.input}
                        placeholder="Delivery Address"
                        onChangeText={setDestinationAddress}
                        value={destinationAddress}
                    />

                    {/* Note */}
                    <ThemedTextInput
                        style={styles.input}
                        placeholder="Notes or details about the route (optional)"
                        onChangeText={setNotes}
                        value={notes}
                        multiline
                    />

                    <Spacer height={10} />

                    {/* Selezione Data, la data è gia formattata in italiano grazie alla funzione toLocateDateString */}
                    <ThemedButton
                        onPress={() => setShowDatePicker(true)}
                        style={styles.input}
                    >
                        <Text style={styles.btnText}>
                            Date: {date.toLocaleDateString('it-IT')} 
                        </Text>
                    </ThemedButton>

                    {/* Selezione Ora */}
                    <ThemedButton
                        onPress={() => setShowTimePicker(true)}
                        style={styles.input}
                    >
                        <Text style={styles.btnText}>
                            Time: {time.toLocaleTimeString('it-IT', { hour: '2-digit', minute: '2-digit' })}
                        </Text>
                    </ThemedButton>

                    <Spacer height={30} />

                    {/* Accompagnatore */}
                    <ThemedText style={styles.label}>
                        Companion
                    </ThemedText>
                    <ThemedView style={styles.pickerWrapper}>
                        
                        {/*selectedValue, lo stato attuale */}
                        <Picker
                            selectedValue={companion}
                            onValueChange={(itemValue) => setCompanion(itemValue)}
                        >
                            {COMPANIONS.map((item) => (
                                <Picker.Item key={item.value ?? 'none'} label={item.label} value={item.value} />
                            ))}
                        </Picker>
                    </ThemedView>

                    <Spacer height={30} />

                    {/* Tipo di viaggio */}
                    <ThemedText style={styles.label}>
                        Type of Transport
                    </ThemedText>
                    <ThemedView style={styles.pickerWrapper}>
                        
                        {/*selectedValue, lo stato attuale */}
                        <Picker
                            selectedValue={typeOfTransport}
                            onValueChange={(itemValue) => setTypeOfTransport(itemValue)}
                        >
                            {TYPE_OF_TRANSPORT.map((item) => (
                                <Picker.Item key={item.value ?? 'none'} label={item.label} value={item.value} />
                            ))}
                        </Picker>
                    </ThemedView>

                    <Spacer height={30} /> 

                    {/* Direzione del viaggio */}
                    <ThemedText style={styles.label}>
                        Direction
                    </ThemedText>
                    
                    <View style={styles.roleOptions}>
                        {DIRECTIONS.map((item) => (
                            <Pressable
                                key={item.value}
                                onPress={() => setDirection(item.value)}
                                    style={[styles.roleOption, direction === item.value && styles.selectedRole]}
                            >
                                <ThemedText>{item.label}</ThemedText>
                            </Pressable>
                        ))}
                    </View>

                    <Spacer height={30} /> 

                    {/* Per chi (gruppo cura) - placeholder */}
                    <ThemedText style={styles.label}>
                        Select the Care Group
                    </ThemedText>
                    <ThemedView style={styles.pickerWrapper}>
                    <Picker
                        selectedValue={careGroup}
                        onValueChange={(itemValue) => setCareGroup(itemValue)}
                    >
                    {CARE_GROUPS.map((item) => (
                        <Picker.Item key={item.value ?? 'none'} label={item.label} value={item.value} />
                    ))}
                    </Picker>

                </ThemedView>

                    <Spacer height={30} />

                    <ThemedButton onPress={handleSubmit} style={styles.input}>
                        <Text style={styles.btnText}>Transport Confirmation</Text>
                    </ThemedButton>

                    <Spacer height={50} />
                </ScrollView>

                {/* MODAL DATE PICKER (iOS) / NATIVO (Android) */}
                {showDatePicker && (
                    Platform.OS === 'ios' ? (
                        <Modal transparent={true} animationType="slide" visible={showDatePicker}>
                            <TouchableOpacity style={styles.modalOverlay} activeOpacity={1} onPress={() => setShowDatePicker(false)}>
                                <View style={styles.modalContent}>
                                    <View style={styles.modalHeader}>
                                        <TouchableOpacity onPress={() => setShowDatePicker(false)}>
                                            <Text style={styles.doneText}>Conferma</Text>
                                        </TouchableOpacity>
                                    </View>
                                    <DateTimePicker
                                        value={date}
                                        mode="date"
                                        display="spinner"
                                        onChange={onChangeDate}
                                        textColor="#000000"
                                        themeVariant="light"
                                    />
                                </View>
                            </TouchableOpacity>
                        </Modal>
                    ) : (
                        <DateTimePicker
                            value={date}
                            mode="date"
                            display="default"
                            onChange={onChangeDate}
                        />
                    )
                )}

                {/* MODAL TIME PICKER (iOS) / NATIVO (Android) */}
                {showTimePicker && (
                    Platform.OS === 'ios' ? (
                        <Modal transparent={true} animationType="slide" visible={showTimePicker}>
                            <TouchableOpacity style={styles.modalOverlay} activeOpacity={1} onPress={() => setShowTimePicker(false)}>
                                <View style={styles.modalContent}>
                                    <View style={styles.modalHeader}>
                                        <TouchableOpacity onPress={() => setShowTimePicker(false)}>
                                            <Text style={styles.doneText}>Conferma</Text>
                                        </TouchableOpacity>
                                    </View>
                                    <DateTimePicker
                                        value={time}
                                        mode="time"
                                        is24Hour={true}
                                        display="spinner"
                                        onChange={onChangeTime}
                                        textColor="#000000"
                                        themeVariant="light"
                                    />
                                </View>
                            </TouchableOpacity>
                        </Modal>
                    ) : (
                        <DateTimePicker
                            value={time}
                            mode="time"
                            is24Hour={true}
                            display="default"
                            onChange={onChangeTime}
                        />
                    )
                )}
            </ThemedView>
        </TouchableWithoutFeedback>
    )
}

export default Create

const styles = StyleSheet.create({
    container: {
        flex: 1,
    },
    scrollContent: {
        alignItems: 'center',
        paddingBottom: 40,
    },
    title: {
        fontWeight: 'bold',
        textAlign: 'center',
        fontSize: 18,
        marginBottom: 30,
        marginTop: 20
    },
    input: {
        width: '80%',
        marginBottom: 15,
    },
    label: {
        alignSelf: 'flex-start',
        marginLeft: '10%',
        marginBottom: 5,
    },
    btnText: {
        color: '#f2f2f2',
    },
    pickerWrapper: {
        width: '80%',
        borderRadius: 8,
        overflow: 'hidden',
    },
    modalOverlay: {
        flex: 1,
        justifyContent: 'flex-end',
        backgroundColor: 'rgba(0,0,0,0.5)',
    },
    modalContent: {
        backgroundColor: '#ffffff',
        borderTopLeftRadius: 16,
        borderTopRightRadius: 16,
        paddingBottom: 30,
    },
    modalHeader: {
        alignItems: 'flex-end',
        padding: 15,
        borderBottomWidth: 1,
        borderBottomColor: '#eee',
        backgroundColor: '#f8f8f8',
        borderTopLeftRadius: 16,
        borderTopRightRadius: 16,
    },
    doneText: {
        color: '#007AFF',
        fontWeight: 'bold',
        fontSize: 16,
    },
    roleOptions: {
    width: '80%',
    flexDirection: 'row',
    gap: 8,
    marginBottom: 10,
    },
    roleOption: {
        flex: 1,
        padding: 12,
        borderWidth: 1,
        borderColor: '#aaa',
        borderRadius: 6,
        alignItems: 'center',
    },
    selectedRole: {
        borderColor: '#2f80ed',
        backgroundColor: '#dbeafe',
    },
})