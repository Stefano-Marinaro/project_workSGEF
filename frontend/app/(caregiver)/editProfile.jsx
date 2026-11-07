import { useCallback, useState } from "react";
import { Text, StyleSheet } from "react-native";
import { useFocusEffect, useLocalSearchParams, useRouter } from "expo-router";

import api from "../../config/httpClient.js";
import { getItem, deleteItem } from "../../config/storage.js";
import { C, toIsoDay, apiErrorMessage } from "../../constants/caregiver";
import DateTimeField from "../../components/caregiver/DateTimeField";
import {
  Page,
  Scroll,
  Loading,
  PageTitle,
  Muted,
  Field,
  Row,
  Footer,
  Btn,
  ErrorText,
} from "../../components/caregiver/ui";

// Mockup "Modifica profilo". L'email si cambia con un flusso dedicato
// (/auth/change-email, richiede la password), quindi qui è in sola lettura.
const EditProfile = () => {
  const router = useRouter();
  const { next } = useLocalSearchParams();

  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");
  const [errors, setErrors] = useState({});
  const [saving, setSaving] = useState(false);

  const [name, setName] = useState("");
  const [surname, setSurname] = useState("");
  const [birthDate, setBirthDate] = useState("");
  const [phone, setPhone] = useState("");
  const [email, setEmail] = useState("");
  const [homeAddress, setHomeAddress] = useState(null);

  useFocusEffect(
    useCallback(() => {
      const loadProfile = async () => {
        try {
          const response = await api.get("/me/profile");
          // Nome e cognome inseriti in registrazione (il backend non li riceve a quel punto)
          let pending = null;
          if (!response.data.name) {
            try {
              pending = JSON.parse((await getItem("pendingProfile")) ?? "null");
            } catch (e) {}
            if (pending?.email !== (response.data.email ?? "").toLowerCase()) pending = null;
          }
          setName(response.data.name ?? pending?.name ?? "");
          setSurname(response.data.surname ?? pending?.surname ?? "");
          setBirthDate(response.data.birthDate ?? "");
          setPhone(response.data.phone ?? "");
          setEmail(response.data.email ?? "");
          setHomeAddress(response.data.homeAddress ?? null);
          setErrors({});
          setErrorMessage("");
        } catch (error) {
          setErrorMessage("Impossibile caricare il profilo");
        } finally {
          setLoading(false);
        }
      };
      loadProfile();
    }, []),
  );

  const goBack = () => {
    if (next === "wizard") router.replace("/transport");
    else router.replace("/profile");
  };

  const handleSave = async () => {
    const found = {};
    if (!name.trim()) found.name = "Inserisci il nome.";
    if (!surname.trim()) found.surname = "Inserisci il cognome.";
    if (!birthDate) found.birthDate = "Inserisci la data di nascita.";
    else if (birthDate >= toIsoDay(new Date())) found.birthDate = "La data di nascita deve essere nel passato.";
    if (!phone.trim()) found.phone = "Inserisci il numero di telefono.";
    else if (!/^\+?[0-9][0-9 ()\-]{5,}$/.test(phone.trim())) found.phone = "Il numero di telefono non è valido.";
    setErrors(found);
    if (Object.keys(found).length) return;

    setSaving(true);
    setErrorMessage("");
    try {
      // La residenza non compare nel modulo: si rimanda quella già salvata per non perderla
      await api.put("/me/profile", {
        name: name.trim(),
        surname: surname.trim(),
        birthDate,
        phone: phone.trim(),
        homeAddress,
      });
      await deleteItem("pendingProfile");
      if (next === "wizard") router.replace("/transport/new");
      else router.replace({ pathname: "/profile", params: { saved: "1" } });
    } catch (error) {
      setErrorMessage(apiErrorMessage(error, "Salvataggio non riuscito"));
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <Loading />;

  return (
    <Page>
      <Scroll>
        <PageTitle>Modifica profilo</PageTitle>
        <Muted style={styles.intro}>Aggiorna i tuoi dati personali e i recapiti.</Muted>

        <Text style={styles.section}>Dati personali</Text>
        <Muted style={styles.hint}>
          Telefono e data di nascita sono necessari per richiedere un trasporto. I campi con * sono obbligatori.
        </Muted>
        <Row>
          <Field label="Nome" required style={styles.flex} value={name} onChangeText={setName} error={errors.name} />
          <Field label="Cognome" required style={styles.flex} value={surname} onChangeText={setSurname} error={errors.surname} />
        </Row>
        <Field label="Data di nascita" required error={errors.birthDate}>
          <DateTimeField mode="date" value={birthDate} onChange={setBirthDate} placeholder="gg/mm/aaaa" maximumDate={new Date()} />
        </Field>

        <Text style={styles.section}>Recapiti</Text>
        <Field label="Email" required value={email} editable={false} />
        <Field label="Telefono" required value={phone} onChangeText={setPhone} keyboardType="phone-pad" error={errors.phone} />

        <ErrorText>{errorMessage}</ErrorText>
      </Scroll>
      <Footer>
        <Btn label="Annulla" variant="secondary" onPress={goBack} />
        <Btn label="Salva modifiche" onPress={handleSave} loading={saving} style={styles.save} />
      </Footer>
    </Page>
  );
};

export default EditProfile;

const styles = StyleSheet.create({
  flex: { flex: 1 },
  intro: { fontSize: 15, marginBottom: 16 },
  section: { fontSize: 14, fontWeight: "600", color: C.muted, marginBottom: 10, marginTop: 4 },
  hint: { fontSize: 13, lineHeight: 20, marginBottom: 14 },
  save: { flex: 1, maxWidth: 200 },
});
