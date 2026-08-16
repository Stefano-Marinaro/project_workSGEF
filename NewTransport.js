import React, { useState } from 'react'; 
import { StatusBar } from 'expo-status-bar';
import { StyleSheet, TextInput, View, FlatList, Text, TouchableOpacity } from 'react-native'; 

//TouchableOpacity non supporta placeholder perciò per inserire un testo al suo interno serve inserire Text
//OnPress serve ad intercettare il tocco del dito sullo schermo

export default function NewTransport() {

// Stato per gestire l'apertura e la chiusura della tendina
  const [isOpen, setIsOpen] = useState(false);

// Stato per memorizzare l'elemento selezionato 
  const [selectedValue, setSelectedValue] = useState('Select an option');

// Dati da mostrare nella tendina
  const data = [
    { id: '1', label: 'Option 1' },
    { id: '2', label: 'Option 2' },
    { id: '3', label: 'Option 3' },
  ];

// Funzione eseguita al momento del tocco della tendina
  const handleSelect = (item) => {
    // salva lo stato dell'opzione scelta
    setSelectedValue(item.label);
    // chiude la tendina
    setIsOpen(false);
  };

  return (
    <View style={styles.container}>

      <Text style={styles.title}>GoCare</Text> 

      <TextInput style={styles.input} placeholder="Insert your destination" />
      
      <StatusBar style="auto" />

      {/* Pulsante principale del menù a tendina, che inverte lo stato isOpen ad ogni tocco (Apri e Chiudi) */}
      <TouchableOpacity 
        style={styles.dropdownButton} 
        onPress={() => setIsOpen(!isOpen)}
      >
        {/* A seconda dello stato viene mostrata una freccia diversa */}
        <Text style={styles.dropdownButtonText}>{selectedValue}</Text>
        <Text style={styles.arrow}>{isOpen ? '▲' : '▼'}</Text>
      </TouchableOpacity>

      {/* Render condizionale, cioè mostra il blocco contenente la FlatList solo se isOpen è true */}
      {isOpen && (
        <View style={styles.dropdownList}>

          {/* Genera le Options */}
          <FlatList
            data={data} // passa l'array di opzioni da visualizzare
            keyExtractor={(item) => item.id} // assegna una chiave univoca ad ogni elemento per ottimizzare il render 
            renderItem={({ item }) => ( // definisce come viene renderizzata ogni singola voce della lista
              <TouchableOpacity 
                style={styles.dropdownItem} 
                onPress={() => handleSelect(item)}
              >
                <Text style={styles.dropdownItemText}>{item.label}</Text>
              </TouchableOpacity>
            )}
          />
        </View>
      )}

    {/* Pulsante di invio che stampa un messaggio in console quando viene prenuto, grazie a onPress */}
    <TouchableOpacity style={styles.transportButton} onPress={() => console.log('Invia')}>
        <Text style={styles.transportButtonText}>Insert Transport</Text>
    </TouchableOpacity>
    </View>
  ); 

}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#F0F4F8',
    alignItems: 'center',
    justifyContent: 'center',
    padding: 20,
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
    marginBottom: 20,
    textcolor: '#2B2D42',
  },
  input: {
    width: '100%',
    borderWidth: 1,
    borderColor: '#2B2D42',
    padding: 10,
    borderRadius: 8,
    marginBottom: 20,
  },
  dropdownButton: {
    flexDirection: 'row',
    width: '100%',
    backgroundColor: '#A8DADC',
    padding: 15,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: '#2B2D42',
    alignItems: 'center',
  },
  dropdownButtonText: {
    flex: 1,
    fontSize: 16,
  },
  arrow: {
    fontSize: 14,
  },
  dropdownList: {
    width: '100%',
    backgroundColor: '#A8DADC',
    borderRadius: 8,
    borderWidth: 1,
    borderColor: '#2B2D42',
    marginTop: 5,
    maxHeight: 150,
  },
  dropdownItem: {
    padding: 15,
    borderBottomWidth: 1,
    borderBottomColor: '#2B2D42',
  },
  dropdownItemText: {
    fontSize: 16,
  },
    transportButton: {
    width: '100%',
    backgroundColor: '#2B2D42',
    padding: 15,
    borderRadius: 8,
    alignItems: 'center',
    justifyContent: 'center',
    marginTop: 20, //Distanzia il pulsante dagli elementi superiori
  },
  transportButtonText: {
    color: '#F0F4F8',
    fontSize: 12,
    fontWeight: 'bold',
  },
});