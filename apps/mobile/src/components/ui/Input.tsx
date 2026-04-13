import React from "react";
import { View, Text, TextInput, TextInputProps } from "react-native";
import { cn } from "../../utils/cn";

interface InputProps extends TextInputProps {
  label?: string;
  error?: string;
  containerClassName?: string;
}

export const Input = ({
  label,
  error,
  containerClassName,
  className,
  ...props
}: InputProps) => {
  return (
    <View className={cn("space-y-1.5", containerClassName)}>
      {label && (
        <Text className="text-sm font-medium text-zinc-900 dark:text-zinc-200 ml-1">
          {label}
        </Text>
      )}
      <TextInput
        placeholderTextColor="#a1a1aa"
        className={cn(
          "w-full px-4 py-3 bg-zinc-50 dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 rounded-xl text-zinc-900 dark:text-zinc-100",
          error && "border-red-500",
          className
        )}
        {...props}
      />
      {error && (
        <Text className="text-xs text-red-500 ml-1 mt-1">
          {error}
        </Text>
      )}
    </View>
  );
};
