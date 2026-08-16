import React, { useState } from 'react';
import { View, Text, Button, StyleSheet, Platform } from 'react-native';
import DateTimePicker from '@react-native-community/datetimepicker';

export default function CalendarAndHour() {

  // Stato che contiene l'oggetto Date completo (giorno, mese, anno, ore e minuti)
  const [date, setDate] = useState(new Date());

  // Stato per gestire cosa mostrare nel picker: 'date' (calendario) o 'time' (orologio)
  const [mode, setMode] = useState('date');

  // Stato booleano per controllare la visibilità del selettore (true = visibile, false = nascosto)
  const [show, setShow] = useState(false);

  // Funzione eseguita ogni volta che l'utente seleziona o cambia un valore nel picker
  const onChange = (event, selectedDate) => {
    // Se l'utente seleziona una nuova data la usa, altrimenti mantiene il valore precedente
    const currentDate = selectedDate || date;
    
    // Su Android la modale si chiude automaticamente dopo la scelta (setShow(false)).
    // Su iOS il picker può rimanere visibile inline, quindi 'show' resta true solo su iOS.
    setShow(Platform.OS === 'ios');
    
    // Aggiorna lo stato 'date' con il nuovo valore scelto
    setDate(currentDate);
  };

  // Funzione per impostare la modalità ('date' o 'time') e aprire la finestra del selettore
  const showMode = (currentMode) => {
    setShow(true);        // Rende visibile il DateTimePicker
    setMode(currentMode); // Definisce se aprire la vista calendario o la vista orologio
  };

  return (
    <View style={styles.container}>
      
      {/* Testo che stampa la data e l'ora correnti formattate in base alle impostazioni di sistema */}
      <Text style={styles.text}>
        Selected date and time: {date.toLocaleString()}
      </Text>

      {/* Contenitore per affiancare orizzontalmente i due pulsanti di selezione */}
      <View style={styles.buttonContainer}>
        {/* Pulsante che attiva il picker in modalità calendario ('date') */}
        <Button onPress={() => showMode('date')} title="Scegli Data" />
        
        {/* Pulsante che attiva il picker in modalità orologio ('time') */}
        <Button onPress={() => showMode('time')} title="Scegli Ora Esatta" />
      </View>

      {/* Render condizionale: il DateTimePicker viene renderizzato solo se 'show' è true */}
      {show && (
        <DateTimePicker
          value={date}          // Valore di data/ora attualmente selezionato da mostrare
          mode={mode}            // Tipo di vista: 'date' (giorno) o 'time' (ora)
          is24Hour={true}        // Forza la visualizzazione dell'orologio nel formato 24 ore (es. 14:30)
          onChange={onChange}    // Funzione di callback invocata alla modifica o conferma
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,                    // Fa espandere il contenitore su tutto lo schermo disponibile
    justifyContent: 'center',   // Centra tutti gli elementi figlio in verticale
    alignItems: 'center',       // Centra tutti gli elementi figlio in orizzontale
    padding: 20,                // Spazio interno di sicurezza di 20px rispetto ai bordi
  },

  text: {
    fontSize: 16,               // Dimensione del font a 16px
    marginBottom: 20,           // Distanza dal blocco dei pulsanti sottostante
  },

  buttonContainer: {
    flexDirection: 'row',       // Dispone i pulsanti uno affianco all'altro in orizzontale
    gap: 10,                    // Crea una distanza di 10px tra i pulsanti
  },
});