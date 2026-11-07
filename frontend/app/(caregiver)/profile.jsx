import { useCallback, useState } from "react";
import { View, Text, Pressable, StyleSheet } from "react-native";
import { useFocusEffect, useLocalSearchParams, useRouter } from "expo-router";

import api from "../../config/httpClient.js";
import Banner from "../../components/Banner";
import { C, formatIsoDate, apiErrorMessage } from "../../constants/caregiver";
import {
  Page,
  Scroll,
  Loading,
  PageTitle,
  Card,
  Icon,
  Initials,
  CircleButton,
  Divider,
  Btn,
  ErrorText,
} from "../../components/caregiver/ui";

const DataRow = ({ icon, label, value }) => (
  <View style={styles.dataRow}>
    <Icon name={icon} size={20} color={C.muted} />
    <View>
      <Text style={styles.dataLabel}>{label}</Text>
      <Text style={styles.dataValue}>{value || "Non indicato"}</Text>
    </View>
  </View>
);

const Profile = () => {
  const router = useRouter();
  const { saved } = useLocalSearchParams();

  const [loading, setLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");
  const [profile, setProfile] = useState(null);
  const [showSaved, setShowSaved] = useState(false);

  useFocusEffect(
    useCallback(() => {
      setShowSaved(saved === "1");
      const loadProfile = async () => {
        try {
          const response = await api.get("/me/profile");
          setProfile(response.data);
          setErrorMessage("");
        } catch (error) {
          setErrorMessage(apiErrorMessage(error, "Impossibile caricare il profilo"));
        } finally {
          setLoading(false);
        }
      };
      loadProfile();
    }, [saved]),
  );

  if (loading) return <Loading />;

  return (
    <Page>
      <Scroll>
        {showSaved ? (
          <Banner
            variant="success"
            message="Modifiche salvate"
            onDismiss={() => {
              setShowSaved(false);
              router.setParams({ saved: "" });
            }}
            style={styles.banner}
          />
        ) : null}

        <PageTitle>Profilo</PageTitle>
        <ErrorText>{errorMessage}</ErrorText>

        {profile ? (
          <>
            <View style={styles.identity}>
              <Initials name={profile.name} surname={profile.surname} size={52} solid />
              <View style={styles.flex}>
                <Text style={styles.name}>
                  {profile.name} {profile.surname}
                </Text>
                <Text style={styles.email}>{profile.email}</Text>
              </View>
              <CircleButton icon="pencil" label="Modifica profilo" onPress={() => router.push("/editProfile")} />
            </View>

            <Card style={styles.card}>
              <Text style={styles.cardTitle}>Dati personali</Text>
              <DataRow icon="call-outline" label="Telefono" value={profile.phone} />
              <Divider style={styles.divider} />
              <DataRow icon="calendar-outline" label="Data di nascita" value={formatIsoDate(profile.birthDate)} />
            </Card>
          </>
        ) : null}

        <Pressable onPress={() => router.push("/transport/history")} style={({ pressed }) => [styles.history, pressed && styles.pressed]}>
          <View style={styles.flex}>
            <Text style={styles.historyTitle}>Storico dei trasporti</Text>
            <Text style={styles.historySub}>Consulta i trasporti precedenti</Text>
          </View>
          <Icon name="chevron-forward" size={20} color={C.muted} />
        </Pressable>

        <Btn label="Esci" variant="danger" icon="log-out-outline" onPress={() => router.replace("/logout")} style={styles.logout} />
      </Scroll>
    </Page>
  );
};

export default Profile;

const styles = StyleSheet.create({
  flex: { flex: 1 },
  pressed: { opacity: 0.85 },
  banner: { marginTop: 12 },
  identity: { flexDirection: "row", alignItems: "center", gap: 14, marginBottom: 22 },
  name: { fontSize: 19, fontWeight: "600", color: C.text },
  email: { fontSize: 13, color: C.muted, marginTop: 4 },
  card: { paddingVertical: 20, marginBottom: 22 },
  cardTitle: { fontSize: 14, fontWeight: "600", color: C.muted, marginBottom: 18 },
  dataRow: { flexDirection: "row", gap: 16, alignItems: "flex-start" },
  dataLabel: { fontSize: 13, color: C.muted, marginBottom: 4 },
  dataValue: { fontSize: 16, color: C.text },
  divider: { marginVertical: 14 },
  history: {
    flexDirection: "row",
    alignItems: "center",
    backgroundColor: C.card,
    borderRadius: 14,
    padding: 18,
    marginBottom: 36,
  },
  historyTitle: { fontSize: 16, fontWeight: "600", color: C.text },
  historySub: { fontSize: 13, color: C.muted, marginTop: 4 },
  logout: { borderRadius: 12 },
});
