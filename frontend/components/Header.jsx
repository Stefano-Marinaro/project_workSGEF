import { View, Text, Image, Pressable, StyleSheet } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Ionicons } from "@expo/vector-icons";
import { C } from "../constants/caregiver";

// Header del mockup caregiver: campanella a sinistra, logo GoCare al centro,
// iniziali del profilo a destra.
export default function Header({
  initials,
  hasUnreadNotifications,
  onPressAvatar,
  onPressNotifications,
}) {
  const insets = useSafeAreaInsets();
  return (
    <View style={[styles.container, { paddingTop: insets.top + 16 }]}>
      <Pressable
        onPress={onPressNotifications}
        accessibilityLabel="Notifiche"
        style={({ pressed }) => [styles.circle, pressed && styles.pressed]}
      >
        <Ionicons name="notifications-outline" size={21} color={C.primary} />
        {hasUnreadNotifications ? <View style={styles.badge} /> : null}
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
