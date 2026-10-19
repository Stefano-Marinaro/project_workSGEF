import { StyleSheet, View, Text } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { Colors } from "../constants/Colors";
import IconButton from "./IconButton";

const VARIANTS = {
  error: {
    background: "#fbe4e7",
    border: Colors.warning,
    text: Colors.warning,
    icon: "close-circle",
  },
  info: {
    background: "#fcefdd",
    border: Colors.info,
    text: Colors.info,
    icon: "alert-circle",
  },
  success: {
    background: "#e3f6e8",
    border: Colors.success,
    text: Colors.success,
    icon: "checkmark-circle",
  },
};

export default function Banner({ variant, message, onDismiss, style }) {
  const config = VARIANTS[variant] ?? VARIANTS.info;

  return (
    <View
      style={[
        styles.container,
        { backgroundColor: config.background, borderColor: config.border },
        style,
      ]}
    >
      <Ionicons name={config.icon} size={20} color={config.text} />

      <Text style={[styles.message, { color: config.text }]}>{message}</Text>

      {onDismiss && (
        <IconButton
          name="close"
          size={16}
          variant="subtle"
          onPress={onDismiss}
          style={{ backgroundColor: "transparent" }}
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: "row",
    alignItems: "center",
    gap: 10,
    borderWidth: 1.5,
    borderRadius: 10,
    padding: 12,
  },
  message: {
    flex: 1,
    fontSize: 13,
    fontWeight: "600",
  },
});
