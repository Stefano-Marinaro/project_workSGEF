import { Pressable, StyleSheet, useColorScheme, View } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { Colors } from "../constants/Colors";

export default function IconButton({
  name,
  size,
  onPress,
  variant,
  badge,
  style,
}) {
  const colorScheme = useColorScheme();
  const theme = Colors[colorScheme] ?? Colors.light;

  const isPrimary = variant === "primary";
  const backgroundColor = isPrimary ? Colors.primary : theme.uiBackground;
  const iconColor = isPrimary ? "#ffffff" : theme.iconColor;
  const circleSize = size + 20;

  return (
    <Pressable
      onPress={onPress}
      style={({ pressed }) => [
        styles.circle,
        {
          width: circleSize,
          height: circleSize,
          borderRadius: circleSize / 2,
          backgroundColor,
        },
        pressed && styles.pressed,
        style,
      ]}
    >
      <Ionicons name={name} size={size} color={iconColor} />

      {badge && <View style={styles.badge} />}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  circle: {
    alignItems: "center",
    justifyContent: "center",
  },
  pressed: {
    opacity: 0.8,
  },
  badge: {
    position: "absolute",
    top: 2,
    right: 2,
    width: 9,
    height: 9,
    borderRadius: 5,
    backgroundColor: "#cc475a",
    borderWidth: 1.5,
    borderColor: "#ffffff",
  },
});
