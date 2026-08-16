import { StatusBar } from 'expo-status-bar';
import { StyleSheet, Text, View } from 'react-native';
import NewTransport from './NewTransport'; //Importo il componente
import CalendarAndHour from './CalendarAndHour';

export default function App() {
  return (
    <View style={styles.container}>
      <StatusBar style="auto" />
      <NewTransport> </NewTransport> 
      <CalendarAndHour> </CalendarAndHour>

    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
    alignItems: 'center',
    justifyContent: 'center',
  },
});
