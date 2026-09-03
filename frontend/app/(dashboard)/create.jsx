import { ScrollView, StyleSheet, Text, Platform, Keyboard, TouchableWithoutFeedback, Modal, View, TouchableOpacity } from 'react-native'
import { useState } from 'react'
import DateTimePicker from '@react-native-community/datetimepicker'
import { Picker } from '@react-native-picker/picker'

import ThemedView from '../../components/ThemedView'
import Spacer from '../../components/Spacer'
import ThemedText from '../../components/ThemedText'
import ThemedTextInput from '../../components/ThemedTextInput'
import ThemedButton from '../../components/ThemedButton'

const COMPANIONS = [
    { label: 'Nessuno', value: null },
    { label: 'Mario Rossi', value: 'mario_rossi' },
    { label: 'Luigi Bianchi', value: 'luigi_bianchi' },
    { label: 'Anna Verdi', value: 'anna_verdi' },
]

const Create = () => {
    const [pickupAddress, setPickupAddress] = useState('')
    const [destinationAddress, setDestinationAddress] = useState('')
    const [notes, setNotes] = useState('')

    const [date, setDate] = useState(new Date())
    const [time, setTime] = useState(new Date())
    const [companion, setCompanion] = useState(null)

    const [showDatePicker, setShowDatePicker] = useState(false)
    const [showTimePicker, setShowTimePicker] = useState(false)

    const onChangeDate = (event, selectedDate) => {
        if (Platform.OS === 'android') {
            setShowDatePicker(false)
        }
        if (selectedDate) setDate(selectedDate)
    }

    const onChangeTime = (event, selectedTime) => {
        if (Platform.OS === 'android') {
            setShowTimePicker(false)
        }
        if (selectedTime) setTime(selectedTime)
    }

    const handleSubmit = () => {
        const payload = {
            pickupAddress,
            destinationAddress,
            notes,
            date: date.toISOString().split('T')[0],
            time: time.toTimeString().split(' ')[0],
            companion,
        }
        console.log('transport form submitted', payload)
    }

    return (
        <TouchableWithoutFeedback onPress={() => Keyboard.dismiss()}>
            <ThemedView style={styles.container}>
                <ScrollView
                    contentContainerStyle={styles.scrollContent}
                    keyboardShouldPersistTaps="handled"
                >
                    <Spacer />
                    <ThemedText title={true} style={styles.title}>
                        Crea Nuovo Trasporto
                    </ThemedText>

                    {/* Indirizzo di Partenza */}
                    <ThemedTextInput
                        style={styles.input}
                        placeholder="Indirizzo di Partenza"
                        onChangeText={setPickupAddress}
                        value={pickupAddress}
                    />

                    {/* Indirizzo di Arrivo */}
                    <ThemedTextInput
                        style={styles.input}
                        placeholder="Indirizzo di Destinazione"
                        onChangeText={setDestinationAddress}
                        value={destinationAddress}
                    />

                    {/* Note */}
                    <ThemedTextInput
                        style={styles.input}
                        placeholder="Note o dettagli sul percorso (opzionale)"
                        onChangeText={setNotes}
                        value={notes}
                        multiline
                    />

                    <Spacer height={10} />

                    {/* Selezione Data */}
                    <ThemedButton
                        onPress={() => setShowDatePicker(true)}
                        style={styles.input}
                    >
                        <Text style={styles.btnText}>
                            Data: {date.toLocaleDateString('it-IT')}
                        </Text>
                    </ThemedButton>

                    {/* Selezione Ora */}
                    <ThemedButton
                        onPress={() => setShowTimePicker(true)}
                        style={styles.input}
                    >
                        <Text style={styles.btnText}>
                            Ora: {time.toLocaleTimeString('it-IT', { hour: '2-digit', minute: '2-digit' })}
                        </Text>
                    </ThemedButton>

                    {/* Accompagnatore */}
                    <ThemedText style={styles.label}>
                        Accompagnatore
                    </ThemedText>
                    <ThemedView style={styles.pickerWrapper}>
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

                    <ThemedButton onPress={handleSubmit} style={styles.input}>
                        <Text style={styles.btnText}>Conferma Prenotazione</Text>
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
})