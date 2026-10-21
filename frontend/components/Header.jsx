import { View, StyleSheet } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import ThemedView from "./ThemedView";
import ThemedText from "./ThemedText";
import IconButton from "./IconButton";

export default function Header({
  name,
  hasUnreadNotifications,
  onPressAvatar,
  onPressNotifications,
}) {
  const insets = useSafeAreaInsets();
  return (
    <ThemedView style={[styles.container, { paddingTop: insets.top }]}>
      <View style={styles.left}>
        <IconButton
          name="person-outline"
          size={22}
          variant="subtle"
          onPress={onPressAvatar}
        />
        <ThemedText style={styles.name}>{name}</ThemedText>
      </View>

      <IconButton
        name="notifications-outline"
        size={20}
        variant="subtle"
        badge={hasUnreadNotifications}
        onPress={onPressNotifications}
      />
    </ThemedView>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    paddingHorizontal: 20,
    paddingBottom: 12,
  },
  left: {
    flexDirection: "row",
    alignItems: "center",
    gap: 12,
  },
  name: {
    fontSize: 16,
    fontWeight: "600",
  },
});
