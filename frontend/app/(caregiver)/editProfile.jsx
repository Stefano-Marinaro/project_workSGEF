import {
  StyleSheet,
  Image,
  ScrollView,
  useColorScheme,
  ActivityIndicator,
} from "react-native";
import { useState, useEffect } from "react";
import { useRouter } from "expo-router";

import ThemedView from "../../components/ThemedView";
import Spacer from "../../components/Spacer";
import ThemedText from "../../components/ThemedText";
import ThemedTextInput from "../../components/ThemedTextInput";
import ThemedButton from "../../components/ThemedButton";
import { Colors } from "../../constants/Colors";
import api from "../../config/httpClient.js";

const EditProfile = () => {
  const colorScheme = useColorScheme();
  const theme = Colors[colorScheme] ?? Colors.light;
  const router = useRouter();

  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  const [name, setName] = useState("");
  const [surname, setSurname] = useState("");
  const [birthDate, setBirthDate] = useState("");
  const [phone, setPhone] = useState("");

  useEffect(() => {
    const loadProfile = async () => {
      try {
        const response = await api.get("/me/profile");
        setName(response.data.name ?? "");
        setSurname(response.data.surname ?? "");
        setBirthDate(response.data.birthDate ?? "");
        setPhone(response.data.phone ?? "");
      } catch (error) {
        setErrorMessage("Impossibile caricare il profilo");
      } finally {
        setLoading(false);
      }
    };
    loadProfile();
  }, []);

  const handleSave = async () => {
    try {
      // TODO: PUT /me/profile non esiste ancora sul backend (vedi UpdatePersonProfileService,
      // registrato ma senza controller) - va collegato prima che questa chiamata funzioni davvero.
      await api.put("/me/profile", { name, surname, birthDate, phone });
      router.back();
    } catch (error) {
      setErrorMessage(
        error.response?.data?.detail || "Salvataggio non riuscito",
      );
    }
  };

  if (loading) {
    return (
      <ThemedView style={styles.container}>
        <ActivityIndicator size="large" />
      </ThemedView>
    );
  }

  return (
    <ThemedView style={styles.container}>
      <ScrollView contentContainerStyle={styles.scrollContent}>
        <Spacer height={60} />

        <Image
          source={require("../../assets/img/fotoProfilo.jpg")}
          style={[styles.avatar, { borderColor: Colors.primary }]}
        />

        <Spacer height={12} />

        <ThemedText title={true} style={styles.title}>
          Edit Profile
        </ThemedText>

        <Spacer height={30} />

        <ThemedView
          style={[styles.card, { backgroundColor: theme.uiBackground }]}
        >
          <ThemedText style={styles.label}>Nome</ThemedText>
          <ThemedTextInput
            style={styles.textInput}
            value={name}
            onChangeText={setName}
          />

          <Spacer height={16} />

          <ThemedText style={styles.label}>Cognome</ThemedText>
          <ThemedTextInput
            style={styles.textInput}
            value={surname}
            onChangeText={setSurname}
          />

          <Spacer height={16} />

          <ThemedText style={styles.label}>
            Data di nascita (AAAA-MM-GG)
          </ThemedText>
          <ThemedTextInput
            style={styles.textInput}
            value={birthDate}
            onChangeText={setBirthDate}
            placeholder="1990-05-20"
          />

          <Spacer height={16} />

          <ThemedText style={styles.label}>Telefono</ThemedText>
          <ThemedTextInput
            style={styles.textInput}
            value={phone}
            onChangeText={setPhone}
            keyboardType="phone-pad"
          />
        </ThemedView>

        {errorMessage ? (
          <ThemedText style={{ color: "red" }}>{errorMessage}</ThemedText>
        ) : null}

        <Spacer height={30} />

        <ThemedButton style={styles.input} onPress={handleSave}>
          <ThemedText style={styles.btnText}>Save Changes</ThemedText>
        </ThemedButton>

        <Spacer height={15} />

        <ThemedButton
          style={[styles.input, { backgroundColor: Colors.warning }]}
          onPress={() => router.back()}
        >
          <ThemedText style={styles.btnText}>Cancel</ThemedText>
        </ThemedButton>

        <Spacer height={50} />
      </ScrollView>
    </ThemedView>
  );
};

export default EditProfile;

const styles = StyleSheet.create({
  container: { flex: 1 },
  scrollContent: { alignItems: "center", paddingBottom: 40 },
  avatar: {
    width: 110,
    height: 110,
    borderRadius: 55,
    borderWidth: 3,
    alignSelf: "center",
  },
  title: { fontSize: 20, fontWeight: "bold", textAlign: "center" },
  card: { width: "85%", borderRadius: 12, padding: 20 },
  label: { fontSize: 13, opacity: 0.6, marginBottom: 4 },
  textInput: { width: "100%" },
  input: { width: "80%", marginBottom: 0 },
  btnText: { color: "#f2f2f2", textAlign: "center" },
});
