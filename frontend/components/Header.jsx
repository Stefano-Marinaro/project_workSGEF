import { View, Text, Image, Pressable, StyleSheet } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Ionicons } from "@expo/vector-icons";
import { C } from "../constants/caregiver";

// Header dei mockup caregiver e associazione: campanella a sinistra, logo GoCare
// al centro, iniziali del profilo a destra.
// badgeCount: numero di notifiche non lette (associazione); hasUnreadNotifications: solo pallino (caregiver)
// brandOnly: solo logo, per la pagina pubblica dell'operatore
export default function Header({
  initials,
  hasUnreadNotifications,
  badgeCount,
  onPressAvatar,
  onPressNotifications,
  brandOnly = false,
}) {
  const insets = useSafeAreaInsets();
  if (brandOnly) {
    return (
      <View style={[styles.container, styles.centered, { paddingTop: insets.top + 16 }]}>
        <View style={styles.brand}>
          <Image source={require("../assets/img/gocareLogo.png")} style={styles.logo} />
          <Text style={styles.brandText}>GoCare</Text>
        </View>
      </View>
    );
  }
  return (
    <View style={[styles.container, { paddingTop: insets.top + 16 }]}>
      <Pressable
        onPress={onPressNotifications}
        accessibilityLabel="Notifiche"
        style={({ pressed }) => [styles.circle, pressed && styles.pressed]}
      >
        <Ionicons name="notifications-outline" size={21} color={C.primary} />
        {badgeCount > 0 ? (
          <View style={styles.countBadge}>
            <Text style={styles.countText}>{badgeCount}</Text>
          </View>
        ) : hasUnreadNotifications ? (
          <View style={styles.badge} />
        ) : null}
      </Pressable>

      <View style={styles.brand}>
        <Image
          source={require("../assets/img/gocareLogo.png")}
          style={styles.logo}
          accessibilityIgnoresInvertColors
        />
        <Text style={styles.brandText}>GoCare</Text>
      </View>

      <Pressable
        onPress={onPressAvatar}
        accessibilityLabel="Apri profilo"
        style={({ pressed }) => [styles.circle, pressed && styles.pressed]}
      >
        <Text style={styles.initials}>{initials}</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    paddingHorizontal: 20,
    paddingBottom: 12,
    backgroundColor: C.page,
  },
  circle: {
    width: 44,
    height: 44,
    borderRadius: 22,
    backgroundColor: C.card,
    alignItems: "center",
    justifyContent: "center",
  },
  pressed: { opacity: 0.8 },
  badge: {
    position: "absolute",
    top: 8,
    right: 9,
    width: 9,
    height: 9,
    borderRadius: 5,
    backgroundColor: "#cc475a",
    borderWidth: 1.5,
    borderColor: C.card,
  },
  centered: { justifyContent: "center" },
  countBadge: {
    position: "absolute",
    top: 2,
    right: 2,
    minWidth: 17,
    height: 17,
    borderRadius: 9,
    paddingHorizontal: 4,
    backgroundColor: C.primary,
    alignItems: "center",
    justifyContent: "center",
  },
  countText: { color: C.white, fontSize: 10, fontWeight: "700" },
  brand: { flexDirection: "row", alignItems: "center", gap: 6 },
  logo: { width: 44, height: 44, resizeMode: "contain" },
  brandText: {
    fontSize: 28,
    fontWeight: "700",
    letterSpacing: -1,
    color: C.primary,
  },
  initials: { fontSize: 12, fontWeight: "700", color: C.primary },
});
