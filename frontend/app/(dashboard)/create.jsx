import { ScrollView, StyleSheet, Text, Platform, Keyboard, TouchableWithoutFeedback } from 'react-native'
import { useState } from 'react'
import DateTimePicker from '@react-native-community/datetimepicker'
import { Picker } from '@react-native-picker/picker'
 
import ThemedView from '../../components/ThemedView'
import Spacer from '../../components/Spacer'
import ThemedText from '../../components/ThemedText'
import ThemedTextInput from '../../components/ThemedTextInput'
import ThemedButton from '../../components/ThemedButton'
 
// esempio statico degli accompagnatori, da sostituire poi con i dati reali
const COMPANIONS = [
    { label: 'Nessuno', value: null },
    { label: 'Mario Rossi', value: 'mario_rossi' },
    { label: 'Luigi Bianchi', value: 'luigi_bianchi' },
    { label: 'Anna Verdi', value: 'anna_verdi' },
]
 
const Create = () => {
 
    const [address, setAddress] = useState('')
    const [date, setDate] = useState(new Date())
    const [time, setTime] = useState(new Date())
    const [companion, setCompanion] = useState(null)
 
    const [showDatePicker, setShowDatePicker] = useState(false)
    const [showTimePicker, setShowTimePicker] = useState(false)
 
    const onChangeDate = (event, selectedDate) => {
        setShowDatePicker(Platform.OS === 'ios') // su ios resta visibile finché non si chiude, su android si chiude da solo
        if (selectedDate) setDate(selectedDate)
    }
 
    const onChangeTime = (event, selectedTime) => {
        setShowTimePicker(Platform.OS === 'ios')
        if (selectedTime) setTime(selectedTime)
    }
 
    const handleSubmit = () => {
        const payload = {
            address,
            date: date.toISOString().split('T')[0], // yyyy-mm-dd
            time: time.toTimeString().split(' ')[0], // hh:mm:ss
            companion,
        }
        console.log('transport form submitted', payload)
        // chiamata all'api, funzione di creazione trasporto
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
                        Create New Transport
                    </ThemedText>
 
                    <ThemedTextInput
                        style={{ width: '80%', marginBottom: 20 }}
                        placeholder="Address"
                        onChangeText={setAddress}
                        value={address}
                    />
 
                    <Spacer height={10} />
 
                    {/* Selezione della data */}
                    <ThemedButton
                        onPress={() => setShowDatePicker(true)}
                        style={{ width: '80%', marginBottom: 20 }}
                    >
                        <Text style={{ color: '#f2f2f2' }}>
                            Data: {date.toLocaleDateString('it-IT')}
                        </Text>
                    </ThemedButton>
 
                    {showDatePicker && (
                        <DateTimePicker
                            value={date}
                            mode="date"
                            display={Platform.OS === 'ios' ? 'spinner' : 'default'}
                            onChange={onChangeDate}
                        />
                    )}
 
                    {/* Selezione dell'ora */}
                    <ThemedButton
                        onPress={() => setShowTimePicker(true)}
                        style={{ width: '80%', marginBottom: 20 }}
                    >
                        <Text style={{ color: '#f2f2f2' }}>
                            Ora: {time.toLocaleTimeString('it-IT', { hour: '2-digit', minute: '2-digit' })}
                        </Text>
                    </ThemedButton>
 
                    {showTimePicker && (
                        <DateTimePicker
                            value={time}
                            mode="time"
                            is24Hour={true}
                            display={Platform.OS === 'ios' ? 'spinner' : 'default'}
                            onChange={onChangeTime}
                        />
                    )}
 
                    {/* Selezione accompagnatore */}
                    <ThemedText style={{ alignSelf: 'flex-start', marginLeft: '10%', marginBottom: 5 }}>
                        Companion
                    </ThemedText>
                    <ThemedView style={styles.pickerWrapper}>
                        <Picker
                            selectedValue={companion}
                            onValueChange={(itemValue) => setCompanion(itemValue)}
                        >
                            {COMPANIONS.map((item) => (
                                <Picker.Item key={item.value ?? 'nome'} label={item.label} value={item.value} />
                            ))}
                        </Picker>
                    </ThemedView>
 
                    <Spacer height={30} />
 
                    <ThemedButton onPress={handleSubmit}>
                        <Text style={{ color: '#f2f2f2' }}>Create Transport</Text>
                    </ThemedButton>
 
                    <Spacer height={50} />
                </ScrollView>
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
    pickerWrapper: {
        width: '80%',
        borderRadius: 8,
        overflow: 'hidden',
    },
})